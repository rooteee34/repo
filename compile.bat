@echo off
REM ============================================================================
REM System Manager - .NET Framework 4.8 Batch Compiler
REM This script compiles the project using csc.exe without MSBuild
REM ============================================================================

echo.
echo ========================================
echo System Manager Batch Compiler
echo ========================================
echo.

REM Set project paths
set "PROJECT_DIR=%~dp0SystemManager"
set "OUTPUT_DIR=%PROJECT_DIR%\bin\Release"
set "TEMP_DIR=%PROJECT_DIR%\obj\Release"

REM .NET Framework 4.8 paths (adjust if needed)
set "DOTNET_FX=C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
set "CSC=%DOTNET_FX%\csc.exe"
set "RESGEN=%DOTNET_FX%\ResGen.exe"

REM Check if .NET Framework 4.8 is installed
if not exist "%CSC%" (
    echo ERROR: .NET Framework 4.8 C# compiler not found!
    echo Expected location: %CSC%
    echo.
    echo Please install .NET Framework 4.8 or update the path in this script.
    echo Download: https://dotnet.microsoft.com/download/dotnet-framework/net48
    pause
    exit /b 1
)

echo Found .NET Framework 4.8 at: %DOTNET_FX%
echo.

REM Create output directories
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"

echo Creating output directories...
echo   Output: %OUTPUT_DIR%
echo   Temp:   %TEMP_DIR%
echo.

REM ============================================================================
REM Step 1: Generate resources from .resx files
REM ============================================================================

echo [1/4] Generating resources from .resx files...

"%RESGEN%" "%PROJECT_DIR%\MainForm.resx" "%TEMP_DIR%\SystemManager.MainForm.resources"
if errorlevel 1 goto :error

"%RESGEN%" "%PROJECT_DIR%\Properties\Resources.resx" "%TEMP_DIR%\SystemManager.Properties.Resources.resources"
if errorlevel 1 goto :error

echo   ✓ Resources generated successfully
echo.

REM ============================================================================
REM Step 2: Compile all C# source files
REM ============================================================================

echo [2/4] Compiling C# source files...

REM List all source files
set "SOURCES=%PROJECT_DIR%\Program.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\MainForm.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\MainForm.Designer.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Properties\AssemblyInfo.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Properties\Resources.Designer.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Properties\Settings.Designer.cs"

REM Module infrastructure
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\IModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\ModuleManager.cs"

REM Activity Watcher
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\ActivityWatcher\ActivityWatcherModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\ActivityWatcher\BrowserMonitor.cs"

REM Network Monitor
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\NetworkMonitor\NetworkMonitorModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\NetworkMonitor\SignalStrengthMonitor.cs"

REM WiFi Guardian
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\WiFiGuardian\WiFiGuardianModule.cs"

REM Startup Manager
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\StartupManager\StartupManagerModule.cs"

REM System Cleaning
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\SystemCleaning\SystemCleaningModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\SystemCleaning\RestorePointManager.cs"

REM Uninstaller
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Uninstaller\DeepUninstallerModule.cs"

REM Security
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Security\SecurityModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Security\AntivirusScanner.cs"

REM Optimization
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Optimization\OptimizationModule.cs"

REM Password Manager
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\PasswordManager\PasswordManagerModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\PasswordManager\PasswordGenerator.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\PasswordManager\TwoFactorAuth.cs"

REM System Info
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\SystemInfo\SystemInfoModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\SystemInfo\HardwareMonitor.cs"

REM Other modules
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Alarms\AlarmModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\VPN\VPNModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Terminal\TerminalModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\FileTransfer\FileTransferModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\DNS\SmartDNSModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\PacketAnalyzer\PacketAnalyzerModule.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Modules\Logging\LoggingModule.cs"

REM Utilities
set "SOURCES=%SOURCES% %PROJECT_DIR%\Utilities\RegistryHelper.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Utilities\ProcessHelper.cs"
set "SOURCES=%SOURCES% %PROJECT_DIR%\Utilities\NetworkHelper.cs"

REM System references
set "REFERENCES=/r:System.dll"
set "REFERENCES=%REFERENCES% /r:System.Core.dll"
set "REFERENCES=%REFERENCES% /r:System.Management.dll"
set "REFERENCES=%REFERENCES% /r:System.Xml.Linq.dll"
set "REFERENCES=%REFERENCES% /r:System.Data.DataSetExtensions.dll"
set "REFERENCES=%REFERENCES% /r:Microsoft.CSharp.dll"
set "REFERENCES=%REFERENCES% /r:System.Data.dll"
set "REFERENCES=%REFERENCES% /r:System.Deployment.dll"
set "REFERENCES=%REFERENCES% /r:System.Drawing.dll"
set "REFERENCES=%REFERENCES% /r:System.Net.Http.dll"
set "REFERENCES=%REFERENCES% /r:System.Windows.Forms.dll"
set "REFERENCES=%REFERENCES% /r:System.Xml.dll"
set "REFERENCES=%REFERENCES% /r:System.Security.dll"
set "REFERENCES=%REFERENCES% /r:System.ServiceProcess.dll"

REM Embedded resources
set "RESOURCES=/resource:%TEMP_DIR%\SystemManager.MainForm.resources,SystemManager.MainForm.resources"
set "RESOURCES=%RESOURCES% /resource:%TEMP_DIR%\SystemManager.Properties.Resources.resources,SystemManager.Properties.Resources.resources"

REM Compiler options
set "OPTIONS=/target:winexe"
set "OPTIONS=%OPTIONS% /out:%OUTPUT_DIR%\SystemManager.exe"
set "OPTIONS=%OPTIONS% /optimize+"
set "OPTIONS=%OPTIONS% /platform:anycpu"
set "OPTIONS=%OPTIONS% /warn:4"
set "OPTIONS=%OPTIONS% /nologo"
set "OPTIONS=%OPTIONS% /utf8output"

REM Compile
"%CSC%" %OPTIONS% %REFERENCES% %RESOURCES% %SOURCES%

if errorlevel 1 goto :error

echo   ✓ Compilation successful
echo.

REM ============================================================================
REM Step 3: Copy App.config to output directory
REM ============================================================================

echo [3/4] Copying configuration files...

copy /Y "%PROJECT_DIR%\App.config" "%OUTPUT_DIR%\SystemManager.exe.config" >nul
if errorlevel 1 goto :error

echo   ✓ App.config copied
echo.

REM ============================================================================
REM Step 4: Display build information
REM ============================================================================

echo [4/4] Build completed successfully!
echo.
echo ========================================
echo Build Information
echo ========================================
echo Output file: %OUTPUT_DIR%\SystemManager.exe
echo Configuration: Release
echo Platform: AnyCPU
echo Target Framework: .NET Framework 4.8
echo ========================================
echo.

REM Check if executable was created
if exist "%OUTPUT_DIR%\SystemManager.exe" (
    echo ✓ SystemManager.exe created successfully!
    dir "%OUTPUT_DIR%\SystemManager.exe" | find "SystemManager.exe"
) else (
    echo ERROR: SystemManager.exe was not created!
    goto :error
)

echo.
echo Build completed! You can now run:
echo   %OUTPUT_DIR%\SystemManager.exe
echo.
pause
exit /b 0

:error
echo.
echo ========================================
echo BUILD FAILED!
echo ========================================
echo.
echo An error occurred during compilation.
echo Please check the error messages above.
echo.
pause
exit /b 1
