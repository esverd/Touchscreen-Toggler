using System;
using System.Windows.Forms;
using System.Drawing;
using System.Management;
using Touchscreen_Toggler;
using System.Windows;

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
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);
    }

    private bool IsTouchscreenEnabled()
    {
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId)) return false;

        string query = $"SELECT * FROM Win32_PnPEntity WHERE DeviceID = \"{deviceId.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        foreach (ManagementObject device in searcher.Get())
        {
            return device["Status"].ToString() == "OK";
        }
        return false;
    }

    private void ToggleTouchscreen(object? sender, EventArgs e)
    {
        bool enable = !IsTouchscreenEnabled();
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId)) return;

        string command = enable ? "enable" : "disable";
        string devconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devcon.exe");

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = devconPath,
            Arguments = $"{command} \"{deviceId}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                process?.WaitForExit();
                string output = process?.StandardOutput.ReadToEnd();
                UpdateContextMenu();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
