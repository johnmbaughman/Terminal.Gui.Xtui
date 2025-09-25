#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Pack the library and perform a quick install smoke test.
.DESCRIPTION
    Creates a NuGet package from the Terminal.Gui.Xaml project, then scaffolds a temporary console app
    to verify the package can be added and the project builds. Cleans the temp project afterward.
.PARAMETER Configuration
    Build configuration for packing. Defaults to Debug.
.EXAMPLE
    ./pack-and-test.ps1 -Configuration Release
    Produces a Release package and validates install/build in a throwaway project.
.NOTES
    Uses a local ./nupkg output folder as a temporary NuGet source during the test.
#>

param(
    [string]$Configuration = "Debug"
)

Write-Host "Packing Terminal.Gui.Xaml NuGet package..."
dotnet pack src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj -c $Configuration --output ./nupkg

Write-Host "Running package install test..."
dotnet new console -n TestInstall -o ./TestInstall
cd ./TestInstall
Write-Host "Adding Terminal.Gui.Xaml package..."
dotnet add package Terminal.Gui.Xaml --source ../nupkg --prerelease
Write-Host "Restoring packages..."
dotnet restore
Write-Host "Build test project..."
dotnet build
cd ..
Write-Host "Cleaning up test project..."
Remove-Item -Recurse -Force ./TestInstall
Write-Host "Pack and install test complete."
