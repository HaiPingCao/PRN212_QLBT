using QLBT.Client.Teacher.Services;
using QLBT.Client.Teacher.Views;
using QLBT.Shared.Client;
using QLBT.Shared.Transport;
using System.Windows;

namespace QLBT.Client.Teacher
{
    public partial class MainWindow : Window
    {
        private readonly TcpClient _tcpClient;

        public MainWindow(TcpClient tcpClient, AuthClientService auth)
        {
            InitializeComponent();

            _tcpClient = tcpClient;
            Closed += (_, _) => _tcpClient.Dispose();

            var classService = new ClassClientService(tcpClient, auth);
            var assignmentService = new TeacherAssignmentClientService(tcpClient, auth);
            var gradingService = new GradingClientService(tcpClient, auth);

            Title = $"QLBT - Giáo viên ({auth.FullName})";

            tab_Class.Content = new MyClassesView(classService);
            tab_Assignment.Content = new AssignmentManagementView(classService, assignmentService);
            tab_Grading.Content = new GradingView(assignmentService, gradingService);
        }
    }
}
