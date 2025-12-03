# Anoreksik Suite V10 Source Code

This directory contains the source code for Anoreksik Suite V10, refactored into multiple files for better maintainability and modularity.

## Structure

*   `Program.cs`: Entry point of the application.
*   `MainForm.cs`: Main form class, containing core logic and initialization.
*   `NativeMethods.cs`: P/Invoke declarations for Windows APIs.
*   `Modules/`: Contains partial class implementations for each tab/feature.
    *   `WifiModule.cs`: WiFi Watchdog and optimization (Fixed infinite loop/startup bug).
    *   `ActivityWatcher.cs`: **NEW** Tracks active window and usage time.
        *   *Note*: Browser URL tracking is currently limited to Window Title analysis to avoid external dependencies.
    *   `StartupManager.cs`: **NEW** Manage Registry Run keys and Scheduled Tasks. Includes System Restore point creation.
    *   `SecurityModule.cs`: **NEW** TinyWall-like features and AV scanning (Hash-based).
    *   `PasswordManager.cs`: **NEW** Secure password storage using DPAPI (Data Protection API).
    *   `SystemInfo.cs`: **NEW** Detailed system information (WMI).
    *   `ToolsModule.cs`: **NEW** Alarm, Terminal, VPN shortcuts.
    *   `LegacyModules.cs`: Contains the original RAM, CPU, DISK, and NET logic from V7, adapted to the new structure.

## Compilation

To compile this project, you need a C# compiler (Visual Studio, Roslyn, or `csc`).

1.  Create a new Windows Forms Project (.NET Framework 4.7.2 or higher recommended).
2.  Add `System.Management` reference (for System Info and System Restore).
3.  Add `System.Security` reference (for Password Manager).
4.  Include all files in the `src` directory.

## Features Added

*   **Modular Architecture**: Split into multiple files.
*   **Activity Watcher**: Tracks usage.
*   **Startup Manager**: Registry Run editor & Task Scheduler Viewer.
*   **Security Tab**: Firewall controls and File Hashing.
*   **Password Manager**: Encrypted storage (DPAPI).
*   **System Info**: Detailed hardware info.
*   **Fixes**: WiFi Watchdog checkbox logic fixed.
*   **Legacy Support**: All original optimization features (RAM cleaning, CPU priority, Network tuning) are preserved.

## Note
Ensure you run the application as Administrator for all features to work correctly (Service management, Registry access, etc.).
