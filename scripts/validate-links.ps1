#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Link validation script for Terminal.Gui XAML Framework documentation
.DESCRIPTION
    Validates links in the generated documentation site by scanning HTML files for broken internal links.
    It serves as a backup/supplement to DocFX's built-in link validation and operates on the built _site output.
    External (http/https), mailto, and fragment-only (#) links are ignored.
.PARAMETER SiteDirectory
    Path to the built documentation site (_site directory)
.PARAMETER ExitOnFailure
    Exit with non-zero code if broken links are found
.EXAMPLE
    ./validate-links.ps1 -SiteDirectory "docs/_site" -ExitOnFailure
.NOTES
    Use this after a successful DocFX build. Useful in CI pipelines as a secondary safety net.
#>

param(
    [Parameter()]
    [string]$SiteDirectory = "",

    [Parameter()]
    [switch]$ExitOnFailure
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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

function Test-HtmlLinks {
    param(
        [string]$SiteDir
    )
    
    if (-not (Test-Path $SiteDir)) {
        Write-Error "Site directory not found: $SiteDir"
        return $false
    }

    $htmlFiles = Get-ChildItem -Path $SiteDir -Recurse -Filter "*.html"
    $brokenLinks = @()
    $checkedLinks = @{}
    $totalLinks = 0

    Write-Host "Found $($htmlFiles.Count) HTML files to check" -ForegroundColor Cyan
    
    foreach ($htmlFile in $htmlFiles) {
        try {
            $content = Get-Content -Path $htmlFile.FullName -Raw
            
            # Simple regex to find href attributes (internal links starting with ~ or relative paths)
            $pattern = 'href\s*=\s*["\'']([^"\'']*)["\'']'
            $linkMatches = [regex]::Matches($content, $pattern)
            
            foreach ($match in $linkMatches) {
                $totalLinks++
                $link = $match.Groups[1].Value
                
                # Skip external links, anchors, and mailto links
                if ($link -match '^(https?://|mailto:|#)') {
                    continue
                }
                
                # Skip already checked links
                if ($checkedLinks.ContainsKey($link)) {
                    continue
                }
                $checkedLinks[$link] = $true
                
                # Convert ~ to site root and resolve relative paths
                $targetPath = $link
                if ($targetPath -match '^~/?(.*)') {
                    $targetPath = $matches[1]
                }
                
                # Remove fragment identifiers for file existence check
                $targetPath = $targetPath -replace '#.*$', ''
                
                # Skip empty paths
                if (-not $targetPath -or $targetPath -eq '') {
                    continue
                }
                
                # Build full path to check
                $fullPath = Join-Path $SiteDir $targetPath
                
                # Check if target exists
                if (-not (Test-Path $fullPath)) {
                    $brokenLinks += @{
                        File = $htmlFile.FullName
                        Link = $link
                        TargetPath = $targetPath
                        RelativePath = $htmlFile.FullName.Substring($SiteDir.Length)
                    }
                }
            }
        }
        catch {
            Write-Warning "Failed to process $($htmlFile.FullName): $($_.Exception.Message)"
        }
    }

    # Report results
    Write-Host "`nLink validation results:" -ForegroundColor Cyan
    Write-Host "Total links checked: $($checkedLinks.Count)" -ForegroundColor White
    Write-Host "Total broken links: $($brokenLinks.Count)" -ForegroundColor White
    
    if ($brokenLinks.Count -gt 0) {
        Write-Header "Broken Links Found"
        foreach ($broken in $brokenLinks | Sort-Object File) {
            Write-Host "$($broken.RelativePath) -> $($broken.Link)" -ForegroundColor Red
        }
        return $false
    } else {
        Write-Success "No broken internal links found"
        return $true
    }
}

# Main execution
try {
    Write-Header "HTML Link Validation"
    
    # Default to docs/_site if not specified
    if (-not $SiteDirectory) {
        $SiteDirectory = Join-Path $PSScriptRoot '..' | Join-Path -ChildPath 'docs/_site'
        $SiteDirectory = [System.IO.Path]::GetFullPath($SiteDirectory)
    }
    
    Write-Host "Validating links in: $SiteDirectory" -ForegroundColor Cyan
    
    $success = Test-HtmlLinks -SiteDir $SiteDirectory
    
    if ($success) {
        Write-Success "HTML link validation completed successfully"
        exit 0
    } else {
        Write-Error "HTML link validation found broken links"
        if ($ExitOnFailure) {
            exit 1
        }
        exit 0
    }
}
catch {
    Write-Error "Link validation script failed: $($_.Exception.Message)"
    exit 1
}