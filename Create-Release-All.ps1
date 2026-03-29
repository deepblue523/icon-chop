# Create-Release-All.ps1 - Build and package both framework-dependent and self-contained releases, plus MSI
# Run from the project root (same folder as IconChop.csproj).
# Output: release\IconChop-<version>.zip, release\IconChop-<version>-self-contained.zip, release\IconChop-<version>.msi
#
# Usage:
#   .\Create-Release-All.ps1                  # Build with current version in csproj
#   .\Create-Release-All.ps1 -Bump patch      # Auto-increment patch (e.g. 2.0.0 -> 2.0.1) then build
#   .\Create-Release-All.ps1 -Bump minor      # Auto-increment minor (e.g. 2.0.0 -> 2.1.0) then build
#   .\Create-Release-All.ps1 -Bump major      # Auto-increment major (e.g. 2.0.0 -> 3.0.0) then build

param(
    [ValidateSet("patch", "minor", "major", "")]
    [string]$Bump
)

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot

# Optionally bump version before building
if ($Bump) {
    $version = & (Join-Path $projectDir "Bump-Version.ps1") -Bump $Bump
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

& (Join-Path $projectDir "Create-Release.ps1") -Version $version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
& (Join-Path $projectDir "Create-Release-SelfContained.ps1") -Version $version
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "All release packages (zips + MSI) are in: $(Join-Path $projectDir 'release')" -ForegroundColor Green
