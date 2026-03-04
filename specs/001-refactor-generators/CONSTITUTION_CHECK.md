# Constitution Compliance Check: Refactor Generators

**Feature**: `specs/001-refactor-generators`  
**Created**: 2025-12-15  
**Purpose**: Document how this refactoring feature addresses each Terminal.Gui.Xtui Constitution principle

## Constitution Principles Compliance

### 1. Code Quality & Maintainability ✅

**Principle**: Roslyn incremental generators use proper caching; generator logic is pure functions; magic strings eliminated.

**How This Feature Addresses It**:
- **FR-002, FR-003, FR-004, FR-005**: Extract duplicated Roslyn construction patterns into well-tested helper classes (`SyntaxHelpers`, `TypeNameHelpers`, `NamespaceHelpers`, `ChildProcessingHelpers`, `ClassGenerationHelpers`)
- **FR-007**: Extract complex TopLevelGenerator transformations into `FieldTransformationHelpers` with >95% test coverage
- **SC-003**: Reduce duplicated code by ≥80% (1,400 lines → <300 lines)
- **SC-004**: Reduce total generator code lines by ≥40%
- **T003-T012b**: Each helper gets comprehensive unit tests (>90% coverage; >95% for FieldTransformationHelpers)
- **Tasks**: Systematic helper extraction (T003-T012b) eliminates duplication and improves maintainability

**Evidence**: Code metrics tracked in benchmark artifacts demonstrate measurable quality improvement.

---

### 2. Test-First Development ✅

**Principle**: All code written test-first with tests passing before implementation.

**How This Feature Addresses It**:
- **T003, T005, T007, T009, T011, T012a**: Add failing unit tests for each helper BEFORE implementation
- **T004, T006, T008, T010, T012, T012b**: Implement helpers to satisfy tests (tests pass)
- **FR-008**: Comprehensive unit tests for every helper method (minimum 2 test scenarios per public method)
- **T002**: Test skeleton with intentionally failing placeholder test created before any implementation
- **Plan.md TDD Commit Convention**: Each helper PR must include separate commits: (1) failing tests, (2) implementation that makes tests pass

**Evidence**: Tasks explicitly enforce test-first workflow. Infrastructure tasks (T001-T002, T013, T028-T031) are exempt per Constitution Principle II (applies to production code, not scaffolding).

---

### 3. Integration Testing Over Mocks ✅

**Principle**: Tests validate against real Terminal.Gui v2 controls, not mocks.

**How This Feature Addresses It**:
- **T015**: Integration test `BaselineGenerationTests.cs` runs the generator for real baseline input and asserts exact match with `refactor/generated-baseline.cs`
- **T014**: CI job runs generator to produce baseline target and performs diff against real baseline file
- **FR-001**: Byte-for-byte parity required with real generated output (not mocked)
- **SC-001**: 100% match with real baseline file `refactor/generated-baseline.cs`
- **Zero-tolerance policy**: Any diff in real generated output is a bug

**Evidence**: Integration tests use actual generator execution, not mocks. Baseline diff validation runs in CI against real output.

---

### 4. Generated Code Quality ✅

**Principle**: Human-readable, properly formatted, compiles without warnings.

**How This Feature Addresses It**:
- **FR-001**: Generated output must be byte-for-byte identical to `refactor/generated-baseline.cs` (preserves formatting, readability, compilation)
- **T014**: CI baseline diff ensures no regressions in generated code quality
- **Zero-tolerance rebaseline policy**: Refactor does NOT change parser behavior or generation logic—only internal organization
- **SC-001**: 100% match with baseline ensures generated code quality is preserved

**Evidence**: Baseline parity validation enforced in CI. Human-readable output preserved exactly.

---

### 5. Performance & Efficiency ✅

**Principle**: Incremental generation validated; benchmarks established.

**How This Feature Addresses It**:
- **T013**: Capture benchmark baseline (N=5 runs, median performance, code metrics)
- **T016**: Capture benchmark after changes and compare against baseline
- **SC-006**: Mean generation time must not exceed baseline median by >10%; CI fails on regression
- **FR-009**: After each refactoring phase, run full test suite and baseline diff before merging
- **Benchmark methodology**: N=5 runs, median metric, code metrics (LOC analysis), CI-enforced regression threshold
- **Scripts**: `run-benchmarks.ps1` and `compare-benchmarks.ps1` provide automated performance validation

**Evidence**: Benchmark artifacts include performance metrics (median_ms) and code metrics. CI enforces <10% regression threshold.

---

### 6. User Experience Consistency ✅

**Principle**: Auto-generated XSD provides complete IntelliSense; consistent XTUI authoring experience.

**How This Feature Addresses It**:
- **Scope**: This refactor is internal code reorganization only—does NOT change XTUI parsing, schema, or user-facing APIs
- **FR-001**: Byte-for-byte parity ensures user-facing generated code is identical
- **Zero-tolerance policy**: No changes to parser behavior, attributes, schema, or generation logic
- **Public API contract**: FR-008 clarification—internal implementation MAY use Roslyn `Syntax` nodes, but public-facing generator APIs MUST return string source only

**Evidence**: User experience unchanged. Refactor is pure internal restructuring.

---

### 7. Modularity & Separation ✅

**Principle**: Clear boundaries between XtuiLoader, GeneratorFactory, individual generators, helpers, mappers.

**How This Feature Addresses It**:
- **FR-002-FR-007**: Create clear separation with dedicated helper modules (`SyntaxHelpers`, `TypeNameHelpers`, `NamespaceHelpers`, `ChildProcessingHelpers`, `ClassGenerationHelpers`, `FieldTransformationHelpers`)
- **FR-006**: Introduce `BaseControlGenerator` and `BaseContainerGenerator` to enforce template method pattern and separate concerns
- **Deliverables**: Organized `Generators/Helpers/` directory with focused, single-responsibility helpers
- **T024/T025**: Stacked PRs enforce incremental, reviewable changes with clear boundaries
- **SC-003/SC-004**: Duplication reduction and code size reduction improve modularity

**Evidence**: Clear directory structure (`Generators/Helpers/`), focused helper APIs, base class abstractions.

---

## Governance & CI Enforcement

**T028**: GitHub Actions workflows (`ci-tests.yml`, `benchmarks.yml`) enforce constitution compliance:
- Unit tests must pass (Principle 2: Test-First)
- Baseline diff must match exactly (Principles 3, 4: Integration Testing, Generated Code Quality)
- Performance regression <10% (Principle 5: Performance & Efficiency)
- Code metrics tracked (Principle 1: Code Quality & Maintainability)

**T030**: This file (`CONSTITUTION_CHECK.md`) documents compliance and will be verified in CI.

**T031**: Unit test enforces public generator APIs return `string` type (Principle 6: User Experience Consistency—public contracts preserved).

---

## Summary

This refactoring feature fully adheres to all 7 Terminal.Gui.Xtui Constitution principles through:

1. **Systematic code quality improvement** (duplication reduction, helper extraction)
2. **Strict test-first workflow** (T003-T012b)
3. **Real integration testing** (T014, T015, baseline diff)
4. **Generated code quality preservation** (byte-for-byte parity, zero-tolerance policy)
5. **Performance monitoring** (T013, T016, benchmark artifacts, CI regression gates)
6. **User experience unchanged** (internal refactor only, public API contracts preserved)
7. **Clear modular architecture** (dedicated helpers, base classes, organized structure)

All principles are enforced via CI gates, task dependencies, and acceptance criteria.
