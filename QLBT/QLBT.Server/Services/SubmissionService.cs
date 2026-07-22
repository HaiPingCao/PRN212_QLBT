using Microsoft.EntityFrameworkCore;
using System.IO;
using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly Prn212PQlbtContext _db;
        private readonly Dictionary<string, UploadSession> _uploads = new();
        private readonly object _uploadLock = new();

        public string RootFolder { get; set; } = "Submissions";

        public SubmissionService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public string BeginUpload(int assignmentId, string studentId, string fileName, long fileSize)
        {
            var assignment = _db.BaiTaps.FirstOrDefault(bt => bt.Id == assignmentId);
            if (assignment == null)
                throw new InvalidOperationException("Bài tập không tồn tại.");

            if (DateTime.Now > assignment.HanNop)
                throw new InvalidOperationException("Đã hết hạn nộp bài, không thể nộp/nộp lại.");

            var existing = _db.BaiNops.FirstOrDefault(b => b.BaiTapId == assignmentId && b.Mssv == studentId);
            if (existing != null && existing.Diem.HasValue)
                throw new InvalidOperationException("Bài đã được chấm điểm, không thể nộp lại.");

            var uploadId = Guid.NewGuid().ToString();
            var session = new UploadSession
            {
                UploadId = uploadId,
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileName = fileName,
                FileSize = fileSize,
                Chunks = new Dictionary<int, byte[]>()
            };
            lock (_uploadLock) _uploads[uploadId] = session;
            return uploadId;
        }

        public void AddChunk(string uploadId, int chunkIndex, string base64Data)
        {
            lock (_uploadLock)
            {
                if (!_uploads.TryGetValue(uploadId, out var session))
                    throw new InvalidOperationException("UploadId does not exist");

                session.Chunks[chunkIndex] = Convert.FromBase64String(base64Data);
            }
        }

        public int FinalizeUpload(string uploadId)
        {
            UploadSession session;
            lock (_uploadLock)
            {
                if (!_uploads.TryGetValue(uploadId, out session!))
                    throw new InvalidOperationException("UploadId does not exist");
                _uploads.Remove(uploadId);
            }

            var orderedChunks = session.Chunks
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .ToArray();

            var folder = Path.Combine(RootFolder, session.AssignmentId.ToString());
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(session.FileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var savedFileName = $"{session.StudentId}_{timestamp}{ext}";
            var filePath = Path.Combine(folder, savedFileName);

            File.WriteAllBytes(filePath, orderedChunks);

            var existing = _db.BaiNops.FirstOrDefault(b =>
                b.BaiTapId == session.AssignmentId && b.Mssv == session.StudentId);

            int submissionId;

            if (existing != null)
            {
                if (File.Exists(existing.DuongDanFile))
                    File.Delete(existing.DuongDanFile);

                existing.TenFile = savedFileName;
                existing.DuongDanFile = filePath;
                existing.SoLanNop += 1;
                existing.NgayNop = DateTime.Now;
                existing.Diem = null; // nop lai thi bo diem cu, cho cham lai
                submissionId = existing.Id;
            }
            else
            {
                var assignment = _db.BaiTaps.First(bt => bt.Id == session.AssignmentId);

                var newSubmission = new BaiNop
                {
                    BaiTapId = session.AssignmentId,
                    LopId = assignment.LopId,
                    Mssv = session.StudentId,
                    TenFile = savedFileName,
                    DuongDanFile = filePath,
                    SoLanNop = 1,
                    NgayNop = DateTime.Now
                };
                _db.BaiNops.Add(newSubmission);
                _db.SaveChanges();
                submissionId = newSubmission.Id;
            }

            _db.SaveChanges();
            return submissionId;
        }

        public string? GetSubmissionFilePath(int submissionId, string requesterId, Role role)
        {
            var bn = _db.BaiNops.Include(b => b.BaiTap).ThenInclude(bt => bt.Lop)
                .FirstOrDefault(b => b.Id == submissionId);

            if (bn == null || !File.Exists(bn.DuongDanFile)) return null;

            var allowed = role == Role.Student
                ? bn.Mssv == requesterId
                : bn.BaiTap.Lop.Msgv == requesterId;

            return allowed ? bn.DuongDanFile : null;
        }

        public SubmissionDto? GetSubmissionByAssignment(int assignmentId, string studentId)
        {
            var bn = _db.BaiNops.FirstOrDefault(b => b.BaiTapId == assignmentId && b.Mssv == studentId);
            return bn == null ? null : ToDto(bn);
        }

        public List<SubmissionDto> GetClassSubmissions(int assignmentId, string teacherId)
        {
            var assignment = _db.BaiTaps.Include(bt => bt.Lop)
                .FirstOrDefault(bt => bt.Id == assignmentId && bt.Lop.Msgv == teacherId);
            if (assignment == null) return new List<SubmissionDto>();

            var submitted = _db.BaiNops
                .Where(bn => bn.BaiTapId == assignmentId)
                .ToDictionary(bn => bn.Mssv);

            var pastDue = DateTime.Now > assignment.HanNop;

            var enrolled = _db.SinhVienLops
                .Where(svl => svl.LopId == assignment.LopId && svl.DangThamGia)
                .Include(svl => svl.MssvNavigation)
                .OrderBy(svl => svl.Mssv)
                .ToList();

            var result = new List<SubmissionDto>();
            foreach (var svl in enrolled)
            {
                if (submitted.TryGetValue(svl.Mssv, out var bn))
                {
                    result.Add(ToDto(bn, svl.MssvNavigation.HoTen));
                }
                else
                {
                    result.Add(new SubmissionDto
                    {
                        Id = 0,
                        AssignmentId = assignmentId,
                        StudentId = svl.Mssv,
                        StudentName = svl.MssvNavigation.HoTen,
                        FileName = "(Chưa nộp)",
                        SubmitCount = 0,
                        SubmittedAt = default,
                        Grade = pastDue ? 0 : null
                    });
                }
            }

            return result;
        }

        public bool SetGrade(int submissionId, string teacherId, decimal? grade)
        {
            if (grade.HasValue && (grade.Value < 0 || grade.Value > 10))
                throw new ArgumentOutOfRangeException(nameof(grade), "Điểm phải là số từ 0 đến 10.");

            var bn = _db.BaiNops.Include(b => b.BaiTap).ThenInclude(bt => bt.Lop)
                .FirstOrDefault(b => b.Id == submissionId && b.BaiTap.Lop.Msgv == teacherId);
            if (bn == null) return false;

            bn.Diem = grade;
            _db.SaveChanges();
            return true;
        }

        private static SubmissionDto ToDto(BaiNop bn, string? studentName = null)
        {
            return new SubmissionDto
            {
                Id = bn.Id,
                AssignmentId = bn.BaiTapId,
                StudentId = bn.Mssv,
                StudentName = studentName ?? "",
                FileName = bn.TenFile,
                SubmitCount = bn.SoLanNop,
                SubmittedAt = bn.NgayNop,
                Grade = bn.Diem
            };
        }
    }

    internal class UploadSession
    {
        public string UploadId { get; set; } = "";
        public int AssignmentId { get; set; }
        public string StudentId { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public Dictionary<int, byte[]> Chunks { get; set; } = new();
    }
}
