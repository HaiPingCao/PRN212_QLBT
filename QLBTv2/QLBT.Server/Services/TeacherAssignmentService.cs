using System.IO;
using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class TeacherAssignmentService : ITeacherAssignmentService
    {
        private readonly Prn212PQlbtContext _db;

        public string RootFolder { get; set; } = "Submissions";

        public TeacherAssignmentService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<TeacherAssignmentDto> GetAll(string teacherId, int classId)
        {
            var query = _db.BaiTaps.Where(bt => bt.Lop.Msgv == teacherId);
            if (classId > 0)
                query = query.Where(bt => bt.LopId == classId);

            return query
                .OrderByDescending(bt => bt.NgayTao)
                .Select(bt => new TeacherAssignmentDto
                {
                    Id = bt.Id,
                    ClassId = bt.LopId,
                    TenLop = bt.Lop.TenLop,
                    Title = bt.TieuDe,
                    Description = bt.MoTa,
                    ProblemFileName = bt.TenFileDe,
                    DueDate = bt.HanNop,
                    CreatedAt = bt.NgayTao,
                    SubmissionCount = bt.BaiNops.Count
                })
                .ToList();
        }

        public TeacherAssignmentDto? Create(string teacherId, CreateAssignmentRequest input)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == input.ClassId && l.Msgv == teacherId);
            if (lop == null) return null;

            var baiTap = new BaiTap
            {
                LopId = input.ClassId,
                TieuDe = input.Title,
                MoTa = input.Description,
                HanNop = input.DueDate,
                NgayTao = DateTime.Now
            };

            _db.BaiTaps.Add(baiTap);
            _db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(input.ProblemFileName) && input.ProblemFileBase64 != null)
            {
                var (tenFile, duongDan) = SaveProblemFile(baiTap.Id, input.ProblemFileName, input.ProblemFileBase64);
                baiTap.TenFileDe = tenFile;
                baiTap.DuongDanFileDe = duongDan;
                _db.SaveChanges();
            }

            return new TeacherAssignmentDto
            {
                Id = baiTap.Id,
                ClassId = baiTap.LopId,
                TenLop = lop.TenLop,
                Title = baiTap.TieuDe,
                Description = baiTap.MoTa,
                ProblemFileName = baiTap.TenFileDe,
                DueDate = baiTap.HanNop,
                CreatedAt = baiTap.NgayTao,
                SubmissionCount = 0
            };
        }

        public TeacherAssignmentDto? Update(string teacherId, UpdateAssignmentRequest input)
        {
            var baiTap = _db.BaiTaps.FirstOrDefault(b => b.Id == input.Id && b.Lop.Msgv == teacherId);
            if (baiTap == null) return null;

            baiTap.TieuDe = input.Title;
            baiTap.MoTa = input.Description;
            baiTap.HanNop = input.DueDate;

            if (!string.IsNullOrWhiteSpace(input.ProblemFileName) && input.ProblemFileBase64 != null)
            {
                if (!string.IsNullOrEmpty(baiTap.DuongDanFileDe) && File.Exists(baiTap.DuongDanFileDe))
                    File.Delete(baiTap.DuongDanFileDe);

                var (tenFile, duongDan) = SaveProblemFile(baiTap.Id, input.ProblemFileName, input.ProblemFileBase64);
                baiTap.TenFileDe = tenFile;
                baiTap.DuongDanFileDe = duongDan;
            }

            _db.SaveChanges();

            return new TeacherAssignmentDto
            {
                Id = baiTap.Id,
                ClassId = baiTap.LopId,
                TenLop = baiTap.Lop.TenLop,
                Title = baiTap.TieuDe,
                Description = baiTap.MoTa,
                ProblemFileName = baiTap.TenFileDe,
                DueDate = baiTap.HanNop,
                CreatedAt = baiTap.NgayTao,
                SubmissionCount = _db.BaiNops.Count(bn => bn.BaiTapId == baiTap.Id)
            };
        }

        public bool Delete(string teacherId, int id)
        {
            var baiTap = _db.BaiTaps.FirstOrDefault(b => b.Id == id && b.Lop.Msgv == teacherId);
            if (baiTap == null) return false;

            if (!string.IsNullOrEmpty(baiTap.DuongDanFileDe) && File.Exists(baiTap.DuongDanFileDe))
                File.Delete(baiTap.DuongDanFileDe);

            _db.BaiTaps.Remove(baiTap);
            _db.SaveChanges();
            return true;
        }

        public string? GetProblemFilePath(int assignmentId, string teacherId)
        {
            var bt = _db.BaiTaps.FirstOrDefault(b => b.Id == assignmentId && b.Lop.Msgv == teacherId);
            if (bt == null || string.IsNullOrEmpty(bt.DuongDanFileDe)) return null;
            return bt.DuongDanFileDe;
        }

        private (string TenFile, string DuongDan) SaveProblemFile(int assignmentId, string fileName, string base64)
        {
            var folder = Path.Combine(RootFolder, "ProblemFiles", assignmentId.ToString());
            Directory.CreateDirectory(folder);

            var destPath = Path.Combine(folder, fileName);
            File.WriteAllBytes(destPath, Convert.FromBase64String(base64));

            return (fileName, destPath);
        }
    }
}
