# Merge Guidelines: Feature 001-refactor-generators

**Purpose**: Document branch management, PR workflow, and quality gates for the generator refactoring feature  
**Created**: 2025-12-18  
**Feature**: `specs/001-refactor-generators/`

## Branching Strategy

### Branch Naming Convention

- **Feature root branch**: `001-refactor-generators`
- **Child branches**: `001-refactor-generators-<short-descriptive-name>`
  - Example: `001-refactor-generators-typename-helper`
  - Example: `001-refactor-generators-button-refactor`
  - Example: `001-refactor-generators-cross-cutting-docs`

### Stacked PR Workflow

This feature uses **stacked PRs** (iterative, small branches) to enable:
- Incremental review and feedback
- Isolated test coverage per helper/component
- Parallel development when tasks don't conflict
- Clear commit history with test-first evidence

**Workflow**:
1. Create child branch from current feature branch
2. Open PR targeting the parent branch (not main/master)
3. When parent branch receives updates, rebase or merge child branches
4. Document updates in PR comments with CI run links

## Pull Request Requirements

### TDD Commit Convention (REQUIRED)

Each PR implementing a helper or generator refactor **MUST** follow this commit pattern:

1. **First commit**: Add failing tests
   - Tests must fail initially
   - Include test file changes only
   - Commit message: "Add failing tests for [ComponentName]"

2. **Second commit**: Implementation that makes tests pass
   - Implementation code only
   - Commit message: "Implement [ComponentName] to satisfy tests"
   - All tests must pass

**Do NOT squash commits until review is complete.** Reviewers need to see the test-first workflow.

### Code Coverage Requirements

- **Helpers**: Target >90% code coverage per helper
- **FieldTransformationHelpers**: Target >95% coverage (T012a/T012b)
- CI must validate coverage thresholds before merge

### Required PR Content

Every PR must include:

- [ ] **Baseline Diff Check**: Include evidence that baseline diff was run
  - Link to CI run showing baseline comparison
  - OR local diff output in PR description
  - **CRITICAL**: Any baseline diff is a BUG (see Zero-Tolerance Policy below)

- [ ] **Benchmark Artifacts** (when relevant):
  - Link to CI benchmark artifacts
  - Note whether benchmarks were run locally or in CI
  - For implementation PRs: Must show <10% regression vs baseline median

- [ ] **Test Evidence**:
  - Separate commits showing failing then passing tests
  - Coverage report link or summary

- [ ] **CI Status**:
  - All CI checks must pass (tests, baseline diff, benchmarks)
  - Constitution compliance check must pass

## Zero-Tolerance Rebaseline Policy

**CRITICAL RULE**: This refactor does NOT change generator output—only internal code organization.

### Policy Statement

**Any PR showing a baseline diff is a BUG and must be fixed before merge.**

This is NOT a rebaseline scenario. The generator output must remain byte-for-byte identical to the baseline.

### Baseline Validation

Every PR must pass the baseline diff check:

```powershell
git --no-pager diff --no-index --ignore-cr-at-eol refactor\generated-baseline.cs path\to\generated\file.cs
```

**Expected result**: No differences (exit code 0)

**If diff appears**:
1. **DO NOT** update the baseline file
2. **DO NOT** merge the PR
3. Treat as a bug in the refactored code
4. Debug and fix the generator to produce identical output
5. Re-run tests and baseline diff

### Integration Test Enforcement

The integration test `BaselineGenerationTests.cs` validates exact baseline match:

```csharp
// Must assert exact match with refactor/generated-baseline.cs
Assert.Equal(expectedBaseline, actualGenerated);
```

This test must pass for all PRs.

## CI Requirements

### Required CI Checks

Every PR must pass these CI jobs before merge:

1. **Unit Tests** (`.github/workflows/ci-tests.yml`)
   - All helper tests pass
   - All generator tests pass
   - Coverage thresholds met

2. **Baseline Diff** (part of ci-tests.yml)
   - Baseline comparison shows no differences
   - Integration test passes

3. **Benchmarks** (`.github/workflows/benchmarks.yml`)
   - Benchmarks run successfully
   - No regression >10% vs baseline median
   - Artifacts uploaded to `artifacts/benchmarks/`

4. **Constitution Compliance**
   - `CONSTITUTION_CHECK.md` file exists
   - All 7 principles documented

### CI Artifact Locations

- **Benchmarks**: `artifacts/benchmarks/summary.json`
- **Generated files**: `artifacts/generated/`
- **Test results**: Available in CI logs

## Branch Update Protocol

When parent branches receive fixes or updates:

1. Update child branch via rebase or merge:
   ```powershell
   git checkout 001-refactor-generators-child-branch
   git rebase 001-refactor-generators  # or git merge
   ```

2. Add PR comment documenting the update:
   ```markdown
   Updated branch with latest changes from parent branch.
   - CI run: [link to workflow run]
   - All checks passing ✓
   ```

3. Verify CI passes after update

## Merge Checklist

Before merging any PR:

- [ ] TDD commit pattern followed (failing test → passing implementation)
- [ ] All CI checks pass (tests, baseline diff, benchmarks)
- [ ] Code coverage meets requirements (>90% for helpers, >95% for FieldTransformationHelpers)
- [ ] Baseline diff shows ZERO differences (zero-tolerance policy)
- [ ] Benchmark shows <10% regression (or improvement)
- [ ] PR includes benchmark artifact links
- [ ] PR includes baseline diff evidence
- [ ] Constitution compliance check passes
- [ ] Code review approved
- [ ] Branch is up-to-date with parent branch

## Parallel Development Guidelines

Tasks marked `[P]` in `tasks.md` can be developed in parallel when they:
- Touch different files
- Don't have cross-dependencies
- Are in the same phase (e.g., all helper implementations)

**Safe for parallel**:
- Different helper implementations (T004, T006, T008, T010, T012, T012b)
- Different generator refactors (after integration validation passes)

**Must be sequential**:
- Setup tasks before implementation (T001, T002, T013, T028-T030)
- Failing tests before implementation (T003→T004, T005→T006, etc.)
- Integration validation before generator refactors (T014-T016 before T017-T019)

## References

- **Tasks**: `specs/001-refactor-generators/tasks.md`
- **Plan**: `specs/001-refactor-generators/plan.md`
- **Requirements**: `specs/001-refactor-generators/checklists/requirements.md`
- **Constitution**: `specs/001-refactor-generators/CONSTITUTION_CHECK.md`
- **Baseline**: `refactor/generated-baseline.cs`
- **CI Workflows**: `.github/workflows/ci-tests.yml`, `.github/workflows/benchmarks.yml`

## Notes

- This document fulfills task T022
- Zero-tolerance rebaseline policy ensures pure internal refactoring
- Stacked PR workflow enables incremental, reviewable changes
- TDD commit convention provides audit trail of test-first development
