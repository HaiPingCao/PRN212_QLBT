using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Truy van lop cho giao vien (chi xem, khong quan ly) - viec tao/sua/xoa lop
    /// va them/xoa sinh vien nay do man hinh admin cuc bo o QLBT.Server dam nhiem (xem IAdminService).
    /// </summary>
    public interface IClassService
    {
        List<ClassDto> GetClassesOfTeacher(string teacherId);
        List<ClassEnrollmentDto>? GetClassStudents(string teacherId, int classId);
    }
}
