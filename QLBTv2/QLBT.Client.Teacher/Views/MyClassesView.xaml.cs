using QLBT.Client.Teacher.Services;
using QLBT.Shared.Models;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Client.Teacher.Views
{
    public partial class MyClassesView : UserControl
    {
        private readonly ClassClientService _classService;

        private List<ClassDto> _allClasses = new();
        private List<ClassEnrollmentDto> _allStudents = new();
        private int? _selectedClassId;

        public MyClassesView(ClassClientService classService)
        {
            InitializeComponent();
            _classService = classService;

            Loaded += (_, _) => LoadClasses();
        }

        private void LoadClasses()
        {
            _allClasses = _classService.GetClassList();
            ApplyClassFilter();
        }

        private void ApplyClassFilter()
        {
            var keyword = tb_SearchClass.Text.Trim();
            IEnumerable<ClassDto> filtered = _allClasses;

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allClasses.Where(c =>
                    c.TenLop.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.TenHocKy.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.ChuyenNganh.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            dg_Classes.ItemsSource = filtered.ToList();
        }

        private void ApplyStudentFilter()
        {
            var keyword = tb_SearchStudent.Text.Trim();
            IEnumerable<ClassEnrollmentDto> filtered = _allStudents;

            if (!string.IsNullOrEmpty(keyword))
            {
                filtered = _allStudents.Where(s =>
                    s.Mssv.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    s.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            dg_Students.ItemsSource = filtered.ToList();
        }

        private void dg_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_Classes.SelectedItem is not ClassDto item)
            {
                _selectedClassId = null;
                _allStudents = new();
                tb_StudentsTitle.Text = "Chọn một lớp để xem danh sách sinh viên";
                dg_Students.ItemsSource = null;
                return;
            }

            _selectedClassId = item.Id;
            tb_StudentsTitle.Text = $"Sinh viên lớp {item.TenLop}";
            tb_SearchStudent.Text = "";
            _allStudents = _classService.GetClassStudents(item.Id);
            ApplyStudentFilter();
        }

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();

            if (_selectedClassId.HasValue)
            {
                _allStudents = _classService.GetClassStudents(_selectedClassId.Value);
                ApplyStudentFilter();
            }
        }

        private void tb_SearchClass_TextChanged(object sender, TextChangedEventArgs e) => ApplyClassFilter();

        private void tb_SearchStudent_TextChanged(object sender, TextChangedEventArgs e) => ApplyStudentFilter();
    }
}
