using System;
using System.Windows.Forms;

namespace SystemManager.Modules.DNS
{
    public class SmartDNSModule : IModule
    {
        private UserControl _control;

        public string ModuleName => "Smart DNS";
        public string Description => "Smart DNS with auto-switching to fastest server";
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
            _control.Controls.Add(new Label { Text = "Smart DNS Module", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter });
        }
    }
}
