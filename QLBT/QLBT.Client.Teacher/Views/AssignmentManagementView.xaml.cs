using Microsoft.Win32;
using QLBT.Client.Teacher.Services;
using QLBT.Shared;
using QLBT.Shared.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Client.Teacher.Views
{
    public partial class AssignmentManagementView : UserControl
    {
        private readonly ClassClientService _classService;
        private readonly TeacherAssignmentClientService _assignmentService;

        private static readonly string DownloadFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QLBT_Downloads");

        private int? _editingId;
        private string? _chosenFilePath;
        private List<TeacherAssignmentDto> _all = new();

        public AssignmentManagementView(ClassClientService classService, TeacherAssignmentClientService assignmentService)
        {
            InitializeComponent();
            _classService = classService;
            _assignmentService = assignmentService;

            Loaded += (_, _) =>
            {
                LoadClasses();
                LoadAssignments();
                ResetFormToNew();
            };
        }

        private void LoadClasses()
        {
            var classes = _classService.GetClassList();
            cb_Lop.ItemsSource = classes;
            if (classes.Count > 0) cb_Lop.SelectedIndex = 0;
        }

        private void LoadAssignments()
        {
            _all = _assignmentService.GetAll();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Assignments.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.Where(a =>
                    a.Title.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                    a.TenLop.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void ResetFormToNew()
        {
            _editingId = null;
            _chosenFilePath = null;

            tb_FormTitle.Text = "Thêm bài tập mới";
            cb_Lop.IsEnabled = true;
            if (cb_Lop.Items.Count > 0) cb_Lop.SelectedIndex = 0;

            tb_TieuDe.Text = "";
            tb_MoTa.Text = "";
            dp_HanNop.SelectedDate = DateTime.Today;
            tb_HanNopGio.Text = "23:59";

            tb_SelectedFileName.Text = "";
            tb_CurrentFileName.Text = "";

            btn_Delete.Visibility = Visibility.Collapsed;
            dg_Assignments.SelectedItem = null;
        }

        private void LoadFormForEdit(TeacherAssignmentDto item)
        {
            _editingId = item.Id;
            _chosenFilePath = null;

            tb_FormTitle.Text = $"Sửa bài tập #{item.Id}";

            cb_Lop.SelectedItem = ((List<ClassDto>)cb_Lop.ItemsSource).FirstOrDefault(c => c.Id == item.ClassId);
            cb_Lop.IsEnabled = false;

            tb_TieuDe.Text = item.Title;
            tb_MoTa.Text = item.Description ?? "";
            dp_HanNop.SelectedDate = item.DueDate.Date;
            tb_HanNopGio.Text = item.DueDate.ToString("HH:mm");

            tb_SelectedFileName.Text = "";
            tb_CurrentFileName.Text = string.IsNullOrEmpty(item.ProblemFileName)
                ? "(Chưa có file đề)"
                : $"File hiện tại: {item.ProblemFileName}";

            btn_Delete.Visibility = Visibility.Visible;
        }

        private bool TryBuildHanNop(out DateTime hanNop)
        {
            hanNop = default;

            if (dp_HanNop.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn hạn nộp.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!TimeSpan.TryParse(tb_HanNopGio.Text, out var time))
            {
                MessageBox.Show("Giờ hạn nộp không hợp lệ. Định dạng HH:mm, ví dụ 23:59.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            hanNop = dp_HanNop.SelectedDate.Value.Date + time;
            return true;
        }

        private void dg_Assignments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Assignments.SelectedItem is TeacherAssignmentDto item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadAssignments();

        private void btn_ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Chọn file đề bài", Filter = "Tất cả file (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                _chosenFilePath = dialog.FileName;
                tb_SelectedFileName.Text = $"Đã chọn: {Path.GetFileName(dialog.FileName)}";
            }
        }

        private void btn_OpenProblem_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Assignments.SelectedItem is not TeacherAssignmentDto item)
            {
                MessageBox.Show("Vui lòng chọn một bài tập.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(item.ProblemFileName))
            {
                MessageBox.Show("Bài tập này chưa có file đề.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var path = _assignmentService.DownloadProblemFile(item.Id, DownloadFolder);
            if (path == null)
            {
                MessageBox.Show("Không thể tải file đề.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TieuDe.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!InputValidation.IsWithinLength(tb_TieuDe.Text, 255))
            {
                MessageBox.Show("Tiêu đề tối đa 255 ký tự.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryBuildHanNop(out var hanNop)) return;

            if (_editingId == null && hanNop <= DateTime.Now)
            {
                MessageBox.Show("Hạn nộp phải ở tương lai.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_editingId == null)
            {
                if (cb_Lop.SelectedItem is not ClassDto lop)
                {
                    MessageBox.Show("Vui lòng chọn lớp.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = _assignmentService.Create(lop.Id, tb_TieuDe.Text.Trim(),
                    string.IsNullOrWhiteSpace(tb_MoTa.Text) ? null : tb_MoTa.Text.Trim(), hanNop, _chosenFilePath);

                if (result == null)
                {
                    MessageBox.Show("Không thể thêm bài tập.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Đã thêm bài tập mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var result = _assignmentService.Update(_editingId.Value, tb_TieuDe.Text.Trim(),
                    string.IsNullOrWhiteSpace(tb_MoTa.Text) ? null : tb_MoTa.Text.Trim(), hanNop, _chosenFilePath);

                if (result == null)
                {
                    MessageBox.Show("Không thể cập nhật bài tập.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Đã cập nhật bài tập.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LoadAssignments();
            ResetFormToNew();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e) => ResetFormToNew();

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa bài tập này? Hành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            if (!_assignmentService.Delete(_editingId.Value))
            {
                MessageBox.Show("Không thể xóa bài tập.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadAssignments();
            ResetFormToNew();
        }
    }
}
