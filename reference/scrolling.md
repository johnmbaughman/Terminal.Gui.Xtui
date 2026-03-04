# Scrolling

Terminal.Gui provides a rich system for how `View` objects can scroll content with the keyboard and/or mouse.

## Lexicon & Taxonomy

- Content Area: The total area of content that can be scrolled, defined by `View.GetContentSize()`.
- Scroll: The act of causing content to move horizontally or vertically within the `View.Viewport`.
- ScrollBar: Visual control showing scrollable content size and position; includes buttons and a `ScrollSlider`.
- ScrollSlider: A draggable indicator showing the proportion of scrollable content relative to the `Viewport`.
- Viewport: The visible "portal" into the View's Content Area.
- ViewportSettings: Flags that adjust scrolling behavior (e.g., allowing negative coordinates).

## Overview

The ability to scroll content is built into `View`. The `Viewport` represents the scrollable portal into the View's Content Area (as returned by `GetContentSize()`).

By default, `View` does not include key bindings for directional keyboard or mouse input; to enable scrolling you must:

1. Make the `Viewport` size smaller than the `GetContentSize()` so content can scroll.
2. Create key bindings and call `ScrollHorizontal(int)` / `ScrollVertical(int)` as needed.
3. Subscribe to `MouseEvent` and call `ScrollHorizontal(int)` / `ScrollVertical(int)` as needed.
4. Enable the built-in scroll bars by setting `HorizontalScrollBar` or `VerticalScrollBar` visible or enable automatic show/hide (`ScrollBar.AutoShow`).

`ScrollBar` can be used standalone but is typically enabled via `View.HorizontalScrollBar` / `View.VerticalScrollBar`.

## Examples

Example scenarios in the UI Catalog demonstrating scrolling:

- Scrolling - demonstrates `ScrollBar` objects built into `View`.
- ScrollBar Demo - demonstrates `ScrollBar` as a standalone view.
- ViewportSettings - interactive demo of `ViewportSettingsFlags`.
- Character Map - complex scrolling scenario showing headers, keyboard/mouse support, and more.
- `ListView` and `HexEdit` - good reference implementations for reusable scrolling behavior.

## ViewportSettings

Use `ViewportSettings` to adjust scrolling behavior:

- `AllowNegativeX` / `AllowNegativeY` — permit viewport coordinates less than 0 to scroll beyond the top-left.
- `AllowXGreaterThanContentWidth` / `AllowYGreaterThanContentHeight` — permit viewport size greater than content size to scroll beyond bottom-right; otherwise the viewport location is constrained so the last column/row remains visible.
- `ClipContentOnly` — apply clipping only to the visible content area (viewport).
- `ClearContentOnly` — `ClearViewport` clears only the portion of content visible within the viewport; useful when content area is larger than viewport and outside area should appear distinct.

## See Also

- View Deep Dive
- Layout
- Arrangement

---

*Content converted from https://gui-cs.github.io/Terminal.Gui/docs/scrolling.html (main body).*