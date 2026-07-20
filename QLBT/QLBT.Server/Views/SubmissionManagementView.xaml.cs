using QLBT.Server.Models;
using QLBT.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    /// <summary>
    /// Interaction logic for SubmissionManagementView.xaml
    /// </summary>
    public partial class SubmissionManagementView : UserControl
    {
        private const int AllOptionId = -1;

        private ITeacherSubmissionService Service => ((App)Application.Current).Host.TeacherSubmissionService;
        private ITeacherAssignmentService AssignmentService => ((App)Application.Current).Host.TeacherAssignmentService;

        public SubmissionManagementView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadLopOptions();
                LoadBaiTapOptions(null);
                LoadSubmissions(null, null);
            };
        }

        // Load dữ liệu

        private void LoadLopOptions()
        {
            var lops = new List<LopOption> { new() { Id = AllOptionId, TenLop = "-- Tất cả --" } };
            lops.AddRange(AssignmentService.GetAllLops());
            cb_Lop.ItemsSource = lops;
            cb_Lop.SelectedIndex = 0;
        }

        private void LoadBaiTapOptions(int? lopId)
        {
            var baiTaps = Service.GetAllBaiTaps();
            if (lopId.HasValue)
                baiTaps = baiTaps.Where(bt => bt.LopId == lopId.Value).ToList();

            var options = new List<BaiTapOption> { new() { Id = AllOptionId, TieuDe = "-- Tất cả --" } };
            options.AddRange(baiTaps);
            cb_BaiTap.ItemsSource = options;
            cb_BaiTap.SelectedIndex = 0;
        }

        private void LoadSubmissions(int? lopId, int? baiTapId)
        {
            dg_Submissions.ItemsSource = Service.GetAll(lopId, baiTapId);
            btn_Delete.IsEnabled = false;
        }

        // -------------------------------------------------------
        // Events
        // -------------------------------------------------------

        private void cb_Lop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lopId = (cb_Lop.SelectedItem as LopOption)?.Id;
            LoadBaiTapOptions(lopId is null or AllOptionId ? null : lopId);
        }

        private void btn_Filter_Click(object sender, RoutedEventArgs e)
        {
            var lopId = (cb_Lop.SelectedItem as LopOption)?.Id;
            var baiTapId = (cb_BaiTap.SelectedItem as BaiTapOption)?.Id;

            LoadSubmissions(
                lopId is null or AllOptionId ? null : lopId,
                baiTapId is null or AllOptionId ? null : baiTapId);
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            btn_Filter_Click(sender, e);
        }

        private void dg_Submissions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_Delete.IsEnabled = dg_Submissions.SelectedItem is TeacherSubmissionItem;
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Submissions.SelectedItem is not TeacherSubmissionItem item) return;

            var confirm = MessageBox.Show(
                $"Xóa bài nộp của sinh viên {item.Mssv} ({item.HoTenSinhVien}) cho bài tập '{item.TieuDeBaiTap}'?\nHành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.Delete(item.Id);
                btn_Filter_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa bài nộp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
