# Tasks for Feature 001-refactor-generators

Phase 1: Setup

- [ ] T001 [P] Create helpers directory at `src/Terminal.Gui.Xtui/Generators/Helpers/`
- [ ] T002 [P] Create tests skeleton for helpers at `src/Terminal.Gui.Xtui.Tests/Generators.Helpers.Tests/`

Test-first subtasks (TDD enforcement)

- [ ] T001a [P] Add failing unit tests for `TypeNameHelpers` (test-first)
- [ ] T001b [P] Implement `TypeNameHelpers` to satisfy tests
- [ ] T002a [P] Add failing unit tests for `SyntaxHelpers` (test-first)
- [ ] T002b [P] Implement `SyntaxHelpers` to satisfy tests

Foundational (Phase 1 implementation)

- [ ] T003 [P] Implement `TypeNameHelpers` in `src/Terminal.Gui.Xtui/Generators/Helpers/TypeNameHelpers.cs` (follow T001a/T001b test-first pattern)
- [ ] T004 [P] Implement `SyntaxHelpers` in `src/Terminal.Gui.Xtui/Generators/Helpers/SyntaxHelpers.cs` (follow T002a/T002b test-first pattern)
- [ ] T005 [P] Implement `NamespaceHelpers` in `src/Terminal.Gui.Xtui/Generators/Helpers/NamespaceHelpers.cs` (add failing tests first)
- [ ] T006 [P] Implement `ChildProcessingHelpers` in `src/Terminal.Gui.Xtui/Generators/Helpers/ChildProcessingHelpers.cs` (add failing tests first)
- [ ] T007 [P] Implement `ClassGenerationHelpers` in `src/Terminal.Gui.Xtui/Generators/Helpers/ClassGenerationHelpers.cs` (add failing tests first)
- [ ] T008 [P] Add unit tests for each helper in `src/Terminal.Gui.Xtui.Tests/` (one test class per helper)

User Story Phases (organized by independent user story)

- [ ] T009 [US1] Add CI job step to run generator and perform a byte-for-byte diff against `refactor/generated-baseline.cs` (CI config file: `.github/workflows/ci.yml` or equivalent)
- [ ] T010 [US1] Add integration test `src/Terminal.Gui.Xtui.Tests/BaselineGenerationTests.cs` that runs the generator for the baseline input and asserts exact match with `refactor/generated-baseline.cs`

- [ ] T017 [US1] Capture benchmark baseline: run `Terminal.Gui.Xtui.Benchmarks` GeneratorBenchmarks and store results in CI artifacts before code changes
- [ ] T018 [US1] Capture benchmark after changes and include comparison in PR (fail PR if regression >10%)

- [ ] T011 [P] [US2] Refactor `src/Terminal.Gui.Xtui/Generators/ButtonGenerator.cs` to use helper APIs and add tests verifying generated statements remain identical to pre-refactor output
- [ ] T012 [P] [US2] Refactor remaining simple generators to use helpers: `CheckBoxGenerator.cs`, `LabelGenerator.cs`, `TextFieldGenerator.cs`, `ListViewGenerator.cs`, `GenericGenerator.cs` (files in `src/Terminal.Gui.Xtui/Generators/`)

- [ ] T013 [US3] Implement `BaseControlGenerator` and `BaseContainerGenerator` at `src/Terminal.Gui.Xtui/Generators/` and refactor container generators (`MenuBarGenerator.cs`, `StatusBarGenerator.cs`, `WindowGenerator.cs`) to extend them
- [ ] T014 [US3] Extract `FieldTransformationHelpers.cs` from `TopLevelGenerator.cs` and add unit tests to ensure variable→field rewriting is preserved

- [ ] T019 Update `.specify` scripts and docs so `check-prerequisites` accepts `specs/` or `src/specs/` locations (ensure prereq script works in both layouts)

Final Phase: Polish & Cross-cutting

- [ ] T015 Create `specs/001-refactor-generators/checklists/merge_guidelines.md` documenting branch/PR/rebaseline governance and CI requirements
- [ ] T016 Update `README.md` or project docs with new helper API usage notes at `docs/REFACTORING.md`

Dependencies & Execution Order

- `T001`, `T002` must complete before `T003`-`T008`.
- `T008` (helper tests) and `T009` (CI diff) must pass before T011.
- `T011` is a validation step: once it passes baseline parity, `T012` can run in parallel across remaining generators.
- `T013` and `T014` depend on the helper implementations and the per-generator refactors.

Parallelization notes

- Tasks marked `[P]` are safe to implement in parallel (different files, no ordering dependencies).

Estimated effort

- Setup & scaffolding: 1–2 hours
- Implementing each helper: 1–3 hours each (5–10 hours total)
- Tests + CI integration: 3–4 hours
- Per-generator refactor validation: 1–2 hours per generator

Total estimated Phase 1 effort: ~9–16 hours
