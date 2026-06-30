using QLBT.Server.Models;
using System.Collections.Generic;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quản lý bài tập từ phía giáo viên (UI Server), khác IAssignmentService
    /// vốn phục vụ truy vấn của sinh viên qua TCP.
    /// </summary>
    public interface ITeacherAssignmentService
    {
        /// <summary>Toàn bộ bài tập trong hệ thống, kèm tên lớp và số bài đã nộp.</summary>
        List<TeacherAssignmentItem> GetAll();

        /// <summary>Toàn bộ lớp trong hệ thống, dùng cho ComboBox chọn lớp.</summary>
        List<LopOption> GetAllLops();

        /// <summary>Tạo bài tập mới. Nếu input.SourceFilePath khác null, copy file đề vào RootFolder.</summary>
        TeacherAssignmentItem Add(CreateAssignmentInput input);

        /// <summary>Cập nhật bài tập đã có. Không cho phép đổi lớp.</summary>
        TeacherAssignmentItem? Update(UpdateAssignmentInput input);

        /// <summary>Xóa bài tập. Trả về false nếu không tìm thấy.</summary>
        bool Delete(int id);
    }
}
