using Microsoft.Extensions.Configuration;
using QLBT.Client.Services;
using QLBT.Shared.Transport;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace QLBT.Client;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        LoadDefaultServerSettings();
    }

    private void LoadDefaultServerSettings()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        tb_ServerIp.Text = config["ServerSettings:IpAddress"] ?? "127.0.0.1";
        tb_ServerPort.Text = config["ServerSettings:Port"] ?? "9000";
    }

    private void pb_Password_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            btn_Login_Click(sender, e);
    }

    private async void btn_Login_Click(object sender, RoutedEventArgs e)
    {
        var ip = tb_ServerIp.Text.Trim();
        var studentId = tb_StudentId.Text.Trim();
        var password = pb_Password.Password;

        if (string.IsNullOrWhiteSpace(ip) || !int.TryParse(tb_ServerPort.Text.Trim(), out var port))
        {
            tb_Status.Text = "Địa chỉ server hoặc cổng không hợp lệ.";
            return;
        }

        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(password))
        {
            tb_Status.Text = "Vui lòng nhập mã số sinh viên và mật khẩu.";
            return;
        }

        btn_Login.IsEnabled = false;
        tb_Status.Foreground = System.Windows.Media.Brushes.Gray;
        tb_Status.Text = "Đang kết nối...";

        var client = new TcpClient(ip, port);

        var connected = await Task.Run(() =>
        {
            client.Connect();
            return client.IsConnected;
        });

        if (!connected)
        {
            tb_Status.Foreground = System.Windows.Media.Brushes.Red;
            tb_Status.Text = "Không thể kết nối tới server.";
            btn_Login.IsEnabled = true;
            client.Dispose();
            return;
        }

        tb_Status.Text = "Đang đăng nhập...";

        var auth = new AuthClientService(client);
        var ok = await Task.Run(() => auth.Login(studentId, password));

        if (!ok)
        {
            tb_Status.Foreground = System.Windows.Media.Brushes.Red;
            tb_Status.Text = "Sai mã số sinh viên hoặc mật khẩu.";
            btn_Login.IsEnabled = true;
            client.Dispose();
            return;
        }

        var mainWindow = new MainWindow(client, auth);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }
}
