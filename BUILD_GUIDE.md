# Build and Deployment Guide

## Prerequisites

### Required Software
1. **Windows Operating System** (Windows 10 or later)
2. **Visual Studio 2019 or later** (Community, Professional, or Enterprise)
   - Download from: https://visualstudio.microsoft.com/downloads/
3. **.NET Framework 4.8 Developer Pack**
   - Usually included with Visual Studio
   - Manual download: https://dotnet.microsoft.com/download/dotnet-framework/net48

### Optional (for command-line builds)
- **MSBuild Tools** (included with Visual Studio)
- **NuGet CLI** (for package management)

## Building the Application

### Method 1: Using Visual Studio (Recommended)

1. **Open the Solution**
   ```
   Double-click SystemManager.sln
   ```

2. **Restore NuGet Packages**
   - Right-click on Solution in Solution Explorer
   - Select "Restore NuGet Packages"
   - Or: Tools → NuGet Package Manager → Restore

3. **Build the Solution**
   - Press `Ctrl + Shift + B` OR
   - Build → Build Solution (F6)
   - Choose Debug or Release configuration

4. **Run the Application**
   - Press `F5` to run with debugging
   - Press `Ctrl + F5` to run without debugging

### Method 2: Using MSBuild (Command Line)

1. **Open Developer Command Prompt for VS**
   - Start Menu → Visual Studio 2019 → Developer Command Prompt

2. **Navigate to Project Directory**
   ```cmd
   cd path\to\repo
   ```

3. **Build the Project**
   ```cmd
   msbuild SystemManager.sln /p:Configuration=Release /p:Platform="Any CPU"
   ```

4. **Output Location**
   - Debug build: `SystemManager\bin\Debug\SystemManager.exe`
   - Release build: `SystemManager\bin\Release\SystemManager.exe`

### Method 3: Using .NET CLI (if applicable)

Note: This project targets .NET Framework 4.8, which is not fully supported by `dotnet` CLI. Use MSBuild instead.

## Build Configurations

### Debug Configuration
- Includes debugging symbols
- No optimizations
- Larger executable size
- Use for development and testing

### Release Configuration
- Optimized code
- No debugging symbols
- Smaller executable size
- Use for production deployment

## Troubleshooting Build Issues

### Issue: "The type or namespace name 'TaskScheduler' could not be found"

**Solution:** Install the Task Scheduler NuGet package
```
Tools → NuGet Package Manager → Manage NuGet Packages for Solution
Search for: Microsoft.Win32.TaskScheduler
Install the package
```

### Issue: Missing .NET Framework 4.8

**Solution:** Install .NET Framework 4.8 Developer Pack
- Download from Microsoft's website
- Restart Visual Studio after installation

### Issue: "Cannot find MSBuild"

**Solution:** Add MSBuild to PATH or use Developer Command Prompt
```
C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin
```

### Issue: Build fails with WMI references

**Solution:** Ensure System.Management reference is properly added
- Right-click project → Add Reference
- Assemblies → Framework → System.Management

## Deployment

### Standalone Deployment

1. **Build in Release Mode**
   ```cmd
   msbuild SystemManager.sln /p:Configuration=Release
   ```

2. **Copy Output Files**
   - Copy all files from `SystemManager\bin\Release\` folder
   - Required files:
     - SystemManager.exe
     - SystemManager.exe.config
     - Any .dll files in the output folder

3. **Create Installer (Optional)**
   - Use InstallShield, WiX, or other installer tools
   - Package the executable and dependencies

### System Requirements for Deployment

**Minimum Requirements:**
- Windows 10 or later
- .NET Framework 4.8 Runtime
- 100 MB available disk space
- 512 MB RAM
- Administrator rights (for some features)

**Recommended:**
- Windows 10/11
- 4 GB RAM
- SSD storage
- Active internet connection (for external IP and updates)

## Administrator Privileges

Some features require administrator privileges:
- System restore point creation
- Registry backup
- Deep system cleaning
- Firewall monitoring
- Startup management
- WiFi monitoring

**To run as Administrator:**
- Right-click `SystemManager.exe` → Run as administrator
- Or: Set compatibility mode to always run as admin

## Feature-Specific Notes

### Activity Watcher
- Requires active browser windows
- Works with Chrome, Firefox, Edge, Opera, Brave
- May need UI Automation permissions

### Network Monitor
- Requires network adapters to be active
- WiFi signal strength only available on WiFi connections
- May require firewall permissions

### WiFi Guardian
- Only works when WiFi adapter is present
- Must be manually enabled via checkbox
- Does NOT auto-start with the application

### Security Scanner
- Virus signature database is minimal by default
- Consider integrating external AV database
- Scanning large directories may take time

### System Information
- Temperature monitoring may not work on all hardware
- Requires WMI service to be running
- Some info requires admin privileges

### Password Manager
- Passwords are stored locally
- Hashed using SHA-256
- Consider adding encryption for storage

## Development Notes

### Adding New Modules

1. Create a new class that implements `IModule`
2. Implement all required methods
3. Register the module in `MainForm.InitializeModules()`
4. The module will automatically appear in the UI

Example:
```csharp
public class MyNewModule : IModule
{
    public string ModuleName => "My Module";
    public string Description => "Description";
    public bool IsEnabled { get; set; }
    
    // Implement other IModule methods...
}
```

### Modular Architecture Benefits

- **Zero Resource Usage**: Disabled modules consume no memory or CPU
- **Easy Extension**: Add new modules without modifying core
- **Maintainability**: Each module is self-contained
- **Testability**: Modules can be tested independently

## Testing

### Manual Testing Checklist

- [ ] Application launches successfully
- [ ] All modules appear in the tab list
- [ ] Activity Watcher tracks browser activity
- [ ] Network Monitor displays speed and signal
- [ ] WiFi Guardian checkbox controls activation
- [ ] Startup Manager lists registry entries
- [ ] System Cleaning creates restore points
- [ ] Uninstaller lists installed programs
- [ ] Security scanner runs without errors
- [ ] Password Manager generates and saves passwords
- [ ] System Info displays hardware details
- [ ] System tray icon works correctly
- [ ] "Start with Windows" checkbox functions
- [ ] Application minimizes to tray
- [ ] Exit button closes application

### Automated Testing (Future)

Consider adding:
- Unit tests for utility classes
- Integration tests for module initialization
- UI automation tests for critical workflows

## Performance Considerations

- **Activity Watcher**: Polls every 1 second
- **Network Monitor**: Samples every 1 second, keeps 1 hour history
- **WiFi Guardian**: Checks every 2 seconds when enabled
- **System Info**: Updates every 1 second

To reduce resource usage:
- Adjust polling intervals in module constructors
- Disable unused modules
- Use Task Manager to monitor resource usage

## Security Best Practices

1. **Code Signing**: Sign the executable with a code signing certificate
2. **Virus Scanning**: Test with multiple antivirus solutions
3. **Permissions**: Request only necessary permissions
4. **Data Protection**: Encrypt sensitive stored data
5. **Updates**: Implement secure update mechanism

## Logging and Debugging

The application includes a logging module that captures:
- Module initialization events
- Error conditions
- User actions
- System events

Logs are displayed in the "Logs" tab and can be exported.

## Support and Maintenance

### Common Issues

**Q: Application won't start**
- Check .NET Framework 4.8 is installed
- Run as Administrator
- Check Windows Event Viewer for errors

**Q: Features not working**
- Verify administrator privileges
- Check Windows Firewall settings
- Ensure required services are running

**Q: High CPU usage**
- Disable unused modules
- Check for infinite loops in custom modules
- Use Performance Profiler in Visual Studio

### Getting Help

For issues and questions:
1. Check this documentation
2. Review the README.md
3. Check Windows Event Logs
4. Review application logs in Logs tab

## Version History

- **v1.0.0** - Initial release with core modules

## Future Roadmap

- Database integration for virus signatures
- Cloud backup for passwords
- Email/WhatsApp notifications
- VPN protocol implementations
- Advanced packet filtering
- Auto-update functionality
- Multi-language support

---

**Last Updated:** December 2025
