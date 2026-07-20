using System;

namespace QLBT.Server.Models
{
    /// <summary>
    /// Dữ liệu hiển thị bài nộp trên UI giáo viên (DataGrid), gộp thông tin sinh viên/lớp/bài tập.
    /// </summary>
    public class TeacherSubmissionItem
    {
        public int Id { get; set; }
        public string Mssv { get; set; } = "";
        public string HoTenSinhVien { get; set; } = "";
        public int LopId { get; set; }
        public string TenLop { get; set; } = "";
        public int BaiTapId { get; set; }
        public string TieuDeBaiTap { get; set; } = "";
        public string TenFile { get; set; } = "";
        public int SoLanNop { get; set; }
        public DateTime NgayNop { get; set; }
    }

    /// <summary>Bài tập dùng cho ComboBox lọc, kèm LopId để lọc theo lớp đang chọn.</summary>
    public class BaiTapOption
    {
        public int Id { get; set; }
        public int LopId { get; set; }
        public string TieuDe { get; set; } = "";

        public override string ToString() => TieuDe;
    }
}
