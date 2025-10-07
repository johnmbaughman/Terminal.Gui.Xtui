# Quickstart: Contributing Documentation with DocFX

This quickstart helps you preview and update the documentation locally, verify examples, and prepare contributions.

## Prerequisites
- .NET 8 SDK installed
- PowerShell 7+ (`pwsh`) recommended on Windows
- DocFX installed or use repo scripts (if available)

## Preview the Docs Locally
```pwsh
# From repo root
# If DocFX is not installed, install it once (user scope):
# dotnet tool update -g docfx

# Build and serve the docs folder
cd docs
# If using global tool
# docfx docfx.json --serve
# Or if using a local build script, prefer it (if present):
# pwsh ../scripts/build.ps1
```

Open the served URL printed in the console (e.g., http://localhost:8080) to preview changes.

## Add or Update Content
- Conceptual pages: `docs/articles/` and `docs/index.md`
- API reference: ensure XML documentation in `src/Terminal.Gui.Xaml` is current
- TOC: update `docs/toc.yml` and `docs/articles/toc.yml` accordingly

## Authoring Guidelines
- Use GitHub-flavored Markdown
- Each page should cover: What, How, Why
- Provide runnable examples with prerequisites and expected output
- Use relative links; ensure anchors exist; avoid dead links
- Add “Introduced in/Deprecated in” metadata where applicable

## Validate Changes
```pwsh
# Link and build validation
# If using docfx as a tool:
# docfx build docfx.json

# Optionally run repository validation scripts if available
pwsh ./scripts/build.ps1
pwsh ./scripts/test.ps1
```

## Submit Changes
- Create a feature branch and open a PR
- Ensure CI passes (build, tests, link checks)
- Reference related issues or specs
