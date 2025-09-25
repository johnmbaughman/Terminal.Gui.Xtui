#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Simple example validation for Terminal.Gui XAML Framework documentation
.DESCRIPTION
    Validates code examples from documentation by extracting fenced code blocks and performing
    light heuristics (length/shape) checks without compilation.
.PARAMETER ShowDetails
    Enable detailed output
.EXAMPLE
    ./validate-examples-simple.ps1 -ShowDetails
#>

param(
    [Parameter()]
    [switch]$ShowDetails
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Continue"

function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Green
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    if ($ShowDetails) {
        Write-Host "ℹ️ $Message" -ForegroundColor Cyan
    }
}

function Extract-CodeBlocks {
    param(
        [string]$FilePath,
        [string]$Language
    )
    
    $content = Get-Content -Path $FilePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) { return @() }
    
    $codeBlocks = @()
    $pattern = '```' + $Language + '[^`]*?```'
    $regexMatches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    foreach ($match in $regexMatches) {
        # Extract just the code part (remove language marker and closing ticks)
        $openPattern = '```' + $Language + '\r?\n'
        $closePattern = '\r?\n```'
        $code = $match.Value -replace $openPattern, "" -replace $closePattern, ""
        if ($code.Trim()) {
            $codeBlocks += $code.Trim()
        }
    }
    
    return $codeBlocks
}

function Test-ExampleFile {
    param([string]$FilePath)
    
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    Write-Header "Validating: $fileName"
    
    $results = @{
        FileName = $fileName
        CSharpBlocks = 0
        PowerShellBlocks = 0
        XmlBlocks = 0
        TotalBlocks = 0
    }
    
    # Extract code blocks
    $csharpBlocks = @(Extract-CodeBlocks -FilePath $FilePath -Language "csharp")
    $powershellBlocks = @(Extract-CodeBlocks -FilePath $FilePath -Language "powershell")
    $xmlBlocks = @(Extract-CodeBlocks -FilePath $FilePath -Language "xml")
    
    $results.CSharpBlocks = $csharpBlocks.Count
    $results.PowerShellBlocks = $powershellBlocks.Count
    $results.XmlBlocks = $xmlBlocks.Count
    $results.TotalBlocks = $csharpBlocks.Count + $powershellBlocks.Count + $xmlBlocks.Count
    
    Write-Info "Found $($results.CSharpBlocks) C# blocks"
    Write-Info "Found $($results.PowerShellBlocks) PowerShell blocks"
    Write-Info "Found $($results.XmlBlocks) XML blocks"
    
    # Basic validation - just check if we extracted content
    foreach ($block in $csharpBlocks) {
        if ($block.Length -gt 10) { # Basic length check
            Write-Info "C# block validation passed"
        }
    }
    
    foreach ($block in $powershellBlocks) {
        if ($block.Length -gt 5) { # Basic length check
            Write-Info "PowerShell block validation passed"
        }
    }
    
    foreach ($block in $xmlBlocks) {
        if ($block.Contains("<") -and $block.Contains(">")) { # Basic XML check
            Write-Info "XML block validation passed"
        }
    }
    
    return $results
}

# Main execution
try {
    Write-Header "Example Validation (Simple Mode)"
    
    # Find examples directory
    $examplesDir = Join-Path $PSScriptRoot '..' | Join-Path -ChildPath 'docs/articles/examples'
    $examplesDir = [System.IO.Path]::GetFullPath($examplesDir)
    
    Write-Host "Examples directory: $examplesDir" -ForegroundColor Cyan
    
    if (-not (Test-Path $examplesDir)) {
        Write-Warning "Examples directory not found: $examplesDir"
        exit 0
    }
    
    # Find example files
    $exampleFiles = Get-ChildItem -Path $examplesDir -Filter "*.md" | Where-Object { $_.Name -ne "index.md" }
    
    if ($exampleFiles.Count -eq 0) {
        Write-Warning "No example files found"
        exit 0
    }
    
    Write-Host "Found $($exampleFiles.Count) example files" -ForegroundColor Cyan
    
    # Process files
    $allResults = @()
    foreach ($file in $exampleFiles) {
        $result = Test-ExampleFile -FilePath $file.FullName
        $allResults += $result
    }
    
    # Summary
    $totalBlocks = ($allResults | Measure-Object TotalBlocks -Sum).Sum
    $totalFiles = $allResults.Count
    
    Write-Header "Validation Summary"
    Write-Host "Files processed: $totalFiles" -ForegroundColor White
    Write-Host "Code blocks found: $totalBlocks" -ForegroundColor White
    
    if ($totalBlocks -gt 0) {
        Write-Success "Example validation completed - found $totalBlocks code blocks"
    } else {
        Write-Warning "No code blocks found in examples"
    }
    
    exit 0
}
catch {
    Write-Host "❌ Example validation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}