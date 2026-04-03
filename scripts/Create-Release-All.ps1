# Create-Release-All.ps1 - Build and package both framework-dependent and self-contained releases, plus MSI
# Repo root is the parent of this scripts folder (contains IconChop.csproj).
# Output: release\IconChop-<version>.zip, release\IconChop-<version>-self-contained.zip, release\IconChop-<version>.msi
#
# Usage:
#   .\scripts\Create-Release-All.ps1                  # Build with current version in csproj
#   .\scripts\Create-Release-All.ps1 -Bump patch      # Auto-increment patch (e.g. 2.0.0 -> 2.0.1) then build
#   .\scripts\Create-Release-All.ps1 -Bump minor      # Auto-increment minor (e.g. 2.0.0 -> 2.1.0) then build
#   .\scripts\Create-Release-All.ps1 -Bump major      # Auto-increment major (e.g. 2.0.0 -> 3.0.0) then build
#   .\scripts\Create-Release-All.ps1 -UploadToGitHub  # After build, create/update GitHub release v<version> and upload all artifacts (needs gh CLI)

param(
    [ValidateSet("patch", "minor", "major", "")]
    [string]$Bump,

    [switch]$UploadToGitHub,

    # When set (and -Bump is not), use this version instead of reading IconChop.csproj
    [string]$Version
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$projectDir = Split-Path $PSScriptRoot -Parent

# Optionally bump version before building
if ($Bump) {
    $version = & (Join-Path $scriptDir "Bump-Version.ps1") -Bump $Bump
} elseif ($Version) {
    $version = $Version
} else {
    $csprojPath = Join-Path $projectDir "IconChop.csproj"
    $version = "1.0.0"
    if (Test-Path $csprojPath) {
        $content = Get-Content $csprojPath -Raw
        if ($content -match '<Version>([^<]+)</Version>') { $version = $Matches[1].Trim() }
    }
}

Write-Host "Building IconChop v$version release packages (both variants)..." -ForegroundColor Cyan
Write-Host ""

& (Join-Path $scriptDir "Create-Release.ps1") -Version $version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
& (Join-Path $scriptDir "Create-Release-SelfContained.ps1") -Version $version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "All release packages (zips + MSI) are in: $(Join-Path $projectDir 'release')" -ForegroundColor Green

if ($UploadToGitHub) {
    Write-Host ""
    & (Join-Path $scriptDir "Publish-GitHubRelease.ps1") -ProjectDir $projectDir -Version $version
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
