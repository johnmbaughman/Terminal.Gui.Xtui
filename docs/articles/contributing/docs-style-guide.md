# Docs Style Guide

This style guide de## Metadata and Versioning

### Version Information Template

All feature pages should include version metadata in a consistent format:

```markdown
> **Introduced in**: Terminal.Gui.Xaml vX.Y (Release Date)  
> **Last Updated**: Month Year  
> **Status**: Stable | Preview | Deprecated
```

**Examples:**

```markdown
> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable
```

```markdown
> **Introduced in**: Terminal.Gui.Xaml v1.2 (November 2025)  
> **Last Updated**: November 2025  
> **Status**: Preview
```

### Deprecation Notices

For deprecated features, add a clear notice at the top:

```markdown
> ⚠️ **DEPRECATED**: This feature is deprecated as of Terminal.Gui.Xaml vX.Y and will be removed in vZ.0. Use [New Feature](../new-feature.md) instead.
```

### Version-Specific Content

When documenting features available in multiple versions:

- **Default**: Document current stable version behavior
- **Version differences**: Use callout boxes for version-specific notes
- **Breaking changes**: Highlight changes that affect existing code

**Example:**

```markdown
## Using Data Binding

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)

Basic binding syntax:
```xml
<Label Text="{Binding UserName}" />
```

> 💡 **v1.2+**: Two-way binding support added for all input controls. See guidance on two-way binding in the Binding guide.

> ⚠️ **Breaking Change in v2.0**: The `Binding` syntax changed from `{Data UserName}` to `{Binding UserName}`. Update existing XAML files.
```

### Metadata Guidelines

- **Place version info** after the main title and description
- **Update "Last Updated"** whenever content changes significantly  
- **Use "Introduced in"** for new features, major API changes, or new components
- **Add deprecation notices** when features become obsolete
- **Keep terminology consistent** with the Glossary and What's New pagess authoring standards for the Terminal.Gui.Xaml documentation. All contributions must follow these rules.

## Principles
- Audience: Developers integrating Terminal.Gui.Xaml, from beginner to advanced
- Goals: Clarity, accuracy, consistency, minimal cognitive load
- Format: GitHub-flavored Markdown (GFM) only

## Page Structure
Every page should answer the three questions:
- What: Define the concept or feature and when to use it
- How: Show step-by-step usage with runnable examples
- Why: Explain rationale, trade-offs, and design intent

Recommended sections:
- Title (H1)
- Overview (What)
- How to Use (How)
- Rationale and Best Practices (Why)
- Examples
- See Also (cross-links to API and related topics)

## Markdown Conventions
- Headings: Use `#` H1 once per page; then `##`, `###` for subsections
- Code blocks: Use language fences (e.g., ` ```csharp `, ` ```pwsh `, ` ```xml `)
- Links: Use relative links within `docs/`; prefer descriptive link text
- Tables: Keep simple and accessible; avoid excessive width
- Images: Provide alt text; store under `docs/images/`

## Examples Policy
- Provide copy-pasteable, minimal examples
- State prerequisites (.NET 8+, OS/shell if relevant)
- Include expected output or screenshots when helpful
- Prefer `pwsh` commands on Windows; include `bash` variant if OS specifics differ
- Keep each example focused on a single concept

## API Reference Cross-Linking
- From conceptual/guides pages, link to relevant API reference pages (`api/`)
- Use a “See Also” section to connect concepts, guides, examples, and APIs

## Metadata and Versioning
- Note “Introduced in vX.Y” and “Deprecated in vX.Y” where applicable
- Use consistent terminology from the Glossary
- Keep change notes in What’s New and link to updated pages

## Accessibility
- Use clear language and proper heading hierarchy
- Provide keyboard navigation guidance where relevant
- Use color with sufficient contrast in images/diagrams

## Writing Style
- Use active voice and concise sentences
- Avoid idioms and region-specific slang
- Define acronyms on first use
- Use consistent capitalization for UI elements and APIs

## Review Checklist
- Covers What/How/Why
- Examples are runnable with prerequisites and expected output
- Links are relative and valid; no broken anchors
- Cross-links to API reference and related pages are present
- Version metadata applied when applicable
- Page added to `toc.yml` appropriately
