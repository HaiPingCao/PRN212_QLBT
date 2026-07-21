using QLBT.Server.Models;
using QLBT.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLBT.Server.Views
{
    /// <summary>
    /// Interaction logic for ClassView.xaml
    /// </summary>
    public partial class ClassView : UserControl
    {
        private ITeacherClassService Service => ((App)Application.Current).Host.TeacherClassService;

        /// <summary>Id lớp đang sửa trên form bên phải. Null nghĩa là form đang ở chế độ Thêm mới.</summary>
        private int? _editingId = null;

        /// <summary>Lớp đang được chọn để quản lý sinh viên (panel bên dưới danh sách lớp). Null = chưa chọn lớp nào.</summary>
        private int? _selectedClassIdForEnrollment = null;

        public ClassView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadGiaoViens();
                LoadClasses();
                ResetFormToNew();
                ClearEnrollmentPanel();
            };
        }

        // Load dữ liệu

        private void LoadGiaoViens()
        {
            var giaoViens = Service.GetAllGiaoViens();
            cb_GiaoVien.ItemsSource = giaoViens;
            if (giaoViens.Count > 0)
                cb_GiaoVien.SelectedIndex = 0;
        }

        private void LoadClasses()
        {
            dg_Classes.ItemsSource = Service.GetAll();
        }

        // Form: Thêm mới / Sửa lớp

        private void ResetFormToNew()
        {
            _editingId = null;

            tb_FormTitle.Text = "Thêm lớp mới";
            if (cb_GiaoVien.Items.Count > 0)
                cb_GiaoVien.SelectedIndex = 0;

            tb_TenLop.Text = "";
            tb_KiHoc.Text = "";
            tb_ChuyenNganh.Text = "";

            btn_Delete.Visibility = Visibility.Collapsed;

            dg_Classes.SelectedItem = null;
        }

        private void LoadFormForEdit(ClassItem item)
        {
            _editingId = item.Id;

            tb_FormTitle.Text = $"Sửa lớp #{item.Id}";

            cb_GiaoVien.SelectedItem = ((List<GiaoVienOption>)cb_GiaoVien.ItemsSource)
                .FirstOrDefault(gv => gv.Msgv == item.Msgv);

            tb_TenLop.Text = item.TenLop;
            tb_KiHoc.Text = item.KiHoc;
            tb_ChuyenNganh.Text = item.ChuyenNganh;

            btn_Delete.Visibility = Visibility.Visible;
        }

        // Panel quản lý sinh viên trong lớp

        private void ClearEnrollmentPanel()
        {
            _selectedClassIdForEnrollment = null;

            tb_EnrollmentTitle.Text = "Chọn một lớp để quản lý sinh viên";
            sp_AddStudent.IsEnabled = false;
            cb_AvailableStudents.ItemsSource = null;
            dg_Enrollments.ItemsSource = null;
            btn_ReactivateStudent.IsEnabled = false;
            btn_RemoveStudent.IsEnabled = false;
        }

        private void LoadEnrollmentPanel(ClassItem lop)
        {
            _selectedClassIdForEnrollment = lop.Id;

            tb_EnrollmentTitle.Text = $"Sinh viên lớp {lop.TenLop}";
            sp_AddStudent.IsEnabled = true;

            RefreshEnrollments();
            RefreshAvailableStudents();
        }

        private void RefreshEnrollments()
        {
            if (_selectedClassIdForEnrollment == null) return;
            dg_Enrollments.ItemsSource = Service.GetEnrollments(_selectedClassIdForEnrollment.Value);
            btn_ReactivateStudent.IsEnabled = false;
            btn_RemoveStudent.IsEnabled = false;
        }

        private void RefreshAvailableStudents()
        {
            if (_selectedClassIdForEnrollment == null) return;

            var available = Service.GetAvailableStudents(_selectedClassIdForEnrollment.Value);
            cb_AvailableStudents.ItemsSource = available;
            if (available.Count > 0)
                cb_AvailableStudents.SelectedIndex = 0;
        }

        // -------------------------------------------------------
        // Events: danh sách lớp / form Thêm-Sửa
        // -------------------------------------------------------

        private void dg_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Classes.SelectedItem is ClassItem item)
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

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TenLop.Text) ||
                string.IsNullOrWhiteSpace(tb_KiHoc.Text) ||
                string.IsNullOrWhiteSpace(tb_ChuyenNganh.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên lớp, kỳ học và chuyên ngành.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cb_GiaoVien.SelectedItem is not GiaoVienOption giaoVien)
            {
                MessageBox.Show("Vui lòng chọn giáo viên chủ nhiệm.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingId == null)
                {
                    Service.Add(new CreateClassInput
                    {
                        Msgv = giaoVien.Msgv,
                        TenLop = tb_TenLop.Text.Trim(),
                        KiHoc = tb_KiHoc.Text.Trim(),
                        ChuyenNganh = tb_ChuyenNganh.Text.Trim()
                    });

                    MessageBox.Show("Đã thêm lớp mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Service.Update(new UpdateClassInput
                    {
                        Id = _editingId.Value,
                        Msgv = giaoVien.Msgv,
                        TenLop = tb_TenLop.Text.Trim(),
                        KiHoc = tb_KiHoc.Text.Trim(),
                        ChuyenNganh = tb_ChuyenNganh.Text.Trim()
                    });

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

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
        }

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
                Service.Delete(_editingId.Value);
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
        // Events: quản lý sinh viên trong lớp
        // -------------------------------------------------------

        private void dg_Enrollments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Enrollments.SelectedItem is ClassEnrollmentItem item)
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

        private void btn_AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassIdForEnrollment == null) return;

            if (cb_AvailableStudents.SelectedItem is not StudentOption sv)
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần thêm.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Service.AddStudentToClass(_selectedClassIdForEnrollment.Value, sv.Mssv);
                RefreshEnrollments();
                RefreshAvailableStudents();
                LoadClasses(); // cập nhật lại số sinh viên hiển thị trên danh sách lớp
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể thêm sinh viên vào lớp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_RemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClassIdForEnrollment == null) return;
            if (dg_Enrollments.SelectedItem is not ClassEnrollmentItem item) return;

            var confirm = MessageBox.Show(
                $"Cho sinh viên '{item.Mssv}' rời khỏi lớp này? Bài nộp cũ vẫn được giữ lại.",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.RemoveStudentFromClass(_selectedClassIdForEnrollment.Value, item.Mssv);
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
            if (_selectedClassIdForEnrollment == null) return;
            if (dg_Enrollments.SelectedItem is not ClassEnrollmentItem item) return;

            try
            {
                Service.ReactivateStudentInClass(_selectedClassIdForEnrollment.Value, item.Mssv);
                RefreshEnrollments();
                RefreshAvailableStudents();
                LoadClasses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}