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

        // View / delete submission
        GetSubmission,
        DeleteSubmission,

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

        // Quan ly tai khoan sinh vien (Teacher: xem/them/sua - khong duoc xoa, xoa chi lam o man hinh admin cua Server)
        GetStudentList,
        CreateStudent,
        UpdateStudent,
    }
}
