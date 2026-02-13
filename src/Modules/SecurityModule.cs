using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace AnoreksikSuite {
    public partial class MainForm {
        private TabPage securityPage;

        private void InitTab_Security() {
            securityPage = new TabPage("GUVENLIK");
            securityPage.BackColor = Color.FromArgb(30,30,30);
            securityPage.Name = "SECURITY";

            GroupBox gFirewall = CreateGroup("Guvenlik Duvari (TinyWall stili)", 10, 10, 660, 150, securityPage);

            Button btnBlockAll = CreateBtn("TUM BAGLANTILARI KES (Block All)", 10, 25, 300, Color.DarkRed, gFirewall);
            btnBlockAll.Click += (s,e) => RunCMD("netsh advfirewall set allprofiles state on & netsh advfirewall set allprofiles firewallpolicy blockinbound,blockoutbound");

            Button btnAllowOut = CreateBtn("SADECE CIKIS IZNI (Allow Out)", 330, 25, 300, Color.Orange, gFirewall);
            btnAllowOut.Click += (s,e) => RunCMD("netsh advfirewall set allprofiles firewallpolicy blockinbound,allowoutbound");

            Button btnNormal = CreateBtn("NORMAL MOD", 10, 80, 300, Color.Green, gFirewall);
            btnNormal.Click += (s,e) => RunCMD("netsh advfirewall reset");

            GroupBox gAV = CreateGroup("Dosya Tarama (Imza Bazli)", 10, 180, 660, 150, securityPage);
            Button btnScanFile = CreateBtn("DOSYA SEC VE TARA", 10, 25, 200, Color.Blue, gAV);
            btnScanFile.Click += (s,e) => {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK) {
                    string hash = GetMD5(ofd.FileName);
                    SafeMessageBox("Dosya Hash (MD5): " + hash + "\n\n(Veritabani baglantisi olmadigi icin sadece hash gosteriliyor)");
                }
            };

            tabs.TabPages.Add(securityPage);
        }

        private string GetMD5(string filename) {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
