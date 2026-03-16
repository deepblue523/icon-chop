# Create-Release-All.ps1 - Build and package both framework-dependent and self-contained releases
# Run from the project root (same folder as IconChop.csproj).
# Output: release\IconChop-<version>.zip and release\IconChop-<version>-self-contained.zip

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot

# Run both release scripts in order (shared version comes from csproj; both write to release\)
Write-Host "Building IconChop release packages (both variants)..." -ForegroundColor Cyan
Write-Host ""

& (Join-Path $projectDir "Create-Release.ps1")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
& (Join-Path $projectDir "Create-Release-SelfContained.ps1")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "All release packages are in: $(Join-Path $projectDir 'release')" -ForegroundColor Green
