using System;
using System.Management;

namespace SystemManager.Modules.SystemInfo
{
    public class HardwareMonitor
    {
        public SystemHardwareInfo GetHardwareInfo()
        {
            var info = new SystemHardwareInfo
            {
                CPUInfo = GetCPUInfo(),
                RAMInfo = GetRAMInfo(),
                GPUInfo = GetGPUInfo(),
                OSInfo = GetOSInfo(),
                Uptime = GetUptime(),
                LocalIP = GetLocalIP(),
                ExternalIP = GetExternalIP(),
                DNS = GetDNS()
            };
            return info;
        }

        private string GetCPUInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return $"{obj["Name"]} ({obj["NumberOfCores"]} cores, {obj["NumberOfLogicalProcessors"]} threads)";
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetRAMInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    long totalRAM = 0;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalRAM += Convert.ToInt64(obj["Capacity"]);
                    }
                    return $"{totalRAM / (1024 * 1024 * 1024)} GB";
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetGPUInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetOSInfo()
        {
            return Environment.OSVersion.ToString();
        }

        private TimeSpan GetUptime()
        {
            return TimeSpan.FromMilliseconds(Environment.TickCount);
        }

        private string GetLocalIP()
        {
            return Utilities.NetworkHelper.GetLocalIPAddress();
        }

        private string GetExternalIP()
        {
            return Utilities.NetworkHelper.GetExternalIPAddress();
        }

        private string GetDNS()
        {
            var dns = Utilities.NetworkHelper.GetDNSServers();
            return dns.Count > 0 ? string.Join(", ", dns) : "N/A";
        }

        public int GetCPUTemperature()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var temp = Convert.ToDouble(obj["CurrentTemperature"]);
                        return (int)((temp - 2732) / 10.0); // Convert from tenths of Kelvin to Celsius
                    }
                }
            }
            catch { }
            return -1;
        }
    }

    public class SystemHardwareInfo
    {
        public string CPUInfo { get; set; }
        public string RAMInfo { get; set; }
        public string GPUInfo { get; set; }
        public string OSInfo { get; set; }
        public TimeSpan Uptime { get; set; }
        public string LocalIP { get; set; }
        public string ExternalIP { get; set; }
        public string DNS { get; set; }
    }
}
