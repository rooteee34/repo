using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.Diagnostics;
using System.IO;

namespace AnoreksikSuite {
    public static class Program {
        [STAThread]
        public static void Main() {
            try {
                if(! IsAdmin()) {
                    try {
                        ProcessStartInfo p = new ProcessStartInfo();
                        p.UseShellExecute = true;
                        p.WorkingDirectory = Environment.CurrentDirectory;
                        p.FileName = Application.ExecutablePath;
                        p.Verb = "runas";
                        Process.Start(p);
                    } catch (Exception ex) {
                        MessageBox.Show("Yonetici modu hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            } catch (Exception ex) {
                MessageBox.Show("KRITIK HATA:\n\n" + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace,
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try {
                    File.WriteAllText("CRASH.txt",
                        DateTime.Now.ToString() + "\n\n" + ex.ToString());
                } catch { }
            }
        }

        private static bool IsAdmin() {
            try {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal p = new WindowsPrincipal(id);
                return p.IsInRole(WindowsBuiltInRole.Administrator);
            } catch {
                return false;
            }
        }
    }
}
