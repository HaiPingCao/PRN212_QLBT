using QLBT.Server.Models;
using QLBT.Server.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    /// <summary>
    /// Interaction logic for ClassManagementView.xaml
    /// </summary>
    public partial class ClassManagementView : UserControl
    {
        private IClassService Service
            => ((App)Application.Current).Host.ClassService;

        // null = Thêm mới
        private int? _editingId = null;

        public ClassManagementView()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                LoadClasses();
                ResetForm();
            };
        }

        // ============================
        // Load dữ liệu
        // ============================

        private void LoadClasses()
        {
            dg_Classes.ItemsSource = Service.GetAll();
        }

        // ============================
        // Form
        // ============================

        private void ResetForm()
        {
            _editingId = null;

            tb_FormTitle.Text = "Thêm lớp mới";

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

            tb_TenLop.Text = item.TenLop;
            tb_KiHoc.Text = item.KiHoc;
            tb_ChuyenNganh.Text = item.ChuyenNganh;

            btn_Delete.Visibility = Visibility.Visible;
        }

        // ============================
        // Events
        // ============================

        private void dg_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Classes.SelectedItem is ClassItem item)
            {
                LoadFormForEdit(item);
            }
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TenLop.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập tên lớp.",
                    "Thiếu dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(tb_KiHoc.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập kỳ học.",
                    "Thiếu dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(tb_ChuyenNganh.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập chuyên ngành.",
                    "Thiếu dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                if (_editingId == null)
                {
                    Service.Add(new CreateClassInput
                    {
                        TenLop = tb_TenLop.Text.Trim(),
                        KiHoc = tb_KiHoc.Text.Trim(),
                        ChuyenNganh = tb_ChuyenNganh.Text.Trim()
                    });

                    MessageBox.Show(
                        "Đã thêm lớp.",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    Service.Update(new UpdateClassInput
                    {
                        Id = _editingId.Value,
                        TenLop = tb_TenLop.Text.Trim(),
                        KiHoc = tb_KiHoc.Text.Trim(),
                        ChuyenNganh = tb_ChuyenNganh.Text.Trim()
                    });

                    MessageBox.Show(
                        "Đã cập nhật lớp.",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                LoadClasses();
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId == null)
                return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa lớp này?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                Service.Delete(_editingId.Value);

                MessageBox.Show(
                    "Đã xóa lớp.",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadClasses();
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không thể xóa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
