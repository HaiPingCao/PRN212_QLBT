using System.Windows;

namespace QLBT.Server;

public partial class App : Application
{
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
