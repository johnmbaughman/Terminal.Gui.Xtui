
# Implementation Plan: Terminal.Gui XAML Framework

**Branch**: `001-build-a-framework` | **Date**: 2025-09-23 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-build-a-framework/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Build a comprehensive XAML framework for Terminal.Gui v2+ that enables declarative UI development with Microsoft XAML standards compatibility. The framework uses Roslyn source generators for build-time code generation, supports both MVVM and non-MVVM patterns, and leverages .NET 8+ features for cross-platform compatibility (Windows, Linux, macOS). Key capabilities include XAML parsing, data binding, event handling, and design-time tooling integration.

## Technical Context
**Language/Version**: C# with .NET 8+ (LTS and latest features)  
**Primary Dependencies**: Terminal.Gui v2+, Microsoft.CodeAnalysis (Roslyn), System.Xml.Linq, minimal third-party references  
**Storage**: File-based XAML documents, MSBuild integration files  
**Testing**: xUnit, FluentAssertions, Microsoft.CodeAnalysis.Testing  
**Target Platform**: Cross-platform (Windows, Linux, macOS) via .NET 8+ runtime  
**Project Type**: Single library project with MSBuild integration  
**Performance Goals**: XAML parsing <100ms, UI rendering >30 FPS, memory <50MB, initialization <50ms  
**Constraints**: Minimal external dependencies, cross-platform compatibility, .NET 8+ exclusive, Terminal.Gui v2+ compatibility  
**Scale/Scope**: Support for complex XAML documents (1000+ elements), enterprise-grade applications, extensive tooling integration

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Code Quality Standards Check:**
- [x] All planned components follow Microsoft C# coding conventions
- [x] Static analysis integration planned (StyleCop, FxCop/Analyzers)
- [x] XML documentation strategy for public APIs defined
- [x] SOLID principles applied to architectural design

**Test-Driven Development Check:**
- [x] Test-first approach planned for all new features
- [x] Unit test coverage target ≥80% for new code established
- [x] Integration tests planned for all public API endpoints
- [x] XAML parsing and UI component test strategy defined

**User Experience Consistency Check:**
- [x] Terminal.Gui design patterns and conventions followed
- [x] XAML markup structure follows semantic standards
- [x] Keyboard navigation and accessibility support planned
- [x] Consistent error handling and user feedback strategy

**Performance Requirements Check:**
- [x] XAML parsing performance target <100ms for typical documents
- [x] UI rendering performance target >30 FPS established
- [x] Memory usage constraint <50MB for standard document sizes
- [x] Component initialization performance target <50ms
- [x] Performance regression detection strategy planned

## Project Structure

### Documentation (this feature)
```
specs/001-build-a-framework/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
### Source Code (repository root)
```
# Terminal.Gui XAML Framework - Single Library Project
src/
├── Core/               # Core XAML parsing and processing
├── CodeGeneration/     # Roslyn-based source generators  
├── Runtime/            # Runtime binding and execution
├── MSBuild/            # MSBuild integration tasks
└── Extensions/         # Extensibility and markup extensions

tests/
├── Unit/              # Unit tests for individual components
├── Integration/       # Integration tests for end-to-end scenarios  
├── Performance/       # Performance and benchmark tests
└── Samples/           # Sample XAML files for testing
```

**Structure Decision**: Single library project optimized for .NET 8+ cross-platform deployment with clear separation of concerns between parsing, code generation, runtime, and tooling integration.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - Roslyn source generator patterns for XAML → research task
   - .NET 8+ specific features for cross-platform support → research task  
   - Terminal.Gui v2+ control architecture and extensibility → research task
   - Microsoft XAML standard compliance requirements → research task

2. **Generate and dispatch research agents**:
   ```
   Task: "Research Roslyn source generator best practices for XAML processing"
   Task: "Research .NET 8+ cross-platform deployment and runtime features"
   Task: "Research Terminal.Gui v2+ control model and extensibility patterns"
   Task: "Research Microsoft XAML standards and compliance requirements"
   Task: "Research MSBuild integration patterns for build-time code generation"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]  
   - Alternatives considered: [what else evaluated]
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - XAML parsing components (XamlDocument, XamlElement, XamlAttribute)
   - Code generation models (CodeGenerator, GeneratedClass, BindingExpression)
   - Runtime components (ViewFactory, DataBinding, EventHandler)
   - Build integration models (XamlBuildTask, SourceGeneratorContext)

2. **Generate API contracts** from functional requirements:
   - For each public interface → API specification
   - XAML parsing API contracts
   - Code generation service contracts
   - Runtime binding service contracts
   - Output API documentation to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per major API surface
   - Assert interface contracts and behavior
   - Tests must fail (no implementation yet)
   - Focus on constitutional compliance (performance, quality)

4. **Extract test scenarios** from user stories:
   - Each user story → integration test scenario
   - XAML compilation workflow scenarios
   - Data binding scenario validation
   - Cross-platform compatibility scenarios
   - Quickstart test = complete XAML-to-UI workflow

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each API contract → contract test task [P] (xaml-parser-api, code-generation-api, runtime-binding-api, msbuild-integration-api)
- Each core entity → model creation task [P] (XamlDocument, XamlElement, CodeGenerator, DataBindingEngine)
- Each quickstart scenario → integration test task (XAML-to-UI workflow, MVVM pattern, custom controls, error handling, performance)
- Implementation tasks to make tests pass (parser, code generator, runtime engine, MSBuild integration)

**Ordering Strategy**:
- TDD order: Contract tests before implementation
- Dependency order: Core models → Parsing → Code Generation → Runtime → MSBuild integration
- Mark [P] for parallel execution (independent files and components)
- Performance tests after functional implementation

**Framework-Specific Task Categories**:
- **Setup**: Project structure, .NET 8 configuration, Terminal.Gui v2+ references
- **Core Parsing**: XAML document model, XML processing, validation
- **Code Generation**: Roslyn source generators, template engine, C# code emission
- **Runtime Binding**: Data binding engine, event handling, value conversion
- **MSBuild Integration**: Build tasks, NuGet packaging, Visual Studio tooling
- **Cross-Platform Testing**: Windows, Linux, macOS validation
- **Performance Optimization**: Constitutional compliance validation

**Estimated Output**: 35-40 numbered, ordered tasks in tasks.md with clear dependency chains

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [x] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented (none required)

---
*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
