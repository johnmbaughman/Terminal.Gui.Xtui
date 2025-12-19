$env:GENERATE_BASELINE = 'true'

Write-Output "Running baseline generation test..."

# Run the specific test that writes the generated baseline when GENERATE_BASELINE=true
dotnet test Terminal.Gui.Xtui.Tests/Terminal.Gui.Xtui.Tests.csproj --filter 'FullyQualifiedName~BaselineGenerationTests.GenerateBaseline_ForCI' --no-build

# Determine repo root (parent of src)
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Split-Path $scriptRoot -Parent
$genFile = Join-Path $repoRoot 'artifacts\generated\generated-baseline.cs'

Write-Output "Looking for generated file: $genFile"

if (Test-Path $genFile) {
    $diffFile = Join-Path $repoRoot 'artifacts\generated\baseline-diff.txt'
    git --no-pager diff --no-index --ignore-cr-at-eol refactor/generated-baseline.cs $genFile > $diffFile
    Write-Output '--- DIFF ---'
    Get-Content $diffFile -Raw
} else {
    Write-Error "generated file not found at: $genFile"
    exit 2
}
