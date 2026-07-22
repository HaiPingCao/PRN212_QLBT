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
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia)
                .Select(svl => svl.LopId)
                .ToList();

            var assignments = _db.BaiTaps
                .Where(bt => classIds.Contains(bt.LopId))
                .ToList();

            var grades = _db.BaiNops
                .Where(bn => bn.Mssv == studentId)
                .ToDictionary(bn => bn.BaiTapId, bn => bn.Diem);

            return assignments.Select(bt => ToDto(bt, grades)).ToList();
        }

        public AssignmentDto? GetAssignmentDetail(int assignmentId, string studentId)
        {
            var classIds = _db.SinhVienLops
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia)
                .Select(svl => svl.LopId)
                .ToList();

            var bt = _db.BaiTaps.FirstOrDefault(b => b.Id == assignmentId && classIds.Contains(b.LopId));
            if (bt == null) return null;

            var submission = _db.BaiNops.FirstOrDefault(bn => bn.BaiTapId == assignmentId && bn.Mssv == studentId);

            return ToDto(bt, submission?.Diem, hasSubmitted: submission != null);
        }

        public string? GetProblemFilePath(int assignmentId, string studentId)
        {
            var classIds = _db.SinhVienLops
                .Where(svl => svl.Mssv == studentId && svl.DangThamGia)
                .Select(svl => svl.LopId)
                .ToList();

            var bt = _db.BaiTaps.FirstOrDefault(b => b.Id == assignmentId && classIds.Contains(b.LopId));
            if (bt == null || string.IsNullOrEmpty(bt.DuongDanFileDe))
                return null;

            return bt.DuongDanFileDe;
        }

        private static AssignmentDto ToDto(BaiTap bt, Dictionary<int, decimal?> grades)
        {
            var hasSubmitted = grades.TryGetValue(bt.Id, out var grade);
            return ToDto(bt, grade, hasSubmitted);
        }

        private static AssignmentDto ToDto(BaiTap bt, decimal? grade, bool hasSubmitted)
        {
            return new AssignmentDto
            {
                Id = bt.Id,
                ClassId = bt.LopId,
                Title = bt.TieuDe,
                Description = bt.MoTa ?? "",
                ProblemFileName = bt.TenFileDe,
                DueDate = bt.HanNop,
                CreatedAt = bt.NgayTao,
                Grade = grade,
                HasSubmitted = hasSubmitted
            };
        }
    }
}
