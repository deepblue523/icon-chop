@echo off
:: Remove "Open with IconChop" from Windows Explorer context menu (image file types).

for %%e in (.png .bmp .jpg .jpeg .gif .tiff .tif .ico .webp) do (
    reg delete "HKCU\Software\Classes\%%e\shell\Open with IconChop" /f >nul 2>&1
)

echo "Open with IconChop" has been removed from the context menu for image files.
echo If no entries existed, nothing was changed.
pause
