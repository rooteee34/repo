using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage activityPage;
        private CheckBox chkActivityActive;
        private ListBox lstActivityLog;
        private Label lblActivityInfo;
        private System.Windows.Forms.Timer activityTimer;
        private string lastProcessName = "";
        private string lastWindowTitle = "";
        private DateTime lastSwitchTime = DateTime.Now;
        private Dictionary<string, TimeSpan> appUsage = new Dictionary<string, TimeSpan>();

        private void InitTab_ActivityWatcher() {
            activityPage = new TabPage("ACTIVITY");
            activityPage.BackColor = Color.FromArgb(30,30,30);
            activityPage.Name = "ACTIVITY";

            chkActivityActive = CreateCheck("Aktivite Takibini Baslat", 10, 10, activityPage);
            chkActivityActive.ForeColor = Color.Yellow;
            chkActivityActive.CheckedChanged += (s,e) => {
                if (chkActivityActive.Checked) {
                    activityTimer.Start();
                } else {
                    activityTimer.Stop();
                }
            };

            lblActivityInfo = CreateLabel("Aktif pencere ve tarayici URL takibi", 10, 40, 8, activityPage);
            lblActivityInfo.ForeColor = Color.Gray;

            lstActivityLog = new ListBox();
            lstActivityLog.Location = new Point(10, 70);
            lstActivityLog.Size = new Size(660, 400);
            lstActivityLog.BackColor = Color.Black;
            lstActivityLog.ForeColor = Color.Cyan;
            activityPage.Controls.Add(lstActivityLog);

            activityTimer = new System.Windows.Forms.Timer();
            activityTimer.Interval = 1000;
            activityTimer.Tick += ActivityLoop;

            Button btnExport = CreateBtn("RAPOR KAYDET", 10, 480, 200, Color.DarkGreen, activityPage);
            btnExport.Click += (s,e) => ExportActivityLog();

            tabs.TabPages.Add(activityPage);
        }

        private void ActivityLoop(object s, EventArgs e) {
            try {
                IntPtr handle = NativeMethods.GetForegroundWindow();
                int pid;
                NativeMethods.GetWindowThreadProcessId(handle, out pid);
                Process p = Process.GetProcessById(pid);

                string currentProcess = p.ProcessName;
                string currentTitle = p.MainWindowTitle;

                // Browser URL detection (Basic via Title if UIAutomation not avail)
                // Note: Getting URL requires UIAutomation which needs System.Windows.Automation
                // Since I cannot add references easily in this single file context without project file modification,
                // I will use Title based tracking which browsers usually put Title - BrowserName.

                if (currentProcess != lastProcessName || currentTitle != lastWindowTitle) {
                    TimeSpan duration = DateTime.Now - lastSwitchTime;
                    if (duration.TotalSeconds > 1) {
                         string log = string.Format("[{0}] {1} ({2}s) - {3}",
                             lastSwitchTime.ToString("HH:mm:ss"),
                             lastProcessName,
                             (int)duration.TotalSeconds,
                             lastWindowTitle);

                         lstActivityLog.Items.Insert(0, log);

                         if (appUsage.ContainsKey(lastProcessName))
                             appUsage[lastProcessName] += duration;
                         else
                             appUsage[lastProcessName] = duration;
                    }

                    lastProcessName = currentProcess;
                    lastWindowTitle = currentTitle;
                    lastSwitchTime = DateTime.Now;
                }
            } catch { }
        }

        private void ExportActivityLog() {
            try {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("ACTIVITY REPORT - " + DateTime.Now.ToString());
                sb.AppendLine("------------------------------------------------");
                foreach (var kvp in appUsage) {
                    sb.AppendLine(string.Format("{0}: {1}", kvp.Key, kvp.Value));
                }
                File.WriteAllText("ActivityReport.txt", sb.ToString());
                SafeMessageBox("Rapor kaydedildi: ActivityReport.txt");
            } catch (Exception ex) {
                SafeMessageBox("Hata: " + ex.Message);
            }
        }
    }
}
