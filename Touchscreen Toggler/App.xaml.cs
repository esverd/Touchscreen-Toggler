using System.Windows;
using Touchscreen_Toggler.Properties;

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
            // Load the selected device from settings
            string selectedDevice = Properties.Settings.Default.SelectedDevice;
            if (!string.IsNullOrEmpty(selectedDevice))
            {
                // Use the saved device ID if available
                // Implement logic to use this device ID
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
