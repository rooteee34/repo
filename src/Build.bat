@echo off
setlocal enabledelayedexpansion
title Anoreksik Suite V10 Builder

echo ========================================================
echo   ANOREKSIK SUITE V10 - OTO DERLEYICI
echo ========================================================
echo.

:: 1. CSC.EXE (C# Compiler) Konumunu Bul
echo [1/3] Derleyici (csc.exe) araniyor...
set "CSC_PATH="

:: Once en yeni Framework'e bak (v4.0.30319 genellikle sonuncusudur)
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC_PATH=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
) else (
    if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
        set "CSC_PATH=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
    )
)

if "!CSC_PATH!"=="" (
    echo [HATA] .NET Framework 4.5+ derleyicisi bulunamadi!
    echo Lutfen Windows sisteminizde .NET Framework yuklu oldugundan emin olun.
    pause
    exit /b 1
)

echo     Bulundu: !CSC_PATH!
echo.

:: 2. Derleme Islemi
echo [2/3] Kodlar derleniyor...

:: Referanslar
set "REFS=/reference:System.dll"
set "REFS=!REFS!,System.Windows.Forms.dll"
set "REFS=!REFS!,System.Drawing.dll"
set "REFS=!REFS!,System.Core.dll"
set "REFS=!REFS!,System.Data.dll"
set "REFS=!REFS!,System.Management.dll"
set "REFS=!REFS!,System.Security.dll"

:: Kaynak Dosyalar
set "SOURCES=Program.cs MainForm.cs MainForm.Designer.cs NativeMethods.cs Modules\*.cs"

:: Derleme Komutu
"!CSC_PATH!" /target:winexe /out:AnoreksikSuiteV10.exe /platform:anycpu /optimize+ /warn:0 !REFS! !SOURCES!

if %errorlevel% neq 0 (
    echo.
    echo [KRITIK HATA] Derleme sirasinda hata olustu!
    echo Lutfen yukaridaki hata mesajlarini kontrol edin.
    pause
    exit /b 1
)

echo.
echo [3/3] Derleme BASARILI!
echo.
echo ========================================================
echo   AnoreksikSuiteV10.exe olusturuldu.
echo   Yonetici olarak calistirmaniz tavsiye edilir.
echo ========================================================
echo.
pause
