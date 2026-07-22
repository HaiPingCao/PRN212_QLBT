using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quan ly tai khoan sinh vien tu phia giao vien: CRUD tren bang sinh_vien.
    /// Khac voi dang nhap/nop bai cua sinh vien (IAssignmentService/ISubmissionService).
    /// </summary>
    public interface IStudentService
    {
        /// <summary>Toan bo sinh vien trong he thong, kem so lop dang tham gia.</summary>
        List<StudentDto> GetAll();

        /// <summary>Tao sinh vien moi. Nem InvalidOperationException neu MSSV hoac email da ton tai.</summary>
        StudentDto Add(CreateStudentRequest input);

        /// <summary>Cap nhat sinh vien da co. Tra ve null neu khong tim thay.</summary>
        StudentDto? Update(UpdateStudentRequest input);

        /// <summary>
        /// Xoa sinh vien. Tra ve false neu khong tim thay.
        /// Nem InvalidOperationException neu sinh vien con du lieu tham gia lop —
        /// phai cho sinh vien roi het lop truoc khi xoa.
        /// </summary>
        bool Delete(string mssv);
    }
}
