using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface ISubmissionService
    {
        // Upload
        string BeginUpload(int assignmentId, string studentId, string fileName, long fileSize);
        void AddChunk(string uploadId, int chunkIndex, string base64Data);
        int FinalizeUpload(string uploadId);

        // Download
        string? GetSubmissionFilePath(int submissionId, string requesterId, Role role);

        // Management (student)
        /// <summary>Tra bai nop cua sinh vien theo bai tap (moi SV chi co toi da 1 bai nop/bai tap).</summary>
        SubmissionDto? GetSubmissionByAssignment(int assignmentId, string studentId);

        // Management (teacher)
        List<SubmissionDto> GetClassSubmissions(int assignmentId, string teacherId);
        bool SetGrade(int submissionId, string teacherId, decimal? grade);
    }
}
