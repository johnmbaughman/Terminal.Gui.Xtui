# Documentation Generation Benchmarks

This project contains BenchmarkDotNet benchmarks for the simulated documentation generation services.

## Benchmarks
- Full Generation
- Incremental Generation (should be faster or comparable)

## Persistence
After each run, mean execution times (in nanoseconds) are written to `.benchmarks/last-results.json` relative to the current working directory. Example structure:
```json
{
  "Full Generation": 123456.0,
  "Incremental Generation": 84567.2
}
```

## Regression Detection
If a previous `last-results.json` exists, current means are compared. Any benchmark >20% slower is reported.

To fail CI on regression, set environment variable:
```
TGXAML_FAIL_ON_BENCH_REGRESSION=1
```
Exit code 2 indicates a regression.

## Running
From repository root:
```
dotnet run -c Release -p tests/Terminal.Gui.Xaml.Benchmarks/Terminal.Gui.Xaml.Benchmarks.csproj
```

## Notes
- Benchmarks use a single warmup and 5 iterations for quick feedback.
- Adjust job attributes in `DocumentationGenerationBenchmarks` for deeper analysis.
- The generation service is simulated; results are relative indicators for regressions, not absolute DocFX performance.
