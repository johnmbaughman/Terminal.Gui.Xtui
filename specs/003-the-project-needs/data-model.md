# Data Model: Documentation Domain

Date: 2025-09-24
Branch: 003-the-project-needs

## Entities

- DocumentationSet
  - Description: The complete body of content for a library version.
  - Attributes: Version, BuildConfig, IA (toc.yml), Index, Redirects
  - Relationships: has many Topics, ReferenceItems, Examples, Tutorials

- Topic
  - Description: Conceptual subject area (e.g., Layout, Binding).
  - Attributes: Title, Summary, Body, Tags, Links, LastUpdated
  - Relationships: links to ReferenceItems, Examples, Tutorials

- ReferenceItem
  - Description: Public API type or member.
  - Attributes: Name, Kind (type/member), Summary, Parameters, Returns, Exceptions, Remarks, SinceVersion, DeprecatedIn
  - Relationships: links to Topics, Examples

- Example
  - Description: Focused code sample.
  - Attributes: Title, Code, Language, Prerequisites, ExpectedOutput, Notes, Verified (CI)
  - Relationships: links to Topics, ReferenceItems

- Tutorial
  - Description: End-to-end guide combining multiple topics.
  - Attributes: Title, Steps, Prerequisites, Outcome, EstimatedTime
  - Relationships: links to Topics, Examples

- Navigation
  - Description: Sitemap and table of contents.
  - Attributes: TOC entries, Breadcrumbs, Stable URLs, Redirects

- ChangeLog
  - Description: Curated list of notable changes with links.
  - Attributes: Version, Date, Changes, Links

## Validation Rules
- Every ReferenceItem must have a Summary and at least one linkage (Topic or Example).
- Every Example must list prerequisites and expected outcome.
- Every Topic must answer “What,” “How,” and “Why.”
- Stable URLs must be maintained; moved pages require redirects.
- Introduced/Deprecated metadata should be present where applicable.
