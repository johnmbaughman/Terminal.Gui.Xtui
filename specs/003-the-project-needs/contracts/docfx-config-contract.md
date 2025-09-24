# Contract: DocFX Configuration Expectations

DocFX configuration (docs/docfx.json) MUST support:

## Inputs
- Conceptual content under `docs/` (e.g., `index.md`, `articles/*`)
- API reference generated from assemblies and XML documentation in `src/Terminal.Gui.Xaml/bin/*` with proper `metadata` config

## Outputs
- Static site output folder (default `_site` or configured)
- Search index generation enabled
- Clean build option for CI

## Navigation
- `toc.yml` declared at root; nested TOCs for articles and API
- Friendly titles for key sections

## Validation
- Warnings treated as errors in CI where feasible
- Link validation enabled

## Extensibility
- Room for future multi-version strategy (folder-per-version or branch-per-version)
