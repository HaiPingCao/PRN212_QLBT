namespace QLBT.Server.Models
{
    /// <summary>
    /// Cac lop hien thi/nhap lieu dung rieng cho man hinh admin cuc bo (QLBT.Server),
    /// khong di qua TCP nen khong dat trong QLBT.Shared.
    /// </summary>
    public class HocKyAdminItem
    {
        public int Id { get; set; }
        public string TenHocKy { get; set; } = "";
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public int SoLop { get; set; }
    }

    public class GiaoVienAdminItem
    {
        public string Msgv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public int SoLopDangDay { get; set; }
    }

    public class LopAdminItem
    {
        public int Id { get; set; }
        public string Msgv { get; set; } = "";
        public string TenGiaoVien { get; set; } = "";
        public string TenLop { get; set; } = "";
        public int HocKyId { get; set; }
        public string TenHocKy { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";
        public int SoSinhVien { get; set; }
        public int SoBaiTap { get; set; }
    }

    /// <summary>Mot dong sinh_vien_lop trong lop, kem thong tin sinh vien, dung cho panel quan ly sinh vien trong lop.</summary>
    public class ClassEnrollmentAdminItem
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public bool DangThamGia { get; set; }
        public DateTime NgayThamGia { get; set; }
        public DateTime? NgayRoiLop { get; set; }
    }

    /// <summary>Ket qua them nhieu sinh vien vao lop cung luc.</summary>
    public class AddStudentsToClassResult
    {
        public List<string> Added { get; set; } = new();
        public List<string> NotFound { get; set; } = new();
        public List<string> AlreadyInClass { get; set; } = new();
    }

    /// <summary>Boc StudentDto them co chon, dung cho DataGrid checkbox de them nhieu sinh vien cung luc.</summary>
    public class SelectableStudent
    {
        public bool IsSelected { get; set; }
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
