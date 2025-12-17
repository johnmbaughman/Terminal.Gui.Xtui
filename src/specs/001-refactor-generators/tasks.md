# Tasks for Feature 001-refactor-generators

Phase 1: Setup

**Infrastructure Setup (Test-First Exemption)**: Tasks T001, T002, T013, T028–T031 are infrastructure/scaffolding tasks and exempt from the test-first workflow (Constitution Principle II applies to production code, not CI YAML or governance docs). These tasks MUST complete before TDD implementation tasks (T003–T012) begin.

- [X] T001 [P] Create helpers directory at `src/Terminal.Gui.Xtui/Generators/Helpers/` (acceptance: directory exists, committed in small PR)
- [X] T002 [P] Create tests skeleton for helpers at `src/Terminal.Gui.Xtui.Tests/Generators.Helpers.Tests/` (acceptance: test project compiles, contains placeholder test that fails)

- [X] T013 [US1] Capture benchmark baseline: run `Terminal.Gui.Xtui.Benchmarks` `GeneratorBenchmarks` N=5 times (see plan.md methodology), compute median, analyze code metrics (LOC), store results in CI artifacts as `artifacts/benchmarks/summary.json` with both performance and code metrics data (acceptance: benchmark summary artifact uploaded with median_ms, code_metrics.generators, code_metrics.helpers, and run_id)

- [X] T028 Create GitHub Actions workflows: add `.github/workflows/ci-tests.yml` and `.github/workflows/benchmarks.yml` implementing unit tests, baseline diff, and benchmark artifact upload (acceptance: workflows committed to feature root branch `001-refactor-generators` and run at least once)
- [X] T029 Validate CI workflows on feature root branch: ensure workflows run successfully and upload artifacts to `artifacts/generated/` and `artifacts/benchmarks/`; record run URLs in the feature tracking doc (acceptance: at least one successful `ci-tests` run and one `benchmarks` run with artifacts)

- [X] T030 Constitution compliance check: add `specs/001-refactor-generators/CONSTITUTION_CHECK.md` listing all 7 constitution principles and documenting how this feature addresses each (with task/requirement references). Add CI step to verify file exists (acceptance: file created with principle enumeration; CI check added)

- [X] T031 Enforce public generator API outputs are strings: add unit test in `src/Terminal.Gui.Xtui.Tests/` asserting public generator methods (e.g., `GenerateClass`) return `string` type. Add CI step to run this test (acceptance: test added to test project; CI job includes test execution).

Branching & PR workflow (stacked PRs)

The implementation will use iterative, small branches and stacked PRs. Additions below make the process explicit and traceable in tasks.

- [ ] T024 Create per-helper child branches (naming convention: `001-refactor-generators/phase1/<short-task>`) and open stacked PRs against the previous branch (acceptance: one child branch per helper with PR created)
- [ ] T025 For each helper PR (TypeName, Syntax, Namespace, ChildProcessing, ClassGeneration) include a failing test commit followed by an implementation commit (acceptance: PR contains both failing and fixing commits or clearly-separated commits that make tests pass)
- [ ] T026 When parent branches receive fixes, update child branches by rebasing or merging and add a short PR comment documenting the update (acceptance: child PRs reflect parent fixes and CI passes)
- [ ] T027 Ensure each stacked PR includes links to benchmark artifacts (when relevant) and a short note about whether baseline diff was run locally/CI (acceptance: PR description includes artifact links or a CI run reference)

Test-First (TDD) pattern — implement each helper via a failing test then implementation

**TDD Commit Convention**: Each helper PR must include separate commits: (1) commit adding failing tests, (2) commit with implementation that makes tests pass. Do not squash until review complete. Target >90% code coverage per helper.

- [X] T003 [P] Add failing unit tests for `TypeNameHelpers` (test-first; acceptance: tests added and fail)
- [X] T004 [P] Implement `TypeNameHelpers` to satisfy tests (acceptance: tests pass)

- [X] T005 [P] Add failing unit tests for `SyntaxHelpers` (test-first; acceptance: tests added and fail)
- [X] T006 [P] Implement `SyntaxHelpers` to satisfy tests (acceptance: tests pass)

- [X] T007 [P] Add failing unit tests for `NamespaceHelpers` (test-first; acceptance: tests added and fail)
- [X] T008 [P] Implement `NamespaceHelpers` to satisfy tests (acceptance: tests pass)

- [X] T009 [P] Add failing unit tests for `ChildProcessingHelpers` (test-first; acceptance: tests added and fail)
- [X] T010 [P] Implement `ChildProcessingHelpers` to satisfy tests (acceptance: tests pass)

- [X] T011 [P] Add failing unit tests for `ClassGenerationHelpers` (test-first; acceptance: tests added and fail)
- [X] T012 [P] Implement `ClassGenerationHelpers` to satisfy tests (acceptance: tests pass)

- [X] T012a [P] [US3] Add failing unit tests for `FieldTransformationHelpers` (extracted from TopLevelGenerator; test-first; >95% coverage target; acceptance: comprehensive tests added and fail)
- [X] T012b [P] [US3] Implement `FieldTransformationHelpers` to satisfy tests (extract variable→field transformation logic from TopLevelGenerator; acceptance: tests pass with >95% coverage)

Integration & Validation (baseline parity, CI, benchmarks)

- [X] T014 [US1] Add CI job step to run helper tests, run generator to produce the baseline target, and perform a diff against `refactor/generated-baseline.cs` using:
	```powershell
	git --no-pager diff --no-index --ignore-cr-at-eol refactor\generated-baseline.cs path\to\generated\file.cs
	```

- [X] T015 [US1] Add integration test `src/Terminal.Gui.Xtui.Tests/BaselineGenerationTests.cs` that runs the generator for the baseline input and asserts exact match with `refactor/generated-baseline.cs` (acceptance: test added and initially fails until generator produces expected output)
- [X] T016 [US1] Capture benchmark after changes and include comparison in PR (measure mean generation time in milliseconds; CI fails if regression >10% vs baseline median; include code metrics comparison to demonstrate progress toward SC-004 code size reduction target)

Refactor validation steps (incremental, low-risk)

- [X] T017 [P] [US2] Refactor `src/Terminal.Gui.Xtui/Generators/ButtonGenerator.cs` to use helper APIs (follow T003/T004 test-first pattern for generator-level tests). Acceptance: baseline parity preserved and unit/integration tests pass.
- [X] T018 [P] [US2] Refactor remaining simple generators to use helpers: `CheckBoxGenerator.cs`, `LabelGenerator.cs`, `TextFieldGenerator.cs`, `ListViewGenerator.cs`, `GenericGenerator.cs` (each follows test-first pattern and preserves baseline parity)

- [ ] T019 [US3] Implement `BaseControlGenerator` and `BaseContainerGenerator` and refactor container generators (`MenuBarGenerator.cs`, `StatusBarGenerator.cs`, `WindowGenerator.cs`) to extend them (test-first, acceptance: parity preserved)

Cross-cutting & Governance

- [ ] T021 Update `.specify` scripts and docs so `check-prerequisites` accepts `specs/` or `src/specs/` locations (acceptance: prereq script succeeds from repo root and `src` layouts)
- [ ] T022 Create `specs/001-refactor-generators/checklists/merge_guidelines.md` documenting branch/PR/zero-tolerance rebaseline policy and CI requirements (acceptance: file created, documents that baseline diffs are bugs not rebaseline scenarios, and referenced in PR template)
- [ ] T023 Update `README.md` or project docs with new helper API usage notes at `docs/REFACTORING.md` (acceptance: docs added)

Dependencies & Execution Order

- Setup: `T001`, `T002` must complete first.
- For each helper: add failing tests then implement (`T003`→`T004`, `T005`→`T006`, `T007`→`T008`, `T009`→`T010`, `T011`→`T012`, `T012a`→`T012b`).
- Integration validation (`T013`–`T016`) must run before generator refactors (`T017`–`T019`).
- `T021` (specify script update) should run early (can be done in parallel with setup) so prereq scripts work.

- Important: `T013` (benchmark baseline capture) is a *blocker* that must be completed before any helper implementation tasks (`T004`, `T006`, `T008`, `T010`, `T012`, `T012b`) and before any generator refactor (`T017`–`T019`). This ensures you have a performance baseline to compare against before code changes.


- Important: `T028`/`T029`/`T030` (GitHub Actions workflows creation and validation, and Constitution compliance check) are HARD prerequisites and must be completed *before any implementation tasks begin* (this includes helper implementations `T004`, `T006`, `T008`, `T010`, `T012`, `T012b` and any generator refactors `T017`–`T019`). Do not open implementation child branches until the workflows exist and have at least one successful run on a parent branch and the constitution check file is present.

- Zero-tolerance rebaseline policy: Any PR showing a baseline diff must be treated as a bug and fixed before merge. This refactor does NOT change generator output—only internal code organization.

- Branching note: Each helper implementation (`T004`, `T006`, `T008`, `T010`, `T012`, `T012b`) must be implemented on its own child branch and opened as a stacked PR per `T024`/`T025`. Do not implement multiple helpers in a single monolithic PR.

Parallelization notes

- Tasks marked `[P]` are safe to implement in parallel when they touch different files. Generator refactors can be done in parallel after integration validation passes.

Estimated effort

- Setup & scaffolding: 1–2 hours
- Implementing each helper (test-first): 1–3 hours each
- Tests + CI integration + benchmarks: 4–6 hours
- Per-generator refactor validation: 1–2 hours per generator

Total estimated Phase 1 effort: ~10–18 hours
