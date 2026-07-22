namespace QLBT.Server.Models;

public partial class SinhVien
{
    public string Mssv { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public virtual ICollection<SinhVienLop> SinhVienLops { get; set; } = new List<SinhVienLop>();
}
