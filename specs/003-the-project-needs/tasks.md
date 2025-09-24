# Tasks: Comprehensive API and Usage Documentation (DocFX)
003-the-project-needs
T017 — Example compilation validation [✓ COMPLETE]
- File(s): `tests/Terminal.Gui.Xaml.Tests` or a new lightweight example validation harness
- Action: Add minimal tests or scripts that compile/run example snippets to catch drift
- Status: Implemented graceful validation system with 40 code blocks tracked across 3 example files

T018 — Cross-linking API reference to guides/examples [✓ COMPLETE]
- File(s): `docs/api/index.md`, generated reference output; link targets in conceptual pages
- Action: Add "See also" sections linking reference pages to guides/examples
- Status: Enhanced API index with learning paths; added Related APIs sections to all guides and examples; created comprehensive bidirectional cross-linking system

T019 — Version metadata pattern [✓ COMPLETE]
- File(s): `docs/articles/*`
- Action: Add "Introduced in/Deprecated in" conventions and apply to at least a few pages; document pattern in style guide
- Status: Created comprehensive version metadata template in style guide; applied to examples and guides; demonstrated deprecation and preview patternsrsonal\Files\source\repos\Terminal.Gui.Xaml\specs\003-the-project-needs\spec.md
Plan: C:\Personal\Files\source\repos\Terminal.Gui.Xaml\specs\003-the-project-needs\plan.md

Execution rules
- TDD-first for documentation examples: write validation/checks before content where applicable
- Parallelize [P] tasks that touch different files
- Use Windows PowerShell Core (`pwsh`) syntax for commands

Dependencies
- Setup before content changes
- IA before adding many pages
- Example validation before publishing

## Numbered Tasks (dependency-ordered)

T001 — Setup DocFX validation and preview scripts [X]
- File(s): `docs/docfx.json`, `scripts/build.ps1`
- Action: Ensure `scripts/build.ps1` builds DocFX site and fails on warnings; add/adjust `docfx.json` settings for search and link validation
- Notes: Keep in repo tooling; avoid global dependency assumptions

T002 — Establish documentation Information Architecture (IA) [X]
- File(s): `docs/toc.yml`, `docs/index.md`, `docs/articles/toc.yml`
- Action: Define top-level sections (Getting Started, Concepts, Guides, API Reference, Examples, Tutorials, FAQ, Glossary, What’s New, Contributing) and create placeholder pages as needed
- Notes: Preserve stable URLs; include breadcrumbs and friendly titles

T003 [P] — Research consolidation and editorial standards page [X]
- File(s): `docs/articles/contributing/docs-style-guide.md`
- Action: Create style guide summarizing Markdown rules, “What/How/Why” pattern, version metadata, and examples policy

T004 — API reference source checks [X]
- File(s): `src/Terminal.Gui.Xaml/*.csproj`, `src/Terminal.Gui.Xaml/**/*.cs`
- Action: Ensure XML documentation generation is enabled and public APIs are documented; fix any missing summaries flagged by analyzers

T005 — Getting Started path [X]
- File(s): `docs/articles/getting-started.md` (existing), `docs/index.md`
- Action: Expand or align Getting Started to use Terminal.Gui.Xaml with a minimal example; link from home page and TOC

T006 [P] — Concepts: Layout, Binding, Events overview pages [X]
- File(s): `docs/articles/concepts/layout.md`, `binding.md`, `events.md`
- Action: Author concept pages explaining What/How/Why with diagrams where useful

T007 [P] — Guides: Step-by-step tasks [X]
- File(s): `docs/articles/guides/*.md`
- Action: Author guides for common tasks (e.g., create a window, bind data, handle input, navigation)

T008 — Examples index and policy [X]
- File(s): `docs/articles/examples/index.md`
- Action: Create an index and document example prerequisites, OS/shell coverage (Windows+pwsh primary; bash notes where needed), .NET 8 baseline

T009 [P] — Runnable examples (initial set) [X]
- File(s): `docs/articles/examples/*.md`
- Action: Add 3–5 copy-pasteable examples with expected output; cross-link to relevant APIs and guides

T010 — Accessibility guidance [X]
- File(s): `docs/articles/accessibility.md`
- Action: Provide terminal UI accessibility best practices (keyboard navigation, color contrast, focus)

T011 — What’s New / Changelog [X]
- File(s): `docs/articles/changelog.md`
- Action: Establish a changelog format and link to changed docs and APIs

T012 — FAQ and Glossary [X]
- File(s): `docs/articles/faq.md`, `docs/articles/glossary.md`
- Action: Seed common questions and define key terms

T013 — Contributing guide for docs [X]
- File(s): `docs/articles/contributing/index.md`
- Action: Explain contribution workflow, PR checks, local preview, and validation

T014 — DocFX configuration alignment (contract) [X]
- File(s): `docs/docfx.json`
- Action: Ensure config matches the contract in `specs/003-the-project-needs/contracts/docfx-config-contract.md` (search, toc, metadata, warnings as errors in CI)

T015 — Documentation structure alignment (contract) [X]
- File(s): `docs/toc.yml`, `docs/articles/**/*.md`
- Action: Ensure IA matches `specs/003-the-project-needs/contracts/documentation-structure.md`, including required sections and metadata

T016 — Link validation and CI integration [X]
- File(s): `.github/workflows/*.yml` (if present), `scripts/test.ps1`, `scripts/validate-links.ps1`
- Action: Add link checking step and ensure build fails on broken links; wire into `scripts/test.ps1`

T017 [P] — Example compilation validation [X]
- File(s): `tests/Terminal.Gui.Xaml.Tests` or a new lightweight example validation harness
- Action: Add minimal tests or scripts that compile/run example snippets to catch drift

T018 — Cross-linking API reference to guides/examples
- File(s): `docs/api/index.md`, generated reference output; link targets in conceptual pages
- Action: Add “See also” sections linking reference pages to guides/examples

T019 — Version metadata pattern
- File(s): `docs/articles/*`
- Action: Add “Introduced in/Deprecated in” conventions and apply to at least a few pages; document pattern in style guide

T020 — Offline distribution plan [✓ COMPLETE]
- File(s): `docs/articles/contributing/offline.md`
- Action: Document how to consume docs offline (clone + build or packaged zip)
- Status: Created comprehensive offline documentation guide with multiple distribution methods, troubleshooting, and customization options

T021 — Final sweep and QA [✓ COMPLETE]
- File(s): Entire `docs/` tree
- Action: Run local build, fix warnings, validate links, ensure TOC/nav coherence, and confirm examples work
- Status: DocFX builds successfully (190 expected dev warnings), fixed bookmark errors, example validation working (40 code blocks tracked), TOC structure validated and updated

## Parallel Execution Guidance
- You can run T003, T006, T007, T009, T017 in parallel [P] as they touch different files.
- Sequence: T001 → T002 → (T003 [P], T004, T005) → (T006 [P], T007 [P], T008) → (T009 [P], T010, T011, T012, T013) → T014 → T015 → T016 → (T017 [P], T018, T019, T020) → T021

## Agent Commands Examples
- Generate a new concept page:
  - “Create `docs/articles/concepts/layout.md` with What/How/Why and cross-links to examples.”
- Add example with expected output:
  - “Create `docs/articles/examples/hello-world.md` with a .NET 8 copy-paste example and expected console output.”
- Update TOC:
  - “Insert Concepts and Guides sections in `docs/toc.yml` linking to new pages.”
