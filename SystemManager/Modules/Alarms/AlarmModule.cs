using System;
using System.Windows.Forms;

namespace SystemManager.Modules.Alarms
{
    public class AlarmModule : IModule
    {
        private UserControl _control;

        public string ModuleName => "Alarms & To-Do";
        public string Description => "Alarm and reminder system with notifications";
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
            _control.Controls.Add(new Label { Text = "Alarms & To-Do Module", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter });
        }
    }
}
