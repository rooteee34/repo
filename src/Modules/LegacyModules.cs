using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Win32;
using System.Linq;

namespace AnoreksikSuite {
    public partial class MainForm {
        // --- FIELDS for Legacy Logic ---

        // RAM & CPU
        private NumericUpDown numRamCpu, numRamTrigger, numRamInterval, numStandbyTrigger;
        private CheckBox chkRamAuto, chkStandbyAuto, chkPageFaultProtect;
        private Label lblRamBig, lblRamInfo, lblPageFaultInfo;
        private int cfgRamCpu = 90;
        private long cfgRamTriggerMB = 1024;
        private long cfgStandbyTriggerMB = 1024;
        private ComboBox cmbProcList, cmbProcListAffinity;
        private ListBox lstBoosted, lstAffinitySet;
        private ComboBox cmbPriority, cmbIoPriority;
        private Label lblCpuInfo;

        // DISK
        private CheckBox chkTemp, chkPrefetch, chkUpdate, chkRecycle, chkDism, chkWer, chkThumbnails, chkBrowser, chkInstallerCache, chkVSS;
        private Label lblDiskGain, lblDiskStatus, lblDiskInfo;
        private long calculatedJunkSize = 0;
        private ProgressBar prgDisk;
        private ComboBox cmbBrowserList;

        // NET
        private ComboBox cmbDnsProvider;
        private Label lblNetInfo, lblTcpInfo, lblQuantumInfo, lblNicInfo, lblArpInfo, lblDscpInfo, lblMsmqInfo;
        private ComboBox cmbNetAdapter;

        // --- SHARED HELPER METHODS (Unifying here) ---
        private void RunCMD(string cmd) {
            try {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + cmd);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi).WaitForExit(5000);
            } catch { }
        }

        // --- DASHBOARD LOGIC ---
        private void InitTab_Dashboard() {
            dashboardPage = new TabPage("ANASAYFA");
            dashboardPage.BackColor = Color.FromArgb(30,30,30);
            dashboardPage.Name = "DASHBOARD";
            dashboardPage.AutoScroll = true;

            Label lblDashInfo = CreateLabel("Kullanmak istediginiz ozellikleri aktif edin - Kapali moduller CPU/RAM kullanmaz", 10, 10, 9, dashboardPage);
            lblDashInfo.ForeColor = Color.Cyan;
            lblDashInfo.Font = new Font("Arial", 9, FontStyle.Bold);

            GroupBox gMain = CreateGroup("Ana Moduller", 10, 40, 660, 100, dashboardPage);

            chkEnableWifi = CreateCheck("WiFi Bekcisi", 20, 30, gMain);
            chkEnableWifi.Checked = true;
            chkEnableWifi.CheckedChanged += (s,e) => {
                cachedWifiEnabled = chkEnableWifi.Checked;
                ApplyDashboardVisibility(); SaveDashboardConfig();
            };

            chkEnableRam = CreateCheck("RAM Optimizasyonu", 180, 30, gMain);
            chkEnableRam.Checked = true;
            chkEnableRam.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

            chkEnableCpu = CreateCheck("CPU Onceliklendirme", 360, 30, gMain);
            chkEnableCpu.Checked = true;
            chkEnableCpu.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

            chkEnableNet = CreateCheck("Ag Optimizasyonu", 20, 60, gMain);
            chkEnableNet.Checked = true;
            chkEnableNet.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

            chkEnableDisk = CreateCheck("Disk Temizligi", 180, 60, gMain);
            chkEnableDisk.Checked = true;
            chkEnableDisk.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

            chkEnablePano = CreateCheck("Pano Yoneticisi", 360, 60, gMain);
            chkEnablePano.Checked = true;
            chkEnablePano.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

            Button btnSaveDash = CreateBtn("KAYDET VE UYGULA", 10, 500, 660, Color.DarkGreen, dashboardPage);
            btnSaveDash.Height = 40;
            btnSaveDash.Click += (s,e) => {
                SaveDashboardConfig();
                ApplyDashboardVisibility();
                SafeMessageBox("Ayarlar kaydedildi ve uygulandi!");
            };

            tabs.TabPages.Add(dashboardPage);
        }

        private void LoadDashboardConfig() {
            try {
                if (File.Exists(dashConfigPath)) {
                    foreach (string line in File.ReadAllLines(dashConfigPath)) {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2) {
                            string key = parts[0];
                            bool value = parts[1] == "True";
                            if (key == "EnableWifi" && chkEnableWifi != null) chkEnableWifi.Checked = value;
                            if (key == "EnableRam" && chkEnableRam != null) chkEnableRam.Checked = value;
                            if (key == "EnableCpu" && chkEnableCpu != null) chkEnableCpu.Checked = value;
                            if (key == "EnableNet" && chkEnableNet != null) chkEnableNet.Checked = value;
                            if (key == "EnableDisk" && chkEnableDisk != null) chkEnableDisk.Checked = value;
                            if (key == "EnablePano" && chkEnablePano != null) chkEnablePano.Checked = value;
                        }
                    }
                }
            } catch { }
            if (chkEnableWifi != null) cachedWifiEnabled = chkEnableWifi.Checked;
        }

        private void SaveDashboardConfig() {
            try {
                List<string> lines = new List<string>();
                SafeInvokeSync(() => {
                    if (chkEnableWifi != null) lines.Add(string.Format("EnableWifi={0}", chkEnableWifi.Checked));
                    if (chkEnableRam != null) lines.Add(string.Format("EnableRam={0}", chkEnableRam.Checked));
                    if (chkEnableCpu != null) lines.Add(string.Format("EnableCpu={0}", chkEnableCpu.Checked));
                    if (chkEnableNet != null) lines.Add(string.Format("EnableNet={0}", chkEnableNet.Checked));
                    if (chkEnableDisk != null) lines.Add(string.Format("EnableDisk={0}", chkEnableDisk.Checked));
                    if (chkEnablePano != null) lines.Add(string.Format("EnablePano={0}", chkEnablePano.Checked));
                });
                string tempPath = dashConfigPath + ".tmp";
                File.WriteAllLines(tempPath, lines.ToArray());
                if (File.Exists(dashConfigPath)) File.Delete(dashConfigPath);
                File.Move(tempPath, dashConfigPath);
            } catch { }
        }

        private void ApplyDashboardVisibility() {
            SafeInvoke(() => {
                bool ramEnabled = chkEnableRam != null && chkEnableRam.Checked;
                if (ramEnabled) { if (!tabs.TabPages.Contains(ramPage)) tabs.TabPages.Add(ramPage); coreTimer.Start(); }
                else { if (tabs.TabPages.Contains(ramPage)) tabs.TabPages.Remove(ramPage); coreTimer.Stop(); }

                bool cpuEnabled = chkEnableCpu != null && chkEnableCpu.Checked;
                if (cpuEnabled) { if (!tabs.TabPages.Contains(cpuPage)) tabs.TabPages.Add(cpuPage); }
                else { if (tabs.TabPages.Contains(cpuPage)) tabs.TabPages.Remove(cpuPage); }

                bool netEnabled = chkEnableNet != null && chkEnableNet.Checked;
                if (netEnabled) { if (!tabs.TabPages.Contains(netPage)) tabs.TabPages.Add(netPage); }
                else { if (tabs.TabPages.Contains(netPage)) tabs.TabPages.Remove(netPage); }

                bool diskEnabled = chkEnableDisk != null && chkEnableDisk.Checked;
                if (diskEnabled) { if (!tabs.TabPages.Contains(diskPage)) tabs.TabPages.Add(diskPage); }
                else { if (tabs.TabPages.Contains(diskPage)) tabs.TabPages.Remove(diskPage); }

                bool wifiEnabled = chkEnableWifi != null && chkEnableWifi.Checked;
                if (wifiEnabled) {
                    if (!tabs.TabPages.Contains(wifiPage)) tabs.TabPages.Add(wifiPage);
                }
                else {
                    if (tabs.TabPages.Contains(wifiPage)) tabs.TabPages.Remove(wifiPage);
                    StopWifiWatchdog();
                }

                bool panoEnabled = chkEnablePano != null && chkEnablePano.Checked;
                if (panoEnabled) { if (!tabs.TabPages.Contains(panoPage)) tabs.TabPages.Add(panoPage); clipboardTimer.Start(); }
                else { if (tabs.TabPages.Contains(panoPage)) tabs.TabPages.Remove(panoPage); clipboardTimer.Stop(); }
            });
        }

        // --- RAM MODULE ---
        private void InitTab_RAM() {
            ramPage = new TabPage("RAM");
            ramPage.BackColor = Color.FromArgb(30,30,30);
            ramPage.Name = "RAM";

            lblRamBig = CreateLabel("--- MB BOS", 10, 10, 24, ramPage);
            lblRamBig.Size = new Size(660, 50);
            lblRamBig.TextAlign = ContentAlignment.MiddleCenter;
            lblRamBig.ForeColor = Color.Red;

            GroupBox g = CreateGroup("Oto Temizlik", 10, 85, 660, 220, ramPage);
            CreateLabel("Max CPU (%):", 20, 30, g);
            numRamCpu = CreateNum(90, 1, 100, 150, 28, 60, g);
            CreateLabel("Min Bos RAM (MB):", 20, 60, g);
            numRamTrigger = CreateNum(1024, 128, 16384, 150, 58, 80, g);
            CreateLabel("Kontrol (sn):", 20, 90, g);
            numRamInterval = CreateNum(2, 1, 300, 150, 88, 60, g);
            chkRamAuto = CreateCheck("Otomatik RAM Temizlik Aktif", 20, 120, g);
            chkRamAuto.Checked = true;

            CreateLabel("Standby Limit (MB):", 20, 150, g);
            numStandbyTrigger = CreateNum(1024, 512, 8192, 150, 148, 80, g);
            chkStandbyAuto = CreateCheck("Standby List Oto-Temizlik", 250, 150, g);

            chkPageFaultProtect = CreateCheck("Page Fault Koruma (Anti-Stutter)", 20, 180, g);
            chkPageFaultProtect.Checked = true;

            Button btnSave = CreateBtn("KAYDET", 500, 25, 140, Color.DarkGreen, g);
            btnSave.Height=30;
            btnSave.Click += (s,e) => {
                cfgRamCpu = (int)numRamCpu.Value;
                cfgRamTriggerMB = (long)numRamTrigger.Value;
                cfgStandbyTriggerMB = (long)numStandbyTrigger.Value;
                coreTimer.Interval = (int)numRamInterval.Value * 1000;
                SafeMessageBox("Ayarlar Kaydedildi!");
            };

            Button btnNuke = CreateBtn("STANDBY SIL", 10, 320, 320, Color.DarkBlue, ramPage);
            btnNuke.Click += (s,e) => Task.Run(() => NukeStandby());

            Button btnCompress = CreateBtn("SIKISTIR (Aktif Haric)", 340, 320, 330, Color.Teal, ramPage);
            btnCompress.Click += (s,e) => CompressApps();

            Button btnCache = CreateBtn("FILE CACHE TEMIZLE (Native Kernel)", 10, 370, 660, Color.Purple, ramPage);
            btnCache.Click += (s,e) => TrimSystemFileCache();

            tabs.TabPages.Add(ramPage);
        }

        private void CoreLoop(object s, EventArgs e) {
            if (isShuttingDown) return;
            bool ramEnabled = false;
            SafeInvokeSync(() => { if (chkEnableRam != null) ramEnabled = chkEnableRam.Checked; });
            if (!ramEnabled) return;

            float currentCpu = 0;
            try { if (cpuCounter != null) currentCpu = cpuCounter.NextValue(); } catch { }

            NativeMethods.MEMORYSTATUSEX mem = new NativeMethods.MEMORYSTATUSEX();
            mem.dwLength = (uint)Marshal.SizeOf(typeof(NativeMethods.MEMORYSTATUSEX));
            if (NativeMethods.GlobalMemoryStatusEx(ref mem)) {
                long freeMB = (long)(mem.ullAvailPhys / (1024*1024));

                if (! isHidden && this.Visible) {
                    SafeInvoke(() => {
                        if (lblRamBig != null && ! lblRamBig.IsDisposed) {
                            lblRamBig.Text = string.Format("{0} MB BOS", freeMB);
                            lblRamBig.ForeColor = freeMB < cfgRamTriggerMB ? Color.Orange : Color.Lime;
                        }
                    });
                }

                // Simplified Auto-Clean Logic
                bool autoRam = false;
                SafeInvokeSync(() => { if (chkRamAuto != null) autoRam = chkRamAuto.Checked; });

                if (autoRam && freeMB < cfgRamTriggerMB && currentCpu < cfgRamCpu) {
                    if (Interlocked.CompareExchange(ref ramCleaningFlag, 1, 0) == 0) {
                        Task.Run(() => {
                            try { NukeStandbyInternal(); }
                            finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
                        });
                    }
                }
            }
        }

        private long GetStandbyListSize() {
            try { if (standbyCounter != null) return (long)(standbyCounter.NextValue() / (1024 * 1024)); } catch { } return 0;
        }

        private void NukeStandbyInternal() {
            GCHandle h = default(GCHandle);
            try {
                int command = NativeMethods.MemoryPurgeStandbyList;
                h = GCHandle.Alloc(command, GCHandleType.Pinned);
                NativeMethods.NtSetSystemInformation(NativeMethods.SystemMemoryListInformation, h.AddrOfPinnedObject(), Marshal.SizeOf(typeof(int)));
                Thread.Sleep(50);
                GC.Collect();
                NativeMethods.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, new IntPtr(-1), new IntPtr(-1));
            } catch {} finally { if (h.IsAllocated) h.Free(); Thread.Sleep(500); }
        }

        private void NukeStandby() {
            if (Interlocked.CompareExchange(ref ramCleaningFlag, 1, 0) != 0) return;
            try { NukeStandbyInternal(); } finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
        }

        private void TrimSystemFileCache() {
             Task.Run(() => {
                try {
                    if (Environment.Is64BitOperatingSystem) {
                        NativeMethods.SYSTEM_CACHE_INFORMATION_64_BIT cacheInfo = new NativeMethods.SYSTEM_CACHE_INFORMATION_64_BIT();
                        cacheInfo.MinimumWorkingSet = -1;
                        cacheInfo.MaximumWorkingSet = -1;
                        GCHandle handle = GCHandle.Alloc(cacheInfo, GCHandleType.Pinned);
                        NativeMethods.NtSetSystemInformation(NativeMethods.SystemFileCacheInformation, handle.AddrOfPinnedObject(), Marshal.SizeOf(cacheInfo));
                        handle.Free();
                    }
                    SafeMessageBox("Sistem Dosya Onbellegi temizlendi!");
                } catch {}
             });
        }

        private void CompressApps() {
             Task.Run(() => {
                 Process current = Process.GetCurrentProcess();
                 foreach(Process p in Process.GetProcesses()) {
                     if (p.Id != current.Id) try { NativeMethods.EmptyWorkingSet(p.Handle); } catch {}
                 }
                 SafeMessageBox("RAM Sikistirildi.");
             });
        }

        // --- CPU MODULE ---
        private void InitTab_CPU() {
            cpuPage = new TabPage("CPU");
            cpuPage.BackColor = Color.FromArgb(30,30,30);
            cpuPage.Name = "CPU";

            GroupBox gPrio = CreateGroup("Oncelik Yonetimi", 10, 30, 660, 140, cpuPage);
            CreateLabel("Uygulama:", 15, 25, gPrio);
            cmbProcList = new ComboBox(); cmbProcList.Location = new Point(15, 45); cmbProcList.Size = new Size(300, 25); gPrio.Controls.Add(cmbProcList);
            Button btnRef = CreateBtn("Yenile", 325, 44, 200, Color.Gray, gPrio); btnRef.Height = 23; btnRef.Click += (s,e) => RefreshProcs();

            CreateLabel("CPU Oncelik:", 15, 75, gPrio);
            cmbPriority = new ComboBox(); cmbPriority.Items.AddRange(new string[] {"High", "Normal", "Low"}); cmbPriority.SelectedIndex = 1; cmbPriority.Location = new Point(15, 95); gPrio.Controls.Add(cmbPriority);

            Button btnBoost = CreateBtn("UYGULA", 255, 90, 270, Color.DarkRed, gPrio); btnBoost.Height=30; btnBoost.Click += (s,e) => BoostProc();

            tabs.TabPages.Add(cpuPage);
        }

        private void RefreshProcs() {
             cmbProcList.Items.Clear();
             foreach(Process p in Process.GetProcesses()) cmbProcList.Items.Add(p.ProcessName);
        }

        private void BoostProc() {
             if (cmbProcList.SelectedItem == null) return;
             string name = cmbProcList.SelectedItem.ToString();
             foreach(Process p in Process.GetProcessesByName(name)) {
                 try { p.PriorityClass = ProcessPriorityClass.High; } catch {}
             }
             SafeMessageBox(name + " Boosted!");
        }

        // --- NET MODULE ---
        private void InitTab_NET() {
            netPage = new TabPage("NET");
            netPage.BackColor = Color.FromArgb(30,30,30);
            netPage.Name = "NET";

            GroupBox gAdapt = CreateGroup("Adaptor Secimi", 10, 30, 660, 80, netPage);
            cmbNetAdapter = new ComboBox(); cmbNetAdapter.Location = new Point(110, 28); cmbNetAdapter.Size = new Size(300, 25); gAdapt.Controls.Add(cmbNetAdapter);

            Button btnRef = CreateBtn("YENILE", 420, 27, 220, Color.Gray, gAdapt);
            btnRef.Click += (s,e) => {
                cmbNetAdapter.Items.Clear();
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    if (ni.OperationalStatus == OperationalStatus.Up) cmbNetAdapter.Items.Add(ni.Name);
            };

            GroupBox gDns = CreateGroup("DNS Switcher", 10, 120, 660, 110, netPage);
            cmbDnsProvider = new ComboBox();
            cmbDnsProvider.Items.AddRange(new string[] {"Cloudflare", "Google", "OpenDNS"});
            cmbDnsProvider.Location = new Point(15, 50); gDns.Controls.Add(cmbDnsProvider);

            Button btnSetDns = CreateBtn("DNS AYARLA", 230, 45, 410, Color.DarkGreen, gDns);
            btnSetDns.Click += (s,e) => {
                if (cmbDnsProvider.Text == "Cloudflare") SetDNS("1.1.1.1", "1.0.0.1");
                else if (cmbDnsProvider.Text == "Google") SetDNS("8.8.8.8", "8.8.4.4");
            };

            tabs.TabPages.Add(netPage);
        }

        private void SetDNS(string p, string s) {
             RunCMD(string.Format("netsh interface ip set dns \"{0}\" static {1} primary", activeAdapter, p));
             RunCMD(string.Format("netsh interface ip add dns \"{0}\" {1} index=2", activeAdapter, s));
             SafeMessageBox("DNS Ayarlandi!");
        }

        // --- DISK MODULE ---
        private void InitTab_DISK() {
            diskPage = new TabPage("DISK");
            diskPage.BackColor = Color.FromArgb(30,30,30);
            diskPage.Name = "DISK";

            GroupBox gClean = CreateGroup("Deep Clean", 10, 30, 660, 430, diskPage);
            chkTemp = CreateCheck("Temp Klasorleri", 20, 30, gClean); chkTemp.Checked=true;

            Button btnDeep = CreateBtn("TEMIZLE", 225, 315, 195, Color.DarkRed, gClean);
            btnDeep.Click += (s,e) => Task.Run(() => DeepClean());

            tabs.TabPages.Add(diskPage);
        }

        private void DeepClean() {
            if (chkTemp.Checked) {
                CleanDirSafe(Environment.GetEnvironmentVariable("TEMP"));
                CleanDirSafe(@"C:\Windows\Temp");
            }
            SafeMessageBox("Temizlik Tamamlandi!");
        }

        private void CleanDirSafe(string path) {
             if (!Directory.Exists(path)) return;
             try {
                 foreach(var f in Directory.GetFiles(path)) try { File.Delete(f); } catch {}
                 foreach(var d in Directory.GetDirectories(path)) try { Directory.Delete(d, true); } catch {}
             } catch { }
        }

        // --- HELPERS ---
        private string GetActiveAdapter() {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                if (ni.OperationalStatus == OperationalStatus.Up && (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)) return ni.Name;
            }
            return "Wi-Fi";
        }

        private Guid GetAdapterGuid(string name) {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                if (ni.Name == name) return Guid.Parse(ni.Id);
            }
            return Guid.Empty;
        }

        private void ClipboardLoop(object s, EventArgs e) {
             if (Clipboard.ContainsText()) {
                 string txt = Clipboard.GetText();
                 if (txt != lastClipText) {
                     lastClipText = txt;
                     if (rtbClip != null) rtbClip.Text = txt + "\n\n" + rtbClip.Text;
                 }
             }
        }

        private void StopActivityWatcher() {
             if (activityTimer != null) activityTimer.Stop();
        }
    }
}
