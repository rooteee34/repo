using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage startupPage;
        private ListBox lstStartup;
        private TextBox txtStartupName, txtStartupPath;

        private void InitTab_Startup() {
            startupPage = new TabPage("BASLANGIC");
            startupPage.BackColor = Color.FromArgb(30,30,30);
            startupPage.Name = "STARTUP";

            CreateLabel("Baslangic Ogeleri (Registry & Task Scheduler)", 10, 10, 10, startupPage).ForeColor = Color.Yellow;

            lstStartup = new ListBox();
            lstStartup.Location = new Point(10, 40);
            lstStartup.Size = new Size(660, 300);
            lstStartup.BackColor = Color.Black;
            lstStartup.ForeColor = Color.Lime;
            startupPage.Controls.Add(lstStartup);

            Button btnRefresh = CreateBtn("YENILE", 10, 350, 150, Color.Gray, startupPage);
            btnRefresh.Click += (s,e) => RefreshStartupList();

            Button btnDelete = CreateBtn("SIL", 170, 350, 150, Color.Red, startupPage);
            btnDelete.Click += (s,e) => DeleteStartupItem();

            GroupBox gAdd = CreateGroup("Yeni Ekle", 10, 400, 660, 120, startupPage);
            CreateLabel("Ad:", 10, 25, gAdd);
            txtStartupName = new TextBox(); txtStartupName.Location = new Point(50, 22); txtStartupName.Size = new Size(200, 20); gAdd.Controls.Add(txtStartupName);

            CreateLabel("Yol:", 10, 55, gAdd);
            txtStartupPath = new TextBox(); txtStartupPath.Location = new Point(50, 52); txtStartupPath.Size = new Size(400, 20); gAdd.Controls.Add(txtStartupPath);

            Button btnBrowse = CreateBtn("...", 460, 50, 40, Color.Gray, gAdd);
            btnBrowse.Height = 23;
            btnBrowse.Click += (s,e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) txtStartupPath.Text = ofd.FileName;
            };

            Button btnAdd = CreateBtn("EKLE", 520, 20, 100, Color.DarkGreen, gAdd);
            btnAdd.Height = 60;
            btnAdd.Click += (s,e) => AddStartupItem();

            // System Restore Button
            Button btnRestore = CreateBtn("SISTEM GERI YUKLEME NOKTASI OLUSTUR", 10, 530, 300, Color.Blue, startupPage);
            btnRestore.Click += (s,e) => CreateRestorePoint();

            RefreshStartupList();
            tabs.TabPages.Add(startupPage);
        }

        private void RefreshStartupList() {
            lstStartup.Items.Clear();
            try {
                // Registry
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if (key != null) foreach (string v in key.GetValueNames()) lstStartup.Items.Add("[HKCU] " + v + " = " + key.GetValue(v));
                }
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run")) {
                    if (key != null) foreach (string v in key.GetValueNames()) lstStartup.Items.Add("[HKLM] " + v + " = " + key.GetValue(v));
                }

                // Task Scheduler (Parsed from schtasks CSV output)
                ProcessStartInfo psi = new ProcessStartInfo("schtasks", "/query /fo CSV /nh");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                var p = Process.Start(psi);
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                foreach (string line in output.Split('\n')) {
                    if (!string.IsNullOrWhiteSpace(line)) {
                        string[] parts = line.Split(',');
                        if (parts.Length > 0 && parts[0].Trim('"').StartsWith("\\")) {
                            lstStartup.Items.Add("[TASK] " + parts[0].Trim('"') + " (" + parts[1].Trim('"') + ")");
                        }
                    }
                }
            } catch { }
        }

        private void DeleteStartupItem() {
            if (lstStartup.SelectedItem == null) return;
            string item = lstStartup.SelectedItem.ToString();

            try {
                if (item.StartsWith("[HKCU]")) {
                    string name = item.Substring(7, item.IndexOf(" = ") - 7);
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                        key.DeleteValue(name);
                } else if (item.StartsWith("[HKLM]")) {
                    string name = item.Substring(7, item.IndexOf(" = ") - 7);
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                        key.DeleteValue(name);
                } else if (item.StartsWith("[TASK]")) {
                     string taskName = item.Split(' ')[1]; // Simplistic parsing
                     Process.Start("schtasks", "/delete /tn \"" + taskName + "\" /f");
                }
                RefreshStartupList();
            } catch (Exception ex) { SafeMessageBox("Hata: " + ex.Message); }
        }

        private void AddStartupItem() {
            if (string.IsNullOrEmpty(txtStartupName.Text) || string.IsNullOrEmpty(txtStartupPath.Text)) return;
            try {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    key.SetValue(txtStartupName.Text, txtStartupPath.Text);
                RefreshStartupList();
                SafeMessageBox("Eklendi!");
            } catch (Exception ex) { SafeMessageBox("Hata: " + ex.Message); }
        }

        private void CreateRestorePoint() {
            Task.Run(() => {
                try {
                    // WMI SystemRestore class usually only available on Client OS (Win 7/8/10/11)
                    ManagementScope scope = new ManagementScope("\\\\localhost\\root\\default");
                    ManagementPath path = new ManagementPath("SystemRestore");
                    ObjectGetOptions options = new ObjectGetOptions();
                    ManagementClass processClass = new ManagementClass(scope, path, options);

                    ManagementBaseObject inParams = processClass.GetMethodParameters("CreateRestorePoint");
                    inParams["Description"] = "Anoreksik Suite Auto Point " + DateTime.Now.ToString();
                    inParams["RestorePointType"] = 12; // MODIFY_SETTINGS
                    inParams["EventType"] = 100; // BEGIN_SYSTEM_CHANGE

                    ManagementBaseObject outParams = processClass.InvokeMethod("CreateRestorePoint", inParams, null);

                    SafeMessageBox("Restore Point Olusturuldu (Sonuc: " + outParams["ReturnValue"] + ")");
                } catch (Exception ex) {
                    SafeMessageBox("Hata: " + ex.Message + "\n\nNot: System Restore servisi acik olmalidir.");
                }
            });
        }
    }
}
