# Feature Specification: Refactor Generators

**Feature Branch**: `001-refactor-generators`  
**Created**: 2025-12-15  
**Status**: Draft  
**Input**: User description: "Read all docs in `refactor/` and create a Refactoring spec to consolidate generator helpers, reduce duplication, and preserve generated output exactly as baseline."

## Clarifications

### Session 2025-12-15

- Q: Should byte-for-byte parity be required for all existing generated targets or only the provided baseline? → A: Option A - Parity required only for `refactor/generated-baseline.cs`.

- Q: What approval policy should govern rebaseline/merge decisions? → A: Option B - Feature owner approval plus CI sign-off required before merging rebaseline changes.

### Session 2025-12-15 - Implementation Clarifications

- Item 1: Internal implementation MAY use Roslyn `Syntax` node types for performance and fidelity. Public-facing generator APIs or outputs (what producers or downstream consumers call) MUST NOT return `Syntax` nodes and MUST return string source only. This preserves data safety and public contracts.
- Item 2: Follow the plan's recommendations (test-first, incremental phases); Phase 1 will be implemented test-first.
- Item 3: Diff command: use `git --no-pager diff --no-index --ignore-cr-at-eol <baseline> <generated>` for byte-for-byte comparison while ignoring CR/EOL differences. Files are UTF-8 encoded; line-ending normalization is ignored for now.
- Item 4: Benchmarks: a benchmark baseline MUST be captured before any functional code changes and captured again after the refactor; see Plan tasks for commands and CI integration.
- Item 5: `.specify` helper scripts updated to accept specs under `specs/` or `src/specs/` (script change recorded in repo).
- Q: The spec mentions a "10%" performance regression threshold for benchmarks but doesn't specify what performance metric is being measured or what the baseline acceptable range is. → A: Measure mean generation time in milliseconds; fail CI if >10% regression vs baseline median
- Q: The spec identifies edge cases around "rebaseline scenarios" but doesn't specify the concrete procedure or criteria for when a rebaseline is acceptable vs. when it indicates a bug. → A: Never rebaseline during this refactor; any diff is a bug (strictest control for pure refactoring)
- Q: The specification mentions that "TopLevelGenerator has complex rewriting" and needs special handling, but doesn't clarify whether this complexity should be extracted incrementally with comprehensive test coverage or deferred to a later phase. → A: Extract incrementally in Phase 1 with >95% test coverage for transformation logic
- Item 6: Benchmarks MUST include code metrics analysis (lines of code, file count, code/comment/blank line breakdown) to measure code quality improvements. Benchmark artifacts include both performance metrics and LOC analysis for before/after comparison. 

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Preserve Generated Output (Priority: P1)

Maintain byte-for-byte identical generated source for existing known inputs so downstream consumers and binary artifacts remain unchanged.

**Why this priority**: Preserving existing generated output is critical to avoid regressions for consumers and to keep API/behavior compatibility.

**Independent Test**: Run the generator against the same input used to create the baseline file (`UICatalogTop_14D97289.g.cs`), then perform a byte-for-byte diff against `refactor/generated-baseline.cs`.

**Acceptance Scenarios**:

1. **Given** a repository state prior to refactoring and the baseline file `refactor/generated-baseline.cs`, **When** the refactored generators are used to generate the same target file, **Then** the new generated file must match the baseline exactly (including whitespace and ordering).
2. **Given** the CI pipeline, **When** the generation task runs, **Then** it reports no differences for the baseline generation.

---

### User Story 2 - Reduce Duplication & Centralize Helpers (Priority: P2)

Refactor generator implementations to extract shared functionality into a small set of helper classes and well-defined base classes so future maintenance is easier.

**Why this priority**: Reducing duplication reduces bugs, speeds development, and makes tests easier.

**Independent Test**: Unit tests that validate helper APIs and small end-to-end generation tests demonstrating identical output for baseline inputs.

**Acceptance Scenarios**:

1. **Given** the refactored helpers and generators, **When** running unit tests for helpers, **Then** all helper tests pass.
2. **Given** the refactored code, **When** running a targeted generation of the baseline example, **Then** it still matches the baseline (see P1) and helper unit tests validate behavior of extracted logic.

---

### User Story 3 - Incremental, Low-Risk Refactoring (Priority: P3)

Perform refactoring in small, verifiable phases with targeted tests and validation after each phase.

**Why this priority**: Minimizes risk and enables rollback if a phase fails validation.

**Independent Test**: A phase-level checklist with steps (create helpers → refactor one generator → validate baseline → run tests). Each phase should complete with no differences and all tests passing.

**Acceptance Scenarios**:

1. **Given** Phase 1 (helpers extraction) completed, **When** running generation and unit tests, **Then** the baseline generation is unchanged and helper unit tests pass.
2. **Given** Phase 2 (refactor single generator), **When** running generation for impacted files, **Then** no regressions are detected and behavior remains identical.

---

### Edge Cases

- This refactor is a pure internal code restructuring with **zero tolerance for output changes**. Any diff in generated output indicates a bug that must be fixed before proceeding.
- Rebaseline scenarios are **out of scope**: this refactor does not change parser behavior, does not add new attributes/schema support, and does not modify generation logic—only the internal organization of that logic.
- Ordering dependencies that affect whitespace/formatting: The refactor must preserve deterministic ordering exactly as currently implemented.
- Complex rewriting in TopLevelGenerator (variable→field transformation): Must be preserved with byte-for-byte fidelity; extract to `FieldTransformationHelpers` in Phase 1 with >95% test coverage proving exact equivalence via comprehensive unit tests and baseline diff validation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Generation output MUST be byte-for-byte identical to `refactor/generated-baseline.cs` for the provided baseline input (critical requirement).
- **FR-002**: Extract shared Roslyn construction patterns into `Generators/Helpers/SyntaxHelpers.cs` with well-documented, unit-tested methods (e.g., `CreateObjectWithInitializer`, `CreateLocalDeclaration`).
- **FR-003**: Add `Generators/Helpers/TypeNameHelpers.cs` to centralize `GetLocalTypeName` and var-name generation logic.
- **FR-004**: Add `Generators/Helpers/NamespaceHelpers.cs` to centralize namespace collection and using directive generation.
- **FR-005**: Add `Generators/Helpers/ChildProcessingHelpers.cs` and `ClassGenerationHelpers.cs` to encapsulate child processing and class/InitializeComponent assembly.
- **FR-006**: Implement `BaseControlGenerator` and `BaseContainerGenerator` to reduce duplicate generator code and enforce a template method pattern for common workflows.
- **FR-007**: Provide `FieldTransformationHelpers.cs` to encapsulate TopLevelGenerator's complex variable→field transformation with >95% test coverage for all transformation logic to ensure semantic parity.
- **FR-008**: Add comprehensive unit tests for every helper method (target: >90% code coverage per helper, >95% for FieldTransformationHelpers, minimum 2 test scenarios per public method) and integration tests for each generator to validate behavior.
- **FR-009**: After each refactoring phase, run the full test suite and the baseline-generation diff; no differences and all tests must pass before merging.
- **FR-010**: Maintain clear rollback points (separate branches for each phase) and a CI gate that prevents merge on mismatch.

### Key Entities *(include if feature involves data)*

- **Generator**: Abstract base type for all concrete generator implementations (existing)
- **ElementNode**: In-memory representation of a XTUI element node (attributes, children, namespaces)
- **SyntaxHelpers**: New helper that exposes common Roslyn syntax construction utilities
- **TypeNameHelpers**: New helper for parsing and creating type/variable names
- **NamespaceHelpers**: New helper to map XML namespace URIs to C# namespaces and build using directives
- **ChildProcessingHelpers**: New helper for common child processing patterns (container vs. direct children)
- **ClassGenerationHelpers**: New helper for building InitializeComponent method, class skeletons, and compilation units
- **BaseControlGenerator / BaseContainerGenerator**: New base classes that encapsulate repeated generator patterns
- **FieldTransformationHelpers**: Helper for variable-to-field rewriting and syntactic transformations in TopLevelGenerator

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Baseline parity — For the baseline input, generated source is byte-for-byte identical to `refactor/generated-baseline.cs` (100% match).
- **SC-002**: Automated tests — All unit and integration tests pass in CI (100% pass rate for the test suite).
- **SC-003**: Duplication reduction — Lines of duplicated code identified in assessment reduced by at least 80% (target: ~1,400 lines reduced to <300 duplicated lines).
- **SC-004**: Code size reduction — Total generator code lines reduced by ≥40% vs. current baseline. Code metrics (LOC analysis) must be captured in benchmark artifacts for before/after comparison.
- **SC-005**: Incremental validation — Each refactoring phase includes a checklist and CI run; no phase proceeds to the next without passing validation.
- **SC-006**: Performance baseline — Mean generation time (milliseconds) must not exceed baseline median by >10%; CI fails on regression. Benchmark artifacts must include code metrics (total lines, code lines, files count, avg lines/file) for quality improvement tracking.

## Assumptions

- The repository can be built locally and CI provides an environment to run the generator and unit tests.
- `refactor/generated-baseline.cs` is the authoritative baseline for at least one important generator input (UICatalogTop example).
- Developers will perform phase-level PRs and validate changes in CI; we will not land large multi-file changes in a single PR without intermediate validation.
- The refactor will not change public runtime behavior beyond the generator internals; runtime tests will be used to detect regressions.
- **Zero-tolerance rebaseline policy**: This refactor does NOT change how generators parse inputs or construct outputs. Parity is required for `refactor/generated-baseline.cs` and any diff indicates a bug. Future improvements to generation logic are out of scope and must be addressed in separate features.

- **Governance:** Pull requests that change generator output are **not permitted** during this refactor. Any PR showing a baseline diff must fix the regression before merge. CI must fail on any mismatch.

- **Branching & PR strategy:** All refactor work will follow an iterative-branching and stacked PRs approach. Implement each phase or helper in a small child branch and open a focused PR against the previous phase's branch (stacked PRs). Fixes to earlier branches should be made in their respective branches and propagated to child branches by rebasing or merging so child PRs reflect those fixes. Each stacked PR must be individually reviewable, include failing→passing tests for its scope, include benchmark comparisons when performance-relevant, and pass the baseline diff CI check before merging into the parent branch.

- **CI / GitHub Actions requirement:** GitHub Actions workflows will be created to run the CI tasks required by this refactor: unit tests, the baseline-generation diff, and benchmark capture/upload. The presence of these workflows (in `.github/workflows/`) is a hard prerequisite: no implementation tasks (helper implementations or generator refactors) may begin until the required GitHub Actions workflows are present and configured on a parent branch or mainline branch. Workflows must:
	- Run unit tests for changed projects.
	- Produce and upload benchmark artifacts for `Terminal.Gui.Xtui.Benchmarks` when requested (T013/T016).
	- Run the baseline diff step and fail the job on mismatch.
	- Expose artifact links in PRs (or upload artifacts to the run) so reviewers can inspect benchmark outputs and diff results.

	Put simply: creating and validating the GitHub Actions workflows that implement T013–T016 is required before starting T004/T006/T008/T010/T012 or any generator refactor. This reduces wasted work and ensures CI coverage is present for all incremental PRs.

## Deliverables

- `src/Terminal.Gui.Xtui/Generators/Helpers/` with Phase 1 helper classes (SyntaxHelpers, TypeNameHelpers, NamespaceHelpers, ChildProcessingHelpers, ClassGenerationHelpers, FieldTransformationHelpers)
- Phase 2 (deferred): `BaseControlGenerator`, `BaseContainerGenerator`
- Refactored generator implementations using helpers and base classes
- Unit tests for each helper (>90% coverage; >95% for FieldTransformationHelpers) and integration tests for generators
- `specs/001-refactor-generators/spec.md` (this file)
- `specs/001-refactor-generators/checklists/requirements.md` (quality checklist)

## Next Steps

1. Create the helper classes and unit tests (Phase 1).  
2. Refactor a single low-risk generator (e.g., `ButtonGenerator`) to use helpers and validate baseline parity.  
3. Iterate through remaining generators in small PRs, validating baseline parity and test results after each PR.  
4. Create base classes and refactor container generators, validate again.  


**Spec created by:** automated / assistant from `refactor/` docs (assessment & plan).