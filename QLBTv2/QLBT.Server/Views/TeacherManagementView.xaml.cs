using QLBT.Server.Models;
using QLBT.Server.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    public partial class TeacherManagementView : UserControl
    {
        private IAdminService Service => ((App)Application.Current).Host.AdminService;

        private string? _editingMsgv;
        private List<GiaoVienAdminItem> _all = new();

        public TeacherManagementView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadTeachers();
                ResetFormToNew();
            };
        }

        private void LoadTeachers()
        {
            _all = Service.GetAllGiaoVien();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Teachers.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.Where(g =>
                    g.Msgv.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    g.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    g.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ResetFormToNew()
        {
            _editingMsgv = null;

            tb_FormTitle.Text = "Thêm giáo viên mới";
            tb_Msgv.IsEnabled = true;

            tb_Msgv.Text = "";
            tb_HoTen.Text = "";
            tb_Email.Text = "";
            pb_MatKhau.Password = "";

            tb_MatKhauLabel.Text = "Mật khẩu:";
            tb_MatKhauHint.Visibility = Visibility.Collapsed;

            btn_Delete.Visibility = Visibility.Collapsed;
            dg_Teachers.SelectedItem = null;
        }

        private void LoadFormForEdit(GiaoVienAdminItem item)
        {
            _editingMsgv = item.Msgv;

            tb_FormTitle.Text = $"Sửa giáo viên {item.Msgv}";

            tb_Msgv.Text = item.Msgv;
            tb_Msgv.IsEnabled = false;

            tb_HoTen.Text = item.HoTen;
            tb_Email.Text = item.Email;
            pb_MatKhau.Password = "";

            tb_MatKhauLabel.Text = "Mật khẩu mới:";
            tb_MatKhauHint.Visibility = Visibility.Visible;

            btn_Delete.Visibility = Visibility.Visible;
        }

        private void dg_Teachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Teachers.SelectedItem is GiaoVienAdminItem item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadTeachers();

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_Msgv.Text) ||
                string.IsNullOrWhiteSpace(tb_HoTen.Text) ||
                string.IsNullOrWhiteSpace(tb_Email.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ MSGV, họ tên và email.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingMsgv == null)
                {
                    if (string.IsNullOrWhiteSpace(pb_MatKhau.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu cho giáo viên mới.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Service.AddGiaoVien(tb_Msgv.Text.Trim(), tb_HoTen.Text.Trim(), tb_Email.Text.Trim(), pb_MatKhau.Password);
                    MessageBox.Show("Đã thêm giáo viên mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Service.UpdateGiaoVien(_editingMsgv, tb_HoTen.Text.Trim(), tb_Email.Text.Trim(),
                        string.IsNullOrWhiteSpace(pb_MatKhau.Password) ? null : pb_MatKhau.Password);
                    MessageBox.Show("Đã cập nhật giáo viên.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadTeachers();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu giáo viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingMsgv == null) return;

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa giáo viên '{_editingMsgv}'? Hành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.DeleteGiaoVien(_editingMsgv);
                LoadTeachers();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa giáo viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
