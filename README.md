# System Manager - Comprehensive System Management Application

A powerful, modular system management application built in C# targeting .NET Framework 4.8.

## Features

### 🔍 Activity Watcher
- Browser URL tracking and monitoring
- Domain-based activity tracking (e.g., TradingView usage)
- Daily logs with active time per domain
- Session uptime display

### 📡 Network Monitor Dashboard
- Real-time signal strength monitoring
- Download/Upload speed tracking with graphs
- Connection stability analysis
- Jitter measurement
- Network statistics dashboard

### 📶 WiFi Guardian
- WiFi connection monitoring
- Checkbox-controlled activation (NOT auto-start)
- Connection status alerts
- Activity logging

### 🚀 Startup Manager
- Registry startup programs listing (HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run)
- Task Scheduler startup tasks listing
- Comprehensive startup management

### 🧹 System Cleaning
- Deep system cleaning tools
- System restore point creation (WMI-based)
- Registry backup functionality
- Temp files, cache, and system log cleanup

### 🗑️ Deep Uninstaller
- Advanced program uninstaller
- Leftover file detection and cleanup
- Registry cleanup after uninstallation

### 🛡️ Security Module
- Antivirus scanner with Quick/Medium/Deep scan options
- Auto-scan functionality
- Threat detection and logging
- Firewall monitoring
- Blocked connections display
- Active connections viewer

### ⚡ Optimization Module
- System optimization tools
- Performance enhancements

### 🔐 Password Manager
- Secure password storage with SHA-256 hashing
- Google Authenticator-style 2FA support
- Strong password generator
- Service-based password organization

### 💻 System Information
- CPU information (name, cores, threads) - CPU-Z style
- RAM information with slot details
- GPU information
- OS details
- System uptime display
- CPU temperature monitoring (WMI-based)
- Local IP address
- External IP address
- DNS servers display

### ⏰ Alarms & To-Do
- Alarm system
- To-do reminders
- Notification support (planned: audio, email, WhatsApp)

### 🌐 Network Features
- Built-in VPN module
- Smart DNS with auto-switching to fastest server
- Wireshark-style packet analyzer
- NetLimiter-style network tab

### 🔧 Additional Tools
- Built-in PuTTY-style terminal
- FileZilla-style file transfer
- 4t-tray minimizer features
- Comprehensive logging system

## Architecture

### Modular Design
The application uses a **100% modular architecture**:
- Only enabled modules are loaded and run
- Unused modules consume zero resources
- `ModuleManager` handles module lifecycle
- Each module implements the `IModule` interface

## Building the Project

### Requirements
- Visual Studio 2019 or later
- .NET Framework 4.8 Developer Pack
- MSBuild Tools

### Build Instructions

#### Using MSBuild (Windows)
```bash
# Build solution
msbuild SystemManager.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### Output
- Debug: `SystemManager/bin/Debug/SystemManager.exe`
- Release: `SystemManager/bin/Release/SystemManager.exe`

## Usage

### Running the Application
1. Launch `SystemManager.exe`
2. The main window displays all available modules as tabs
3. Select modules from the left panel
4. Each module can be enabled/disabled individually

### System Tray
- The application minimizes to system tray
- Right-click tray icon for quick actions
- Double-click to restore window

### Startup with Windows
- Check "Start with Windows" in the top panel
- The application will auto-start on Windows login

## Technical Details

### Technologies Used
- **Language**: C# 
- **Framework**: .NET Framework 4.8
- **UI**: Windows Forms
- **WMI**: System information and management
- **Registry API**: Startup management and system configuration
- **Task Scheduler API**: Scheduled task management
- **Network APIs**: NetworkInformation, Sockets
- **Cryptography**: SHA-256, HMAC-SHA1, RNG

## License
All rights reserved © 2025