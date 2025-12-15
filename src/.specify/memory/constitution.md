<!--
Sync Impact Report - Version 1.0.0

Version Change: INITIAL → 1.0.0
Rationale: Initial constitution establishing foundational governance for Terminal.Gui.Xtui project

Principles Established:
  1. Code Quality & Maintainability - Roslyn generator code standards
  2. Test-First Development (NON-NEGOTIABLE) - TDD mandatory for all features
  3. Integration Testing Over Mocks - Real Terminal.Gui integration required
  4. Generated Code Quality - Output code must meet production standards
  5. Performance & Efficiency - Incremental generation, benchmarking required
  6. User Experience Consistency - Template adherence and IntelliSense completeness
  7. Modularity & Separation of Concerns - Generator/mapper/loader isolation

Templates Updated:
  ✅ plan-template.md - Constitution Check section aligns with 7 principles
  ✅ spec-template.md - User scenarios support independent testing requirement
  ✅ tasks-template.md - Test-first workflow enforced in task structure

Follow-up TODOs: None - all placeholders resolved

Date: 2025-12-13
-->

# Terminal.Gui.Xtui Constitution

## Core Principles

### I. Code Quality & Maintainability

**Generator Code MUST meet these standards:**

- Roslyn incremental generators MUST use proper caching and change detection
 - All generator logic MUST be pure functions accepting `ElementNode` and returning code strings for public-facing APIs
 - INTERNAL IMPLEMENTATION EXCEPTION: Generator implementations are permitted to use Roslyn `Syntax` node types internally for performance, correctness, and to avoid repeated string parsing. Any use of `Syntax` nodes must be encapsulated and must not be exposed through public APIs: public generator outputs (what other projects consume) MUST be plain string source code. See Amendment 1 below for rationale and governance.
- Magic strings MUST be eliminated—use constants, nameof, or reflection where appropriate
- Code generation MUST produce properly indented, formatted C# following project conventions
- Complex expression parsing (Pos/Dim) MUST be isolated in dedicated helper classes
- Generator classes MUST have single responsibility—one generator per control type or logical grouping

**Rationale:** Source generators are infrastructure code that produces production code. Poor generator quality multiplies technical debt across every consuming project. Maintainability is critical because generator bugs are discovered late and impact all users.

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

## Development Standards

### Testing Requirements

- **Unit Tests**: All generator classes, parsers, and mappers MUST have dedicated test classes with >90% code coverage
- **Integration Tests**: Every control type MUST have end-to-end tests (XTUI → Generated Code → Runtime execution)
- **Roslyn Tests**: Generator infrastructure MUST have incremental generation tests using Roslyn testing APIs
- **Test Naming**: Test methods MUST follow pattern `MethodName_Scenario_ExpectedOutcome` (e.g., `ButtonGenerator_WithClickHandler_GeneratesEventSubscription`)
- **Test Organization**: Tests MUST be grouped by feature/control type in dedicated test classes (e.g., `ButtonGeneratorTests.cs`)

### Code Review Gates

- All PRs MUST pass automated test suite (142+ unit tests)
- All PRs MUST include tests for new functionality or bug fixes
- All PRs MUST not introduce compiler warnings or analyzer violations
- All PRs MUST update relevant documentation (README.md, XML docs, examples)
- Performance-impacting changes MUST include benchmark results

### Documentation Expectations

- Public APIs MUST have XML documentation comments
- Complex algorithms (expression parsing, Pos/Dim resolution) MUST have inline comments explaining logic
- README MUST be updated when adding new control support or changing XTUI syntax
- Examples folder MUST demonstrate new features within one release cycle

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

- Constitution changes require documented justification and impact analysis
- MAJOR version bump: Removing/changing core principles or breaking backward compatibility
- MINOR version bump: Adding new principles or expanding existing guidance
- PATCH version bump: Clarifications, typos, non-semantic improvements
- All amendments MUST include Sync Impact Report documenting template changes

### Amendment 1 (2025-12-15)

Rationale: To allow practical, high-performance generator implementation while preserving the constitution's original intent of public string outputs, we permit internal use of Roslyn `Syntax` node APIs with constraints.

Rules:
- Internal generator code MAY manipulate and construct `Syntax` nodes to build code. 
- Public-facing generator contracts and produced artifacts MUST be strings (e.g., the final `GenerateClass` return value or files written to disk).
- Any `Syntax` usage MUST be isolated behind helper modules and covered by unit tests demonstrating that the final string output is identical to expected baselines.
- This amendment follows the project's amendment process and is recorded here for traceability.

### Compliance Verification

- All feature planning MUST include Constitution Check section verifying principle adherence
- Code reviews MUST explicitly verify test-first workflow was followed
- Generated code quality MUST be spot-checked in PR reviews
- Integration test failures MUST block merges

### Exception Handling

- Principle violations MUST be explicitly documented in PR description with rationale
- Temporary exceptions require issue tracking technical debt and remediation plan
- Repeated exceptions to same principle trigger constitution amendment discussion

**Version**: 1.0.0 | **Ratified**: 2025-12-13 | **Last Amended**: 2025-12-13
