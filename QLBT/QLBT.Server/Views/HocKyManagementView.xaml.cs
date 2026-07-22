using QLBT.Server.Models;
using QLBT.Server.Services;
using QLBT.Shared;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    public partial class HocKyManagementView : UserControl
    {
        private IAdminService Service => ((App)Application.Current).Host.AdminService;

        private int? _editingId;
        private List<HocKyAdminItem> _all = new();

        public HocKyManagementView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadHocKy();
                ResetFormToNew();
            };
        }

        private void LoadHocKy()
        {
            _all = Service.GetAllHocKy();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_HocKy.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.Where(h => h.TenHocKy.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ResetFormToNew()
        {
            _editingId = null;

            tb_FormTitle.Text = "Thêm học kỳ mới";
            tb_TenHocKy.Text = "";
            dp_BatDau.SelectedDate = null;
            dp_KetThuc.SelectedDate = null;

            btn_Delete.Visibility = Visibility.Collapsed;
            dg_HocKy.SelectedItem = null;
        }

        private void LoadFormForEdit(HocKyAdminItem item)
        {
            _editingId = item.Id;

            tb_FormTitle.Text = $"Sửa học kỳ #{item.Id}";
            tb_TenHocKy.Text = item.TenHocKy;
            dp_BatDau.SelectedDate = item.NgayBatDau?.ToDateTime(TimeOnly.MinValue);
            dp_KetThuc.SelectedDate = item.NgayKetThuc?.ToDateTime(TimeOnly.MinValue);

            btn_Delete.Visibility = Visibility.Visible;
        }

        private void dg_HocKy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_HocKy.SelectedItem is HocKyAdminItem item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadHocKy();

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TenHocKy.Text))
            {
                MessageBox.Show("Vui lòng nhập tên học kỳ.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!InputValidation.IsWithinLength(tb_TenHocKy.Text, InputValidation.MaxHocKyNameLength))
            {
                MessageBox.Show($"Tên học kỳ tối đa {InputValidation.MaxHocKyNameLength} ký tự.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var batDau = dp_BatDau.SelectedDate.HasValue ? DateOnly.FromDateTime(dp_BatDau.SelectedDate.Value) : (DateOnly?)null;
            var ketThuc = dp_KetThuc.SelectedDate.HasValue ? DateOnly.FromDateTime(dp_KetThuc.SelectedDate.Value) : (DateOnly?)null;

            if (batDau.HasValue != ketThuc.HasValue)
            {
                MessageBox.Show("Phải nhập đồng thời cả ngày bắt đầu và ngày kết thúc, hoặc để trống cả hai.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (batDau.HasValue && ketThuc.HasValue && batDau.Value > ketThuc.Value)
            {
                MessageBox.Show("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingId == null)
                {
                    Service.AddHocKy(tb_TenHocKy.Text.Trim(), batDau, ketThuc);
                    MessageBox.Show("Đã thêm học kỳ mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Service.UpdateHocKy(_editingId.Value, tb_TenHocKy.Text.Trim(), batDau, ketThuc);
                    MessageBox.Show("Đã cập nhật học kỳ.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadHocKy();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu học kỳ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa học kỳ này?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.DeleteHocKy(_editingId.Value);
                LoadHocKy();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa học kỳ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
