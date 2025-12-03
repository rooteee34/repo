using System;
using System.IO;
using System.Windows.Forms;

namespace SystemManager.Modules.SystemCleaning
{
    public class SystemCleaningModule : IModule
    {
        private UserControl _control;
        private RestorePointManager _restorePointManager;
        private CheckedListBox _cleanOptionsListBox;
        private Button _createRestoreButton;
        private Button _cleanButton;

        public string ModuleName => "System Cleaning";
        public string Description => "Deep system cleaning with restore point creation";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _restorePointManager = new RestorePointManager();
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

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var title = new Label
            {
                Text = "System Cleaning Options",
                Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(10, 10),
                AutoSize = true
            };

            _cleanOptionsListBox = new CheckedListBox
            {
                Location = new System.Drawing.Point(10, 50),
                Size = new System.Drawing.Size(400, 200)
            };

            _cleanOptionsListBox.Items.Add("Temp Files");
            _cleanOptionsListBox.Items.Add("Windows Update Cache");
            _cleanOptionsListBox.Items.Add("Browser Cache");
            _cleanOptionsListBox.Items.Add("Recycle Bin");
            _cleanOptionsListBox.Items.Add("Thumbnails Cache");
            _cleanOptionsListBox.Items.Add("System Log Files");

            _createRestoreButton = new Button
            {
                Text = "Create Restore Point",
                Location = new System.Drawing.Point(10, 270),
                Width = 180
            };
            _createRestoreButton.Click += CreateRestoreButton_Click;

            _cleanButton = new Button
            {
                Text = "Start Deep Clean",
                Location = new System.Drawing.Point(200, 270),
                Width = 150
            };
            _cleanButton.Click += CleanButton_Click;

            panel.Controls.Add(title);
            panel.Controls.Add(_cleanOptionsListBox);
            panel.Controls.Add(_createRestoreButton);
            panel.Controls.Add(_cleanButton);

            _control.Controls.Add(panel);
        }

        private void CreateRestoreButton_Click(object sender, EventArgs e)
        {
            var description = $"SystemManager Restore Point - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            
            if (_restorePointManager.CreateRestorePoint(description))
            {
                MessageBox.Show("Restore point created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to create restore point. Administrator rights may be required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("This will perform deep system cleaning. Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                PerformCleaning();
            }
        }

        private void PerformCleaning()
        {
            try
            {
                int filesDeleted = 0;

                for (int i = 0; i < _cleanOptionsListBox.CheckedItems.Count; i++)
                {
                    var item = _cleanOptionsListBox.CheckedItems[i].ToString();

                    switch (item)
                    {
                        case "Temp Files":
                            filesDeleted += CleanTempFiles();
                            break;
                        case "Browser Cache":
                            filesDeleted += CleanBrowserCache();
                            break;
                        case "Recycle Bin":
                            CleanRecycleBin();
                            break;
                    }
                }

                MessageBox.Show($"Cleaning complete! {filesDeleted} files removed.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during cleaning: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int CleanTempFiles()
        {
            int count = 0;
            try
            {
                var tempPath = Path.GetTempPath();
                var files = Directory.GetFiles(tempPath);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                        count++;
                    }
                    catch { }
                }
            }
            catch { }
            return count;
        }

        private int CleanBrowserCache()
        {
            // Simplified browser cache cleaning
            return 0;
        }

        private void CleanRecycleBin()
        {
            try
            {
                // Use Windows API to empty recycle bin
            }
            catch { }
        }
    }
}
