using QLBT.Server.Models;
using QLBT.Server.Services;
using QLBT.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    public partial class ClassManagementView : UserControl
    {
        private IAdminService Service => ((App)Application.Current).Host.AdminService;

        /// <summary>Id lop dang sua tren form ben phai. Null nghia la form dang o che do Them moi.</summary>
        private int? _editingId;

        /// <summary>Lop dang duoc chon de quan ly sinh vien. Null = chua chon lop nao.</summary>
        private int? _selectedClassId;

        private List<LopAdminItem> _allClasses = new();

        public ClassManagementView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadGiaoViens();
                LoadHocKy();
                LoadClasses();
                ResetFormToNew();
                ClearEnrollmentPanel();
            };
        }

        // Load du lieu

        private void LoadGiaoViens()
        {
            var list = Service.GetAllGiaoVien();
            cb_GiaoVien.ItemsSource = list;
            if (list.Count > 0) cb_GiaoVien.SelectedIndex = 0;
        }

        private void LoadHocKy()
        {
            var list = Service.GetAllHocKy();
            cb_HocKy.ItemsSource = list;
            if (list.Count > 0) cb_HocKy.SelectedIndex = 0;
        }

        private void LoadClasses()
        {
            _allClasses = Service.GetAllLop();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Classes.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _allClasses
                : _allClasses.Where(l =>
                    l.TenLop.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    l.TenGiaoVien.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    l.TenHocKy.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    l.ChuyenNganh.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        // Form: Them moi / Sua

        private void ResetFormToNew()
        {
            _editingId = null;

            tb_FormTitle.Text = "Thêm lớp mới";
            if (cb_GiaoVien.Items.Count > 0) cb_GiaoVien.SelectedIndex = 0;
            if (cb_HocKy.Items.Count > 0) cb_HocKy.SelectedIndex = 0;

            tb_TenLop.Text = "";
            tb_ChuyenNganh.Text = "";

            btn_Delete.Visibility = Visibility.Collapsed;
            dg_Classes.SelectedItem = null;
        }

        private void LoadFormForEdit(LopAdminItem item)
        {
            _editingId = item.Id;

            tb_FormTitle.Text = $"Sửa lớp #{item.Id}";
            cb_GiaoVien.SelectedItem = ((List<GiaoVienAdminItem>)cb_GiaoVien.ItemsSource).FirstOrDefault(g => g.Msgv == item.Msgv);
            cb_HocKy.SelectedItem = ((List<HocKyAdminItem>)cb_HocKy.ItemsSource).FirstOrDefault(h => h.Id == item.HocKyId);

            tb_TenLop.Text = item.TenLop;
            tb_ChuyenNganh.Text = item.ChuyenNganh;

            btn_Delete.Visibility = Visibility.Visible;
        }

        // Panel quan ly sinh vien trong lop

        private void ClearEnrollmentPanel()
        {
            _selectedClassId = null;

            tb_EnrollmentTitle.Text = "Chọn một lớp để quản lý sinh viên";
            dg_Enrollments.ItemsSource = null;
            dg_AvailableStudents.ItemsSource = null;
            btn_ReactivateStudent.IsEnabled = false;
            btn_RemoveStudent.IsEnabled = false;
            btn_AddSelected.IsEnabled = false;
        }

        private void LoadEnrollmentPanel(LopAdminItem lop)
        {
            _selectedClassId = lop.Id;
            tb_EnrollmentTitle.Text = $"Sinh viên lớp {lop.TenLop}";
            btn_AddSelected.IsEnabled = true;

            RefreshEnrollments();
            RefreshAvailableStudents();
        }

        private void RefreshEnrollments()
        {
            if (_selectedClassId == null) return;
            dg_Enrollments.ItemsSource = Service.GetClassStudents(_selectedClassId.Value);
            btn_ReactivateStudent.IsEnabled = false;
            btn_RemoveStudent.IsEnabled = false;
        }

        private void RefreshAvailableStudents()
        {
            if (_selectedClassId == null) return;
            dg_AvailableStudents.ItemsSource = Service.GetAvailableStudents(_selectedClassId.Value);
        }

        // -------------------------------------------------------
        // Events: danh sach lop / form Them-Sua
        // -------------------------------------------------------

        private void dg_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Classes.SelectedItem is LopAdminItem item)
            {
                LoadFormForEdit(item);
                LoadEnrollmentPanel(item);
            }
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
            ClearEnrollmentPanel();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadClasses();

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TenLop.Text) || string.IsNullOrWhiteSpace(tb_ChuyenNganh.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên lớp và chuyên ngành.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!InputValidation.IsWithinLength(tb_ChuyenNganh.Text, 100))
            {
                MessageBox.Show("Chuyên ngành tối đa 100 ký tự.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!InputValidation.IsValidClassCode(tb_TenLop.Text))
            {
                MessageBox.Show("Mã lớp phải gồm đúng 6 chữ số.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cb_GiaoVien.SelectedItem is not GiaoVienAdminItem giaoVien)
            {
                MessageBox.Show("Vui lòng chọn giáo viên chủ nhiệm.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cb_HocKy.SelectedItem is not HocKyAdminItem hocKy)
            {
                MessageBox.Show("Vui lòng chọn học kỳ.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingId == null)
                {
                    Service.AddLop(giaoVien.Msgv, tb_TenLop.Text.Trim(), hocKy.Id, tb_ChuyenNganh.Text.Trim());
                    MessageBox.Show("Đã thêm lớp mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Service.UpdateLop(_editingId.Value, giaoVien.Msgv, tb_TenLop.Text.Trim(), hocKy.Id, tb_ChuyenNganh.Text.Trim());
                    MessageBox.Show("Đã cập nhật lớp.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadClasses();
                ResetFormToNew();
                ClearEnrollmentPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu lớp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa lớp này? Toàn bộ bài tập, bài nộp và danh sách sinh viên tham gia của lớp " +
                "sẽ bị xóa theo. Hành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.DeleteLop(_editingId.Value);
                LoadClasses();
                ResetFormToNew();
                ClearEnrollmentPanel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa lớp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // -------------------------------------------------------
        // Events: quan ly sinh vien trong lop
        // -------------------------------------------------------

        private void dg_Enrollments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Enrollments.SelectedItem is ClassEnrollmentAdminItem item)
            {
                btn_RemoveStudent.IsEnabled = item.DangThamGia;
                btn_ReactivateStudent.IsEnabled = !item.DangThamGia;
            }
            else
            {
                btn_RemoveStudent.IsEnabled = false;
                btn_ReactivateStudent.IsEnabled = false;
            }
        }

        private void btn_RemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassId == null) return;
            if (dg_Enrollments.SelectedItem is not ClassEnrollmentAdminItem item) return;

            var confirm = MessageBox.Show(
                $"Cho sinh viên '{item.Mssv}' rời khỏi lớp này? Bài nộp cũ vẫn được giữ lại.",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.RemoveStudentFromClass(_selectedClassId.Value, item.Mssv);
                RefreshEnrollments();
                RefreshAvailableStudents();
                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_ReactivateStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassId == null) return;
            if (dg_Enrollments.SelectedItem is not ClassEnrollmentAdminItem item) return;

            try
            {
                Service.ReactivateStudentInClass(_selectedClassId.Value, item.Mssv);
                RefreshEnrollments();
                RefreshAvailableStudents();
                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_AddSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassId == null) return;
            if (dg_AvailableStudents.ItemsSource is not List<SelectableStudent> available) return;

            var selectedIds = available.Where(s => s.IsSelected).Select(s => s.Mssv).ToList();
            if (selectedIds.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một sinh viên (tick vào ô Chọn).", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = Service.AddStudentsToClass(_selectedClassId.Value, selectedIds);

            RefreshEnrollments();
            RefreshAvailableStudents();
            LoadClasses();

            MessageBox.Show($"Đã thêm {result.Added.Count} sinh viên vào lớp.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
