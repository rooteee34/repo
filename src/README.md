# Anoreksik Suite V10 Source Code

This directory contains the source code for Anoreksik Suite V10, a modular system optimization tool.

## How to Compile (Derleme)

1.  Open the folder containing these files.
2.  Double-click on **`Build.bat`**.
3.  The script will automatically find the C# compiler on your system and create `AnoreksikSuiteV10.exe`.

## Requirements

*   Windows 7, 8, 10, or 11.
*   .NET Framework 4.7.2 or higher (Pre-installed on most Windows 10/11 systems).

## Structure

*   `Program.cs`: Entry point.
*   `MainForm.cs`: Main UI logic.
*   `NativeMethods.cs`: Windows API definitions.
*   `Modules/`: Feature implementations (WiFi, Security, Startup, etc.).
*   `Build.bat`: One-click compile script.

## Features

*   **Modular Architecture**: Clean code structure.
*   **WiFi Watchdog**: Auto-reset connection on ping failure (Fixed checkbox bug).
*   **Activity Watcher**: Track window usage time.
*   **Startup Manager**: Manage Registry Run keys and Scheduled Tasks.
*   **Security**: Firewall controls, File Hash scanner.
*   **Password Manager**: Encrypted storage (DPAPI).
*   **System Info**: Hardware details via WMI.
*   **Optimization**: RAM cleaning, CPU priority, Disk deep clean.

## Important Note

This application performs system-level operations (Registry editing, Service management, Network configuration). **Always run the generated `.exe` as Administrator.**
