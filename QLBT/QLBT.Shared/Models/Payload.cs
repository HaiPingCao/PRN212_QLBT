namespace QLBT.Shared.Models
{
    public enum Role
    {
        Student,
        Teacher
    }

    // Auth
    public class LoginRequest
    {
        public string UserId { get; set; } = "";
        public string Password { get; set; } = "";
        public Role Role { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string FullName { get; set; } = "";
        public Role Role { get; set; }
    }

    // Assignment (student-facing read)
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
        public decimal? Grade { get; set; }
        public bool HasSubmitted { get; set; }

        /// <summary>Trang thai hien thi tren bang bai tap cua sinh vien.</summary>
        public string TrangThai => HasSubmitted
            ? "Đã nộp"
            : (DateTime.Now > DueDate ? "Quá hạn - chưa nộp" : "Chưa nộp");
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
    /// <summary>Sinh vien tra bai nop cua chinh minh theo bai tap (uq_bai_nop dam bao moi SV chi co 1 bai nop/bai tap).</summary>
    public class GetSubmissionRequest
    {
        public int AssignmentId { get; set; }
    }

    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string FileName { get; set; } = "";
        public int SubmitCount { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? Grade { get; set; }
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

    // Download chunks (Server → Client)
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

    // Lop (Teacher view, chi xem - quan ly lop nam o man hinh admin cua QLBT.Server)
    public class ClassDto
    {
        public int Id { get; set; }
        public string TenLop { get; set; } = "";
        public int HocKyId { get; set; }
        public string TenHocKy { get; set; } = "";
        public string ChuyenNganh { get; set; } = "";
        public int SoSinhVien { get; set; }
    }

    public class GetClassStudentsRequest
    {
        public int ClassId { get; set; }
    }

    /// <summary>Mot dong sinh_vien_lop trong lop, kem thong tin sinh vien, dung cho panel quan ly sinh vien trong lop.</summary>
    public class ClassEnrollmentDto
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public bool DangThamGia { get; set; }
        public DateTime NgayThamGia { get; set; }
        public DateTime? NgayRoiLop { get; set; }
    }

    // Quan ly tai khoan sinh vien (Teacher CRUD, khac viec them SV da co san vao lop o tren)
    public class StudentDto
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";

        /// <summary>So lop ma sinh vien dang tham gia.</summary>
        public int SoLopDangHoc { get; set; }
    }

    public class CreateStudentRequest
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string MatKhau { get; set; } = "";
    }

    public class UpdateStudentRequest
    {
        public string Mssv { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";

        /// <summary>Null hoac rong = giu nguyen mat khau cu.</summary>
        public string? MatKhauMoi { get; set; }
    }

    // Bai tap (Teacher CRUD)
    public class GetTeacherAssignmentsRequest
    {
        /// <summary>0 = tat ca lop cua giao vien.</summary>
        public int ClassId { get; set; }
    }

    public class TeacherAssignmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string TenLop { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? ProblemFileName { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SubmissionCount { get; set; }
    }

    public class CreateAssignmentRequest
    {
        public int ClassId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }

        /// <summary>Ten file de, null neu khong dinh kem.</summary>
        public string? ProblemFileName { get; set; }

        /// <summary>Toan bo noi dung file de, ma hoa Base64. Du nho de gui trong mot message (de bai thuong khong lon).</summary>
        public string? ProblemFileBase64 { get; set; }
    }

    public class UpdateAssignmentRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }

        /// <summary>Null = giu nguyen file de cu.</summary>
        public string? ProblemFileName { get; set; }
        public string? ProblemFileBase64 { get; set; }
    }

    public class DeleteAssignmentRequest
    {
        public int Id { get; set; }
    }

    // Cham diem (Teacher view)
    public class GetClassSubmissionsRequest
    {
        public int AssignmentId { get; set; }
    }

    public class GradeSubmissionRequest
    {
        public int SubmissionId { get; set; }
        public decimal? Grade { get; set; }
    }
}
