using System;
using System.Collections.Generic;

namespace QLBT.Server.Models;

public partial class GiaoVien
{
    public string Msgv { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
}
