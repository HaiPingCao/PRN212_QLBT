using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface ITeacherAssignmentService
    {
        List<TeacherAssignmentDto> GetAll(string teacherId, int classId);
        TeacherAssignmentDto? Create(string teacherId, CreateAssignmentRequest input);
        TeacherAssignmentDto? Update(string teacherId, UpdateAssignmentRequest input);
        bool Delete(string teacherId, int id);

        /// <summary>Duong dan file de, dung khi giao vien bam "Mo file"/tai de cua chinh bai tap minh tao.</summary>
        string? GetProblemFilePath(int assignmentId, string teacherId);
    }
}
