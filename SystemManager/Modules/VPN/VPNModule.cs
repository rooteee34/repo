using System;
using System.Windows.Forms;

namespace SystemManager.Modules.VPN
{
    public class VPNModule : IModule
    {
        private UserControl _control;

        public string ModuleName => "VPN";
        public string Description => "Built-in VPN functionality";
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
            _control.Controls.Add(new Label { Text = "VPN Module", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter });
        }
    }
}
