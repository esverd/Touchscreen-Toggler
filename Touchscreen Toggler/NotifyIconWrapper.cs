using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Management;

namespace Touchscreen_Toggler
{
    public class NotifyIconWrapper : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        public NotifyIconWrapper()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                ContextMenu = new ContextMenu(new[]
                {
                new MenuItem("Enable Touchscreen", EnableTouchscreen),
                new MenuItem("Disable Touchscreen", DisableTouchscreen),
                new MenuItem("Settings", OpenSettings),
                new MenuItem("Exit", Exit)
            })
            };
        }

        private void EnableTouchscreen(object sender, EventArgs e)
        {
            ToggleTouchscreen(true);
        }

        private void DisableTouchscreen(object sender, EventArgs e)
        {
            ToggleTouchscreen(false);
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            });
        }

        private void Exit(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleTouchscreen(bool enable)
        {
            string deviceId = GetTouchscreenDeviceId();
            if (string.IsNullOrEmpty(deviceId)) return;

            string command = enable ? "enable" : "disable";
            System.Diagnostics.Process.Start("devcon.exe", $"{command} {deviceId}");
        }

        private string GetTouchscreenDeviceId()
        {
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject device in searcher.Get())
            {
                return device["DeviceID"].ToString();
            }
            return null;
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }
    }
}
