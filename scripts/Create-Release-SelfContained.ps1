# Create-Release-SelfContained.ps1 - Build and package IconChop for release (self-contained)
# Repo root is the parent of this scripts folder (contains IconChop.csproj).
# Output includes .NET runtime; no runtime needed on target machine.
# Pass -Version to override the version from csproj (used by Create-Release-All.ps1 after a bump).

param(
    [string]$Version
)

$ErrorActionPreference = "Stop"
$projectDir = Split-Path $PSScriptRoot -Parent
$releaseDir = Join-Path $projectDir "release"
$publishDir = Join-Path $projectDir "publish-selfcontained"
$csprojPath = Join-Path $projectDir "IconChop.csproj"
$wixprojPath = Join-Path $projectDir "IconChop.Setup.wixproj"

# Use provided version or read from csproj (fallback to 1.0.0)
if ($Version) {
    $version = $Version
} else {
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
# Limit to en-US satellites so WiX harvest has no culture subfolders and MSI builds cleanly
dotnet publish $csprojPath -c Release -r win-x64 --self-contained true -o $publishDir -p:SatelliteResourceLanguages=en-US

if (-not (Test-Path $publishDir)) {
    Write-Error "Publish failed: output folder not found."
}

# Create release directory and zip
if (-not (Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Creating zip: $zipName" -ForegroundColor Yellow
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

# Build MSI (same publish folder is harvested by WiX)
$msiName = "IconChop-$version.msi"
$msiPath = Join-Path $releaseDir $msiName
Write-Host "Building MSI: $msiName" -ForegroundColor Yellow
$publishPathFull = (Resolve-Path $publishDir).Path
dotnet build $wixprojPath -c Release -p:Version=$version -p:PublishPath=$publishPathFull -nologo -v:minimal
if ($LASTEXITCODE -ne 0) { throw "MSI build failed." }
# WiX project uses Platform=x64 → output is bin\x64\Release\, not bin\Release\
$builtMsi = @(
    Join-Path $projectDir "bin\x64\Release\IconChop.Setup.msi"
    Join-Path $projectDir "bin\Release\IconChop.Setup.msi"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if ($builtMsi) {
    Copy-Item $builtMsi $msiPath -Force
    Write-Host "  MSI: $msiPath" -ForegroundColor Green
} else {
    throw "MSI build output not found (looked under bin\x64\Release and bin\Release)."
}

# Optional: remove publish folder to avoid clutter
Remove-Item $publishDir -Recurse -Force

Write-Host ""
Write-Host "Done. Release packages: $zipPath, $msiPath" -ForegroundColor Green
