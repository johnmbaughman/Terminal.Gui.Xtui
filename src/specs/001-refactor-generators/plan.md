```markdown
# Implementation Plan: Refactor Generators — Phase 1 (Helpers)

**Feature Branch**: `001-refactor-generators`
**Created**: 2025-12-15
**Phase**: 1 — Foundation (Helpers extraction)

## Goal

Extract common Roslyn syntax construction and generator helper logic into a small, well-tested `Generators/Helpers/` library so subsequent generator refactors are low-risk and incremental. Preserve byte-for-byte parity for the authoritative baseline (`refactor/generated-baseline.cs`) after each change.

## Deliverables (Phase 1)

- `src/Terminal.Gui.Xtui/Generators/Helpers/SyntaxHelpers.cs`
- `src/Terminal.Gui.Xtui/Generators/Helpers/TypeNameHelpers.cs`
- `src/Terminal.Gui.Xtui/Generators/Helpers/NamespaceHelpers.cs`
- `src/Terminal.Gui.Xtui/Generators/Helpers/ChildProcessingHelpers.cs`
- `src/Terminal.Gui.Xtui/Generators/Helpers/ClassGenerationHelpers.cs`
- Unit tests under `src/Terminal.Gui.Xtui.Tests/Generators.Helpers.Tests/` (one test class per helper, target >90% coverage)
- A CI job step to run helper unit tests and run baseline generation diff

**Note**: `BaseControlGenerator`, `BaseContainerGenerator`, and `FieldTransformationHelpers` are deferred to Phase 2.

## Tasks

1. Create helpers skeleton
   - Create the `Generators/Helpers/` directory and add the 5 helper files with public/internal static APIs (no implementation detail borrow yet).
   - Add XML doc comments for each public method explaining contract and expected invariants.
   - Estimated: 1–2 hours

2. Implement `TypeNameHelpers.cs`
   - Implement `GetLocalTypeName`, `GetNamespace`, `IsQualified`, `GenerateVarName`, `GetControlVarName`.
   - Add unit tests covering simple and edge cases (qualified names, null/empty inputs, `Id` attribute present).
   - Estimated: 1 hour

3. Implement `SyntaxHelpers.cs`
   - Implement `CreateObjectWithInitializer`, `CreateLocalDeclaration`, `CreateFieldDeclaration`, property/this assignment helpers, and Add invocation helpers.
   - Add unit tests that assert produced SyntaxNodes match expected shapes (use `.ToFullString()` comparisons where appropriate).
   - Estimated: 2–3 hours

4. Implement `NamespaceHelpers.cs`
   - Implement namespace collection and `MapXmlNamespaceUriToCSharp` logic per assessment plan.
   - Add tests for XML namespace varieties (clr-namespace:, terminal gui xtui, plain strings).
   - Estimated: 1 hour

5. Implement `ChildProcessingHelpers.cs`
   - Implement `GetContainerOrDirectChildren` and `ProcessChildrenWithParamArrayAdd` (statements list mutation style used by generators).
   - Add tests simulating small element trees (mock `ElementNode` instances or minimal real ones) to validate produced statements and order.
   - Estimated: 1–2 hours

6. Implement `ClassGenerationHelpers.cs`
   - Implement `BuildInitializeComponentMethod`, `BuildPartialClass`, `BuildCompilationUnit`, and `AddNullableDirective`.
   - Add tests that build a small compilation unit and assert `ToFullString()` contains expected structure and using directives.
   - Estimated: 1–2 hours

7. CI and baseline diff integration
    - Add a CI job (or augment existing test job) to run helper tests and generate the baseline target using the current generator, then run a byte-for-byte diff against `refactor/generated-baseline.cs`.
    - Use this command for diff (ignores CR/EOL differences):
       ```powershell
       git --no-pager diff --no-index --ignore-cr-at-eol refactor\generated-baseline.cs path\to\generated\file.cs
       ```
   - This step must run on each PR touching `src/Generators/Helpers/*` or generator files.
   - Estimated: 1–2 hours

8. Refactor one low-risk generator to use helpers (validation step)
   - Pick `ButtonGenerator` (or `GenericGenerator`) and replace duplicated patterns with helper calls. Keep generator logic functionally identical.
   - Before making changes, run generator benchmarks to capture baseline performance; after changes, run benchmarks again and compare. Use `dotnet run -p Terminal.Gui.Xtui.Benchmarks --framework net7.0` (or CI benchmark job) and capture results. PRs must include benchmark comparison and explanation for any regression >10%.
   - Run tests and baseline diff.
   - Estimated: 1–2 hours

## Validation & Acceptance (Phase 1)

- All helper unit tests pass locally and in CI.
- Generated baseline file (for UICatalogTop example) is byte-for-byte identical to `refactor/generated-baseline.cs` after refactoring one generator in this phase.
- Code review PR created for helper extraction containing small, self-contained changes and tests.

## Risks & Mitigations

- Risk: Subtle whitespace/order changes in Syntax generation. Mitigation: Use `ToFullString()` comparisons in tests and run exact diff against baseline during PRs.
- Risk: Helper API decisions that make later refactors awkward. Mitigation: Keep helpers narrowly focused; prefer small, composable methods.

## Estimated Total Effort (Phase 1)

Approximately 9–14 developer-hours (split across small PRs). Time estimates assume familiarity with Roslyn API used by the project.

## Next steps after Phase 1

- Proceed to Phase 2: create `BaseControlGenerator` and `BaseContainerGenerator`, refactor multiple simple generators to extend them, and run validations per Phase 2 plan.

## Branching & Stacked PRs (how to apply the plan)

All work in Phase 1 (and subsequent phases) must follow the iterative-branching, stacked-PRs strategy described in the spec. Practical guidance:

- Create a small child branch for each logical step (e.g., `001-refactor-generators/phase1/helpers-skeleton`, `.../type-name-helpers`, `.../syntax-helpers`).
- Open a focused PR from each child branch targeting the previous phase's branch (not `main`) so reviewers can see incremental changes in order. Name and describe the PR with the phase and small scope (e.g., "Phase 1.1: Add TypeNameHelpers tests + skeleton").
- Each PR must be test-first: include failing tests that express the contract, then the implementation commit that makes tests pass within the same PR (or as two ordered commits). Prefer small, easily-reviewable commits.
- Before implementing functional helpers, ensure `T013` (benchmark baseline capture) is completed and artifacts are available; include benchmark output links in PR description when a change can affect performance.
- If a parent branch receives fixes after child branches are opened, update child branches by rebasing onto the parent or merging the parent's branch so child PRs reflect fixes. Document rebases/merges in PR comments.
- CI gates: every stacked PR must pass unit tests and the baseline diff check (if the PR touches generation code or helper APIs). Rebaseline changes require explicit feature-owner approval and a detailed diff explanation.

This approach keeps changes incremental, simplifies review, and ensures child PRs remain a truthful, test-backed progression from skeleton → helpers → generator refactors.

## CI / GitHub Actions requirement

The presence of GitHub Actions workflows that implement the CI tasks required by this refactor is a hard prerequisite before any implementation work begins. Workflows MUST be committed to the feature root branch `001-refactor-generators` and validated with at least one successful run before opening child implementation branches. Child branches and stacked PRs will inherit CI coverage from the feature root.

Minimum required workflows and behavior:

- `.github/workflows/ci-tests.yml` — runs on `push` and `pull_request` and must:
   - Restore, build and run unit tests for changed projects.
   - Run the generator to produce the baseline output to a canonical artifact path: `artifacts/generated/generated-baseline.cs`.
   - Run the baseline diff step comparing the source baseline against the generated artifact:
   ```powershell
   git --no-pager diff --no-index --ignore-cr-at-eol refactor\generated-baseline.cs artifacts\generated\generated-baseline.cs
   ```
   - Fail the job if the diff reports differences and upload the generated file and the diff output as CI artifacts (use `actions/upload-artifact`).
   - Benchmark artifacts MUST be uploaded under `artifacts/benchmarks/` with a predictable filename, e.g. `artifacts/benchmarks/GeneratorBenchmarks-<run-id>.json`.
   - Provide a small summary JSON `artifacts/benchmarks/summary.json` with the structure: `{ "benchmark": "GeneratorBenchmarks", "median_ms": <number>, "runs": <number>, "run_id": "<id>" }` so PR reviewers and bots can compare before/after.

- `.github/workflows/benchmarks.yml` — runs on `workflow_dispatch` and on `push` to benchmark/CI branches and must:
   - Build and run `Terminal.Gui.Xtui.Benchmarks` (e.g. `dotnet run -p Terminal.Gui.Xtui.Benchmarks --framework net7.0` or the appropriate target framework in CI).
   - Capture benchmark output (stdout and any produced result files) and upload as artifacts.
   - Optionally publish a simple JSON summary artifact used by PR reviewers / bots to compare before/after values.

Benchmark measurement methodology (required)

- Run-count: perform N=5 independent runs per benchmark job and collect elapsed times (wall-clock milliseconds). Use the median of the N runs as the canonical metric to reduce noise.
- Runner/environment: CI will use `ubuntu-latest` for benchmark consistency. Document OS/runtime used in benchmark artifact metadata.
- Command (example):
   ```powershell
   # Run the benchmark N=5 times and capture elapsed ms
   $results = @()
   for ($i = 1; $i -le 5; $i++) {
      $start = [DateTime]::UtcNow
      dotnet run -p Terminal.Gui.Xtui.Benchmarks --framework net8.0 --no-build
      $end = [DateTime]::UtcNow
      $results += ([math]::Round(($end - $start).TotalMilliseconds,0))
   }
   $median = ($results | Sort-Object)[int]([math]::Floor($results.Count/2))
   $summary = @{ benchmark = 'GeneratorBenchmarks'; median_ms = $median; runs = 5; run_id = (Get-Date -Format 'yyyyMMddHHmmss') }
   $summary | ConvertTo-Json | Out-File artifacts\benchmarks\summary.json -Encoding utf8
   $results | ConvertTo-Json | Out-File artifacts\benchmarks\raw-runs.json -Encoding utf8
   ```

- Artifact names: upload `artifacts/generated/generated-baseline.cs`, `artifacts/benchmarks/GeneratorBenchmarks-<run-id>.json`, and `artifacts/benchmarks/summary.json` for each run. PR descriptions should link these artifacts.

- Interpretation: PRs must compare `summary.json` median_ms values between parent and child runs; regressions >10% require justification or rollback.

Guidance and gating rules:

- Workflows must be present and green on a parent branch before child implementation branches begin. That means either workflows are already in `main` or they are added early on the feature root branch and validated by at least one successful run.
- Each stacked PR must run these workflows in CI. PR descriptions should include links to the workflow run(s) and any benchmark artifacts when relevant.
- Rebaseline PRs or PRs that intentionally change generated output require explicit feature-owner approval and must include the CI run links and a clear diff explanation in the PR body.

Why this is required: having CI coverage (tests, baseline diff, and benchmarks) in place before implementation prevents wasted effort on local changes that cannot be validated in CI and ensures every stacked PR is verifiable by reviewers.

```