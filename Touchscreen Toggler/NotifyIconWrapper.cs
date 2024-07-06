using System;
using System.Drawing;
using System.Management;
using System.Windows;
using System.Windows.Forms;
using Touchscreen_Toggler;

public class NotifyIconWrapper : IDisposable
{
    private readonly NotifyIcon _notifyIcon;

    public NotifyIconWrapper()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Touchscreen Toggler",
            ContextMenuStrip = new ContextMenuStrip()
        };

        UpdateContextMenu();
        ShowStartupNotification();
    }

    private void UpdateContextMenu()
    {
        _notifyIcon.ContextMenuStrip.Items.Clear();
        if (IsTouchscreenEnabled())
        {
            _notifyIcon.ContextMenuStrip.Items.Add("Disable Touchscreen", null, ToggleTouchscreen);
        }
        else
        {
            _notifyIcon.ContextMenuStrip.Items.Add("Enable Touchscreen", null, ToggleTouchscreen);
        }
        _notifyIcon.ContextMenuStrip.Items.Add("Settings", null, OpenSettings);
        _notifyIcon.ContextMenuStrip.Items.Add("Quit", null, Exit); // Changed "Exit" to "Quit"
    }

    private bool IsTouchscreenEnabled()
    {
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId)) return false;

        var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE DeviceID = '{deviceId.Replace("\\", "\\\\")}'");
        foreach (ManagementObject device in searcher.Get())
        {
            return device["Status"].ToString() == "OK";
        }
        return false;
    }

    private void ToggleTouchscreen(object? sender, EventArgs e)
    {
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId))
        {
            System.Windows.MessageBox.Show("Touchscreen device not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        bool enable = !IsTouchscreenEnabled();
        try
        {
            SetDeviceState(deviceId, enable);
            UpdateContextMenu();
        }
        catch (ManagementException ex)
        {
            System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetDeviceState(string deviceId, bool enable)
    {
        string query = $"SELECT * FROM Win32_PnPEntity WHERE DeviceID = '{deviceId.Replace("\\", "\\\\")}'";
        var searcher = new ManagementObjectSearcher(query);
        foreach (ManagementObject device in searcher.Get())
        {
            device.InvokeMethod(enable ? "Enable" : "Disable", new object[] { false });
        }
    }

    private void OpenSettings(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        });
    }

    private void Exit(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private string? GetTouchscreenDeviceId()
    {
        string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
        var searcher = new ManagementObjectSearcher(query);
        foreach (ManagementObject device in searcher.Get())
        {
            return device["DeviceID"]?.ToString();
        }
        return null;
    }

    private void ShowStartupNotification()
    {
        _notifyIcon.BalloonTipTitle = "Touchscreen Toggler";
        _notifyIcon.BalloonTipText = "The application has started successfully.";
        _notifyIcon.ShowBalloonTip(3000); // Show notification for 3 seconds
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}
