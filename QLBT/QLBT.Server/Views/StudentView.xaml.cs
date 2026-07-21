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

using QLBT.Server.Models;
using QLBT.Server.Services;

namespace QLBT.Server.Views
{
    /// <summary>
    /// Interaction logic for StudentView.xaml
    /// </summary>
    public partial class StudentView : UserControl
    {
        private ITeacherStudentService Service => ((App)Application.Current).Host.TeacherStudentService;

        /// <summary>MSSV đang sửa. Null nghĩa là form đang ở chế độ Thêm mới.</summary>
        private string? _editingMssv = null;

        public StudentView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadStudents();
                ResetFormToNew();
            };
        }

        private void LoadStudents()
        {
            dg_Students.ItemsSource = Service.GetAll();
        }

        // Form: Thêm mới / Sửa

        private void ResetFormToNew()
        {
            _editingMssv = null;

            tb_FormTitle.Text = "Thêm sinh viên mới";
            tb_Mssv.IsEnabled = true;

            tb_Mssv.Text = "";
            tb_HoTen.Text = "";
            tb_Email.Text = "";
            pb_MatKhau.Password = "";

            tb_MatKhauLabel.Text = "Mật khẩu:";
            tb_MatKhauHint.Visibility = Visibility.Collapsed;

            btn_Delete.Visibility = Visibility.Collapsed;

            dg_Students.SelectedItem = null;
        }

        private void LoadFormForEdit(StudentItem item)
        {
            _editingMssv = item.Mssv;

            tb_FormTitle.Text = $"Sửa sinh viên {item.Mssv}";

            // Không cho phép đổi MSSV khi sửa vì đây là khóa chính.
            tb_Mssv.Text = item.Mssv;
            tb_Mssv.IsEnabled = false;

            tb_HoTen.Text = item.HoTen;
            tb_Email.Text = item.Email;
            pb_MatKhau.Password = "";

            tb_MatKhauLabel.Text = "Mật khẩu mới:";
            tb_MatKhauHint.Visibility = Visibility.Visible;

            btn_Delete.Visibility = Visibility.Visible;
        }

        // -------------------------------------------------------
        // Events
        // -------------------------------------------------------

        private void dg_Students_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Students.SelectedItem is StudentItem item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_Mssv.Text) ||
                string.IsNullOrWhiteSpace(tb_HoTen.Text) ||
                string.IsNullOrWhiteSpace(tb_Email.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ MSSV, họ tên và email.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingMssv == null)
                {
                    if (string.IsNullOrWhiteSpace(pb_MatKhau.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu cho sinh viên mới.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Service.Add(new CreateStudentInput
                    {
                        Mssv = tb_Mssv.Text.Trim(),
                        HoTen = tb_HoTen.Text.Trim(),
                        Email = tb_Email.Text.Trim(),
                        MatKhau = pb_MatKhau.Password
                    });

                    MessageBox.Show("Đã thêm sinh viên mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Service.Update(new UpdateStudentInput
                    {
                        Mssv = _editingMssv,
                        HoTen = tb_HoTen.Text.Trim(),
                        Email = tb_Email.Text.Trim(),
                        MatKhauMoi = string.IsNullOrWhiteSpace(pb_MatKhau.Password) ? null : pb_MatKhau.Password
                    });

                    MessageBox.Show("Đã cập nhật sinh viên.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadStudents();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingMssv == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa sinh viên '{_editingMssv}'? Hành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.Delete(_editingMssv);
                LoadStudents();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
