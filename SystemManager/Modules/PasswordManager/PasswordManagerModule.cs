using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SystemManager.Modules.PasswordManager
{
    public class PasswordManagerModule : IModule
    {
        private UserControl _control;
        private PasswordGenerator _generator;
        private TwoFactorAuth _twoFactor;
        private ListView _passwordListView;
        private TextBox _serviceTextBox;
        private TextBox _passwordTextBox;
        private TextBox _generatedPasswordTextBox;

        public string ModuleName => "Password Manager";
        public string Description => "Secure password storage with hash and 2FA support";
        public bool IsEnabled { get; set; }

        public event EventHandler ConfigurationChanged;

        public void Initialize()
        {
            _generator = new PasswordGenerator();
            _twoFactor = new TwoFactorAuth();
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

            // Service Name
            var serviceLabel = new Label
            {
                Text = "Service:",
                Location = new System.Drawing.Point(10, 15),
                AutoSize = true
            };

            _serviceTextBox = new TextBox
            {
                Location = new System.Drawing.Point(100, 10),
                Width = 200
            };

            // Password Entry
            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(10, 45),
                AutoSize = true
            };

            _passwordTextBox = new TextBox
            {
                Location = new System.Drawing.Point(100, 40),
                Width = 200,
                UseSystemPasswordChar = true
            };

            // Generate Password Section
            var generateButton = new Button
            {
                Text = "Generate",
                Location = new System.Drawing.Point(310, 40),
                Width = 100
            };
            generateButton.Click += GenerateButton_Click;

            _generatedPasswordTextBox = new TextBox
            {
                Location = new System.Drawing.Point(100, 70),
                Width = 300,
                ReadOnly = true
            };

            var copyButton = new Button
            {
                Text = "Copy",
                Location = new System.Drawing.Point(410, 70),
                Width = 60
            };
            copyButton.Click += (s, e) => Clipboard.SetText(_generatedPasswordTextBox.Text);

            // Save Button
            var saveButton = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(100, 100),
                Width = 100
            };
            saveButton.Click += SaveButton_Click;

            // Password List
            _passwordListView = new ListView
            {
                Location = new System.Drawing.Point(10, 140),
                Size = new System.Drawing.Size(600, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _passwordListView.Columns.Add("Service", 200);
            _passwordListView.Columns.Add("Hash", 300);
            _passwordListView.Columns.Add("Date", 100);

            panel.Controls.Add(serviceLabel);
            panel.Controls.Add(_serviceTextBox);
            panel.Controls.Add(passwordLabel);
            panel.Controls.Add(_passwordTextBox);
            panel.Controls.Add(generateButton);
            panel.Controls.Add(_generatedPasswordTextBox);
            panel.Controls.Add(copyButton);
            panel.Controls.Add(saveButton);
            panel.Controls.Add(_passwordListView);

            _control.Controls.Add(panel);
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            var password = _generator.GeneratePassword(16, true, true, true);
            _generatedPasswordTextBox.Text = password;
            _passwordTextBox.Text = password;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var service = _serviceTextBox.Text;
            var password = _passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter service name and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var hash = _generator.HashPassword(password);
            var item = new ListViewItem(service);
            item.SubItems.Add(hash);
            item.SubItems.Add(DateTime.Now.ToString("yyyy-MM-dd"));
            _passwordListView.Items.Add(item);

            _serviceTextBox.Clear();
            _passwordTextBox.Clear();
            _generatedPasswordTextBox.Clear();

            MessageBox.Show("Password saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
