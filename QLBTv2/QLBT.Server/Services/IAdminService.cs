using QLBT.Server.Models;

namespace QLBT.Server.Services
{
    /// <summary>
    /// Quan ly Hoc ky / Giao vien / Lop tu man hinh admin cuc bo cua QLBT.Server.
    /// Khac ClassService (chi thao tac tren lop cua mot giao vien cu the qua TCP) -
    /// service nay khong rang buoc theo giao vien nao, dung cho nguoi van hanh he thong.
    /// </summary>
    public interface IAdminService
    {
        // Hoc ky
        List<HocKyAdminItem> GetAllHocKy();
        HocKyAdminItem AddHocKy(string tenHocKy, DateOnly? ngayBatDau, DateOnly? ngayKetThuc);
        HocKyAdminItem? UpdateHocKy(int id, string tenHocKy, DateOnly? ngayBatDau, DateOnly? ngayKetThuc);
        bool DeleteHocKy(int id);

        // Giao vien
        List<GiaoVienAdminItem> GetAllGiaoVien();
        GiaoVienAdminItem AddGiaoVien(string msgv, string hoTen, string email, string matKhau);
        GiaoVienAdminItem? UpdateGiaoVien(string msgv, string hoTen, string email, string? matKhauMoi);
        bool DeleteGiaoVien(string msgv);

        // Lop (toan quyen, khong gioi han theo giao vien)
        List<LopAdminItem> GetAllLop();
        LopAdminItem AddLop(string msgv, string tenLop, int hocKyId, string chuyenNganh);
        LopAdminItem? UpdateLop(int id, string msgv, string tenLop, int hocKyId, string chuyenNganh);
        bool DeleteLop(int id);

        // Sinh vien trong lop (toan quyen, khong gioi han theo giao vien)
        List<ClassEnrollmentAdminItem> GetClassStudents(int classId);
        List<SelectableStudent> GetAvailableStudents(int classId);
        AddStudentsToClassResult AddStudentsToClass(int classId, List<string> studentIds);
        bool RemoveStudentFromClass(int classId, string studentId);
        bool ReactivateStudentInClass(int classId, string studentId);
    }
}
