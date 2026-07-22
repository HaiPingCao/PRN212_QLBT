using QLBT.Shared;
using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Windows;

namespace QLBT.Client.Student
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            tb_Error.Text = "";

            if (!NetworkValidation.IsValidIp(tb_Ip.Text))
            {
                tb_Error.Text = "Địa chỉ IP không hợp lệ.";
                return;
            }

            if (!NetworkValidation.IsValidPort(tb_Port.Text, out var port))
            {
                tb_Error.Text = "Cổng không hợp lệ (1-65535).";
                return;
            }

            if (string.IsNullOrWhiteSpace(tb_Mssv.Text) || string.IsNullOrWhiteSpace(tb_Password.Password))
            {
                tb_Error.Text = "Vui lòng nhập MSSV và mật khẩu.";
                return;
            }

            var tcpClient = new TcpClient(tb_Ip.Text.Trim(), port);
            try
            {
                tcpClient.Connect();
            }
            catch (Exception ex)
            {
                tb_Error.Text = $"Không thể kết nối tới máy chủ: {ex.Message}";
                return;
            }

            if (!tcpClient.IsConnected)
            {
                tb_Error.Text = "Không thể kết nối tới máy chủ.";
                tcpClient.Dispose();
                return;
            }

            var auth = new AuthClientService(tcpClient);
            if (!auth.Login(tb_Mssv.Text.Trim(), tb_Password.Password, Role.Student))
            {
                tb_Error.Text = "Sai MSSV hoặc mật khẩu.";
                tcpClient.Dispose();
                return;
            }

            var main = new MainWindow(tcpClient, auth);
            main.Show();
            Close();
        }
    }
}
