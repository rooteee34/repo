using System;
using System.Drawing;
using System.Windows.Forms;
using SystemManager.Modules;
using SystemManager.Modules.ActivityWatcher;
using SystemManager.Modules.NetworkMonitor;
using SystemManager.Modules.WiFiGuardian;
using SystemManager.Modules.StartupManager;
using SystemManager.Modules.SystemCleaning;
using SystemManager.Modules.Uninstaller;
using SystemManager.Modules.Security;
using SystemManager.Modules.Optimization;
using SystemManager.Modules.PasswordManager;
using SystemManager.Modules.SystemInfo;
using SystemManager.Modules.Alarms;
using SystemManager.Modules.VPN;
using SystemManager.Modules.Terminal;
using SystemManager.Modules.FileTransfer;
using SystemManager.Modules.DNS;
using SystemManager.Modules.PacketAnalyzer;
using SystemManager.Modules.Logging;

namespace SystemManager
{
    public partial class MainForm : Form
    {
        private ModuleManager _moduleManager;
        private TabControl _mainTabControl;
        private NotifyIcon _notifyIcon;
        private CheckBox _startWithWindowsCheckbox;

        public MainForm()
        {
            InitializeComponent();
            InitializeModules();
            SetupUI();
            SetupTrayIcon();
        }

        private void InitializeComponent()
        {
            this.Text = "System Manager - Comprehensive System Management";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;
        }

        private void InitializeModules()
        {
            _moduleManager = new ModuleManager();

            // Register all modules
            _moduleManager.RegisterModule(new ActivityWatcherModule());
            _moduleManager.RegisterModule(new NetworkMonitorModule());
            _moduleManager.RegisterModule(new WiFiGuardianModule());
            _moduleManager.RegisterModule(new StartupManagerModule());
            _moduleManager.RegisterModule(new SystemCleaningModule());
            _moduleManager.RegisterModule(new DeepUninstallerModule());
            _moduleManager.RegisterModule(new SecurityModule());
            _moduleManager.RegisterModule(new OptimizationModule());
            _moduleManager.RegisterModule(new PasswordManagerModule());
            _moduleManager.RegisterModule(new SystemInfoModule());
            _moduleManager.RegisterModule(new AlarmModule());
            _moduleManager.RegisterModule(new VPNModule());
            _moduleManager.RegisterModule(new TerminalModule());
            _moduleManager.RegisterModule(new FileTransferModule());
            _moduleManager.RegisterModule(new SmartDNSModule());
            _moduleManager.RegisterModule(new PacketAnalyzerModule());
            _moduleManager.RegisterModule(new LoggingModule());
        }

        private void SetupUI()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // Top control panel
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(240, 240, 240) };
            
            var titleLabel = new Label
            {
                Text = "System Manager",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(10, 12),
                AutoSize = true
            };

            _startWithWindowsCheckbox = new CheckBox
            {
                Text = "Start with Windows",
                Location = new Point(250, 15),
                AutoSize = true
            };
            _startWithWindowsCheckbox.CheckedChanged += StartWithWindowsCheckbox_CheckedChanged;

            var exitButton = new Button
            {
                Text = "Exit",
                Location = new Point(420, 12),
                Width = 80,
                Height = 25
            };
            exitButton.Click += ExitButton_Click;

            topPanel.Controls.Add(titleLabel);
            topPanel.Controls.Add(_startWithWindowsCheckbox);
            topPanel.Controls.Add(exitButton);

            // Module selection panel
            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(250, 250, 250) };
            var moduleListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10)
            };

            foreach (var module in _moduleManager.GetAllModules())
            {
                moduleListBox.Items.Add(module.ModuleName);
            }

            moduleListBox.SelectedIndexChanged += ModuleListBox_SelectedIndexChanged;
            leftPanel.Controls.Add(moduleListBox);

            // Main content panel
            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Add all modules as tabs
            foreach (var module in _moduleManager.GetAllModules())
            {
                _moduleManager.EnableModule(module.ModuleName);
                var tabPage = new TabPage(module.ModuleName);
                tabPage.Controls.Add(module.GetControl());
                _mainTabControl.TabPages.Add(tabPage);
            }

            mainPanel.Controls.Add(_mainTabControl);
            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(topPanel);

            this.Controls.Add(mainPanel);

            // Set initial selection
            if (moduleListBox.Items.Count > 0)
            {
                moduleListBox.SelectedIndex = 0;
            }
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "System Manager",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            contextMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
        }

        private void ModuleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedIndex >= 0 && listBox.SelectedIndex < _mainTabControl.TabPages.Count)
            {
                _mainTabControl.SelectedIndex = listBox.SelectedIndex;
            }
        }

        private void StartWithWindowsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (_startWithWindowsCheckbox.Checked)
                {
                    startupKey?.SetValue("SystemManager", Application.ExecutablePath);
                }
                else
                {
                    startupKey?.DeleteValue("SystemManager", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                _notifyIcon.ShowBalloonTip(2000, "System Manager", "Application minimized to tray", ToolTipIcon.Info);
            }
            else
            {
                _moduleManager?.DisposeAll();
                _notifyIcon?.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}
