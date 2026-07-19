using System;
using System.Collections.Generic;
using System.Text;

namespace QLBT.Server.Models
{
    public class ClassItem
    {
        public int Id { get; set; }

        public string TenLop { get; set; } = "";

        public string KiHoc { get; set; } = "";

        public string ChuyenNganh { get; set; } = "";

        public int SoSinhVien { get; set; }
    }
}
