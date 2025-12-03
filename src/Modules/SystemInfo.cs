using System;
using System.Windows.Forms;
using System.Drawing;
using System.Management;
using System.Threading.Tasks;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage sysInfoPage;
        private Label lblSysInfo;

        private void InitTab_SystemInfo() {
            sysInfoPage = new TabPage("SISTEM");
            sysInfoPage.BackColor = Color.FromArgb(30,30,30);
            sysInfoPage.Name = "SYSTEM";

            lblSysInfo = CreateLabel("Sistem Bilgileri Toplaniyor...", 10, 10, 10, sysInfoPage);
            lblSysInfo.AutoSize = true;

            Button btnRefresh = CreateBtn("YENILE", 10, 400, 150, Color.Blue, sysInfoPage);
            btnRefresh.Click += (s,e) => UpdateSystemInfo();

            tabs.TabPages.Add(sysInfoPage);
            UpdateSystemInfo();
        }

        private void UpdateSystemInfo() {
            Task.Run(() => {
                string info = "";
                try {
                    info += "OS: " + Environment.OSVersion + "\n";
                    info += "Machine: " + Environment.MachineName + "\n";
                    info += "User: " + Environment.UserName + "\n";
                    info += "Processors: " + Environment.ProcessorCount + "\n";

                    // WMI Queries
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor")) {
                        foreach (ManagementObject obj in searcher.Get()) {
                            info += "CPU: " + obj["Name"] + "\n";
                        }
                    }

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_VideoController")) {
                        foreach (ManagementObject obj in searcher.Get()) {
                            info += "GPU: " + obj["Name"] + "\n";
                        }
                    }

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem")) {
                        foreach (ManagementObject obj in searcher.Get()) {
                            info += "Last Boot: " + ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString()) + "\n";
                        }
                    }

                    // Temp (Might fail if not supported)
                    try {
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature")) {
                            foreach (ManagementObject obj in searcher.Get()) {
                                double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                                temp = (temp - 2732) / 10.0;
                                info += "Temp: " + temp + " C\n";
                            }
                        }
                    } catch {}

                } catch (Exception ex) { info += "Error getting info: " + ex.Message; }

                SafeInvoke(() => { if (lblSysInfo != null) lblSysInfo.Text = info; });
            });
        }
    }
}
