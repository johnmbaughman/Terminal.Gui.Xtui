# Terminal.Gui.Xaml – AI Agent Implementation Spec (Code-Only)

This specification distills the repository’s code and project structure (ignoring docs content) into clear objectives, contracts, and workflows that an AI coding agent can use to implement features safely and consistently.

Note: Focus on source, tests, samples, targets, scripts, and project files. Ignore docs/ and docs/_site/ when interpreting requirements.

## 1. Mission and Scope

Build a .NET 8 library that enables XAML-driven development for Terminal.Gui v2+, including:
- A composable runtime model (parsing, generation, binding, validation)
- Optional MSBuild integration to generate and validate documentation (opt-in)
- A small set of runnable samples demonstrating usage
- A comprehensive automated test suite and basic performance benchmarks

Non-goals:
- Shipping production DocFX integrations (current MSBuild task is a simple/controlled facade)
- Heavy third-party dependencies beyond Terminal.Gui, Roslyn, and .NET SDK

## 2. Repository Topology (Code Only)

- `src/Terminal.Gui.Xaml/`
  - Build integration: `Build/Terminal.Gui.Xaml.targets`, `Build/Terminal.Gui.Xaml.Documentation.targets`, `Build/GenerateDocumentationTask.cs`, `Build/BuildIntegrationService.cs`, `Build/IBuildIntegrationService.cs`
  - Domain namespaces (per commit inventory):
    - `Terminal.Gui.Xaml.Binding.*`
    - `Terminal.Gui.Xaml.Build.*`
    - `Terminal.Gui.Xaml.Documentation.*` (Logging, Services)
    - `Terminal.Gui.Xaml.Generation.*`
    - `Terminal.Gui.Xaml.Logging.*`
    - `Terminal.Gui.Xaml.Model.*`
    - `Terminal.Gui.Xaml.Parsing.*`
    - `Terminal.Gui.Xaml.Performance.*`
    - `Terminal.Gui.Xaml.Runtime.*`
    - `Terminal.Gui.Xaml.Validation.*`
  - Project: `src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj`
    - TargetFramework: net8.0
    - Warnings as errors, analyzers enabled
    - NuGet: `Terminal.Gui` 2.x (pre-release), Roslyn packages
    - Packs buildTransitive targets
    - Defaults: `<EnableXamlDocumentation>false</EnableXamlDocumentation>`, `<SkipDocumentationTasks>true` when not explicitly enabled

- `tests/`
  - `Terminal.Gui.Xaml.Tests/` (xUnit)
    - Unit + integration tests around build and documentation surfaces, validation, configuration
    - Has `ProjectReference` to main library
    - Ensures `SkipDocumentationTasks=true` so tests don’t load MSBuild task unless explicitly set
  - `Terminal.Gui.Xaml.Benchmarks/`
    - Benchmarks for documentation generation

- `samples/`
  - Sample apps: `DataBindingSample`, `DialogSample`, `FormValidationSample`, `LayoutSample`, `MultiWindowSample`, `ThemingSample`
  - Shared props/targets: `samples/Directory.Build.props`, `samples/Directory.Build.targets`
    - Disable documentation tasks in samples by default
  - Each sample is `net8.0` and references the main library and Terminal.Gui

- Root build config
  - `Directory.Build.props`, `Directory.Build.targets`
  - `.editorconfig`, `stylecop.json` → code style and analyzers
  - Scripts under `scripts/` for CI/local workflows

## 3. Build and MSBuild Contracts

- The library publishes two build-transitive targets files:
  - `Terminal.Gui.Xaml.targets` → wrapper import
  - `Terminal.Gui.Xaml.Documentation.targets` → defines UsingTask and targets
- Key properties:
  - `EnableXamlDocumentation` (bool): opt-in switch; default false
  - `SkipDocumentationTasks` (bool): default true unless explicitly enabled
  - `XamlDocumentationOutputPath` (string): default to documentation output path
  - `XamlDocumentationBuildMode` (string): Incremental|Full
- Targets:
  - `XamlGenerateDocumentation` (AfterTargets=Build) when enabled
  - `XamlValidateDocumentation` (manual)
  - `XamlCleanDocumentation` (AfterTargets=Clean)
- Build expectations:
  - Normal builds must succeed without documentation tasks running
  - Enabling documentation tasks should not impact consumers unless opted-in

## 4. Functional Areas and Responsibilities

- Binding (`Terminal.Gui.Xaml.Binding.*`)
  - Provide binding mode definitions and a simple binding engine capable of tracking property changes and updating TUI models accordingly.
  - Contracts: subscribe/unsubscribe patterns, error handling via `BindingException`.

- Parsing and Generation (`Terminal.Gui.Xaml.Parsing.*`, `Terminal.Gui.Xaml.Generation.*`)
  - Tokenize and parse XAML into intermediate models (`Model.*`), then generate code or runtime structures.
  - Contracts: deterministic parsing results, well-structured error reporting (`XamlParsingError`, severity), generation idempotency.

- Runtime (`Terminal.Gui.Xaml.Runtime.*`)
  - Map model/templates to concrete Terminal.Gui views (factory/loader abstractions) at runtime.

- Validation (`Terminal.Gui.Xaml.Validation.*`)
  - Validate models and configurations; return structured `ValidationResult` objects and severities.

- Documentation (facade) (`Terminal.Gui.Xaml.Build.*`, `Terminal.Gui.Xaml.Documentation.*`)
  - `GenerateDocumentationTask` and `BuildIntegrationService` simulate documentation build flows (opt-in), logging through `Documentation.Logging.*`.

- Logging (`Terminal.Gui.Xaml.Logging.*`)
  - Provide a minimal API for domain logging with adapters for Microsoft.Extensions.Logging.

- Performance (`Terminal.Gui.Xaml.Performance.*`)
  - Counters/monitors supporting profiling and basic telemetry of critical paths.

## 5. Public API – Quick Contracts (Agent-Facing)

- `Terminal.Gui.Xaml.Build.GenerateDocumentationTask`
  - Inputs (MSBuild properties): `DocumentationEnabled` (bool), `DocumentationOutputPath` (string), `DocumentationBuildMode` (string)
  - Execute() → bool success
  - Error Modes: Missing configuration, IO errors; should not crash the build when skipped.

- `Terminal.Gui.Xaml.Documentation.Services.*`
  - `IDocumentationGeneratorService` / `DocumentationConfigurationService`
  - Generate/Validate methods return typed results with issues and severities.

- `Terminal.Gui.Xaml.Binding.IDataBindingEngine`
  - Bind(source, target, mode), Unbind, Track changes; return binding handles or results with diagnostics.

- `Terminal.Gui.Xaml.Runtime.ControlFactory`
  - Create/mutate Terminal.Gui controls from model descriptors; extensible for custom controls.

Note: Names above reflect namespace inventory; verify exact method signatures when implementing changes.

## 6. Engineering Standards and Constraints

- Language/Framework: C# + .NET 8
- Treat warnings as errors; analyzers enabled; StyleCop file `stylecop.json`
- Minimize third-party libs; prefer .NET APIs and Terminal.Gui
- Tests: xUnit; ensure determinism, avoid long-running or flaky tests
- Performance: Avoid heavy allocations and unnecessary reflection in hot paths
- Packaging: Include buildTransitive targets; do not force optional tasks on consumers

## 7. Quality Gates (Agent Must Enforce)

- Build: `dotnet build` succeeds across solution
- Tests: `dotnet test` passes locally; doc tasks remain skipped during tests unless explicitly enabled
- Static Analysis: No analyzer violations; no new warnings
- Samples: Each sample builds successfully; runnable where feasible
- MSBuild: Opt-in documentation targets do not break default builds

## 8. Common Tasks for the Agent (with Accept/Done Criteria)

1) Add a new control mapping in the runtime factory
- Steps: extend `Runtime.ControlFactory`, update model mapping, add unit tests
- Done: New control type can be instantiated from model; tests cover happy path + invalid config

2) Extend the XAML parser with a new attribute
- Steps: Update tokenizer and parser to recognize attribute; map to model; update generation/runtime if needed; add tests
- Done: Valid XAML compiles to model with attribute; invalid syntax yields a structured error

3) Improve binding engine to support OneTime mode
- Steps: Update `BindingMode` enum and engine; add tests; document behavior via XML docs
- Done: OneTime bindings set values once; no ongoing subscriptions; unit tests pass

4) Add a new MSBuild property controlling documentation behavior
- Steps: Update targets to honor the property; add unit tests for config reading; ensure default remains off
- Done: Property is recognized and alters behavior only when set; no impact on default builds

5) Introduce a performance measurement around parsing
- Steps: Add counters/timing; add a benchmark; ensure opt-in or negligible overhead
- Done: Benchmarks run and produce output; no analyzer regressions

## 9. Edge Cases to Consider

- Terminal.Gui version differences (v2 pre-release namespaces may evolve)
- Partial or invalid XAML documents → graceful errors, not crashes
- Concurrent parsing/generation (thread-safety where applicable)
- File system errors when generating outputs (permissions/locked files)
- Consumers without opting into documentation tasks

## 10. Prompts You Can Use (Agent Templates)

- Implement a new control mapping
  - “Add support for <ControlName> in `Terminal.Gui.Xaml.Runtime.ControlFactory`. Update model mapping and add unit tests in `tests/Terminal.Gui.Xaml.Tests/Unit/` covering creation and invalid configuration. Ensure code follows StyleCop rules.”

- Extend XAML parser
  - “Teach `Terminal.Gui.Xaml.Parsing.XamlTokenizer` and `XamlParser` to recognize `<NewAttribute>`. Update the model in `Terminal.Gui.Xaml.Model.*`; add tests for valid/invalid cases.”

- Add MSBuild property
  - “Introduce `<MyNewProperty>` respected by `Terminal.Gui.Xaml.Documentation.targets`. Include unit tests ensuring default is off and behavior toggles only when property is set.”

- Improve binding engine
  - “Add OneTime binding mode to `Terminal.Gui.Xaml.Binding.BindingMode` and support it in `SimpleDataBindingEngine`. Add tests for initial set and no subsequent updates.”

- Performance instrumentation
  - “Add timing/counters for `XamlParser.Parse` and a small BenchmarkDotNet test in `tests/Terminal.Gui.Xaml.Benchmarks` to track regression.”

## 11. How to Validate (Agent Checklist)

- Build the solution in Release
- Run unit tests with coverage-sensitive suites
- Verify analyzers (no warnings)
- Build samples
- Optional: Run benchmarks (time-boxed)

## 12. Known Risks / Open Items

- Terminal.Gui 2.x API surface is evolving; namespaces and types may change. Keep samples aligned with the actual referenced version (e.g., `Terminal.Gui.App`, `Terminal.Gui.ViewBase`).
- Documentation MSBuild task is intentionally minimal; it should remain opt-in and fail-safe.

## 13. Contribution Rules for the Agent

- Do not modify documentation content when implementing code tasks (unless the task explicitly involves docs)
- Maintain public API stability; changes to public types require unit tests
- Keep targets opt-in; never enable documentation tasks by default for consumers
- Follow repository code style and analyzers; keep builds warning-free

---

This spec is intended to be copy-pasteable as context for an AI coding agent, providing enough structure to implement features and maintain quality without referencing the docs content. 