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
            // _notifyIcon initialization is moved to the constructor
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
