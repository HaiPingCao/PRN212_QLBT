using System;
using System.Collections.Generic;
using System.Text;

namespace QLBT.Server.Models
{
    public class UpdateStudentInput
    {
        public string MSSV { get; set; } = "";

        public string HoTen { get; set; } = "";

        public string Email { get; set; } = "";

        // Nếu để trống thì giữ nguyên mật khẩu cũ
        public string? Password { get; set; }

        public int LopId { get; set; }

        public bool DangThamGia { get; set; }
    }
}
