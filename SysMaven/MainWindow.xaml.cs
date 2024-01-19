using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

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

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.75);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        #region Internal methods
        private void InfoMsg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo("https://sebastiensiddi.com")
            {
                UseShellExecute = true
            });
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            cpuUsage.Content = RefreshCpuUsage();
            RefreshRamInfos();
            RefreshTempInfos();
        }

        private void RefreshTempInfos()
        {
            Double temperature = 0;
            string instanceName = "";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject obj in searcher.Get())
            {
                temperature = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                temperature = (temperature - 2732) / 10.0;
                instanceName = obj["InstanceName"].ToString();
            }
            temp.Content = $"{temperature} °C";  
            
            if (temperature > 80)
            {
                temp.Foreground = Brushes.Red;
            }
            else if (temperature > 60)
            {
                temp.Foreground = Brushes.Orange;
            }
            else
            {
                temp.Foreground = Brushes.Green;
            }
        }

        public void RefreshRamInfos()
        {
            ramTotal.Content = $"Total : {FormatSize(GetTotalMemory())}";
            ramUsed.Content = $"Used : {FormatSize(GetUsedMemory())}";
            ramFree.Content = $"Free : {FormatSize(GetAvailMemory())}";

            string[] maxValue = FormatSize(GetTotalMemory()).Split(' ');
            progressBar.Maximum = float.Parse(maxValue[0]);
            string[] memValue = FormatSize(GetUsedMemory()).Split(' ');
            progressBar.Value = float.Parse(memValue[0]);
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

        public string RefreshCpuUsage()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            dynamic firstVal = cpuCounter.NextValue();
            System.Threading.Thread.Sleep(100);
            dynamic val = cpuCounter.NextValue();  
            
            RotateTransform rotateTransform = new RotateTransform((val * 2.7f) - 90);
            needle.RenderTransform = rotateTransform;

            decimal roundVal = Convert.ToDecimal(val);
            roundVal = Math.Round(roundVal, 2);

            return roundVal + " %";
        }

        #region RAM Fonctions
        [DllImport("kernel32.dll")]
        [return: MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

        public struct MEMORY_INFO
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

       static string FormatSize(double size)
        {
            double d = (double)size;
            int i = 0;
            while((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }
            string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return (string.Format("{0} {1}", Math.Round(d, 2), unit[i]));
        }

        public static MEMORY_INFO GetMemoryInfo()
        {
            MEMORY_INFO mi = new MEMORY_INFO();
            mi.dwLength = (uint)Marshal.SizeOf(mi);
            GlobalMemoryStatusEx(ref mi);
            return mi;
        }

        public static ulong GetAvailMemory()
        {
              MEMORY_INFO mi = GetMemoryInfo();
            return mi.ullAvailPhys;
        }

        public static ulong GetUsedMemory()
        {
            MEMORY_INFO mi = GetMemoryInfo();
            return mi.ullTotalPhys - mi.ullAvailPhys;
        }

        public static ulong GetTotalMemory()
        {
            MEMORY_INFO mi = GetMemoryInfo();
            return mi.ullTotalPhys;
        }      
        #endregion

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