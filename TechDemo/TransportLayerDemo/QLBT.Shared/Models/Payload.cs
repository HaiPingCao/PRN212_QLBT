namespace QLBT.Shared.Models
{
    // Auth
    public class LoginRequest
    {
        public string StudentId { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string FullName { get; set; } = "";
    }

    // Assignment
    public class GetAssignmentListRequest
    {
        public int ClassId { get; set; }
    }

    public class GetAssignmentDetailRequest
    {
        public int AssignmentId { get; set; }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ProblemFileName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Download problem file
    public class DownloadProblemFileRequest
    {
        public int AssignmentId { get; set; }
    }

    // Submit assignment - upload
    public class SubmitAssignmentRequest
    {
        public int AssignmentId { get; set; }
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
    }

    public class SubmitReadyResponse
    {
        public string UploadId { get; set; } = "";
    }

    public class FileChunkData
    {
        public string UploadId { get; set; } = "";
        public int ChunkIndex { get; set; }
        public string Base64Data { get; set; } = "";
    }

    public class FileEndData
    {
        public string UploadId { get; set; } = "";
    }

    public class SubmitOkResponse
    {
        public int SubmissionId { get; set; }
    }

    // Submission management
    public class GetSubmissionRequest
    {
        public int SubmissionId { get; set; }
    }

    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StudentId { get; set; } = "";
        public string FileName { get; set; } = "";
        public int SubmitCount { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class DeleteSubmissionRequest
    {
        public int SubmissionId { get; set; }
    }

    // Download submission
    public class DownloadSubmissionRequest
    {
        public int SubmissionId { get; set; }
    }

    // Download chunks (Server → Student)
    public class FileChunkDownData
    {
        public int ChunkIndex { get; set; }
        public string Base64Data { get; set; } = "";
    }

    public class FileEndDownData
    {
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
    }
}