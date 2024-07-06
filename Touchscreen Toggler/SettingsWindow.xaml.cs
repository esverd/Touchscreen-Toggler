using System.Windows;
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
        }

        private void LoadDevices()
        {
            DeviceList.Items.Clear();
            string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%HID-compliant touch screen%'";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject device in searcher.Get())
            {
                DeviceList.Items.Add(device["Name"]);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save the selected device (for future use, if necessary)
            Settings.Default.SelectedDevice = DeviceList.SelectedItem?.ToString();
            Settings.Default.Save();
            MessageBox.Show("Device saved");
            this.Hide(); // Use Hide instead of Close to keep the application running
        }
    }
}
