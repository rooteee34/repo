using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Concurrent;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage wifiPage;
        private CheckBox chkWifiActive;
        private Label lblWifiStat, lblWifiPing, lblWifiSSID, lblWifiSignal, lblWifiSpeed, lblWifiInfo;
        private Panel pnlKeepAliveIndicator;
        private ListBox lstWifiLog;
        private ComboBox cmbWifiTarget;
        private NumericUpDown numWifiTimeout, numWifiFailureThreshold, numWifiTTL, numWifiBufferSize, numSignalInterval;

        private string wifiTargetIP = "1.1.1.1";
        private string currentSSID = "";
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

        private void InitTab_WIFI() {
            wifiPage = new TabPage("WIFI");
            wifiPage.BackColor = Color.FromArgb(30,30,30);
            wifiPage.Name = "WIFI";

            chkWifiActive = CreateCheck("Wifi Bekcisini Aktif Et", 10, 10, wifiPage);
            chkWifiActive.ForeColor = Color.Yellow;

            chkWifiActive.CheckedChanged += (s,e) => {
                if (chkWifiActive.Checked) {
                    wifiModuleActive = true;
                    currentSSID = GetSSID();
                    if (lblWifiSSID != null) lblWifiSSID.Text = string.Format("SSID: {0}", currentSSID);

                    if (wifiSignalTimer != null && cachedSignalEnabled && !isHidden) {
                        wifiSignalTimer.Stop();
                        wifiSignalTimer.Interval = signalUpdateInterval;
                        wifiSignalTimer.Start();
                    }

                    StartWifiWatchdog();
                } else {
                    wifiModuleActive = false;
                    if (wifiSignalTimer != null) wifiSignalTimer.Stop();

                    if (lblWifiStat != null) { lblWifiStat.Text = "Pasif"; lblWifiStat.ForeColor = Color.Gray; }
                    if (lblWifiPing != null) lblWifiPing.Text = "Ping: -- ms";
                    if (lblWifiSignal != null) { lblWifiSignal.Text = "Sinyal: --"; lblWifiSignal.ForeColor = Color.Gray; }
                    if (lblWifiSpeed != null) { lblWifiSpeed.Text = "Hiz: --"; lblWifiSpeed.ForeColor = Color.Gray; }
                }
            };
            toolTip.SetToolTip(chkWifiActive, "WiFi izleyiciyi aktif et - baglanti kopmalarina karsi otomatik mudahale");

            lblWifiInfo = CreateLabel("WiFi kopmalari otomatik tespit edilir ve NATIVE API ile aninda resetlenir (Adaptor kapatilmaz)", 10, 35, 8, wifiPage);
            lblWifiInfo.ForeColor = Color.Gray;
            lblWifiInfo.Size = new Size(660, 20);

            CreateLabel("Ping Hedefi:", 10, 60, wifiPage);
            cmbWifiTarget = new ComboBox();
            cmbWifiTarget.Items.AddRange(new string[] {"1.1.1.1", "8.8.8.8", "208.67.222.222"});
            cmbWifiTarget.SelectedIndex = 0;
            cmbWifiTarget.Location = new Point(90, 58);
            cmbWifiTarget.Size = new Size(100, 25);
            wifiPage.Controls.Add(cmbWifiTarget);
            cmbWifiTarget.SelectedIndexChanged += (s,e) => wifiTargetIP = cmbWifiTarget.SelectedItem.ToString();

            lblWifiSSID = CreateLabel("SSID: --", 210, 63, wifiPage);

            lblWifiStat = CreateLabel("Durum: Kapali", 10, 90, wifiPage);
            lblWifiPing = CreateLabel("Ping: -- ms", 140, 90, wifiPage);
            lblWifiSignal = CreateLabel("Sinyal: --", 240, 90, wifiPage);
            lblWifiSpeed = CreateLabel("Hiz: --", 350, 90, wifiPage);

            pnlKeepAliveIndicator = new Panel();
            pnlKeepAliveIndicator.Location = new Point(480, 88);
            pnlKeepAliveIndicator.Size = new Size(16, 16);
            pnlKeepAliveIndicator.BackColor = Color.DarkGreen;
            wifiPage.Controls.Add(pnlKeepAliveIndicator);
            CreateLabel("Keep-Alive", 500, 90, 8, wifiPage).ForeColor = Color.Gray;

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

            Button btnSaveW = CreateBtn("KAYDET", 210, 50, 100, Color.DarkGreen, gSet);
            btnSaveW.Height = 50;
            btnSaveW.Click += (s,e) => SaveWifiConfig();

            Button btnResetW = CreateBtn("MANUEL RESET", 320, 50, 110, Color.DarkRed, gSet);
            btnResetW.Height = 50;
            btnResetW.Click += (s,e) => Task.Run(() => ManualReset());

            Button btnAutoConfigOn = CreateBtn("AG TARAMA AC", 440, 50, 100, Color.DarkGreen, gSet);
            btnAutoConfigOn.Height = 23;
            btnAutoConfigOn.Click += (s,e) => {
                RunFastNetsh(string.Format("wlan set autoconfig enabled=yes interface=\"{0}\"", activeAdapter));
                userDisabledAutoConfig = false;
                SafeMessageBox("Ag Tarama ACILDI");
            };

            Button btnAutoConfigOff = CreateBtn("AG TARAMA KAPAT", 540, 50, 100, Color.Maroon, gSet);
            btnAutoConfigOff.Height = 23;
            btnAutoConfigOff.Click += (s,e) => {
                RunFastNetsh(string.Format("wlan set autoconfig enabled=no interface=\"{0}\"", activeAdapter));
                userDisabledAutoConfig = true;
                SafeMessageBox("Ag Tarama KAPATILDI");
            };

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

        private void StartWifiWatchdog() {
            if (wifiWatchdogTask != null && !wifiWatchdogTask.IsCompleted) return;
            wifiCts = new CancellationTokenSource();
            wifiWatchdogTask = Task.Factory.StartNew(() => WatchdogLoop(wifiCts.Token), wifiCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void StopWifiWatchdog() {
            if (wifiCts != null) {
                wifiCts.Cancel();
                try { if (wifiWatchdogTask != null) wifiWatchdogTask.Wait(1000); } catch { }
                wifiCts.Dispose();
                wifiCts = null;
            }
        }

        private void WatchdogLoop(CancellationToken ct) {
            try { Thread.CurrentThread.Priority = ThreadPriority.Highest; } catch { }
            Stopwatch sw = new Stopwatch();
            using (Ping localPing = new Ping()) {
                byte[] localBuffer = new byte[32];
                PingOptions localOptions = new PingOptions(64, true);
                int lastTtl = 64;
                int lastBufSize = 32;

                int timeout = wifiTimeout;
                int maxFail = wifiFailureThreshold;
                int ttl = wifiTTL;
                int bufSize = wifiBufferSize;

                while (!isShuttingDown && ! ct.IsCancellationRequested) {
                    if (! wifiModuleActive) {
                        Thread.Sleep(IDLE_SLEEP_MS);
                        continue;
                    }
                    try {
                        if (isResetting) { Thread.Sleep(IDLE_SLEEP_MS); continue; }

                        timeout = wifiTimeout;
                        maxFail = wifiFailureThreshold;
                        ttl = wifiTTL;
                        bufSize = wifiBufferSize;

                        if (lastTtl != ttl) { localOptions.Ttl = ttl; lastTtl = ttl; }
                        if (lastBufSize != bufSize) { localBuffer = new byte[bufSize]; lastBufSize = bufSize; }

                        sw.Restart();
                        PingReply reply = null;
                        try { reply = localPing.Send(wifiTargetIP, timeout, localBuffer, localOptions); }
                        catch { Interlocked.Increment(ref failureCount); }
                        finally { sw.Stop(); }

                        if (reply != null && reply.Status == IPStatus.Success) {
                            Interlocked.Exchange(ref failureCount, 0);
                            SafeInvoke(() => {
                                if (lblWifiStat != null) { lblWifiStat.Text = "Online"; lblWifiStat.ForeColor = Color.Lime; }
                                if (lblWifiPing != null) lblWifiPing.Text = "Ping: " + reply.RoundtripTime + " ms";
                            });
                            int wait = timeout - (int)sw.ElapsedMilliseconds;
                            if (wait > 0) Thread.Sleep(wait);
                        } else {
                            Interlocked.Increment(ref failureCount);
                             SafeInvoke(() => {
                                if (lblWifiStat != null) { lblWifiStat.Text = "HATA: " + failureCount + "/" + maxFail; lblWifiStat.ForeColor = Color.Red; }
                            });

                            if (failureCount >= maxFail) {
                                bool shouldReset = false;
                                lock (resetLock) { if (!isResetting) { isResetting = true; shouldReset = true; Interlocked.Exchange(ref failureCount, 0); } }
                                if (shouldReset) PerformReset(ct);
                            }
                        }
                    } catch { Thread.Sleep(ERROR_SLEEP_MS); }
                }
            }
        }

        private void PerformReset(CancellationToken ct) {
             Interlocked.Increment(ref totalDisconnects);
             SafeInvoke(() => { if (lblWifiStat != null) lblWifiStat.Text = "RESETLENIYOR..."; });
             try {
                 if (userDisabledAutoConfig) {
                     RunFastNetsh("wlan set autoconfig enabled=yes interface=\"" + activeAdapter + "\"");
                     Thread.Sleep(AUTOCONFIG_WAIT_MS);
                 }
                 NativeWifiDisconnect().Wait(500);
                 Thread.Sleep(DISCONNECT_WAIT_MS);
                 var t = NativeWifiConnect();
                 t.Wait(1000);
                 if (!t.Result) {
                     RunFastNetsh("wlan connect name=\"" + currentSSID + "\"");
                     Thread.Sleep(FALLBACK_WAIT_MS);
                 }
                 if (userDisabledAutoConfig) {
                     Thread.Sleep(AUTOCONFIG_OFF_WAIT_MS);
                     RunFastNetsh("wlan set autoconfig enabled=no interface=\"" + activeAdapter + "\"");
                 }
                 if (cachedLoggingEnabled) AddWifiLog("KOPMA #" + totalDisconnects + " - RESET TAMAMLANDI");
             } finally {
                 lock(resetLock) { isResetting = false; }
             }
        }

        private void ManualReset() {
             if (userDisabledAutoConfig) RunFastNetsh(string.Format("wlan set autoconfig enabled=yes interface=\"{0}\"", activeAdapter));
             SafeInvoke(() => { if (lblWifiStat != null) lblWifiStat.Text = "Resetleniyor..."; });
             NativeWifiDisconnect().Wait(500);
             Thread.Sleep(50);
             NativeWifiConnect().Wait(1000);
             if (userDisabledAutoConfig) RunFastNetsh(string.Format("wlan set autoconfig enabled=no interface=\"{0}\"", activeAdapter));
             SafeMessageBox("Hizli Reset Tamamlandi!");
        }

        private void WifiSignalLoop(object s, EventArgs e) {
            if (isShuttingDown || !cachedWifiEnabled || !cachedSignalEnabled || !wifiModuleActive) return;
            if (Interlocked.CompareExchange(ref isSignalProcessing, 1, 0) == 1) return;

            Task.Run(() => {
                try {
                    string signalStr = GetWifiSignalStrength();
                    string speedStr = GetWifiSpeed();

                    int signalValue = 0;
                    int speedValue = 0;

                    if (!string.IsNullOrEmpty(signalStr)) int.TryParse(signalStr.Replace("%", ""), out signalValue);
                    if (!string.IsNullOrEmpty(speedStr)) int.TryParse(Regex.Match(speedStr, @"\d+").Value, out speedValue);

                    SafeInvoke(() => {
                        if (lblWifiSignal != null) {
                            lblWifiSignal.Text = "Sinyal: " + signalStr;
                            lblWifiSignal.ForeColor = signalValue > 70 ? Color.Lime : signalValue > 40 ? Color.Yellow : Color.Red;
                        }
                        if (lblWifiSpeed != null) {
                            lblWifiSpeed.Text = "Hiz: " + speedStr;
                            lblWifiSpeed.ForeColor = speedValue >= 100 ? Color.Cyan : Color.LightGray;
                        }
                    });
                } finally { Interlocked.Exchange(ref isSignalProcessing, 0); }
            });
        }

        private void KeepAliveLoop(object s, EventArgs e) {
             Task.Run(() => {
                 try {
                     var reply = keepAlivePing.Send(wifiTargetIP, 100, keepAliveBuffer, new PingOptions(64, true));
                     if (reply != null && reply.Status == IPStatus.Success) {
                         SafeInvoke(() => { if (pnlKeepAliveIndicator != null) pnlKeepAliveIndicator.BackColor = Color.Lime; });
                         System.Threading.Timer t = null;
                         t = new System.Threading.Timer(_ => {
                             SafeInvoke(() => { if (pnlKeepAliveIndicator != null) pnlKeepAliveIndicator.BackColor = Color.DarkGreen; });
                             if(t!=null) t.Dispose();
                         }, null, 100, Timeout.Infinite);
                     }
                 } catch {}
             });
        }

        private Task<bool> NativeWifiDisconnect() {
             return Task.Factory.StartNew(() => {
                bool success = false;
                wlanSem.Wait();
                try {
                    uint ver; IntPtr handle = IntPtr.Zero; IntPtr ifacePtr = IntPtr.Zero;
                    if(NativeMethods.WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0) {
                        if(NativeMethods.WlanEnumInterfaces(handle, IntPtr.Zero, out ifacePtr) == 0) {
                            var list = (NativeMethods.WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(ifacePtr, typeof(NativeMethods.WLAN_INTERFACE_INFO_LIST));
                            if(list.dwNumberOfItems > 0) {
                                var info = (NativeMethods.WLAN_INTERFACE_INFO)Marshal.PtrToStructure(new IntPtr(ifacePtr.ToInt64() + 8), typeof(NativeMethods.WLAN_INTERFACE_INFO));
                                NativeMethods.WlanDisconnect(handle, ref info.InterfaceGuid, IntPtr.Zero);
                                success = true;
                            }
                        }
                    }
                    if (ifacePtr != IntPtr.Zero) NativeMethods.WlanFreeMemory(ifacePtr);
                    if (handle != IntPtr.Zero) NativeMethods.WlanCloseHandle(handle, IntPtr.Zero);
                } catch { } finally { wlanSem.Release(); }
                return success;
            });
        }

        private Task<bool> NativeWifiConnect() {
            return Task.Factory.StartNew(() => {
                bool success = false;
                wlanSem.Wait();
                try {
                    uint ver; IntPtr handle = IntPtr.Zero;
                    if(NativeMethods.WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0 && activeGuid != Guid.Empty) {
                         IntPtr profilePtr = Marshal.StringToHGlobalUni(currentSSID);
                         NativeMethods.WLAN_CONNECTION_PARAMETERS param = new NativeMethods.WLAN_CONNECTION_PARAMETERS();
                         param.wlanConnectionMode = 0;
                         param.strProfile = profilePtr;
                         param.dot11BssType = 1;
                         Guid tempGuid = activeGuid;
                         uint result = NativeMethods.WlanConnect(handle, ref tempGuid, ref param, IntPtr.Zero);
                         success = (result == 0);
                         Marshal.FreeHGlobal(profilePtr);
                    }
                    if (handle != IntPtr.Zero) NativeMethods.WlanCloseHandle(handle, IntPtr.Zero);
                } catch { } finally { wlanSem.Release(); }
                return success;
            });
        }

        private string GetSSID() {
            try {
                uint ver; IntPtr handle = IntPtr.Zero; IntPtr ifacePtr = IntPtr.Zero;
                if(NativeMethods.WlanOpenHandle(2, IntPtr.Zero, out ver, out handle) == 0) {
                    if(NativeMethods.WlanEnumInterfaces(handle, IntPtr.Zero, out ifacePtr) == 0) {
                        var list = (NativeMethods.WLAN_INTERFACE_INFO_LIST)Marshal.PtrToStructure(ifacePtr, typeof(NativeMethods.WLAN_INTERFACE_INFO_LIST));
                        if(list.dwNumberOfItems > 0) {
                            var info = (NativeMethods.WLAN_INTERFACE_INFO)Marshal.PtrToStructure(new IntPtr(ifacePtr.ToInt64() + 8), typeof(NativeMethods.WLAN_INTERFACE_INFO));
                            uint sz; IntPtr dataPtr = IntPtr.Zero;
                            Guid tempGuid = info.InterfaceGuid;
                            if(NativeMethods.WlanQueryInterface(handle, ref tempGuid, NativeMethods.WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection, IntPtr.Zero, out sz, out dataPtr, IntPtr.Zero) == 0) {
                                if (dataPtr != IntPtr.Zero) {
                                    var attr = (NativeMethods.WLAN_CONNECTION_ATTRIBUTES)Marshal.PtrToStructure(dataPtr, typeof(NativeMethods.WLAN_CONNECTION_ATTRIBUTES));
                                    byte[] ssidBytes = attr.wlanAssociationAttributes.dot11Ssid.ucSSID;
                                    int ssidLen = (int)attr.wlanAssociationAttributes.dot11Ssid.uSSIDLength;
                                    if (ssidLen > 0 && ssidLen <= 32) {
                                        string ssid = Encoding.UTF8.GetString(ssidBytes, 0, ssidLen);
                                        NativeMethods.WlanFreeMemory(dataPtr);
                                        NativeMethods.WlanCloseHandle(handle, IntPtr.Zero);
                                        NativeMethods.WlanFreeMemory(ifacePtr);
                                        return ssid;
                                    }
                                }
                            }
                        }
                    }
                    if (ifacePtr != IntPtr.Zero) NativeMethods.WlanFreeMemory(ifacePtr);
                    NativeMethods.WlanCloseHandle(handle, IntPtr.Zero);
                }
            } catch { }
            return "Bulunamadi";
        }

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
                p.WaitForExit(2000);
                if (!p.HasExited) p.Kill();

                Match match = Regex.Match(output, @"Signal\s*:\s*(\d+)%", RegexOptions.IgnoreCase);
                if (!match.Success) match = Regex.Match(output, @"Sinyal\s*:\s*(\d+)%", RegexOptions.IgnoreCase);
                if (match.Success) return match.Groups[1].Value + "%";
            } catch { } finally { if(p!=null) p.Dispose(); }
            return "0%";
        }

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
                p.WaitForExit(2000);
                if (!p.HasExited) p.Kill();

                Match match = Regex.Match(output, @"Receive rate.*?:\s*(\d+)", RegexOptions.IgnoreCase);
                if (!match.Success) match = Regex.Match(output, @"Alma h[ıi]z[ıi].*?:\s*(\d+)", RegexOptions.IgnoreCase);
                if (match.Success) return match.Groups[1].Value + " Mbps";
            } catch { } finally { if(p!=null) p.Dispose(); }
            return "0 Mbps";
        }

        private void RunFastNetsh(string args) {
             try {
                 ProcessStartInfo psi = new ProcessStartInfo {
                     FileName = "netsh.exe", Arguments = args, CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden
                 };
                 Process.Start(psi).WaitForExit(2000);
             } catch {}
        }

        private void AddWifiLog(string msg) {
            if (! cachedLoggingEnabled) return;
            string logMsg = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, msg);
            while (wifiLogBuffer.Count >= 250) { string temp; wifiLogBuffer.TryDequeue(out temp); }
            wifiLogBuffer.Enqueue(logMsg);
            if (! isHidden && this.Visible) {
                SafeInvoke(() => {
                    if (lstWifiLog != null && ! lstWifiLog.IsDisposed) {
                        lstWifiLog.Items.Insert(0, logMsg);
                        if (lstWifiLog.Items.Count > 250) lstWifiLog.Items.RemoveAt(lstWifiLog.Items.Count - 1);
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
                lstWifiLog.Items.Clear();
                lstWifiLog.Items.AddRange(snapshot);
                lstWifiLog.EndUpdate();
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

                wifiTimeout = timeout;
                wifiFailureThreshold = threshold;
                wifiTTL = ttl;
                wifiBufferSize = buffer;
                signalUpdateInterval = signalInt;

                if (wifiSignalTimer != null) {
                    bool wasEnabled = wifiSignalTimer.Enabled;
                    wifiSignalTimer.Stop();
                    if (signalUpdateInterval >= 1000) wifiSignalTimer.Interval = signalUpdateInterval;
                    if (wasEnabled) wifiSignalTimer.Start();
                }
                SafeMessageBox("Ayarlar kaydedildi.");
            } catch (Exception ex) { SafeMessageBox("Kaydetme hatasi: " + ex.Message); }
        }
    }
}
