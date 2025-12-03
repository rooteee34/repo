@echo off
REM ============================================================================
REM System Manager - Project Verification Script
REM Checks if all required files are present
REM ============================================================================

echo.
echo ========================================
echo System Manager - Project Verification
echo ========================================
echo.

set "ERROR_COUNT=0"
set "PROJECT_DIR=%~dp0SystemManager"

echo Checking required files...
echo.

REM Check main files
echo [Core Files]
if exist "%PROJECT_DIR%\Program.cs" (
    echo   ✓ Program.cs
) else (
    echo   ✗ Program.cs MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\MainForm.cs" (
    echo   ✓ MainForm.cs
) else (
    echo   ✗ MainForm.cs MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\SystemManager.csproj" (
    echo   ✓ SystemManager.csproj
) else (
    echo   ✗ SystemManager.csproj MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\App.config" (
    echo   ✓ App.config
) else (
    echo   ✗ App.config MISSING!
    set /a ERROR_COUNT+=1
)

echo.
echo [Module Infrastructure]
if exist "%PROJECT_DIR%\Modules\IModule.cs" (
    echo   ✓ IModule.cs
) else (
    echo   ✗ IModule.cs MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\ModuleManager.cs" (
    echo   ✓ ModuleManager.cs
) else (
    echo   ✗ ModuleManager.cs MISSING!
    set /a ERROR_COUNT+=1
)

echo.
echo [Modules - Implemented]
set "MODULE_COUNT=0"

if exist "%PROJECT_DIR%\Modules\ActivityWatcher\ActivityWatcherModule.cs" (
    echo   ✓ Activity Watcher
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Activity Watcher MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\NetworkMonitor\NetworkMonitorModule.cs" (
    echo   ✓ Network Monitor
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Network Monitor MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\WiFiGuardian\WiFiGuardianModule.cs" (
    echo   ✓ WiFi Guardian
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ WiFi Guardian MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\StartupManager\StartupManagerModule.cs" (
    echo   ✓ Startup Manager
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Startup Manager MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\SystemCleaning\SystemCleaningModule.cs" (
    echo   ✓ System Cleaning
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ System Cleaning MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\Uninstaller\DeepUninstallerModule.cs" (
    echo   ✓ Deep Uninstaller
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Deep Uninstaller MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\Security\SecurityModule.cs" (
    echo   ✓ Security
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Security MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\PasswordManager\PasswordManagerModule.cs" (
    echo   ✓ Password Manager
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Password Manager MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\SystemInfo\SystemInfoModule.cs" (
    echo   ✓ System Info
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ System Info MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Modules\Logging\LoggingModule.cs" (
    echo   ✓ Logging
    set /a MODULE_COUNT+=1
) else (
    echo   ✗ Logging MISSING!
    set /a ERROR_COUNT+=1
)

echo.
echo [Utility Classes]
if exist "%PROJECT_DIR%\Utilities\RegistryHelper.cs" (
    echo   ✓ RegistryHelper
) else (
    echo   ✗ RegistryHelper MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Utilities\ProcessHelper.cs" (
    echo   ✓ ProcessHelper
) else (
    echo   ✗ ProcessHelper MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Utilities\NetworkHelper.cs" (
    echo   ✓ NetworkHelper
) else (
    echo   ✗ NetworkHelper MISSING!
    set /a ERROR_COUNT+=1
)

echo.
echo [Resources]
if exist "%PROJECT_DIR%\MainForm.resx" (
    echo   ✓ MainForm.resx
) else (
    echo   ✗ MainForm.resx MISSING!
    set /a ERROR_COUNT+=1
)

if exist "%PROJECT_DIR%\Properties\Resources.resx" (
    echo   ✓ Resources.resx
) else (
    echo   ✗ Resources.resx MISSING!
    set /a ERROR_COUNT+=1
)

echo.
echo ========================================
echo Verification Results
echo ========================================
echo Total Modules Found: %MODULE_COUNT%
echo Errors Found: %ERROR_COUNT%
echo.

if %ERROR_COUNT% EQU 0 (
    echo ✓ All required files are present!
    echo ✓ Project is ready to compile!
    echo.
    echo Next step: Run compile.bat to build the project
) else (
    echo ✗ Some files are missing!
    echo ✗ Please ensure all files are present before compiling
)

echo.
echo ========================================
echo.
pause
exit /b %ERROR_COUNT%
