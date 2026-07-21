using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBT.Server.Models
{
    /// <summary>
    /// Dữ liệu hiển thị lớp trên UI giáo viên (DataGrid), kèm tên giáo viên chủ nhiệm,
    /// số sinh viên đang tham gia và số bài tập.
    /// </summary>
    public class ClassItem
    {
        public int Id { get; set; }
        public string Msgv { get; set; } = "";
        public string TenGiaoVien { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string KiHoc { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";
        public int SoSinhVien { get; set; }
        public int SoBaiTap { get; set; }
    }

    /// <summary>Dữ liệu đầu vào khi Thêm mới lớp.</summary>
    public class CreateClassInput
    {
        public string Msgv { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string KiHoc { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";
    }

    /// <summary>Dữ liệu đầu vào khi Sửa lớp, bao gồm cả đổi giáo viên chủ nhiệm.</summary>
    public class UpdateClassInput
    {
        public int Id { get; set; }
        public string Msgv { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string KiHoc { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";
    }

    /// <summary>Giáo viên dùng cho ComboBox chọn giáo viên chủ nhiệm.</summary>
    public class GiaoVienOption
    {
        public string Msgv { get; set; } = "";
        public string HoTen { get; set; } = "";

        public override string ToString() => $"{HoTen} ({Msgv})";
    }

    /// <summary>Dòng sinh viên trong lớp (bảng sinh_vien_lop), kèm thông tin sinh viên.</summary>
    public class ClassEnrollmentItem
    {
        public int LopId { get; set; }
        public string Mssv { get; set; } = "";
        public string HoTenSinhVien { get; set; } = "";
        public string Email { get; set; } = "";
        public bool DangThamGia { get; set; }
        public DateTime NgayThamGia { get; set; }
        public DateTime? NgayRoiLop { get; set; }
    }

    /// <summary>Sinh viên chưa từng thuộc lớp đang xét, dùng cho ComboBox thêm vào lớp.</summary>
    public class StudentOption
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";

        public override string ToString() => $"{HoTen} ({Mssv})";
    }
}

