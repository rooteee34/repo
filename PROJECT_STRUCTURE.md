# Project Structure

## Directory Layout

```
repo/
├── README.md                          # Project overview and features
├── BUILD_GUIDE.md                     # Build and deployment guide
├── TECHNICAL_DOCUMENTATION.md         # Architecture and design
├── QUICKSTART.md                      # Quick start guide
├── FEATURES.md                        # Feature implementation matrix
├── PROJECT_STRUCTURE.md               # This file
├── .gitignore                         # Git exclusions
├── SystemManager.sln                  # Visual Studio solution
│
└── SystemManager/                     # Main project directory
    ├── App.config                     # Application configuration
    ├── Program.cs                     # Application entry point
    ├── SystemManager.csproj           # Project file
    ├── packages.config                # NuGet packages
    │
    ├── MainForm.cs                    # Main application window
    ├── MainForm.Designer.cs           # Form designer code
    ├── MainForm.resx                  # Form resources
    │
    ├── Properties/                    # Assembly properties
    │   ├── AssemblyInfo.cs           # Assembly information
    │   ├── Resources.resx            # Embedded resources
    │   ├── Resources.Designer.cs     # Resource designer
    │   ├── Settings.settings         # Application settings
    │   └── Settings.Designer.cs      # Settings designer
    │
    ├── Modules/                       # All modules directory
    │   ├── IModule.cs                # Module interface contract
    │   ├── ModuleManager.cs          # Module lifecycle manager
    │   │
    │   ├── ActivityWatcher/          # Browser activity monitoring
    │   │   ├── ActivityWatcherModule.cs
    │   │   └── BrowserMonitor.cs
    │   │
    │   ├── NetworkMonitor/           # Network performance monitoring
    │   │   ├── NetworkMonitorModule.cs
    │   │   └── SignalStrengthMonitor.cs
    │   │
    │   ├── WiFiGuardian/             # WiFi connection monitoring
    │   │   └── WiFiGuardianModule.cs
    │   │
    │   ├── StartupManager/           # Windows startup management
    │   │   └── StartupManagerModule.cs
    │   │
    │   ├── SystemCleaning/           # System cleanup and maintenance
    │   │   ├── SystemCleaningModule.cs
    │   │   └── RestorePointManager.cs
    │   │
    │   ├── Uninstaller/              # Program uninstaller
    │   │   └── DeepUninstallerModule.cs
    │   │
    │   ├── Security/                 # Security and antivirus
    │   │   ├── SecurityModule.cs
    │   │   └── AntivirusScanner.cs
    │   │
    │   ├── Optimization/             # System optimization
    │   │   └── OptimizationModule.cs
    │   │
    │   ├── PasswordManager/          # Password management
    │   │   ├── PasswordManagerModule.cs
    │   │   ├── PasswordGenerator.cs
    │   │   └── TwoFactorAuth.cs
    │   │
    │   ├── SystemInfo/               # Hardware information
    │   │   ├── SystemInfoModule.cs
    │   │   └── HardwareMonitor.cs
    │   │
    │   ├── Alarms/                   # Alarms and reminders
    │   │   └── AlarmModule.cs
    │   │
    │   ├── VPN/                      # VPN functionality
    │   │   └── VPNModule.cs
    │   │
    │   ├── Terminal/                 # Terminal emulator
    │   │   └── TerminalModule.cs
    │   │
    │   ├── FileTransfer/             # File transfer
    │   │   └── FileTransferModule.cs
    │   │
    │   ├── DNS/                      # Smart DNS
    │   │   └── SmartDNSModule.cs
    │   │
    │   ├── PacketAnalyzer/           # Network packet analysis
    │   │   └── PacketAnalyzerModule.cs
    │   │
    │   └── Logging/                  # Application logging
    │       └── LoggingModule.cs
    │
    └── Utilities/                     # Shared utility classes
        ├── RegistryHelper.cs         # Windows Registry operations
        ├── ProcessHelper.cs          # Process management
        └── NetworkHelper.cs          # Network operations
```

## Module Organization

### Core Infrastructure
- **Program.cs** - Application entry point
- **MainForm.cs** - Main window with tab control
- **ModuleManager.cs** - Module lifecycle management

### Module Pattern
Each module follows this structure:
```
ModuleName/
├── ModuleNameModule.cs    # Main module class (implements IModule)
├── Helper1.cs             # Supporting class 1
└── Helper2.cs             # Supporting class 2
```

### Utility Pattern
Shared utilities are static helper classes:
```csharp
public static class HelperName
{
    public static ReturnType MethodName(params) { }
}
```

## Class Hierarchy

```
IModule (interface)
│
├── ActivityWatcherModule
│   └── uses BrowserMonitor
│
├── NetworkMonitorModule
│   └── uses SignalStrengthMonitor
│
├── WiFiGuardianModule
│
├── StartupManagerModule
│
├── SystemCleaningModule
│   └── uses RestorePointManager
│
├── DeepUninstallerModule
│
├── SecurityModule
│   └── uses AntivirusScanner
│
├── OptimizationModule
│
├── PasswordManagerModule
│   ├── uses PasswordGenerator
│   └── uses TwoFactorAuth
│
├── SystemInfoModule
│   └── uses HardwareMonitor
│
├── AlarmModule
├── VPNModule
├── TerminalModule
├── FileTransferModule
├── SmartDNSModule
├── PacketAnalyzerModule
└── LoggingModule
```

## Data Flow

### Module Lifecycle
```
Registration → Initialization → Start → Running → Stop → Disposal
     ↓              ↓             ↓        ↓        ↓        ↓
ModuleManager  Initialize()  Start()  Active  Stop()  Dispose()
```

### UI Flow
```
User Selects Module
        ↓
ModuleManager.EnableModule()
        ↓
Module.Initialize()
        ↓
Module.Start()
        ↓
Module.GetControl() → Display in Tab
        ↓
Module Running
        ↓
User Disables Module
        ↓
Module.Stop()
        ↓
Module.Dispose()
```

## File Relationships

### Main Form Dependencies
```
MainForm.cs
├── requires Program.cs
├── requires ModuleManager.cs
├── requires IModule.cs
└── instantiates all *Module.cs classes
```

### Module Dependencies
```
*Module.cs
├── implements IModule.cs
├── may use Utilities/*.cs
└── may have helper classes in same folder
```

### Utility Dependencies
```
Utilities/*.cs
├── static helper classes
├── no dependencies on modules
└── used by multiple modules
```

## Build Order

1. **Utilities** (no dependencies)
   - RegistryHelper.cs
   - ProcessHelper.cs
   - NetworkHelper.cs

2. **Module Infrastructure** (depends on utilities)
   - IModule.cs
   - ModuleManager.cs

3. **Module Implementations** (depends on infrastructure + utilities)
   - All *Module.cs files
   - Helper classes per module

4. **Main Application** (depends on everything)
   - MainForm.cs
   - Program.cs

## Configuration Files

| File | Purpose | Format |
|------|---------|--------|
| App.config | Application settings | XML |
| SystemManager.csproj | Project configuration | MSBuild XML |
| packages.config | NuGet packages | XML |
| *.resx | Embedded resources | ResX XML |
| *.settings | User settings | XML |

## Resource Files

| File | Type | Purpose |
|------|------|---------|
| MainForm.resx | Form | Main window resources |
| Resources.resx | Application | Global resources |
| AssemblyInfo.cs | Assembly | Version and metadata |

## Key Interfaces

### IModule Interface
```csharp
public interface IModule
{
    string ModuleName { get; }
    string Description { get; }
    bool IsEnabled { get; set; }
    
    void Initialize();
    void Start();
    void Stop();
    void Dispose();
    Control GetControl();
    
    event EventHandler ConfigurationChanged;
}
```

### Module Pattern
```csharp
public class ExampleModule : IModule
{
    private UserControl _control;
    private Timer _updateTimer;
    
    public string ModuleName => "Example";
    public string Description => "Example module";
    public bool IsEnabled { get; set; }
    
    public void Initialize() { /* Setup */ }
    public void Start() { /* Begin work */ }
    public void Stop() { /* Pause work */ }
    public void Dispose() { /* Cleanup */ }
    public Control GetControl() { return _control; }
    
    public event EventHandler ConfigurationChanged;
}
```

## Dependencies Graph

```
Program.cs
    └── MainForm.cs
            ├── ModuleManager.cs
            │       └── IModule.cs
            │               ├── ActivityWatcherModule.cs
            │               │       └── BrowserMonitor.cs
            │               ├── NetworkMonitorModule.cs
            │               │       └── SignalStrengthMonitor.cs
            │               ├── WiFiGuardianModule.cs
            │               ├── StartupManagerModule.cs
            │               ├── SystemCleaningModule.cs
            │               │       └── RestorePointManager.cs
            │               ├── DeepUninstallerModule.cs
            │               ├── SecurityModule.cs
            │               │       └── AntivirusScanner.cs
            │               ├── OptimizationModule.cs
            │               ├── PasswordManagerModule.cs
            │               │       ├── PasswordGenerator.cs
            │               │       └── TwoFactorAuth.cs
            │               ├── SystemInfoModule.cs
            │               │       └── HardwareMonitor.cs
            │               └── [Other Modules...]
            │
            └── Utilities/
                    ├── RegistryHelper.cs
                    ├── ProcessHelper.cs
                    └── NetworkHelper.cs
```

## Assembly References

### .NET Framework
- System
- System.Core
- System.Management
- System.Windows.Forms
- System.Drawing
- System.Net.Http
- System.Security
- System.ServiceProcess

### Optional NuGet
- TaskScheduler (for enhanced Task Scheduler features)

## Lines of Code by Component

| Component | LOC | Percentage |
|-----------|-----|------------|
| Modules | ~3,500 | 78% |
| Utilities | ~500 | 11% |
| Main Form | ~250 | 5.5% |
| Properties | ~250 | 5.5% |
| **Total** | **~4,500** | **100%** |

## Module Statistics

| Module | Files | LOC | Status |
|--------|-------|-----|--------|
| Activity Watcher | 2 | ~365 | ✅ Complete |
| Network Monitor | 2 | ~555 | ✅ Complete |
| WiFi Guardian | 1 | ~200 | ✅ Complete |
| Startup Manager | 1 | ~150 | ✅ Complete |
| System Cleaning | 2 | ~280 | ✅ Complete |
| Deep Uninstaller | 1 | ~180 | ✅ Complete |
| Security | 2 | ~300 | ✅ Complete |
| Password Manager | 3 | ~230 | ✅ Complete |
| System Info | 2 | ~240 | ✅ Complete |
| Optimization | 1 | ~40 | 🚧 Stub |
| Alarms | 1 | ~30 | 🚧 Stub |
| VPN | 1 | ~30 | 🚧 Stub |
| Terminal | 1 | ~30 | 🚧 Stub |
| File Transfer | 1 | ~30 | 🚧 Stub |
| Smart DNS | 1 | ~30 | 🚧 Stub |
| Packet Analyzer | 1 | ~30 | 🚧 Stub |
| Logging | 1 | ~80 | ✅ Complete |

## Documentation Statistics

| Document | Size | Lines | Purpose |
|----------|------|-------|---------|
| README.md | 4 KB | 140 | Overview |
| BUILD_GUIDE.md | 8.5 KB | 340 | Building |
| TECHNICAL_DOCUMENTATION.md | 15 KB | 600 | Architecture |
| QUICKSTART.md | 8.8 KB | 350 | Quick start |
| FEATURES.md | 15 KB | 650 | Features |
| PROJECT_STRUCTURE.md | This | ~500 | Structure |

## Build Outputs

### Debug Build
```
SystemManager/bin/Debug/
├── SystemManager.exe
├── SystemManager.exe.config
├── SystemManager.pdb
└── [Referenced DLLs]
```

### Release Build
```
SystemManager/bin/Release/
├── SystemManager.exe
├── SystemManager.exe.config
└── [Referenced DLLs]
```

## Key Design Patterns

1. **Plugin Architecture** - Modules as plugins
2. **Interface-Based Design** - IModule interface
3. **Singleton Manager** - ModuleManager
4. **Observer Pattern** - Module events
5. **Factory Pattern** - Module creation
6. **Facade Pattern** - Utility helpers

## Testing Strategy

### Module Testing
- Each module can be tested independently
- Mock IModule for testing ModuleManager
- Unit test utility classes

### Integration Testing
- Test module initialization
- Test module lifecycle
- Test UI integration

### Manual Testing
- All features tested on Windows 10/11
- Various hardware configurations
- Admin and non-admin scenarios

## Extensibility Points

### Adding Modules
1. Create class implementing IModule
2. Place in Modules/ directory
3. Register in MainForm.InitializeModules()

### Adding Utilities
1. Create static helper class
2. Place in Utilities/ directory
3. Use from any module

### Customizing UI
1. Each module returns its own Control
2. Modify Control in module's GetControl()
3. Use WinForms designer or code

---

**This structure provides:**
- ✅ Clear organization
- ✅ Easy navigation
- ✅ Simple extension
- ✅ Maintainable code
- ✅ Testable components
