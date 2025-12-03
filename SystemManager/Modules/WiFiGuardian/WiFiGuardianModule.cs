using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace SystemManager.Modules.WiFiGuardian
{
    /// <summary>
    /// WiFi Guardian Module - Monitors WiFi connection
    /// Only active when checkbox is enabled (not auto-start on program launch)
    /// </summary>
    public class WiFiGuardianModule : IModule
    {
        private UserControl _control;
        private System.Timers.Timer _guardianTimer;
        private CheckBox _enableCheckbox;
        private Label _statusLabel;
        private ListView _logListView;
        private bool _isMonitoring;

        public string ModuleName => "WiFi Guardian";
        public string Description => "Monitors WiFi connection and alerts on disconnections";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _isMonitoring = false; // Explicitly start as disabled
            CreateControl();
        }

        public void Start()
        {
            // Module loaded but monitoring NOT started automatically
            // Will only start when checkbox is checked
        }

        public void Stop()
        {
            StopMonitoring();
        }

        public void Dispose()
        {
            StopMonitoring();
            _guardianTimer?.Dispose();
            _control?.Dispose();
        }

        public Control GetControl()
        {
            return _control;
        }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            _enableCheckbox = new CheckBox
            {
                Text = "Enable WiFi Guardian",
                Location = new System.Drawing.Point(10, 10),
                AutoSize = true,
                Checked = false // Start unchecked
            };
            _enableCheckbox.CheckedChanged += EnableCheckbox_CheckedChanged;

            _statusLabel = new Label
            {
                Text = "Status: Disabled",
                Location = new System.Drawing.Point(10, 40),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };

            _logListView = new ListView
            {
                Location = new System.Drawing.Point(10, 70),
                Size = new System.Drawing.Size(600, 300),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            _logListView.Columns.Add("Time", 150);
            _logListView.Columns.Add("Event", 200);
            _logListView.Columns.Add("Details", 250);

            panel.Controls.Add(_enableCheckbox);
            panel.Controls.Add(_statusLabel);
            panel.Controls.Add(_logListView);

            _control.Controls.Add(panel);
        }

        private void EnableCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_enableCheckbox.Checked)
            {
                StartMonitoring();
            }
            else
            {
                StopMonitoring();
            }
        }

        private void StartMonitoring()
        {
            if (_isMonitoring) return;

            _isMonitoring = true;
            _statusLabel.Text = "Status: Monitoring WiFi...";
            AddLog("WiFi Guardian", "Monitoring started");

            _guardianTimer = new System.Timers.Timer(2000); // Check every 2 seconds
            _guardianTimer.Elapsed += GuardianTimer_Elapsed;
            _guardianTimer.Start();
        }

        private void StopMonitoring()
        {
            if (!_isMonitoring) return;

            _isMonitoring = false;
            _guardianTimer?.Stop();
            _guardianTimer?.Dispose();
            _guardianTimer = null;

            if (_statusLabel != null)
            {
                _statusLabel.Invoke((Action)(() => _statusLabel.Text = "Status: Disabled"));
            }
            AddLog("WiFi Guardian", "Monitoring stopped");
        }

        private void GuardianTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                bool isConnected = CheckWiFiConnection();
                
                if (!isConnected)
                {
                    if (_statusLabel != null)
                    {
                        _statusLabel.Invoke((Action)(() =>
                        {
                            _statusLabel.Text = "Status: WiFi Disconnected!";
                            _statusLabel.ForeColor = System.Drawing.Color.Red;
                        }));
                    }
                    AddLog("WiFi Status", "WiFi connection lost!");
                }
                else
                {
                    if (_statusLabel != null)
                    {
                        _statusLabel.Invoke((Action)(() =>
                        {
                            _statusLabel.Text = "Status: WiFi Connected";
                            _statusLabel.ForeColor = System.Drawing.Color.Green;
                        }));
                    }
                }
            }
            catch
            {
                // Handle errors silently
            }
        }

        private bool CheckWiFiConnection()
        {
            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                        adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Handle exceptions
            }

            return false;
        }

        private void AddLog(string eventType, string details)
        {
            if (_logListView != null && !_logListView.IsDisposed)
            {
                try
                {
                    _logListView.Invoke((Action)(() =>
                    {
                        var item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        item.SubItems.Add(eventType);
                        item.SubItems.Add(details);
                        _logListView.Items.Insert(0, item);

                        // Keep only last 100 logs
                        while (_logListView.Items.Count > 100)
                        {
                            _logListView.Items.RemoveAt(_logListView.Items.Count - 1);
                        }
                    }));
                }
                catch
                {
                    // Handle invoke errors
                }
            }
        }
    }
}
