# Quick Start Guide

Get up and running with System Manager in minutes!

## Prerequisites

- ✅ Windows 10 or later
- ✅ .NET Framework 4.8 (or Visual Studio with .NET Framework support)

## Quick Build & Run

### Option 1: Visual Studio (Easiest)

1. **Open the Solution**
   ```
   Double-click: SystemManager.sln
   ```

2. **Build & Run**
   ```
   Press F5
   ```

That's it! The application will build and launch.

### Option 2: Command Line

```cmd
# Open Developer Command Prompt for VS
# Navigate to the project folder

cd path\to\repo
msbuild SystemManager.sln /p:Configuration=Release
cd SystemManager\bin\Release
SystemManager.exe
```

## First Launch

When you launch System Manager, you'll see:

```
┌─────────────────────────────────────────────┐
│  System Manager    [Start w/ Win] [Exit]   │
├───────────┬─────────────────────────────────┤
│           │                                 │
│ Activity  │     Welcome to System Manager   │
│ Network   │                                 │
│ WiFi      │     Select a module from the    │
│ Startup   │     list to get started.        │
│ Cleaning  │                                 │
│ ...       │                                 │
│           │                                 │
└───────────┴─────────────────────────────────┘
```

## Quick Module Overview

### 🔍 Activity Watcher
**What it does:** Tracks which websites you visit and for how long  
**Try it:**
1. Click "Activity Watcher" in the module list
2. Open a browser and visit some websites
3. Come back and click "Refresh" to see your activity

### 📡 Network Monitor
**What it does:** Shows your internet speed and connection quality  
**Try it:**
1. Click "Network Monitor"
2. Watch the real-time graph
3. See download/upload speeds and signal strength

### 📶 WiFi Guardian
**What it does:** Alerts you if your WiFi disconnects  
**Try it:**
1. Click "WiFi Guardian"
2. Check the "Enable WiFi Guardian" checkbox
3. If your WiFi disconnects, you'll see an alert

### 🚀 Startup Manager
**What it does:** Shows what programs run when Windows starts  
**Try it:**
1. Click "Startup Manager"
2. View Registry and Task Scheduler tabs
3. See all your startup programs

### 🧹 System Cleaning
**What it does:** Cleans up temporary files and creates restore points  
**Try it:**
1. Click "System Cleaning"
2. Click "Create Restore Point" (requires admin)
3. Select cleanup options and click "Start Deep Clean"

### 🔐 Password Manager
**What it does:** Generates and stores secure passwords  
**Try it:**
1. Click "Password Manager"
2. Click "Generate" for a random secure password
3. Enter a service name and click "Save"

### 💻 System Info
**What it does:** Shows your computer's hardware specs  
**Try it:**
1. Click "System Info"
2. See CPU, RAM, GPU, temps, IPs, etc.
3. Click "Refresh" to update

## Common Tasks

### Minimize to System Tray

1. Click the ❌ (close) button
2. App minimizes to tray instead of closing
3. Double-click tray icon to restore

### Start with Windows

1. Check "Start with Windows" at the top
2. App will auto-start on login

### Exit Completely

1. Click "Exit" button at the top
2. Confirm exit
3. App closes completely

## Running as Administrator

Some features need admin rights:
- System restore points
- Deep system cleaning
- Registry operations

**To run as admin:**
- Right-click `SystemManager.exe`
- Select "Run as administrator"

## Tips & Tricks

### 🎯 Performance Tips
- Disable modules you don't use
- Each disabled module uses zero resources
- Keep only what you need active

### 🔒 Security Tips
- Run as admin only when needed
- Review what programs have startup access
- Use the password generator for strong passwords

### 📊 Monitoring Tips
- Activity Watcher: Export logs regularly
- Network Monitor: Watch stability % for connection quality
- WiFi Guardian: Keep it enabled if WiFi is unreliable

### 🧹 Maintenance Tips
- Create restore points before system changes
- Run System Cleaning weekly
- Check Startup Manager for unwanted programs

## Troubleshooting

### Application won't start
- ✅ Install .NET Framework 4.8
- ✅ Try running as administrator

### Module not working
- ✅ Check if feature needs admin rights
- ✅ Verify Windows Firewall isn't blocking
- ✅ Check the Logs tab for errors

### High CPU/Memory usage
- ✅ Disable unused modules
- ✅ Reduce polling intervals (code modification)
- ✅ Close and reopen the app

### WiFi Guardian always off
- ✅ This is by design! Check the checkbox to enable
- ✅ It will NOT auto-start to save resources

## Next Steps

### For Users
1. ✅ Explore all modules
2. ✅ Configure "Start with Windows" if desired
3. ✅ Set up password manager with your accounts
4. ✅ Review startup programs and disable unwanted ones

### For Developers
1. ✅ Read `TECHNICAL_DOCUMENTATION.md`
2. ✅ Review module implementations
3. ✅ Create your own modules using `IModule` interface
4. ✅ Contribute improvements!

## Module Checklist

Try each module:
- [ ] Activity Watcher - Track browsing time
- [ ] Network Monitor - Check internet speed
- [ ] WiFi Guardian - Monitor WiFi connection
- [ ] Startup Manager - Review startup programs
- [ ] System Cleaning - Clean temp files
- [ ] Deep Uninstaller - Uninstall programs cleanly
- [ ] Security - Scan for threats
- [ ] Optimization - Optimize system
- [ ] Password Manager - Generate secure passwords
- [ ] System Info - View hardware specs
- [ ] Alarms & To-Do - Set reminders (future)
- [ ] VPN - Connect to VPN (future)
- [ ] Terminal - Use built-in terminal (future)
- [ ] File Transfer - Transfer files (future)
- [ ] Smart DNS - Auto-switch DNS (future)
- [ ] Packet Analyzer - Analyze packets (future)
- [ ] Logs - View application logs

## Getting Help

1. **Check Documentation**
   - `README.md` - Feature overview
   - `BUILD_GUIDE.md` - Building instructions
   - `TECHNICAL_DOCUMENTATION.md` - Architecture details

2. **Check Logs**
   - Open the "Logs" tab
   - Look for error messages
   - Export logs for troubleshooting

3. **Windows Event Viewer**
   - Check for application errors
   - Look in Application logs

## Screenshots

### Main Interface
```
┌─────────────────────────────────────────────┐
│ [Module List]  │  [Tab Content Area]        │
│                │                             │
│ • Activity     │  Selected module displays   │
│ • Network      │  its interface here         │
│ • WiFi         │                             │
│ • Startup      │  Each module has its own    │
│ • Cleaning     │  controls and features      │
│ • Uninstaller  │                             │
│ • Security     │                             │
│ • ...          │                             │
└────────────────┴─────────────────────────────┘
```

## Quick Reference Card

| Module | Purpose | Key Feature |
|--------|---------|-------------|
| Activity Watcher | Track browsing | Domain-based logs |
| Network Monitor | Monitor speed | Real-time graphs |
| WiFi Guardian | WiFi alerts | Checkbox-controlled |
| Startup Manager | Manage startups | Registry + Tasks |
| System Cleaning | Clean system | Restore points |
| Deep Uninstaller | Remove programs | Leftover cleanup |
| Security | Scan threats | 3 scan modes |
| Password Manager | Store passwords | 2FA + Generator |
| System Info | Show specs | Temps + IPs |
| Logs | View logs | All events |

## Keyboard Shortcuts

- `F5` (in VS) - Build & Run
- `Ctrl + Shift + B` (in VS) - Build Solution
- `Alt + F4` - Close (minimizes to tray)

## System Requirements

| Component | Requirement |
|-----------|-------------|
| OS | Windows 10+ |
| .NET | Framework 4.8 |
| RAM | 512 MB min, 2 GB recommended |
| Disk | 100 MB |
| Admin | Required for some features |

## What's Next?

Now that you're up and running:

1. **Explore** - Try all the modules
2. **Customize** - Enable only what you need
3. **Monitor** - Track your system health
4. **Maintain** - Regular cleaning and checks
5. **Secure** - Scan and monitor security

## Support

For detailed information:
- Technical issues → `TECHNICAL_DOCUMENTATION.md`
- Build problems → `BUILD_GUIDE.md`
- Feature questions → `README.md`

---

**Happy System Managing! 🚀**

*Remember: Only enable the modules you actually use. The modular design means disabled modules consume zero resources!*
