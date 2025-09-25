# Tasks: DocFX Documentation Generation

**Input**: Design documents from `/specs/002-use-docfx-for/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
Based on plan.md structure: Single project with documentation tooling extension
- **Framework source**: `src/Terminal.Gui.Xaml/`
- **Documentation tools**: `src/Terminal.Gui.Xaml/Documentation/`, `src/Terminal.Gui.Xaml/Build/`
- **Tests**: `tests/Terminal.Gui.Xaml.Tests/`
- **Generated docs**: `docs/`

## Phase 3.1: Setup
- [x] T001 Create documentation project structure per implementation plan (docs/, src/Terminal.Gui.Xaml/Documentation/, src/Terminal.Gui.Xaml/Build/)
- [x] T002 Add DocFX NuGet package dependency to Terminal.Gui.Xaml.csproj
- [x] T003 [P] Configure DocFX MSBuild integration targets in Directory.Build.targets
- [x] T004 [P] Create initial docfx.json configuration file in docs/docfx.json

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [x] T005 [P] Contract test IDocumentationGeneratorService.GenerateDocumentationAsync in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationGeneratorServiceContractTests.cs
- [x] T006 [P] Contract test IDocumentationGeneratorService.ValidateDocumentationAsync in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationValidationContractTests.cs
- [x] T007 [P] Contract test IDocumentationConfigurationService contract in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationConfigurationContractTests.cs
- [x] T008 [P] Contract test IBuildIntegrationService MSBuild integration in tests/Terminal.Gui.Xaml.Tests/Contracts/BuildIntegrationContractTests.cs
- [x] T009 [P] Integration test end-to-end documentation generation in tests/Terminal.Gui.Xaml.Tests/Integration/DocumentationGenerationIntegrationTests.cs
- [x] T010 [P] Test DocumentationMetadata models with validation in tests/Terminal.Gui.Xaml.Tests/Models/DocumentationMetadataModelsTests.cs
- [x] T011 [P] Integration test MSBuild target execution in tests/Terminal.Gui.Xaml.Tests/Integration/MSBuildTargetExecutionTests.cs
- [x] T012 [P] Integration test documentation site navigation and search in tests/Terminal.Gui.Xaml.Tests/Integration/DocumentationSiteTests.cs

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [x] T013 [P] DocFxConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/DocFxConfiguration.cs
- [x] T014 [P] TemplateConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/TemplateConfiguration.cs
- [x] T015 [P] BuildConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/BuildConfiguration.cs
- [x] T016 [P] ApiDocumentation model in src/Terminal.Gui.Xaml/Documentation/Models/ApiDocumentation.cs
- [x] T017 [P] DocumentationValidationResult model in src/Terminal.Gui.Xaml/Documentation/Models/DocumentationValidationResult.cs
- [x] T018 [P] ValidationIssue model in src/Terminal.Gui.Xaml/Documentation/Models/ValidationIssue.cs
- [x] T019 [P] CoverageMetrics model in src/Terminal.Gui.Xaml/Documentation/Models/CoverageMetrics.cs
- [x] T020 IDocumentationGeneratorService interface in src/Terminal.Gui.Xaml/Documentation/Services/IDocumentationGeneratorService.cs
- [x] T021 DocumentationGeneratorService implementation in src/Terminal.Gui.Xaml/Documentation/Services/DocumentationGeneratorService.cs
- [x] T022 IDocumentationConfigurationService interface in src/Terminal.Gui.Xaml/Documentation/Services/IDocumentationConfigurationService.cs
- [x] T023 DocumentationConfigurationService implementation in src/Terminal.Gui.Xaml/Documentation/Services/DocumentationConfigurationService.cs
- [x] T024 IBuildIntegrationService interface (consolidated: uses existing interface in src/Terminal.Gui.Xaml/Documentation/Services/IBuildIntegrationService.cs; duplicate Build path removed)
- [x] T025 BuildIntegrationService implementation in src/Terminal.Gui.Xaml/Build/BuildIntegrationService.cs
- [x] T026 GenerateDocumentationTask MSBuild task in src/Terminal.Gui.Xaml/Build/GenerateDocumentationTask.cs
- [x] T027 Documentation validation and coverage analysis in DocumentationGeneratorService.ValidateDocumentationAsync
- [x] T028 Error handling and logging integration across all services

# Phase 3.4: Integration
- [x] T029 MSBuild targets file integration in src/Terminal.Gui.Xaml/Build/Terminal.Gui.Xaml.Documentation.targets
- [x] T030 NuGet package configuration for documentation tools
- [x] T031 DocFX template customization and branding in docs/templates/
- [x] T032 CI/CD pipeline integration for documentation deployment in .github/workflows/documentation.yml
- [x] T033 Documentation site hosting and search configuration

## Phase 3.5: Polish & Constitutional Compliance
- [x] T034 [P] Unit tests for DocFxConfiguration validation in tests/Terminal.Gui.Xaml.Tests/Unit/DocFxConfigurationTests.cs
- [x] T035 [P] Unit tests for DocumentationGeneratorService in tests/Terminal.Gui.Xaml.Tests/Unit/DocumentationGeneratorServiceTests.cs
- [x] T036 [P] Unit tests for BuildIntegrationService in tests/Terminal.Gui.Xaml.Tests/Unit/BuildIntegrationServiceTests.cs
- [x] T037 Performance tests (documentation generation <5 minutes, memory <200MB) in tests/Terminal.Gui.Xaml.Tests/Performance/DocumentationPerformanceTests.cs
- [x] T038 [P] Static analysis compliance (.NET analyzers via .editorconfig) for all documentation code
- [x] T039 [P] XML documentation for all public documentation APIs
- [x] T040 [P] Update main README.md with DocFX documentation setup instructions
- [x] T041 [P] Create documentation architecture guide in docs/articles/architecture.md
- [x] T042 Code quality review (cyclomatic complexity <15, SOLID principles) for documentation services — extracted helpers in SimpleDocumentationGeneratorService to reduce method complexity and improve separation of concerns
- [x] T043 UX consistency validation (error messages, build integration patterns) — standardized build error messages with BUILDxxxx codes and normalized progress messages in SimpleBuildIntegrationService
- [x] T044 Remove code duplication across documentation services — introduced shared PathHelpers (repo root + ResolvePath + normalization) and refactored SimpleBuildIntegrationService and SimpleDocumentationGeneratorService to use it
- [x] T045 Execute quickstart.md manual testing scenarios — documented manual checks and added `QuickstartSmokeTests` to automate basic generation/validation/build flows

## Dependencies
- Setup (T001-T004) before tests (T005-T012)
- Tests (T005-T012) before implementation (T013-T028)
- Models (T013-T019) before services (T020-T028)
- Services (T020-T028) before integration (T029-T033)
- Core implementation before polish & constitutional compliance (T034-T045)

## Parallel Example
```
# Launch T005-T008 together (contract tests):
Task: "Contract test IDocumentationGeneratorService.GenerateDocumentationAsync in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationGeneratorServiceContractTests.cs"
Task: "Contract test IDocumentationGeneratorService.ValidateDocumentationAsync in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationValidationContractTests.cs"
Task: "Contract test IDocumentationConfigurationService in tests/Terminal.Gui.Xaml.Tests/Contracts/DocumentationConfigurationContractTests.cs"
Task: "Contract test IBuildIntegrationService in tests/Terminal.Gui.Xaml.Tests/Contracts/BuildIntegrationContractTests.cs"

# Launch T013-T019 together (model creation):
Task: "DocFxConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/DocFxConfiguration.cs"
Task: "TemplateConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/TemplateConfiguration.cs"
Task: "BuildConfiguration model in src/Terminal.Gui.Xaml/Documentation/Models/BuildConfiguration.cs"
Task: "ApiDocumentation model in src/Terminal.Gui.Xaml/Documentation/Models/ApiDocumentation.cs"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Focus on TDD approach with comprehensive test coverage
- Maintain constitutional compliance throughout implementation

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - documentation-services.md → T005-T007 (service contract tests)
   - msbuild-integration.md → T008 (build integration contract test)
   - Each service interface → T020, T022, T024 (interface definitions)
   - Each service implementation → T021, T023, T025 (implementations)

2. **From Data Model**:
   - DocFxConfiguration → T013 (model creation)
   - TemplateConfiguration → T014 (model creation)
   - BuildConfiguration → T015 (model creation)
   - ApiDocumentation → T016 (model creation)
   - DocumentationValidationResult → T017 (model creation)
   - ValidationIssue → T018 (model creation)
   - CoverageMetrics → T019 (model creation)

3. **From Quickstart Guide**:
   - Installation workflow → T009 (basic generation test)
   - XML documentation validation → T010 (validation test)
   - MSBuild integration → T011 (build test)
   - Documentation site → T012 (site test)
   - Manual testing scenarios → T045 (validation)

4. **Ordering**:
   - Setup → Tests → Models → Services → Integration → Polish
   - Dependencies prevent parallel execution where files are shared

## Validation Checklist
*GATE: Checked by main() before returning*

- [x] All contracts have corresponding tests (T005-T008 for documentation-services.md and msbuild-integration.md)
- [x] All entities have model tasks (T013-T019 for all data model entities)
- [x] All tests come before implementation (T005-T012 before T013-T028)
- [x] Parallel tasks truly independent (different file paths)
- [x] Each task specifies exact file path
- [x] No task modifies same file as another [P] task
- [x] TDD approach followed (failing tests before implementation)
- [x] Constitutional compliance tasks included (performance, code quality, documentation)
