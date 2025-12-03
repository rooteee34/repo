using System;
using System.Collections.Generic;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SystemManager.Modules.Uninstaller
{
    public class DeepUninstallerModule : IModule
    {
        private UserControl _control;
        private ListView _listView;

        public string ModuleName => "Deep Uninstaller";
        public string Description => "Advanced program uninstaller with leftover cleanup";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            CreateControl();
        }

        public void Start()
        {
            RefreshInstalledPrograms();
        }

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

            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            _listView.Columns.Add("Program Name", 300);
            _listView.Columns.Add("Publisher", 200);
            _listView.Columns.Add("Version", 100);
            _listView.Columns.Add("Install Date", 100);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(10, 8),
                Width = 100
            };
            refreshButton.Click += (s, e) => RefreshInstalledPrograms();

            var uninstallButton = new Button
            {
                Text = "Deep Uninstall",
                Location = new System.Drawing.Point(120, 8),
                Width = 120
            };
            uninstallButton.Click += UninstallButton_Click;

            buttonPanel.Controls.Add(refreshButton);
            buttonPanel.Controls.Add(uninstallButton);

            _control.Controls.Add(_listView);
            _control.Controls.Add(buttonPanel);
        }

        private void RefreshInstalledPrograms()
        {
            _listView.Items.Clear();

            var programs = GetInstalledPrograms();
            foreach (var program in programs)
            {
                var item = new ListViewItem(program.Name);
                item.SubItems.Add(program.Publisher);
                item.SubItems.Add(program.Version);
                item.SubItems.Add(program.InstallDate);
                item.Tag = program;
                _listView.Items.Add(item);
            }
        }

        private List<InstalledProgram> GetInstalledPrograms()
        {
            var programs = new List<InstalledProgram>();

            try
            {
                var uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstallKey != null)
                {
                    foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                    {
                        var subKey = uninstallKey.OpenSubKey(subKeyName);
                        var name = subKey?.GetValue("DisplayName")?.ToString();
                        
                        if (!string.IsNullOrEmpty(name))
                        {
                            programs.Add(new InstalledProgram
                            {
                                Name = name,
                                Publisher = subKey.GetValue("Publisher")?.ToString() ?? "Unknown",
                                Version = subKey.GetValue("DisplayVersion")?.ToString() ?? "N/A",
                                InstallDate = subKey.GetValue("InstallDate")?.ToString() ?? "Unknown",
                                UninstallString = subKey.GetValue("UninstallString")?.ToString()
                            });
                        }
                    }
                }
            }
            catch { }

            return programs;
        }

        private void UninstallButton_Click(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a program to uninstall.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var program = (InstalledProgram)_listView.SelectedItems[0].Tag;
            var result = MessageBox.Show($"Deep uninstall {program.Name}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                PerformDeepUninstall(program);
            }
        }

        private void PerformDeepUninstall(InstalledProgram program)
        {
            try
            {
                if (!string.IsNullOrEmpty(program.UninstallString))
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = program.UninstallString;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    process.WaitForExit();

                    MessageBox.Show("Uninstallation initiated. Check for leftovers...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshInstalledPrograms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class InstalledProgram
        {
            public string Name { get; set; }
            public string Publisher { get; set; }
            public string Version { get; set; }
            public string InstallDate { get; set; }
            public string UninstallString { get; set; }
        }
    }
}
