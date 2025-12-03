using System;
using System.Windows.Forms;

namespace SystemManager.Modules.ActivityWatcher
{
    /// <summary>
    /// Activity Watcher Module - Monitors browser URLs and tracks domain-based activity
    /// Shows daily logs with active time per domain (e.g., TradingView usage)
    /// </summary>
    public class ActivityWatcherModule : IModule
    {
        private BrowserMonitor _browserMonitor;
        private UserControl _control;
        private ListView _listView;
        private Label _uptimeLabel;
        private System.Windows.Forms.Timer _updateTimer;

        public string ModuleName => "Activity Watcher";
        public string Description => "Monitors browser activity and tracks domain usage with daily logs";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _browserMonitor = new BrowserMonitor();
            CreateControl();
        }

        public void Start()
        {
            _browserMonitor?.Start();
            _updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        public void Stop()
        {
            _updateTimer?.Stop();
            _browserMonitor?.Stop();
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
            _browserMonitor?.Stop();
            _control?.Dispose();
        }

        public Control GetControl()
        {
            return _control;
        }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill };

            var panel = new Panel { Dock = DockStyle.Fill };

            _uptimeLabel = new Label
            {
                Text = "Session Uptime: 00:00:00",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10)
            };

            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            _listView.Columns.Add("Domain", 250);
            _listView.Columns.Add("Total Time", 120);
            _listView.Columns.Add("Visits", 80);
            _listView.Columns.Add("First Seen", 150);
            _listView.Columns.Add("Last Seen", 150);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(10, 8),
                Width = 100
            };
            refreshButton.Click += (s, e) => RefreshData();

            var exportButton = new Button
            {
                Text = "Export Logs",
                Location = new System.Drawing.Point(120, 8),
                Width = 100
            };
            exportButton.Click += (s, e) => ExportLogs();

            buttonPanel.Controls.Add(refreshButton);
            buttonPanel.Controls.Add(exportButton);

            panel.Controls.Add(_listView);
            panel.Controls.Add(_uptimeLabel);
            panel.Controls.Add(buttonPanel);

            _control.Controls.Add(panel);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_browserMonitor != null)
            {
                var uptime = _browserMonitor.GetSessionUptime();
                _uptimeLabel.Text = $"Session Uptime: {uptime:hh\\:mm\\:ss}";
            }
        }

        private void RefreshData()
        {
            if (_browserMonitor == null) return;

            _listView.Items.Clear();
            var activities = _browserMonitor.GetDomainActivities();

            foreach (var activity in activities.Values)
            {
                var item = new ListViewItem(activity.Domain);
                item.SubItems.Add(activity.TotalTime.ToString(@"hh\:mm\:ss"));
                item.SubItems.Add(activity.VisitCount.ToString());
                item.SubItems.Add(activity.FirstSeen.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(activity.LastSeen.ToString("yyyy-MM-dd HH:mm:ss"));
                _listView.Items.Add(item);
            }
        }

        private void ExportLogs()
        {
            if (_browserMonitor == null) return;

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv",
                    DefaultExt = "txt",
                    FileName = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var logs = _browserMonitor.GetDailyLogs();
                    var content = new System.Text.StringBuilder();

                    content.AppendLine("Activity Watcher - Daily Logs");
                    content.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    content.AppendLine(new string('=', 80));

                    foreach (var log in logs)
                    {
                        content.AppendLine($"\nDate: {log.Date:yyyy-MM-dd}");
                        content.AppendLine($"Total Active Time: {log.TotalActiveTime:hh\\:mm\\:ss}");
                        content.AppendLine("Domains:");

                        foreach (var domain in log.Domains)
                        {
                            content.AppendLine($"  - {domain.Key}: {domain.Value:hh\\:mm\\:ss}");
                        }
                    }

                    System.IO.File.WriteAllText(saveDialog.FileName, content.ToString());
                    MessageBox.Show("Logs exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
