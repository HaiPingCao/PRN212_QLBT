using System;
using System.Collections.Generic;

namespace QLBT.Server.Models;

public partial class Lop
{
    public int Id { get; set; }

    public string Msgv { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public string KiHoc { get; set; } = null!;

    public string ChuyenNganh { get; set; } = null!;

    public virtual ICollection<BaiTap> BaiTaps { get; set; } = new List<BaiTap>();

    public virtual GiaoVien MsgvNavigation { get; set; } = null!;

    public virtual ICollection<SinhVienLop> SinhVienLops { get; set; } = new List<SinhVienLop>();
}
