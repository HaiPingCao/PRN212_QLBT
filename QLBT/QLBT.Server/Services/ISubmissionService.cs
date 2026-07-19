using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface ISubmissionService
    {
        // Upload
        /// <summary>
        /// Initializes an upload session. Returns uploadId.
        /// </summary>
        string BeginUpload(int assignmentId, string studentId, string fileName, long fileSize);

        void AddChunk(string uploadId, int chunkIndex, string base64Data);

        /// <summary>
        /// Assembles chunks, saves file, writes to DB. Returns submissionId.
        /// </summary>
        int FinalizeUpload(string uploadId);

        // Download
        /// <summary>
        /// Returns problem file path for the server to stream chunks to the student.
        /// </summary>
        string? GetProblemFilePath(int assignmentId, string studentId);

        /// <summary>
        /// Returns submission file path. Validates student ownership.
        /// </summary>
        string? GetSubmissionFilePath(int submissionId, string studentId);

        // Management
        SubmissionDto? GetSubmission(int submissionId, string studentId);

        /// <summary>
        /// Deletes submission record and file on disk.
        /// </summary>
        bool DeleteSubmission(int submissionId, string studentId);
    }
}