using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Management;
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
            ContextMenuStrip = new ContextMenuStrip()
        };
        _notifyIcon.ContextMenuStrip.Items.Add("Enable Touchscreen", null, EnableTouchscreen);
        _notifyIcon.ContextMenuStrip.Items.Add("Disable Touchscreen", null, DisableTouchscreen);
        _notifyIcon.ContextMenuStrip.Items.Add("Settings", null, OpenSettings);
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);
    }

    private void EnableTouchscreen(object? sender, EventArgs e)
    {
        ToggleTouchscreen(true);
    }

    private void DisableTouchscreen(object? sender, EventArgs e)
    {
        ToggleTouchscreen(false);
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

    private void ToggleTouchscreen(bool enable)
    {
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId)) return;

        string command = enable ? "enable" : "disable";
        System.Diagnostics.Process.Start("devcon.exe", $"{command} {deviceId}");
    }

    private string? GetTouchscreenDeviceId()
    {
        string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        foreach (ManagementObject device in searcher.Get())
        {
            return device["DeviceID"]?.ToString();
        }
        return null;
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}
