#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build script for Terminal.Gui XAML Framework
.DESCRIPTION
    This script builds the Terminal.Gui XAML Framework with constitutional compliance checks.
.PARAMETER Configuration
    Build configuration (Debug or Release)
.PARAMETER Clean
    Clean build output before building
.PARAMETER SkipTests
    Skip running tests
.EXAMPLE
    ./build.ps1 -Configuration Release -Clean
#>

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [Parameter()]
    [switch]$Clean,

    [Parameter()]
    [switch]$SkipTests,

    [Parameter()]
    [switch]$VerboseOutput,

    [Parameter()]
    [switch]$SkipDocs,

    [Parameter()]
    [switch]$SkipVersionMatrixValidation,

    [Parameter()]
    [switch]$DescendingVersionMatrix
)
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Constitutional Performance Requirements
$script:XAML_PARSE_TIME_MS = 100
$script:UI_RENDER_FPS = 30
$script:MEMORY_LIMIT_MB = 50
$script:INIT_TIME_MS = 50

function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Green
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor Yellow
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"

    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK version: $dotnetVersion"
    }
    catch {
        Write-Error ".NET SDK is not installed"
        exit 1
    }

    # Check required version (8.0+)
    if (-not ($dotnetVersion -match "^[8-9]|^[1-9][0-9]")) {
        Write-Error ".NET 8+ is required, found: $dotnetVersion"
        exit 1
    }
}

function Invoke-CleanBuild {
    Write-Header "Cleaning Build Output"

    try {
        dotnet clean --configuration $Configuration --verbosity minimal
        Write-Success "Build output cleaned"
    }
    catch {
        Write-Error "Failed to clean build output"
        exit 1
    }
}

function Invoke-RestorePackages {
    Write-Header "Restoring NuGet Packages"

    try {
        dotnet restore --verbosity minimal
        Write-Success "NuGet packages restored"
    }
    catch {
        Write-Error "Failed to restore NuGet packages"
        exit 1
    }
}

function Invoke-BuildProjects {
    Write-Header "Building Projects"

    try {
        $buildArgs = @(
            "build"
            "--configuration", $Configuration
            "--no-restore"
            "--verbosity", $(if ($VerboseOutput) { "normal" } else { "minimal" })
        )

        dotnet @buildArgs
        Write-Success "Projects built successfully"
    }
    catch {
        Write-Error "Build failed"
        exit 1
    }
}

function Invoke-RunTests {
    if ($SkipTests) {
        Write-Warning "Skipping tests as requested"
        return
    }

    Write-Header "Running Tests"

    try {
        $testArgs = @(
            "test"
            "--configuration", $Configuration
            "--no-build"
            "--verbosity", "normal"
            "--logger", "console;verbosity=normal"
        )

        dotnet @testArgs
        Write-Success "All tests passed"
    }
    catch {
        Write-Error "Tests failed"
        exit 1
    }
}

function Invoke-BuildDocs {
    if ($SkipDocs) {
        Write-Warning "Skipping documentation build as requested"
        return
    }

    Write-Header "Building Documentation (DocFX)"

    $docfxConfig = Join-Path $PSScriptRoot '..' | Join-Path -ChildPath 'docs/docfx.json'
    $docfxConfig = [System.IO.Path]::GetFullPath($docfxConfig)
    if (-not (Test-Path $docfxConfig)) {
        Write-Warning "DocFX config not found at $docfxConfig; skipping docs build"
        return
    }

    $docfx = Get-Command docfx -ErrorAction SilentlyContinue
    if (-not $docfx) {
        Write-Warning "DocFX CLI not found. Install with: dotnet tool update -g docfx; skipping docs build"
        return
    }

    try {
    $docfxArgs = @('build', $docfxConfig, '--warningsAsErrors')
    if ($VerboseOutput) { $docfxArgs += '--logLevel'; $docfxArgs += 'Verbose' }
    & $docfx.Source $docfxArgs
        if ($LASTEXITCODE -ne 0) { throw "DocFX build failed with exit code $LASTEXITCODE" }
        Write-Success "Documentation built successfully (warnings treated as errors)"
    }
    catch {
        Write-Error "Documentation build failed: $($_.Exception.Message)"
        exit 1
    }
}

function Test-ConstitutionalCompliance {
    Write-Header "Constitutional Compliance Validation"

    # This is a placeholder for future constitutional checks
    # Will be implemented as we add the actual framework code
    Write-Success "Constitutional compliance checks passed (placeholder)"
}

function Invoke-VersionMatrixValidation {
    if ($SkipVersionMatrixValidation) {
        Write-Warning "Skipping version & compatibility matrix validation as requested"
        return
    }

    Write-Header "Validating Version & Compatibility Matrix"
    $scriptPath = Join-Path $PSScriptRoot 'validate-version-matrix.ps1'
    if (-not (Test-Path $scriptPath)) {
        Write-Warning "Validation script not found at $scriptPath"
        return
    }
    try {
    $validationArgs = @()
    if ($DescendingVersionMatrix) { $validationArgs += '-Descending' }
    & $scriptPath @validationArgs
        if ($LASTEXITCODE -ne 0) { throw "Matrix validation failed with exit code $LASTEXITCODE" }
        Write-Success "Version & compatibility matrix validation passed"
    }
    catch {
        Write-Error "Version matrix validation failed: $($_.Exception.Message)"
        exit 1
    }
}

# Main execution
try {
    Write-Header "Terminal.Gui XAML Framework Build Script"
    Write-Host "Configuration: $Configuration" -ForegroundColor Cyan

    Test-Prerequisites
    Invoke-RestorePackages

    if ($Clean) {
        Invoke-CleanBuild
    }

    Invoke-BuildProjects
    Invoke-RunTests
    Test-ConstitutionalCompliance
    Invoke-VersionMatrixValidation
    Invoke-BuildDocs

    Write-Header "Build Completed Successfully"
    Write-Success "Terminal.Gui XAML Framework build completed"
}
catch {
    Write-Error "Build script failed: $($_.Exception.Message)"
    exit 1
}
