@echo off
setlocal
:: Add "Open with IconChop" to Windows Explorer context menu for all files.
:: Run this batch from the same folder as IconChop.exe (e.g. bin\Debug\net10.0-windows).

set "EXE=%~dp0IconChop.exe"
if not exist "%EXE%" (
    echo IconChop.exe not found in: %~dp0
    echo Please run this batch from the folder that contains IconChop.exe.
    pause
    exit /b 1
)

reg add "HKCU\Software\Classes\*\shell\Open with IconChop" /ve /d "Open with IconChop" /f >nul
reg add "HKCU\Software\Classes\*\shell\Open with IconChop\command" /ve /d "\"%EXE%\" \"%%1\"" /f >nul

if %errorlevel% equ 0 (
    echo Context menu "Open with IconChop" has been added.
    echo Right-click any file and choose "Open with IconChop" to open it.
) else (
    echo Failed to add context menu. Error: %errorlevel%
)
pause
