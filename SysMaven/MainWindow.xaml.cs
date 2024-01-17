using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SysMaven
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GetAllSystemInfos();
        }

        #region Internal methods
        private void InfoMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo("https://sebastiensiddi.com")
            {
                UseShellExecute = true
            });
        }
        #endregion

        #region Public methods
        public void GetAllSystemInfos()
        {
            SystemInfos systemInfo = new SystemInfos();
            OSName.Content += systemInfo.GetOSInfos("os");
            OSArchitecture.Content += systemInfo.GetOSInfos("architecture");
            CPUName.Content += systemInfo.GetCPUInfos();
            GPUName.Content += systemInfo.GetGPUInfos();
        }
        #endregion
    }

    /// <summary>
    /// Get system informations
    /// </summary>
    public class SystemInfos
    {
        #region Public methods
        public string GetOSInfos(string param)
        {
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject managementObject in managementObjectSearcher.Get())
            {
                switch (param)
                {
                    case "os":
                        return managementObject["Caption"].ToString();
                    case "architecture":
                        return managementObject["OSArchitecture"].ToString();
                    case "version":
                        return managementObject["Version"].ToString();
                }
            }
            return "N/A";
        }

        public string GetCPUInfos()
        {            
            RegistryKey cpuName = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);
            if (cpuName != null)
            {
                return cpuName.GetValue("ProcessorNameString").ToString();
            }
            return "N/A";
        }

        public string GetGPUInfos()
        {
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                {
                    Console.WriteLine($"Name - {managementObject["Name"]}");
                    Console.WriteLine($"Status - {managementObject["Status"]}");
                    Console.WriteLine($"Caption - {managementObject["Caption"]}");
                    Console.WriteLine($"DeviceID - {managementObject["DeviceID"]}");
                    Console.WriteLine($"AdapterRAM - {managementObject["AdapterRAM"]}");
                    Console.WriteLine($"AdapterDACType - {managementObject["AdapterDACType"]}");
                    Console.WriteLine($"Monochrome - {managementObject["Monochrome"]}");
                    Console.WriteLine($"InstalledDisplayDrivers - {managementObject["InstalledDisplayDrivers"]}");
                    Console.WriteLine($"DriverVersion - {managementObject["DriverVersion"]}");
                    Console.WriteLine($"VideoProcessor - {managementObject["VideoProcessor"]}");
                    Console.WriteLine($"VideoArchitecture - {managementObject["VideoArchitecture"]}");
                    Console.WriteLine($"VideoMemoryType - {managementObject["VideoMemoryType"]}");

                    return managementObject["Name"].ToString();
                }
            }
            return "N/A";
        }
        #endregion
    }
}