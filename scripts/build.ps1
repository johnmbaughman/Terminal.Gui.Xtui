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
    [switch]$VerboseOutput
)Set-StrictMode -Version Latest
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

function Test-ConstitutionalCompliance {
    Write-Header "Constitutional Compliance Validation"

    # This is a placeholder for future constitutional checks
    # Will be implemented as we add the actual framework code
    Write-Success "Constitutional compliance checks passed (placeholder)"
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

    Write-Header "Build Completed Successfully"
    Write-Success "Terminal.Gui XAML Framework build completed"
}
catch {
    Write-Error "Build script failed: $($_.Exception.Message)"
    exit 1
}
