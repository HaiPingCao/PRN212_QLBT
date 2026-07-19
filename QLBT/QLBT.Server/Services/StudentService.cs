using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    public class StudentService : IStudentService
    {
        private readonly Prn212PQlbtContext _db;

        public StudentService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<StudentItem> GetAll()
        {
            return _db.SinhVienLops

                .Include(x => x.MssvNavigation)

                .Include(x => x.Lop)

                .Select(x => new StudentItem
                {
                    MSSV = x.Mssv,

                    HoTen = x.MssvNavigation.HoTen,

                    Email = x.MssvNavigation.Email,

                    LopId = x.LopId,

                    TenLop = x.Lop.TenLop,

                    DangThamGia = x.DangThamGia
                })

                .OrderBy(x => x.MSSV)

                .ToList();
        }

        public List<LopOption> GetAllClasses()
        {
            return _db.Lops

                .OrderBy(x => x.TenLop)

                .Select(x => new LopOption
                {
                    Id = x.Id,
                    TenLop = x.TenLop,
                    KiHoc = x.KiHoc,
                    ChuyenNganh = x.ChuyenNganh
                })

                .ToList();
        }

        public StudentItem Add(CreateStudentInput input)
        {
            // Kiểm tra MSSV
            if (_db.SinhViens.Any(x => x.Mssv == input.MSSV))
                throw new Exception("MSSV đã tồn tại.");

            // Kiểm tra Email
            if (_db.SinhViens.Any(x => x.Email == input.Email))
                throw new Exception("Email đã tồn tại.");

            // Tạo sinh viên
            var sv = new SinhVien
            {
                Mssv = input.MSSV,
                HoTen = input.HoTen,
                Email = input.Email,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };

            _db.SinhViens.Add(sv);

            // Thêm vào lớp
            var svl = new SinhVienLop
            {
                LopId = input.LopId,
                Mssv = input.MSSV,
                DangThamGia = true,
                NgayThamGia = DateTime.Now
            };

            _db.SinhVienLops.Add(svl);

            _db.SaveChanges();

            var lop = _db.Lops.First(x => x.Id == input.LopId);

            return new StudentItem
            {
                MSSV = sv.Mssv,
                HoTen = sv.HoTen,
                Email = sv.Email,
                LopId = lop.Id,
                TenLop = lop.TenLop,
                DangThamGia = true
            };
        }

        public StudentItem? Update(UpdateStudentInput input)
        {
            var sv = _db.SinhViens.FirstOrDefault(x => x.Mssv == input.MSSV);

            if (sv == null)
                return null;

            sv.HoTen = input.HoTen;
            sv.Email = input.Email;

            // Chỉ đổi mật khẩu nếu giáo viên nhập
            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                sv.MatKhau = BCrypt.Net.BCrypt.HashPassword(input.Password);
            }

            var svl = _db.SinhVienLops.FirstOrDefault(x => x.Mssv == input.MSSV);

            if (svl != null)
            {
                svl.LopId = input.LopId;
                svl.DangThamGia = input.DangThamGia;

                if (!input.DangThamGia)
                    svl.NgayRoiLop = DateTime.Now;
                else
                    svl.NgayRoiLop = null;
            }

            _db.SaveChanges();

            var lop = _db.Lops.First(x => x.Id == input.LopId);

            return new StudentItem
            {
                MSSV = sv.Mssv,
                HoTen = sv.HoTen,
                Email = sv.Email,
                LopId = lop.Id,
                TenLop = lop.TenLop,
                DangThamGia = input.DangThamGia
            };
        }

        public bool Delete(string mssv)
        {
            var sv = _db.SinhViens.FirstOrDefault(x => x.Mssv == mssv);

            if (sv == null)
                return false;

            var svl = _db.SinhVienLops.Where(x => x.Mssv == mssv);

            _db.SinhVienLops.RemoveRange(svl);

            _db.SinhViens.Remove(sv);

            _db.SaveChanges();

            return true;
        }
    }
}