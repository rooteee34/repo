# Technical Documentation

## System Architecture

### Overview

System Manager is built using a **modular plugin architecture** where each feature is implemented as an independent module. This design ensures:

- **Isolation**: Modules operate independently without affecting others
- **Scalability**: New modules can be added without modifying core code
- **Performance**: Only enabled modules consume resources
- **Maintainability**: Bugs in one module don't affect others

### Core Components

```
SystemManager/
├── Program.cs              # Application entry point
├── MainForm.cs             # Main UI container
├── Modules/
│   ├── IModule.cs          # Module interface contract
│   ├── ModuleManager.cs    # Module lifecycle manager
│   └── [Specific Modules]  # Individual feature implementations
└── Utilities/              # Shared helper classes
    ├── RegistryHelper.cs   # Windows Registry operations
    ├── ProcessHelper.cs    # Process management
    └── NetworkHelper.cs    # Network operations
```

## Module System

### IModule Interface

Every module must implement the `IModule` interface:

```csharp
public interface IModule
{
    string ModuleName { get; }           // Display name
    string Description { get; }          // Module description
    bool IsEnabled { get; set; }         // Activation state
    
    void Initialize();                    // Setup resources
    void Start();                         // Begin operations
    void Stop();                          // Pause operations
    void Dispose();                       // Cleanup resources
    Control GetControl();                 // Return UI control
    
    event EventHandler ConfigurationChanged;
}
```

### Module Lifecycle

1. **Registration**: Module is registered with `ModuleManager`
2. **Initialization**: `Initialize()` called when module is enabled
3. **Start**: `Start()` begins module operations
4. **Running**: Module performs its functions
5. **Stop**: `Stop()` pauses operations when disabled
6. **Disposal**: `Dispose()` releases all resources

### ModuleManager

The `ModuleManager` class handles all module operations:

```csharp
public class ModuleManager
{
    // Register a module (doesn't activate it)
    public void RegisterModule(IModule module)
    
    // Enable a module (initializes and starts)
    public void EnableModule(string moduleName)
    
    // Disable a module (stops and disposes)
    public void DisableModule(string moduleName)
    
    // Check if module is enabled
    public bool IsModuleEnabled(string moduleName)
    
    // Get all enabled modules
    public IEnumerable<IModule> GetEnabledModules()
}
```

## Module Implementations

### 1. Activity Watcher Module

**Purpose**: Monitor browser activity and track time spent on domains

**Key Classes**:
- `ActivityWatcherModule`: UI and module interface
- `BrowserMonitor`: Core monitoring logic
- `DomainActivity`: Activity data model

**Technology**:
- Win32 API for window enumeration
- Timer-based polling (1 second intervals)
- Dictionary-based activity tracking

**Key Methods**:
```csharp
GetForegroundWindow()      // Get active window
GetWindowText()            // Extract window title
ExtractDomain()            // Parse domain from URL
TrackDomainActivity()      // Update activity statistics
GetDailyLogs()             // Generate daily reports
```

### 2. Network Monitor Module

**Purpose**: Real-time network performance monitoring

**Key Classes**:
- `NetworkMonitorModule`: UI and visualization
- `SignalStrengthMonitor`: Network sampling
- `NetworkStatistics`: Performance metrics

**Technology**:
- `NetworkInterface` API for adapter info
- WMI for WiFi signal strength
- Real-time graphing with GDI+

**Metrics Tracked**:
- Download/Upload speeds (bytes/sec)
- Signal strength (0-100%)
- Connection stability (%)
- Jitter (ms)
- Peak speeds

**Sampling Strategy**:
```csharp
// Sample every 1 second
// Keep 3600 samples (1 hour)
// Calculate rolling averages
// Detect anomalies and instability
```

### 3. WiFi Guardian Module

**Purpose**: Monitor WiFi connection status

**Key Features**:
- Checkbox-controlled activation (NOT auto-start)
- Connection state monitoring
- Event logging
- Alert notifications

**Technology**:
- `NetworkInterface` API
- Timer-based checking (2 second intervals)
- Status change detection

**Important Behavior**:
```csharp
// Module loads but does NOT start monitoring
// Monitoring only starts when checkbox is checked
// Explicitly designed to prevent auto-start
```

### 4. Startup Manager Module

**Purpose**: Manage Windows startup programs

**Data Sources**:
1. **Registry Locations**:
   - `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
   - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`
   - `RunOnce` variants

2. **Task Scheduler**:
   - Boot triggers
   - Logon triggers
   - Scheduled startup tasks

**Technology**:
- `Microsoft.Win32.Registry` API
- Task Scheduler COM API
- WMI for service management

### 5. System Cleaning Module

**Purpose**: Deep system cleaning and restore point management

**Key Classes**:
- `SystemCleaningModule`: UI and orchestration
- `RestorePointManager`: WMI-based restore points

**Cleaning Targets**:
- Temporary files (`%TEMP%`)
- Browser caches
- Windows Update cache
- Thumbnails cache
- Recycle Bin
- System log files

**Restore Point Creation**:
```csharp
// Using WMI (not vssadmin)
ManagementScope("\\\\localhost\\root\\default")
SystemRestore.CreateRestorePoint(description, type, eventType)
```

**Registry Backup**:
```csharp
// Using reg.exe command
reg export HKLM backup.reg /y
```

### 6. Deep Uninstaller Module

**Purpose**: Uninstall programs and remove leftovers

**Features**:
- List installed programs from Registry
- Execute uninstall strings
- Detect leftover files
- Clean registry entries

**Registry Locations**:
- `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`
- `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` (64-bit)

**Cleanup Process**:
1. Execute official uninstaller
2. Scan for leftover files in Program Files
3. Check for registry entries
4. Remove shortcuts
5. Clean Start Menu entries

### 7. Security Module

**Purpose**: Antivirus scanning and firewall monitoring

**Key Classes**:
- `SecurityModule`: UI and coordination
- `AntivirusScanner`: File scanning engine

**Scan Types**:
- **Quick Scan**: User directory, common locations
- **Medium Scan**: System directories, recursively
- **Deep Scan**: Full disk scan with all files

**Scanning Strategy**:
```csharp
// File-based signature matching
// EICAR test file detection
// Hash-based identification
// Behavior analysis (future)
```

**Firewall Monitoring**:
- Active TCP connections
- Blocked connection logging
- Connection state tracking
- Protocol analysis

### 8. Password Manager Module

**Purpose**: Secure password storage with 2FA

**Key Classes**:
- `PasswordManagerModule`: UI
- `PasswordGenerator`: Cryptographic password generation
- `TwoFactorAuth`: TOTP implementation

**Security Features**:
- SHA-256 password hashing
- RNG-based password generation
- TOTP 2FA codes (30-second intervals)
- Service-based organization

**Password Generation**:
```csharp
// Cryptographically secure random
using RNGCryptoServiceProvider
// Character pools: lowercase, uppercase, digits, special
// Configurable length and complexity
```

**TOTP Implementation**:
```csharp
// RFC 6238 compliant
// HMAC-SHA1 algorithm
// 30-second time steps
// 6-digit codes
```

### 9. System Information Module

**Purpose**: Display comprehensive hardware information

**Key Classes**:
- `SystemInfoModule`: UI
- `HardwareMonitor`: WMI-based information gathering

**Information Gathered**:
- **CPU**: Name, cores, threads, temperature
- **RAM**: Total capacity, slot information
- **GPU**: Model, VRAM (if available)
- **OS**: Version, build, install date
- **Network**: Local IP, external IP, DNS servers
- **Uptime**: System uptime counter

**Temperature Monitoring**:
```csharp
// Using WMI
SELECT * FROM MSAcpi_ThermalZoneTemperature
// Convert from Kelvin to Celsius
temperature_celsius = (temp_kelvin - 2732) / 10
```

### 10. Logging Module

**Purpose**: Centralized application logging

**Features**:
- Time-stamped log entries
- Module-based categorization
- Log level filtering
- Export functionality

**Log Levels**:
- Info: General information
- Warning: Potential issues
- Error: Error conditions
- Debug: Detailed debugging info

## Utility Classes

### RegistryHelper

**Purpose**: Simplify Windows Registry operations

**Key Methods**:
```csharp
GetStartupPrograms()        // List all startup items
BackupRegistryKey()         // Export registry key
```

### ProcessHelper

**Purpose**: Process management and monitoring

**Key Methods**:
```csharp
GetRunningProcesses()       // List all processes
GetProcessCommandLine()     // Get process command
TerminateProcess()          // Kill a process safely
```

### NetworkHelper

**Purpose**: Network operations and information

**Key Methods**:
```csharp
GetNetworkAdapters()        // List all adapters
GetLocalIPAddress()         // Get local IP
GetExternalIPAddress()      // Get public IP via API
GetDNSServers()            // List DNS servers
PingHost()                  // Measure latency
GetActiveTCPConnections()   // List connections
```

## User Interface

### Main Form Layout

```
┌─────────────────────────────────────────────┐
│  System Manager    [Start w/ Win] [Exit]   │
├───────────┬─────────────────────────────────┤
│           │                                 │
│  Module   │                                 │
│  List     │       Tab Control               │
│           │    (Module Content Area)        │
│  Activity │                                 │
│  Network  │                                 │
│  WiFi     │                                 │
│  Startup  │                                 │
│  ...      │                                 │
│           │                                 │
└───────────┴─────────────────────────────────┘
```

### Tray Icon Features

- Minimize to tray on close
- Right-click context menu
- Double-click to restore
- Balloon tip notifications

## Data Flow

### Activity Tracking Flow

```
Browser Window
    ↓
Win32 API (GetForegroundWindow)
    ↓
Extract URL from Title
    ↓
Parse Domain
    ↓
Update Activity Dictionary
    ↓
Store in Daily Logs
    ↓
Display in UI
```

### Network Monitoring Flow

```
Network Interfaces
    ↓
Get Statistics (bytes sent/received)
    ↓
Calculate Speed (bytes/sec)
    ↓
Measure Signal Strength (WMI)
    ↓
Store Sample (1 second)
    ↓
Calculate Metrics (avg, peak, stability)
    ↓
Update UI and Graph
```

## Threading Model

### UI Thread
- Main form and controls
- User interactions
- Display updates

### Background Threads
- Timer-based monitoring (Activity, Network, WiFi)
- File scanning (Security)
- Long-running operations

### Thread Safety
- `Invoke()` for UI updates from background threads
- Lock objects for shared data structures
- Thread-safe collections where appropriate

## Performance Optimization

### Memory Management
- Limit historical data (e.g., 3600 network samples)
- Dispose resources properly
- Use `using` statements for IDisposable

### CPU Usage
- Adjustable polling intervals
- Efficient algorithms
- Lazy loading of modules

### Startup Time
- Modules initialized on-demand
- Parallel initialization (future)
- Cached data where appropriate

## Error Handling

### Strategy
- Graceful degradation
- Silent handling of non-critical errors
- User notification for critical errors
- Detailed logging for debugging

### Example Pattern
```csharp
try
{
    // Risky operation
}
catch (SecurityException)
{
    // Handle security-specific error
}
catch (Exception ex)
{
    // Log error
    // Notify user if critical
    // Continue execution
}
```

## Security Considerations

### Privilege Requirements
- Admin rights for system modifications
- UAC elevation prompts
- Minimal privilege principle

### Data Protection
- SHA-256 password hashing
- Secure random generation
- Local-only storage (no cloud sync)

### Input Validation
- Sanitize file paths
- Validate URLs
- Check process IDs

## Extension Points

### Adding New Modules

1. Create class implementing `IModule`
2. Implement required interface methods
3. Create UI control
4. Register in `MainForm.InitializeModules()`

### Adding New Utilities

1. Create static helper class
2. Implement public methods
3. Add XML documentation
4. Use from modules as needed

## Testing Strategy

### Unit Tests (Future)
- Utility class methods
- Module initialization
- Data transformation logic

### Integration Tests (Future)
- Module interaction
- Registry operations
- Network operations

### Manual Testing
- All features tested on Windows 10/11
- Admin and non-admin scenarios
- Various hardware configurations

## Known Limitations

1. **Browser Monitoring**: Limited to window title parsing
2. **Temperature**: Not supported on all hardware
3. **WiFi Signal**: Only for WiFi adapters
4. **Task Scheduler**: Requires Microsoft.Win32.TaskScheduler package
5. **.NET Framework**: Windows-only, not cross-platform

## Future Enhancements

### Planned Features
- Database integration for AV signatures
- Cloud password sync (encrypted)
- Email notifications
- VPN protocol support
- SSH/Telnet terminal
- FTP/SFTP file transfer
- Advanced packet capture

### Architecture Improvements
- Plugin hot-loading
- Auto-update mechanism
- Multi-language support
- Theme customization
- Configuration profiles

## Appendix

### API References
- Win32 API: Window management
- WMI: System information
- Registry API: Windows Registry
- Network API: NetworkInformation namespace

### External Dependencies
- .NET Framework 4.8
- System.Management (WMI)
- System.Windows.Forms (UI)
- Microsoft.Win32.TaskScheduler (optional)

### Code Metrics
- Total Lines of Code: ~4500+
- Number of Modules: 17
- Utility Classes: 3
- UI Forms: 1 main + 17 module UIs

---

**Document Version:** 1.0  
**Last Updated:** December 2025  
**Maintainer:** Development Team
