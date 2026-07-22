using Microsoft.Win32;
using QLBT.Client.Student.Services;
using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace QLBT.Client.Student
{
    public partial class MainWindow : Window
    {
        private readonly TcpClient _tcpClient;
        private readonly AuthClientService _auth;
        private readonly AssignmentClientService _assignment;
        private readonly SubmissionClientService _submission;

        private static readonly string DownloadFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QLBT_Downloads");

        private AssignmentDto? _selected;
        private List<AssignmentDto> _all = new();

        public MainWindow(TcpClient tcpClient, AuthClientService auth)
        {
            InitializeComponent();

            _tcpClient = tcpClient;
            _auth = auth;
            _assignment = new AssignmentClientService(tcpClient, auth);
            _submission = new SubmissionClientService(tcpClient, auth);

            tb_Welcome.Text = $"Xin chào, {_auth.FullName}";
            Closed += (_, _) => _tcpClient.Dispose();

            LoadAssignments();
            ClearDetail();
        }

        private void LoadAssignments()
        {
            var list = _assignment.GetAssignmentList();
            if (list == null)
            {
                MessageBox.Show("Không thể tải danh sách bài tập từ máy chủ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                list = new List<AssignmentDto>();
            }

            _all = list;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = tb_Search.Text.Trim();
            dg_Assignments.ItemsSource = string.IsNullOrEmpty(keyword)
                ? _all
                : _all.Where(a => a.Title.Contains(keyword, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void tb_Search_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ApplyFilter();

        private void ClearDetail()
        {
            _selected = null;
            tb_Title.Text = "";
            tb_Description.Text = "";
            tb_DueDate.Text = "";
            tb_Grade.Text = "";
            btn_DownloadProblem.IsEnabled = false;
            btn_Submit.IsEnabled = false;
            btn_OpenSubmission.IsEnabled = false;
        }

        private void dg_Assignments_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dg_Assignments.SelectedItem is not AssignmentDto item)
            {
                ClearDetail();
                return;
            }

            _selected = item;
            tb_Title.Text = item.Title;
            tb_Description.Text = item.Description;
            tb_DueDate.Text = $"Hạn nộp: {item.DueDate:dd/MM/yyyy HH:mm} — {item.TrangThai}";
            tb_Grade.Text = item.Grade.HasValue ? $"Điểm: {item.Grade}" : "Chưa có điểm";

            var isPastDue = DateTime.Now > item.DueDate;
            var isGraded = item.Grade.HasValue;

            btn_DownloadProblem.IsEnabled = !string.IsNullOrEmpty(item.ProblemFileName);
            btn_Submit.IsEnabled = !isPastDue && !isGraded;
            btn_OpenSubmission.IsEnabled = item.HasSubmitted;
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
            ClearDetail();
        }

        private void btn_DownloadProblem_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            var path = _assignment.DownloadProblemFile(_selected.Id, DownloadFolder);
            if (path == null)
            {
                MessageBox.Show("Không thể tải file đề.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFile(path);
        }

        private void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            var dialog = new OpenFileDialog { Title = "Chọn file bài làm", Filter = "Tất cả file (*.*)|*.*" };
            if (dialog.ShowDialog() != true) return;

            var (ok, _, error) = _submission.SubmitAssignment(_selected.Id, dialog.FileName);
            if (!ok)
            {
                MessageBox.Show(string.IsNullOrEmpty(error) ? "Nộp bài thất bại." : error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Nộp bài thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadAssignments();
            ClearDetail();
        }

        private void btn_OpenSubmission_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            var mySubmission = _submission.GetSubmissionByAssignment(_selected.Id);
            if (mySubmission == null)
            {
                MessageBox.Show("Bạn chưa nộp bài này.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var path = _submission.DownloadSubmission(mySubmission.Id, DownloadFolder);
            if (path == null)
            {
                MessageBox.Show("Không thể tải bài đã nộp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFile(path);
        }

        private static void OpenFile(string path)
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
