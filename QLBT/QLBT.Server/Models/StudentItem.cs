using System;
using System.Collections.Generic;
using System.Text;

namespace QLBT.Server.Models
{
    public class StudentItem
    {
        public string MSSV { get; set; } = "";

        public string HoTen { get; set; } = "";

        public string Email { get; set; } = "";

        public int LopId { get; set; }

        public string TenLop { get; set; } = "";

        public bool DangThamGia { get; set; }
    }
}