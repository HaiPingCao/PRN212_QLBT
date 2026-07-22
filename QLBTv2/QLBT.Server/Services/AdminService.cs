using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    public class AdminService : IAdminService
    {
        private readonly Prn212PQlbtContext _db;

        public AdminService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        // -------------------------------------------------------
        // Hoc ky
        // -------------------------------------------------------

        public List<HocKyAdminItem> GetAllHocKy()
        {
            return _db.HocKis
                .OrderByDescending(h => h.Id)
                .Select(h => new HocKyAdminItem
                {
                    Id = h.Id,
                    TenHocKy = h.TenHocKy,
                    NgayBatDau = h.NgayBatDau,
                    NgayKetThuc = h.NgayKetThuc,
                    SoLop = h.Lops.Count
                })
                .ToList();
        }

        public HocKyAdminItem AddHocKy(string tenHocKy, DateOnly? ngayBatDau, DateOnly? ngayKetThuc)
        {
            var ten = tenHocKy.Trim();

            if (_db.HocKis.Any(h => h.TenHocKy == ten))
                throw new InvalidOperationException($"Hoc ky '{ten}' da ton tai.");

            var hocKy = new HocKi { TenHocKy = ten, NgayBatDau = ngayBatDau, NgayKetThuc = ngayKetThuc };
            _db.HocKis.Add(hocKy);
            _db.SaveChanges();

            return new HocKyAdminItem { Id = hocKy.Id, TenHocKy = hocKy.TenHocKy, NgayBatDau = hocKy.NgayBatDau, NgayKetThuc = hocKy.NgayKetThuc, SoLop = 0 };
        }

        public HocKyAdminItem? UpdateHocKy(int id, string tenHocKy, DateOnly? ngayBatDau, DateOnly? ngayKetThuc)
        {
            var hocKy = _db.HocKis.FirstOrDefault(h => h.Id == id);
            if (hocKy == null) return null;

            var ten = tenHocKy.Trim();
            if (_db.HocKis.Any(h => h.Id != id && h.TenHocKy == ten))
                throw new InvalidOperationException($"Hoc ky '{ten}' da ton tai.");

            hocKy.TenHocKy = ten;
            hocKy.NgayBatDau = ngayBatDau;
            hocKy.NgayKetThuc = ngayKetThuc;
            _db.SaveChanges();

            return new HocKyAdminItem
            {
                Id = hocKy.Id,
                TenHocKy = hocKy.TenHocKy,
                NgayBatDau = hocKy.NgayBatDau,
                NgayKetThuc = hocKy.NgayKetThuc,
                SoLop = _db.Lops.Count(l => l.HocKiId == id)
            };
        }

        public bool DeleteHocKy(int id)
        {
            var hocKy = _db.HocKis.FirstOrDefault(h => h.Id == id);
            if (hocKy == null) return false;

            if (_db.Lops.Any(l => l.HocKiId == id))
                throw new InvalidOperationException("Khong the xoa: hoc ky nay dang duoc gan cho it nhat mot lop.");

            _db.HocKis.Remove(hocKy);
            _db.SaveChanges();
            return true;
        }

        // -------------------------------------------------------
        // Giao vien
        // -------------------------------------------------------

        public List<GiaoVienAdminItem> GetAllGiaoVien()
        {
            return _db.GiaoViens
                .OrderBy(gv => gv.HoTen)
                .Select(gv => new GiaoVienAdminItem
                {
                    Msgv = gv.Msgv,
                    HoTen = gv.HoTen,
                    Email = gv.Email,
                    SoLopDangDay = gv.Lops.Count
                })
                .ToList();
        }

        public GiaoVienAdminItem AddGiaoVien(string msgv, string hoTen, string email, string matKhau)
        {
            var id = msgv.Trim();

            if (_db.GiaoViens.Any(g => g.Msgv == id))
                throw new InvalidOperationException($"Ma so giao vien '{id}' da ton tai.");

            var em = email.Trim();
            if (_db.GiaoViens.Any(g => g.Email == em))
                throw new InvalidOperationException($"Email '{em}' da duoc su dung.");
            if (_db.SinhViens.Any(s => s.Email == em))
                throw new InvalidOperationException($"Email '{em}' da duoc su dung boi mot sinh vien.");

            if (string.IsNullOrWhiteSpace(matKhau))
                throw new InvalidOperationException("Vui long nhap mat khau cho giao vien moi.");

            var gv = new GiaoVien
            {
                Msgv = id,
                HoTen = hoTen.Trim(),
                Email = em,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhau)
            };

            _db.GiaoViens.Add(gv);
            _db.SaveChanges();

            return new GiaoVienAdminItem { Msgv = gv.Msgv, HoTen = gv.HoTen, Email = gv.Email, SoLopDangDay = 0 };
        }

        public GiaoVienAdminItem? UpdateGiaoVien(string msgv, string hoTen, string email, string? matKhauMoi)
        {
            var gv = _db.GiaoViens.FirstOrDefault(g => g.Msgv == msgv);
            if (gv == null) return null;

            var em = email.Trim();
            if (_db.GiaoViens.Any(g => g.Email == em && g.Msgv != msgv))
                throw new InvalidOperationException($"Email '{em}' da duoc su dung.");
            if (_db.SinhViens.Any(s => s.Email == em))
                throw new InvalidOperationException($"Email '{em}' da duoc su dung boi mot sinh vien.");

            gv.HoTen = hoTen.Trim();
            gv.Email = em;

            if (!string.IsNullOrWhiteSpace(matKhauMoi))
                gv.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);

            _db.SaveChanges();

            return new GiaoVienAdminItem
            {
                Msgv = gv.Msgv,
                HoTen = gv.HoTen,
                Email = gv.Email,
                SoLopDangDay = _db.Lops.Count(l => l.Msgv == msgv)
            };
        }

        public bool DeleteGiaoVien(string msgv)
        {
            var gv = _db.GiaoViens.FirstOrDefault(g => g.Msgv == msgv);
            if (gv == null) return false;

            if (_db.Lops.Any(l => l.Msgv == msgv))
                throw new InvalidOperationException("Khong the xoa: giao vien nay dang chu nhiem it nhat mot lop.");

            _db.GiaoViens.Remove(gv);
            _db.SaveChanges();
            return true;
        }

        // -------------------------------------------------------
        // Lop (toan quyen)
        // -------------------------------------------------------

        public List<LopAdminItem> GetAllLop()
        {
            return _db.Lops
                .OrderBy(l => l.TenLop)
                .Select(l => new LopAdminItem
                {
                    Id = l.Id,
                    Msgv = l.Msgv,
                    TenGiaoVien = l.MsgvNavigation.HoTen,
                    TenLop = l.TenLop,
                    HocKyId = l.HocKiId,
                    TenHocKy = l.HocKi.TenHocKy,
                    ChuyenNganh = l.ChuyenNganh,
                    SoSinhVien = l.SinhVienLops.Count(svl => svl.DangThamGia),
                    SoBaiTap = l.BaiTaps.Count
                })
                .ToList();
        }

        public LopAdminItem AddLop(string msgv, string tenLop, int hocKyId, string chuyenNganh)
        {
            var giaoVien = _db.GiaoViens.Find(msgv)
                ?? throw new InvalidOperationException($"Giao vien '{msgv}' khong ton tai.");

            var hocKy = _db.HocKis.Find(hocKyId)
                ?? throw new InvalidOperationException("Hoc ky khong ton tai.");

            var ten = tenLop.Trim();
            var nganh = chuyenNganh.Trim();

            if (_db.Lops.Any(l => l.TenLop == ten && l.HocKiId == hocKyId && l.ChuyenNganh == nganh))
                throw new InvalidOperationException($"Lop '{ten}' (hoc ky {hocKy.TenHocKy}, {nganh}) da ton tai.");

            var lop = new Lop { Msgv = msgv, TenLop = ten, HocKiId = hocKyId, ChuyenNganh = nganh };
            _db.Lops.Add(lop);
            _db.SaveChanges();

            return new LopAdminItem
            {
                Id = lop.Id,
                Msgv = lop.Msgv,
                TenGiaoVien = giaoVien.HoTen,
                TenLop = lop.TenLop,
                HocKyId = lop.HocKiId,
                TenHocKy = hocKy.TenHocKy,
                ChuyenNganh = lop.ChuyenNganh,
                SoSinhVien = 0,
                SoBaiTap = 0
            };
        }

        public LopAdminItem? UpdateLop(int id, string msgv, string tenLop, int hocKyId, string chuyenNganh)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == id);
            if (lop == null) return null;

            var giaoVien = _db.GiaoViens.Find(msgv)
                ?? throw new InvalidOperationException($"Giao vien '{msgv}' khong ton tai.");

            var hocKy = _db.HocKis.Find(hocKyId)
                ?? throw new InvalidOperationException("Hoc ky khong ton tai.");

            var ten = tenLop.Trim();
            var nganh = chuyenNganh.Trim();

            if (_db.Lops.Any(l => l.Id != id && l.TenLop == ten && l.HocKiId == hocKyId && l.ChuyenNganh == nganh))
                throw new InvalidOperationException($"Lop '{ten}' (hoc ky {hocKy.TenHocKy}, {nganh}) da ton tai.");

            lop.Msgv = msgv;
            lop.TenLop = ten;
            lop.HocKiId = hocKyId;
            lop.ChuyenNganh = nganh;
            _db.SaveChanges();

            return new LopAdminItem
            {
                Id = lop.Id,
                Msgv = lop.Msgv,
                TenGiaoVien = giaoVien.HoTen,
                TenLop = lop.TenLop,
                HocKyId = lop.HocKiId,
                TenHocKy = hocKy.TenHocKy,
                ChuyenNganh = lop.ChuyenNganh,
                SoSinhVien = _db.SinhVienLops.Count(svl => svl.LopId == lop.Id && svl.DangThamGia),
                SoBaiTap = _db.BaiTaps.Count(bt => bt.LopId == lop.Id)
            };
        }

        public bool DeleteLop(int id)
        {
            var lop = _db.Lops.FirstOrDefault(l => l.Id == id);
            if (lop == null) return false;

            // Xoa lop se cascade xoa bai_tap, bai_nop va sinh_vien_lop lien quan (da cau hinh o DB).
            _db.Lops.Remove(lop);
            _db.SaveChanges();
            return true;
        }

        // -------------------------------------------------------
        // Sinh vien trong lop (toan quyen)
        // -------------------------------------------------------

        public List<ClassEnrollmentAdminItem> GetClassStudents(int classId)
        {
            return _db.SinhVienLops
                .Where(svl => svl.LopId == classId)
                .OrderByDescending(svl => svl.DangThamGia)
                .ThenBy(svl => svl.Mssv)
                .Select(svl => new ClassEnrollmentAdminItem
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

        public List<SelectableStudent> GetAvailableStudents(int classId)
        {
            var daThamGia = _db.SinhVienLops
                .Where(svl => svl.LopId == classId)
                .Select(svl => svl.Mssv);

            return _db.SinhViens
                .Where(sv => !daThamGia.Contains(sv.Mssv))
                .OrderBy(sv => sv.HoTen)
                .Select(sv => new SelectableStudent { Mssv = sv.Mssv, HoTen = sv.HoTen, Email = sv.Email })
                .ToList();
        }

        public AddStudentsToClassResult AddStudentsToClass(int classId, List<string> studentIds)
        {
            var result = new AddStudentsToClassResult();

            if (!_db.Lops.Any(l => l.Id == classId))
            {
                result.NotFound.AddRange(studentIds);
                return result;
            }

            var alreadyInClass = _db.SinhVienLops
                .Where(svl => svl.LopId == classId)
                .Select(svl => svl.Mssv)
                .ToHashSet();

            foreach (var mssv in studentIds.Distinct())
            {
                if (alreadyInClass.Contains(mssv))
                {
                    result.AlreadyInClass.Add(mssv);
                    continue;
                }

                if (!_db.SinhViens.Any(s => s.Mssv == mssv))
                {
                    result.NotFound.Add(mssv);
                    continue;
                }

                _db.SinhVienLops.Add(new SinhVienLop { LopId = classId, Mssv = mssv });
                result.Added.Add(mssv);
            }

            _db.SaveChanges();
            return result;
        }

        public bool RemoveStudentFromClass(int classId, string studentId)
        {
            var svl = _db.SinhVienLops.FirstOrDefault(s => s.LopId == classId && s.Mssv == studentId);
            if (svl == null) return false;

            svl.DangThamGia = false;
            svl.NgayRoiLop = DateTime.Now;
            _db.SaveChanges();
            return true;
        }

        public bool ReactivateStudentInClass(int classId, string studentId)
        {
            var svl = _db.SinhVienLops.FirstOrDefault(s => s.LopId == classId && s.Mssv == studentId);
            if (svl == null) return false;

            svl.DangThamGia = true;
            svl.NgayThamGia = DateTime.Now;
            svl.NgayRoiLop = null;
            _db.SaveChanges();
            return true;
        }
    }
}
