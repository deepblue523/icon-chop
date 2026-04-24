# Publish-GitHubRelease.ps1 - Upload release zips + MSI to GitHub Releases (used by Create-Release-All.ps1 -UploadToGitHub)
# Requires GitHub CLI: https://cli.github.com/  (run: gh auth login)
# Runs from repo root so gh resolves the correct remote.

param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir,

    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI (gh) is required for -UploadToGitHub. Install from https://cli.github.com/ and run: gh auth login"
}

$releaseDir = Join-Path $ProjectDir "release"
$tag = "v$Version"
$fdZip = Join-Path $releaseDir "IconChop-$Version.zip"
$scZip = Join-Path $releaseDir "IconChop-$Version-self-contained.zip"
$msi = Join-Path $releaseDir "IconChop-$Version.msi"

foreach ($path in @($fdZip, $scZip, $msi)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Release artifact not found: $path"
    }
}

Push-Location $ProjectDir
try {
    # gh prints "release not found" to stderr when probing — must not terminate the script.
    $prevEap = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"
    $prevNativeErr = $null
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $prevNativeErr = $PSNativeCommandUseErrorActionPreference
        $PSNativeCommandUseErrorActionPreference = $false
    }
    try {
        & gh release view $tag 2>$null | Out-Null
        $releaseExists = ($LASTEXITCODE -eq 0)
    } finally {
        $ErrorActionPreference = $prevEap
        if ($null -ne $prevNativeErr) {
            $PSNativeCommandUseErrorActionPreference = $prevNativeErr
        }
    }

    if ($releaseExists) {
        Write-Host "Uploading assets to existing release $tag..." -ForegroundColor Yellow
        & gh release upload $tag $fdZip $scZip $msi --clobber
    } else {
        Write-Host "Creating GitHub release $tag with assets..." -ForegroundColor Yellow
        & gh release create $tag $fdZip $scZip $msi --title "IconChop $Version" --generate-notes
    }

    if ($LASTEXITCODE -ne 0) {
        throw "gh exited with code $LASTEXITCODE"
    }

    Write-Host "GitHub release $tag is up to date." -ForegroundColor Green
} finally {
    Pop-Location
}
