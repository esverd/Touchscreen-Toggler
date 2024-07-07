using System.Windows;
using System.ComponentModel;
using System.Management;
using Touchscreen_Toggler.Properties;

namespace Touchscreen_Toggler
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadDevices();
            this.Closing += SettingsWindow_Closing;
        }

        private void SettingsWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the closing event
            this.Hide(); // Hide the window instead
        }

        private void LoadDevices()
        {
            DeviceList.Items.Clear();
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject device in searcher.Get())
            {
                DeviceList.Items.Add(device);
            }
            DeviceList.DisplayMemberPath = "Name"; // Display the device name
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                ManagementObject device = (ManagementObject)DeviceList.SelectedItem;
                Settings.Default.SelectedDevice = device["DeviceID"].ToString();
                Settings.Default.Save();
                MessageBox.Show("Device saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Hide();
            }
            else
            {
                MessageBox.Show("No device selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
