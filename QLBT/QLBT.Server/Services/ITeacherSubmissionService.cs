using QLBT.Server.Models;
using System.Collections.Generic;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quản lý bài nộp từ phía giáo viên (UI Server): xem trạng thái nộp bài của sinh viên
    /// trên mọi lớp/bài tập và xóa bài nộp khi cần.
    /// </summary>
    public interface ITeacherSubmissionService
    {
        /// <summary>
        /// Toàn bộ bài nộp, có thể lọc theo lớp và/hoặc bài tập. Null nghĩa là không lọc theo tiêu chí đó.
        /// </summary>
        List<TeacherSubmissionItem> GetAll(int? lopId, int? baiTapId);

        /// <summary>Toàn bộ bài tập trong hệ thống, dùng cho ComboBox lọc.</summary>
        List<BaiTapOption> GetAllBaiTaps();

        /// <summary>Xóa bài nộp (kèm file trên đĩa). Trả về false nếu không tìm thấy.</summary>
        bool Delete(int id);
    }
}
