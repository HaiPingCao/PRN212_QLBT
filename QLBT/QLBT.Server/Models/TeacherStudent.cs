using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBT.Server.Models
{
    /// <summary>
    /// Dữ liệu hiển thị sinh viên trên UI giáo viên (DataGrid), kèm số lớp đang tham gia.
    /// </summary>
    public class StudentItem
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";

        /// <summary>Số lớp mà sinh viên đang tham gia (DangThamGia = true).</summary>
        public int SoLopDangHoc { get; set; }
    }

    /// <summary>
    /// Dữ liệu đầu vào khi Thêm mới sinh viên. MatKhau là mật khẩu dạng chữ thường,
    /// sẽ được băm bằng BCrypt trước khi lưu.
    /// </summary>
    public class CreateStudentInput
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";
    }

    /// <summary>
    /// Dữ liệu đầu vào khi Sửa sinh viên. Mssv không đổi được (là khóa chính).
    /// </summary>
    public class UpdateStudentInput
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";

        /// <summary>Mật khẩu mới (chữ thường). Null hoặc rỗng nghĩa là giữ nguyên mật khẩu cũ.</summary>
        public string? MatKhauMoi { get; set; }
    }
}
