using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface IAssignmentService
    {
        /// <summary>
        /// Returns all assignments for classes the student is enrolled in.
        /// </summary>
        List<AssignmentDto> GetAssignmentList(string studentId);

        /// <summary>
        /// Returns assignment detail. Null if student has no access or assignment does not exist.
        /// </summary>
        AssignmentDto? GetAssignmentDetail(int assignmentId, string studentId);

        /// <summary>
        /// Returns the problem file path on disk. Null if no file or no access.
        /// </summary>
        string? GetProblemFilePath(int assignmentId, string studentId);
    }
}