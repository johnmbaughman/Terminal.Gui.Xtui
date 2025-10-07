<#
.SYNOPSIS
Converts .benchmarks/history.csv into structured JSON with trend data.

.DESCRIPTION
Parses the CSV produced by benchmarks and emits two JSON files:
- history.json: raw rows
- summary.json: aggregated latest metrics per benchmark with percent change vs previous and vs first.

.PARAMETER Input
Path to history.csv (default ./.benchmarks/history.csv)

.PARAMETER OutputDir
Directory to emit JSON files (default ./.benchmarks)
#>
[CmdletBinding()]
param(
  [string]$HistoryPath = "./.benchmarks/history.csv"
 , [string]$OutputDir = "./.benchmarks"
)

if (!(Test-Path $HistoryPath)) { Write-Error "History file not found: $HistoryPath"; exit 1 }
if (!(Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }

$rows = Import-Csv -Path $HistoryPath

# Normalize & sort
$rows = $rows | Sort-Object { [DateTime]$_.'timestamp' } | ForEach-Object {
  [PSCustomObject]@{
    timestamp = [DateTime]$_.timestamp
    commit    = $_.commit
    benchmark = $_.benchmark
    mean_ns   = [double]$_.mean_ns
  }
}

$historyPath = Join-Path $OutputDir 'history.json'
$rows | ConvertTo-Json -Depth 4 | Out-File -Encoding UTF8 $historyPath

$summary = @{}
$grouped = $rows | Group-Object benchmark
foreach ($g in $grouped) {
  $ordered = $g.Group | Sort-Object timestamp
  $latest = $ordered[-1]
  $previous = if ($ordered.Count -gt 1) { $ordered[-2] } else { $null }
  $first = $ordered[0]
  $changePrev = if ($previous) { (($latest.mean_ns - $previous.mean_ns) / $previous.mean_ns) * 100.0 } else { 0 }
  $changeFirst = if ($first) { (($latest.mean_ns - $first.mean_ns) / $first.mean_ns) * 100.0 } else { 0 }
  $summary[$g.Name] = [PSCustomObject]@{
    benchmark = $g.Name
    latest_mean_ns = [math]::Round($latest.mean_ns,2)
    previous_mean_ns = if ($previous) { [math]::Round($previous.mean_ns,2) } else { $null }
    first_mean_ns = [math]::Round($first.mean_ns,2)
    change_vs_previous_pct = [math]::Round($changePrev,2)
    change_vs_first_pct = [math]::Round($changeFirst,2)
    latest_commit = $latest.commit
    latest_timestamp = $latest.timestamp.ToString('o')
  }
}

$summaryPath = Join-Path $OutputDir 'summary.json'
($summary.GetEnumerator() | ForEach-Object { $_.Value }) | ConvertTo-Json -Depth 4 | Out-File -Encoding UTF8 $summaryPath

Write-Host "Wrote $historyPath and $summaryPath"