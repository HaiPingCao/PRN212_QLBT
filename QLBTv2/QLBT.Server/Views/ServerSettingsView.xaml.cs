using QLBT.Shared;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QLBT.Server.Views
{
    public partial class ServerSettingsView : UserControl
    {
        private ServerHost Host => ((App)Application.Current).Host;

        public ServerSettingsView()
        {
            InitializeComponent();
            LoadSettingsIntoForm();
            RefreshStatus();
        }

        private void LoadSettingsIntoForm()
        {
            var settings = Host.LoadSettings();
            tb_IpAddress.Text = settings.IpAddress;
            tb_Port.Text = settings.Port.ToString();
            tb_FSPath.Text = settings.RootFolder;
        }

        private void RefreshStatus()
        {
            if (Host.IsRunning)
            {
                tb_Status.Text = "Trạng thái: Đang chạy";
                btn_Start.IsEnabled = false;
                btn_Stop.IsEnabled = true;
            }
            else
            {
                tb_Status.Text = "Trạng thái: Đã dừng";
                btn_Start.IsEnabled = true;
                btn_Stop.IsEnabled = false;
            }
        }

        private bool TryReadFormSettings(out ServerSettings settings)
        {
            settings = new ServerSettings();

            if (!NetworkValidation.IsValidIp(tb_IpAddress.Text))
            {
                MessageBox.Show("Địa chỉ IP không hợp lệ.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!NetworkValidation.IsValidPort(tb_Port.Text, out var port))
            {
                MessageBox.Show("Cổng không hợp lệ. Vui lòng nhập số từ 1 đến 65535.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tb_FSPath.Text))
            {
                MessageBox.Show("Vui lòng nhập đường dẫn lưu file.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            settings.IpAddress = tb_IpAddress.Text.Trim();
            settings.Port = port;
            settings.RootFolder = tb_FSPath.Text.Trim();
            return true;
        }

        private void btn_LoadDefault_Click(object sender, RoutedEventArgs e)
        {
            LoadSettingsIntoForm();
        }

        private void btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!TryReadFormSettings(out var settings)) return;

            Host.SaveSettings(settings);
            MessageBox.Show("Đã lưu cài đặt.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!TryReadFormSettings(out var settings)) return;

            try
            {
                Host.Start(settings);
                RefreshStatus();
                MessageBox.Show($"Server đã khởi động tại {settings.IpAddress}:{settings.Port}.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi động server: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                RefreshStatus();
            }
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            Host.Stop();
            RefreshStatus();
        }
    }
}
