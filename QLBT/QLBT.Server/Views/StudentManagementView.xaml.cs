using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QLBT.Server.Models;
using QLBT.Server.Services;

namespace QLBT.Server.Views
{
    public partial class StudentManagementView : UserControl
    {
        private IStudentService Service =>
            ((App)Application.Current).Host.StudentService;

        private bool _editing = false;

        public StudentManagementView()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                LoadStudents();
                LoadClasses();
                ClearForm();
            };
        }

        private void LoadStudents()
        {
            dg_Students.ItemsSource = Service.GetAll();
        }

        private void LoadClasses()
        {
            var lops = Service.GetAllClasses();

            cb_Lop.ItemsSource = lops;

            if (lops.Count > 0)
                cb_Lop.SelectedIndex = 0;
        }

        private void ClearForm()
        {
            _editing = false;

            tb_Title.Text = "Thêm sinh viên";

            tb_MSSV.IsEnabled = true;

            tb_MSSV.Text = "";

            tb_HoTen.Text = "";

            tb_Email.Text = "";

            tb_Password.Password = "";

            ck_DangHoc.IsChecked = true;

            if (cb_Lop.Items.Count > 0)
                cb_Lop.SelectedIndex = 0;

            dg_Students.SelectedItem = null;
        }

        private void dg_Students_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Students.SelectedItem is not StudentItem sv)
                return;

            _editing = true;

            tb_Title.Text = "Cập nhật sinh viên";

            tb_MSSV.Text = sv.MSSV;

            tb_MSSV.IsEnabled = false;

            tb_HoTen.Text = sv.HoTen;

            tb_Email.Text = sv.Email;

            tb_Password.Password = "";

            ck_DangHoc.IsChecked = sv.DangThamGia;

            cb_Lop.SelectedItem =
                ((System.Collections.Generic.List<LopOption>)cb_Lop.ItemsSource)
                .FirstOrDefault(x => x.Id == sv.LopId);
        }

        private void btn_New_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_MSSV.Text))
            {
                MessageBox.Show("Vui lòng nhập MSSV.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tb_HoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tb_Email.Text))
            {
                MessageBox.Show("Vui lòng nhập Email.");
                return;
            }

            if (cb_Lop.SelectedItem is not LopOption lop)
            {
                MessageBox.Show("Vui lòng chọn lớp.");
                return;
            }

            try
            {
                if (!_editing)
                {
                    if (string.IsNullOrWhiteSpace(tb_Password.Password))
                    {
                        MessageBox.Show("Vui lòng nhập mật khẩu.");
                        return;
                    }

                    Service.Add(new CreateStudentInput
                    {
                        MSSV = tb_MSSV.Text.Trim(),
                        HoTen = tb_HoTen.Text.Trim(),
                        Email = tb_Email.Text.Trim(),
                        Password = tb_Password.Password,
                        LopId = lop.Id
                    });

                    MessageBox.Show("Thêm sinh viên thành công.");
                }
                else
                {
                    Service.Update(new UpdateStudentInput
                    {
                        MSSV = tb_MSSV.Text.Trim(),
                        HoTen = tb_HoTen.Text.Trim(),
                        Email = tb_Email.Text.Trim(),
                        Password = string.IsNullOrWhiteSpace(tb_Password.Password)
                                    ? null
                                    : tb_Password.Password,
                        LopId = lop.Id,
                        DangThamGia = ck_DangHoc.IsChecked == true
                    });

                    MessageBox.Show("Cập nhật sinh viên thành công.");
                }

                LoadStudents();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!_editing)
                return;

            var result = MessageBox.Show(
                "Bạn có chắc muốn xóa sinh viên này?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                Service.Delete(tb_MSSV.Text);

                MessageBox.Show("Đã xóa sinh viên.");

                LoadStudents();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Lỗi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}