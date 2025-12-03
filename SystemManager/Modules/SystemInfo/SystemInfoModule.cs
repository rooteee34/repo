using System;
using System.Windows.Forms;

namespace SystemManager.Modules.SystemInfo
{
    public class SystemInfoModule : IModule
    {
        private UserControl _control;
        private HardwareMonitor _monitor;
        private Label _cpuLabel;
        private Label _ramLabel;
        private Label _gpuLabel;
        private Label _osLabel;
        private Label _uptimeLabel;
        private Label _localIPLabel;
        private Label _externalIPLabel;
        private Label _dnsLabel;
        private Label _cpuTempLabel;
        private System.Windows.Forms.Timer _updateTimer;

        public string ModuleName => "System Info";
        public string Description => "Display system hardware information and temperatures";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _monitor = new HardwareMonitor();
            CreateControl();
        }

        public void Start()
        {
            RefreshInfo();
            _updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        public void Stop()
        {
            _updateTimer?.Stop();
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
            _control?.Dispose();
        }

        public Control GetControl()
        {
            return _control;
        }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var yPos = 10;
            var labelHeight = 30;

            _cpuLabel = CreateInfoLabel("CPU: Loading...", yPos);
            yPos += labelHeight;
            _cpuTempLabel = CreateInfoLabel("CPU Temperature: Loading...", yPos);
            yPos += labelHeight;
            _ramLabel = CreateInfoLabel("RAM: Loading...", yPos);
            yPos += labelHeight;
            _gpuLabel = CreateInfoLabel("GPU: Loading...", yPos);
            yPos += labelHeight;
            _osLabel = CreateInfoLabel("OS: Loading...", yPos);
            yPos += labelHeight;
            _uptimeLabel = CreateInfoLabel("Uptime: Loading...", yPos);
            yPos += labelHeight;
            _localIPLabel = CreateInfoLabel("Local IP: Loading...", yPos);
            yPos += labelHeight;
            _externalIPLabel = CreateInfoLabel("External IP: Loading...", yPos);
            yPos += labelHeight;
            _dnsLabel = CreateInfoLabel("DNS: Loading...", yPos);

            panel.Controls.Add(_cpuLabel);
            panel.Controls.Add(_cpuTempLabel);
            panel.Controls.Add(_ramLabel);
            panel.Controls.Add(_gpuLabel);
            panel.Controls.Add(_osLabel);
            panel.Controls.Add(_uptimeLabel);
            panel.Controls.Add(_localIPLabel);
            panel.Controls.Add(_externalIPLabel);
            panel.Controls.Add(_dnsLabel);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(20, yPos + 30),
                Width = 100
            };
            refreshButton.Click += (s, e) => RefreshInfo();
            panel.Controls.Add(refreshButton);

            _control.Controls.Add(panel);
        }

        private Label CreateInfoLabel(string text, int yPos)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(10, yPos),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 10)
            };
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var info = _monitor.GetHardwareInfo();
            _uptimeLabel.Text = $"Uptime: {info.Uptime:dd\\.hh\\:mm\\:ss}";
            
            var temp = _monitor.GetCPUTemperature();
            if (temp > 0)
            {
                _cpuTempLabel.Text = $"CPU Temperature: {temp}°C";
            }
        }

        private void RefreshInfo()
        {
            var info = _monitor.GetHardwareInfo();
            
            _cpuLabel.Text = $"CPU: {info.CPUInfo}";
            _ramLabel.Text = $"RAM: {info.RAMInfo}";
            _gpuLabel.Text = $"GPU: {info.GPUInfo}";
            _osLabel.Text = $"OS: {info.OSInfo}";
            _uptimeLabel.Text = $"Uptime: {info.Uptime:dd\\.hh\\:mm\\:ss}";
            _localIPLabel.Text = $"Local IP: {info.LocalIP}";
            _externalIPLabel.Text = $"External IP: {info.ExternalIP}";
            _dnsLabel.Text = $"DNS: {info.DNS}";

            var temp = _monitor.GetCPUTemperature();
            _cpuTempLabel.Text = temp > 0 ? $"CPU Temperature: {temp}°C" : "CPU Temperature: N/A";
        }
    }
}
