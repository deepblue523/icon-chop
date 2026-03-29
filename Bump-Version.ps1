# Bump-Version.ps1 - Increment Version and AssemblyVersion in IconChop.csproj
# Usage: .\Bump-Version.ps1 [-Bump patch|minor|major]
# Returns the new version string so callers can capture it.

param(
    [ValidateSet("patch", "minor", "major")]
    [string]$Bump = "patch"
)

$ErrorActionPreference = "Stop"
$csprojPath = Join-Path $PSScriptRoot "IconChop.csproj"

if (-not (Test-Path $csprojPath)) {
    throw "IconChop.csproj not found at $csprojPath"
}

$content = Get-Content $csprojPath -Raw
if ($content -notmatch '<Version>([^<]+)</Version>') {
    throw "Could not find <Version> element in IconChop.csproj"
}

$current = $Matches[1].Trim()
$parts = $current.Split('.')
$major = [int]$parts[0]
$minor = if ($parts.Length -gt 1) { [int]$parts[1] } else { 0 }
$patch = if ($parts.Length -gt 2) { [int]$parts[2] } else { 0 }

switch ($Bump) {
    "major" { $major++; $minor = 0; $patch = 0 }
    "minor" { $minor++; $patch = 0 }
    "patch" { $patch++ }
}

$newVersion = "$major.$minor.$patch"
$assemblyVersion = "$newVersion.0"

$content = $content -replace '<Version>[^<]+</Version>', "<Version>$newVersion</Version>"
$content = $content -replace '<AssemblyVersion>[^<]+</AssemblyVersion>', "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
Set-Content $csprojPath $content -NoNewline

Write-Host "Version bumped: $current -> $newVersion ($Bump)" -ForegroundColor Green
return $newVersion
