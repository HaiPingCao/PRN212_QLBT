using System;
using System.Collections.Generic;
using System.Text;

namespace QLBT.Server.Models
{
    public class CreateStudentInput
    {
        public string MSSV { get; set; } = "";

        public string HoTen { get; set; } = "";

        public string Email { get; set; } = "";

        public string Password { get; set; } = "";

        public int LopId { get; set; }
    }
}