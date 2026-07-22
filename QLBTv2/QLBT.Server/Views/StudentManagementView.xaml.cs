using QLBT.Server.Services;
using QLBT.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    public partial class StudentManagementView : UserControl
    {
        private IStudentService Service => ((App)Application.Current).Host.StudentService;

        private string? _editingMssv;
        private List<StudentDto> _all = new();

        public StudentManagementView()
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
            _all = Service.GetAll();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Students.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.FindAll(s =>
                    s.Mssv.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    s.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

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

        private void LoadFormForEdit(StudentDto item)
        {
            _editingMssv = item.Mssv;

            tb_FormTitle.Text = $"Sửa sinh viên {item.Mssv}";

            tb_Mssv.Text = item.Mssv;
            tb_Mssv.IsEnabled = false;

            tb_HoTen.Text = item.HoTen;
            tb_Email.Text = item.Email;
            pb_MatKhau.Password = "";

            tb_MatKhauLabel.Text = "Mật khẩu mới:";
            tb_MatKhauHint.Visibility = Visibility.Visible;

            btn_Delete.Visibility = Visibility.Visible;
        }

        private void dg_Students_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Students.SelectedItem is StudentDto item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadStudents();

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

                    Service.Add(new CreateStudentRequest
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
                    Service.Update(new UpdateStudentRequest
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

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

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
