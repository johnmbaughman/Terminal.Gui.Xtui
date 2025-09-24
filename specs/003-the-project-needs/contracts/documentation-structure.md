# Contract: Documentation Structure and IA

## Required Top-Level Sections
- Getting Started
- Concepts
- Guides
- API Reference (auto-generated from XML docs)
- Examples
- Tutorials
- FAQ
- Glossary
- What’s New (Changelog)
- Contributing

## Page Requirements
- Each page must include: purpose (What), actionable steps (How), rationale (Why)
- Examples must include: prerequisites, steps, expected output, notes
- Use GitHub-flavored Markdown elements only

## Navigation and URLs
- TOC entries defined in `docs/toc.yml` and nested TOCs
- Stable URLs; provide redirects when moving/renaming pages
- Breadcrumbs reflect IA depth

## Metadata
- Indicate version introduced/deprecated where relevant
- Tags for searchability (DocFX metadata or front matter)

## Quality Gates
- No broken internal links
- Build succeeds with zero warnings where practical
- Examples compile or run as described
