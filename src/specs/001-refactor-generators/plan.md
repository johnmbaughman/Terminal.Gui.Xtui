```markdown
# Implementation Plan: Refactor Generators — Phase 1 (Helpers)

**Feature Branch**: `001-refactor-generators`
**Created**: 2025-12-15
**Phase**: 1 — Foundation (Helpers extraction)

## Goal

Extract common Roslyn syntax construction and generator helper logic into a small, well-tested `Generators/Helpers/` library so subsequent generator refactors are low-risk and incremental. Preserve byte-for-byte parity for the authoritative baseline (`refactor/generated-baseline.cs`) after each change.

## Deliverables (Phase 1)

- `src/Generators/Helpers/SyntaxHelpers.cs`
- `src/Generators/Helpers/TypeNameHelpers.cs`
- `src/Generators/Helpers/NamespaceHelpers.cs`
- `src/Generators/Helpers/ChildProcessingHelpers.cs`
- `src/Generators/Helpers/ClassGenerationHelpers.cs`
- Unit tests under `Tests/Generators.Helpers.Tests/` (one test class per helper)
- A CI job step to run helper unit tests and run baseline generation diff

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

```