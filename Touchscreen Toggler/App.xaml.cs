using System.Windows;

namespace Touchscreen_Toggler
{
    public partial class App : Application
    {
        private NotifyIconWrapper _notifyIcon;

        public App()
        {
            _notifyIcon = new NotifyIconWrapper();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Application starts without a main window
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
