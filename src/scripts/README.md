# Benchmark Scripts for Feature 001-refactor-generators

This directory contains benchmark and code metrics analysis scripts for tracking refactoring progress.

## Scripts

### `run-benchmarks.ps1`

Runs GeneratorBenchmarks N=5 times, computes median performance, analyzes code metrics, and stores results.

**Usage:**
```powershell
.\scripts\run-benchmarks.ps1
```

**Optional Parameters:**
- `-RunCount` (default: 5): Number of benchmark runs
- `-BenchmarkProject`: Path to benchmark project
- `-OutputDir`: Output directory for artifacts (default: `artifacts/benchmarks/`)
- `-GeneratorsDir`: Path to Generators directory for LOC analysis

**Output:**
- `artifacts/benchmarks/summary.json`: Contains median performance (ms) and code metrics
- `artifacts/benchmarks/raw-runs-<run-id>.json`: Contains all individual run times

**Code Metrics Captured:**
- Files count
- Total lines, code lines, comment lines, blank lines
- Average lines per file
- Separate metrics for `Generators/` and `Generators/Helpers/` directories

### `compare-benchmarks.ps1`

Compares baseline benchmark results with current results to detect regressions.

**Usage:**
```powershell
.\scripts\compare-benchmarks.ps1 -BaselinePath .\artifacts\benchmarks\baseline-summary.json -CurrentPath .\artifacts\benchmarks\summary.json
```

**Parameters:**
- `-BaselinePath` (required): Path to baseline summary.json
- `-CurrentPath` (required): Path to current summary.json
- `-RegressionThreshold` (default: 10.0): Performance regression threshold percentage

**Exit Codes:**
- 0: PASSED - Performance within acceptable range
- 1: FAILED - Performance regression exceeds threshold

**Output:**
- Performance comparison (baseline vs current, % change)
- Code metrics comparison (LOC changes, file count changes)
- Total code impact analysis

## Workflow

1. **Capture Baseline** (T013):
   ```powershell
   .\scripts\run-benchmarks.ps1
   Copy-Item .\artifacts\benchmarks\summary.json .\artifacts\benchmarks\baseline-summary.json
   ```

2. **After Code Changes** (T016):
   ```powershell
   # Run benchmarks with current code
   .\scripts\run-benchmarks.ps1
   
   # Compare against baseline
   .\scripts\compare-benchmarks.ps1 -BaselinePath .\artifacts\benchmarks\baseline-summary.json -CurrentPath .\artifacts\benchmarks\summary.json
   ```

3. **CI Integration**:
   - GitHub Actions workflows will run these scripts automatically
   - Artifacts uploaded for each PR
   - CI fails if performance regression >10%

## Success Criteria

- **SC-004**: Total generator code lines reduced by ≥40% vs baseline
- **SC-006**: Mean generation time must not exceed baseline median by >10%

Both criteria are validated by these scripts and enforced in CI.
