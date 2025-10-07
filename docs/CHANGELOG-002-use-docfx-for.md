# Changelog — feature/002-use-docfx-for

Date: 2025-10-06

Summary

- Centralized documentation prefix and utilities.
- Added runtime adapter to ensure build-engine logs are prefixed for CI parsing.
- Fixed stray markdown fences in tests preventing compilation.
- Added unit test for `BuildEnginePrefixedAdapter` and updated integration test for quickstart.
- Marked tasks T044 and T045 done in `specs/002-use-docfx-for/tasks.md`.

Files changed (high level)

- src/Terminal.Gui.Xaml/Documentation/Utilities/DocumentationUtilities.cs (existing)
- src/Terminal.Gui.Xaml/Build/BuildEnginePrefixedAdapter.cs (new)
- src/Terminal.Gui.Xaml/Build/GenerateDocumentationTask.cs (modified: BuildEngine property wraps adapter)
- tests/Terminal.Gui.Xaml.Tests/Unit/BuildEnginePrefixedAdapterTests.cs (new)
- tests/Terminal.Gui.Xaml.Tests/Integration/QuickstartManualRunner.cs (fixed markdown fences)
- specs/002-use-docfx-for/tasks.md (T044/T045 marked done)

Notes

- The PR includes a small runtime wiring change so documentation-related build logs use the centralized prefix (`DocumentationUtilities.DocsPrefix`). This keeps CI parsing deterministic.
- The quickstart workflow is exercised by an integration test that runs as part of the test suite; if you prefer a true DocFX invocation, I can add an optional guarded integration.

How to validate locally

1. Run the test suite:

```powershell
dotnet test "Terminal.Gui.Xaml.sln" --no-build -v minimal
```

2. To manually inspect the quickstart integration, run the specific test or run the project and invoke the GenerateDocumentationTask in your environment.

---
