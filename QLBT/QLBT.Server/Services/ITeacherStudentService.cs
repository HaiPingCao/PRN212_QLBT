using QLBT.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quản lý sinh viên từ phía giáo viên (UI Server): CRUD trên bảng sinh_vien.
    /// Khác với đăng nhập/nộp bài của sinh viên vốn phục vụ qua TCP (IAssignmentService/ISubmissionService).
    /// </summary>
    public interface ITeacherStudentService
    {
        /// <summary>Toàn bộ sinh viên trong hệ thống, kèm số lớp đang tham gia.</summary>
        List<StudentItem> GetAll();

        /// <summary>Tạo sinh viên mới. Ném InvalidOperationException nếu MSSV hoặc email đã tồn tại.</summary>
        StudentItem Add(CreateStudentInput input);

        /// <summary>Cập nhật sinh viên đã có. Trả về null nếu không tìm thấy.</summary>
        StudentItem? Update(UpdateStudentInput input);

        /// <summary>
        /// Xóa sinh viên. Trả về false nếu không tìm thấy.
        /// Ném InvalidOperationException nếu sinh viên còn dữ liệu tham gia lớp (sinh_vien_lop) —
        /// phải cho sinh viên rời hết lớp trước khi xóa.
        /// </summary>
        bool Delete(string mssv);
    }
}

