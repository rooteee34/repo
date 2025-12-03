using System;
using System.Runtime.InteropServices;

namespace AnoreksikSuite {
    internal static class NativeMethods {
        // === NATIVE WIFI API ===
        [DllImport("wlanapi.dll")] public static extern uint WlanOpenHandle(uint v, IntPtr p, out uint n, out IntPtr h);
        [DllImport("wlanapi.dll")] public static extern uint WlanCloseHandle(IntPtr h, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanEnumInterfaces(IntPtr h, IntPtr p, out IntPtr i);
        [DllImport("wlanapi.dll")] public static extern uint WlanDisconnect(IntPtr h, ref Guid g, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanConnect(IntPtr h, ref Guid g, ref WLAN_CONNECTION_PARAMETERS par, IntPtr p);
        [DllImport("wlanapi.dll")] public static extern void WlanFreeMemory(IntPtr p);
        [DllImport("wlanapi.dll")] public static extern uint WlanQueryInterface(IntPtr h, ref Guid g, WLAN_INTF_OPCODE op, IntPtr p, out uint sz, out IntPtr dat, IntPtr t);
        [DllImport("wlanapi.dll")] public static extern uint WlanScan(IntPtr h, ref Guid g, IntPtr pDot11Ssid, IntPtr pIeData, IntPtr pReserved);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct WLAN_INTERFACE_INFO { public Guid InterfaceGuid; [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)] public string strInterfaceDescription; public uint isState; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_INTERFACE_INFO_LIST { public uint dwNumberOfItems; public uint dwIndex; public WLAN_INTERFACE_INFO InterfaceInfo; }
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] public struct DOT11_SSID { public uint uSSIDLength; [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] public byte[] ucSSID; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_CONNECTION_PARAMETERS { public uint wlanConnectionMode; public IntPtr strProfile; public IntPtr pDot11Ssid; public IntPtr pDesiredBssidList; public uint dot11BssType; public uint dwFlags; }
        public enum WLAN_INTF_OPCODE { wlan_intf_opcode_current_connection = 7, wlan_intf_opcode_rssi = 0x10000102 }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_CONNECTION_ATTRIBUTES { public uint isState; public uint wlanConnectionMode; [MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)] public string strProfileName; public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes; public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_ASSOCIATION_ATTRIBUTES { public DOT11_SSID dot11Ssid; public uint dot11BssType; [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)] public byte[] dot11Bssid; public uint dot11PhyType; public uint uDot11PhyIndex; public uint wlanSignalQuality; public uint ulRxRate; public uint ulTxRate; }
        [StructLayout(LayoutKind.Sequential)] public struct WLAN_SECURITY_ATTRIBUTES { [MarshalAs(UnmanagedType.Bool)] public bool bSecurityEnabled; [MarshalAs(UnmanagedType.Bool)] public bool bOneXEnabled; public uint dot11AuthAlgorithm; public uint dot11CipherAlgorithm; }

        // === KERNEL API ===
        [DllImport("ntdll.dll")] public static extern int NtSetSystemInformation(int c, IntPtr p, int l);
        [DllImport("ntdll.dll")] public static extern int NtSetInformationProcess(IntPtr h, int c, ref int i, int l);
        [DllImport("psapi.dll")] public static extern int EmptyWorkingSet(IntPtr h);
        [DllImport("psapi.dll")] public static extern bool SetProcessWorkingSetSize(IntPtr h, IntPtr min, IntPtr max);
        [DllImport("kernel32.dll")] public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX l);
        [DllImport("kernel32.dll")] public static extern bool SetProcessAffinityMask(IntPtr h, IntPtr m);
        [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out int p);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)] public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("advapi32.dll", SetLastError = true)] internal static extern bool AdjustTokenPrivileges(IntPtr t, bool d, ref TokPriv1Luid n, int l, IntPtr p, IntPtr r);
        [DllImport("advapi32.dll")] internal static extern bool OpenProcessToken(IntPtr h, int a, ref IntPtr t);
        [DllImport("advapi32.dll", SetLastError = true)] internal static extern bool LookupPrivilegeValue(string s, string n, ref long l);
        [DllImport("shell32.dll")] public static extern int SHEmptyRecycleBin(IntPtr h, string r, uint f);

        [StructLayout(LayoutKind.Sequential, Pack=1)] internal struct TokPriv1Luid { public int Count; public long Luid; public int Attr; }
        [StructLayout(LayoutKind.Sequential)] public struct MEMORYSTATUSEX { public uint dwLength; public uint dwMemoryLoad; public ulong ullTotalPhys; public ulong ullAvailPhys; public ulong ullTotalPageFile; public ulong ullAvailPageFile; public ulong ullTotalVirtual; public ulong ullAvailVirtual; public ulong ullAvailExtendedVirtual; }

        // === SYSTEM CACHE STRUCTURES ===
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct SYSTEM_CACHE_INFORMATION {
            public uint CurrentSize;
            public uint PeakSize;
            public uint PageFaultCount;
            public uint MinimumWorkingSet;
            public uint MaximumWorkingSet;
            public uint Unused1;
            public uint Unused2;
            public uint Unused3;
            public uint Unused4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct SYSTEM_CACHE_INFORMATION_64_BIT {
            public long CurrentSize;
            public long PeakSize;
            public long PageFaultCount;
            public long MinimumWorkingSet;
            public long MaximumWorkingSet;
            public long Unused1;
            public long Unused2;
            public long Unused3;
            public long Unused4;
        }

        public const int SystemFileCacheInformation = 0x0015; // Class 21
        public const int SystemMemoryListInformation = 0x0050; // Class 80
        public const int MemoryPurgeStandbyList = 4;
        public const int SE_PRIVILEGE_ENABLED = 2;
        public const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        public const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
    }
}
