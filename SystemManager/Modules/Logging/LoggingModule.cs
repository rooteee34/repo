using System;
using System.Windows.Forms;

namespace SystemManager.Modules.Logging
{
    public class LoggingModule : IModule
    {
        private UserControl _control;
        private ListView _logListView;

        public string ModuleName => "Logs";
        public string Description => "Detailed application logs";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize() { CreateControl(); }
        public void Start() { }
        public void Stop() { }
        public void Dispose() { _control?.Dispose(); }
        public Control GetControl() { return _control; }

        private void CreateControl()
        {
            _control = new UserControl { Dock = DockStyle.Fill };
            
            _logListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            
            _logListView.Columns.Add("Time", 150);
            _logListView.Columns.Add("Module", 150);
            _logListView.Columns.Add("Level", 100);
            _logListView.Columns.Add("Message", 400);
            
            _control.Controls.Add(_logListView);
        }

        public void AddLog(string module, string level, string message)
        {
            if (_logListView != null && !_logListView.IsDisposed)
            {
                try
                {
                    _logListView.Invoke((Action)(() =>
                    {
                        var item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        item.SubItems.Add(module);
                        item.SubItems.Add(level);
                        item.SubItems.Add(message);
                        _logListView.Items.Insert(0, item);

                        while (_logListView.Items.Count > 1000)
                        {
                            _logListView.Items.RemoveAt(_logListView.Items.Count - 1);
                        }
                    }));
                }
                catch { }
            }
        }
    }
}
