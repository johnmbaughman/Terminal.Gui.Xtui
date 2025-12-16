# Benchmark comparison script
# Compares current benchmark results against baseline to detect regressions

param(
    [Parameter(Mandatory=$true)]
    [string]$BaselinePath,
    
    [Parameter(Mandatory=$true)]
    [string]$CurrentPath,
    
    [double]$RegressionThreshold = 10.0  # Percentage threshold for performance regression
)

$ErrorActionPreference = "Stop"

Write-Host "Comparing benchmark results..." -ForegroundColor Cyan
Write-Host "  Baseline: $BaselinePath"
Write-Host "  Current:  $CurrentPath"
Write-Host ""

# Load JSON files
$baseline = Get-Content $BaselinePath | ConvertFrom-Json
$current = Get-Content $CurrentPath | ConvertFrom-Json

# Compare performance
$baselineMs = $baseline.median_ms
$currentMs = $current.median_ms
$perfChange = (($currentMs - $baselineMs) / $baselineMs) * 100
$perfChangeAbs = [math]::Abs($perfChange)

Write-Host "Performance Comparison:" -ForegroundColor Yellow
Write-Host "  Baseline median: $baselineMs ms"
Write-Host "  Current median:  $currentMs ms"

if ($perfChange -gt 0) {
    $color = if ($perfChange -gt $RegressionThreshold) { "Red" } else { "Yellow" }
    Write-Host "  Change: +$([math]::Round($perfChange, 2))% (SLOWER)" -ForegroundColor $color
} elseif ($perfChange -lt 0) {
    Write-Host "  Change: $([math]::Round($perfChange, 2))% (FASTER)" -ForegroundColor Green
} else {
    Write-Host "  Change: 0% (IDENTICAL)" -ForegroundColor Gray
}

# Compare code metrics
Write-Host "`nCode Metrics Comparison:" -ForegroundColor Yellow

$baselineGen = $baseline.code_metrics.generators
$currentGen = $current.code_metrics.generators

$codeLineChange = $currentGen.code_lines - $baselineGen.code_lines
$codeLineChangePct = if ($baselineGen.code_lines -gt 0) { 
    [math]::Round((($currentGen.code_lines - $baselineGen.code_lines) / $baselineGen.code_lines) * 100, 1) 
} else { 0 }

$fileChange = $currentGen.files - $baselineGen.files

Write-Host "`nGenerators/ Directory:"
Write-Host "  Files: $($baselineGen.files) → $($currentGen.files) ($(if($fileChange -gt 0){"+"})$fileChange)"
Write-Host "  Code Lines: $($baselineGen.code_lines) → $($currentGen.code_lines) ($(if($codeLineChange -gt 0){"+"})$codeLineChange, $(if($codeLineChangePct -gt 0){"+"})$codeLineChangePct%)"
Write-Host "  Avg Lines/File: $($baselineGen.avg_lines_per_file) → $($currentGen.avg_lines_per_file)"

# Compare helpers if present
$baselineHelpers = $baseline.code_metrics.helpers
$currentHelpers = $current.code_metrics.helpers

if ($currentHelpers.files -gt 0 -or $baselineHelpers.files -gt 0) {
    Write-Host "`nGenerators/Helpers/ Directory:"
    Write-Host "  Files: $($baselineHelpers.files) → $($currentHelpers.files)"
    Write-Host "  Code Lines: $($baselineHelpers.code_lines) → $($currentHelpers.code_lines)"
    Write-Host "  Avg Lines/File: $($baselineHelpers.avg_lines_per_file) → $($currentHelpers.avg_lines_per_file)"
}

# Calculate total code reduction (accounting for helpers)
$baselineTotal = $baselineGen.code_lines + $baselineHelpers.code_lines
$currentTotal = $currentGen.code_lines + $currentHelpers.code_lines
$totalReduction = $baselineTotal - $currentTotal
$totalReductionPct = if ($baselineTotal -gt 0) {
    [math]::Round(($totalReduction / $baselineTotal) * 100, 1)
} else { 0 }

Write-Host "`nTotal Code Impact:" -ForegroundColor Cyan
Write-Host "  Total Code Lines: $baselineTotal → $currentTotal"
if ($totalReduction -gt 0) {
    Write-Host "  Reduction: -$totalReduction lines (-$totalReductionPct%)" -ForegroundColor Green
} elseif ($totalReduction -lt 0) {
    Write-Host "  Increase: +$([math]::Abs($totalReduction)) lines (+$([math]::Abs($totalReductionPct))%)" -ForegroundColor Yellow
} else {
    Write-Host "  Change: 0 lines (0%)" -ForegroundColor Gray
}

# Determine exit code based on regression
Write-Host ""
if ($perfChange -gt $RegressionThreshold) {
    Write-Host "❌ FAILED: Performance regression exceeds threshold ($RegressionThreshold%)" -ForegroundColor Red
    Write-Host "   Regression: +$([math]::Round($perfChange, 2))% (threshold: $RegressionThreshold%)" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✓ PASSED: Performance within acceptable range" -ForegroundColor Green
    if ($totalReduction -gt 0) {
        Write-Host "✓ BONUS: Code size reduced by $totalReduction lines ($totalReductionPct%)" -ForegroundColor Green
    }
    exit 0
}
