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

namespace AnoreksikSuite {
    public class MainForm : Form {
        // === NATIVE WIFI API ===
        [DllImport("wlanapi.dll")] public static extern uint WlanOpenHandle(uint v, IntPtr p, out uint n, out IntPtr h);
        [DllImport("wlanapi.dll")] public static extern uint WlanCloseHandle(IntPtr h, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanEnumInterfaces(IntPtr h, IntPtr p, out IntPtr i);
        [DllImport("wlanapi.dll")] public static extern uint WlanDisconnect(IntPtr h, ref Guid g, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanConnect(IntPtr h, ref Guid g, ref WLAN_CONNECTION_PARAMETERS par, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern void WlanFreeMemory(IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanQueryInterface(IntPtr h, ref Guid g, WLAN_INTF_OPCODE op, IntPtr p, out uint sz, out IntPtr dat, IntPtr t);
        [DllImport("wlanapi.dll")] public static extern uint WlanScan(IntPtr h, ref Guid g, IntPtr pDot11Ssid, IntPtr pIeData, IntPtr pReserved);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct WLAN_INTERFACE_INFO { public Guid InterfaceGuid; [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)] public string strInterfaceDescription; public uint isState; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_INTERFACE_INFO_LIST { public uint dwNumberOfItems; public uint dwIndex; public WLAN_INTERFACE_INFO InterfaceInfo; }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct DOT11_SSID { public uint uSSIDLength; [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] public byte[] ucSSID; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_CONNECTION_PARAMETERS { public uint wlanConnectionMode; public IntPtr strProfile; public IntPtr pDot11Ssid; public IntPtr pDesiredBssidList; public uint dot11BssType; public uint dwFlags; }
        public enum WLAN_INTF_OPCODE { wlan_intf_opcode_current_connection = 7, wlan_intf_opcode_rssi = 0x10000102 }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_CONNECTION_ATTRIBUTES { public uint isState; public uint wlanConnectionMode; [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)] public string strProfileName; public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes; public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_ASSOCIATION_ATTRIBUTES { public DOT11_SSID dot11Ssid; public uint dot11BssType; [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)] public byte[] dot11Bssid; public uint dot11PhyType; public uint uDot11PhyIndex; public uint wlanSignalQuality; public uint ulRxRate; public uint ulTxRate; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_SECURITY_ATTRIBUTES { [MarshalAs(UnmanagedType.Bool)] public bool bSecurityEnabled; [MarshalAs(UnmanagedType.Bool)] public bool bOneXEnabled; public uint dot11AuthAlgorithm; public uint dot11CipherAlgorithm; }

        // === KERNEL API ===
        [DllImport("ntdll.dll")] public static extern int NtSetSystemInformation(int c, IntPtr p, int l);
        [DllImport("ntdll.dll")] public static extern int NtSetInformationProcess(IntPtr h, int c, ref int i, int l);
        [DllImport("psapi.dll")] public static extern int EmptyWorkingSet(IntPtr h);
        [DllImport("psapi.dll")] public static extern bool SetProcessWorkingSetSize(IntPtr h, IntPtr min, IntPtr max);
        [DllImport("kernel32.dll")] public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX l);
        [DllImport("kernel32.dll")] public static extern bool SetProcessAffinityMask(IntPtr h, IntPtr m);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr h, out int p);
        [DllImport("advapi32.dll", SetLastError = true)] internal static extern bool AdjustTokenPrivileges(IntPtr t, bool d, ref TokPriv1Luid n, int l, IntPtr p, IntPtr r);
        [DllImport("advapi32.dll")] internal static extern bool OpenProcessToken(IntPtr h, int a, ref IntPtr t);
        [DllImport("advapi32.dll", SetLastError = true)] internal static extern bool LookupPrivilegeValue(string s, string n, ref long l);
        [DllImport("shell32.dll")] public static extern int SHEmptyRecycleBin(IntPtr h, string r, uint f);

        [StructLayout(LayoutKind.Sequential, Pack=1)] internal struct TokPriv1Luid { public int Count; public long Luid; public int Attr; }
        [StructLayout(LayoutKind.Sequential)] public struct MEMORYSTATUSEX { public uint dwLength; public uint dwMemoryLoad; public ulong ullTotalPhys; public ulong ullAvailPhys; public ulong ullTotalPageFile; public ulong ullAvailPageFile; public ulong ullTotalVirtual; public ulong ullAvailVirtual; public ulong ullAvailExtendedVirtual; }

        // === SYSTEM CACHE STRUCTURES (Native File Cache Temizliği için) ===
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct SYSTEM_CACHE_INFORMATION {
            public uint CurrentSize;
            public uint PeakSize;
            public uint PageFaultCount;
            public uint MinimumWorkingSet;
            public uint MaximumWorkingSet;
            public uint Unused1;
            public uint Unused2;
            public uint Unused3;
            public uint Unused4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct SYSTEM_CACHE_INFORMATION_64_BIT {
            public long CurrentSize;
            public long PeakSize;
            public long PageFaultCount;
            public long MinimumWorkingSet;
            public long MaximumWorkingSet;
            public long Unused1;
            public long Unused2;
            public long Unused3;
            public long Unused4;
        }

        // System Information Classes
        private const int SystemFileCacheInformation = 0x0015; // Class 21
        private const int SystemMemoryListInformation = 0x0050; // Class 80
        private const int MemoryPurgeStandbyList = 4;
        private const int SE_PRIVILEGE_ENABLED = 2;
        private const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        private const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";

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
        private System.Windows.Forms.Timer coreTimer, wifiSignalTimer, wifiKeepAliveTimer;
        private bool isHidden = false;
        private PerformanceCounter cpuCounter, pageFaultCounter;
        private volatile bool isShuttingDown = false;

private PerformanceCounter standbyCounter;

private System.Windows.Forms.Timer clipboardTimer;

        // Thread kontrol flagları
        private volatile bool wifiModuleActive = false;
        private CancellationTokenSource wifiCts;
        private Task wifiWatchdogTask;
private volatile bool cachedLoggingEnabled = true;
private volatile bool cachedWifiEnabled = true;
private volatile bool cachedSignalEnabled = true;


        // DASHBOARD (Ana Sayfa)
        private TabPage dashboardPage, ramPage, cpuPage, netPage, diskPage, wifiPage, panoPage;
        private CheckBox chkEnableWifi, chkEnableRam, chkEnableCpu, chkEnableNet, chkEnableDisk, chkEnablePano;

        // Dashboard - WiFi Alt Özellikleri
        private CheckBox chkWifiPingSelect, chkWifiSignal, chkWifiCustomInterval, chkWifiTTL;
        private CheckBox chkWifiKeepAlive, chkWifiLogging;

        // Dashboard - RAM Alt Özellikleri
        private CheckBox chkDashRamAuto, chkDashStandbyAuto, chkDashPageFault;

        // Dashboard - Disk Alt Özellikleri
        private CheckBox chkDashTemp, chkDashPrefetch, chkDashUpdate, chkDashRecycle;
        private CheckBox chkDashWer, chkDashThumbnails, chkDashDism, chkDashBrowser;
        private CheckBox chkDashInstaller, chkDashVSS;

        private Label lblDashInfo;

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

        // WIFI
        private Label lblWifiStat, lblWifiPing, lblWifiSSID, lblWifiSignal, lblWifiSpeed, lblWifiInfo;
        private Panel pnlKeepAliveIndicator; // Görsel Keep-Alive göstergesi
        private ListBox lstWifiLog;
        private CheckBox chkWifiActive;
        private ComboBox cmbWifiTarget, cmbNetAdapter;
        private NumericUpDown numWifiTimeout, numWifiFailureThreshold, numWifiTTL, numWifiBufferSize, numSignalInterval;

        private string wifiTargetIP = "1.1.1.1";
        private string currentSSID = "";
        private string activeAdapter = "Wi-Fi";
        private Guid activeGuid = Guid.Empty;
        private int failureCount = 0;
        private volatile bool isResetting = false;
        private int totalDisconnects = 0;
private volatile bool userDisabledAutoConfig = false;
        private string lockedArpGateway = "";
        private string lockedArpMac = "";

        private int wifiTimeout = 1000;
        private int wifiFailureThreshold = 2;
        private int wifiTTL = 64;
        private int wifiBufferSize = 32;
        private int signalUpdateInterval = 5000;
        private ConcurrentQueue<string> wifiLogBuffer = new ConcurrentQueue<string>();


        private byte[] keepAliveBuffer;
        private Ping keepAlivePing;

        // PANO
        private RichTextBox rtbClip;
        private NumericUpDown numClipLimit;
        private string lastClipText = "";
        private string historyPath = "";
        private string configPath = "";
        private string wifiConfigPath = "";
        private string dashConfigPath = "";
        private int clipLimit = 5000;
        private Label lblPanoInfo;

        // NET
        private ComboBox cmbDnsProvider;
        private Label lblNetInfo, lblTcpInfo, lblQuantumInfo, lblNicInfo, lblArpInfo, lblDscpInfo, lblMsmqInfo;

        private ToolTip toolTip;

        private void LogError(string ctx, Exception ex) {
            try {
                string logPath = Path.Combine(Application.StartupPath, "errors.log");
                // Pre-allocation için dosyayı açıp yazmak
                using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096)) {
                    byte[] data = Encoding.UTF8.GetBytes(string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}: {2}\n{3}\n\n", DateTime.Now, ctx, ex.Message, ex.StackTrace));
                    fs.Write(data, 0, data.Length);
                }
            } catch { }
        }

public MainForm() {
    EnablePrivilege("SeIncreaseQuotaPrivilege");
    EnablePrivilege("SeProfileSingleProcessPrivilege");
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

    // ═══════════════════════════════════════════════════════
    // TIMER'LAR - SADECE BİR KEZ OLUŞTUR
    // ═══════════════════════════════════════════════════════
    coreTimer = new System.Windows.Forms.Timer();
    coreTimer.Interval = 2000;
    coreTimer.Tick += CoreLoop;

    // WiFi Signal Timer - WifiSignalLoop kullan (SignalLoop DEĞİL)
    wifiSignalTimer = new System.Windows.Forms.Timer();
    wifiSignalTimer.Interval = signalUpdateInterval; // Config'den oku
    wifiSignalTimer.Tick += WifiSignalLoop; // ← TEK METOD

    wifiKeepAliveTimer = new System.Windows.Forms.Timer();
    wifiKeepAliveTimer.Interval = 3000;
    wifiKeepAliveTimer.Tick += KeepAliveLoop;

    this.Text = "ANOREKSIK SUITE V9.0 - ULTIMATE ENTERPRISE EDITION";
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

    this.Controls.Add(tabs);

    trayIcon = new NotifyIcon();
    try { trayIcon.Icon = this.Icon; } catch { }
    trayIcon.Text = "Anoreksik Suite V9.0";
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

private void CoreLoop(object s, EventArgs e) {
    if (isShuttingDown) return;

    // RAM modülü kapalıysa hiçbir şey yapma
    bool ramEnabled = false;
    SafeInvokeSync(() => { if (chkEnableRam != null) ramEnabled = chkEnableRam.Checked; });
    if (!ramEnabled) return;

    float currentCpu = 0;
    try { if (cpuCounter != null) currentCpu = cpuCounter.NextValue(); } catch { }

    MEMORYSTATUSEX mem = new MEMORYSTATUSEX();
    mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
    if (GlobalMemoryStatusEx(ref mem)) {
        long freeMB = (long)(mem.ullAvailPhys / (1024*1024));

        // UI güncelleme - sadece görünürse
        if (! isHidden && this.Visible) {
            SafeInvoke(() => {
                if (lblRamBig != null && ! lblRamBig.IsDisposed) {
                    lblRamBig.Text = string.Format("{0} MB BOS", freeMB);
                    lblRamBig.ForeColor = freeMB < cfgRamTriggerMB ? Color.Orange : Color.Lime;
                }
            });
        }

        // Otomatik RAM Temizlik Ayarlarını Oku
        bool autoRam = false, autoStandby = false, pageFaultProtect = false;
        SafeInvokeSync(() => {
            if (chkRamAuto != null) autoRam = chkRamAuto.Checked;
            if (chkStandbyAuto != null) autoStandby = chkStandbyAuto.Checked;
            if (chkPageFaultProtect != null) pageFaultProtect = chkPageFaultProtect.Checked;
        });

        // ═══════════════════════════════════════════════════════
        // OTOMATİK RAM TEMİZLİK - DÜZELTME
        // ramCleaningFlag ile kontrol (ramCleaning değil!)
        // ═══════════════════════════════════════════════════════

        // Koşul 1: Oto RAM Temizlik aktif + Boş RAM düşük + CPU müsait
        if (autoRam && freeMB < cfgRamTriggerMB && currentCpu < cfgRamCpu) {
            // Zaten temizlik yapılıyor mu kontrol et
            if (Interlocked.CompareExchange(ref ramCleaningFlag, 1, 0) == 0) {
                // Page Fault Koruma aktifse kontrol et
                if (pageFaultProtect) {
                    float pf = 0;
                    try { if (pageFaultCounter != null) pf = pageFaultCounter.NextValue(); } catch { }
                    if (pf < 50) {
                        Task.Run(() => {
                            try { NukeStandbyInternal(); }
                            finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
                        });
                    } else {
                        Interlocked.Exchange(ref ramCleaningFlag, 0); // Flag'ı sıfırla
                    }
                } else {
                    Task.Run(() => {
                        try { NukeStandbyInternal(); }
                        finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
                    });
                }
            }
        }

        // Koşul 2: Standby Oto-Temizlik aktif + Standby yüksek
        if (autoStandby && Interlocked.CompareExchange(ref ramCleaningFlag, 0, 0) == 0) {
            long standbyMB = GetStandbyListSize();
            if (standbyMB > cfgStandbyTriggerMB) {
                if (Interlocked.CompareExchange(ref ramCleaningFlag, 1, 0) == 0) {
                    if (pageFaultProtect) {
                        float pf = 0;
                        try { if (pageFaultCounter != null) pf = pageFaultCounter.NextValue(); } catch { }
                        if (pf < 50) {
                            Task.Run(() => {
                                try { NukeStandbyInternal(); }
                                finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
                            });
                        } else {
                            Interlocked.Exchange(ref ramCleaningFlag, 0);
                        }
                    } else {
                        Task.Run(() => {
                            try { NukeStandbyInternal(); }
                            finally { Interlocked.Exchange(ref ramCleaningFlag, 0); }
                        });
                    }
                }
            }
        }
    }
}


private void KeepAliveLoop(object s, EventArgs e) {
    if (isShuttingDown) return;

    bool wifiEnabled = false, keepAliveEnabled = false;
    SafeInvokeSync(() => {
        if (chkEnableWifi != null) wifiEnabled = chkEnableWifi.Checked;
        if (chkWifiKeepAlive != null) keepAliveEnabled = chkWifiKeepAlive.Checked;
    });

    if (! wifiEnabled || ! keepAliveEnabled) return;

    Task.Run(() => {
        try {
            var reply = keepAlivePing.Send(wifiTargetIP, 100, keepAliveBuffer, new PingOptions(64, true));
            if (reply != null && reply.Status == IPStatus.Success) {
                SafeInvoke(() => {
                    if (pnlKeepAliveIndicator != null && ! pnlKeepAliveIndicator.IsDisposed) {
                        pnlKeepAliveIndicator.BackColor = Color.Lime;
                    }
                });

                // C# 5 uyumlu timer
                System.Threading.Timer fadeTimer = null;
                fadeTimer = new System.Threading.Timer(_ => {
                    SafeInvoke(() => {
                        if (pnlKeepAliveIndicator != null && !pnlKeepAliveIndicator.IsDisposed) {
                            pnlKeepAliveIndicator.BackColor = Color.DarkGreen;
                        }
                    });
                    if (fadeTimer != null) fadeTimer.Dispose(); // C# 5 uyumlu
                }, null, 100, Timeout.Infinite);
            }
        } catch { }
    });
}

        private void StartWifiWatchdog() {
            if (wifiModuleActive) return;

            wifiModuleActive = true;
            wifiCts = new CancellationTokenSource();

            wifiWatchdogTask = Task.Factory.StartNew(() => WatchdogLoop(wifiCts.Token), wifiCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void StopWifiWatchdog() {
            wifiModuleActive = false;
            if (wifiCts != null) {
                wifiCts.Cancel();
                try { if (wifiWatchdogTask != null) wifiWatchdogTask.Wait(1000); } catch { }
                wifiCts.Dispose();
                wifiCts = null;
            }
        }

       // Const tanımları (Talimat 1.10)

private void WatchdogLoop(CancellationToken ct) {
    try { Thread.CurrentThread.Priority = ThreadPriority.Highest; } catch { }

    Stopwatch sw = new Stopwatch();

    using (Ping localPing = new Ping()) {
        byte[] localBuffer = new byte[32];
        PingOptions localOptions = new PingOptions(64, true);
        int lastTtl = 64;
        int lastBufSize = 32;

        // Varsayılan değerler - UI'dan okumadan çalışır
        int timeout = wifiTimeout;
        int maxFail = wifiFailureThreshold;
        int ttl = wifiTTL;
        int bufSize = wifiBufferSize;

        while (!isShuttingDown && ! ct.IsCancellationRequested) {

            if (! wifiModuleActive) {
                Thread.Sleep(IDLE_SLEEP_MS);
                continue;
            }

            bool hasError = false;
            try {

                if (isResetting) {
                    Thread.Sleep(IDLE_SLEEP_MS);
                    continue;
                }

                // Sınıf değişkenlerinden oku (SaveWifiConfig ile güncelleniyor)
                timeout = wifiTimeout;
                maxFail = wifiFailureThreshold;
                ttl = wifiTTL;
                bufSize = wifiBufferSize;

                // Buffer/TTL güncelle (sadece değiştiyse)
                if (lastTtl != ttl) {
                    localOptions.Ttl = ttl;
                    lastTtl = ttl;
                }
                if (lastBufSize != bufSize) {
                    localBuffer = new byte[bufSize];
                    lastBufSize = bufSize;
                }

                string targetIP = wifiTargetIP;

                // ═══════════════════════════════════════════════════════
                // PING - Saf hız, hiçbir bekleme yok
                // ═══════════════════════════════════════════════════════
                sw.Restart();
                PingReply reply = null;
                try {
                    reply = localPing.Send(targetIP, timeout, localBuffer, localOptions);
                }
                catch (PingException) {
                    Interlocked.Increment(ref failureCount);
                }
                catch (ObjectDisposedException) {
                    break;
                }
                catch {
                    Interlocked.Increment(ref failureCount);
                }
                finally {
                    sw.Stop();
                }

                if (reply != null && reply.Status == IPStatus.Success) {
                    Interlocked.Exchange(ref failureCount, 0);

                    // UI güncelleme - ASENKRON (BeginInvoke - BEKLEMEZ)
                    if (! isHidden && this.Visible) {
                        long pingTime = reply.RoundtripTime;
                        SafeInvoke(() => {
                            if (lblWifiStat != null && ! lblWifiStat.IsDisposed) {
                                lblWifiStat.Text = "Online";
                                lblWifiStat.ForeColor = Color.Lime;
                            }
                            if (lblWifiPing != null && ! lblWifiPing.IsDisposed) {
                                lblWifiPing.Text = "Ping: " + pingTime + " ms";
                            }
                        });
                    }

                    int waitTime = timeout - (int)sw.ElapsedMilliseconds;
                    if (waitTime > 0) Thread.Sleep(waitTime);

                } else {
                    Interlocked.Increment(ref failureCount);

                    // UI güncelleme - ASENKRON
                    if (!isHidden && this.Visible) {
                        int fc = failureCount, mf = maxFail;
                        SafeInvoke(() => {
                            if (lblWifiStat != null && !lblWifiStat.IsDisposed) {
                                lblWifiStat.Text = "HATA: " + fc + "/" + mf;
                                lblWifiStat.ForeColor = Color.Red;
                            }
                        });
                    }

                    if (failureCount >= maxFail) {
                        bool shouldReset = false;
                        lock (resetLock) {
                            if (! isResetting) {
                                isResetting = true;
                                shouldReset = true;
                                Interlocked.Exchange(ref failureCount, 0);
                            }
                        }

                        if (shouldReset) {
                            Interlocked.Increment(ref totalDisconnects);
                            int disconnectNum = totalDisconnects;

                            // UI - ASENKRON
                            SafeInvoke(() => {
                                if (lblWifiStat != null && ! lblWifiStat.IsDisposed)
                                    lblWifiStat.Text = "RESETLENIYOR...";
                            });

                            try {
                                // ═══════════════════════════════════════════════════════
                                // AUTOCONFIG - userDisabledAutoConfig flag'ına göre
                                // ═══════════════════════════════════════════════════════

                                if (userDisabledAutoConfig) {
                                    RunFastNetsh("wlan set autoconfig enabled=yes interface=\"" + activeAdapter + "\"");
                                    Thread.Sleep(AUTOCONFIG_WAIT_MS);
                                }

                                // Disconnect
                                try { NativeWifiDisconnect(). Wait(500); } catch { }
                                Thread.Sleep(DISCONNECT_WAIT_MS);

                                // Connect
                                bool connectSuccess = false;
                                try {
                                    var task = NativeWifiConnect();
                                    task.Wait(1000);
                                    connectSuccess = task.IsCompleted && task.Result;
                                } catch { }

                                // Fallback
                                if (!connectSuccess) {
                                    RunFastNetsh("wlan connect name=\"" + currentSSID + "\"");
                                    Thread.Sleep(FALLBACK_WAIT_MS);
                                }

                                // AutoConfig geri kapat
                                if (userDisabledAutoConfig) {
                                    Thread.Sleep(AUTOCONFIG_OFF_WAIT_MS);
                                    RunFastNetsh("wlan set autoconfig enabled=no interface=\"" + activeAdapter + "\"");
                                }

                                // Stabilizasyon
                                for (int waitStep = 0; waitStep < RESET_WAIT_STEPS && !isShuttingDown && !ct.IsCancellationRequested; waitStep++) {
                                    try {
                                        var check = localPing.Send(targetIP, POST_RESET_PING_TIMEOUT, localBuffer, localOptions);
                                        if (check != null && check.Status == IPStatus.Success) break;
                                    } catch { }
                                    Thread.Sleep(STABILIZATION_SLEEP_MS);
                                }

if (cachedLoggingEnabled) {
                                    int dn = disconnectNum;
                                    Task.Run(() => {
                                        try {
                                            AddWifiLog("KOPMA #" + dn + " - RESET TAMAMLANDI");
                                        } catch { }
                                    });
                                }

                            } finally {
                                lock (resetLock) { isResetting = false; }
                            }
                        }
                    }
                }
            } catch {
                hasError = true;
            }

            if (hasError) Thread.Sleep(ERROR_SLEEP_MS);
        }
    }
}


        private Task<bool> NativeWifiDisconnect() {
            return Task.Factory.StartNew(() => {
                bool success = false;
                wlanSem.Wait();
                try {
                    uint ver; IntPtr handle = IntPtr.Zero; IntPtr ifacePtr = IntPtr.Zero;
                    try {
                        if(WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0) {
                            if(WlanEnumInterfaces(handle, IntPtr.Zero, out ifacePtr) == 0) {
                                WLAN_INTERFACE_INFO_LIST list = (WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(ifacePtr, typeof(WLAN_INTERFACE_INFO_LIST));
                                if(list.dwNumberOfItems > 0) {
                                    WLAN_INTERFACE_INFO info = (WLAN_INTERFACE_INFO)Marshal.PtrToStructure(new IntPtr(ifacePtr.ToInt64() + 8), typeof(WLAN_INTERFACE_INFO));
                                    WlanDisconnect(handle, ref info.InterfaceGuid, IntPtr.Zero);
                                    success = true;
                                }
                            }
                        }
                    } finally {
                        if (ifacePtr != IntPtr.Zero) WlanFreeMemory(ifacePtr);
                        if (handle != IntPtr.Zero) WlanCloseHandle(handle, IntPtr.Zero);
                    }
                } catch { } finally { wlanSem.Release(); }
                return success;
            });
        }

private Task<bool> NativeWifiConnect() {
    return Task.Factory.StartNew(() => {
        bool success = false;
        wlanSem.Wait();
        IntPtr profilePtr = IntPtr.Zero;
        try {
            uint ver; IntPtr handle = IntPtr.Zero;
            try {
                if(WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0 && activeGuid != Guid.Empty) {
                    profilePtr = Marshal.StringToHGlobalUni(currentSSID);

                    WLAN_CONNECTION_PARAMETERS param = new WLAN_CONNECTION_PARAMETERS();
                    param.wlanConnectionMode = 0; // wlan_connection_mode_profile
                    param.strProfile = profilePtr;
                    param.pDot11Ssid = IntPtr.Zero;
                    param.pDesiredBssidList = IntPtr.Zero;
                    param.dot11BssType = 1; // dot11_BSS_type_infrastructure
                    param.dwFlags = 0;

                    Guid tempGuid = activeGuid;
                    uint result = WlanConnect(handle, ref tempGuid, ref param, IntPtr.Zero);
                    success = (result == 0);
                }
            } finally {
                if (profilePtr != IntPtr.Zero) Marshal.FreeHGlobal(profilePtr);
                if (handle != IntPtr.Zero) WlanCloseHandle(handle, IntPtr.Zero);
            }
        } catch (Exception ex) {
            LogError("NativeWifiConnect", ex);
        } finally {
            wlanSem.Release();
        }
        return success;
    });
}

        private Guid GetAdapterGuid(string name) {
            try {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (ni.Name == name) return Guid.Parse(ni.Id);
                }
            } catch { }
            return Guid.Empty;
        }

        private string GetActiveAdapter() {
            try {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (ni.OperationalStatus == OperationalStatus.Up && (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)) return ni.Name;
                }
            } catch { }
            return "Wi-Fi";
        }


// ═══════════════════════════════════════════════════════
// GetWifiSignalStrength - DÜZELTILMIŞ VERSİYON
// ═══════════════════════════════════════════════════════
private string GetWifiSignalStrength() {
    Process p = null;
    try {
        ProcessStartInfo psi = new ProcessStartInfo {
            FileName = "netsh.exe",
            Arguments = "wlan show interfaces",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        p = new Process();
        p.StartInfo = psi;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        bool exited = p.WaitForExit(2000);

        if (!exited && !p.HasExited) {
            try { p.Kill(); } catch { }
        }

        if (string.IsNullOrEmpty(output)) return null;

        // Regex ile sinyal bul (İngilizce ve Türkçe)
        Match match = Regex.Match(output, @"Signal\s*:\s*(\d+)%", RegexOptions.IgnoreCase);
        if (! match.Success) {
            match = Regex.Match(output, @"Sinyal\s*:\s*(\d+)%", RegexOptions.IgnoreCase);
        }

        if (match.Success) {
            return match.Groups[1]. Value;
        }
    }
    catch { }
    finally {
        if (p != null) {
            try { if (!p.HasExited) p.Kill(); } catch { }
            try { p.Dispose(); } catch { }
        }
    }

    return null;
}

// ═══════════════════════════════════════════════════════
// GetWifiSpeed - DÜZELTILMIŞ VERSİYON
// ═══════════════════════════════════════════════════════
private string GetWifiSpeed() {
    Process p = null;
    try {
        ProcessStartInfo psi = new ProcessStartInfo {
            FileName = "netsh.exe",
            Arguments = "wlan show interfaces",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        p = new Process();
        p.StartInfo = psi;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        bool exited = p.WaitForExit(2000);

        if (! exited && !p.HasExited) {
            try { p.Kill(); } catch { }
        }

        if (string.IsNullOrEmpty(output)) return null;

        // Receive rate (İngilizce)
        Match match = Regex.Match(output, @"Receive rate.*?:\s*(\d+)", RegexOptions.IgnoreCase);
        if (! match.Success) {
            // Alma hızı (Türkçe)
            match = Regex.Match(output, @"Alma h[ıi]z[ıi].*?:\s*(\d+)", RegexOptions.IgnoreCase);
        }

        if (match.Success) {
            return match.Groups[1].Value;
        }
    }
    catch { }
    finally {
        if (p != null) {
            try { if (!p.HasExited) p.Kill(); } catch { }
            try { p.Dispose(); } catch { }
        }
    }

    return null;
}




private void RunFastNetsh(string args) {
    try {
        ProcessStartInfo psi = new ProcessStartInfo {
            FileName = "netsh.exe",
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using (Process p = Process.Start(psi)) {
            if (p != null) p.WaitForExit(2000);
        }
    } catch { }
}

private void AddWifiLog(string msg) {
    if (! cachedLoggingEnabled) return;

    string logMsg = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, msg);

    // Buffer limit
    while (wifiLogBuffer.Count >= 250) {
        string temp;
        wifiLogBuffer.TryDequeue(out temp);
    }
    wifiLogBuffer.Enqueue(logMsg);

    if (! isHidden && this.Visible) {
        SafeInvoke(() => {
            if (lstWifiLog != null && ! lstWifiLog.IsDisposed) {
                lstWifiLog.Items.Insert(0, logMsg);
                if (lstWifiLog.Items.Count > 250) {
                    lstWifiLog.Items.RemoveAt(lstWifiLog.Items.Count - 1);
                }
            }
        });
    }
}

private void FlushWifiLogs() {
    if (wifiLogBuffer.IsEmpty) return;
    SafeInvoke(() => {
        if (lstWifiLog == null || lstWifiLog.IsDisposed) return;

        string[] snapshot = wifiLogBuffer.ToArray();
        Array.Reverse(snapshot);

        lstWifiLog.BeginUpdate();
        try {
            lstWifiLog.Items.Clear();
            lstWifiLog.Items.AddRange(snapshot);
        }
        finally {
            lstWifiLog.EndUpdate();
        }
    });
}


private long GetStandbyListSize() {
    try {
        if (standbyCounter != null) {
            return (long)(standbyCounter.NextValue() / (1024 * 1024));
        }
    } catch { }
    return 0;
}

// Sınıf seviyesinde

/// <summary>
/// Standby List'i temizler ve RAM'i agresif şekilde boşaltır
/// Admin yetkisi gereklidir, aksi halde STATUS_ACCESS_DENIED (0xC0000022) döner
/// </summary>
private void NukeStandbyInternal() {
    GCHandle h = default(GCHandle);
    try {
        int command = MemoryPurgeStandbyList;
        h = GCHandle.Alloc(command, GCHandleType.Pinned);

        int ntStatus = NtSetSystemInformation(
            SystemMemoryListInformation,
            h.AddrOfPinnedObject(),
            Marshal.SizeOf(typeof(int)));

        Thread.Sleep(50);

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

        try {
            SetProcessWorkingSetSize(
                Process.GetCurrentProcess(). Handle,
                new IntPtr(-1),
                new IntPtr(-1)
            );
        } catch { }

    } catch (Exception ex) {
        LogError("NukeStandbyInternal", ex);
    } finally {
        if (h.IsAllocated) h.Free();
        Thread.Sleep(500); // Ardışık tetikleme önleme
    }
}

private void NukeStandby() {
    if (Interlocked.CompareExchange(ref ramCleaningFlag, 1, 0) != 0) return;

    try {
        NukeStandbyInternal();
    } finally {
        Interlocked.Exchange(ref ramCleaningFlag, 0);
    }
}


        private void TrimSystemFileCache() {
    Task.Run(() => {
        try {
            if (! IsAdmin()) {
                SafeMessageBox("Bu islem icin yonetici yetkisi gerekli!");
                return;
            }

            if (! SetIncreasePrivilege(SE_INCREASE_QUOTA_NAME)) {
                SafeMessageBox("SeIncreaseQuotaPrivilege yetkisi alinamadi!");
                return;
            }

            // Önceki boyutu al
            long cacheBefore = 0;
            try {
                using (PerformanceCounter cacheCounter = new PerformanceCounter("Memory", "Cache Bytes", true)) {
                    cacheBefore = (long)cacheCounter.NextValue();
                }
            } catch { }

            int result;

            if (Environment.Is64BitOperatingSystem) {
                SYSTEM_CACHE_INFORMATION_64_BIT cacheInfo = new SYSTEM_CACHE_INFORMATION_64_BIT();
                cacheInfo.MinimumWorkingSet = -1;
                cacheInfo.MaximumWorkingSet = -1;

                int size = Marshal.SizeOf(cacheInfo);
                GCHandle handle = GCHandle.Alloc(cacheInfo, GCHandleType.Pinned);
                try {
                    result = NtSetSystemInformation(SystemFileCacheInformation, handle.AddrOfPinnedObject(), size);
                } finally {
                    handle.Free();
                }
            } else {
                SYSTEM_CACHE_INFORMATION cacheInfo = new SYSTEM_CACHE_INFORMATION();
                cacheInfo.MinimumWorkingSet = uint.MaxValue;
                cacheInfo.MaximumWorkingSet = uint.MaxValue;

                int size = Marshal.SizeOf(cacheInfo);
                GCHandle handle = GCHandle.Alloc(cacheInfo, GCHandleType.Pinned);
                try {
                    result = NtSetSystemInformation(SystemFileCacheInformation, handle.AddrOfPinnedObject(), size);
                } finally {
                    handle.Free();
                }
            }

            if (result != 0) {
                SafeMessageBox(string.Format("NtSetSystemInformation hatasi: 0x{0:X8}\n\nTamper Protection aktif olabilir.", result));
                return;
            }

            Thread.Sleep(200);

            // Sonraki boyutu al
            long cacheAfter = 0;
            try {
                using (PerformanceCounter cacheCounter = new PerformanceCounter("Memory", "Cache Bytes", true)) {
                    cacheAfter = (long)cacheCounter.NextValue();
                }
            } catch { }

            long freedMB = (cacheBefore - cacheAfter) / (1024 * 1024);
            if (freedMB < 0) freedMB = 0;

            // NukeStandby KALDIRILDI - File Cache ve Standby farklı şeyler!

            SafeMessageBox(string.Format("Sistem Dosya Onbellegi temizlendi!\n\nBosaltilan: {0} MB", freedMB));

        } catch (Exception ex) {
            SafeMessageBox(string.Format("Hata: {0}", ex.Message));
            LogError("TrimSystemFileCache", ex);
        }
    });
}

        private bool SetIncreasePrivilege(string privilegeName) {
            try {
                using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges)) {
                    TokPriv1Luid tokPriv1Luid;
                    tokPriv1Luid.Count = 1;
                    tokPriv1Luid.Luid = 0L;
                    tokPriv1Luid.Attr = SE_PRIVILEGE_ENABLED;

                    if (! LookupPrivilegeValue(null, privilegeName, ref tokPriv1Luid.Luid)) return false;
                    return AdjustTokenPrivileges(current.Token, false, ref tokPriv1Luid, 0, IntPtr.Zero, IntPtr.Zero);
                }
            } catch { return false; }
        }

        private void CompressApps() {
            Task.Run(() => {
                try {
                    Process currentProc = Process.GetCurrentProcess();
                    IntPtr foregroundWindow = GetForegroundWindow();
                    int foregroundPid = 0;
                    try { GetWindowThreadProcessId(foregroundWindow, out foregroundPid); } catch { }

                    int compressed = 0;
                    foreach(Process p in Process.GetProcesses()) {
                        try {
                            if (p.Id == currentProc.Id || p.Id == foregroundPid || p.Id == 4) continue;
                            EmptyWorkingSet(p.Handle);
                            compressed++;
                        } catch { }
                    }
                    SafeMessageBox(string.Format("{0} program sikistirildi (Aktif pencere korundu).", compressed));
                } catch { }
            });
        }

// ═══════════════════════════════════════════════════════
// WifiSignalLoop - DÜZELTILMIŞ VERSİYON (Timer'dan çağrılır)
// ═══════════════════════════════════════════════════════
private void WifiSignalLoop(object s, EventArgs e) {
    if (isShuttingDown) return;

    // WiFi modülü kapalıysa çık
    if (!cachedWifiEnabled) return;

    // Sinyal gösterimi kapalıysa çık
    if (! cachedSignalEnabled) return;

    // WiFi Tab aktif değilse çık
    bool wifiTabActive = false;
    SafeInvokeSync(() => {
        if (chkWifiActive != null) wifiTabActive = chkWifiActive.Checked;
    });
    if (! wifiTabActive) return;

    // Re-entrancy koruması
    if (Interlocked.CompareExchange(ref isSignalProcessing, 1, 0) == 1) return;

    Task.Run(() => {
        try {
            // Sinyal gücü al
            string signalStr = GetWifiSignalStrength();
            int signalValue = 0;
            if (! string.IsNullOrEmpty(signalStr)) {
                int.TryParse(signalStr, out signalValue);
            }

            // Hız al
            string speedStr = GetWifiSpeed();
            int speedValue = 0;
            if (!string.IsNullOrEmpty(speedStr)) {
                int.TryParse(speedStr, out speedValue);
            }

            // UI güncellemesi - HER ZAMAN yap (isHidden kontrolü kaldırıldı)
            SafeInvoke(() => {
                if (isShuttingDown) return;

                if (lblWifiSignal != null && !lblWifiSignal.IsDisposed) {
                    if (signalValue > 0) {
                        lblWifiSignal.Text = string.Format("Sinyal: %{0}", signalValue);
                        lblWifiSignal.ForeColor = signalValue > 70 ? Color.Lime : signalValue > 40 ? Color.Yellow : Color.Red;
                    } else {
                        lblWifiSignal.Text = "Sinyal: --";
                        lblWifiSignal.ForeColor = Color.Gray;
                    }
                }

                if (lblWifiSpeed != null && !lblWifiSpeed.IsDisposed) {
                    if (speedValue > 0) {
                        lblWifiSpeed.Text = string.Format("Hiz: {0} Mbps", speedValue);
                        lblWifiSpeed.ForeColor = speedValue >= 100 ? Color.Cyan : Color.LightGray;
                    } else {
                        lblWifiSpeed.Text = "Hiz: --";
                        lblWifiSpeed.ForeColor = Color.Gray;
                    }
                }
            });
        }
        catch { }
        finally {
            Interlocked.Exchange(ref isSignalProcessing, 0);
        }
    });
}


        private void LoadWifiConfig() {
            try {
                if (File.Exists(wifiConfigPath)) {
                    foreach (string line in File.ReadAllLines(wifiConfigPath)) {
                        string[] p = line.Split('=');
                        if (p.Length == 2) {
                            try {
                                if(p[0]=="Timeout") wifiTimeout = int.Parse(p[1]);
                                if(p[0]=="Threshold") wifiFailureThreshold = int.Parse(p[1]);
                                if(p[0]=="TTL") wifiTTL = int.Parse(p[1]);
                                if(p[0]=="Buffer") wifiBufferSize = int.Parse(p[1]);
                                if(p[0]=="SignalInterval") signalUpdateInterval = int.Parse(p[1]);
                            } catch { }
                        }
                    }
                }
            } catch { }
        }

private void SaveWifiConfig() {
    try {
        int timeout=1000, threshold=2, ttl=64, buffer=32, signalInt=5000;
        SafeInvokeSync(() => {
            if (numWifiTimeout != null) timeout = (int)numWifiTimeout.Value;
            if (numWifiFailureThreshold != null) threshold = (int)numWifiFailureThreshold.Value;
            if (numWifiTTL != null) ttl = (int)numWifiTTL.Value;
            if (numWifiBufferSize != null) buffer = (int)numWifiBufferSize.Value;
            if (numSignalInterval != null) signalInt = (int)numSignalInterval.Value * 1000;
        });

        string tempPath = wifiConfigPath + ".tmp";
        File.WriteAllLines(tempPath, new string[] {
            string.Format("Timeout={0}", timeout),
            string.Format("Threshold={0}", threshold),
            string.Format("TTL={0}", ttl),
            string.Format("Buffer={0}", buffer),
            string.Format("SignalInterval={0}", signalInt)
        });

        if (File.Exists(wifiConfigPath)) File.Delete(wifiConfigPath);
        File.Move(tempPath, wifiConfigPath);

        // Sınıf değişkenlerini güncelle
        wifiTimeout = timeout;
        wifiFailureThreshold = threshold;
        wifiTTL = ttl;
        wifiBufferSize = buffer;
        signalUpdateInterval = signalInt;

        // Timer interval güncelle - ÇALIŞIRKEN DE
        if (wifiSignalTimer != null) {
            bool wasEnabled = wifiSignalTimer.Enabled;
            if (wasEnabled) wifiSignalTimer.Stop();

            if (signalUpdateInterval >= 1000) {
                wifiSignalTimer.Interval = signalUpdateInterval;
            }

            if (wasEnabled) wifiSignalTimer.Start();
        }

        SafeMessageBox(string.Format("Ayarlar kaydedildi.\n\nSinyal guncelleme: {0} saniye", signalUpdateInterval / 1000));
    } catch (Exception ex) {
        SafeMessageBox(string.Format("Kaydetme hatasi: {0}", ex.Message));
    }
}

private void LoadDashboardConfig() {
    try {
        if (File.Exists(dashConfigPath)) {
            foreach (string line in File.ReadAllLines(dashConfigPath)) {
                string[] parts = line.Split('=');
                if (parts.Length == 2) {
                    string key = parts[0];
                    bool value = parts[1] == "True";

                    // Ana modüller
                    if (key == "EnableWifi" && chkEnableWifi != null) chkEnableWifi.Checked = value;
                    if (key == "EnableRam" && chkEnableRam != null) chkEnableRam.Checked = value;
                    if (key == "EnableCpu" && chkEnableCpu != null) chkEnableCpu.Checked = value;
                    if (key == "EnableNet" && chkEnableNet != null) chkEnableNet.Checked = value;
                    if (key == "EnableDisk" && chkEnableDisk != null) chkEnableDisk.Checked = value;
                    if (key == "EnablePano" && chkEnablePano != null) chkEnablePano.Checked = value;

                    // WiFi alt özellikleri
                    if (key == "WifiPingSelect" && chkWifiPingSelect != null) chkWifiPingSelect.Checked = value;
                    if (key == "WifiSignal" && chkWifiSignal != null) chkWifiSignal.Checked = value;
                    if (key == "WifiCustomInterval" && chkWifiCustomInterval != null) chkWifiCustomInterval.Checked = value;
                    if (key == "WifiTTL" && chkWifiTTL != null) chkWifiTTL.Checked = value;
                    if (key == "WifiKeepAlive" && chkWifiKeepAlive != null) chkWifiKeepAlive.Checked = value;
                    if (key == "WifiLogging" && chkWifiLogging != null) chkWifiLogging.Checked = value;

                    // Disk alt özellikleri
                    if (key == "DiskTemp" && chkDashTemp != null) chkDashTemp.Checked = value;
                    if (key == "DiskPrefetch" && chkDashPrefetch != null) chkDashPrefetch.Checked = value;
                    if (key == "DiskUpdate" && chkDashUpdate != null) chkDashUpdate.Checked = value;
                    if (key == "DiskRecycle" && chkDashRecycle != null) chkDashRecycle.Checked = value;
                    if (key == "DiskWer" && chkDashWer != null) chkDashWer.Checked = value;
                    if (key == "DiskThumbnails" && chkDashThumbnails != null) chkDashThumbnails.Checked = value;
                    if (key == "DiskDism" && chkDashDism != null) chkDashDism.Checked = value;
                    if (key == "DiskBrowser" && chkDashBrowser != null) chkDashBrowser.Checked = value;

                    // RAM alt özellikleri
                    if (key == "RamAuto" && chkDashRamAuto != null) chkDashRamAuto.Checked = value;
                    if (key == "StandbyAuto" && chkDashStandbyAuto != null) chkDashStandbyAuto.Checked = value;
                    if (key == "PageFault" && chkDashPageFault != null) chkDashPageFault.Checked = value;
                }
            }
        }
    } catch (Exception ex) { LogError("LoadDashboardConfig", ex); }

if (chkWifiLogging != null) cachedLoggingEnabled = chkWifiLogging.Checked;
if (chkEnableWifi != null) cachedWifiEnabled = chkEnableWifi.Checked;  // EKLE
if (chkWifiSignal != null) cachedSignalEnabled = chkWifiSignal.Checked;  // EKLE

}

        private void SaveDashboardConfig() {
            try {
                List<string> lines = new List<string>();
                SafeInvokeSync(() => {
                    // Ana Modüller
                    if (chkEnableWifi != null) lines.Add(string.Format("EnableWifi={0}", chkEnableWifi.Checked));
                    if (chkEnableRam != null) lines.Add(string.Format("EnableRam={0}", chkEnableRam.Checked));
                    if (chkEnableCpu != null) lines.Add(string.Format("EnableCpu={0}", chkEnableCpu.Checked));
                    if (chkEnableNet != null) lines.Add(string.Format("EnableNet={0}", chkEnableNet.Checked));
                    if (chkEnableDisk != null) lines.Add(string.Format("EnableDisk={0}", chkEnableDisk.Checked));
                    if (chkEnablePano != null) lines.Add(string.Format("EnablePano={0}", chkEnablePano.Checked));

                    // WiFi Alt Özellikleri
                    if (chkWifiPingSelect != null) lines.Add(string.Format("WifiPingSelect={0}", chkWifiPingSelect.Checked));
                    if (chkWifiSignal != null) lines.Add(string.Format("WifiSignal={0}", chkWifiSignal.Checked));
                    if (chkWifiCustomInterval != null) lines.Add(string.Format("WifiCustomInterval={0}", chkWifiCustomInterval.Checked));
                    if (chkWifiTTL != null) lines.Add(string.Format("WifiTTL={0}", chkWifiTTL.Checked));
                    if (chkWifiKeepAlive != null) lines.Add(string.Format("WifiKeepAlive={0}", chkWifiKeepAlive.Checked));
                    if (chkWifiLogging != null) lines.Add(string.Format("WifiLogging={0}", chkWifiLogging.Checked));

                    // Disk Alt Özellikleri
                    if (chkDashTemp != null) lines.Add(string.Format("DiskTemp={0}", chkDashTemp.Checked));
                    if (chkDashPrefetch != null) lines.Add(string.Format("DiskPrefetch={0}", chkDashPrefetch.Checked));
                    if (chkDashUpdate != null) lines.Add(string.Format("DiskUpdate={0}", chkDashUpdate.Checked));
                    if (chkDashRecycle != null) lines.Add(string.Format("DiskRecycle={0}", chkDashRecycle.Checked));
                    if (chkDashWer != null) lines.Add(string.Format("DiskWer={0}", chkDashWer.Checked));
                    if (chkDashThumbnails != null) lines.Add(string.Format("DiskThumbnails={0}", chkDashThumbnails.Checked));
                    if (chkDashDism != null) lines.Add(string.Format("DiskDism={0}", chkDashDism.Checked));
                    if (chkDashBrowser != null) lines.Add(string.Format("DiskBrowser={0}", chkDashBrowser.Checked));

                    // RAM Alt Özellikleri
                    if (chkDashRamAuto != null) lines.Add(string.Format("RamAuto={0}", chkDashRamAuto.Checked));
                    if (chkDashStandbyAuto != null) lines.Add(string.Format("StandbyAuto={0}", chkDashStandbyAuto.Checked));
                    if (chkDashPageFault != null) lines.Add(string.Format("PageFault={0}", chkDashPageFault.Checked));
                });

                string tempPath = dashConfigPath + ".tmp";
                File.WriteAllLines(tempPath, lines.ToArray());

                if (File.Exists(dashConfigPath)) File.Delete(dashConfigPath);
                File.Move(tempPath, dashConfigPath);
            } catch { }
        }
private void ClipboardLoop(object s, EventArgs e) {
    if (isShuttingDown) return;

    bool panoEnabled = false;
    SafeInvokeSync(() => { if (chkEnablePano != null) panoEnabled = chkEnablePano.Checked; });
    if (! panoEnabled) return;

    try {
        if (Clipboard.ContainsText()) {
            string txt = Clipboard.GetText();
            if (txt != lastClipText && !string.IsNullOrEmpty(txt)) {
                if (txt.Length < clipLimit) {
                    lastClipText = txt;
                    string entry = string.Format("--- [{0:HH:mm:ss}] ---\n{1}\n\n", DateTime.Now, txt);
                    SafeInvoke(() => {
                        if (rtbClip != null && ! rtbClip.IsDisposed)
                            rtbClip.Text = entry + rtbClip.Text;
                    });
                }
            }
        }
    } catch { }
}

private void ApplyDashboardVisibility() {
    SafeInvoke(() => {
        // === RAM TAB ===
        bool ramEnabled = chkEnableRam != null && chkEnableRam.Checked;
        if (ramEnabled) {
            if (! tabs.TabPages.Contains(ramPage)) tabs.TabPages.Add(ramPage);
            coreTimer.Start();
        } else {
            if (tabs.TabPages.Contains(ramPage)) tabs.TabPages.Remove(ramPage);
            coreTimer.Stop();
        }

        // === CPU TAB ===
        bool cpuEnabled = chkEnableCpu != null && chkEnableCpu.Checked;
        if (cpuEnabled) {
            if (!tabs.TabPages.Contains(cpuPage)) tabs.TabPages.Add(cpuPage);
        } else {
            if (tabs.TabPages.Contains(cpuPage)) tabs.TabPages.Remove(cpuPage);
        }

        // === NET TAB ===
        bool netEnabled = chkEnableNet != null && chkEnableNet.Checked;
        if (netEnabled) {
            if (!tabs.TabPages.Contains(netPage)) tabs.TabPages.Add(netPage);
        } else {
            if (tabs.TabPages.Contains(netPage)) tabs.TabPages.Remove(netPage);
        }

        // === DISK TAB ===
        bool diskEnabled = chkEnableDisk != null && chkEnableDisk.Checked;
        if (diskEnabled) {
            if (!tabs.TabPages.Contains(diskPage)) tabs.TabPages.Add(diskPage);
        } else {
            if (tabs.TabPages.Contains(diskPage)) tabs.TabPages.Remove(diskPage);
        }

        // === WIFI TAB ===
bool wifiEnabled = chkEnableWifi != null && chkEnableWifi.Checked;
if (wifiEnabled) {
    if (! tabs.TabPages.Contains(wifiPage)) tabs.TabPages.Add(wifiPage);
    StartWifiWatchdog();

    bool signalEnabled = chkWifiSignal != null && chkWifiSignal.Checked;
    bool wifiTabActive = chkWifiActive != null && chkWifiActive.Checked;

    cachedWifiEnabled = true;
    cachedSignalEnabled = signalEnabled;

    // Timer'ı SADECE WiFi Tab aktifse başlat
    if (signalEnabled && wifiTabActive) {
        if (wifiSignalTimer != null) {
            wifiSignalTimer.Stop();
            wifiSignalTimer.Interval = signalUpdateInterval;
            wifiSignalTimer.Start();
        }
    } else {
        if (wifiSignalTimer != null) wifiSignalTimer.Stop();
    }

    // Keep-Alive
    bool keepAliveEnabled = chkWifiKeepAlive != null && chkWifiKeepAlive.Checked;
    if (keepAliveEnabled) {
        if (wifiKeepAliveTimer != null && !wifiKeepAliveTimer.Enabled) {
            wifiKeepAliveTimer.Start();
        }
    } else {
        if (wifiKeepAliveTimer != null) wifiKeepAliveTimer.Stop();
    }
} else {
    cachedWifiEnabled = false;
    if (tabs.TabPages.Contains(wifiPage)) tabs.TabPages.Remove(wifiPage);
    StopWifiWatchdog();
    if (wifiSignalTimer != null) wifiSignalTimer.Stop();
    if (wifiKeepAliveTimer != null) wifiKeepAliveTimer.Stop();
}

        // === PANO TAB ===
        bool panoEnabled = chkEnablePano != null && chkEnablePano.Checked;
        if (panoEnabled) {
            if (!tabs.TabPages.Contains(panoPage)) tabs.TabPages.Add(panoPage);
            clipboardTimer.Start();
        } else {
            if (tabs.TabPages.Contains(panoPage)) tabs.TabPages.Remove(panoPage);
            clipboardTimer.Stop();
        }

        // === WIFI ALT ÖZELLİKLERİ GÖRÜNÜRLÜKLERİ ===
        if (wifiEnabled && wifiPage != null) {
            bool pingSelect = chkWifiPingSelect != null && chkWifiPingSelect.Checked;
            bool customInterval = chkWifiCustomInterval != null && chkWifiCustomInterval.Checked;
            bool ttlCustom = chkWifiTTL != null && chkWifiTTL.Checked;
            bool loggingEnabled = chkWifiLogging != null && chkWifiLogging.Checked;

            if (cmbWifiTarget != null) cmbWifiTarget.Visible = pingSelect;
            if (numWifiTimeout != null) numWifiTimeout.Visible = customInterval;
            if (numWifiTTL != null) numWifiTTL.Visible = ttlCustom;
            if (lstWifiLog != null) lstWifiLog.Visible = loggingEnabled;
        }

        // === DISK TAB - CHECKBOX SYNC ===
        if (diskEnabled && diskPage != null) {
            if (chkTemp != null && chkDashTemp != null) chkTemp.Checked = chkDashTemp.Checked;
            if (chkPrefetch != null && chkDashPrefetch != null) chkPrefetch.Checked = chkDashPrefetch.Checked;
            if (chkUpdate != null && chkDashUpdate != null) chkUpdate.Checked = chkDashUpdate.Checked;
            if (chkRecycle != null && chkDashRecycle != null) chkRecycle.Checked = chkDashRecycle.Checked;
            if (chkWer != null && chkDashWer != null) chkWer.Checked = chkDashWer.Checked;
            if (chkThumbnails != null && chkDashThumbnails != null) chkThumbnails.Checked = chkDashThumbnails.Checked;
            if (chkDism != null && chkDashDism != null) chkDism.Checked = chkDashDism.Checked;
            if (chkBrowser != null && chkDashBrowser != null) chkBrowser.Checked = chkDashBrowser.Checked;
            if (chkInstallerCache != null && chkDashInstaller != null) chkInstallerCache.Checked = chkDashInstaller.Checked;
            if (chkVSS != null && chkDashVSS != null) chkVSS.Checked = chkDashVSS.Checked;
        }

        // === RAM TAB - CHECKBOX SYNC ===
        if (ramEnabled && ramPage != null) {
            if (chkRamAuto != null && chkDashRamAuto != null) chkRamAuto.Checked = chkDashRamAuto.Checked;
            if (chkStandbyAuto != null && chkDashStandbyAuto != null) chkStandbyAuto.Checked = chkDashStandbyAuto.Checked;
            if (chkPageFaultProtect != null && chkDashPageFault != null) chkPageFaultProtect.Checked = chkDashPageFault.Checked;
        }
    });
}

        private void InitTab_Dashboard() {
    dashboardPage = new TabPage("ANASAYFA");
    dashboardPage.BackColor = Color.FromArgb(30,30,30);
    dashboardPage.Name = "DASHBOARD";
    dashboardPage.AutoScroll = true;

    lblDashInfo = CreateLabel("Kullanmak istediginiz ozellikleri aktif edin - Kapali moduller CPU/RAM kullanmaz", 10, 10, 9, dashboardPage);
    lblDashInfo.ForeColor = Color.Cyan;
    lblDashInfo.Font = new Font("Arial", 9, FontStyle.Bold);

    // === ANA MODÜLLER ===
    GroupBox gMain = CreateGroup("Ana Moduller", 10, 40, 660, 100, dashboardPage);

    chkEnableWifi = CreateCheck("WiFi Bekcisi", 20, 30, gMain);
    chkEnableWifi.Checked = true;
    chkEnableWifi.CheckedChanged += (s,e) => {
cachedWifiEnabled = chkEnableWifi.Checked;
ApplyDashboardVisibility(); SaveDashboardConfig(); };

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

    // === WIFI DETAYLI AYARLAR ===
    GroupBox gWifi = CreateGroup("WiFi Detayli Ayarlar", 10, 150, 660, 130, dashboardPage);

    // Satır 1 (Y=25)
    chkWifiPingSelect = CreateCheck("Ping Hedefi Secimi", 20, 25, gWifi);
    chkWifiPingSelect.Checked = true;
    chkWifiPingSelect.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

    chkWifiSignal = CreateCheck("Sinyal Gosterimi", 180, 25, gWifi);
    chkWifiSignal.Checked = true;
    chkWifiSignal.CheckedChanged += (s,e) => {
cachedSignalEnabled = chkWifiSignal.Checked;
        if (chkWifiSignal.Checked && chkEnableWifi.Checked) wifiSignalTimer.Start();
        else wifiSignalTimer.Stop();
        SaveDashboardConfig();
    };

    chkWifiCustomInterval = CreateCheck("Ping Araligi Ayari", 340, 25, gWifi);
    chkWifiCustomInterval.Checked = true;
    chkWifiCustomInterval.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

    chkWifiTTL = CreateCheck("TTL Ayari", 500, 25, gWifi);
    chkWifiTTL.Checked = true;
    chkWifiTTL.CheckedChanged += (s,e) => { ApplyDashboardVisibility(); SaveDashboardConfig(); };

    chkWifiKeepAlive = CreateCheck("WiFi Keep-Alive", 200, 55, gWifi);
    chkWifiKeepAlive.Checked = true;
    chkWifiKeepAlive.CheckedChanged += (s,e) => {
        if (chkWifiKeepAlive.Checked && chkEnableWifi.Checked) wifiKeepAliveTimer.Start();
        else wifiKeepAliveTimer.Stop();
        SaveDashboardConfig();
    };

chkWifiLogging = CreateCheck("WiFi Loglama", 340, 55, gWifi);
chkWifiLogging.Checked = true;
chkWifiLogging.CheckedChanged += (s,e) => {
    cachedLoggingEnabled = chkWifiLogging.Checked;  // Cache güncelle
    ApplyDashboardVisibility();
    SaveDashboardConfig();
};

    Label lblWifiNote = CreateLabel("Secili olmayan ozellikler CPU/RAM kullanmaz", 20, 105, 7, gWifi);
    lblWifiNote.ForeColor = Color.Gray;

    // === RAM DETAYLI AYARLAR ===
    GroupBox gRam = CreateGroup("RAM Detayli Ayarlar", 10, 290, 660, 80, dashboardPage);

    chkDashRamAuto = CreateCheck("Otomatik RAM Temizlik", 20, 25, gRam);
    chkDashRamAuto.Checked = true;
    chkDashRamAuto.CheckedChanged += (s,e) => {
        if (chkRamAuto != null) chkRamAuto.Checked = chkDashRamAuto.Checked;
        SaveDashboardConfig();
    };

    chkDashStandbyAuto = CreateCheck("Standby Oto-Temizlik", 200, 25, gRam);
    chkDashStandbyAuto.CheckedChanged += (s,e) => {
        if (chkStandbyAuto != null) chkStandbyAuto.Checked = chkDashStandbyAuto.Checked;
        SaveDashboardConfig();
    };

    chkDashPageFault = CreateCheck("Page Fault Koruma", 400, 25, gRam);
    chkDashPageFault.Checked = true;
    chkDashPageFault.CheckedChanged += (s,e) => {
        if (chkPageFaultProtect != null) chkPageFaultProtect.Checked = chkDashPageFault.Checked;
        SaveDashboardConfig();
    };

    Label lblRamNote = CreateLabel("Page Fault Koruma: Oyun donmalarini engeller", 20, 50, 7, gRam);
    lblRamNote.ForeColor = Color.Gray;

    // === DISK DETAYLI AYARLAR ===
    GroupBox gDisk = CreateGroup("Disk Temizlik Secenekleri", 10, 380, 660, 110, dashboardPage);

    chkDashTemp = CreateCheck("Temp Klasorleri", 20, 25, gDisk);
    chkDashTemp.Checked = true;
    chkDashTemp.CheckedChanged += (s,e) => { if (chkTemp != null) chkTemp.Checked = chkDashTemp.Checked; SaveDashboardConfig(); };

    chkDashPrefetch = CreateCheck("Prefetch", 160, 25, gDisk);
    chkDashPrefetch.Checked = true;
    chkDashPrefetch.CheckedChanged += (s,e) => { if (chkPrefetch != null) chkPrefetch.Checked = chkDashPrefetch.Checked; SaveDashboardConfig(); };

    chkDashUpdate = CreateCheck("Update Kalintilari", 280, 25, gDisk);
    chkDashUpdate.CheckedChanged += (s,e) => { if (chkUpdate != null) chkUpdate.Checked = chkDashUpdate.Checked; SaveDashboardConfig(); };

    chkDashRecycle = CreateCheck("Cop Kutusu", 430, 25, gDisk);
    chkDashRecycle.Checked = true;
    chkDashRecycle.CheckedChanged += (s,e) => { if (chkRecycle != null) chkRecycle.Checked = chkDashRecycle.Checked; SaveDashboardConfig(); };

    chkDashWer = CreateCheck("Hata Raporlari", 20, 50, gDisk);
    chkDashWer.CheckedChanged += (s,e) => { if (chkWer != null) chkWer.Checked = chkDashWer.Checked; SaveDashboardConfig(); };

    chkDashThumbnails = CreateCheck("Thumbnails", 160, 50, gDisk);
    chkDashThumbnails.CheckedChanged += (s,e) => { if (chkThumbnails != null) chkThumbnails.Checked = chkDashThumbnails.Checked; SaveDashboardConfig(); };

    chkDashDism = CreateCheck("DISM Cleanup", 280, 50, gDisk);
    chkDashDism.CheckedChanged += (s,e) => { if (chkDism != null) chkDism.Checked = chkDashDism.Checked; SaveDashboardConfig(); };

    chkDashBrowser = CreateCheck("Tarayici Cache", 430, 50, gDisk);
    chkDashBrowser.CheckedChanged += (s,e) => { if (chkBrowser != null) chkBrowser.Checked = chkDashBrowser.Checked; SaveDashboardConfig(); };

    chkDashInstaller = CreateCheck("Installer Cache", 20, 75, gDisk);
    chkDashInstaller.CheckedChanged += (s,e) => { if (chkInstallerCache != null) chkInstallerCache.Checked = chkDashInstaller.Checked; SaveDashboardConfig(); };

    chkDashVSS = CreateCheck("Shadow Copies (VSS)", 160, 75, gDisk);
    chkDashVSS.CheckedChanged += (s,e) => { if (chkVSS != null) chkVSS.Checked = chkDashVSS.Checked; SaveDashboardConfig(); };

    // === KAYDET BUTONU ===
    Button btnSaveDash = CreateBtn("KAYDET VE UYGULA", 10, 500, 660, Color.DarkGreen, dashboardPage);
    btnSaveDash.Height = 40;
    btnSaveDash.Click += (s,e) => {
        SaveDashboardConfig();
        ApplyDashboardVisibility();
        SafeMessageBox("Ayarlar kaydedildi ve uygulandi!\n\nKapali moduller artik CPU/RAM kullanmiyor. ");
    };

    Label lblInfo = CreateLabel("BILGI: Kapali moduller tamamen durdurulur ve sistem kaynaklarini KULLANMAZ.", 10, 550, 8, dashboardPage);
    lblInfo.ForeColor = Color.Yellow;

    tabs.TabPages.Add(dashboardPage);
}

        private void InitTab_RAM() {
            ramPage = new TabPage("RAM");
            ramPage.BackColor = Color.FromArgb(30,30,30);
            ramPage.Name = "RAM";

            lblRamBig = CreateLabel("--- MB BOS", 10, 10, 24, ramPage);
            lblRamBig.Size = new Size(660, 50);
            lblRamBig.TextAlign = ContentAlignment.MiddleCenter;
            lblRamBig.ForeColor = Color.Red;

            lblRamInfo = CreateLabel("RAM optimizasyonu sistem performansini arttirir", 10, 65, 8, ramPage);
            lblRamInfo.ForeColor = Color.Gray;

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
            lblPageFaultInfo = CreateLabel("Oyun donmalarini engeller - RAM temizligi sadece guvenli anlarda yapilir", 40, 200, 7, g);
            lblPageFaultInfo.ForeColor = Color.DarkGray;

            Button btnSave = CreateBtn("KAYDET", 500, 25, 140, Color.DarkGreen, g);
            btnSave.Height=30;
            btnSave.Click += (s,e) => {
                cfgRamCpu = (int)numRamCpu.Value;
                cfgRamTriggerMB = (long)numRamTrigger.Value;
                cfgStandbyTriggerMB = (long)numStandbyTrigger.Value;
                coreTimer.Interval = (int)numRamInterval.Value * 1000;
                SafeMessageBox("Ayarlar Kaydedildi!");
            };
            toolTip.SetToolTip(btnSave, "Ayarlari kaydet ve uygula");

            Button btnNuke = CreateBtn("STANDBY SIL", 10, 320, 320, Color.DarkBlue, ramPage);
            btnNuke.Click += (s,e) => Task.Run(() => NukeStandby());
            toolTip.SetToolTip(btnNuke, "Beklemede olan RAM'i aninda temizle");

            Button btnCompress = CreateBtn("SIKISTIR (Aktif Haric)", 340, 320, 330, Color.Teal, ramPage);
            btnCompress.Click += (s,e) => CompressApps();
            toolTip.SetToolTip(btnCompress, "Tum programlarin RAM kullanimini azalt (Aktif pencere haric)");

            Button btnCache = CreateBtn("FILE CACHE TEMIZLE (Native Kernel)", 10, 370, 660, Color.Purple, ramPage);
            btnCache.Click += (s,e) => TrimSystemFileCache();
            toolTip.SetToolTip(btnCache, "NtSetSystemInformation ile sistem file cache temizle - Erisim Engellendi hatasi vermez");

            Label lblCacheInfo = CreateLabel("File Cache: Native Kernel yontemi ile temizlenir.Process.GetProcessById(4) KULLANILMAZ.\nBu yontem 'Erisim Engellendi' hatasi vermez.", 10, 420, 8, ramPage);
            lblCacheInfo.ForeColor = Color.Lime;
            lblCacheInfo.Size = new Size(660, 40);

            tabs.TabPages.Add(ramPage);
        }

private void InitTab_WIFI() {
    wifiPage = new TabPage("WIFI");
    wifiPage.BackColor = Color.FromArgb(30,30,30);
    wifiPage.Name = "WIFI";

    // --- 1.Başlık ve Aktivasyon ---
    chkWifiActive = CreateCheck("Wifi Bekcisini Aktif Et", 10, 10, wifiPage);
    chkWifiActive.ForeColor = Color.Yellow;

    // === EVENT HANDLER - wifiModuleActive İLE KONTROL ===
    chkWifiActive.CheckedChanged += (s,e) => {
        if (chkWifiActive.Checked) {
            // === WATCHDOG'U BAŞLAT ===
            wifiModuleActive = true;

            currentSSID = GetSSID();
            if (lblWifiSSID != null) lblWifiSSID.Text = string.Format("SSID: {0}", currentSSID);

            // Sinyal timer'ı başlat (sadece görünürse)
            if (wifiSignalTimer != null && cachedSignalEnabled && !isHidden) {
                wifiSignalTimer.Stop();
                wifiSignalTimer.Interval = signalUpdateInterval;
                wifiSignalTimer.Start();
            }

            // İlk sinyal güncellemesi - HEMEN yap (sadece görünürse)
            if (! isHidden) {
                Task.Run(() => {
                    try {
                        string signalStr = GetWifiSignalStrength();
                        string speedStr = GetWifiSpeed();

                        int signalValue = 0;
                        int speedValue = 0;

                        if (!string.IsNullOrEmpty(signalStr)) int.TryParse(signalStr, out signalValue);
                        if (! string.IsNullOrEmpty(speedStr)) int.TryParse(speedStr, out speedValue);

                        SafeInvoke(() => {
                            if (lblWifiSignal != null && !lblWifiSignal.IsDisposed) {
                                lblWifiSignal.Text = signalValue > 0 ? string.Format("Sinyal: %{0}", signalValue) : "Sinyal: --";
                                lblWifiSignal.ForeColor = signalValue > 70 ? Color.Lime : signalValue > 40 ? Color.Yellow : Color.Red;
                            }
                            if (lblWifiSpeed != null && !lblWifiSpeed.IsDisposed) {
                                lblWifiSpeed.Text = speedValue > 0 ? string.Format("Hiz: {0} Mbps", speedValue) : "Hiz: --";
                                lblWifiSpeed.ForeColor = speedValue >= 100 ? Color.Cyan : Color.LightGray;
                            }
                        });
                    } catch { }
                });
            }

        } else {
            // === WATCHDOG'U DURDUR ===
            wifiModuleActive = false;

            // Sinyal timer'ı durdur
            if (wifiSignalTimer != null) wifiSignalTimer.Stop();

            // UI güncelle - Pasif durumu göster
            if (lblWifiStat != null) {
                lblWifiStat.Text = "Pasif";
                lblWifiStat.ForeColor = Color.Gray;
            }
            if (lblWifiPing != null) lblWifiPing.Text = "Ping: -- ms";
            if (lblWifiSignal != null) {
                lblWifiSignal.Text = "Sinyal: --";
                lblWifiSignal.ForeColor = Color.Gray;
            }
            if (lblWifiSpeed != null) {
                lblWifiSpeed.Text = "Hiz: --";
                lblWifiSpeed.ForeColor = Color.Gray;
            }
        }
    };
    toolTip.SetToolTip(chkWifiActive, "WiFi izleyiciyi aktif et - baglanti kopmalarina karsi otomatik mudahale");

    lblWifiInfo = CreateLabel("WiFi kopmalari otomatik tespit edilir ve NATIVE API ile aninda resetlenir (Adaptor kapatilmaz)", 10, 35, 8, wifiPage);
    lblWifiInfo.ForeColor = Color.Gray;
    lblWifiInfo.Size = new Size(660, 20);

    // --- 2.Hedef ve Durum Göstergeleri ---
    CreateLabel("Ping Hedefi:", 10, 60, wifiPage);
    cmbWifiTarget = new ComboBox();
    cmbWifiTarget.Items.AddRange(new string[] {"1.1.1.1", "8.8.8.8", "208.67.222.222"});
    cmbWifiTarget.SelectedIndex = 0;
    cmbWifiTarget.Location = new Point(90, 58);
    cmbWifiTarget.Size = new Size(100, 25);
    wifiPage.Controls.Add(cmbWifiTarget);
    cmbWifiTarget.SelectedIndexChanged += (s,e) => wifiTargetIP = cmbWifiTarget.SelectedItem.ToString();
    toolTip.SetToolTip(cmbWifiTarget, "Baglanti kontrolu icin ping atilacak sunucu");

    // Durum Etiketleri
    lblWifiSSID = CreateLabel("SSID: --", 210, 63, wifiPage);

    // Alt Satır (Y=90)
    lblWifiStat = CreateLabel("Durum: Kapali", 10, 90, wifiPage);
    lblWifiPing = CreateLabel("Ping: -- ms", 140, 90, wifiPage);
    lblWifiSignal = CreateLabel("Sinyal: --", 240, 90, wifiPage);
    lblWifiSpeed = CreateLabel("Hiz: --", 350, 90, wifiPage);

    // Keep-Alive Görseli
    pnlKeepAliveIndicator = new Panel();
    pnlKeepAliveIndicator.Location = new Point(480, 88);
    pnlKeepAliveIndicator.Size = new Size(16, 16);
    pnlKeepAliveIndicator.BackColor = Color.DarkGreen;
    wifiPage.Controls.Add(pnlKeepAliveIndicator);

    Label lblKeepAliveText = CreateLabel("Keep-Alive", 500, 90, 8, wifiPage);
    lblKeepAliveText.ForeColor = Color.Gray;

    // --- 3.Gelişmiş Ayarlar Grubu ---
    GroupBox gSet = CreateGroup("Gelismis Ayarlar", 10, 115, 660, 120, wifiPage);

    CreateLabel("Ping Araligi (ms):", 10, 25, gSet);
    numWifiTimeout = CreateNum(wifiTimeout, 100, 5000, 120, 22, 60, gSet);

    CreateLabel("Max Hata:", 200, 25, gSet);
    numWifiFailureThreshold = CreateNum(wifiFailureThreshold, 1, 10, 270, 22, 40, gSet);

    CreateLabel("TTL:", 330, 25, gSet);
    numWifiTTL = CreateNum(wifiTTL, 1, 255, 370, 22, 40, gSet);

    CreateLabel("Byte:", 430, 25, gSet);
    numWifiBufferSize = CreateNum(wifiBufferSize, 1, 1024, 480, 22, 50, gSet);

    CreateLabel("Sinyal Guncelleme (sn):", 10, 55, gSet);
    numSignalInterval = CreateNum(signalUpdateInterval/1000, 1, 60, 150, 52, 40, gSet);

    // KAYDET
    Button btnSaveW = CreateBtn("KAYDET", 210, 50, 100, Color.DarkGreen, gSet);
    btnSaveW.Height = 50;
    btnSaveW.Click += (s,e) => SaveWifiConfig();
    toolTip.SetToolTip(btnSaveW, "Tum WiFi ayarlarini kaydet");

    // MANUEL RESET
    Button btnResetW = CreateBtn("MANUEL RESET", 320, 50, 110, Color.DarkRed, gSet);
    btnResetW.Height = 50;
    btnResetW.Click += (s,e) => Task.Run(() => {
        // userDisabledAutoConfig flag'ını kullan (chkWifiAutoConfig yok)
        if (userDisabledAutoConfig) {
            RunFastNetsh(string.Format("wlan set autoconfig enabled=yes interface=\"{0}\"", activeAdapter));
            Thread.Sleep(50);
        }

        SafeInvoke(() => { if (lblWifiStat != null) lblWifiStat.Text = "Resetleniyor..."; });
        Task<bool> disconnectTask = NativeWifiDisconnect();
        disconnectTask.Wait(500);
        Thread.Sleep(50);

        Task<bool> connectTask = NativeWifiConnect();
        connectTask.Wait(1000);
        bool success = connectTask.Result;

        if (! success) {
            RunFastNetsh(string.Format("wlan connect name=\"{0}\"", currentSSID));
        }

        if (userDisabledAutoConfig) {
            Thread.Sleep(200);
            RunFastNetsh(string.Format("wlan set autoconfig enabled=no interface=\"{0}\"", activeAdapter));
        }

        SafeMessageBox("Hizli Reset Tamamlandi!\n(Adaptor kapatilmadi - Sadece baglanti yenilendi)");
    });
    toolTip.SetToolTip(btnResetW, "Native API ile anlik reset atar");

    // AG TARAMA AÇ
    Button btnAutoConfigOn = CreateBtn("AG TARAMA AC", 440, 50, 100, Color.DarkGreen, gSet);
    btnAutoConfigOn.Height = 23;
    btnAutoConfigOn.Click += (s,e) => {
        RunFastNetsh(string.Format("wlan set autoconfig enabled=yes interface=\"{0}\"", activeAdapter));
        userDisabledAutoConfig = false;
        SafeMessageBox("Ag Tarama ACILDI\n\nWatchdog bu ayara dokunmayacak.");
    };
    toolTip.SetToolTip(btnAutoConfigOn, "WiFi ag taramasini ac");

    // AG TARAMA KAPAT
    Button btnAutoConfigOff = CreateBtn("AG TARAMA KAPAT", 540, 50, 100, Color.Maroon, gSet);
    btnAutoConfigOff.Height = 23;
    btnAutoConfigOff.Click += (s,e) => {
        RunFastNetsh(string.Format("wlan set autoconfig enabled=no interface=\"{0}\"", activeAdapter));
        userDisabledAutoConfig = true;
        SafeMessageBox("Ag Tarama KAPATILDI\n\nWatchdog reset sirasinda gecici acar, sonra kapatir.");
    };
    toolTip.SetToolTip(btnAutoConfigOff, "WiFi ag taramasini kapat - Ping spike onlenir");

    // --- 4.Log Listesi ---
    lstWifiLog = new ListBox();
    lstWifiLog.Location = new Point(10, 245);
    lstWifiLog.Size = new Size(660, 280);
    lstWifiLog.BackColor = Color.Black;
    lstWifiLog.ForeColor = Color.Lime;
    wifiPage.Controls.Add(lstWifiLog);

    Label lblResetInfo = CreateLabel("RESET MANTIGI: Native Disconnect -> Native Connect -> (Fallback) Netsh Connect\nAdaptor KAPATILMAZ - Milisaniye seviyesinde reset atilir.", 10, 530, 8, wifiPage);
    lblResetInfo.ForeColor = Color.Cyan;
    lblResetInfo.Size = new Size(660, 30);

    tabs.TabPages.Add(wifiPage);
}

        private void EnableCTCP() {
            Task.Run(() => {
                try {
                    RunCMD("netsh int tcp set global congestionprovider=ctcp");
                    SafeMessageBox("CTCP (Compound TCP) aktif edildi!\n\nNetwork optimizasyonu icin onemli bir ayar.");
                } catch (Exception ex) {
                    SafeMessageBox(string.Format("Hata: {0}", ex.Message));
                }
            });
        }

private void DisableCTCP() {
    Task.Run(() => {
        try {
            RunCMD("netsh int tcp set global congestionprovider=default");
            SafeMessageBox("TCP Congestion Provider varsayilana donduruldu.");
        } catch (Exception ex) {
            SafeMessageBox(string.Format("Hata: {0}", ex.Message));
        }
    });
}

        private void InitTab_CPU() {
            cpuPage = new TabPage("CPU");
            cpuPage.BackColor = Color.FromArgb(30,30,30);
            cpuPage.Name = "CPU";

            lblCpuInfo = CreateLabel("CPU onceliklendirme programlarin hizini arttirir", 10, 10, 8, cpuPage);
            lblCpuInfo.ForeColor = Color.Gray;

            GroupBox gPrio = CreateGroup("Oncelik Yonetimi", 10, 30, 660, 140, cpuPage);
            CreateLabel("Uygulama:", 15, 25, gPrio);
            cmbProcList = new ComboBox();
            cmbProcList.Location = new Point(15, 45);
            cmbProcList.Size = new Size(300, 25);
            gPrio.Controls.Add(cmbProcList);
            Button btnRef = CreateBtn("Yenile", 325, 44, 200, Color.Gray, gPrio);
            btnRef.Height = 23;
            btnRef.Click += (s,e) => RefreshProcs();
            toolTip.SetToolTip(btnRef, "Program listesini yenile");

            CreateLabel("CPU Oncelik:", 15, 75, gPrio);
            cmbPriority = new ComboBox();
            cmbPriority.Items.AddRange(new string[] {"High", "Normal", "Low"});
            cmbPriority.SelectedIndex = 1;
            cmbPriority.Location = new Point(15, 95);
            cmbPriority.Size = new Size(100, 25);
            gPrio.Controls.Add(cmbPriority);

            CreateLabel("Disk Oncelik:", 135, 75, gPrio);
            cmbIoPriority = new ComboBox();
            cmbIoPriority.Items.AddRange(new string[] {"Very High", "High", "Normal", "Low", "Very Low"});
            cmbIoPriority.SelectedIndex = 2;
            cmbIoPriority.Location = new Point(135, 95);
            cmbIoPriority.Size = new Size(100, 25);
            gPrio.Controls.Add(cmbIoPriority);

            Button btnBoost = CreateBtn("UYGULA", 255, 90, 270, Color.DarkRed, gPrio);
            btnBoost.Height=30;
            btnBoost.Click += (s,e) => BoostProc();
            toolTip.SetToolTip(btnBoost, "Secili programa oncelik ata");

            CreateLabel("Aktif Onceliklendirmeler:", 15, 180, cpuPage);
            lstBoosted = new ListBox();
            lstBoosted.Location = new Point(15, 200);
            lstBoosted.Size = new Size(645, 100);
            lstBoosted.BackColor = Color.Black;
            lstBoosted.ForeColor = Color.Lime;
            cpuPage.Controls.Add(lstBoosted);
            Button btnRevert = CreateBtn("NORMALE CEVIR", 15, 310, 645, Color.DimGray, cpuPage);
            btnRevert.Click += (s,e) => RevertProc();
            toolTip.SetToolTip(btnRevert, "Secili programi normale dondur");

            GroupBox gAff = CreateGroup("CPU Affinity (Cekirdek Atama)", 10, 355, 660, 140, cpuPage);
            CreateLabel("Uygulama:", 15, 25, gAff);
            cmbProcListAffinity = new ComboBox();
            cmbProcListAffinity.Location = new Point(15, 45);
            cmbProcListAffinity.Size = new Size(300, 25);
            gAff.Controls.Add(cmbProcListAffinity);
            Button btnRefAff = CreateBtn("Yenile", 325, 44, 200, Color.Gray, gAff);
            btnRefAff.Height = 23;
            btnRefAff.Click += (s,e) => RefreshProcs();

            Button btnAff = CreateBtn("SON 2 CEKIRDEGE AT", 15, 75, 265, Color.DarkOrange, gAff);
            btnAff.Click += (s,e) => SetAffinity();
            toolTip.SetToolTip(btnAff, "Arka plan programlarini son 2 cekirdege at - oyun icin cekirdek ayir");

            Button btnRevertAff = CreateBtn("NORMALE CEVIR", 290, 75, 255, Color.DimGray, gAff);
            btnRevertAff.Click += (s,e) => RevertAffinity();
            toolTip.SetToolTip(btnRevertAff, "Secili programin cekirdek atamasini normale dondur");

            CreateLabel("Atananlar:", 15, 505, cpuPage);
            lstAffinitySet = new ListBox();
            lstAffinitySet.Location = new Point(15, 525);
            lstAffinitySet.Size = new Size(645, 80);
            lstAffinitySet.BackColor = Color.Black;
            lstAffinitySet.ForeColor = Color.Cyan;
            cpuPage.Controls.Add(lstAffinitySet);

GroupBox gQuantum = CreateGroup("CPU Quantum (Zaman Dilimi)", 10, 615, 660, 80, cpuPage);

            Label lblQuantumDesc = CreateLabel("Islemcinin programlara zaman dilimi dagitim seklini optimize eder", 15, 25, 8, gQuantum);
            lblQuantumDesc.ForeColor = Color.Gray;

            Button btnQuantum = CreateBtn("OYUN MODU", 15, 45, 200, Color.DarkMagenta, gQuantum);
            btnQuantum.Height = 25;
            btnQuantum.Click += (s,e) => OptimizeCpuQuantum();
            toolTip.SetToolTip(btnQuantum, "Islemci dilimlemesini oyunlar icin optimize et");

            Button btnQuantumNormal = CreateBtn("NORMAL", 225, 45, 200, Color.Maroon, gQuantum);
            btnQuantumNormal.Height = 25;
            btnQuantumNormal.Click += (s,e) => ResetCpuQuantum();
            toolTip.SetToolTip(btnQuantumNormal, "CPU Quantum ayarini normale dondur");

            tabs.TabPages.Add(cpuPage);
        }

        private void InitTab_NET() {
            netPage = new TabPage("NET");
            netPage.BackColor = Color.FromArgb(30,30,30);
            netPage.Name = "NET";
            netPage.AutoScroll = true;

            lblNetInfo = CreateLabel("Ag optimizasyonu ping ve baglanti hizini arttirir", 10, 10, 8, netPage);
            lblNetInfo.ForeColor = Color.Gray;

            GroupBox gAdapt = CreateGroup("Adaptor Secimi", 10, 30, 660, 80, netPage);
            CreateLabel("Aktif Adaptor:", 15, 30, gAdapt);
            cmbNetAdapter = new ComboBox();
            cmbNetAdapter.Location = new Point(110, 28);
            cmbNetAdapter.Size = new Size(300, 25);
            gAdapt.Controls.Add(cmbNetAdapter);
            Button btnRefAdapter = CreateBtn("YENILE", 420, 27, 220, Color.Gray, gAdapt);
            btnRefAdapter.Height=23;
            btnRefAdapter.Click += (s,e) => {
                cmbNetAdapter.Items.Clear();
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (ni.OperationalStatus == OperationalStatus.Up) cmbNetAdapter.Items.Add(ni.Name);
                }
                if (cmbNetAdapter.Items.Count > 0) cmbNetAdapter.SelectedIndex = 0;
            };
            cmbNetAdapter.SelectedIndexChanged += (s,e) => {
                if (cmbNetAdapter.SelectedItem != null) {
                    activeAdapter = cmbNetAdapter.SelectedItem.ToString();
                    activeGuid = GetAdapterGuid(activeAdapter);
                }
            };
            btnRefAdapter.PerformClick();
            toolTip.SetToolTip(btnRefAdapter, "Kullanilabilir ag adaptorlerini listele");

            GroupBox gDns = CreateGroup("DNS Switcher", 10, 120, 660, 110, netPage);
            CreateLabel("Servis:", 15, 30, gDns);
            cmbDnsProvider = new ComboBox();
            cmbDnsProvider.Items.AddRange(new string[] {"Cloudflare (1.1.1.1)", "Google (8.8.8.8)", "AdGuard (94.140.14.14)", "Quad9 (9.9.9.9)", "Control D (76.76.2.0)", "Otomatik (DHCP)"});
            cmbDnsProvider.SelectedIndex = 0;
            cmbDnsProvider.Location = new Point(15, 50);
            cmbDnsProvider.Size = new Size(200, 25);
            gDns.Controls.Add(cmbDnsProvider);

            Button btnSetDns = CreateBtn("DNS AYARLA", 230, 45, 410, Color.DarkGreen, gDns);
            btnSetDns.Height=35;
            btnSetDns.Click += (s,e) => {
                string sel = cmbDnsProvider.SelectedItem != null ? cmbDnsProvider.SelectedItem.ToString() : "";
                if (sel.Contains("Cloudflare")) SetDNS("1.1.1.1", "1.0.0.1");
                else if (sel.Contains("Google")) SetDNS("8.8.8.8", "8.8.4.4");
                else if (sel.Contains("AdGuard")) SetDNS("94.140.14.14", "94.140.15.15");
                else if (sel.Contains("Quad9")) SetDNS("9.9.9.9", "149.112.112.112");
                else if (sel.Contains("Control")) SetDNS("76.76.2.0", "76.76.10.0");
                else if (sel.Contains("Otomatik")) SetDNSAuto();
            };
            toolTip.SetToolTip(btnSetDns, "Secili DNS sunucusunu ayarla");

            GroupBox gTcp = CreateGroup("TCP Nagle Optimizasyon", 10, 240, 660, 115, netPage);
            lblTcpInfo = CreateLabel("Nagle Algoritmasini kapatir - kucuk paketler hemen gonderilir (FPS oyunlar icin)", 15, 25, 8, gTcp);
            lblTcpInfo.ForeColor = Color.Gray;
            lblTcpInfo.Size = new Size(630, 30);

            Button btnTcpOn = CreateBtn("OYUN MODU AC", 15, 60, 310, Color.DarkGreen, gTcp);
            btnTcpOn.Click += (s,e) => SetTcp(1);
            toolTip.SetToolTip(btnTcpOn, "TCP Nagle algoritmasini kapat - daha hizli paket gonderimi");

            Button btnTcpOff = CreateBtn("NORMALE CEVIR", 335, 60, 305, Color.Maroon, gTcp);
            btnTcpOff.Click += (s,e) => SetTcp(0);
            toolTip.SetToolTip(btnTcpOff, "TCP ayarlarini varsayilana dondur");

GroupBox gCtcp = CreateGroup("CTCP (Compound TCP) Optimizasyon", 10, 365, 660, 115, netPage);
Label lblCtcpInfo = CreateLabel("Compound TCP - Yuksek bant genislikli baglantilarda performans artisi", 15, 25, 8, gCtcp);
lblCtcpInfo.ForeColor = Color.Gray;
lblCtcpInfo.Size = new Size(630, 30);

Button btnCtcpOn = CreateBtn("CTCP AKTIF ET", 15, 60, 310, Color.DarkCyan, gCtcp);
btnCtcpOn.Click += (s,e) => EnableCTCP();
toolTip.SetToolTip(btnCtcpOn, "Compound TCP congestion provider aktif et");

Button btnCtcpOff = CreateBtn("CTCP KAPAT", 335, 60, 305, Color.Maroon, gCtcp);
btnCtcpOff.Click += (s,e) => DisableCTCP();
toolTip.SetToolTip(btnCtcpOff, "TCP congestion provider'i varsayilana dondur");

// MSMQ (Y=490)
GroupBox gMsmq = CreateGroup("MSMQ TCP Gecikmesi", 10, 490, 660, 115, netPage);
lblMsmqInfo = CreateLabel("Sistem geneli (MSMQ) Nagle devre disi - tum uygulamalarda dusuk gecikme", 15, 25, 8, gMsmq);
lblMsmqInfo.ForeColor = Color.Gray;
lblMsmqInfo.Size = new Size(630, 30);

Button btnMsmqOn = CreateBtn("MSMQ NO-DELAY AC", 15, 60, 310, Color.DarkMagenta, gMsmq);
btnMsmqOn.Click += (s,e) => ForceTcpNoDelayMSMQ(true);
toolTip.SetToolTip(btnMsmqOn, "Sistem seviyesinde TCP gecikmesini kapat");

Button btnMsmqOff = CreateBtn("NORMALE CEVIR", 335, 60, 305, Color.Maroon, gMsmq);
btnMsmqOff.Click += (s,e) => ForceTcpNoDelayMSMQ(false);
toolTip.SetToolTip(btnMsmqOff, "MSMQ ayarlarini normale dondur");

// DSCP (Y=615)
GroupBox gDscp = CreateGroup("DSCP QoS Onceliklendirme", 10, 615, 660, 115, netPage);
lblDscpInfo = CreateLabel("Oyun paketlerine DSCP-46 damgasi vurur - modem sirada en one alir", 15, 25, 8, gDscp);
lblDscpInfo.ForeColor = Color.Gray;
lblDscpInfo.Size = new Size(630, 30);

Button btnDscpOn = CreateBtn("DSCP-46 AKTIF ET", 15, 60, 310, Color.DarkCyan, gDscp);
btnDscpOn.Click += (s,e) => EnableDSCP();
toolTip.SetToolTip(btnDscpOn, "QoS DSCP onceliklendirme aktif et");

Button btnDscpOff = CreateBtn("DSCP KALDIR", 335, 60, 305, Color.Maroon, gDscp);
btnDscpOff.Click += (s,e) => DisableDSCP();
toolTip.SetToolTip(btnDscpOff, "QoS DSCP politikasini kaldir");

// GELISMIS (Y=740)
GroupBox gAdv = CreateGroup("Gelismis", 10, 740, 660, 160, netPage);
Button btnFlush = CreateBtn("NETWORK FLUSH", 15, 25, 200, Color.DarkOrange, gAdv);
btnFlush.Click += (s,e) => {
    Task.Run(() => {
        SafeInvoke(() => { if (lblNetInfo != null) lblNetInfo.Text = "Network Flush yapiliyor..."; });

        // Event Log temizleme
        RunCMD("wevtutil cl \"Microsoft-Windows-WLAN-AutoConfig/Operational\"");
        RunCMD("wevtutil cl System");
        RunCMD("wevtutil cl Application");

        // IP yenileme
        RunCMD("ipconfig /release");
        Thread.Sleep(500);
        RunCMD("ipconfig /renew");
        Thread.Sleep(500);

        // DNS ve ARP temizleme
        RunCMD("ipconfig /flushdns");
        RunCMD("arp -d *");
        RunCMD("netsh winsock reset");

        SafeInvoke(() => { if (lblNetInfo != null) lblNetInfo.Text = "Ag optimizasyonu ping ve baglanti hizini arttirir"; });
        SafeMessageBox("Network Flush tamamlandi!\n\n- Event Logs temizlendi\n- IP yenilendi\n- DNS temizlendi\n- ARP temizlendi\n- Winsock sifirlandi");
    });
};
toolTip.SetToolTip(btnFlush, "Event Logs, IP, DNS, Winsock ve ARP temizle");
toolTip.SetToolTip(btnFlush, "DNS, Winsock ve ARP onbelleklerini temizle");

Button btnNic = CreateBtn("NIC INTERRUPT KAPAT", 15, 70, 200, Color.DarkSlateGray, gAdv);
btnNic.Click += (s,e) => DisableNicInterruptModeration();
toolTip.SetToolTip(btnNic, "Ag karti paket biriktirmeyi kapat - daha dusuk gecikme");

Button btnNicNormal = CreateBtn("NIC NORMAL", 225, 70, 200, Color.Maroon, gAdv);
btnNicNormal.Click += (s,e) => EnableNicInterruptModeration();
toolTip.SetToolTip(btnNicNormal, "NIC Interrupt ayarini normale dondur");

Button btnArp = CreateBtn("ARP LOCK", 435, 70, 100, Color.DarkSlateBlue, gAdv);
btnArp.Click += (s,e) => LockArpCache();
toolTip.SetToolTip(btnArp, "Gateway MAC adresini kilitle - daha stabil baglanti");

Button btnArpUnlock = CreateBtn("UNLOCK", 540, 70, 100, Color.DarkSlateBlue, gAdv);
btnArpUnlock.Click += (s,e) => UnlockArpCache();
toolTip.SetToolTip(btnArpUnlock, "ARP kilidini kaldir");

// BILGILENDIRME (Y=910)
GroupBox gInfo = CreateGroup("Bilgilendirme", 10, 910, 660, 130, netPage);
lblQuantumInfo = CreateLabel("CPU Quantum: Islemcinin programlara zaman dilimi dagitim sekli", 15, 25, 7, gInfo);
lblQuantumInfo.ForeColor = Color.LightGray;
lblNicInfo = CreateLabel("NIC Interrupt Moderation: Ag kartinin paketleri toplu islemesi - kapatilinca her paket aninda islenir", 15, 45, 7, gInfo);
lblNicInfo.ForeColor = Color.LightGray;
lblNicInfo.Size = new Size(630, 30);
lblArpInfo = CreateLabel("ARP Lock: Gateway'in MAC adresini sabitleyerek ARP spoofing saldirilarini engeller ve baglanti daha stabil olur", 15, 75, 7, gInfo);
lblArpInfo.ForeColor = Color.LightGray;
lblArpInfo.Size = new Size(630, 30);

tabs.TabPages.Add(netPage);
        }

        private void InitTab_DISK() {
            diskPage = new TabPage("DISK");
            diskPage.BackColor = Color.FromArgb(30,30,30);
            diskPage.Name = "DISK";
            diskPage.AutoScroll = true;

            lblDiskInfo = CreateLabel("Disk temizligi GB'larca alan acabilir", 10, 10, 8, diskPage);
            lblDiskInfo.ForeColor = Color.Gray;

            GroupBox gClean = CreateGroup("Deep Clean", 10, 30, 660, 430, diskPage);

            chkTemp = CreateCheck("Temp Klasorleri", 20, 30, gClean);
            chkTemp.Checked=true;
            chkPrefetch = CreateCheck("Prefetch", 20, 55, gClean);
            chkPrefetch.Checked=true;
            chkUpdate = CreateCheck("Update Kalintilari", 20, 80, gClean);
            chkRecycle = CreateCheck("Cop Kutusu (Native)", 20, 105, gClean);
            chkRecycle.Checked=true;
            chkWer = CreateCheck("Hata Raporlari (WER)", 20, 130, gClean);
            chkThumbnails = CreateCheck("Thumbnails", 20, 155, gClean);
            chkDism = CreateCheck("DISM Cleanup", 20, 180, gClean);
            chkBrowser = CreateCheck("Tarayici Cache'leri", 330, 30, gClean);
            chkInstallerCache = CreateCheck("Installer $PatchCache$", 330, 55, gClean);
            chkVSS = CreateCheck("VSS (Shadow Copies)", 330, 80, gClean);

            CreateLabel("Tarayici Sec:", 330, 110, gClean);
            cmbBrowserList = new ComboBox();
            cmbBrowserList.Location = new Point(330, 130);
            cmbBrowserList.Size = new Size(280, 25);
            cmbBrowserList.DropDownStyle = ComboBoxStyle.DropDownList;
            gClean.Controls.Add(cmbBrowserList);

            Button btnDetectBrowsers = CreateBtn("TARAYICI TARA", 330, 160, 280, Color.Gray, gClean);
            btnDetectBrowsers.Height = 25;
            btnDetectBrowsers.Click += (s,e) => DetectInstalledBrowsers();
            toolTip.SetToolTip(btnDetectBrowsers, "Yuklu tarayicilari tespit et");

            CreateLabel("D3DSCache, Event Logs da temizlenir.", 20, 210, 8, gClean);

            lblDiskGain = new Label();
            lblDiskGain.Text="Tahmini: -- MB";
            lblDiskGain.AutoSize=true;
            lblDiskGain.Location=new Point(20, 240);
            lblDiskGain.ForeColor=Color.Cyan;
            lblDiskGain.Font=new Font("Arial", 11, FontStyle.Bold);
            gClean.Controls.Add(lblDiskGain);
            lblDiskStatus = new Label();
            lblDiskStatus.Text="Durum: Bekliyor";
            lblDiskStatus.AutoSize=true;
            lblDiskStatus.Location=new Point(20, 260);
            lblDiskStatus.ForeColor=Color.Gray;
            gClean.Controls.Add(lblDiskStatus);

            prgDisk = new ProgressBar();
            prgDisk.Location = new Point(20, 285);
            prgDisk.Size = new Size(620, 20);
            gClean.Controls.Add(prgDisk);

            Button btnScan = CreateBtn("TARA", 20, 315, 195, Color.Gray, gClean);
            btnScan.Height=30;
            btnScan.Click += (s,e) => Task.Run(() => ScanDisk());
            toolTip.SetToolTip(btnScan, "Temizlenebilir alan miktarini hesapla");

            Button btnDeep = CreateBtn("TEMIZLE", 225, 315, 195, Color.DarkRed, gClean);
            btnDeep.Height=30;
            btnDeep.Click += (s,e) => Task.Run(() => DeepClean());
            toolTip.SetToolTip(btnDeep, "Secili ogeleri temizle");

            Button btnExplorer = CreateBtn("EXPLORER HIST", 430, 315, 210, Color.Purple, gClean);
            btnExplorer.Height=30;
            btnExplorer.Click += (s,e) => CleanExplorerHistory();
            toolTip.SetToolTip(btnExplorer, "Dosya Gezgini gecmisini temizle");

            Button btnBrowserCache = CreateBtn("SECILI TARAYICI TEMIZLE", 20, 355, 620, Color.DarkOrange, gClean);
            btnBrowserCache.Height = 30;
            btnBrowserCache.Click += (s,e) => CleanSelectedBrowser();
            toolTip.SetToolTip(btnBrowserCache, "Secili tarayicinin cache'ini temizle");

            Button btnDismCleanup = CreateBtn("DISM COMPONENT CLEANUP", 20, 395, 620, Color.DarkSlateBlue, gClean);
            btnDismCleanup.Height = 25;
            btnDismCleanup.Click += (s,e) => RunDismCleanup();
            toolTip.SetToolTip(btnDismCleanup, "Windows bilesenleri temizligi - GB'larca alan acabilir");

            Button btnTrim = CreateBtn("FORCE TRIM", 10, 470, 210, Color.Purple, diskPage);
            btnTrim.Click += (s,e) => { Task.Run(() => { RunCMD("defrag /C /O /V"); SafeMessageBox("TRIM Iletildi. "); }); };
            toolTip.SetToolTip(btnTrim, "SSD TRIM komutu gonder");

            Button btnDefender = CreateBtn("DEFENDER KAPAT", 230, 470, 210, Color.DarkRed, diskPage);
            btnDefender.Click += (s,e) => DisableDefenderAggressive();
            toolTip.SetToolTip(btnDefender, "Windows Defender'i tamamen kapat - Tamper Protection uyarisi gosterir");

            Button btnDefenderOn = CreateBtn("DEFENDER AC", 450, 470, 220, Color.DarkGreen, diskPage);
            btnDefenderOn.Click += (s,e) => EnableDefender();
            toolTip.SetToolTip(btnDefenderOn, "Windows Defender'i ac");

            tabs.TabPages.Add(diskPage);
        }

private void InitTab_TOOLS() {
    panoPage = new TabPage("PANO");
    panoPage.BackColor = Color.FromArgb(30,30,30);
    panoPage.Name = "PANO";

    lblPanoInfo = CreateLabel("Kopyaladiginiz metinler otomatik kaydedilir", 10, 10, 8, panoPage);
    lblPanoInfo.ForeColor = Color.Gray;

    // === KARAKTER LİMİTİ ===
    CreateLabel("Karakter Siniri:", 10, 35, panoPage);
    numClipLimit = CreateNum(5000, 100, 1000000, 120, 32, 80, panoPage);

    // === TIMER ARALIĞI (YENİ) ===
    CreateLabel("Kontrol Araligi (ms):", 220, 35, panoPage);
    NumericUpDown numClipInterval = CreateNum(500, 100, 5000, 360, 32, 70, panoPage);

    // Config'den yükle
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
            // Değerleri kaydet
            clipLimit = (int)numClipLimit.Value;
            cachedClipInterval = (int)numClipInterval.Value;

            // Timer'ı güncelle
            clipboardTimer.Interval = cachedClipInterval;

            // Dosyaya yaz
            File.WriteAllLines(configPath, new string[] {
                string.Format("Limit={0}", clipLimit),
                string.Format("Interval={0}", cachedClipInterval)
            });

            SafeMessageBox(string.Format("Ayarlar Kaydedildi!\n\nKarakter Limiti: {0}\nKontrol Araligi: {1} ms", clipLimit, cachedClipInterval));
        } catch (Exception ex) {
            SafeMessageBox(string.Format("Hata: {0}", ex.Message));
        }
    };
    toolTip.SetToolTip(btnSetLimit, "Pano ayarlarini kaydet - Timer hemen guncellenir");

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
        lastClipText = ""; // Reset
        try { if(File.Exists(historyPath)) File.Delete(historyPath); } catch { }
        SafeMessageBox("Temizlendi! ");
    };
    toolTip.SetToolTip(btnClear, "Tum pano gecmisini sil");

    tabs.TabPages.Add(panoPage);
}

        private void DetectInstalledBrowsers() {
            Task.Run(() => {
                List<string> browsers = new List<string>();

                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (Directory.Exists(Path.Combine(local, @"Google\Chrome"))) browsers.Add("Google Chrome");
                if (Directory.Exists(Path.Combine(local, @"Microsoft\Edge"))) browsers.Add("Microsoft Edge");
                if (Directory.Exists(Path.Combine(local, @"Mozilla\Firefox"))) browsers.Add("Mozilla Firefox");
                if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Opera Software"))) browsers.Add("Opera");
                if (Directory.Exists(Path.Combine(local, @"BraveSoftware\Brave-Browser"))) browsers.Add("Brave");

                SafeInvoke(() => {
                    if (cmbBrowserList != null) {
                        cmbBrowserList.Items.Clear();
                        foreach (string browser in browsers) cmbBrowserList.Items.Add(browser);
                        if (cmbBrowserList.Items.Count > 0) cmbBrowserList.SelectedIndex = 0;
                    }
                });

                SafeMessageBox(string.Format("{0} tarayici bulundu!", browsers.Count));
            });
        }

        private void CleanSelectedBrowser() {
            Task.Run(() => {
                string selected = null;
                SafeInvokeSync(() => {
                    if (cmbBrowserList != null && cmbBrowserList.SelectedItem != null)
                        selected = cmbBrowserList.SelectedItem.ToString();
                });

                if (string.IsNullOrEmpty(selected)) {
                    SafeMessageBox("Lutfen once tarayici secin!");
                    return;
                }

                string processName = "";
                string cachePath = "";
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (selected.Contains("Chrome")) {
                    processName = "chrome";
                    cachePath = Path.Combine(local, @"Google\Chrome\User Data\Default\Cache");
                } else if (selected.Contains("Edge")) {
                    processName = "msedge";
                    cachePath = Path.Combine(local, @"Microsoft\Edge\User Data\Default\Cache");
                } else if (selected.Contains("Firefox")) {
                    processName = "firefox";
                    cachePath = Path.Combine(local, @"Mozilla\Firefox\Profiles");
                } else if (selected.Contains("Opera")) {
                    processName = "opera";
                    cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Opera Software\Opera Stable\Cache");
                } else if (selected.Contains("Brave")) {
                    processName = "brave";
                    cachePath = Path.Combine(local, @"BraveSoftware\Brave-Browser\User Data\Default\Cache");
                }

                Process[] procs = Process.GetProcessesByName(processName);
                if (procs.Length > 0) {
                    DialogResult res = MessageBox.Show(
                        string.Format("{0} acik!  Kapatilsin mi?\n\nEVET: Kapatilacak ve cache silinecek\nHAYIR: Islem iptal", selected),
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (res == DialogResult.Yes) {
                        foreach (var p in procs) { try { p.Kill(); p.WaitForExit(3000); } catch { } }
                        Thread.Sleep(500);
                    } else {
                        return;
                    }
                }

                long cleaned = 0;
                try {
                    if (Directory.Exists(cachePath)) {
                        cleaned = GetSize(cachePath);
                        CleanDirSafe(cachePath);
                    }
                } catch { }

                SafeMessageBox(string.Format("{0} cache temizlendi!\nTemizlenen: {1} MB", selected, cleaned/1024/1024));
            });
        }

        private void RunDismCleanup() {
            Task.Run(() => {
                DialogResult res = MessageBox.Show(
                    "DISM Component Cleanup GB'larca alan acabilir ama uzun surebilir.\n\nDevam edilsin mi?",
                    "DISM Cleanup",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (res == DialogResult.Yes) {
                    SafeInvoke(() => { if (lblDiskStatus != null) lblDiskStatus.Text = "DISM calisiyor... "; });
                    RunCMD("DISM /Online /Cleanup-Image /StartComponentCleanup /ResetBase");
                    SafeMessageBox("DISM Component Cleanup tamamlandi!");
                    SafeInvoke(() => { if (lblDiskStatus != null) lblDiskStatus.Text = "DISM tamamlandi"; });
                }
            });
        }

private void ScanDisk() {
    SafeInvoke(() => {
        if (lblDiskStatus != null) {
            lblDiskStatus.Text = "Hesaplaniyor...";
            lblDiskStatus.ForeColor = Color.Yellow;
        }
    });

    long totalSize = 0;
    try {
        bool temp=false, prefetch=false, update=false, wer=false, thumbnails=false, recycle=false, browser=false, installer=false, vss=false, dism=false;
        SafeInvokeSync(() => {
            if (chkTemp != null) temp = chkTemp.Checked;
            if (chkPrefetch != null) prefetch = chkPrefetch.Checked;
            if (chkUpdate != null) update = chkUpdate.Checked;
            if (chkWer != null) wer = chkWer.Checked;
            if (chkThumbnails != null) thumbnails = chkThumbnails.Checked;
            if (chkRecycle != null) recycle = chkRecycle.Checked;
            if (chkBrowser != null) browser = chkBrowser.Checked;
            if (chkInstallerCache != null) installer = chkInstallerCache.Checked;
            if (chkVSS != null) vss = chkVSS.Checked;
            if (chkDism != null) dism = chkDism.Checked;
        });

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // === TEMP ===
        if (temp) {
            totalSize += GetSize(Environment.GetEnvironmentVariable("TEMP"));
            totalSize += GetSize(@"C:\Windows\Temp");
            totalSize += GetSize(@"C:\Windows\Logs");
            totalSize += GetSize(Path.Combine(localAppData, "Temp"));
            // Tüm kullanıcıların Temp'i
            try {
                foreach (string userDir in Directory.GetDirectories(@"C:\Users")) {
                    totalSize += GetSize(Path.Combine(userDir, @"AppData\Local\Temp"));
                }
            } catch { }
        }

        // === PREFETCH ===
        if (prefetch) totalSize += GetSize(@"C:\Windows\Prefetch");

        // === UPDATE ===
        if (update) {
            totalSize += GetSize(@"C:\Windows\SoftwareDistribution\Download");
            totalSize += GetSize(@"C:\Windows\SoftwareDistribution\DataStore");
            totalSize += GetSize(@"C:\Windows\SoftwareDistribution\DeliveryOptimization");
            totalSize += GetSize(@"C:\Windows\Logs\CBS");
        }

        // === WER ===
        if (wer) {
            totalSize += GetSize(Path.Combine(localAppData, @"Microsoft\Windows\WER"));
            totalSize += GetSize(@"C:\ProgramData\Microsoft\Windows\WER");
            totalSize += GetSize(@"C:\Windows\LiveKernelReports");
            totalSize += GetSize(@"C:\Windows\Minidump");
            // MEMORY.DMP dosyası
            try {
                string dmpPath = @"C:\Windows\MEMORY.DMP";
                if (File.Exists(dmpPath)) totalSize += new FileInfo(dmpPath).Length;
            } catch { }
        }

        // === THUMBNAILS ===
        if (thumbnails) {
            string explorerPath = Path.Combine(localAppData, @"Microsoft\Windows\Explorer");
            try {
                foreach (string file in Directory.GetFiles(explorerPath, "thumbcache_*.db")) {
                    try { totalSize += new FileInfo(file). Length; } catch { }
                }
                foreach (string file in Directory.GetFiles(explorerPath, "iconcache_*.db")) {
                    try { totalSize += new FileInfo(file). Length; } catch { }
                }
            } catch { }
        }

        // === RECYCLE ===
        if (recycle) totalSize += GetRecycleBinSize();

        // === BROWSER ===
        if (browser) totalSize += GetBrowserCacheSize();

        // === INSTALLER ===
        if (installer) {
            totalSize += GetSize(@"C:\Windows\Installer\$PatchCache$");
            // Temp installer dosyaları
            try {
                foreach (string file in Directory.GetFiles(@"C:\Windows\Installer", "*.tmp")) {
                    try { totalSize += new FileInfo(file). Length; } catch { }
                }
            } catch { }
        }

        // === VSS (tahmini) ===
        if (vss) {
            // vssadmin list shadows çıktısına göre tahmini
            totalSize += 500 * 1024 * 1024; // 500 MB varsayım
        }

        // === HER ZAMAN TEMİZLENEN EKSTRALAR ===
        // D3DSCache
        totalSize += GetSize(Path.Combine(localAppData, "D3DSCache"));
        // Shader Caches
        totalSize += GetSize(Path.Combine(localAppData, @"NVIDIA\DXCache"));
        totalSize += GetSize(Path.Combine(localAppData, @"NVIDIA\GLCache"));
        totalSize += GetSize(Path.Combine(localAppData, @"AMD\DxCache"));
        totalSize += GetSize(Path.Combine(localAppData, @"Intel\ShaderCache"));
        // Font Cache
        totalSize += GetSize(@"C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache");
        // CrashDumps
        totalSize += GetSize(Path.Combine(localAppData, "CrashDumps"));
        // INetCache
        totalSize += GetSize(Path.Combine(localAppData, @"Microsoft\Windows\INetCache"));
        // Recent Items
        totalSize += GetSize(Path.Combine(appData, @"Microsoft\Windows\Recent"));
        // Windows.old (varsa)
        totalSize += GetSize(@"C:\Windows.old");
        totalSize += GetSize(@"C:\$Windows.~BT");
        totalSize += GetSize(@"C:\$Windows.~WS");

        calculatedJunkSize = totalSize;

        SafeInvoke(() => {
            if (lblDiskGain != null && lblDiskStatus != null) {
                lblDiskGain.Text = string.Format("Tahmini: {0:N0} MB ({1:N2} GB)", totalSize/1024/1024, (double)totalSize/1024/1024/1024);
                lblDiskStatus.Text = "Tarama tamamlandi";
                lblDiskStatus.ForeColor = Color.Lime;
            }
        });
    } catch (Exception ex) {
        SafeInvoke(() => {
            if (lblDiskStatus != null) {
                lblDiskStatus.Text = "Hata! ";
                lblDiskStatus.ForeColor = Color.Red;
            }
        });
        LogError("ScanDisk", ex);
    }
}

        private long GetBrowserCacheSize() {
            long size = 0;
            try {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                size += GetSize(Path.Combine(local, @"Google\Chrome\User Data\Default\Cache"));
                size += GetSize(Path.Combine(local, @"Microsoft\Edge\User Data\Default\Cache"));
                size += GetSize(Path.Combine(local, @"Mozilla\Firefox\Profiles"));
            } catch { }
            return size;
        }

        private void CleanBrowserCache() {
            try {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                CleanDirSafe(Path.Combine(local, @"Google\Chrome\User Data\Default\Cache"));
                CleanDirSafe(Path.Combine(local, @"Microsoft\Edge\User Data\Default\Cache"));
                CleanDirSafe(Path.Combine(local, @"Mozilla\Firefox\Profiles"));
            } catch { }
        }

        private void CleanExplorerHistory() {
            Task.Run(() => {
                try {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", false);
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs", false);
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", false);

                    Process[] explorer = Process.GetProcessesByName("explorer");
                    foreach(Process pr in explorer) pr.Kill();

                    SafeMessageBox("Explorer gecmisi temizlendi!");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        /// <summary>
        /// Defender kapatma - Tamper Protection uyarısı ile (DeepSeek yöntemi)
        /// </summary>
        private void DisableDefenderAggressive() {
            Task.Run(() => {
                try {
                    // Tamper Protection Uyarısı
                    DialogResult res = MessageBox.Show(
                        "UYARI: Windows Defender'i tamamen kapatmak icin once Tamper Protection'i manuel olarak kapatmaniz gerekebilir.\n\n" +
                        "Adimlar:\n" +
                        "1.Windows Guvenlik'i acin\n" +
                        "2.Virus & Threat Protection'a gidin\n" +
                        "3.Manage Settings'e tiklayin\n" +
                        "4.Tamper Protection'i KAPATIN\n\n" +
                        "Devam etmek istiyor musunuz?",
                        "Tamper Protection Uyarisi",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (res != DialogResult.Yes) return;

                    // Real-time protection kapatma
                    RunCMD("powershell -Command \"Set-MpPreference -DisableRealtimeMonitoring $true -DisableBehaviorMonitoring $true -DisableBlockAtFirstSeen $true -DisableIOAVProtection $true -DisablePrivacyMode $true -SignatureDisableUpdateOnStartupWithoutEngine $true -DisableArchiveScanning $true -DisableIntrusionPreventionSystem $true -DisableScriptScanning $true -SubmitSamplesConsent 2\"");
                    Thread.Sleep(500);

                    // Registry anahtarları
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiSpyware /t REG_DWORD /d 1 /f");
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableRealtimeMonitoring /t REG_DWORD /d 1 /f");
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableBehaviorMonitoring /t REG_DWORD /d 1 /f");
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableOnAccessProtection /t REG_DWORD /d 1 /f");
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableScanOnRealtimeEnable /t REG_DWORD /d 1 /f");
                    RunCMD("reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableIOAVProtection /t REG_DWORD /d 1 /f");
                    Thread.Sleep(300);

                    // Servisleri durdur
                    RunCMD("sc config WinDefend start= disabled");
                    RunCMD("sc stop WinDefend");
                    RunCMD("sc config WdNisSvc start= disabled");
                    RunCMD("sc stop WdNisSvc");
                    Thread.Sleep(300);

                    // Process'leri sonlandır
                    RunCMD("taskkill /F /IM MsMpEng.exe");
                    RunCMD("taskkill /F /IM NisSrv.exe");
                    RunCMD("taskkill /F /IM SecurityHealthService.exe");
                    Thread.Sleep(200);

                    // Zamanlanmış görevleri devre dışı bırak
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance\" /Disable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup\" /Disable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan\" /Disable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Verification\" /Disable");

                    SafeMessageBox("Windows Defender kapatildi.\n\nNot: Tamper Protection aktifse bazi ayarlar uygulanmamis olabilir.");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}\n\nYonetici gerekli!", ex.Message)); }
            });
        }

        private void EnableDefender() {
            Task.Run(() => {
                try {
                    // Registry anahtarlarını sil
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v DisableAntiSpyware /f");
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableRealtimeMonitoring /f");
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableBehaviorMonitoring /f");
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableOnAccessProtection /f");
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableScanOnRealtimeEnable /f");
                    RunCMD("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v DisableIOAVProtection /f");
                    Thread.Sleep(300);

                    // Servisleri aktif et
                    RunCMD("sc config WinDefend start= auto");
                    RunCMD("sc start WinDefend");
                    RunCMD("sc config WdNisSvc start= auto");
                    RunCMD("sc start WdNisSvc");
                    Thread.Sleep(300);

                    // Zamanlanmış görevleri aktif et
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance\" /Enable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup\" /Enable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan\" /Enable");
                    RunCMD("schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Verification\" /Enable");
                    Thread.Sleep(200);

                    // Real-time protection aç
                    RunCMD("powershell -Command \"Set-MpPreference -DisableRealtimeMonitoring $false\"");

                    SafeMessageBox("Windows Defender acildi.");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

/// <summary>
/// En yüksek yetkiyle dosya/klasör silme - TrustedInstaller seviyesi
/// </summary>
private void CleanDirSafe(string path) {
    if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

    Stack<string> dirs = new Stack<string>();
    dirs.Push(path);

    List<string> failedFiles = new List<string>();
    long deletedSize = 0;
    int deletedCount = 0;

    // === AŞAMA 1: . NET ile hızlı silme ===
    while (dirs.Count > 0) {
        string currentDir = dirs.Pop();
        try {
            DirectoryInfo di = new DirectoryInfo(currentDir);

            foreach (DirectoryInfo subDir in di.GetDirectories()) {
                dirs.Push(subDir.FullName);
            }

            foreach (FileInfo file in di.GetFiles()) {
                try {
                    // Tüm attributeları kaldır
                    file.Attributes = FileAttributes.Normal;
                    long size = file.Length;
                    file.Delete();
                    deletedSize += size;
                    deletedCount++;
                } catch {
                    failedFiles.Add(file.FullName);
                }
            }
        } catch { }
    }

    // === AŞAMA 2: Başarısız dosyalar için CMD ile zorla silme ===
    if (failedFiles.Count > 0) {
        // Toplu silme - daha hızlı
        StringBuilder batchCmd = new StringBuilder();
        foreach (string filePath in failedFiles) {
            // takeown + icacls + del kombinasyonu
            batchCmd.AppendFormat("takeown /F \"{0}\" /A >nul 2>&1 & ", filePath);
            batchCmd.AppendFormat("icacls \"{0}\" /grant Administrators:F >nul 2>&1 & ", filePath);
            batchCmd.AppendFormat("del /F /Q /A \"{0}\" >nul 2>&1 & ", filePath);
        }

        if (batchCmd.Length > 0) {
            RunCMD(batchCmd.ToString());
            Thread.Sleep(100);
        }

        // Hala silinemeyen dosyalar için tek tek robocopy trick
        foreach (string filePath in failedFiles) {
            if (File.Exists(filePath)) {
                try {
                    // Robocopy boş klasör trick - dosyayı siler
                    string tempEmpty = Path.Combine(Path.GetTempPath(), "EmptyDir_" + Guid.NewGuid(). ToString("N"));
                    Directory.CreateDirectory(tempEmpty);
                    string parentDir = Path.GetDirectoryName(filePath);
                    string fileName = Path.GetFileName(filePath);

                    // /MIR ile boş klasörü hedefe kopyala = hedef dosya silinir
                    RunCMD(string.Format("robocopy \"{0}\" \"{1}\" /MIR /NFL /NDL /NJH /NJS /nc /ns /np >nul 2>&1", tempEmpty, parentDir));

                    try { Directory.Delete(tempEmpty, true); } catch { }
                } catch { }
            }
        }
    }

    // === AŞAMA 3: Boş klasörleri sil ===
    try {
        var allDirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                               .OrderByDescending(d => d.Length);
        foreach (string dir in allDirs) {
            try {
                // Önce yetki al
                RunCMD(string.Format("takeown /F \"{0}\" /A /R /D Y >nul 2>&1", dir));
                RunCMD(string.Format("icacls \"{0}\" /grant Administrators:F /T >nul 2>&1", dir));
                Directory.Delete(dir, false);
            } catch { }
        }
    } catch { }
}

private void DeepClean() {
    SafeInvoke(() => {
        if (lblDiskStatus != null) {
            lblDiskStatus.Text = "Temizleniyor...";
            lblDiskStatus.ForeColor = Color.Red;
            if (prgDisk != null) prgDisk.Value = 0;
        }
    });

    long freeSpaceBefore = 0;
    try {
        DriveInfo di = new DriveInfo("C");
        freeSpaceBefore = di.AvailableFreeSpace;
    } catch { }

    try {
        bool temp=false, prefetch=false, update=false, wer=false, thumbnails=false, recycle=false, dism=false, browser=false, installer=false, vss=false;
        SafeInvokeSync(() => {
            if (chkTemp != null) temp = chkTemp.Checked;
            if (chkPrefetch != null) prefetch = chkPrefetch.Checked;
            if (chkUpdate != null) update = chkUpdate.Checked;
            if (chkWer != null) wer = chkWer.Checked;
            if (chkThumbnails != null) thumbnails = chkThumbnails.Checked;
            if (chkRecycle != null) recycle = chkRecycle.Checked;
            if (chkDism != null) dism = chkDism.Checked;
            if (chkBrowser != null) browser = chkBrowser.Checked;
            if (chkInstallerCache != null) installer = chkInstallerCache.Checked;
            if (chkVSS != null) vss = chkVSS.Checked;
        });

        int totalSteps = 12;
        int currentStep = 0;

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        // === WINDOWS UPDATE SERVİSLERİ DURDUR ===
        if (update) {
            RunCMD("net stop wuauserv /y");
            RunCMD("net stop bits /y");
            Thread.Sleep(300);
        }

        // === TEMP ===
        if (temp) {
            CleanDirSafe(Environment.GetEnvironmentVariable("TEMP"));
            CleanDirSafe(@"C:\Windows\Temp");
            CleanDirSafe(Path.Combine(localAppData, "Temp"));
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === PREFETCH ===
        if (prefetch) {
            CleanDirSafe(@"C:\Windows\Prefetch");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === UPDATE ===
        if (update) {
            CleanDirSafe(@"C:\Windows\SoftwareDistribution\Download");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === WER ===
        if (wer) {
            CleanDirSafe(Path.Combine(localAppData, @"Microsoft\Windows\WER"));
            CleanDirSafe(@"C:\ProgramData\Microsoft\Windows\WER");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === RECYCLE ===
        if (recycle) {
            try { SHEmptyRecycleBin(IntPtr.Zero, null, 0x0007); } catch { }
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === THUMBNAILS - GÜVENLİ (Explorer kapatmadan) ===
        if (thumbnails) {
            string explorerCache = Path.Combine(localAppData, @"Microsoft\Windows\Explorer");
            try {
                foreach (string file in Directory.GetFiles(explorerCache, "thumbcache_*.db")) {
                    try {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    } catch { }
                }
            } catch { }
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === D3DSCACHE + SHADER CACHE ===
        CleanDirSafe(Path.Combine(localAppData, "D3DSCache"));
        CleanDirSafe(Path.Combine(localAppData, @"NVIDIA\DXCache"));
        CleanDirSafe(Path.Combine(localAppData, @"NVIDIA\GLCache"));
        CleanDirSafe(Path.Combine(localAppData, @"AMD\DxCache"));
        currentStep++;
        SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });

        // === EVENT LOGS ===
        RunCMD("wevtutil cl System");
        RunCMD("wevtutil cl Application");
        currentStep++;
        SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });

        // === BROWSER ===
        if (browser) {
            CleanBrowserCache();
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === INSTALLER ===
        if (installer) {
            CleanDirSafe(@"C:\Windows\Installer\$PatchCache$");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === VSS ===
        if (vss) {
            RunCMD("vssadmin delete shadows /all /quiet");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === DISM ===
        if (dism) {
            SafeMessageBox("DISM baslatiliyor...  (Bu islem birkac dakika surebilir)");
            RunCMD("dism /online /cleanup-image /startcomponentcleanup /resetbase /norestart");
            currentStep++;
            SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = (currentStep * 100) / totalSteps; });
        }

        // === SERVİSLERİ YENİDEN BAŞLAT ===
        if (update) {
            RunCMD("net start wuauserv");
        }

        GC.Collect();
        SafeInvoke(() => { if (prgDisk != null) prgDisk.Value = 100; });

        // Gerçek silinen miktarı hesapla
        Thread.Sleep(500);

        long freeSpaceAfter = 0;
        try {
            DriveInfo di = new DriveInfo("C");
            freeSpaceAfter = di.AvailableFreeSpace;
        } catch { }

        long actualFreedSpace = freeSpaceAfter - freeSpaceBefore;
        if (actualFreedSpace < 0) actualFreedSpace = 0;

        SafeMessageBox(string.Format("Temizlik Tamamlandi!\n\nKazanilan Alan: {0:N0} MB", actualFreedSpace / 1024 / 1024));

        Thread.Sleep(300);
        ScanDisk();

    } catch (Exception ex) {
        SafeMessageBox(string.Format("Temizlik sirasinda hata: {0}", ex.Message));
        LogError("DeepClean", ex);
    }
}


private long GetRecycleBinSize() {
    long totalSize = 0;
    try {
        DriveInfo[] drives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in drives) {
            try {
                if (drive.DriveType == DriveType.Fixed && drive.IsReady) {
                    string recyclePath = string.Format(@"{0}$Recycle.Bin", drive.Name);
                    totalSize += GetSize(recyclePath);
                }
            } catch { }
        }
    } catch (Exception ex) {
        LogError("GetRecycleBinSize", ex);
    }
    return totalSize;
}

private long GetSize(string path) {
    long totalSize = 0;

    if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return 0;

    Stack<string> dirs = new Stack<string>(20);
    dirs.Push(path);

    while (dirs.Count > 0) {
        string currentDir = dirs.Pop();

        try {
            DirectoryInfo di = new DirectoryInfo(currentDir);

            // 1.Dosyaları Topla
            foreach (FileInfo fi in di.EnumerateFiles()) {
                try {
                    totalSize += fi.Length;
                } catch { }
            }

            // 2.Alt Klasörleri Stack'e Ekle
            foreach (DirectoryInfo subDi in di.EnumerateDirectories()) {
                dirs.Push(subDi.FullName);
            }
        }
        catch {
            // Erişim engellendiyse atla
        }
    }

    return totalSize;
}

        private void SetDNS(string p, string s) {
            Task.Run(() => {
                try {
                    // Adaptör ismi tırnak içinde - boşluklu isimler için
                    RunCMD(string.Format("netsh interface ip delete dns \"{0}\" all", activeAdapter));
                    Thread.Sleep(200);

                    RunCMD(string.Format("netsh interface ip set dns \"{0}\" static {1} primary", activeAdapter, p));
                    Thread.Sleep(100);
                    RunCMD(string.Format("netsh interface ip add dns \"{0}\" {1} index=2", activeAdapter, s));
                    Thread.Sleep(100);

                    RunCMD("ipconfig /flushdns");

                    SafeMessageBox(string.Format("DNS Ayarlandi!\n\nPrimary: {0}\nSecondary: {1}", p, s));
                } catch (Exception ex) {
                    SafeMessageBox(string.Format("DNS Ayarlama Hatasi: {0}", ex.Message));
                }
            });
        }

        private void SetDNSAuto() {
            Task.Run(() => {
                try {
                    RunCMD(string.Format("netsh interface ip set dns \"{0}\" dhcp", activeAdapter));
                    Thread.Sleep(100);
                    RunCMD("ipconfig /flushdns");
                    SafeMessageBox("DNS Otomatige alindi (DHCP)!");
                } catch (Exception ex) {
                    SafeMessageBox(string.Format("Hata: {0}", ex.Message));
                }
            });
        }

        private void SetTcp(int val) {
            Task.Run(() => {
                try {
                    string key = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key)) {
                        if (rk == null) return;
                        foreach (string subkey in rk.GetSubKeyNames()) {
                            using (RegistryKey iKey = Registry.LocalMachine.OpenSubKey(key + "\\" + subkey, true)) {
                                if (iKey != null) {
                                    iKey.SetValue("TcpAckFrequency", val, RegistryValueKind.DWord);
                                    iKey.SetValue("TCPNoDelay", val, RegistryValueKind.DWord);
                                }
                            }
                        }
                    }
                    SafeMessageBox(val == 1 ? "TCP OYUN MODU AKTIF!" : "TCP Normal Mod. ");
                } catch { SafeMessageBox("Yonetici gerekli! "); }
            });
        }

        private void ForceTcpNoDelayMSMQ(bool enable) {
            Task.Run(() => {
                try {
                    string keyPath = @"SOFTWARE\Microsoft\MSMQ\Parameters";
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath)) {
                        if (key != null) {
                            if (enable) {
                                key.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                                SafeMessageBox("MSMQ seviyesinde TCP NoDelay aktif edildi!");
                            } else {
                                key.DeleteValue("TCPNoDelay", false);
                                SafeMessageBox("MSMQ TCP NoDelay normale donduruldu.");
                            }
                        }
                    }
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}\n\nYonetici gerekli!", ex.Message)); }
            });
        }

        private void EnableDSCP() {
            Task.Run(() => {
                try {
                    string gameName = "GameTraffic";

                    SafeInvokeSync(() => {
                        Form inputForm = new Form();
                        inputForm.Text = "DSCP Ayari";
                        inputForm.Size = new Size(400, 180);
                        inputForm.StartPosition = FormStartPosition.CenterParent;
                        inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                        inputForm.MaximizeBox = false;
                        inputForm.MinimizeBox = false;
                        inputForm.BackColor = Color.FromArgb(30, 30, 30);
                        inputForm.ForeColor = Color.White;

                        Label lblInfo = new Label();
                        lblInfo.Text = "Program adi girin (ornek: csgo.exe)\nBos birakir = Tum trafik";
                        lblInfo.Location = new Point(20, 20);
                        lblInfo.Size = new Size(350, 40);
                        lblInfo.ForeColor = Color.LightGray;
                        inputForm.Controls.Add(lblInfo);

                        TextBox txtInput = new TextBox();
                        txtInput.Location = new Point(20, 70);
                        txtInput.Size = new Size(340, 25);
                        txtInput.BackColor = Color.FromArgb(50, 50, 50);
                        txtInput.ForeColor = Color.White;
                        inputForm.Controls.Add(txtInput);

                        Button btnOk = new Button();
                        btnOk.Text = "TAMAM";
                        btnOk.Location = new Point(200, 105);
                        btnOk.Size = new Size(80, 30);
                        btnOk.BackColor = Color.DarkGreen;
                        btnOk.ForeColor = Color.White;
                        btnOk.FlatStyle = FlatStyle.Flat;
                        btnOk.DialogResult = DialogResult.OK;
                        inputForm.Controls.Add(btnOk);

                        Button btnCancel = new Button();
                        btnCancel.Text = "IPTAL";
                        btnCancel.Location = new Point(290, 105);
                        btnCancel.Size = new Size(70, 30);
                        btnCancel.BackColor = Color.DarkRed;
                        btnCancel.ForeColor = Color.White;
                        btnCancel.FlatStyle = FlatStyle.Flat;
                        btnCancel.DialogResult = DialogResult.Cancel;
                        inputForm.Controls.Add(btnCancel);

                        inputForm.AcceptButton = btnOk;
                        inputForm.CancelButton = btnCancel;

                        if (inputForm.ShowDialog() == DialogResult.OK) {
                            if (! string.IsNullOrEmpty(txtInput.Text.Trim())) {
                                gameName = txtInput.Text.Trim();
                            }
                        } else {
                            return;
                        }
                    });

                    string cmd = string.Format("New-NetQosPolicy -Name \"Anoreksik_{0}\" -AppPathNameMatchCondition \"{0}\" -NetworkProfile All -ThrottleRateActionBitsPerSecond 0 -DSCPAction 46", gameName);
                    RunPowerShell(cmd);

                    SafeMessageBox("DSCP-46 QoS politikasi aktif edildi!");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void DisableDSCP() {
            Task.Run(() => {
                try {
                    RunPowerShell("Get-NetQosPolicy | Where-Object {$_.Name -like 'Anoreksik_*'} | Remove-NetQosPolicy -Confirm:$false");
                    SafeMessageBox("DSCP politikalari kaldirildi.");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

private void RunPowerShell(string cmd) {
    try {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "powershell.exe";
        psi.Arguments = string.Format("-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", cmd);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        // psi.Verb = "runas"; ← KALDIR (zaten admin'iz)

        using (Process p = Process.Start(psi)) {
            if (p != null) p.WaitForExit(10000);
        }
    } catch (Exception ex) { LogError("RunPowerShell", ex); }
}

        private void OptimizeCpuQuantum() {
            Task.Run(() => {
                try {
                    string keyPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true)) {
                        if (key != null) {
                            key.SetValue("Win32PrioritySeparation", 38, RegistryValueKind.DWord);
                            SafeMessageBox("CPU Quantum optimize edildi!  (Oyun performansi +)");
                        }
                    }
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void ResetCpuQuantum() {
            Task.Run(() => {
                try {
                    string keyPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true)) {
                        if (key != null) {
                            key.SetValue("Win32PrioritySeparation", 2, RegistryValueKind.DWord);
                            SafeMessageBox("CPU Quantum normale donduruldu.");
                        }
                    }
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void DisableNicInterruptModeration() {
            Task.Run(() => {
                try {
                    string basePath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
                    using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey(basePath, true)) {
                        if (classKey != null) {
                            foreach (string subKeyName in classKey.GetSubKeyNames()) {
                                using (RegistryKey subKey = classKey.OpenSubKey(subKeyName, true)) {
                                    if (subKey != null && subKey.GetValue("*InterruptModeration") != null) {
                                        subKey.SetValue("*InterruptModeration", "0", RegistryValueKind.String);
                                    }
                                }
                            }
                        }
                    }
                    // Adaptör disable/enable - boşluklu isimler için tırnak içinde
                    RunFastNetsh(string.Format("interface set interface \"{0}\" disable", activeAdapter));
                    Thread.Sleep(500);
                    RunFastNetsh(string.Format("interface set interface \"{0}\" enable", activeAdapter));

                    SafeMessageBox("NIC Interrupt Moderation kapatildi!");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void EnableNicInterruptModeration() {
            Task.Run(() => {
                try {
                    string basePath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";
                    using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey(basePath, true)) {
                        if (classKey != null) {
                            foreach (string subKeyName in classKey.GetSubKeyNames()) {
                                using (RegistryKey subKey = classKey.OpenSubKey(subKeyName, true)) {
                                    if (subKey != null && subKey.GetValue("*InterruptModeration") != null) {
                                        subKey.SetValue("*InterruptModeration", "1", RegistryValueKind.String);
                                    }
                                }
                            }
                        }
                    }
                    RunFastNetsh(string.Format("interface set interface \"{0}\" disable", activeAdapter));
                    Thread.Sleep(500);
                    RunFastNetsh(string.Format("interface set interface \"{0}\" enable", activeAdapter));

                    SafeMessageBox("NIC Interrupt Moderation normale donduruldu.");
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void LockArpCache() {
            Task.Run(() => {
                try {
                    arpLock.Wait();
                    try {
                        string gateway = GetDefaultGateway();
                        string mac = GetGatewayMac(gateway);
                        if (!string.IsNullOrEmpty(gateway) && !string.IsNullOrEmpty(mac)) {
                            // Adaptör ismi tırnak içinde
                            RunCMD(string.Format("netsh interface ip add neighbors \"{0}\" \"{1}\" \"{2}\"", activeAdapter, gateway, mac));
                            lockedArpGateway = gateway;
                            lockedArpMac = mac;
                            SafeMessageBox(string.Format("ARP Cache kilitlendi:\n{0} -> {1}", gateway, mac));
                        } else {
                            SafeMessageBox("Gateway veya MAC bulunamadi!\n\nAg baglantisi kontrol edin.");
                        }
                    } finally {
                        arpLock.Release();
                    }
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private void UnlockArpCache() {
            Task.Run(() => {
                try {
                    arpLock.Wait();
                    try {
                        if (! string.IsNullOrEmpty(lockedArpGateway)) {
                            RunCMD(string.Format("netsh interface ip delete neighbors \"{0}\" \"{1}\"", activeAdapter, lockedArpGateway));
                            SafeMessageBox(string.Format("ARP kilidi kaldirildi:\n{0}", lockedArpGateway));
                            lockedArpGateway = "";
                            lockedArpMac = "";
                        } else {
                            SafeMessageBox("Aktif ARP kilidi bulunamadi!");
                        }
                    } finally {
                        arpLock.Release();
                    }
                } catch (Exception ex) { SafeMessageBox(string.Format("Hata: {0}", ex.Message)); }
            });
        }

        private string GetDefaultGateway() {
            try {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
                    if (ni.Name == activeAdapter && ni.OperationalStatus == OperationalStatus.Up) {
                        IPInterfaceProperties props = ni.GetIPProperties();
                        foreach (GatewayIPAddressInformation gw in props.GatewayAddresses) {
                            return gw.Address.ToString();
                        }
                    }
                }
            } catch { }
            return "";
        }

        private string GetGatewayMac(string ip) {
            try {
                ProcessStartInfo psi = new ProcessStartInfo("arp", string.Format("-a {0}", ip));
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using (Process p = Process.Start(psi)) {
                    string output = p.StandardOutput.ReadToEnd();
                    foreach (string line in output.Split('\n')) {
                        if (line.Contains(ip)) {
                            string[] parts = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2) return parts[1];
                        }
                    }
                }
            } catch { }
            return "";
        }

        private void RefreshProcs() {
            Task.Run(() => {
                HashSet<string> names = new HashSet<string>();
                foreach(Process p in Process.GetProcesses()) {
                    try {
                        if(p.MainWindowHandle != IntPtr.Zero) names.Add(p.ProcessName);
                    } catch { }
                }
                SafeInvoke(() => {
                    if (cmbProcList != null && ! cmbProcList.IsDisposed) {
                        cmbProcList.Items.Clear();
                        foreach(string n in names) cmbProcList.Items.Add(n);
                        if(cmbProcList.Items.Count > 0) cmbProcList.SelectedIndex = 0;
                    }
                    if (cmbProcListAffinity != null && ! cmbProcListAffinity.IsDisposed) {
                        cmbProcListAffinity.Items.Clear();
                        foreach(string n in names) cmbProcListAffinity.Items.Add(n);
                        if(cmbProcListAffinity.Items.Count > 0) cmbProcListAffinity.SelectedIndex = 0;
                    }
                });
            });
        }

        private void BoostProc() {
            Task.Run(() => {
                string name = null;
                string prioStr = "Normal";
                string ioStr = "Normal";

                SafeInvokeSync(() => {
                    if (cmbProcList != null && cmbProcList.SelectedItem != null) name = cmbProcList.SelectedItem.ToString();
                    if (cmbPriority != null && cmbPriority.SelectedItem != null) prioStr = cmbPriority.SelectedItem.ToString();
                    if (cmbIoPriority != null && cmbIoPriority.SelectedItem != null) ioStr = cmbIoPriority.SelectedItem.ToString();
                });

                if (string.IsNullOrEmpty(name)) return;

                ProcessPriorityClass prio = ProcessPriorityClass.Normal;
                if (prioStr == "High") prio = ProcessPriorityClass.High;
                else if (prioStr == "Low") prio = ProcessPriorityClass.BelowNormal;

                int ioPrio = 2;
                if (ioStr == "Very High") ioPrio = 3;
                else if (ioStr == "High") ioPrio = 2;
                else if (ioStr == "Normal") ioPrio = 2;
                else if (ioStr == "Low") ioPrio = 1;
                else if (ioStr == "Very Low") ioPrio = 0;

                Process[] targets = Process.GetProcessesByName(name);
                int count = 0;
                foreach(Process p in targets) {
                    try {
                        p.PriorityClass = prio;
                        NtSetInformationProcess(p.Handle, 33, ref ioPrio, 4);
                        count++;
                    } catch { }
                }

                SafeInvoke(() => {
                    string entry = string.Format("{0} (CPU:{1}, IO:{2})", name, prioStr, ioStr);
                    if(lstBoosted != null && ! lstBoosted.Items.Contains(entry)) lstBoosted.Items.Add(entry);
                });
                SafeMessageBox(string.Format("{0} adet onceliklendirildi!", count));
            });
        }

        private void RevertProc() {
            Task.Run(() => {
                string entry = null;
                SafeInvokeSync(() => {
                    if (lstBoosted != null && lstBoosted.SelectedItem != null) entry = lstBoosted.SelectedItem.ToString();
                });

                if (string.IsNullOrEmpty(entry)) return;

                string name = entry.Split('(')[0]. Trim();
                Process[] targets = Process.GetProcessesByName(name);
                int ioPrio = 2;
                foreach(Process p in targets) {
                    try {
                        p.PriorityClass = ProcessPriorityClass.Normal;
                        NtSetInformationProcess(p.Handle, 33, ref ioPrio, 4);
                    } catch { }
                }

                SafeInvoke(() => { if (lstBoosted != null) lstBoosted.Items.Remove(entry); });
                SafeMessageBox("Normale donduruldu.");
            });
        }

        private void SetAffinity() {
            Task.Run(() => {
                string name = null;
                SafeInvokeSync(() => {
                    if (cmbProcListAffinity != null && cmbProcListAffinity.SelectedItem != null) name = cmbProcListAffinity.SelectedItem.ToString();
                });

                if (string.IsNullOrEmpty(name)) return;

                int coreCount = Environment.ProcessorCount;
                if (coreCount < 3) {
                    SafeMessageBox("En az 3 cekirdek gerekli!");
                    return;
                }

                IntPtr affinityMask = new IntPtr((1 << (coreCount - 2)) | (1 << (coreCount - 1)));

                Process[] targets = Process.GetProcessesByName(name);
                int count = 0;
                foreach(Process p in targets) {
                    try {
                        SetProcessAffinityMask(p.Handle, affinityMask);
                        count++;
                    } catch { }
                }

                SafeInvoke(() => {
                    string entry = string.Format("{0} -> Son 2 Cekirdek", name);
                    if(lstAffinitySet != null && !lstAffinitySet.Items.Contains(entry)) lstAffinitySet.Items.Add(entry);
                });

                SafeMessageBox(string.Format("{0} adet son 2 cekirdege atildi!", count));
            });
        }

        private void RevertAffinity() {
            Task.Run(() => {
                string entry = null;
                SafeInvokeSync(() => {
                    if (lstAffinitySet != null && lstAffinitySet.SelectedItem != null) entry = lstAffinitySet.SelectedItem.ToString();
                });

                if (string.IsNullOrEmpty(entry)) return;

                string name = entry.Split(new string[] {" ->"}, StringSplitOptions.None)[0].Trim();
                int coreCount = Environment.ProcessorCount;
                IntPtr allCoresMask = new IntPtr((1 << coreCount) - 1);

                Process[] targets = Process.GetProcessesByName(name);
                foreach(Process p in targets) {
                    try {
                        SetProcessAffinityMask(p.Handle, allCoresMask);
                    } catch { }
                }

                SafeInvoke(() => { if (lstAffinitySet != null) lstAffinitySet.Items.Remove(entry); });
                SafeMessageBox("Affinity normale donduruldu.");
            });
        }

private void RunCMD(string cmd) {
    try {
        ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + cmd);
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false; // ← Değişti
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;

        // Program zaten admin olarak çalışıyor - runas gereksiz
        // psi.Verb = "runas"; ← KALDIR

        using (Process p = Process.Start(psi)) {
            if (p != null) p.WaitForExit(10000);
        }
    } catch (Exception ex) {
        LogError("RunCMD:" + cmd, ex);
    }
}

private string GetSSID() {
    try {
        uint ver; IntPtr handle = IntPtr.Zero; IntPtr ifacePtr = IntPtr.Zero;
        try {
            if(WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0) {
                if(WlanEnumInterfaces(handle, IntPtr.Zero, out ifacePtr) == 0) {
                    WLAN_INTERFACE_INFO_LIST list = (WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(ifacePtr, typeof(WLAN_INTERFACE_INFO_LIST));
                    if(list.dwNumberOfItems > 0) {
                        WLAN_INTERFACE_INFO info = (WLAN_INTERFACE_INFO)Marshal.PtrToStructure(new IntPtr(ifacePtr.ToInt64() + 8), typeof(WLAN_INTERFACE_INFO));

                        uint sz; IntPtr dataPtr = IntPtr.Zero;
                        Guid tempGuid = info.InterfaceGuid;
                        if(WlanQueryInterface(handle, ref tempGuid, WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection, IntPtr.Zero, out sz, out dataPtr, IntPtr.Zero) == 0) {
                            if (dataPtr != IntPtr.Zero) {
                                WLAN_CONNECTION_ATTRIBUTES attr = (WLAN_CONNECTION_ATTRIBUTES)Marshal.PtrToStructure(dataPtr, typeof(WLAN_CONNECTION_ATTRIBUTES));
                                byte[] ssidBytes = attr.wlanAssociationAttributes.dot11Ssid.ucSSID;
                                int ssidLen = (int)attr.wlanAssociationAttributes.dot11Ssid.uSSIDLength;
                                if (ssidLen > 0 && ssidLen <= 32) {
                                    string ssid = Encoding.UTF8.GetString(ssidBytes, 0, ssidLen);
                                    WlanFreeMemory(dataPtr);
                                    return ssid;
                                }
                            }
                        }
                    }
                }
            }
        } finally {
            if (ifacePtr != IntPtr.Zero) WlanFreeMemory(ifacePtr);
            if (handle != IntPtr.Zero) WlanCloseHandle(handle, IntPtr.Zero);
        }
    } catch (Exception ex) { LogError("GetSSID", ex); }

    // Fallback: netsh kullan
    try {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "netsh.exe";
        psi.Arguments = "wlan show interfaces";
        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.CreateNoWindow = true;

        using (Process p = Process.Start(psi)) {
            string output = p.StandardOutput.ReadToEnd();
            foreach(string line in output.Split('\n')) {
                if (line.Trim().StartsWith("SSID") && !line.Trim().StartsWith("BSSID")) {
                    int idx = line.IndexOf(':');
                    if (idx > 0) return line.Substring(idx + 1).Trim();
                }
            }
        }
    } catch { }

    return "Bulunamadi";
}

        private void OptimizeMemory() {
            try {
                EmptyWorkingSet(Process.GetCurrentProcess(). Handle);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch { }
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

        private Label CreateLabel(string t, int x, int y, Control p) { return CreateLabel(t, x, y, 9, p); }
        private Label CreateLabel(string t, int x, int y, int size, Control p) { Label l = new Label(); l.Text = t; l.Location = new Point(x, y); l.AutoSize = true; if(size > 0) l.Font = new Font("Arial", size); p.Controls.Add(l); return l; }
        private Button CreateBtn(string t, int x, int y, int w, Color c, Control p) { Button b = new Button(); b.Text=t; b.Location=new Point(x,y); b.Size=new Size(w,40); b.BackColor=c; b.ForeColor=Color.White; b.FlatStyle=FlatStyle.Flat; p.Controls.Add(b); return b; }
        private NumericUpDown CreateNum(int v, int min, int max, int x, int y, int w, Control p) {
            NumericUpDown n = new NumericUpDown();
            n.Minimum = min;
            n.Maximum = max;
            if (v < min) v = min;
            if (v > max) v = max;
            n.Value = v;
            n.Location = new Point(x, y);
            n.Size = new Size(w, 20);
            n.BackColor = Color.FromArgb(40, 40, 40);
            n.ForeColor = Color.White;
            p.Controls.Add(n);
            return n;
        }
        private CheckBox CreateCheck(string t, int x, int y, Control p) { CheckBox c = new CheckBox(); c.Text=t; c.Location=new Point(x,y); c.AutoSize=true; p.Controls.Add(c); return c; }
        private GroupBox CreateGroup(string t, int x, int y, int w, int h, Control p) { GroupBox g = new GroupBox(); g.Text=t; g.ForeColor=Color.White; g.Location=new Point(x,y); g.Size=new Size(w,h); p.Controls.Add(g); return g; }

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

private void ShutdownApp() {
    SaveDashboardConfig();
    SaveHistory();

    isShuttingDown = true;

    try { if (coreTimer != null) { coreTimer.Stop(); coreTimer.Dispose(); } } catch { }
    try { if (wifiSignalTimer != null) { wifiSignalTimer.Stop(); wifiSignalTimer.Dispose(); } } catch { }
    try { if (wifiKeepAliveTimer != null) { wifiKeepAliveTimer.Stop(); wifiKeepAliveTimer.Dispose(); } } catch { }
    try { if (clipboardTimer != null) { clipboardTimer.Stop(); clipboardTimer.Dispose(); } } catch { }

    StopWifiWatchdog();

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

        [STAThread]
        public static void Main() {
            try {
                if(! IsAdmin()) {
                    try {
                        ProcessStartInfo p = new ProcessStartInfo();
                        p.UseShellExecute = true;
                        p.WorkingDirectory = Environment.CurrentDirectory;
                        p.FileName = Application.ExecutablePath;
                        p.Verb = "runas";
                        Process.Start(p);
                    } catch (Exception ex) {
                        MessageBox.Show("Yonetici modu hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            } catch (Exception ex) {
                MessageBox.Show("KRITIK HATA:\n\n" + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace,
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try {
                    File.WriteAllText("CRASH.txt",
                        DateTime.Now.ToString() + "\n\n" + ex.ToString());
                } catch { }
            }
        }

        private static bool IsAdmin() {
            try {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal p = new WindowsPrincipal(id);
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            } catch {
                return false;
            }
        }

        private bool EnablePrivilege(string privilege) {
            try {
                IntPtr hTok = IntPtr.Zero;
                OpenProcessToken(Process.GetCurrentProcess(). Handle, 0x0020 | 0x0008, ref hTok);
                TokPriv1Luid tp;
                tp.Count=1;
                tp.Luid=0;
                tp.Attr=0x00000002;
                LookupPrivilegeValue(null, privilege, ref tp.Luid);
                AdjustTokenPrivileges(hTok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                return true;
            } catch {
                return false;
            }
        }
    }
}