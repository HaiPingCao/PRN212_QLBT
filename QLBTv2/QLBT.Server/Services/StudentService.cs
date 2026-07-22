using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class StudentService : IStudentService
    {
        private readonly Prn212PQlbtContext _db;

        public StudentService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public List<StudentDto> GetAll()
        {
            return _db.SinhViens
                .OrderBy(sv => sv.Mssv)
                .Select(sv => new StudentDto
                {
                    Mssv = sv.Mssv,
                    HoTen = sv.HoTen,
                    Email = sv.Email,
                    SoLopDangHoc = sv.SinhVienLops.Count(svl => svl.DangThamGia)
                })
                .ToList();
        }

        public StudentDto Add(CreateStudentRequest input)
        {
            var mssv = input.Mssv.Trim();

            if (_db.SinhViens.Any(s => s.Mssv == mssv))
                throw new InvalidOperationException($"MSSV '{mssv}' đã tồn tại.");

            var email = input.Email.Trim();
            if (_db.SinhViens.Any(s => s.Email == email))
                throw new InvalidOperationException($"Email '{email}' đã được sử dụng bởi sinh viên khác.");
            if (_db.GiaoViens.Any(g => g.Email == email))
                throw new InvalidOperationException($"Email '{email}' đã được sử dụng bởi một giáo viên.");

            if (string.IsNullOrWhiteSpace(input.MatKhau))
                throw new InvalidOperationException("Vui lòng nhập mật khẩu cho sinh viên mới.");

            var sv = new SinhVien
            {
                Mssv = mssv,
                HoTen = input.HoTen.Trim(),
                Email = email,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(input.MatKhau)
            };

            _db.SinhViens.Add(sv);
            _db.SaveChanges();

            return new StudentDto { Mssv = sv.Mssv, HoTen = sv.HoTen, Email = sv.Email, SoLopDangHoc = 0 };
        }

        public StudentDto? Update(UpdateStudentRequest input)
        {
            var sv = _db.SinhViens.FirstOrDefault(s => s.Mssv == input.Mssv);
            if (sv == null) return null;

            var email = input.Email.Trim();
            if (_db.SinhViens.Any(s => s.Email == email && s.Mssv != input.Mssv))
                throw new InvalidOperationException($"Email '{email}' đã được sử dụng bởi sinh viên khác.");
            if (_db.GiaoViens.Any(g => g.Email == email))
                throw new InvalidOperationException($"Email '{email}' đã được sử dụng bởi một giáo viên.");

            sv.HoTen = input.HoTen.Trim();
            sv.Email = email;

            if (!string.IsNullOrWhiteSpace(input.MatKhauMoi))
                sv.MatKhau = BCrypt.Net.BCrypt.HashPassword(input.MatKhauMoi);

            _db.SaveChanges();

            return new StudentDto
            {
                Mssv = sv.Mssv,
                HoTen = sv.HoTen,
                Email = sv.Email,
                SoLopDangHoc = _db.SinhVienLops.Count(svl => svl.Mssv == sv.Mssv && svl.DangThamGia)
            };
        }

        public bool Delete(string mssv)
        {
            var sv = _db.SinhViens.FirstOrDefault(s => s.Mssv == mssv);
            if (sv == null) return false;

            var soLanThamGia = _db.SinhVienLops.Count(svl => svl.Mssv == mssv);
            if (soLanThamGia > 0)
                throw new InvalidOperationException(
                    $"Không thể xóa: sinh viên '{mssv}' còn dữ liệu tham gia ở {soLanThamGia} lớp. " +
                    "Hãy cho sinh viên rời khỏi các lớp đó trước (mục Quản lý lớp).");

            _db.SinhViens.Remove(sv);
            _db.SaveChanges();
            return true;
        }
    }
}
