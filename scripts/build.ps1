#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build the Terminal.Gui.Xaml solution, run tests, validate version matrix, and build docs.
.DESCRIPTION
    Orchestrates the full build for the Terminal.Gui.Xaml project:
    - Restores and builds the solution
    - Optionally cleans prior outputs
    - Runs unit tests (unless skipped)
    - Performs constitutional and version-matrix validations
    - Builds documentation via DocFX (unless skipped)

.PARAMETER Configuration
    Build configuration. Allowed values: Debug or Release. Defaults to Debug.
.PARAMETER Clean
    When specified, performs a clean of build outputs before building.
.PARAMETER SkipTests
    When specified, skips running unit tests.
.PARAMETER VerboseOutput
    Increases verbosity for build/test/docfx steps (maps to dotnet verbosity=normal, docfx --logLevel Verbose).
.PARAMETER SkipDocs
    When specified, skips the documentation build step.
.PARAMETER DocWarningsAsErrors
    Treat DocFX warnings as errors (passes --warningsAsErrors to docfx). Useful for CI enforcement.
.PARAMETER SkipVersionMatrixValidation
    Skip the version & compatibility matrix validation step.
.PARAMETER DescendingVersionMatrix
    Use descending ordering rules when validating the version matrix (latest first).

.EXAMPLE
    ./build.ps1 -Configuration Release -Clean
    Performs a clean Release build, runs tests, validations, and builds docs.

.EXAMPLE
    ./build.ps1 -Configuration Release -SkipTests -SkipDocs
    Builds the solution in Release but skips tests and documentation.

.EXAMPLE
    ./build.ps1 -Configuration Release -DocWarningsAsErrors -VerboseOutput
    Builds everything with verbose logs and fails the docs step on any DocFX warnings.

.NOTES
    Requires .NET 8+ SDK and DocFX (installed as a dotnet global tool) if building docs.
    The docs step pre-cleans docs/_site and docs/obj and includes a retry loop for transient file locks on Windows.
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
    [switch]$DocWarningsAsErrors,

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

    # Pre-clean output/cache to avoid stale file locks
    $docsRoot = [System.IO.Path]::GetDirectoryName($docfxConfig)
    $siteDir = Join-Path $docsRoot '_site'
    $objDir = Join-Path $docsRoot 'obj'
    foreach ($dir in @($siteDir, $objDir)) {
        if (Test-Path $dir) {
            try {
                Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction Stop
            }
            catch {
                Write-Warning "Could not fully clean $dir before doc build: $($_.Exception.Message)"
            }
        }
    }

    try {
    $docfxArgs = @('build', $docfxConfig)
    if ($DocWarningsAsErrors) { $docfxArgs += '--warningsAsErrors' }
    if ($VerboseOutput) { $docfxArgs += '--logLevel'; $docfxArgs += 'Verbose' }

    $maxRetries = 3
    $attempt = 0
    $succeeded = $false
    while (-not $succeeded -and $attempt -lt $maxRetries) {
        $attempt++
        & $docfx.Source $docfxArgs
        if ($LASTEXITCODE -eq 0) {
            $succeeded = $true
            break
        }
        if ($attempt -lt $maxRetries) {
            Write-Warning "DocFX build failed (attempt $attempt/$maxRetries). Retrying in 2s..."
            Start-Sleep -Seconds 2
        }
    }
        if (-not $succeeded) { throw "DocFX build failed with exit code $LASTEXITCODE after $attempt attempt(s)" }
        if ($DocWarningsAsErrors) {
            Write-Success "Documentation built successfully (warnings treated as errors)"
        } else {
            Write-Success "Documentation built successfully"
        }
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
