namespace QLBT.Server.Models;

public partial class HocKi
{
    public int Id { get; set; }

    public string TenHocKy { get; set; } = null!;

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public virtual ICollection<Lop> Lops { get; set; } = new List<Lop>();
}
