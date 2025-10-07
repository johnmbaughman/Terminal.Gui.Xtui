# Hosting & Search Configuration

Status: Stable

This guide explains how the Terminal.Gui.Xaml documentation site is built, hosted, and how search is customized.

## Hosting

The documentation is deployed automatically to **GitHub Pages** from the `main` branch.

Workflow: `.github/workflows/documentation.yml`

Key steps:
1. Restore and build the solution (Release).
2. Install DocFX as a .NET global tool.
3. Run `docfx build` inside the `docs/` directory.
4. Publish the generated `_site/` directory as a GitHub Pages artifact.
5. Deploy via the `deploy-pages` action.

To preview locally:
```pwsh
pwsh -c "cd docs; docfx build; docfx serve"
```

## URL Structure

DocFX emits clean HTML pages; future customizations can add canonical tags or sitemap if external indexing is required.

## Search Enhancements

DocFX provides a client-side search (Lunr-based index). This project layers a lightweight enhancement to reduce noise:

- A curated stopwords list: `docs/search-stopwords.json`.
- A script: `templates/terminal-gui-xaml/scripts/search-enhancements.js` that loads stopwords and avoids weighting queries consisting solely of common or domain-repetitive terms.

This script currently does not rewrite the visible query (to avoid confusing users) but stores a cleaned version in a data attribute for potential future enhancements.

### Extending Search Further

Potential future improvements:
- Integrate semantic scoring by section weight.
- Add synonyms mapping (e.g., "dialog" → "window").
- Expose filtering (guides vs API vs examples) via UI toggle.

## Adding New Content

When adding new conceptual pages:
1. Create file under `docs/articles/...`.
2. Update `docs/toc.yml` if a new navigation entry is needed.
3. Avoid linking to non-existent anchors; create headings first.
4. If page is planned but not written, add a placeholder with `Status: Planned — placeholder`.

## Deployment Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Pages deploy fails | Artifact missing | Ensure `docfx build` produced `_site/` and no step failed earlier. |
| 404 on GitHub Pages | Cache stale | Hard refresh / purge browser cache. |
| Search returns no results | Index not built | Check for `index.json` in `_site/`. Re-run build. |
| Many InvalidFileLink warnings | Missing placeholders | Create minimal placeholder pages for future links. |

## Maintenance Checklist

- Monthly: Review stopwords list for accidental over-filtering.
- Quarterly: Audit warnings count; ensure trend is downward.
- Each Release: Verify version & compatibility matrix links resolve.

## Related Documents

- Version & Compatibility Matrix: `docs/articles/guides/version-compatibility.md`
- Documentation Hygiene: See README section "Documentation Hygiene".
