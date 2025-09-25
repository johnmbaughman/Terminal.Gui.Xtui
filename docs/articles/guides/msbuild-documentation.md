# Documentation Generation via MSBuild

This guide explains how the Terminal.Gui.Xaml package integrates with MSBuild to optionally generate and validate project documentation.

## Overview

When you add a reference to the `Terminal.Gui.Xaml` package, it brings in build-transitive targets that enable documentation generation and validation through simple MSBuild properties. Nothing runs unless you opt in.

## Key MSBuild Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableXamlDocumentation` | bool | `false` (Debug), `true` (Release) via `DocumentationEnabled` | Master switch enabling documentation targets. |
| `XamlDocumentationOutputPath` | string | `$(OutputPath)docs` | Output folder for generated site artifacts. |
| `XamlDocumentationBuildMode` | string (`Incremental`\|`Full`) | `Incremental` | Chooses between incremental or full generation (future expansion). |
| `RequiredDocumentationCoverage` | double | `80.0` | Minimum coverage gate enforced during validation. |
| `ValidateDocumentationLinks` | bool | `true` | Enables simulated link validation. |
| `ValidateDocumentationExamples` | bool | `true` | Enables simulated example validation. |

## Targets

| Target | When It Runs | Purpose |
|--------|--------------|---------|
| `XamlGenerateDocumentation` | After `Build` when `EnableXamlDocumentation=true` | Performs generation (and implicit validation). |
| `XamlValidateDocumentation` | Manual (`/t:XamlValidateDocumentation`) | Validates documentation without forcing a separate generation pass. |
| `XamlCleanDocumentation` | After `Clean` | Removes documentation output directory. |

## Quick Examples

Enable documentation in a Debug build:
```bash
dotnet build /p:EnableXamlDocumentation=true
```

Force full build mode:
```bash
dotnet build -c Release /p:EnableXamlDocumentation=true /p:XamlDocumentationBuildMode=Full
```

Run validation only:
```bash
dotnet msbuild /t:XamlValidateDocumentation /p:EnableXamlDocumentation=true
```

Override output path:
```bash
dotnet build -c Release /p:EnableXamlDocumentation=true /p:XamlDocumentationOutputPath=artifacts/docs
```

## Correlation & Timing

Generation and validation responses include a `CorrelationId` and timing metrics (milliseconds) for traceability. If you don't supply a correlation id, one is generated automatically.

## Roadmap

- Separate validation-specific task implementation
- Template customization via `docs/templates/`
- Structured build logging for CI analysis

See also: [Version & Compatibility Matrix](version-compatibility.md) for feature maturity status across releases.

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| No documentation generated | `EnableXamlDocumentation` not set | Pass `/p:EnableXamlDocumentation=true` |
| Incremental changes not picked up | Mode set to `Incremental` with stale cache | Re-run with `/p:XamlDocumentationBuildMode=Full` |
| Coverage gate fails | `RequiredDocumentationCoverage` too high | Lower value or improve XML documentation |

Have suggestions? Open an issue or start a discussion.
