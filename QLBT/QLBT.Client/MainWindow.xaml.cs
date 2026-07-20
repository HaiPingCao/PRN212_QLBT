using Microsoft.Win32;
using QLBT.Client.Services;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Client;

public partial class MainWindow : Window
{
    private readonly TcpClient _client;
    private readonly AuthClientService _auth;
    private readonly AssignmentClientService _assignmentService;
    private readonly SubmissionClientService _submissionService;

    private AssignmentDto? _selected;
    private SubmissionDto? _currentSubmission;
    private bool _clientDisposed;

    public MainWindow(TcpClient client, AuthClientService auth)
    {
        InitializeComponent();

        _client = client;
        _auth = auth;
        _assignmentService = new AssignmentClientService(_client, _auth);
        _submissionService = new SubmissionClientService(_client, _auth);

        tb_WelcomeName.Text = $"Xin chào, {_auth.FullName}";
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAssignmentsAsync();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        DisposeClient();
    }

    private void DisposeClient()
    {
        if (_clientDisposed) return;
        _clientDisposed = true;
        _client.Dispose();
    }

    // -------------------------------------------------------
    // Assignment list
    // -------------------------------------------------------

    private async Task LoadAssignmentsAsync()
    {
        SetBusy(true, "Đang tải danh sách bài tập...");
        try
        {
            var list = await Task.Run(() => _assignmentService.GetAssignmentList());
            dg_Assignments.ItemsSource = list;
            ShowNoSelection();
            tb_GlobalStatus.Text = list == null
                ? "Không thể tải danh sách bài tập."
                : $"Đã tải {list.Count} bài tập.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void btn_Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadAssignmentsAsync();
    }

    private async void dg_Assignments_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = dg_Assignments.SelectedItem as AssignmentDto;

        if (_selected == null)
        {
            ShowNoSelection();
            return;
        }

        ShowDetail(_selected);
        await RefreshSubmissionStatusAsync();
    }

    private void ShowNoSelection()
    {
        _selected = null;
        _currentSubmission = null;
        tb_NoSelection.Visibility = Visibility.Visible;
        sv_Detail.Visibility = Visibility.Collapsed;
    }

    private void ShowDetail(AssignmentDto assignment)
    {
        tb_NoSelection.Visibility = Visibility.Collapsed;
        sv_Detail.Visibility = Visibility.Visible;

        tb_DetailTitle.Text = assignment.Title;
        tb_DetailDescription.Text = string.IsNullOrWhiteSpace(assignment.Description)
            ? "(không có mô tả)"
            : assignment.Description;
        tb_DetailDue.Text = $"Hạn nộp: {assignment.DueDate:dd/MM/yyyy HH:mm}";

        btn_DownloadProblem.IsEnabled = !string.IsNullOrEmpty(assignment.ProblemFileName);
    }

    // -------------------------------------------------------
    // Submission status
    // -------------------------------------------------------

    private async Task RefreshSubmissionStatusAsync()
    {
        if (_selected == null) return;
        var assignmentId = _selected.Id;

        tb_SubmissionStatus.Text = "Đang kiểm tra...";
        btn_DownloadSubmission.IsEnabled = false;
        btn_DeleteSubmission.IsEnabled = false;

        var submission = await Task.Run(() => _submissionService.GetSubmissionByAssignment(assignmentId));

        // Selection may have changed while the request was in flight.
        if (_selected == null || _selected.Id != assignmentId) return;

        _currentSubmission = submission;

        if (submission == null)
        {
            tb_SubmissionStatus.Text = "Bạn chưa nộp bài này.";
            btn_DownloadSubmission.IsEnabled = false;
            btn_DeleteSubmission.IsEnabled = false;
        }
        else
        {
            tb_SubmissionStatus.Text =
                $"Đã nộp: {submission.FileName}\nSố lần nộp: {submission.SubmitCount}\nLúc: {submission.SubmittedAt:dd/MM/yyyy HH:mm}";
            btn_DownloadSubmission.IsEnabled = true;
            btn_DeleteSubmission.IsEnabled = true;
        }
    }

    // -------------------------------------------------------
    // Download problem file
    // -------------------------------------------------------

    private async void btn_DownloadProblem_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var assignmentId = _selected.Id;

        await DownloadWithDialogAsync(
            _selected.ProblemFileName ?? "de_bai",
            folder => _assignmentService.DownloadProblemFile(assignmentId, folder));
    }

    // -------------------------------------------------------
    // Submit assignment
    // -------------------------------------------------------

    private async void btn_Submit_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var assignmentId = _selected.Id;

        var dlg = new OpenFileDialog { Filter = "Tất cả file (*.*)|*.*" };
        if (dlg.ShowDialog() != true) return;

        var filePath = dlg.FileName;

        SetBusy(true, "Đang nộp bài...");
        try
        {
            var submissionId = await Task.Run(() => _submissionService.SubmitAssignment(assignmentId, filePath));

            if (submissionId < 0)
            {
                MessageBox.Show("Nộp bài thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            tb_GlobalStatus.Text = "Nộp bài thành công.";
            await RefreshSubmissionStatusAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    // -------------------------------------------------------
    // Download submission
    // -------------------------------------------------------

    private async void btn_DownloadSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSubmission == null) return;
        var submissionId = _currentSubmission.Id;

        await DownloadWithDialogAsync(
            _currentSubmission.FileName,
            folder => _submissionService.DownloadSubmission(submissionId, folder));
    }

    // -------------------------------------------------------
    // Delete submission
    // -------------------------------------------------------

    private async void btn_DeleteSubmission_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSubmission == null) return;

        var confirm = MessageBox.Show(
            $"Xóa bài nộp '{_currentSubmission.FileName}'?",
            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        var submissionId = _currentSubmission.Id;

        SetBusy(true, "Đang xóa bài nộp...");
        try
        {
            var ok = await Task.Run(() => _submissionService.DeleteSubmission(submissionId));
            if (!ok)
            {
                MessageBox.Show("Không thể xóa bài nộp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            tb_GlobalStatus.Text = "Đã xóa bài nộp.";
            await RefreshSubmissionStatusAsync();
        }
        finally
        {
            SetBusy(false);
        }
    }

    // -------------------------------------------------------
    // Shared: download-to-file helper
    // -------------------------------------------------------

    private async Task DownloadWithDialogAsync(string suggestedFileName, Func<string, string?> download)
    {
        var dlg = new SaveFileDialog
        {
            FileName = suggestedFileName,
            Filter = "Tất cả file (*.*)|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        var targetPath = dlg.FileName;
        var folder = Path.GetDirectoryName(targetPath)!;

        SetBusy(true, "Đang tải file...");
        try
        {
            var savedPath = await Task.Run(() => download(folder));
            if (savedPath == null)
            {
                MessageBox.Show("Không thể tải file.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // FileReceiver saves under the server's original file name; rename to match
            // the name the user picked in the dialog if they differ.
            if (!string.Equals(savedPath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(targetPath)) File.Delete(targetPath);
                File.Move(savedPath, targetPath);
                savedPath = targetPath;
            }

            tb_GlobalStatus.Text = $"Đã lưu: {savedPath}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    // -------------------------------------------------------
    // Logout
    // -------------------------------------------------------

    private void btn_Logout_Click(object sender, RoutedEventArgs e)
    {
        _auth.Logout();
        DisposeClient();

        var login = new LoginWindow();
        Application.Current.MainWindow = login;
        login.Show();
        Close();
    }

    // -------------------------------------------------------
    // Busy state
    // -------------------------------------------------------

    private void SetBusy(bool busy, string? status = null)
    {
        btn_Refresh.IsEnabled = !busy;
        dg_Assignments.IsEnabled = !busy;
        btn_DownloadProblem.IsEnabled = !busy && !string.IsNullOrEmpty(_selected?.ProblemFileName);
        btn_Submit.IsEnabled = !busy;
        btn_DownloadSubmission.IsEnabled = !busy && _currentSubmission != null;
        btn_DeleteSubmission.IsEnabled = !busy && _currentSubmission != null;

        if (status != null)
            tb_GlobalStatus.Text = status;
    }
}
