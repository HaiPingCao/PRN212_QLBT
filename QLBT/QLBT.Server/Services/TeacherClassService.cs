using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QLBT.Server.Models;
using QLBT.Server.Services;

namespace QLBT.Server.Services
{
    public class TeacherClassService : ITeacherClassService
    {
        private readonly Prn212PQlbtContext _db;

        public TeacherClassService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<ClassItem> GetAll()
        {
            return _db.Lops
                .OrderBy(l => l.TenLop)
                .Select(l => new ClassItem
                {
                    Id = l.Id,
                    Msgv = l.Msgv,
                    TenGiaoVien = l.MsgvNavigation.HoTen,
                    TenLop = l.TenLop,
                    KiHoc = l.KiHoc,
                    ChuyenNganh = l.ChuyenNganh,
                    SoSinhVien = l.SinhVienLops.Count(svl => svl.DangThamGia),
                    SoBaiTap = l.BaiTaps.Count
                })
                .ToList();
        }

        public List<GiaoVienOption> GetAllGiaoViens()
        {
            return _db.GiaoViens
                .OrderBy(gv => gv.HoTen)
                .Select(gv => new GiaoVienOption { Msgv = gv.Msgv, HoTen = gv.HoTen })
                .ToList();
        }

        public ClassItem Add(CreateClassInput input)
        {
            var giaoVien = _db.GiaoViens.Find(input.Msgv)
                ?? throw new InvalidOperationException($"Giáo viên '{input.Msgv}' không tồn tại.");

            var tenLop = input.TenLop.Trim();
            var kiHoc = input.KiHoc.Trim();
            var chuyenNganh = input.ChuyenNganh.Trim();

            var trung = _db.Lops.Any(l =>
                l.TenLop == tenLop && l.KiHoc == kiHoc && l.ChuyenNganh == chuyenNganh);
            if (trung)
                throw new InvalidOperationException(
                    $"Lớp '{tenLop}' (kỳ {kiHoc}, {chuyenNganh}) đã tồn tại.");

            var lop = new Lop
            {
                Msgv = input.Msgv,
                TenLop = tenLop,
                KiHoc = kiHoc,
                ChuyenNganh = chuyenNganh
            };

            _db.Lops.Add(lop);
            _db.SaveChanges();

            return new ClassItem
            {
                Id = lop.Id,
                Msgv = lop.Msgv,
                TenGiaoVien = giaoVien.HoTen,
                TenLop = lop.TenLop,
                KiHoc = lop.KiHoc,
                ChuyenNganh = lop.ChuyenNganh,
                SoSinhVien = 0,
                SoBaiTap = 0
            };
        }

        public ClassItem? Update(UpdateClassInput input)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == input.Id);
            if (lop == null) return null;

            var giaoVien = _db.GiaoViens.Find(input.Msgv)
                ?? throw new InvalidOperationException($"Giáo viên '{input.Msgv}' không tồn tại.");

            var tenLop = input.TenLop.Trim();
            var kiHoc = input.KiHoc.Trim();
            var chuyenNganh = input.ChuyenNganh.Trim();

            var trung = _db.Lops.Any(l =>
                l.Id != input.Id && l.TenLop == tenLop && l.KiHoc == kiHoc && l.ChuyenNganh == chuyenNganh);
            if (trung)
                throw new InvalidOperationException(
                    $"Lớp '{tenLop}' (kỳ {kiHoc}, {chuyenNganh}) đã tồn tại.");

            lop.Msgv = input.Msgv;
            lop.TenLop = tenLop;
            lop.KiHoc = kiHoc;
            lop.ChuyenNganh = chuyenNganh;

            _db.SaveChanges();

            return new ClassItem
            {
                Id = lop.Id,
                Msgv = lop.Msgv,
                TenGiaoVien = giaoVien.HoTen,
                TenLop = lop.TenLop,
                KiHoc = lop.KiHoc,
                ChuyenNganh = lop.ChuyenNganh,
                SoSinhVien = _db.SinhVienLops.Count(svl => svl.LopId == lop.Id && svl.DangThamGia),
                SoBaiTap = _db.BaiTaps.Count(bt => bt.LopId == lop.Id)
            };
        }

        public bool Delete(int id)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == id);
            if (lop == null) return false;

            // Xóa lớp sẽ cascade xóa bai_tap, bai_nop và sinh_vien_lop liên quan (đã cấu hình ở DB).
            _db.Lops.Remove(lop);
            _db.SaveChanges();
            return true;
        }

        // ---- Quản lý sinh viên trong lớp ----

        public List<ClassEnrollmentItem> GetEnrollments(int lopId)
        {
            return _db.SinhVienLops
                .Where(svl => svl.LopId == lopId)
                .OrderByDescending(svl => svl.DangThamGia)
                .ThenBy(svl => svl.Mssv)
                .Select(svl => new ClassEnrollmentItem
                {
                    LopId = svl.LopId,
                    Mssv = svl.Mssv,
                    HoTenSinhVien = svl.MssvNavigation.HoTen,
                    Email = svl.MssvNavigation.Email,
                    DangThamGia = svl.DangThamGia,
                    NgayThamGia = svl.NgayThamGia,
                    NgayRoiLop = svl.NgayRoiLop
                })
                .ToList();
        }

        public List<StudentOption> GetAvailableStudents(int lopId)
        {
            var daThamGia = _db.SinhVienLops
                .Where(svl => svl.LopId == lopId)
                .Select(svl => svl.Mssv);

            return _db.SinhViens
                .Where(sv => !daThamGia.Contains(sv.Mssv))
                .OrderBy(sv => sv.HoTen)
                .Select(sv => new StudentOption { Mssv = sv.Mssv, HoTen = sv.HoTen })
                .ToList();
        }

        public void AddStudentToClass(int lopId, string mssv)
        {
            if (!_db.Lops.Any(l => l.Id == lopId))
                throw new InvalidOperationException("Lớp không tồn tại.");

            if (!_db.SinhViens.Any(sv => sv.Mssv == mssv))
                throw new InvalidOperationException($"Sinh viên '{mssv}' không tồn tại.");

            if (_db.SinhVienLops.Any(svl => svl.LopId == lopId && svl.Mssv == mssv))
                throw new InvalidOperationException("Sinh viên đã từng thuộc lớp này (xem lại trạng thái bên dưới).");

            _db.SinhVienLops.Add(new SinhVienLop
            {
                LopId = lopId,
                Mssv = mssv,
                DangThamGia = true,
                NgayThamGia = DateTime.Now
            });

            _db.SaveChanges();
        }

        public void RemoveStudentFromClass(int lopId, string mssv)
        {
            var svl = _db.SinhVienLops.FirstOrDefault(x => x.LopId == lopId && x.Mssv == mssv)
                ?? throw new InvalidOperationException("Không tìm thấy sinh viên trong lớp này.");

            svl.DangThamGia = false;
            svl.NgayRoiLop = DateTime.Now;

            _db.SaveChanges();
        }

        public void ReactivateStudentInClass(int lopId, string mssv)
        {
            var svl = _db.SinhVienLops.FirstOrDefault(x => x.LopId == lopId && x.Mssv == mssv)
                ?? throw new InvalidOperationException("Không tìm thấy sinh viên trong lớp này.");

            svl.DangThamGia = true;
            svl.NgayThamGia = DateTime.Now;
            svl.NgayRoiLop = null;

            _db.SaveChanges();
        }
    }
}
