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
    }
}
