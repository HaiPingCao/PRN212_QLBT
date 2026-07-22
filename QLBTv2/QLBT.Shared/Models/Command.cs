namespace QLBT.Shared.Models
{
    public enum Command
    {
        // Auth
        Login,
        LoginResponse,

        // Assignment (Student view)
        GetAssignmentList,
        GetAssignmentDetail,

        // Download problem file (Server → Student/Teacher)
        DownloadProblemFile,

        // Submit assignment - Upload (Student → Server)
        SubmitAssignment,
        SubmitReady,
        FileChunk,
        FileEnd,
        SubmitOk,

        // View / delete submission
        GetSubmission,
        DeleteSubmission,

        // Download submission (Server → Student/Teacher)
        DownloadSubmission,

        // Download chunks (Server → Client)
        FileChunkDown,
        FileEndDown,

        // Lop (Teacher view, chi xem - quan ly lop nam o man hinh admin cua QLBT.Server)
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
