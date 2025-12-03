using System;
using System.Windows.Forms;

namespace SystemManager.Modules.FileTransfer
{
    public class FileTransferModule : IModule
    {
        private UserControl _control;

        public string ModuleName => "File Transfer";
        public string Description => "FileZilla-style file transfer";
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
            _control.Controls.Add(new Label { Text = "File Transfer Module", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter });
        }
    }
}
