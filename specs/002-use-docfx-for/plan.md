
# Implementation Plan: Use DocFX for Documentation Generation

**Branch**: `002-use-docfx-for` | **Date**: September 23, 2025 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-use-docfx-for/spec.md`

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
Implement comprehensive API documentation generation for the Terminal.Gui.Xaml framework using DocFX to automatically generate searchable HTML documentation from XML documentation comments in source code, integrated into the existing build pipeline.

## Technical Context
**Language/Version**: C# with .NET 8+ (LTS)  
**Primary Dependencies**: DocFX, Microsoft.CodeAnalysis (Roslyn), Terminal.Gui v2+  
**Storage**: N/A (static documentation generation)  
**Testing**: xUnit, FluentAssertions for documentation validation tests  
**Target Platform**: Cross-platform (.NET 8+)  
**Project Type**: Single (documentation tooling extension to existing framework)  
**Performance Goals**: Documentation generation <5 minutes for full API surface  
**Constraints**: Must integrate with existing MSBuild pipeline, <50MB output size  
**Scale/Scope**: Complete API surface coverage, searchable documentation site

**User-provided context**: The framework will use DocFX for documentation generation.

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Code Quality Standards Check:**
- [x] All planned components follow Microsoft C# coding conventions
- [x] Static analysis integration planned (StyleCop, FxCop/Analyzers) - existing infrastructure
- [x] XML documentation strategy for public APIs defined - DocFX consumes XML comments
- [x] SOLID principles applied to architectural design - documentation generation as service

**Test-Driven Development Check:**
- [x] Test-first approach planned for all new features
- [x] Unit test coverage target ≥80% for new code established
- [x] Integration tests planned for all public API endpoints - documentation validation
- [x] XAML parsing and UI component test strategy defined - existing infrastructure

**User Experience Consistency Check:**
- [x] Terminal.Gui design patterns and conventions followed - documentation only
- [x] XAML markup structure follows semantic standards - not applicable
- [x] Keyboard navigation and accessibility support planned - web accessibility for docs
- [x] Consistent error handling and user feedback strategy - build-time error reporting

**Performance Requirements Check:**
- [x] XAML parsing performance target <100ms - not impacted by documentation
- [x] UI rendering performance target >30 FPS - not impacted by documentation
- [x] Memory usage constraint <50MB - documentation generation is build-time only
- [x] Component initialization performance target <50ms - not impacted by documentation
- [x] Performance regression detection strategy planned - documentation build time monitoring

## Project Structure

### Documentation (this feature)
```
specs/002-use-docfx-for/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
# Option 1: Single project (SELECTED - documentation tooling extension)
src/
├── Terminal.Gui.Xaml/
│   ├── Documentation/   # DocFX configuration and templates
│   ├── Models/         # Existing
│   ├── Services/       # Existing
│   └── Build/          # MSBuild integration for docs

docs/
├── api/                # Generated API documentation
├── articles/           # Hand-written documentation
├── templates/          # DocFX templates
└── docfx.json          # DocFX configuration

tests/
├── Terminal.Gui.Xaml.Tests/
│   ├── Documentation/ # Documentation validation tests
│   ├── Contract/      # Existing
│   ├── Integration/   # Existing
│   └── Unit/          # Existing

# Option 2: Web application (when "frontend" + "backend" detected)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure]
```

**Structure Decision**: Option 1 - Single project with documentation tooling extension. Documentation generation integrates with existing Terminal.Gui.Xaml framework structure.

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
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
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

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
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*No constitutional violations identified. All requirements align with constitutional principles.*

No complexity deviations documented - implementation follows standard patterns within constitutional guidelines.


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
- [x] Complexity deviations documented

---
*Based on Constitution v2.1.1 - See `/memory/constitution.md`*
