#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validate the version & compatibility matrix against the current package version and rules.
.DESCRIPTION
    Parses the version compatibility markdown and ensures:
    - Current PackageVersion in the .csproj appears in the matrix
    - Feature table has an "Introduced In" column and non-empty values
    - Version rows are ordered (ascending by default, or descending with -Descending)
    - No duplicate versions
    - An upcoming placeholder row exists for the next minor version (configurable)
    Optionally emits a JSON report.
.PARAMETER CompatibilityFile
    Path to the version compatibility markdown file. Defaults to docs/articles/guides/version-compatibility.md.
.PARAMETER ProjectFile
    Path to the Terminal.Gui.Xaml.csproj that contains PackageVersion.
.PARAMETER Descending
    Enforce descending semantic version ordering (latest first).
.PARAMETER ConfigFile
    Optional path to a JSON config controlling validation behavior.
.EXAMPLE
    ./validate-version-matrix.ps1 -Descending
    Validates with descending order rules.
.NOTES
    Returns distinct exit codes for specific failure categories to aid CI troubleshooting.
#>

param(
    [string]$CompatibilityFile = "$(Join-Path $PSScriptRoot '..' 'docs' 'articles' 'guides' 'version-compatibility.md')",
    [string]$ProjectFile = "$(Join-Path $PSScriptRoot '..' 'src' 'Terminal.Gui.Xaml' 'Terminal.Gui.Xaml.csproj')",
    [switch]$Descending,
    [string]$ConfigFile = "$(Join-Path $PSScriptRoot 'version-matrix.config.json')"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Configuration Defaults
$config = [ordered]@{
    ordering = if ($Descending) { 'descending' } else { 'ascending' }
    requireUpcomingPlaceholder = $true
    upcomingPlaceholderPattern = '(planned|future)'
    emitJsonReport = $false
    reportPath = 'temp_test_validation/version-matrix-report.json'
}

if ($ConfigFile -and (Test-Path $ConfigFile)) {
    try {
        $rawConfig = Get-Content -LiteralPath $ConfigFile -Raw | ConvertFrom-Json -ErrorAction Stop
        foreach ($k in $rawConfig.PSObject.Properties.Name) {
            $config[$k] = $rawConfig.$k
        }
        if ($Descending) { $config['ordering'] = 'descending' }
    } catch {
        Write-Warning "Failed to parse config file '$ConfigFile': $($_.Exception.Message). Using defaults."
    }
} elseif ($ConfigFile -and ($ConfigFile -ne '')) {
    Write-Warning "Config file '$ConfigFile' not found. Using defaults."
}

if (-not (Test-Path $CompatibilityFile)) {
    Write-Error "Compatibility file not found: $CompatibilityFile"
    exit 1
}
if (-not (Test-Path $ProjectFile)) {
    Write-Error "Project file not found: $ProjectFile"
    exit 1
}

[xml]$proj = Get-Content -LiteralPath $ProjectFile
$pkgVersion = $null
foreach ($pg in $proj.Project.PropertyGroup) {
    $prop = $pg.PSObject.Properties["PackageVersion"]
    if ($prop -and $prop.Value) {
        $val = $prop.Value.ToString().Trim()
        if ($val.Length -gt 0) { $pkgVersion = $val; break }
    }
}

# Fallback: XPath search (handles unusual whitespace / grouping)
if (-not $pkgVersion) {
    try {
        $node = $proj.SelectSingleNode('//PackageVersion')
        if ($node -and $node.InnerText) {
            $pkgVersion = $node.InnerText.Trim()
        }
    } catch { }
}

if (-not $pkgVersion) {
    Write-Error "PackageVersion not defined in project file."
    exit 2
}

$matrix = Get-Content -LiteralPath $CompatibilityFile -Raw

# Basic check: ensure current package version appears as table row start
if ($matrix -notmatch "(?m)^\|\s*$([regex]::Escape($pkgVersion))\s*\|") {
    Write-Error "Current PackageVersion '$pkgVersion' not referenced in compatibility matrix."
    exit 3
} else {
    Write-Host "[OK] PackageVersion '$pkgVersion' found in compatibility matrix." -ForegroundColor Green
}

# Optionally verify introduced features column alignment later.

# Additional Checks
$errors = @()

# 1. Ensure 'Introduced In' column exists in Feature table
$featureHeaderLine = ($matrix -split "`n") | Where-Object { $_ -match '^\|\s*Feature\s*\|' } | Select-Object -First 1
if (-not $featureHeaderLine) {
    $errors += "Feature table header not found (expected a line starting with '| Feature |')."
} elseif ($featureHeaderLine -notmatch '\|\s*Introduced In\s*\|') {
    $errors += "'Introduced In' column header missing in feature table."
}

# 2. Validate non-empty 'Introduced In' cells for each feature row
if ($featureHeaderLine) {
    $lines = $matrix -split "`n"
    $startIndex = [array]::IndexOf($lines, $featureHeaderLine)
    if ($startIndex -ge 0) {
        # Skip header + separator line
        for ($i = $startIndex + 2; $i -lt $lines.Length; $i++) {
            $l = $lines[$i].TrimEnd()
            if ($l -eq '' -or $l -notmatch '^\|') { break }
            if ($l -match '^\|\s*-+\s*\|') { continue } # skip any accidental divider
            $cells = $l -split '\|' | ForEach-Object { $_.Trim() }
            if ($cells.Count -ge 3) {
                $featureName = $cells[1]
                $introduced = $cells[2]
                if (-not $introduced) {
                    $errors += "Feature '$featureName' missing 'Introduced In' value."
                }
            }
        }
    }
}

# 3. Semantic version ordering & duplicates in Core Compatibility table
$coreHeader = ($matrix -split "`n") | Where-Object { $_ -match '^\|\s*XAML Package Version\s*\|' } | Select-Object -First 1
if ($coreHeader) {
    $lines = $matrix -split "`n"
    $start = [array]::IndexOf($lines, $coreHeader)
    $versionRows = @()
    if ($start -ge 0) {
        for ($i = $start + 2; $i -lt $lines.Length; $i++) {
            $row = $lines[$i].TrimEnd()
            if ($row -eq '' -or $row -notmatch '^\|') { break }
            if ($row -match '^\|\s*-+') { continue }
            if ($row -match '^\|\s*([^.\d]*?)\|') { }
            $m = [regex]::Match($row, '^\|\s*([^|]+)\|')
            if ($m.Success) {
                $rawVersion = $m.Groups[1].Value.Trim()
                # Remove any parenthetical annotations e.g., "1.0.0 (planned)" -> "1.0.0"
                $cleanVersion = ($rawVersion -replace ' \(.*$', '').Trim()
                $versionRows += [PSCustomObject]@{ Raw=$rawVersion; Clean=$cleanVersion }
            }
        }
    }

    function Compare-SemVer($a, $b) {
        $re = '^(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.-]+))?$'
        $ma = [regex]::Match($a, $re)
        $mb = [regex]::Match($b, $re)
        if (-not ($ma.Success -and $mb.Success)) { return [string]::Compare($a,$b,[System.StringComparison]::OrdinalIgnoreCase) }
        $partsA = [int]$ma.Groups[1].Value, [int]$ma.Groups[2].Value, [int]$ma.Groups[3].Value
        $partsB = [int]$mb.Groups[1].Value, [int]$mb.Groups[2].Value, [int]$mb.Groups[3].Value
        for ($i=0;$i -lt 3;$i++){ if ($partsA[$i] -ne $partsB[$i]) { return $partsA[$i] - $partsB[$i] } }
        $preA = $ma.Groups[4].Value
        $preB = $mb.Groups[4].Value
        if ($preA -and -not $preB) { return -1 } # prerelease < release
        if (-not $preA -and $preB) { return 1 }
        if (-not $preA -and -not $preB) { return 0 }
        # Compare prerelease identifiers
        $ida = $preA.Split('.')
        $idb = $preB.Split('.')
        $len = [Math]::Max($ida.Length, $idb.Length)
        for ($i=0;$i -lt $len;$i++) {
            if ($i -ge $ida.Length) { return -1 }
            if ($i -ge $idb.Length) { return 1 }
            $sa = $ida[$i]; $sb = $idb[$i]
            $na = 0; $nb = 0
            $isNumA = [int]::TryParse($sa, [ref]$na)
            $isNumB = [int]::TryParse($sb, [ref]$nb)
            if ($isNumA -and $isNumB) { if ($na -ne $nb) { return $na - $nb } }
            elseif ($isNumA -and -not $isNumB) { return -1 }
            elseif (-not $isNumA -and $isNumB) { return 1 }
            else { $cmp = [string]::Compare($sa,$sb,[System.StringComparison]::OrdinalIgnoreCase); if ($cmp -ne 0) { return $cmp } }
        }
        return 0
    }

    $cleanList = $versionRows.Clean
    # Detect duplicates
    $dupeGroups = $cleanList | Group-Object | Where-Object { $_.Count -gt 1 }
    $dupes = @()
    if ($dupeGroups) { $dupes = $dupeGroups | ForEach-Object { $_.Name } }
    if ($dupes.Length -gt 0) { $errors += "Duplicate version entries found: $($dupes -join ', ')" }

    $enforceDescending = ($config.ordering -eq 'descending')
    # Order check (based on configuration / switch)
    for ($i=1; $i -lt $cleanList.Count; $i++) {
        $prev = $cleanList[$i-1]
        $curr = $cleanList[$i]
        $cmp = Compare-SemVer $prev $curr
        if (-not $enforceDescending) {
            # Ascending: prev <= curr
            if ($cmp -gt 0) {
                $errors += "Version order incorrect (expected ascending): '$($versionRows[$i-1].Raw)' should not come before '$($versionRows[$i].Raw)'."
                break
            }
        } else {
            # Descending: prev >= curr
            if ($cmp -lt 0) {
                $errors += "Version order incorrect (expected descending): '$($versionRows[$i-1].Raw)' should not precede '$($versionRows[$i].Raw)'."
                break
            }
        }
    }

    # Upcoming placeholder enforcement
    if ($config.requireUpcomingPlaceholder) {
        # Highest stable version determines exactly one expected next-minor placeholder.
        $stable = $versionRows | Where-Object { $_.Clean -match '^(\d+\.\d+\.\d+)$' } | ForEach-Object { $_.Clean }
        if ($stable) {
            $parsed = $stable | ForEach-Object {
                $m = [regex]::Match($_,'^(\d+)\.(\d+)\.(\d+)$');
                [PSCustomObject]@{Major=[int]$m.Groups[1].Value;Minor=[int]$m.Groups[2].Value;Patch=[int]$m.Groups[3].Value;Version=$_}
            } | Sort-Object Major,Minor,Patch -Descending | Select-Object -First 1
            if ($parsed) {
                $expectedNext = "$($parsed.Major).$([int]($parsed.Minor+1)).0"
                $placeholderRegex = "^\|\s*$([regex]::Escape($expectedNext))\s+\($($config.upcomingPlaceholderPattern)\)"
                if ($matrix -notmatch $placeholderRegex) {
                    $errors += "Upcoming version placeholder missing: expected row starting with '$expectedNext (planned|future)'."
                }
            }
        }
    }
}

if ($config.emitJsonReport) {
    try {
        $report = [ordered]@{
            packageVersion = $pkgVersion
            orderingMode = $config.ordering
            errors = @($errors)
            success = ($errors.Count -eq 0)
            timestamp = (Get-Date).ToString('o')
            config = $config
        } | ConvertTo-Json -Depth 6
        $reportPath = $config.reportPath
        $reportDir = Split-Path -Parent $reportPath
        if (-not (Test-Path $reportDir)) { New-Item -ItemType Directory -Path $reportDir -Force | Out-Null }
        Set-Content -LiteralPath $reportPath -Value $report -Encoding UTF8
        Write-Host "[INFO] JSON report written to $reportPath" -ForegroundColor Cyan
    } catch {
        Write-Warning "Failed to write JSON report: $($_.Exception.Message)"
    }
}

if ($errors.Count -gt 0) {
    Write-Error ("Version matrix validation failed:`n - " + ($errors -join "`n - "))
    if ($errors -match "Introduced In column header missing") { exit 4 }
    elseif ($errors -match "missing 'Introduced In' value") { exit 5 }
    elseif ($errors -match "Version order incorrect") { exit 6 }
    elseif ($errors -match "Duplicate version entries") { exit 7 }
    elseif ($errors -match "Upcoming version placeholder missing") { exit 8 }
    else { exit 9 }
} else {
    Write-Host "[OK] Additional matrix checks passed." -ForegroundColor Green
    exit 0
}
