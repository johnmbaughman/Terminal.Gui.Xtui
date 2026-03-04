<!--
=============================================================================
SYNC IMPACT REPORT
=============================================================================
Version change  : 1.0.0 → 1.1.0
Bump rationale  : MINOR — Added five new governance sections and one new
                  principle (VIII) covering gaps identified against the
                  Windshield reference constitution: commit workflow, agent
                  commit-message rules, PR description format, defensive
                  error handling, structured logging, living documentation,
                  external-only mocking policy, phase gates, target-
                  framework lock, and secrets/null-safety constraints.
                  All additions are project-agnostic; no project-specific
                  content from the reference constitution was included.

Modified principles :
  - I  (Code Quality): added secrets/credential prohibition and null-
       safety requirement.
  - VII (Modularity): minor wording only (no semantic change).

Added sections :
  - Core Principle VIII  : Living Documentation (NON-NEGOTIABLE)
  - Core Principle IX    : Defensive Error Handling and Typed Exceptions
  - Core Principle X     : Structured Logging
  - Core Principle XI    : External-Only Mocking
  - Additional Constraints: commit discipline, agent commit-message
       workflow, PR description format, target-framework lock.
  - Development Workflow : phase gates.

Removed sections    : N/A

Templates reviewed  :
  ✅ .specify/templates/plan-template.md   — Constitution Check count
       updated to reflect 11 principles.
  ✅ .specify/templates/spec-template.md   — no change required.
  ✅ .specify/templates/tasks-template.md  — no change required.
  ✅ .specify/templates/checklist-template.md — no change required.
  ✅ .specify/templates/agent-file-template.md — no change required.

Follow-up TODOs :
  — Review existing specs/*/tasks.md for agent commit-message workflow
    compliance.
  — Add mocking framework package reference to NuGet.Config if not
    already recorded.
=============================================================================
-->

# Terminal.Gui.Xtui Constitution

## Core Principles

### I. Code Quality & Maintainability

**Generator Code MUST meet these standards:**

- Roslyn incremental generators MUST use proper caching and change detection
- All generator logic MUST be pure functions accepting `ElementNode` and returning code strings for public-facing APIs
- INTERNAL IMPLEMENTATION EXCEPTION: Generator implementations are permitted to use Roslyn `Syntax` node types internally for performance, correctness, and to avoid repeated string parsing. Any use of `Syntax` nodes must be encapsulated and must not be exposed through public APIs: public generator outputs (what other projects consume) MUST be plain string source code. See Amendment 1 below for rationale and governance.
- Magic strings MUST be eliminated—use constants, `nameof`, or reflection where appropriate
- Code generation MUST produce properly indented, formatted C# following project conventions
- Complex expression parsing (Pos/Dim) MUST be isolated in dedicated helper classes
- Generator classes MUST have single responsibility—one generator per control type or logical grouping
- Secrets, credentials, API keys, and environment-specific paths MUST NOT be embedded in source code
- `Nullable=enable` null-safety MUST be enforced across all projects; nullable reference type warnings
  MUST be treated as errors and resolved rather than suppressed with `!` assertions

**Rationale:** Source generators are infrastructure code that produces production code. Poor generator quality
multiplies technical debt across every consuming project. Maintainability is critical because generator bugs
are discovered late and impact all users.

### II. Test-First Development (NON-NEGOTIABLE)

**TDD process is mandatory for all features:**

- Tests MUST be written before implementation code
- Tests MUST fail initially, demonstrating the missing functionality
- Implementation MUST make tests pass without modifying test expectations
- Red-Green-Refactor cycle strictly enforced: failing test → minimal implementation → refactor
- No feature is complete without corresponding test coverage
- Tests MUST be independently runnable and not depend on execution order

**Rationale:** Source generators are notoriously difficult to debug. Test-first development catches issues early, serves as executable specifications, and prevents regression. The incremental nature of Roslyn generators makes untested changes extremely risky.

### III. Integration Testing Over Mocks

**Real Terminal.Gui integration is required:**

- Generated code MUST be tested against actual Terminal.Gui v2 controls, not mocks
- Tests MUST verify that generated `InitializeComponent()` methods produce working UI trees
- Expression parsing (Pos/Dim) MUST be validated against Terminal.Gui's actual API
- New control generators MUST include end-to-end tests loading XTUI and running generated code
- Mock only external dependencies (file I/O, diagnostics), never Terminal.Gui itself
- Integration tests MUST catch breaking changes in Terminal.Gui API surface

**Rationale:** This project's value proposition is generating code that works with Terminal.Gui. Mocks create false confidence. Integration tests verify the actual contract and catch upstream breaking changes immediately.

### IV. Generated Code Quality

**Output code MUST meet production standards:**

- Generated C# MUST compile without warnings on latest C# language version
- Generated code MUST follow naming conventions (`camelCase` locals, `PascalCase` types)
- Generated code MUST be human-readable with proper indentation and spacing
- Generated code MUST include XML doc comments for public partial classes
- Generated code MUST avoid allocations in hot paths where possible
- Generated code MUST use modern C# patterns (e.g., collection expressions, pattern matching where beneficial)

**Rationale:** Developers debug generated code. Poor quality output damages trust and makes troubleshooting impossible. Generated code appears in IDE navigation and must represent the project professionally.

### V. Performance & Efficiency

**Incremental generation and benchmarking required:**

- Roslyn generators MUST use incremental generation pipeline APIs (`IncrementalGeneratorInitializationContext`)
- Changes to one `.xtui` file MUST NOT trigger regeneration of unrelated files
- Generator execution time MUST be benchmarked using `Terminal.Gui.Xtui.Benchmarks` project
- Benchmarks MUST be run before and after performance-related changes
- Regression in generator performance (>10% slower) requires explicit justification
- XSD generation MUST complete in <5 seconds for Terminal.Gui's 51 controls

**Rationale:** Generators run on every keystroke in IDE. Slow generators destroy developer experience. Incremental generation is mandatory for scale. Benchmarks prevent performance regression and validate optimizations.

### VI. User Experience Consistency

**IntelliSense and declarative patterns:**

- XSD schema MUST provide autocomplete for all Terminal.Gui public properties
- XSD MUST be auto-generated from Terminal.Gui metadata, never hand-written
- Breaking changes in generated API (InitializeComponent signature, field names) require MAJOR version bump
- Error messages from generator diagnostics MUST be actionable and include line/column info
- XTUI syntax MUST follow XAML conventions for familiarity (PascalCase elements, attributes for properties)
- Documentation MUST include examples for every supported control generator

**Rationale:** Developers expect XAML-like declarative UI. Inconsistent IntelliSense or error messages break flow. Auto-generated XSD ensures schema stays in sync with Terminal.Gui. Familiar patterns reduce learning curve.

### VII. Modularity & Separation of Concerns

**Generator architecture must enforce boundaries:**

- `XtuiLoader` MUST handle only XML parsing into `ElementNode` tree, no code generation
- `GeneratorFactory` MUST be the single entry point for selecting control-specific generators
- Each generator (Window, Label, Button, etc.) MUST be isolated in its own file
- `ObjectParsingHelpers` MUST handle Pos/Dim expression parsing independently
- `EnumMapper` MUST centralize all enum string-to-value conversions
- No generator MUST directly reference another generator's implementation details

**Rationale:** Clear boundaries enable parallel development, simplify testing, and make the codebase approachable. Coupling between generators creates fragility. Separation allows generators to evolve independently and supports future extensibility.

### VIII. Living Documentation (NON-NEGOTIABLE)

Documentation MUST be kept current at all times alongside the code it describes:

- **`README.md`** MUST contain at minimum: the project name and purpose, prerequisites (.NET version,
  supported Terminal.Gui version), build instructions, quick-start walkthrough, and a description of
  the XTUI syntax.
- Public APIs, XTUI syntax elements, and XSD schema MUST be documented before a feature is considered
  complete. Undocumented public surface is an automatic quality-gate failure.
- XML documentation comments (`<summary>`, `<param>`, `<returns>`, and `<exception>`) MUST be present
  on all public and protected members in `Terminal.Gui.Xtui` and `Terminal.Gui.Xtui.Generator`.
- Documentation MUST accurately reflect shipped behavior only — aspirational or speculative content
  is prohibited.
- Both `README.md` and inline XML docs MUST be updated as part of any change that adds, modifies, or
  removes a user-visible capability. A feature MUST NOT be closed while documentation is out of date.
- All documentation files MUST be UTF-8 encoded without BOM.

**Rationale:** A source-generator library is only useful if developers understand the XTUI syntax and the
generated API. Late or inaccurate documentation causes misuse, breaks consumer trust, and undermines
the project's core value proposition. Documentation is a non-negotiable deliverable.

### IX. Defensive Error Handling and Typed Exceptions

All errors that can reasonably arise at runtime MUST be handled explicitly:

- Generator diagnostics MUST be surfaced as Roslyn `Diagnostic` entries with actionable descriptions,
  severity levels, and file/line/column locations. Generic or swallowed errors are prohibited.
- Typed exception classes MUST be used for domain-specific failure modes (e.g., unsupported element
  types, malformed XTUI, version mismatches) so callers can handle them precisely without catching
  `Exception`.
- All caught exceptions MUST be re-thrown, wrapped in a typed exception, or converted to a Diagnostic.
  Silent catch-and-ignore blocks are prohibited.
- External inputs (XTUI file content, XML attributes, user-supplied values) MUST be validated before
  use; the system MUST fail with a descriptive error rather than silently produce incorrect output.

**Rationale:** Compiler-integrated tools are held to a higher standard of error communication than
run-time libraries. A generator that emits silent or cryptic failures forces developers to debug
generated code rather than the source of truth.

### X. Structured Logging

Where runtime logging is applicable (e.g., Xtui.Mvvm runtime helpers, CLI tooling):

- Logging MUST use `Microsoft.Extensions.Logging` abstractions with an injected provider.
- Log entries MUST include severity level, component name, and contextual properties.
- The logging infrastructure MUST be injectable and substitutable in tests; static singletons MUST NOT
  be called directly from library code.
- If a component has no runtime logging need (pure generator), this principle is satisfied by marking
  it N/A in the Constitution Check.

**Rationale:** Consistent, injectable logging ensures diagnostics are observable and testable without
coupling components to a specific provider.

### XI. External-Only Mocking

`NSubstitute` is the sole permitted mocking/substitution framework in this solution. All other mocking
libraries (e.g., Moq, FakeItEasy) are prohibited and MUST NOT be added as dependencies.

`NSubstitute` MUST NOT be used to substitute any type defined within the `Terminal.Gui.Xtui.*`
projects. This covers all generator classes, helpers, mappers, loaders, domain models, and any other
project-internal type.

Only genuinely external dependencies — such as file-system abstractions, Roslyn compilation APIs
when used as test stubs, or third-party library contracts — are permitted to be replaced with
`NSubstitute` doubles in the test suite.

When an internal component must be substituted to enable testing, a purpose-built in-process stub or
fake MUST be hand-authored and placed in the Tests project. It MUST be a concrete class implementing
the same interface or inheriting the same abstract base, with test-specific behaviour wired explicitly.
`NSubstitute` MUST NOT be used to create these internal substitutes.

**Rationale:** Mocking internal project objects produces tests that verify implementation wiring rather
than observable behaviour. Such tests are brittle under refactoring and give false confidence.
Restricting mocks to the external boundary forces clean interface design and keeps the generator
testable in isolation. A single named framework eliminates ambiguity and keeps the dependency graph
minimal.

### Testing Requirements

- **Unit Tests**: All generator classes, parsers, and mappers MUST have dedicated test classes with >90% code coverage
- **Integration Tests**: Every control type MUST have end-to-end tests (XTUI → Generated Code → Runtime execution)
- **Roslyn Tests**: Generator infrastructure MUST have incremental generation tests using Roslyn testing APIs
- **Test Naming**: Test methods MUST follow pattern `MethodName_Scenario_ExpectedOutcome`
  (e.g., `ButtonGenerator_WithClickHandler_GeneratesEventSubscription`)
- **Test Organization**: Tests MUST be grouped by feature/control type in dedicated test classes

### Code Review Gates

- All PRs MUST pass the automated test suite (no failing tests permitted at merge time)
- All PRs MUST include tests for new functionality or bug fixes
- All PRs MUST NOT introduce compiler warnings or analyzer violations
- All PRs MUST update relevant documentation (README.md, XML docs, examples)
- Performance-impacting changes MUST include benchmark results
- All PRs MUST include the following sections in their description:
  **Problem**, **Approach**, **Risks**, **Tests**, and **Rollout/Rollback** notes

### Documentation Expectations

- Public APIs MUST have XML documentation comments
- Complex algorithms (expression parsing, Pos/Dim resolution) MUST have inline comments explaining logic
- README MUST be updated when adding new control support or changing XTUI syntax
- Examples folder MUST demonstrate new features within one release cycle

## Additional Constraints

- Agent scripts and automation for this repository SHOULD use PowerShell when a script is required.
- Commit messages MUST be imperative, scoped, and concise following Conventional Commits format
  (e.g., `feat: add ButtonGenerator click-handler binding`, `fix: handle null Pos expression in parser`).
- All git commits and pushes that record repository history MUST be performed by a human. Agents are
  permitted to prepare patches and propose changes, but MUST NOT execute commits or pushes. Human
  reviewers MUST apply, review, sign, and push commits.
- The solution target framework is `net8.0`; do not introduce `net9.0` or later targets without a
  documented migration rationale recorded in the relevant spec or plan artifact.

### Agent Commit-Message Workflow (NON-NEGOTIABLE)

- When an agent prepares or applies changes, the agent MUST also generate a clearly identified,
  human-consumable commit message that the developer will copy/paste when performing the actual commit.
  The commit message MUST succinctly state the intent, files changed, and the governance version
  (e.g., `docs: amend constitution to v1.1.0`).
- If the agent did not perform the commit, the agent-generated commit message is considered a "rolling
  change" note. Subsequent agent changes relating to the same logical work MAY append new rolling-change
  messages; agents MUST NOT replace or overwrite previous rolling-change notes without human approval.
- Agents MAY prepare additional patches and commit messages to continue work without forcing an
  immediate commit, but each patch MUST include a clear commit message and reference to the prior
  rolling-change identifier when continuing the same logical change.

## Development Workflow

- Prefer small, incremental changes with clear commit boundaries over large feature branches.
- New source files MUST be placed in the project tier that owns their concern: generator and mapper
  logic in `Terminal.Gui.Xtui.Generator`, runtime/binding helpers in `Terminal.Gui.Xtui`, tests in
  the appropriate Tests project.
- Add or update xUnit tests for any changed logic; run `dotnet build` and `dotnet test` locally
  before preparing changes for review.

### Phase Gates

Before development proceeds to the next delivery phase, ALL of the following MUST be satisfied:

1. ALL xUnit tests covering that phase's scope MUST pass with zero failures. No phase transition is
   permitted while any failing test exists. Failures MUST be resolved and the full suite re-run before
   the next phase begins.
2. A user integration test MUST be performed: the developer MUST manually walk through the
   **Independent Test** scenario defined in `spec.md` for each user story delivered in that phase
   (or the observable build/generation behaviour for infrastructure-only phases). The outcome MUST be
   confirmed as passing before the phase is accepted and the next phase begins.

A phase is not complete until BOTH criteria are met and recorded in the phase-gate task of `tasks.md`.

## Quality Assurance

### Automated Validation

- **Build Integration**: XSD regeneration runs before every build to ensure schema freshness
- **Analyzer Rules**: `EnforceExtendedAnalyzerRules=true` for generator project enforces Roslyn best practices
- **Nullability**: `Nullable=enable` enforces null-safety across codebase
- **Incremental Testing**: `Terminal.Gui.Xtui.RoslynTests` validates generator caching behavior

### Performance Benchmarks

Maintained in `Terminal.Gui.Xtui.Benchmarks`:

- `GeneratorBenchmarks`: Measures end-to-end XTUI parsing and code generation time
- `ExpressionParsingBenchmarks`: Measures Pos/Dim expression parsing performance
- `XtuiLoaderBenchmarks`: Measures XML parsing into ElementNode tree
- Benchmarks MUST be run on representative real-world XTUI files (e.g., UICatalog, ExampleLogin)

### Release Quality Gates

Before any release:

1. All tests passing (unit, integration, Roslyn tests)
2. No compiler warnings
3. Benchmark suite executed, no regressions
4. Examples run successfully against build artifacts
5. XSD schema validated against Terminal.Gui current version

## Governance

### Amendment Process

- Constitution changes require: a documented rationale, a short migration plan for any affected
  in-flight work, and a PR that explicitly references this constitution change.
- The project follows semantic versioning for governance:
  - **MAJOR**: Removing or redefining a Core Principle in a backward-incompatible way
  - **MINOR**: Adding a new principle or section, or materially expanding existing guidance
  - **PATCH**: Clarifications, wording, typo fixes, non-semantic refinements
- All amendments MUST include a Sync Impact Report (as an HTML comment at the top of this file)
  documenting template changes, modified/added/removed sections, and any deferred TODOs.
- Every completed feature MUST include a "Constitution Check" in its `tasks.md` or
  `CONSTITUTION_CHECK.md` confirming that all eleven Core Principles are satisfied (or explicitly
  marked N/A with justification where inapplicable).

### Amendment 1 (2025-12-15)

Rationale: To allow practical, high-performance generator implementation while preserving the
constitution's original intent of public string outputs, we permit internal use of Roslyn `Syntax`
node APIs with constraints.

Rules:
- Internal generator code MAY manipulate and construct `Syntax` nodes to build code.
- Public-facing generator contracts and produced artifacts MUST be strings (e.g., the final
  `GenerateClass` return value or files written to disk).
- Any `Syntax` usage MUST be isolated behind helper modules and covered by unit tests demonstrating
  that the final string output is identical to expected baselines.
- This amendment follows the project's amendment process and is recorded here for traceability.

### Compliance Verification

- All feature planning MUST include a Constitution Check section verifying adherence to all eleven
  Core Principles before Phase 0 research begins, and re-checked after Phase 1 design.
- Code reviews MUST explicitly verify test-first workflow was followed.
- Generated code quality MUST be spot-checked in PR reviews.
- Integration test failures MUST block merges.
- Principle violations MUST be explicitly documented in PR descriptions with rationale.
- Temporary exceptions require an issue tracking the technical debt and a remediation plan.
- Repeated exceptions to the same principle trigger a constitution amendment discussion.

**Version**: 1.1.0 | **Ratified**: 2025-12-13 | **Last Amended**: 2026-03-03
