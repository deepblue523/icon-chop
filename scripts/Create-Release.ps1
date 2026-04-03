# Create-Release.ps1 - Build and package IconChop for release (framework-dependent)
# Repo root is the parent of this scripts folder (contains IconChop.csproj).
# Requires .NET 8 runtime on target machine. For a self-contained build, use Create-Release-SelfContained.ps1.
# Pass -Version to override the version from csproj (used by Create-Release-All.ps1 after a bump).
# Use -UploadToGitHub to build all artifacts (framework zip, self-contained zip, MSI) and publish to GitHub (see Create-Release-All.ps1).

param(
    [string]$Version,

    [switch]$UploadToGitHub
)

$ErrorActionPreference = "Stop"

if ($UploadToGitHub) {
    $allScript = Join-Path $PSScriptRoot "Create-Release-All.ps1"
    $splat = @{ UploadToGitHub = $true }
    if ($PSBoundParameters.ContainsKey("Version")) { $splat.Version = $Version }
    & $allScript @splat
    exit $LASTEXITCODE
}
$projectDir = Split-Path $PSScriptRoot -Parent
$releaseDir = Join-Path $projectDir "release"
$publishDir = Join-Path $projectDir "publish"
$csprojPath = Join-Path $projectDir "IconChop.csproj"

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

$zipName = "IconChop-$version.zip"
$zipPath = Join-Path $releaseDir $zipName

Write-Host "IconChop release package (framework-dependent)" -ForegroundColor Cyan
Write-Host "  Version: $version"
Write-Host "  Output:  $zipPath"
Write-Host ""

# Clean and publish (framework-dependent; requires .NET 8 runtime on target machine)
Write-Host "Publishing (Release)..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish $csprojPath -c Release -o $publishDir

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
