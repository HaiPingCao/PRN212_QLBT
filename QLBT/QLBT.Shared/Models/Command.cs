namespace QLBT.Shared.Models
{
    public enum Command
    {
        // Auth
        Login,

        // Assignment (Student view)
        GetAssignmentList,
        GetAssignmentDetail,

        // Download problem file (Server → Student/Teacher)
        DownloadProblemFile,

        // Submit assignment - Upload (Student → Server)
        SubmitAssignment,
        FileChunk,
        FileEnd,

        // View submission
        GetSubmission,

        // Download submission (Server → Student/Teacher)
        DownloadSubmission,

        // Download chunks (Server → Client)
        FileChunkDown,
        FileEndDown,

        // Lop (Teacher)
        GetClassList,
        GetClassStudents,

        // Bai tap (Teacher CRUD)
        GetTeacherAssignments,
        CreateAssignment,
        UpdateAssignment,
        DeleteAssignment,

        // Cham diem (Teacher view)
        GetClassSubmissions,
        GradeSubmission,
    }
}
