# Documentation Architecture (T041)

Status: Stable — initial architecture documentation.

This document explains how the documentation generation and validation subsystem is structured within the Terminal.Gui.Xaml repository. It focuses on the composable model + service design, build integration points, and performance / quality feedback loops.

## Goals

- Deterministic, test-driven documentation generation pipeline
- Clear separation between configuration (models), orchestration (services), and hosting/build layers (MSBuild targets & tasks)
- Extensibility for future real DocFX engine integration without breaking public contracts
- Performance + coverage visibility (benchmark history, regression detection, dashboards)

## High-Level Layers

| Layer | Purpose | Key Artifacts |
|-------|---------|---------------|
| Models | Strongly-typed configuration & result contracts | `DocFxConfiguration`, `TemplateConfiguration`, `BuildConfiguration`, `CoverageMetrics`, `DocumentationValidationResult`, `ValidationIssue` |
| Services | Orchestrate generation & validation flows | `IDocumentationGeneratorService`, `IDocumentationConfigurationService`, `IBuildIntegrationService` + corresponding implementations |
| Build Integration | Expose documentation actions to host builds | `GenerateDocumentationTask`, `Terminal.Gui.Xaml.Documentation.targets`, `Terminal.Gui.Xaml.targets` |
| Logging | Abstraction for structured + testable logging | `IDocumentationLogger`, `ConsoleDocumentationLogger`, `NullDocumentationLogger` |
| Performance & Quality | Detect regressions, surface metrics | BenchmarkDotNet benchmarks, performance tests, history CSV/JSON, dashboard HTML |

## Models Overview

All models reside under `src/Terminal.Gui.Xaml/Documentation/Models/` and are decorated with `[Required]` and other validation attributes to enable deterministic validation in unit tests. They intentionally mirror key DocFX concepts (metadata, build content, resources) while remaining minimal.

Validation flows use DataAnnotations + manual checks aggregated into `DocumentationValidationResult` which carries a collection of `ValidationIssue` items (with severity + type enums).

## Services

### Configuration Service
`DocumentationConfigurationService` validates and normalizes `DocFxConfiguration` instances (e.g., ensures paths, default values, invariants).

### Generator Service
`DocumentationGeneratorService` simulates generation (placeholder) and produces:
- Timing metrics
- Coverage metrics (placeholder deterministic values for tests)
- Validation summary when invoked in validation mode

### Build Integration Service
`BuildIntegrationService` is a wrapper that delegates to a simple internal implementation, enabling future enrichment (structured logging, retry, concurrency throttling). It exposes both sync + async style discovery of supported targets to support IDE and CLI use-cases.

## Build Targets & Task

`GenerateDocumentationTask` provides a thin facade approximating an MSBuild task while tests avoid a full MSBuild dependency. Real integration would substitute Microsoft.Build types without altering public interfaces.

Targets file (`Terminal.Gui.Xaml.Documentation.targets`) maps MSBuild properties (`EnableXamlDocumentation`, `XamlDocumentationOutputPath`, `XamlDocumentationBuildMode`) to task invocation.

## Performance & Regression Loop

1. BenchmarkDotNet micro-benchmarks measure full vs incremental generation.
2. Results persisted to `.benchmarks/last-results.json`.
3. History appended to `.benchmarks/history.csv` and converted to JSON + summary.
4. GitHub Action posts PR comment diff and publishes history to `benchmarks-data` branch.
5. Static dashboard (HTML) visualizes latest metrics.

Optional regression gating (environment-controlled) can fail CI on >20% slowdown.

## Extensibility Points

- Replace simulation with real DocFX invocation (process start or library) inside `DocumentationGeneratorService`.
- Add richer coverage analysis by parsing real metadata + API model outputs.
- Introduce plugin system for custom validation rules (e.g., bookmarking, link integrity, image dimension checks).
- Implement a DocFX post-processor pipeline for index tuning (stopwords, ranking).

## Error Handling & Logging

Services lean on explicit result objects instead of throwing for validation and generation issues to keep test scenarios deterministic. Exceptions are reserved for infrastructure failures.

`IDocumentationLogger` allows test injection of null/no-op or future structured log sinks.

## Quality & Compliance

- Static analysis: .NET built-in analyzers configured via `.editorconfig` with `TreatWarningsAsErrors`.
- XML docs: All public types documented (see T039).
- Complexity: Services kept intentionally small; orchestration extracted from models.
- Tests: Contract, integration, unit, and performance layers cover the surfaces enumerated in `tasks.md` phases.

## Future Directions

| Area | Potential Enhancement |
|------|-----------------------|
| Real DocFX Engine | Invoke `docfx` with generated config & capture JSON logs |
| Coverage Metrics | Compute real API coverage from metadata files |
| Link Validation | Parse HTML artifacts & detect broken anchors |
| Incremental Mode | Track input file hashes & skip unchanged segments |
| Dashboard UX | Add charting (moving averages, z-score anomalies) |

Contributions and design discussions welcome—open an issue or start a Discussion thread.
