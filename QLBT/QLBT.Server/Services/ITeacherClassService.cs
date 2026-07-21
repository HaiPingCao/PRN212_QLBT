using QLBT.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quản lý lớp từ phía giáo viên (UI Server): CRUD trên bảng lop, và quản lý
    /// sinh viên tham gia lớp (bảng sinh_vien_lop).
    /// </summary>
    public interface ITeacherClassService
    {
        /// <summary>Toàn bộ lớp trong hệ thống, kèm tên giáo viên chủ nhiệm và số liệu tổng hợp.</summary>
        List<ClassItem> GetAll();

        /// <summary>Toàn bộ giáo viên, dùng cho ComboBox chọn giáo viên chủ nhiệm.</summary>
        List<GiaoVienOption> GetAllGiaoViens();

        /// <summary>Tạo lớp mới. Ném InvalidOperationException nếu trùng (TenLop, KiHoc, ChuyenNganh) hoặc MSGV không tồn tại.</summary>
        ClassItem Add(CreateClassInput input);

        /// <summary>Cập nhật lớp đã có (bao gồm cả đổi giáo viên chủ nhiệm). Trả về null nếu không tìm thấy.</summary>
        ClassItem? Update(UpdateClassInput input);

        /// <summary>
        /// Xóa lớp. Trả về false nếu không tìm thấy. Lưu ý: xóa lớp sẽ xóa theo (cascade)
        /// toàn bộ bài tập, sinh viên tham gia và bài nộp liên quan — cảnh báo trước khi gọi.
        /// </summary>
        bool Delete(int id);

        // ---- Quản lý sinh viên trong lớp (sinh_vien_lop) ----

        /// <summary>Danh sách sinh viên từng/đang tham gia lớp, kèm trạng thái.</summary>
        List<ClassEnrollmentItem> GetEnrollments(int lopId);

        /// <summary>Sinh viên chưa từng thuộc lớp này (chưa có dòng sinh_vien_lop tương ứng), dùng cho ComboBox thêm vào lớp.</summary>
        List<StudentOption> GetAvailableStudents(int lopId);

        /// <summary>Thêm sinh viên vào lớp (tạo dòng sinh_vien_lop mới, DangThamGia = true).</summary>
        void AddStudentToClass(int lopId, string mssv);

        /// <summary>Cho sinh viên rời lớp (đánh dấu DangThamGia = false, NgayRoiLop = hiện tại). Không xóa dữ liệu.</summary>
        void RemoveStudentFromClass(int lopId, string mssv);

        /// <summary>Cho sinh viên đã rời lớp tham gia lại (DangThamGia = true, NgayRoiLop = null).</summary>
        void ReactivateStudentInClass(int lopId, string mssv);
    }
}

