@echo off
setlocal
:: Add "Open with IconChop" to Windows Explorer context menu for image files only (.png, .jpg, .gif, etc.).
:: Run this batch from the same folder as IconChop.exe (e.g. bin\Debug\net10.0-windows).

set "EXE=%~dp0IconChop.exe"
if not exist "%EXE%" (
    echo IconChop.exe not found in: %~dp0
    echo Please run this batch from the folder that contains IconChop.exe.
    pause
    exit /b 1
)

for %%e in (.png .bmp .jpg .jpeg .gif .tiff .tif .ico .webp) do (
    reg add "HKCU\Software\Classes\%%e\shell\Open with IconChop" /ve /d "Open with IconChop" /f >nul
    reg add "HKCU\Software\Classes\%%e\shell\Open with IconChop\command" /ve /d "\"%EXE%\" \"%%1\"" /f >nul
)

if %errorlevel% equ 0 (
    echo Context menu "Open with IconChop" has been added for image files.
    echo Right-click a .png, .jpg, .gif, etc. and choose "Open with IconChop".
) else (
    echo Failed to add context menu. Error: %errorlevel%
)
pause
