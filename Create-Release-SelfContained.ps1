# Create-Release-SelfContained.ps1 - Build and package IconChop for release (self-contained)
# Run from the project root (same folder as IconChop.csproj).
# Output includes .NET runtime; no runtime needed on target machine.

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
$releaseDir = Join-Path $projectDir "release"
$publishDir = Join-Path $projectDir "publish-selfcontained"

# Read version from csproj (fallback to 1.0.0)
$csprojPath = Join-Path $projectDir "IconChop.csproj"
$version = "1.0.0"
if (Test-Path $csprojPath) {
    $content = Get-Content $csprojPath -Raw
    if ($content -match '<Version>([^<]+)</Version>') { $version = $Matches[1].Trim() }
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
dotnet publish IconChop.csproj -c Release -r win-x64 --self-contained true -o $publishDir

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
