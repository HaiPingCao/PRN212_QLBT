namespace QLBT.Server.Models;

public partial class BaiTap
{
    public int Id { get; set; }

    public int LopId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? TenFileDe { get; set; }

    public string? DuongDanFileDe { get; set; }

    public DateTime HanNop { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<BaiNop> BaiNops { get; set; } = new List<BaiNop>();

    public virtual Lop Lop { get; set; } = null!;
}
