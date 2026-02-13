using System;
using System.Windows.Forms;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage passwordPage;
        private TextBox txtPassService, txtPassUser, txtPassPass;
        private ListBox lstPasswords;
        private string passFile = "passwords.dat";

        // DPAPI entropy (Optional, adds extra layer but machine dependent)
        private static byte[] s_additionalEntropy = { 9, 8, 7, 6, 5 };

        private void InitTab_Passwords() {
            passwordPage = new TabPage("SIFRE");
            passwordPage.BackColor = Color.FromArgb(30,30,30);
            passwordPage.Name = "PASSWORDS";

            CreateLabel("Servis:", 10, 20, passwordPage);
            txtPassService = new TextBox(); txtPassService.Location = new Point(70, 18); passwordPage.Controls.Add(txtPassService);

            CreateLabel("Kullanici:", 10, 50, passwordPage);
            txtPassUser = new TextBox(); txtPassUser.Location = new Point(70, 48); passwordPage.Controls.Add(txtPassUser);

            CreateLabel("Sifre:", 10, 80, passwordPage);
            txtPassPass = new TextBox(); txtPassPass.Location = new Point(70, 78); passwordPage.Controls.Add(txtPassPass);

            Button btnGen = CreateBtn("OLUSTUR", 200, 75, 80, Color.Gray, passwordPage);
            btnGen.Height = 25;
            btnGen.Click += (s,e) => txtPassPass.Text = GeneratePassword(16);

            Button btnAdd = CreateBtn("KAYDET (Sifreli)", 10, 110, 270, Color.DarkGreen, passwordPage);
            btnAdd.Click += (s,e) => AddPassword();

            lstPasswords = new ListBox();
            lstPasswords.Location = new Point(10, 160);
            lstPasswords.Size = new Size(660, 250);
            lstPasswords.BackColor = Color.Black;
            lstPasswords.ForeColor = Color.Yellow;
            passwordPage.Controls.Add(lstPasswords);

            Button btnShow = CreateBtn("SIFREYI GOSTER", 10, 420, 200, Color.Orange, passwordPage);
            btnShow.Click += (s,e) => ShowPassword();

            LoadPasswords();
            tabs.TabPages.Add(passwordPage);
        }

        private string GeneratePassword(int len) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                byte[] data = new byte[len];
                rng.GetBytes(data);
                foreach (byte b in data) res.Append(chars[b % chars.Length]);
            }
            return res.ToString();
        }

        private void AddPassword() {
            try {
                string encrypted = Encrypt(txtPassPass.Text);
                string line = string.Format("{0}|{1}|{2}", txtPassService.Text, txtPassUser.Text, encrypted);
                File.AppendAllText(passFile, line + Environment.NewLine);
                LoadPasswords();
                txtPassService.Clear(); txtPassUser.Clear(); txtPassPass.Clear();
            } catch (Exception ex) { SafeMessageBox("Sifreleme hatasi: " + ex.Message); }
        }

        private void LoadPasswords() {
            lstPasswords.Items.Clear();
            if (File.Exists(passFile)) {
                foreach (string line in File.ReadAllLines(passFile)) {
                    string[] p = line.Split('|');
                    if (p.Length == 3) lstPasswords.Items.Add(p[0] + " - " + p[1]);
                }
            }
        }

        private void ShowPassword() {
            if (lstPasswords.SelectedIndex == -1) return;
            try {
                string[] lines = File.ReadAllLines(passFile);
                string[] p = lines[lstPasswords.SelectedIndex].Split('|');
                SafeMessageBox("Sifre: " + Decrypt(p[2]));
            } catch (Exception ex) { SafeMessageBox("Sifre cozme hatasi: " + ex.Message); }
        }

        // Use DPAPI for secure local storage without hardcoded keys
        private string Encrypt(string clearText) {
            byte[] data = Encoding.UTF8.GetBytes(clearText);
            byte[] encrypted = ProtectedData.Protect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        private string Decrypt(string cipherText) {
             byte[] data = Convert.FromBase64String(cipherText);
             byte[] decrypted = ProtectedData.Unprotect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
             return Encoding.UTF8.GetString(decrypted);
        }
    }
}
