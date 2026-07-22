namespace QLBT.Server.Models;

public partial class SinhVienLop
{
    public int LopId { get; set; }

    public string Mssv { get; set; } = null!;

    public bool DangThamGia { get; set; }

    public DateTime NgayThamGia { get; set; }

    public DateTime? NgayRoiLop { get; set; }

    public virtual ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();

    public virtual Lop Lop { get; set; } = null!;

    public virtual SinhVien MssvNavigation { get; set; } = null!;
}
