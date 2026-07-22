using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface IAssignmentService
    {
        /// <summary>Tra ve toan bo bai tap thuoc cac lop ma sinh vien dang tham gia.</summary>
        List<AssignmentDto> GetAssignmentList(string studentId);

        /// <summary>Tra ve chi tiet bai tap. Null neu sinh vien khong co quyen truy cap hoac bai tap khong ton tai.</summary>
        AssignmentDto? GetAssignmentDetail(int assignmentId, string studentId);

        /// <summary>Tra ve duong dan file de tren dia. Null neu khong co file hoac khong co quyen.</summary>
        string? GetProblemFilePath(int assignmentId, string studentId);
    }
}
