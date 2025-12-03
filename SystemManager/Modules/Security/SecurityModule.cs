using System;
using System.Windows.Forms;
using SystemManager.Utilities;

namespace SystemManager.Modules.Security
{
    public class SecurityModule : IModule
    {
        private UserControl _control;
        private AntivirusScanner _scanner;
        private ProgressBar _scanProgress;
        private Label _statusLabel;
        private ListView _threatsListView;
        private ListView _blockedConnectionsListView;
        private CheckBox _autoScanCheckbox;
        private ComboBox _scanTypeComboBox;

        public string ModuleName => "Security";
        public string Description => "Security scanner with firewall monitoring";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _scanner = new AntivirusScanner();
            _scanner.ScanProgress += Scanner_ScanProgress;
            _scanner.ThreatFound += Scanner_ThreatFound;
            CreateControl();
        }

        public void Start() { }
        public void Stop() { }

        public void Dispose()
        {
            _control?.Dispose();
        }

        public Control GetControl()
        {
            return _control;
        }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill };

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            // Antivirus Tab
            var avTab = new TabPage("Antivirus");
            CreateAntivirusTab(avTab);
            tabControl.TabPages.Add(avTab);

            // Firewall Tab
            var firewallTab = new TabPage("Firewall Monitor");
            CreateFirewallTab(firewallTab);
            tabControl.TabPages.Add(firewallTab);

            _control.Controls.Add(tabControl);
        }

        private void CreateAntivirusTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            _autoScanCheckbox = new CheckBox
            {
                Text = "Enable Auto-Scan",
                Location = new System.Drawing.Point(10, 10),
                AutoSize = true
            };

            _scanTypeComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(10, 40),
                Width = 150
            };
            _scanTypeComboBox.Items.AddRange(new object[] { "Quick Scan", "Medium Scan", "Deep Scan" });
            _scanTypeComboBox.SelectedIndex = 0;

            var scanButton = new Button
            {
                Text = "Start Scan",
                Location = new System.Drawing.Point(170, 40),
                Width = 100
            };
            scanButton.Click += ScanButton_Click;

            _scanProgress = new ProgressBar
            {
                Location = new System.Drawing.Point(10, 80),
                Size = new System.Drawing.Size(500, 23)
            };

            _statusLabel = new Label
            {
                Text = "Ready",
                Location = new System.Drawing.Point(10, 110),
                AutoSize = true
            };

            _threatsListView = new ListView
            {
                Location = new System.Drawing.Point(10, 140),
                Size = new System.Drawing.Size(600, 200),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _threatsListView.Columns.Add("File", 300);
            _threatsListView.Columns.Add("Threat", 200);
            _threatsListView.Columns.Add("Level", 100);

            panel.Controls.Add(_autoScanCheckbox);
            panel.Controls.Add(_scanTypeComboBox);
            panel.Controls.Add(scanButton);
            panel.Controls.Add(_scanProgress);
            panel.Controls.Add(_statusLabel);
            panel.Controls.Add(_threatsListView);

            tab.Controls.Add(panel);
        }

        private void CreateFirewallTab(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            _blockedConnectionsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _blockedConnectionsListView.Columns.Add("Time", 150);
            _blockedConnectionsListView.Columns.Add("Source", 150);
            _blockedConnectionsListView.Columns.Add("Destination", 150);
            _blockedConnectionsListView.Columns.Add("Protocol", 100);
            _blockedConnectionsListView.Columns.Add("Action", 100);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var refreshButton = new Button
            {
                Text = "Refresh Connections",
                Location = new System.Drawing.Point(10, 8),
                Width = 150
            };
            refreshButton.Click += (s, e) => RefreshConnections();

            buttonPanel.Controls.Add(refreshButton);

            panel.Controls.Add(_blockedConnectionsListView);
            panel.Controls.Add(buttonPanel);

            tab.Controls.Add(panel);
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            _threatsListView.Items.Clear();
            _scanProgress.Value = 0;
            _statusLabel.Text = "Scanning...";

            var scanType = _scanTypeComboBox.SelectedItem.ToString();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (scanType.Contains("Quick"))
            {
                _scanner.StartQuickScan(path);
            }
            else
            {
                _scanner.StartDeepScan(path);
            }

            _statusLabel.Text = "Scan Complete";
        }

        private void Scanner_ScanProgress(object sender, ScanProgressEventArgs e)
        {
            if (_scanProgress.InvokeRequired)
            {
                _scanProgress.Invoke((Action)(() =>
                {
                    _scanProgress.Value = e.Progress;
                    _statusLabel.Text = $"Scanning: {Path.GetFileName(e.CurrentFile)}";
                }));
            }
        }

        private void Scanner_ThreatFound(object sender, ThreatFoundEventArgs e)
        {
            if (_threatsListView.InvokeRequired)
            {
                _threatsListView.Invoke((Action)(() =>
                {
                    var item = new ListViewItem(e.FilePath);
                    item.SubItems.Add(e.ThreatName);
                    item.SubItems.Add(e.ThreatLevel);
                    _threatsListView.Items.Add(item);
                }));
            }
        }

        private void RefreshConnections()
        {
            _blockedConnectionsListView.Items.Clear();
            var connections = NetworkHelper.GetActiveTCPConnections();

            foreach (var conn in connections)
            {
                var item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));
                item.SubItems.Add(conn.LocalEndPoint);
                item.SubItems.Add(conn.RemoteEndPoint);
                item.SubItems.Add(conn.Protocol);
                item.SubItems.Add(conn.State);
                _blockedConnectionsListView.Items.Add(item);
            }
        }
    }
}
