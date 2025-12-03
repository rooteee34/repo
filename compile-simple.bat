@echo off
REM ============================================================================
REM System Manager - Simple MSBuild Compiler (Alternative)
REM This is an alternative if direct CSC compilation has issues
REM ============================================================================

echo.
echo ========================================
echo System Manager - MSBuild Compiler
echo ========================================
echo.

REM Try to find MSBuild
set "MSBUILD="

REM Check Visual Studio 2019
if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
)

REM Check Visual Studio 2022
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
)

REM Check .NET Framework
if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" (
    set "MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)

if "%MSBUILD%"=="" (
    echo ERROR: MSBuild not found!
    echo Please install Visual Studio or .NET Framework SDK.
    pause
    exit /b 1
)

echo Found MSBuild at: %MSBUILD%
echo.

echo Building SystemManager.sln...
"%MSBUILD%" "%~dp0SystemManager.sln" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal

if errorlevel 1 (
    echo.
    echo BUILD FAILED!
    pause
    exit /b 1
)

echo.
echo ========================================
echo BUILD SUCCESSFUL!
echo ========================================
echo.
echo Output: SystemManager\bin\Release\SystemManager.exe
echo.
pause
exit /b 0
