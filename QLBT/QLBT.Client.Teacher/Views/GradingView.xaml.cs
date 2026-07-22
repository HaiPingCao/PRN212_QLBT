using QLBT.Client.Teacher.Services;
using QLBT.Shared.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Client.Teacher.Views
{
    public partial class GradingView : UserControl
    {
        private readonly TeacherAssignmentClientService _assignmentService;
        private readonly GradingClientService _gradingService;

        private static readonly string DownloadFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QLBT_Downloads");

        private List<SubmissionDto> _all = new();

        public GradingView(TeacherAssignmentClientService assignmentService, GradingClientService gradingService)
        {
            InitializeComponent();
            _assignmentService = assignmentService;
            _gradingService = gradingService;

            Loaded += (_, _) => LoadAssignments();
        }

        private void LoadAssignments()
        {
            var list = _assignmentService.GetAll();
            cb_Assignment.ItemsSource = list;
            if (list.Count > 0) cb_Assignment.SelectedIndex = 0;
        }

        private void LoadSubmissions()
        {
            if (cb_Assignment.SelectedItem is not TeacherAssignmentDto assignment)
            {
                _all = new List<SubmissionDto>();
                dg_Submissions.ItemsSource = null;
                return;
            }

            _all = _gradingService.GetClassSubmissions(assignment.Id);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Submissions.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.Where(s =>
                    s.StudentId.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                    s.StudentName.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

        private void cb_Assignment_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadSubmissions();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e) => LoadSubmissions();

        private void dg_Submissions_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is not SubmissionDto submission) return;
            if (e.Column.Header?.ToString() != "Điểm") return;

            if (submission.Id <= 0)
            {
                MessageBox.Show("Sinh viên chưa nộp bài, không thể chấm điểm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Cancel = true;
                return;
            }

            if (e.EditingElement is not TextBox textBox) return;

            decimal? grade = null;
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (!decimal.TryParse(textBox.Text, out var value) || value < 0 || value > 10)
                {
                    MessageBox.Show("Điểm phải là số từ 0 đến 10.", "Dữ liệu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                    return;
                }
                grade = value;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_gradingService.SetGrade(submission.Id, grade))
                    MessageBox.Show("Không thể lưu điểm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
        }

        private void btn_OpenSubmission_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Submissions.SelectedItem is not SubmissionDto submission || submission.Id <= 0)
            {
                MessageBox.Show("Vui lòng chọn một bài nộp.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var path = _gradingService.DownloadSubmission(submission.Id, DownloadFolder);
            if (path == null)
            {
                MessageBox.Show("Không thể tải bài nộp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
