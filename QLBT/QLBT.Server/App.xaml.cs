using System.Configuration;
using System.Data;
using System.Windows;
using QLBT.Server.Services;


namespace QLBT.Server;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Composition root dùng chung cho toàn bộ UI (ServerManagement, AssignmentView...).
    /// Sống xuyên suốt vòng đời ứng dụng, không tạo lại theo từng View.
    /// </summary>
    public ServerHost Host { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Host = new ServerHost();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Host.Stop();
        base.OnExit(e);
    }
}

