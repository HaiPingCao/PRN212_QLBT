using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly Prn212PQlbtContext _db;

        public AssignmentService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<AssignmentDto> GetAssignmentList(string studentId)
        {
            var classIds = _db.SinhVienLops
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia == true)
                .Select(svl => svl.LopId)
                .ToList();

            return _db.BaiTaps
                .Where(bt => classIds.Contains(bt.LopId))
                .Select(bt => new AssignmentDto
                {
                    Id = bt.Id,
                    ClassId = bt.LopId,
                    Title = bt.TieuDe,
                    Description = bt.MoTa ?? "",
                    ProblemFileName = bt.TenFileDe,
                    DueDate = bt.HanNop,
                    CreatedAt = bt.NgayTao
                })
                .ToList();
        }

        public AssignmentDto? GetAssignmentDetail(int assignmentId, string studentId)
        {
            var classIds = _db.SinhVienLops
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia == true)
                .Select(svl => svl.LopId)
                .ToList();

            var bt = _db.BaiTaps.FirstOrDefault(b =>
                b.Id == assignmentId && classIds.Contains(b.LopId));

            if (bt == null) return null;

            return new AssignmentDto
            {
                Id = bt.Id,
                ClassId = bt.LopId,
                Title = bt.TieuDe,
                Description = bt.MoTa ?? "",
                ProblemFileName = bt.TenFileDe,
                DueDate = bt.HanNop,
                CreatedAt = bt.NgayTao
            };
        }

        public string? GetProblemFilePath(int assignmentId, string studentId)
        {
            var classIds = _db.SinhVienLops
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia == true)
                .Select(svl => svl.LopId)
                .ToList();

            var bt = _db.BaiTaps.FirstOrDefault(b =>
                b.Id == assignmentId && classIds.Contains(b.LopId));

            if (bt == null || string.IsNullOrEmpty(bt.DuongDanFileDe))
                return null;

            return bt.DuongDanFileDe;
        }
    }
}