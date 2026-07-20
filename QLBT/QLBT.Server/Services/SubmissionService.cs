using QLBT.Server.Models;
using QLBT.Shared.Models;
using System.IO;

namespace QLBT.Server.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly Prn212PQlbtContext _db;
        private readonly AssignmentService _assignmentService;

        // uploadId → upload state
        private readonly Dictionary<string, UploadSession> _uploads = new();

        private string _rootFolder = "Submissions";

        public string RootFolder
        {
            get => _rootFolder;
            set => _rootFolder = value;
        }

        public SubmissionService(Prn212PQlbtContext db, AssignmentService assignmentService)
        {
            _db = db;
            _assignmentService = assignmentService;
        }

        public string BeginUpload(int assignmentId, string studentId, string fileName, long fileSize)
        {
            var uploadId = Guid.NewGuid().ToString();
            _uploads[uploadId] = new UploadSession
            {
                UploadId = uploadId,
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileName = fileName,
                FileSize = fileSize,
                Chunks = new Dictionary<int, byte[]>()
            };
            return uploadId;
        }

        public void AddChunk(string uploadId, int chunkIndex, string base64Data)
        {
            if (!_uploads.TryGetValue(uploadId, out var session))
                throw new InvalidOperationException("UploadId does not exist");

            session.Chunks[chunkIndex] = Convert.FromBase64String(base64Data);
        }

        public int FinalizeUpload(string uploadId)
        {
            if (!_uploads.TryGetValue(uploadId, out var session))
                throw new InvalidOperationException("UploadId does not exist");

            // Assemble chunks in order
            var orderedChunks = session.Chunks
                .OrderBy(kv => kv.Key)
                .SelectMany(kv => kv.Value)
                .ToArray();

            // Create storage directory
            var folder = Path.Combine(_rootFolder, session.AssignmentId.ToString());
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(session.FileName);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var savedFileName = $"{session.StudentId}_{timestamp}{ext}";
            var filePath = Path.Combine(folder, savedFileName);

            File.WriteAllBytes(filePath, orderedChunks);

            // Upsert: find existing submission if any
            var existing = _db.BaiNops.FirstOrDefault(b =>
                b.BaiTapId == session.AssignmentId && b.Mssv == session.StudentId);

            int submissionId;

            if (existing != null)
            {
                // Delete old file
                if (File.Exists(existing.DuongDanFile))
                    File.Delete(existing.DuongDanFile);

                existing.TenFile = savedFileName;
                existing.DuongDanFile = filePath;
                existing.SoLanNop += 1;
                existing.NgayNop = DateTime.Now;
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
            _uploads.Remove(uploadId);

            return submissionId;
        }

        public string? GetProblemFilePath(int assignmentId, string studentId)
        {
            return _assignmentService.GetProblemFilePath(assignmentId, studentId);
        }

        public string? GetSubmissionFilePath(int submissionId, string studentId)
        {
            var bn = _db.BaiNops.FirstOrDefault(b =>
                b.Id == submissionId && b.Mssv == studentId);

            if (bn == null || !File.Exists(bn.DuongDanFile))
                return null;

            return bn.DuongDanFile;
        }

        public SubmissionDto? GetSubmission(int submissionId, string studentId)
        {
            var bn = _db.BaiNops.FirstOrDefault(b =>
                b.Id == submissionId && b.Mssv == studentId);

            if (bn == null) return null;

            return new SubmissionDto
            {
                Id = bn.Id,
                AssignmentId = bn.BaiTapId,
                StudentId = bn.Mssv,
                FileName = bn.TenFile,
                SubmitCount = bn.SoLanNop,
                SubmittedAt = bn.NgayNop
            };
        }

        public SubmissionDto? GetSubmissionByAssignment(int assignmentId, string studentId)
        {
            var bn = _db.BaiNops.FirstOrDefault(b =>
                b.BaiTapId == assignmentId && b.Mssv == studentId);

            if (bn == null) return null;

            return new SubmissionDto
            {
                Id = bn.Id,
                AssignmentId = bn.BaiTapId,
                StudentId = bn.Mssv,
                FileName = bn.TenFile,
                SubmitCount = bn.SoLanNop,
                SubmittedAt = bn.NgayNop
            };
        }

        public bool DeleteSubmission(int submissionId, string studentId)
        {
            var bn = _db.BaiNops.FirstOrDefault(b =>
                b.Id == submissionId && b.Mssv == studentId);

            if (bn == null) return false;

            if (File.Exists(bn.DuongDanFile))
                File.Delete(bn.DuongDanFile);

            _db.BaiNops.Remove(bn);
            _db.SaveChanges();
            return true;
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