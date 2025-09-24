# Research: Comprehensive API and Usage Documentation (DocFX)

Date: 2025-09-24
Branch: 003-the-project-needs
Spec: C:\Personal\Files\source\repos\Terminal.Gui.Xaml\specs\003-the-project-needs\spec.md

## Decisions

1) Documentation Generator: DocFX (current in-repo configuration)
- Decision: Use existing `docs/docfx.json` and structure as the foundation.
- Rationale: Already integrated; supports API reference from XML docs and Markdown conceptual docs; easy GitHub Pages compatibility.
- Alternatives: Docusaurus, MkDocs, Sphinx. Rejected to avoid churn and duplication; DocFX best aligns with .NET APIs.

2) Versioning Strategy: Start single-version; plan for multi-version
- Decision: Maintain docs for the latest release initially; capture “Introduced in”/“Deprecated in” metadata in pages.
- Rationale: Simplicity now; leaves pathway to multi-version when release cadence requires it.
- Alternatives: Immediate multi-version docsets. Rejected due to overhead without present need.

3) Publishing Channel: GitHub Pages (future)
- Decision: Prepare docset for static site publication to GitHub Pages.
- Rationale: Low friction and standard for open-source.
- Alternatives: Azure Static Web Apps, Netlify. Deferred.

4) Information Architecture (IA)
- Decision: Top-level sections: Getting Started, Concepts, Guides, API Reference, Examples, Tutorials, FAQ, Glossary, What’s New, Contributing.
- Rationale: Familiar IA that balances onboarding, depth, and reference.
- Alternatives: Merge Concepts and Guides; rejected for clarity.

5) Examples Policy
- Decision: Provide copy-paste runnable examples with prerequisites, expected output, and notes; target .NET 8+; primary shell `pwsh` on Windows, include `bash` equivalents when non-Windows specifics arise.
- Rationale: Ensures reliability and developer confidence.
- Alternatives: Pseudocode-only examples; rejected.

6) Accessibility Guidance
- Decision: Include accessibility recommendations for terminal UIs (keyboard navigation, color contrast guidance, focus management, screen reader considerations if applicable).
- Rationale: Inclusivity and alignment with repo’s constitutional UX principles.

7) Validation & Quality Gates
- Decision: Add link checking and doc build checks to CI; compile any example projects as part of validation.
- Rationale: Prevents regressions and broken docs.

## Open Questions (tracked for later clarification)
- Multi-version policy details (how many versions visible; URL scheme like `/vX.Y/`, `/latest/`).
- Offline distribution needs (zip bundle?).
- Supported OS/shell matrix beyond Windows+pwsh and minimal bash notes.

## References
- docs/docfx.json (existing)
- docs/index.md, docs/articles/*, docs/api/*, docs/toc.yml
- src/Terminal.Gui.Xaml XML doc comments (API reference source)
