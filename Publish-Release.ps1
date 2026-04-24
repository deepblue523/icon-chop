# Publish-Release.ps1 - Build release packages locally, then push commit + tag so GitHub Actions publishes the release.
# Requires: git remote "origin", clean working tree (unless -Bump, which leaves only IconChop.csproj modified to commit).
# The Release workflow (on tag v*) builds on Windows and uploads zips + Setup.exe to a GitHub Release.
#
# Usage:
#   .\Publish-Release.ps1                    # Build from current csproj version, tag vX.Y.Z, push (fails if tag exists)
#   .\Publish-Release.ps1 -Bump patch        # Bump version, build, commit csproj, push branch + tag
#   .\Publish-Release.ps1 -Bump minor
#   .\Publish-Release.ps1 -Bump major
#   .\Publish-Release.ps1 -DryRun            # Show steps only
#   .\Publish-Release.ps1 -ForceTag          # Replace existing v* tag on origin (use with care)

param(
    [ValidateSet("patch", "minor", "major", "")]
    [string]$Bump,

    [switch]$DryRun,

    [switch]$ForceTag
)

$ErrorActionPreference = "Stop"
$projectDir = $PSScriptRoot
Set-Location $projectDir

function Get-IconChopVersion {
    $csprojPath = Join-Path $projectDir "IconChop.csproj"
    $content = Get-Content $csprojPath -Raw
    if ($content -match '<Version>([^<]+)</Version>') { return $Matches[1].Trim() }
    throw "Could not read <Version> from IconChop.csproj"
}

function Get-BumpedVersionPreview {
    param([ValidateSet("patch", "minor", "major")][string]$Kind)
    $current = Get-IconChopVersion
    $parts = $current.Split('.')
    $major = [int]$parts[0]
    $minor = if ($parts.Length -gt 1) { [int]$parts[1] } else { 0 }
    $patch = if ($parts.Length -gt 2) { [int]$parts[2] } else { 0 }
    switch ($Kind) {
        "major" { $major++; $minor = 0; $patch = 0 }
        "minor" { $minor++; $patch = 0 }
        "patch" { $patch++ }
    }
    return "$major.$minor.$patch"
}

function Assert-GitAvailable {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        throw "git is not on PATH."
    }
    $top = (git rev-parse --show-toplevel 2>$null).Trim()
    if (-not $top) {
        throw "Not a git repository (or git failed). Run from your clone root."
    }
    $gitRoot = [System.IO.Path]::GetFullPath($top)
    $projRoot = [System.IO.Path]::GetFullPath($projectDir)
    if (-not [string]::Equals($gitRoot, $projRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Run this script from the repository root (same folder as IconChop.csproj). Git root is: $gitRoot"
    }
    git remote get-url origin 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Git remote 'origin' is not configured."
    }
}

Assert-GitAvailable

$preStatus = git status --porcelain
if ($preStatus) {
    if ($DryRun) {
        Write-Host "[DryRun] Working tree is not clean; a real run would stop here until you commit or stash:" -ForegroundColor Yellow
        Write-Host ($preStatus -join "`n")
    }
    else {
        throw @"
Working tree is not clean. Commit or stash changes before publishing:
$($preStatus -join "`n")
"@
    }
}

if ($DryRun) {
    Write-Host "[DryRun] Would run Create-Release-All.ps1$(if ($Bump) { " -Bump $Bump" })." -ForegroundColor Yellow
    if ($Bump) {
        $preview = Get-BumpedVersionPreview -Kind $Bump
        Write-Host "[DryRun] After bump, tag would be v$preview (csproj is still unchanged)." -ForegroundColor Yellow
    }
}

$splat = @{}
if ($Bump) { $splat["Bump"] = $Bump }

if (-not $DryRun) {
    & (Join-Path $projectDir "Create-Release-All.ps1") @splat
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$version = Get-IconChopVersion
$tag = "v$version"

if (-not $DryRun) {
    $postStatus = git status --porcelain
    if ($Bump) {
        if ($postStatus -notmatch 'IconChop.csproj') {
            throw "Expected IconChop.csproj to be modified after -Bump, but git status was:`n$postStatus"
        }
        git add IconChop.csproj
        git commit -m "Bump version to $version"
        Write-Host "Committed version $version" -ForegroundColor Green
    }
    elseif ($postStatus) {
        throw "Unexpected working tree changes after build:`n$postStatus"
    }
}

# In -DryRun with -Bump, csproj was not bumped; skip remote check (tag would differ from current $version).
$remoteTag = $null
if (-not ($DryRun -and $Bump)) {
    $remoteTag = git ls-remote --tags origin "refs/tags/$tag"
}
if ($remoteTag) {
    if (-not $ForceTag) {
        throw "Tag $tag already exists on origin. Remove it on GitHub or use -ForceTag to replace (rewrites published tag)."
    }
    if ($DryRun) {
        Write-Host "[DryRun] Would delete remote tag $tag and recreate." -ForegroundColor Yellow
    }
    else {
        Write-Host "Removing existing tag $tag from origin (ForceTag)..." -ForegroundColor Yellow
        git push origin ":refs/tags/$tag"
        git tag -d $tag 2>$null
    }
}

$branch = git branch --show-current
if (-not $branch) {
    throw "Detached HEAD; checkout a branch before publishing."
}

if ($DryRun) {
    $tagPreview = if ($Bump) { "v$(Get-BumpedVersionPreview -Kind $Bump)" } else { $tag }
    Write-Host "[DryRun] Would: git push origin $branch" -ForegroundColor Yellow
    Write-Host "[DryRun] Would: git tag $tagPreview && git push origin $tagPreview" -ForegroundColor Yellow
    Write-Host "[DryRun] GitHub Actions Release workflow would run for $tagPreview." -ForegroundColor Yellow
    exit 0
}

Write-Host "Pushing branch $branch..." -ForegroundColor Cyan
git push origin $branch
if ($LASTEXITCODE -ne 0) { throw "git push origin $branch failed." }

Write-Host "Creating and pushing tag $tag..." -ForegroundColor Cyan
git tag $tag
git push origin $tag
if ($LASTEXITCODE -ne 0) { throw "git push origin $tag failed." }

Write-Host ""
Write-Host "Done. Pushed $tag - GitHub Actions will build and attach release assets." -ForegroundColor Green
$releaseDir = Join-Path $projectDir "release"
Write-Host "Local packages are still in: $releaseDir" -ForegroundColor Green
