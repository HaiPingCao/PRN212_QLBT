using System;
using System.Collections.Generic;

namespace QLBT.Server.Models;

public partial class BaiNop
{
    public int Id { get; set; }

    public int BaiTapId { get; set; }

    public int LopId { get; set; }

    public string Mssv { get; set; } = null!;

    public string TenFile { get; set; } = null!;

    public string DuongDanFile { get; set; } = null!;

    public int SoLanNop { get; set; }

    public DateTime NgayNop { get; set; }

    public virtual BaiTap BaiTap { get; set; } = null!;

    public virtual SinhVienLop SinhVienLop { get; set; } = null!;
}
