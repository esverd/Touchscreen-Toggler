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
            string selectedDevice = Settings.Default.SelectedDevice;
            if (!string.IsNullOrEmpty(selectedDevice))
            {
                // Assign the selected device ID to the NotifyIconWrapper
                _notifyIcon.SelectedDeviceId = selectedDevice;
                _notifyIcon.UpdateContextMenu(); // Update the context menu based on the initial state
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
