using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SystemManager.Modules.NetworkMonitor
{
    /// <summary>
    /// Network Monitor Module - Displays signal strength, speeds, and stability
    /// Dashboard with real-time monitoring (more stable, not just 2-3 signals)
    /// </summary>
    public class NetworkMonitorModule : IModule
    {
        private SignalStrengthMonitor _monitor;
        private UserControl _control;
        private System.Windows.Forms.Timer _updateTimer;
        
        // UI Controls
        private Label _downloadLabel;
        private Label _uploadLabel;
        private Label _signalLabel;
        private Label _stabilityLabel;
        private Label _jitterLabel;
        private ProgressBar _signalBar;
        private ProgressBar _stabilityBar;
        private Panel _graphPanel;

        public string ModuleName => "Network Monitor";
        public string Description => "Real-time network monitoring with signal strength, speed, and stability tracking";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _monitor = new SignalStrengthMonitor();
            CreateControl();
        }

        public void Start()
        {
            _monitor?.Start();
            _updateTimer = new System.Windows.Forms.Timer { Interval = 500 }; // Update UI twice per second
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        public void Stop()
        {
            _updateTimer?.Stop();
            _monitor?.Stop();
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
            _monitor?.Stop();
            _control?.Dispose();
        }

        public Control GetControl()
        {
            return _control;
        }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill, BackColor = Color.White };

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Statistics Panel
            var statsPanel = CreateStatsPanel();
            mainPanel.Controls.Add(statsPanel, 0, 0);
            mainPanel.SetRowSpan(statsPanel, 2);

            // Signal and Stability Panel
            var signalPanel = CreateSignalPanel();
            mainPanel.Controls.Add(signalPanel, 1, 0);

            var stabilityPanel = CreateStabilityPanel();
            mainPanel.Controls.Add(stabilityPanel, 1, 1);

            // Graph Panel
            _graphPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            _graphPanel.Paint += GraphPanel_Paint;
            mainPanel.Controls.Add(_graphPanel, 0, 2);
            mainPanel.SetColumnSpan(_graphPanel, 2);

            _control.Controls.Add(mainPanel);
        }

        private Panel CreateStatsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            var title = new Label
            {
                Text = "Network Statistics",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };

            _downloadLabel = new Label
            {
                Text = "Download: 0 B/s",
                Font = new Font("Arial", 10),
                Location = new Point(10, 40),
                AutoSize = true
            };

            _uploadLabel = new Label
            {
                Text = "Upload: 0 B/s",
                Font = new Font("Arial", 10),
                Location = new Point(10, 70),
                AutoSize = true
            };

            _jitterLabel = new Label
            {
                Text = "Jitter: 0 ms",
                Font = new Font("Arial", 10),
                Location = new Point(10, 100),
                AutoSize = true
            };

            panel.Controls.Add(title);
            panel.Controls.Add(_downloadLabel);
            panel.Controls.Add(_uploadLabel);
            panel.Controls.Add(_jitterLabel);

            return panel;
        }

        private Panel CreateSignalPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            var title = new Label
            {
                Text = "Signal Strength",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };

            _signalBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Minimum = 0,
                Maximum = 100
            };

            _signalLabel = new Label
            {
                Text = "N/A",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(_signalLabel);
            panel.Controls.Add(_signalBar);
            panel.Controls.Add(title);

            return panel;
        }

        private Panel CreateStabilityPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };

            var title = new Label
            {
                Text = "Connection Stability",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25
            };

            _stabilityBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 25,
                Minimum = 0,
                Maximum = 100
            };

            _stabilityLabel = new Label
            {
                Text = "0%",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(_stabilityLabel);
            panel.Controls.Add(_stabilityBar);
            panel.Controls.Add(title);

            return panel;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_monitor == null) return;

            try
            {
                var stats = _monitor.GetCurrentStatistics();

                _downloadLabel.Text = $"Download: {stats.FormatSpeed(stats.CurrentDownloadSpeed)} (Avg: {stats.FormatSpeed(stats.AverageDownloadSpeed)})";
                _uploadLabel.Text = $"Upload: {stats.FormatSpeed(stats.CurrentUploadSpeed)} (Avg: {stats.FormatSpeed(stats.AverageUploadSpeed)})";
                _jitterLabel.Text = $"Jitter: {stats.Jitter:F2} ms";

                if (stats.CurrentSignalStrength >= 0)
                {
                    _signalBar.Value = stats.CurrentSignalStrength;
                    _signalLabel.Text = $"{stats.CurrentSignalStrength}%";
                }
                else
                {
                    _signalLabel.Text = "N/A (Not WiFi)";
                }

                _stabilityBar.Value = (int)stats.Stability;
                _stabilityLabel.Text = $"{stats.Stability:F1}%";

                _graphPanel.Invalidate(); // Trigger repaint
            }
            catch
            {
                // Handle errors silently
            }
        }

        private void GraphPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_monitor == null) return;

            try
            {
                var samples = _monitor.GetSamples(60); // Last 60 seconds
                if (samples.Count < 2) return;

                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var width = _graphPanel.Width - 40;
                var height = _graphPanel.Height - 40;
                var margin = 20;

                // Draw background grid
                using (var gridPen = new Pen(Color.LightGray))
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        int y = margin + (height * i / 5);
                        g.DrawLine(gridPen, margin, y, margin + width, y);
                    }
                }

                // Find max speed for scaling
                var maxSpeed = samples.Max(s => Math.Max(s.DownloadSpeed, s.UploadSpeed));
                if (maxSpeed == 0) maxSpeed = 1;

                // Draw download speed line
                using (var downloadPen = new Pen(Color.Blue, 2))
                {
                    for (int i = 1; i < samples.Count; i++)
                    {
                        int x1 = margin + (width * (i - 1) / (samples.Count - 1));
                        int x2 = margin + (width * i / (samples.Count - 1));
                        int y1 = margin + height - (int)(height * samples[i - 1].DownloadSpeed / maxSpeed);
                        int y2 = margin + height - (int)(height * samples[i].DownloadSpeed / maxSpeed);
                        g.DrawLine(downloadPen, x1, y1, x2, y2);
                    }
                }

                // Draw upload speed line
                using (var uploadPen = new Pen(Color.Red, 2))
                {
                    for (int i = 1; i < samples.Count; i++)
                    {
                        int x1 = margin + (width * (i - 1) / (samples.Count - 1));
                        int x2 = margin + (width * i / (samples.Count - 1));
                        int y1 = margin + height - (int)(height * samples[i - 1].UploadSpeed / maxSpeed);
                        int y2 = margin + height - (int)(height * samples[i].UploadSpeed / maxSpeed);
                        g.DrawLine(uploadPen, x1, y1, x2, y2);
                    }
                }

                // Draw legend
                g.DrawString("Download", new Font("Arial", 8), Brushes.Blue, margin, 5);
                g.DrawString("Upload", new Font("Arial", 8), Brushes.Red, margin + 80, 5);
            }
            catch
            {
                // Handle painting errors silently
            }
        }
    }
}
