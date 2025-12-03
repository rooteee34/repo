using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Management;
using System.ComponentModel;
using System.Reflection;

namespace AnoreksikSuite {
    public partial class MainForm : Form {
        private readonly object resetLock = new object();
        private readonly object ramCleanLock = new object();
        private readonly SemaphoreSlim wlanSem = new SemaphoreSlim(1,1);
        private readonly SemaphoreSlim arpLock = new SemaphoreSlim(1,1);

        private const int IDLE_SLEEP_MS = 100;
        private const int RESET_WAIT_STEPS = 30;
        private const int POST_RESET_PING_TIMEOUT = 150;
        private const int DISCONNECT_WAIT_MS = 50;
        private const int AUTOCONFIG_WAIT_MS = 30;
        private const int FALLBACK_WAIT_MS = 200;
        private const int AUTOCONFIG_OFF_WAIT_MS = 100;
        private const int ERROR_SLEEP_MS = 500;
        private const int STABILIZATION_SLEEP_MS = 100;

        private int ramCleaningFlag = 0;
        private int isSignalProcessing = 0;
        private int cachedClipInterval = 500;

        private TabControl tabs;
        private NotifyIcon trayIcon;
        private System.Windows.Forms.Timer coreTimer, wifiSignalTimer, wifiKeepAliveTimer, clipboardTimer;
        private bool isHidden = false;
        private PerformanceCounter cpuCounter, pageFaultCounter;
        private volatile bool isShuttingDown = false;
        private PerformanceCounter standbyCounter;

        private volatile bool wifiModuleActive = false;
        private CancellationTokenSource wifiCts;
        private Task wifiWatchdogTask;
        private volatile bool cachedLoggingEnabled = true;
        private volatile bool cachedWifiEnabled = true;
        private volatile bool cachedSignalEnabled = true;

        private ToolTip toolTip;
        private string historyPath, configPath, wifiConfigPath, dashConfigPath;

        public MainForm() {
            EnablePrivilege(NativeMethods.SE_INCREASE_QUOTA_NAME);
            EnablePrivilege(NativeMethods.SE_PROFILE_SINGLE_PROCESS_NAME);
            EnablePrivilege("SeDebugPrivilege");
            EnablePrivilege("SeTcbPrivilege");
            EnablePrivilege("SeSystemProfilePrivilege");
            EnablePrivilege("SeRestorePrivilege");
            EnablePrivilege("SeTakeOwnershipPrivilege");

            try { Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High; } catch { }

            historyPath = Path.Combine(Application.StartupPath, "pano.dat");
            configPath = Path.Combine(Application.StartupPath, "pano_config.dat");
            wifiConfigPath = Path.Combine(Application.StartupPath, "wifi_config.dat");
            dashConfigPath = Path.Combine(Application.StartupPath, "dashboard_config.dat");

            activeAdapter = GetActiveAdapter();
            activeGuid = GetAdapterGuid(activeAdapter);
            LoadWifiConfig();

            keepAlivePing = new Ping();
            keepAliveBuffer = new byte[1];

            toolTip = new ToolTip();
            toolTip.AutoPopDelay = 8000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 200;

            clipboardTimer = new System.Windows.Forms.Timer();
            clipboardTimer.Interval = 700;
            clipboardTimer.Tick += ClipboardLoop;

            coreTimer = new System.Windows.Forms.Timer();
            coreTimer.Interval = 2000;
            coreTimer.Tick += CoreLoop;

            wifiSignalTimer = new System.Windows.Forms.Timer();
            wifiSignalTimer.Interval = signalUpdateInterval;
            wifiSignalTimer.Tick += WifiSignalLoop;

            wifiKeepAliveTimer = new System.Windows.Forms.Timer();
            wifiKeepAliveTimer.Interval = 3000;
            wifiKeepAliveTimer.Tick += KeepAliveLoop;

            this.Text = "ANOREKSIK SUITE V10 - ULTIMATE ENTERPRISE EDITION";
            this.Size = new Size(700, 850);
            this.BackColor = Color.FromArgb(18,18,18);
            this.ForeColor = Color.LightGray;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            try { this.Icon = Icon.ExtractAssociatedIcon(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\taskmgr.exe"); } catch { }

            this.Resize += (s,e) => {
                if (this.WindowState == FormWindowState.Minimized) {
                    this.Hide();
                    this.ShowInTaskbar = false;
                    isHidden = true;
                    Task.Run(() => OptimizeMemory());
                } else {
                    isHidden = false;
                    FlushWifiLogs();
                }
            };

            try { cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"); cpuCounter.NextValue(); } catch { }
            try { pageFaultCounter = new PerformanceCounter("Memory", "Page Faults/sec"); pageFaultCounter.NextValue(); } catch { }

            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Appearance = TabAppearance.FlatButtons;
            tabs.ItemSize = new Size(75, 28);

            InitTab_Dashboard();
            InitTab_RAM();
            InitTab_CPU();
            InitTab_NET();
            InitTab_DISK();
            InitTab_WIFI();
            InitTab_TOOLS();
            InitTab_Pano(); // Added explicit call to InitTab_Pano
            InitTab_ActivityWatcher();
            InitTab_Startup();
            InitTab_Security();
            InitTab_SystemInfo();
            InitTab_Passwords();

            this.Controls.Add(tabs);

            trayIcon = new NotifyIcon();
            try { trayIcon.Icon = this.Icon; } catch { }
            trayIcon.Text = "Anoreksik Suite V10";
            trayIcon.Visible = true;
            trayIcon.MouseDoubleClick += (s,e) => ShowApp();

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Goster", (s,e) => ShowApp());
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("KAPAT", (s,e) => ShutdownApp());
            trayIcon.ContextMenu = menu;

            try {
                standbyCounter = new PerformanceCounter("Memory", "Standby Cache Core Bytes", true);
                standbyCounter.NextValue();
            } catch { standbyCounter = null; }

            LoadDashboardConfig();
            ApplyDashboardVisibility();
        }

        private void SafeInvoke(Action a) { if (isShuttingDown || this.IsDisposed || ! this.IsHandleCreated) return; try { if (this.InvokeRequired) this.BeginInvoke(a); else a(); } catch { } }
        private void SafeInvokeSync(Action a) { if (isShuttingDown || this.IsDisposed || !this.IsHandleCreated) return; try { if (this.InvokeRequired) this.Invoke(a); else a(); } catch { } }
        private void SafeMessageBox(string m, string t = "Bilgi") { SafeInvoke(() => MessageBox.Show(this, m, t, MessageBoxButtons.OK, MessageBoxIcon.Information)); }

        private bool EnablePrivilege(string privilege) {
            try {
                IntPtr hTok = IntPtr.Zero;
                NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle, 0x0020 | 0x0008, ref hTok);
                NativeMethods.TokPriv1Luid tp;
                tp.Count=1;
                tp.Luid=0;
                tp.Attr=0x00000002;
                NativeMethods.LookupPrivilegeValue(null, privilege, ref tp.Luid);
                NativeMethods.AdjustTokenPrivileges(hTok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                return true;
            } catch {
                return false;
            }
        }

        private void LogError(string ctx, Exception ex) {
            try {
                string logPath = Path.Combine(Application.StartupPath, "errors.log");
                using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096)) {
                    byte[] data = Encoding.UTF8.GetBytes(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}\n{3}\n\n", DateTime.Now, ctx, ex.Message, ex.StackTrace));
                    fs.Write(data, 0, data.Length);
                }
            } catch { }
        }

        private void ShutdownApp() {
            SaveDashboardConfig();
            SaveHistory();

            isShuttingDown = true;

            try { if (coreTimer != null) { coreTimer.Stop(); coreTimer.Dispose(); } } catch { }
            try { if (wifiSignalTimer != null) { wifiSignalTimer.Stop(); wifiSignalTimer.Dispose(); } } catch { }
            try { if (wifiKeepAliveTimer != null) { wifiKeepAliveTimer.Stop(); wifiKeepAliveTimer.Dispose(); } } catch { }
            try { if (clipboardTimer != null) { clipboardTimer.Stop(); clipboardTimer.Dispose(); } } catch { }

            StopWifiWatchdog();
            StopActivityWatcher();

            try { if (keepAlivePing != null) keepAlivePing.Dispose(); } catch { }
            try { if (wlanSem != null) wlanSem.Dispose(); } catch { }
            try { if (arpLock != null) arpLock.Dispose(); } catch { }
            try { if (cpuCounter != null) cpuCounter.Dispose(); } catch { }
            try { if (pageFaultCounter != null) pageFaultCounter.Dispose(); } catch { }
            try { if (standbyCounter != null) standbyCounter.Dispose(); } catch { }
            try { trayIcon.Visible = false; if (trayIcon != null) trayIcon.Dispose(); } catch { }
            try { if (toolTip != null) toolTip.Dispose(); } catch { }

            Environment.Exit(0);
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            if(e.CloseReason==CloseReason.UserClosing) {
                e.Cancel=true;
                this.Hide();
                this.ShowInTaskbar=false;
                isHidden=true;
                Task.Run(() => OptimizeMemory());
            } else {
                ShutdownApp();
            }
        }

        private void ShowApp() {
            SafeInvoke(() => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                isHidden = false;
            });
            FlushWifiLogs();
        }

        private void OptimizeMemory() {
            try {
                NativeMethods.EmptyWorkingSet(Process.GetCurrentProcess(). Handle);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch { }
        }
    }
}
