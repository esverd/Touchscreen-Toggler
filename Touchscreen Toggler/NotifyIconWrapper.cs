using System;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Touchscreen_Toggler.Properties;
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
        _notifyIcon.ContextMenuStrip.Items.Add("Quit", null, Exit);
    }

    private bool IsTouchscreenEnabled()
    {
        string? deviceId = GetTouchscreenDeviceId();
        if (string.IsNullOrEmpty(deviceId)) return false;

        try
        {
            var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE DeviceID = '{deviceId.Replace("\\", "\\\\")}'");
            foreach (ManagementObject device in searcher.Get())
            {
                return device["Status"].ToString() == "OK";
            }
        }
        catch (ManagementException ex)
        {
            System.Windows.MessageBox.Show($"Error checking touchscreen status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetDeviceState(string deviceId, bool enable)
    {
        IntPtr deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, null, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);
        if (deviceInfoSet == IntPtr.Zero)
        {
            throw new Exception("Failed to get device information set.");
        }

        try
        {
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);

            for (int i = 0; SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                string currentDeviceId = GetDeviceId(deviceInfoSet, ref deviceInfoData);
                if (currentDeviceId == deviceId)
                {
                    SP_PROPCHANGE_PARAMS propChangeParams = new SP_PROPCHANGE_PARAMS();
                    propChangeParams.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(SP_CLASSINSTALL_HEADER));
                    propChangeParams.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE;
                    propChangeParams.StateChange = enable ? DICS_ENABLE : DICS_DISABLE;
                    propChangeParams.Scope = DICS_FLAG_GLOBAL;
                    propChangeParams.HwProfile = 0;

                    if (!SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref propChangeParams, Marshal.SizeOf(propChangeParams)))
                    {
                        throw new Exception("Failed to set class install parameters.");
                    }

                    if (!SetupDiChangeState(deviceInfoSet, ref deviceInfoData))
                    {
                        throw new Exception("Failed to change device state.");
                    }
                }
            }
        }
        finally
        {
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }
    }

    private string GetDeviceId(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData)
    {
        uint requiredSize = 0;
        SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, null, 0, ref requiredSize);
        if (requiredSize == 0) return null;

        char[] deviceId = new char[requiredSize];
        if (!SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, deviceId, requiredSize, ref requiredSize))
        {
            return null;
        }

        return new string(deviceId).TrimEnd('\0');
    }

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_PROPCHANGE_PARAMS ClassInstallParams, int ClassInstallParamsSize);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiChangeState(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, char[] DeviceInstanceId, uint DeviceInstanceIdSize, ref uint RequiredSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid ClassGuid;
        public int DevInst;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_CLASSINSTALL_HEADER
    {
        public int cbSize;
        public int InstallFunction;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_PROPCHANGE_PARAMS
    {
        public SP_CLASSINSTALL_HEADER ClassInstallHeader;
        public int StateChange;
        public int Scope;
        public int HwProfile;
    }

    private const uint DIGCF_PRESENT = 0x02;
    private const uint DIGCF_ALLCLASSES = 0x04;
    private const int DIF_PROPERTYCHANGE = 0x12;
    private const int DICS_ENABLE = 0x01;
    private const int DICS_DISABLE = 0x02;
    private const int DICS_FLAG_GLOBAL = 0x01;

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
        try
        {
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
            var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject device in searcher.Get())
            {
                return device["DeviceID"]?.ToString();
            }
        }
        catch (ManagementException ex)
        {
            System.Windows.MessageBox.Show($"Error retrieving touchscreen device ID: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return null;
    }

    private void ShowStartupNotification()
    {
        _notifyIcon.BalloonTipTitle = "Touchscreen Toggler";
        _notifyIcon.BalloonTipText = "The application has started successfully.";
        _notifyIcon.ShowBalloonTip(2000); // Show notification for 2 seconds
    }

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}
