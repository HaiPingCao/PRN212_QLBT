using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class ClassService : IClassService
    {
        private readonly Prn212PQlbtContext _db;

        public ClassService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<ClassDto> GetClassesOfTeacher(string teacherId)
        {
            return _db.Lops
                .Where(l => l.Msgv == teacherId)
                .OrderByDescending(l => l.Id)
                .Select(l => new ClassDto
                {
                    Id = l.Id,
                    TenLop = l.TenLop,
                    HocKyId = l.HocKiId,
                    TenHocKy = l.HocKi.TenHocKy,
                    ChuyenNganh = l.ChuyenNganh,
                    SoSinhVien = l.SinhVienLops.Count(svl => svl.DangThamGia)
                })
                .ToList();
        }

        public List<ClassEnrollmentDto>? GetClassStudents(string teacherId, int classId)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == classId && l.Msgv == teacherId);
            if (lop == null) return null;

            return _db.SinhVienLops
                .Where(svl => svl.LopId == classId)
                .OrderByDescending(svl => svl.DangThamGia)
                .ThenBy(svl => svl.Mssv)
                .Select(svl => new ClassEnrollmentDto
                {
                    Mssv = svl.Mssv,
                    HoTen = svl.MssvNavigation.HoTen,
                    Email = svl.MssvNavigation.Email,
                    DangThamGia = svl.DangThamGia,
                    NgayThamGia = svl.NgayThamGia,
                    NgayRoiLop = svl.NgayRoiLop
                })
                .ToList();
        }
    }
}
