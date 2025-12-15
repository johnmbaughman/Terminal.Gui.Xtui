# Tasks for Feature 001-refactor-generators

Phase 1: Setup

- [ ] T001 [P] Create helpers directory at `src/Terminal.Gui.Xtui/Generators/Helpers/` (acceptance: directory exists, committed in small PR)
- [ ] T002 [P] Create tests skeleton for helpers at `src/Terminal.Gui.Xtui.Tests/Generators.Helpers.Tests/` (acceptance: test project compiles, contains placeholder test that fails)

- [ ] T013 [US1] Capture benchmark baseline: run `Terminal.Gui.Xtui.Benchmarks` `GeneratorBenchmarks` and store results in CI artifacts (acceptance: benchmark output saved)

Branching & PR workflow (stacked PRs)

The implementation will use iterative, small branches and stacked PRs. Additions below make the process explicit and traceable in tasks.

- [ ] T024 Create per-helper child branches (naming convention: `001-refactor-generators/phase1/<short-task>`) and open stacked PRs against the previous branch (acceptance: one child branch per helper with PR created)
- [ ] T025 For each helper PR (TypeName, Syntax, Namespace, ChildProcessing, ClassGeneration) include a failing test commit followed by an implementation commit (acceptance: PR contains both failing and fixing commits or clearly-separated commits that make tests pass)
- [ ] T026 When parent branches receive fixes, update child branches by rebasing or merging and add a short PR comment documenting the update (acceptance: child PRs reflect parent fixes and CI passes)
- [ ] T027 Ensure each stacked PR includes links to benchmark artifacts (when relevant) and a short note about whether baseline diff was run locally/CI (acceptance: PR description includes artifact links or a CI run reference)

Test-First (TDD) pattern — implement each helper via a failing test then implementation

- [ ] T003 [P] Add failing unit tests for `TypeNameHelpers` (test-first; acceptance: tests added and fail)
- [ ] T004 [P] Implement `TypeNameHelpers` to satisfy tests (acceptance: tests pass)

- [ ] T005 [P] Add failing unit tests for `SyntaxHelpers` (test-first; acceptance: tests added and fail)
- [ ] T006 [P] Implement `SyntaxHelpers` to satisfy tests (acceptance: tests pass)

- [ ] T007 [P] Add failing unit tests for `NamespaceHelpers` (test-first)
- [ ] T008 [P] Implement `NamespaceHelpers` to satisfy tests

- [ ] T009 [P] Add failing unit tests for `ChildProcessingHelpers` (test-first)
- [ ] T010 [P] Implement `ChildProcessingHelpers` to satisfy tests

- [ ] T011 [P] Add failing unit tests for `ClassGenerationHelpers` (test-first)
- [ ] T012 [P] Implement `ClassGenerationHelpers` to satisfy tests

Integration & Validation (baseline parity, CI, benchmarks)

- [ ] T014 [US1] Add CI job step to run helper tests, run generator to produce the baseline target, and perform a diff against `refactor/generated-baseline.cs` using:
	```powershell
	git --no-pager diff --no-index --ignore-cr-at-eol refactor\generated-baseline.cs path\to\generated\file.cs
	```

- [ ] T015 [US1] Add integration test `src/Terminal.Gui.Xtui.Tests/BaselineGenerationTests.cs` that runs the generator for the baseline input and asserts exact match with `refactor/generated-baseline.cs` (acceptance: test added and initially fails until generator produces expected output)
- [ ] T016 [US1] Capture benchmark after changes and include comparison in PR (fail PR if regression >10%)

Refactor validation steps (incremental, low-risk)

- [ ] T017 [P] [US2] Refactor `src/Terminal.Gui.Xtui/Generators/ButtonGenerator.cs` to use helper APIs (follow T003/T004 test-first pattern for generator-level tests). Acceptance: baseline parity preserved and unit/integration tests pass.
- [ ] T018 [P] [US2] Refactor remaining simple generators to use helpers: `CheckBoxGenerator.cs`, `LabelGenerator.cs`, `TextFieldGenerator.cs`, `ListViewGenerator.cs`, `GenericGenerator.cs` (each follows test-first pattern and preserves baseline parity)

- [ ] T019 [US3] Implement `BaseControlGenerator` and `BaseContainerGenerator` and refactor container generators (`MenuBarGenerator.cs`, `StatusBarGenerator.cs`, `WindowGenerator.cs`) to extend them (test-first, acceptance: parity preserved)
- [ ] T020 [US3] Extract `FieldTransformationHelpers.cs` from `TopLevelGenerator.cs` and add unit tests to ensure variable→field rewriting is preserved (acceptance: parity preserved)

Cross-cutting & Governance

- [ ] T021 Update `.specify` scripts and docs so `check-prerequisites` accepts `specs/` or `src/specs/` locations (acceptance: prereq script succeeds from repo root and `src` layouts)
- [ ] T022 Create `specs/001-refactor-generators/checklists/merge_guidelines.md` documenting branch/PR/rebaseline governance and CI requirements (acceptance: file created and referenced in PR template)
- [ ] T023 Update `README.md` or project docs with new helper API usage notes at `docs/REFACTORING.md` (acceptance: docs added)

Dependencies & Execution Order

- Setup: `T001`, `T002` must complete first.
- For each helper: add failing tests then implement (`T003`→`T004`, `T005`→`T006`, `T007`→`T008`, `T009`→`T010`, `T011`→`T012`).
- Integration validation (`T013`–`T016`) must run before generator refactors (`T017`–`T020`).
- `T021` (specify script update) should run early (can be done in parallel with setup) so prereq scripts work.

- Important: `T013` (benchmark baseline capture) is a *blocker* that must be completed before any helper implementation tasks (`T004`, `T006`, `T008`, `T010`, `T012`) and before any generator refactor (`T017`–`T020`). This ensures you have a performance baseline to compare against before code changes.

- Branching note: Each helper implementation (`T004`, `T006`, `T008`, `T010`, `T012`) must be implemented on its own child branch and opened as a stacked PR per `T024`/`T025`. Do not implement multiple helpers in a single monolithic PR.

Parallelization notes

- Tasks marked `[P]` are safe to implement in parallel when they touch different files. Generator refactors can be done in parallel after integration validation passes.

Estimated effort

- Setup & scaffolding: 1–2 hours
- Implementing each helper (test-first): 1–3 hours each
- Tests + CI integration + benchmarks: 4–6 hours
- Per-generator refactor validation: 1–2 hours per generator

Total estimated Phase 1 effort: ~10–18 hours
