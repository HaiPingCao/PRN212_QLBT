using System;

namespace QLBT.Server.Models
{
    /// <summary>
    /// Dữ liệu hiển thị bài tập trên UI giáo viên (DataGrid).
    /// Khác AssignmentDto (QLBT.Shared) vốn dùng để trả về cho client qua TCP.
    /// </summary>
    public class TeacherAssignmentItem
    {
        public int Id { get; set; }
        public int LopId { get; set; }
        public string TenLop { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string? MoTa { get; set; }
        public string? TenFileDe { get; set; }
        public DateTime HanNop { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoBaiNop { get; set; }
    }

    /// <summary>
    /// Dữ liệu đầu vào khi Thêm mới bài tập.
    /// </summary>
    public class CreateAssignmentInput
    {
        public int LopId { get; set; }
        public string TieuDe { get; set; } = "";
        public string? MoTa { get; set; }
        public DateTime HanNop { get; set; }

        /// <summary>Đường dẫn file đề trên máy giáo viên (sẽ được copy vào RootFolder). Null nếu không đính kèm file.</summary>
        public string? SourceFilePath { get; set; }
    }

    /// <summary>
    /// Dữ liệu đầu vào khi Sửa bài tập. Không bao gồm LopId — không cho phép đổi lớp khi sửa.
    /// </summary>
    public class UpdateAssignmentInput
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = "";
        public string? MoTa { get; set; }
        public DateTime HanNop { get; set; }

        /// <summary>Đường dẫn file đề mới trên máy giáo viên, nếu giáo viên chọn thay file. Null = giữ nguyên file cũ.</summary>
        public string? SourceFilePath { get; set; }
    }

    public class LopOption
    {
        public int Id { get; set; }
        public string TenLop { get; set; } = "";
        public string KiHoc { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";

        public override string ToString() => $"{TenLop} ({KiHoc} - {ChuyenNganh})";
    }
}
