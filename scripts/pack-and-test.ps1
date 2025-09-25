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
