# Version & Compatibility Matrix

This document tracks compatibility across released (and planned) versions of Terminal.Gui.Xaml and its key dependencies and feature behaviors.

## Matrix Legend
- ✅ Supported / Tested
- ⚠️ Partial support or degraded behavior
- ❌ Not supported
- (blank) Not applicable

## Core Compatibility

| XAML Package Version | .NET Runtime | Terminal.Gui | DocFX Tooling | Incremental Docs Mode | Notes |
|----------------------|-------------|--------------|---------------|-----------------------|-------|
| 1.0.0-beta1          | .NET 8 (LTS) ✅ | 2.0.0-dev (current) ✅ | Simulated (no external) | Simulated Incremental ✅ | Initial beta; validation & generation simulated without external DocFX |
| 1.0.0 (planned)      | .NET 8 (LTS) ✅ | 2.x stable ✅ | DocFX 2.x (optional) ⚠️ | Incremental (hybrid) ⚠️ | Introduce optional real DocFX integration; fallback simulation kept |
| 1.1.0 (future)       | .NET 8/9 ✅ | 2.x stable ✅ | DocFX 2.x ✅ | Incremental (improved) ✅ | Distinct validation-only MSBuild task + template customization |
| 1.2.0 (future)       | (TBD) | (TBD) | (TBD) | (TBD) | Placeholder roadmap slot |

## Feature Behavior by Version

| Feature | Introduced In | 1.0.0-beta1 | 1.0.0 (planned) | 1.1.0 (future) |
|---------|---------------|-------------|-----------------|---------------|
| Simulated Generation Engine | 1.0.0-beta1 | ✅ | ✅ (fallback) | ✅ (fallback) |
| Real DocFX Invocation | 1.0.0 (planned) | ❌ | ⚠️ (optional) | ✅ |
| Incremental Build Flag | 1.0.0-beta1 | ✅ (metadata only) | ⚠️ (partial diff) | ✅ (member-level granularity) |
| Validation Coverage Gate | 1.0.0-beta1 | ✅ | ✅ | ✅ (configurable profiles) |
| Link Validation | 1.0.0-beta1 | ✅ (simulated) | ⚠️ (hybrid) | ✅ (actual) |
| Example Compilation | 1.0.0-beta1 | ✅ (simulated) | ⚠️ (hybrid) | ✅ (actual) |
| Correlation IDs | 1.0.0-beta1 | ✅ | ✅ | ✅ |
| Structured Logging | 1.0.0-beta1 | ✅ | ✅ (enriched) | ✅ (with event IDs) |
| Template Customization | 1.1.0 (future) | ❌ | ⚠️ (static copy) | ✅ (merge & override) |
| Versioned API Docs | 1.1.0 (future) | ❌ | ⚠️ (single snapshot) | ✅ (multi-version) |

## Updating This Matrix
1. Add a new row under Core Compatibility when branching a new minor or major version.
2. Update feature columns when a capability transitions (simulated → hybrid → actual).
3. Keep notes concise and action-oriented (why a cell is ⚠️ or limitations to track).
4. For deprecations: append a dagger (†) and add a footnote section.

## Footnotes
*(none yet)*

## Change Log Integration
This matrix complements `articles/changelog.md` by giving an at-a-glance operational compatibility view (changelog stays narrative; this stays tabular).

## Roadmap Extraction
Planned capabilities (⚠️) feed into backlog tasks; when delivered, cells are promoted to ✅ and notes updated.
