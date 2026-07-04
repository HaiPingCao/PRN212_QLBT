using Microsoft.Win32;
using QLBT.Server.Models;
using QLBT.Server.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    /// <summary>
    /// Interaction logic for AssignmentView.xaml
    /// </summary>
    public partial class AssignmentView : UserControl
    {
        private ITeacherAssignmentService Service => ((App)Application.Current).Host.TeacherAssignmentService;

        /// <summary>Id bài tập đang sửa. Null nghĩa là form đang ở chế độ Thêm mới.</summary>
        private int? _editingId = null;

        /// <summary>Đường dẫn file đề mới được chọn từ máy giáo viên (chưa lưu vào DB).</summary>
        private string? _chosenFilePath = null;

        public AssignmentView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                LoadLops();
                LoadAssignments();
                ResetFormToNew();
            };
        }

        // Load dữ liệu

        private void LoadLops()
        {
            var lops = Service.GetAllLops();
            cb_Lop.ItemsSource = lops;
            if (lops.Count > 0)
                cb_Lop.SelectedIndex = 0;
        }

        private void LoadAssignments()
        {
            dg_Assignments.ItemsSource = Service.GetAll();
        }

        // Form: Thêm mới / Sửa

        private void ResetFormToNew()
        {
            _editingId = null;
            _chosenFilePath = null;

            tb_FormTitle.Text = "Thêm bài tập mới";
            cb_Lop.IsEnabled = true;
            if (cb_Lop.Items.Count > 0)
                cb_Lop.SelectedIndex = 0;

            tb_TieuDe.Text = "";
            tb_MoTa.Text = "";
            dp_HanNop.SelectedDate = DateTime.Today;
            tb_HanNopGio.Text = "23:59";

            tb_SelectedFileName.Text = "";
            tb_CurrentFileName.Text = "";

            btn_Delete.Visibility = Visibility.Collapsed;

            dg_Assignments.SelectedItem = null;
        }

        private void LoadFormForEdit(TeacherAssignmentItem item)
        {
            _editingId = item.Id;
            _chosenFilePath = null;

            tb_FormTitle.Text = $"Sửa bài tập #{item.Id}";

            // Không cho phép đổi lớp khi sửa: ComboBox chỉ hiển thị lớp hiện tại và bị khóa.
            cb_Lop.SelectedItem = ((System.Collections.Generic.List<LopOption>)cb_Lop.ItemsSource)
                .FirstOrDefault(l => l.Id == item.LopId);
            cb_Lop.IsEnabled = false;

            tb_TieuDe.Text = item.TieuDe;
            tb_MoTa.Text = item.MoTa ?? "";
            dp_HanNop.SelectedDate = item.HanNop.Date;
            tb_HanNopGio.Text = item.HanNop.ToString("HH:mm");

            tb_SelectedFileName.Text = "";
            tb_CurrentFileName.Text = string.IsNullOrEmpty(item.TenFileDe)
                ? "(Chưa có file đề)"
                : $"File hiện tại: {item.TenFileDe}";

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

        // -------------------------------------------------------
        // Events
        // -------------------------------------------------------

        private void dg_Assignments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Assignments.SelectedItem is TeacherAssignmentItem item)
                LoadFormForEdit(item);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
        }

        private void btn_ChooseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn file đề bài",
                Filter = "Tất cả file (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _chosenFilePath = dialog.FileName;
                tb_SelectedFileName.Text = $"Đã chọn: {System.IO.Path.GetFileName(dialog.FileName)}";
            }
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_TieuDe.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryBuildHanNop(out var hanNop)) return;

            try
            {
                if (_editingId == null)
                {
                    // Thêm mới: bắt buộc chọn Lớp.
                    if (cb_Lop.SelectedItem is not LopOption lop)
                    {
                        MessageBox.Show("Vui lòng chọn lớp.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Service.Add(new CreateAssignmentInput
                    {
                        LopId = lop.Id,
                        TieuDe = tb_TieuDe.Text.Trim(),
                        MoTa = string.IsNullOrWhiteSpace(tb_MoTa.Text) ? null : tb_MoTa.Text.Trim(),
                        HanNop = hanNop,
                        SourceFilePath = _chosenFilePath
                    });

                    MessageBox.Show("Đã thêm bài tập mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Sửa: không gửi LopId, Service giữ nguyên lớp hiện tại.
                    Service.Update(new UpdateAssignmentInput
                    {
                        Id = _editingId.Value,
                        TieuDe = tb_TieuDe.Text.Trim(),
                        MoTa = string.IsNullOrWhiteSpace(tb_MoTa.Text) ? null : tb_MoTa.Text.Trim(),
                        HanNop = hanNop,
                        SourceFilePath = _chosenFilePath
                    });

                    MessageBox.Show("Đã cập nhật bài tập.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadAssignments();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể lưu bài tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetFormToNew();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingId == null) return;

            var confirm = MessageBox.Show(
                "Bạn có chắc muốn xóa bài tập này? Hành động này không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Service.Delete(_editingId.Value);
                LoadAssignments();
                ResetFormToNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa bài tập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
