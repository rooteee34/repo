using System;
using System.Windows.Forms;
using System.Drawing;

namespace AnoreksikSuite {
    public partial class MainForm {
        // --- SHARED FIELDS (Restored from LegacyModules.cs) ---
        // These fields are used across multiple modules.

        // Tab Pages
        private TabPage dashboardPage, ramPage, cpuPage, netPage, diskPage, panoPage;
        // WifiPage is defined in WifiModule.cs but sometimes referenced elsewhere, so keep it careful.
        // Actually WifiModule.cs defines: private TabPage wifiPage;

        // Dashboard Checkboxes
        private CheckBox chkEnableWifi, chkEnableRam, chkEnableCpu, chkEnableNet, chkEnableDisk, chkEnablePano;
    }
}
