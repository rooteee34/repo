using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage toolsPage;

        private void InitTab_TOOLS() {
            toolsPage = new TabPage("ARACLAR");
            toolsPage.BackColor = Color.FromArgb(30,30,30);
            toolsPage.Name = "TOOLS";

            // Alarm
            GroupBox gAlarm = CreateGroup("Alarm & Hatirlatici", 10, 10, 660, 100, toolsPage);
            CreateLabel("Sure (dk):", 10, 25, gAlarm);
            NumericUpDown numAlarm = CreateNum(10, 1, 600, 80, 22, 60, gAlarm);
            Button btnSetAlarm = CreateBtn("ALARM KUR", 150, 20, 150, Color.DarkRed, gAlarm);
            btnSetAlarm.Height = 25;
            btnSetAlarm.Click += (s,e) => {
                Task.Delay((int)numAlarm.Value * 60 * 1000).ContinueWith(t => SafeMessageBox("ALARM! SURE DOLDU!"));
                SafeMessageBox("Alarm kuruldu: " + numAlarm.Value + " dakika.");
            };

            // Terminal
            GroupBox gTerm = CreateGroup("Terminal (CMD Wrapper)", 10, 120, 660, 300, toolsPage);
            RichTextBox rtbTerm = new RichTextBox();
            rtbTerm.Location = new Point(10, 20);
            rtbTerm.Size = new Size(640, 200);
            rtbTerm.BackColor = Color.Black;
            rtbTerm.ForeColor = Color.White;
            gTerm.Controls.Add(rtbTerm);

            TextBox txtCmd = new TextBox();
            txtCmd.Location = new Point(10, 230);
            txtCmd.Size = new Size(540, 25);
            gTerm.Controls.Add(txtCmd);

            Button btnSend = CreateBtn("GONDER", 560, 228, 90, Color.Gray, gTerm);
            btnSend.Height = 25;
            btnSend.Click += (s,e) => {
                string cmd = txtCmd.Text;
                RunCMDAsync(cmd, rtbTerm);
                txtCmd.Clear();
            };

            // VPN Shortcut
            Button btnVpn = CreateBtn("VPN BAGLANTILARI (Windows)", 10, 430, 250, Color.Blue, toolsPage);
            btnVpn.Click += (s,e) => Process.Start("ms-settings:network-vpn");

            tabs.TabPages.Add(toolsPage);
        }

        // Renamed/Added to satisfy Pano Logic
        private void InitTab_Pano() { // Was InitTab_TOOLS in v7 but functionality is Pano
            panoPage = new TabPage("PANO");
            panoPage.BackColor = Color.FromArgb(30,30,30);
            panoPage.Name = "PANO";

            lblPanoInfo = CreateLabel("Kopyaladiginiz metinler otomatik kaydedilir", 10, 10, 8, panoPage);
            lblPanoInfo.ForeColor = Color.Gray;

            CreateLabel("Karakter Siniri:", 10, 35, panoPage);
            numClipLimit = CreateNum(5000, 100, 1000000, 120, 32, 80, panoPage);

            CreateLabel("Kontrol Araligi (ms):", 220, 35, panoPage);
            NumericUpDown numClipInterval = CreateNum(500, 100, 5000, 360, 32, 70, panoPage);

            try {
                if(File.Exists(configPath)) {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines) {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2) {
                            if (parts[0] == "Limit") {
                                numClipLimit.Value = decimal.Parse(parts[1]);
                                clipLimit = int.Parse(parts[1]);
                            }
                            if (parts[0] == "Interval") {
                                numClipInterval.Value = decimal.Parse(parts[1]);
                                cachedClipInterval = int.Parse(parts[1]);
                                clipboardTimer.Interval = cachedClipInterval;
                            }
                        }
                    }
                }
            } catch { }

            Button btnSetLimit = CreateBtn("AYARLARI KAYDET", 450, 28, 210, Color.DarkGreen, panoPage);
            btnSetLimit.Height = 30;
            btnSetLimit.Click += (s,e) => {
                try {
                    clipLimit = (int)numClipLimit.Value;
                    cachedClipInterval = (int)numClipInterval.Value;
                    clipboardTimer.Interval = cachedClipInterval;
                    File.WriteAllLines(configPath, new string[] {
                        string.Format("Limit={0}", clipLimit),
                        string.Format("Interval={0}", cachedClipInterval)
                    });
                    SafeMessageBox("Ayarlar Kaydedildi!");
                } catch (Exception ex) { SafeMessageBox("Hata: " + ex.Message); }
            };

            rtbClip = new RichTextBox();
            rtbClip.Location = new Point(10, 70);
            rtbClip.Size = new Size(660, 350);
            rtbClip.BackColor = Color.Black;
            rtbClip.ForeColor = Color.Yellow;
            rtbClip.ReadOnly = true;
            rtbClip.Font = new Font("Consolas", 9);
            panoPage.Controls.Add(rtbClip);
            LoadHistory();

            Button btnClear = CreateBtn("Gecmisi Temizle", 10, 430, 660, Color.Red, panoPage);
            btnClear.Click += (s,e) => {
                rtbClip.Clear();
                lastClipText = "";
                try { if(File.Exists(historyPath)) File.Delete(historyPath); } catch { }
                SafeMessageBox("Temizlendi! ");
            };

            tabs.TabPages.Add(panoPage);
        }

        private void RunCMDAsync(string cmd, RichTextBox output) {
            Task.Run(() => {
                try {
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + cmd);
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    var p = Process.Start(psi);
                    string o = p.StandardOutput.ReadToEnd();
                    string err = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    SafeInvoke(() => output.AppendText(o + err + "\n"));
                } catch (Exception ex) {
                     SafeInvoke(() => output.AppendText("Error: " + ex.Message + "\n"));
                }
            });
        }

        private void SaveHistory() {
            try {
                string content = "";
                SafeInvokeSync(() => {
                    if (rtbClip != null && !rtbClip.IsDisposed) content = rtbClip.Text;
                });
                if (! string.IsNullOrEmpty(content)) {
                    string tempPath = historyPath + ".tmp";
                    File.WriteAllText(tempPath, content);
                    if (File.Exists(historyPath)) File.Delete(historyPath);
                    File.Move(tempPath, historyPath);
                }
            } catch { }
        }

        private void LoadHistory() {
            try {
                if(File.Exists(historyPath)) {
                    string content = File.ReadAllText(historyPath);
                    SafeInvoke(() => {
                        if (rtbClip != null && ! rtbClip.IsDisposed) rtbClip.Text = content;
                    });
                }
            } catch { }
        }
    }
}
