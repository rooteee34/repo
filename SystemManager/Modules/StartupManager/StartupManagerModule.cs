using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SystemManager.Utilities;
// Note: Microsoft.Win32.TaskScheduler is an optional NuGet package
// If not installed, Task Scheduler tab will show a message
// Install via: Install-Package TaskScheduler

namespace SystemManager.Modules.StartupManager
{
    /// <summary>
    /// Startup Manager - Lists Registry and Task Scheduler startup items
    /// </summary>
    public class StartupManagerModule : IModule
    {
        private UserControl _control;
        private ListView _listView;
        private TabControl _tabControl;

        public string ModuleName => "Startup Manager";
        public string Description => "Manage Windows startup programs from Registry and Task Scheduler";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            CreateControl();
        }

        public void Start()
        {
            RefreshStartupItems();
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

            _tabControl = new TabControl { Dock = DockStyle.Fill };

            // Registry Tab
            var registryTab = new TabPage("Registry Startup");
            var registryListView = CreateListView();
            registryTab.Controls.Add(registryListView);
            _tabControl.TabPages.Add(registryTab);

            // Task Scheduler Tab
            var schedulerTab = new TabPage("Task Scheduler");
            var schedulerListView = CreateListView();
            schedulerTab.Controls.Add(schedulerListView);
            _tabControl.TabPages.Add(schedulerTab);

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new System.Drawing.Point(10, 8),
                Width = 100
            };
            refreshButton.Click += (s, e) => RefreshStartupItems();
            buttonPanel.Controls.Add(refreshButton);

            _control.Controls.Add(_tabControl);
            _control.Controls.Add(buttonPanel);
        }

        private ListView CreateListView()
        {
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };

            listView.Columns.Add("Name", 200);
            listView.Columns.Add("Command", 350);
            listView.Columns.Add("Location", 250);

            return listView;
        }

        private void RefreshStartupItems()
        {
            // Refresh Registry items
            var registryListView = (ListView)_tabControl.TabPages[0].Controls[0];
            registryListView.Items.Clear();

            var registryItems = RegistryHelper.GetStartupPrograms();
            foreach (var item in registryItems)
            {
                var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(item.Command);
                listItem.SubItems.Add(item.Location);
                registryListView.Items.Add(listItem);
            }

            // Refresh Task Scheduler items
            var schedulerListView = (ListView)_tabControl.TabPages[1].Controls[0];
            schedulerListView.Items.Clear();

            var scheduledTasks = GetScheduledStartupTasks();
            foreach (var task in scheduledTasks)
            {
                var listItem = new ListViewItem(task.Name);
                listItem.SubItems.Add(task.Command);
                listItem.SubItems.Add(task.Location);
                schedulerListView.Items.Add(listItem);
            }
        }

        private List<StartupEntry> GetScheduledStartupTasks()
        {
            var tasks = new List<StartupEntry>();

            try
            {
                // Note: This requires Microsoft.Win32.TaskScheduler NuGet package
                // If not available, use alternate method below
                
                // Alternate method: Use WMI to query scheduled tasks
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT * FROM Win32_ScheduledJob"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        tasks.Add(new StartupEntry
                        {
                            Name = obj["Name"]?.ToString() ?? "Unknown",
                            Command = obj["Command"]?.ToString() ?? "N/A",
                            Location = "Task Scheduler",
                            Type = "Scheduled Task"
                        });
                    }
                }
                
                // If TaskScheduler library is available, use this code instead:
                // using (var ts = new TaskService())
                // {
                //     foreach (var task in ts.RootFolder.AllTasks)
                //     {
                //         if (task.Definition.Triggers.Any(t => t.TriggerType == TaskTriggerType.Logon || 
                //                                                t.TriggerType == TaskTriggerType.Boot))
                //         {
                //             var action = task.Definition.Actions.FirstOrDefault();
                //             tasks.Add(new StartupEntry
                //             {
                //                 Name = task.Name,
                //                 Command = action?.ToString() ?? "N/A",
                //                 Location = task.Path,
                //                 Type = "Task Scheduler"
                //             });
                //         }
                //     }
                // }
            }
            catch
            {
                // Handle exceptions - Task Scheduler access may require admin
                // Add a placeholder entry to inform user
                tasks.Add(new StartupEntry
                {
                    Name = "Task Scheduler Access",
                    Command = "Requires Administrator privileges or TaskScheduler NuGet package",
                    Location = "N/A",
                    Type = "Info"
                });
            }

            return tasks;
        }
    }
}
