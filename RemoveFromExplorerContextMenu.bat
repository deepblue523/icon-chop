@echo off
reg delete "HKCU\Software\Classes\*\shell\Open with IconChop" /f >nul 2>&1
if %errorlevel% equ 0 (
    echo "Open with IconChop" has been removed from the context menu.
) else (
    echo No context menu entry found, or already removed.
)
pause
