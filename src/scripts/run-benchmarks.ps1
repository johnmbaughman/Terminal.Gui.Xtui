# Benchmark runner script for T013
# Runs GeneratorBenchmarks N=5 times, computes median, analyzes LOC metrics, and stores results

param(
    [int]$RunCount = 5,
    [string]$BenchmarkProject = "C:\Personal\Files\source\repos\Terminal.Gui.Xtui\src\Terminal.Gui.Xtui.Benchmarks",
    [string]$OutputDir = "C:\Personal\Files\source\repos\Terminal.Gui.Xtui\src\artifacts\benchmarks",
    [string]$GeneratorsDir = "C:\Personal\Files\source\repos\Terminal.Gui.Xtui\src\Terminal.Gui.Xtui\Generators"
)

$ErrorActionPreference = "Stop"

Write-Host "Running GeneratorBenchmarks $RunCount times..." -ForegroundColor Cyan

# Ensure output directory exists
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$results = @()
$runId = Get-Date -Format 'yyyyMMddHHmmss'

for ($i = 1; $i -le $RunCount; $i++) {
    Write-Host "`nRun $i of $RunCount..." -ForegroundColor Yellow
    
    $start = [DateTime]::UtcNow
    
    # Run benchmark in dry mode for faster execution (just measurement, no full BDN overhead)
    # Using --filter to run only GeneratorBenchmarks class
    $output = dotnet run --project $BenchmarkProject --configuration Release --no-build --framework net8.0 -- --filter "*GeneratorBenchmarks*" --job dry 2>&1
    
    $end = [DateTime]::UtcNow
    $elapsedMs = [math]::Round(($end - $start).TotalMilliseconds, 0)
    
    Write-Host "  Elapsed: $elapsedMs ms" -ForegroundColor Gray
    
    $results += $elapsedMs
}

# Compute median
$sorted = $results | Sort-Object
$median = if ($sorted.Count % 2 -eq 0) {
    [math]::Round(($sorted[$sorted.Count / 2 - 1] + $sorted[$sorted.Count / 2]) / 2, 0)
} else {
    $sorted[[math]::Floor($sorted.Count / 2)]
}

Write-Host "`nResults:" -ForegroundColor Green
Write-Host "  Runs: $($results -join ', ') ms"
Write-Host "  Median: $median ms"
Write-Host "  Run ID: $runId"

# Analyze Lines of Code metrics
Write-Host "`nAnalyzing code metrics..." -ForegroundColor Cyan

function Get-CodeMetrics {
    param([string]$Path)
    
    $files = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | Where-Object { 
        $_.FullName -notmatch '\\obj\\' -and 
        $_.FullName -notmatch '\\bin\\' -and
        $_.Name -notmatch '\.g\.cs$'
    }
    
    $totalLines = 0
    $codeLines = 0
    $commentLines = 0
    $blankLines = 0
    $fileCount = 0
    
    foreach ($file in $files) {
        $fileCount++
        $content = Get-Content $file.FullName
        $totalLines += $content.Count
        
        $inBlockComment = $false
        foreach ($line in $content) {
            $trimmed = $line.Trim()
            
            if ($trimmed -eq '') {
                $blankLines++
            }
            elseif ($trimmed.StartsWith('/*')) {
                $inBlockComment = $true
                $commentLines++
            }
            elseif ($inBlockComment) {
                $commentLines++
                if ($trimmed.EndsWith('*/')) {
                    $inBlockComment = $false
                }
            }
            elseif ($trimmed.StartsWith('//')) {
                $commentLines++
            }
            else {
                $codeLines++
            }
        }
    }
    
    return @{
        files = $fileCount
        total_lines = $totalLines
        code_lines = $codeLines
        comment_lines = $commentLines
        blank_lines = $blankLines
        avg_lines_per_file = if ($fileCount -gt 0) { [math]::Round($totalLines / $fileCount, 1) } else { 0 }
    }
}

# Analyze generator code
$generatorMetrics = Get-CodeMetrics -Path $GeneratorsDir

# Analyze helper code if it exists
$helpersDir = Join-Path $GeneratorsDir "Helpers"
$helperMetrics = if (Test-Path $helpersDir) {
    Get-CodeMetrics -Path $helpersDir
} else {
    @{
        files = 0
        total_lines = 0
        code_lines = 0
        comment_lines = 0
        blank_lines = 0
        avg_lines_per_file = 0
    }
}

Write-Host "`nCode Metrics (Generators/):" -ForegroundColor Yellow
Write-Host "  Files: $($generatorMetrics.files)"
Write-Host "  Total Lines: $($generatorMetrics.total_lines)"
Write-Host "  Code Lines: $($generatorMetrics.code_lines)"
Write-Host "  Comment Lines: $($generatorMetrics.comment_lines)"
Write-Host "  Blank Lines: $($generatorMetrics.blank_lines)"
Write-Host "  Avg Lines/File: $($generatorMetrics.avg_lines_per_file)"

if ($helperMetrics.files -gt 0) {
    Write-Host "`nCode Metrics (Generators/Helpers/):" -ForegroundColor Yellow
    Write-Host "  Files: $($helperMetrics.files)"
    Write-Host "  Total Lines: $($helperMetrics.total_lines)"
    Write-Host "  Code Lines: $($helperMetrics.code_lines)"
    Write-Host "  Comment Lines: $($helperMetrics.comment_lines)"
    Write-Host "  Blank Lines: $($helperMetrics.blank_lines)"
    Write-Host "  Avg Lines/File: $($helperMetrics.avg_lines_per_file)"
}

# Create summary JSON with code metrics
$summary = @{
    benchmark = 'GeneratorBenchmarks'
    median_ms = $median
    runs = $RunCount
    run_id = $runId
    timestamp = (Get-Date -Format 'o')
    code_metrics = @{
        generators = $generatorMetrics
        helpers = $helperMetrics
    }
} | ConvertTo-Json -Depth 5

$summaryPath = Join-Path $OutputDir "summary.json"
$summary | Out-File -FilePath $summaryPath -Encoding utf8 -Force

Write-Host "`nSummary written to: $summaryPath" -ForegroundColor Cyan

# Store raw runs
$rawRuns = @{
    runs = $results
    run_id = $runId
    timestamp = (Get-Date -Format 'o')
} | ConvertTo-Json -Depth 3

$rawPath = Join-Path $OutputDir "raw-runs-$runId.json"
$rawRuns | Out-File -FilePath $rawPath -Encoding utf8 -Force

Write-Host "Raw runs written to: $rawPath" -ForegroundColor Cyan

Write-Host "`n✓ Benchmark baseline captured successfully!" -ForegroundColor Green
