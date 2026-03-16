# Create-Release.ps1 - Build and package IconChop for release (framework-dependent)
# Run from the project root (same folder as IconChop.csproj).
# Requires .NET 8 runtime on target machine. For a self-contained build, use Create-Release-SelfContained.ps1.

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$releaseDir = Join-Path $projectDir "release"
$publishDir = Join-Path $projectDir "publish"

# Read version from csproj (fallback to 1.0.0)
$csprojPath = Join-Path $projectDir "IconChop.csproj"
$version = "1.0.0"
if (Test-Path $csprojPath) {
    $content = Get-Content $csprojPath -Raw
    if ($content -match '<Version>([^<]+)</Version>') { $version = $Matches[1].Trim() }
}

$zipName = "IconChop-$version.zip"
$zipPath = Join-Path $releaseDir $zipName

Write-Host "IconChop release package (framework-dependent)" -ForegroundColor Cyan
Write-Host "  Version: $version"
Write-Host "  Output:  $zipPath"
Write-Host ""

# Clean and publish (framework-dependent; requires .NET 8 runtime on target machine)
Write-Host "Publishing (Release)..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish IconChop.csproj -c Release -o $publishDir

if (-not (Test-Path $publishDir)) {
    Write-Error "Publish failed: output folder not found."
}

# Create release directory and zip
if (-not (Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir | Out-Null }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Creating zip: $zipName" -ForegroundColor Yellow
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

# Optional: remove publish folder to avoid clutter
Remove-Item $publishDir -Recurse -Force

Write-Host ""
Write-Host "Done. Release package: $zipPath" -ForegroundColor Green
