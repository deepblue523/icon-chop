# Create-Release-SelfContained.ps1 - Build and package IconChop for release (self-contained)
# Run from the project root (same folder as IconChop.csproj).
# Output includes .NET runtime; no runtime needed on target machine.
# Pass -Version to override the version from csproj (used by Create-Release-All.ps1 after a bump).

param(
    [string]$Version
)

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$releaseDir = Join-Path $projectDir "release"
$publishDir = Join-Path $projectDir "publish-selfcontained"

# Use provided version or read from csproj (fallback to 1.0.0)
if ($Version) {
    $version = $Version
} else {
    $csprojPath = Join-Path $projectDir "IconChop.csproj"
    $version = "1.0.0"
    if (Test-Path $csprojPath) {
        $content = Get-Content $csprojPath -Raw
        if ($content -match '<Version>([^<]+)</Version>') { $version = $Matches[1].Trim() }
    }
}

$zipName = "IconChop-$version-self-contained.zip"
$zipPath = Join-Path $releaseDir $zipName

Write-Host "IconChop release package (self-contained)" -ForegroundColor Cyan
Write-Host "  Version: $version"
Write-Host "  Output:  $zipPath"
Write-Host ""

# Clean and publish (self-contained; includes .NET runtime)
Write-Host "Publishing (Release, self-contained win-x64)..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
# Limit to en-US satellites so WiX harvest has no culture subfolders and installer builds cleanly
dotnet publish IconChop.csproj -c Release -r win-x64 --self-contained true -o $publishDir -p:SatelliteResourceLanguages=en-US

if (-not (Test-Path $publishDir)) {
    Write-Error "Publish failed: output folder not found."
}

# Create release directory and zip
if (-not (Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Creating zip: $zipName" -ForegroundColor Yellow
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

# Build Setup .exe (WiX Burn bundle wrapping the MSI; same publish folder is harvested by WiX)
$setupExeName = "IconChop-$version-Setup.exe"
$setupExePath = Join-Path $releaseDir $setupExeName
Write-Host "Building installer: $setupExeName" -ForegroundColor Yellow
$publishPathFull = (Resolve-Path $publishDir).Path
$bundleProj = Join-Path $projectDir "bundle\IconChop.Bundle.wixproj"
dotnet build $bundleProj -c Release -p:Version=$version -p:PublishPath=$publishPathFull -nologo -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Installer (bundle) build failed." }
$builtExe = Get-ChildItem -Path (Join-Path $projectDir "bundle\bin") -Recurse -Filter "IconChop.Bundle.exe" -ErrorAction SilentlyContinue |
    Select-Object -First 1
if (-not $builtExe) { throw "Installer build succeeded but IconChop.Bundle.exe was not found under bundle\bin." }
Copy-Item $builtExe.FullName $setupExePath -Force
Write-Host "  Installer: $setupExePath" -ForegroundColor Green

# Optional: remove publish folder to avoid clutter
Remove-Item $publishDir -Recurse -Force

Write-Host ""
Write-Host "Done. Release packages: $zipPath, $setupExePath" -ForegroundColor Green
