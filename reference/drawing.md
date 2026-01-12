# Drawing (Text, Lines, and Color)

This is an offline conversion of the "Drawing" deep dive from the Terminal.Gui docs.

## Table of Contents

- Drawing lifecycle
- Coordinate system
- Outputting text
- Formatted text
- Line drawing
- Clipping
- Cells & Attributes
- Color and VisualRole
- Schemes
- Text formatting
- Glyphs and LineCanvas
- Diagnostics
- Accessing application drawing context

---

# Drawing (Text, Lines, and Color)

Terminal.Gui provides APIs for formatting text, line drawing, and character-based graphing.

## Drawing Taxonomy & Lexicon

- `Attribute` — Defines visual styling (foreground, background, text style)
- `BackgroundColor` — Attribute background color
- `Color` — Terminal color (supports TrueColor and named values)
- `ForegroundColor` — Attribute foreground color
- `Glyph` — Graphical representation of a character
- `Rune` — Unicode character
- `Scheme` — Maps `VisualRole` to `Attribute`
- `Style` — Font-like hints (bold, italic, underline)
- `VisualRole` — Semantic role for visual elements (Normal, Focus, Active, Disabled, ReadOnly)


## Drawing Lifecycle

Each MainLoop iteration performs:

1. Layout — Views needing layout are measured and positioned
2. Draw — Views needing drawing update the driver's back buffer
3. Write — Driver writes changed portions of the back buffer to the terminal
4. Cursor — Driver positions cursor with correct visibility

Notes:

- Drawing is deferred to MainLoop iterations. Calling `View.Draw()` updates the driver's back buffer but doesn't immediately write to the terminal; the driver `Refresh()` writes to the terminal (done automatically in MainLoop).
- `Application.LayoutAndDraw()` forces immediate layout and draw (useful for tests).

## Coordinate System for Drawing

Draw APIs use viewport-relative coordinates; `0,0` is the top-left visible cell.

See the Layout Deep Dive for more details: https://gui-cs.github.io/Terminal.Gui/docs/layout.html

## Outputting unformatted text

1. Move draw cursor: `View.Move(int x, int y)`
2. Set attribute: `View.SetAttribute(Terminal.Gui.Drawing.Attribute)`
3. Output glyphs: `View.AddRune(Rune)` or `View.AddStr(string)`

## Outputting formatted text

Use `Text.TextFormatter`:

1. Add text to a `TextFormatter` object
2. Set formatting options (alignment, wrap)
3. Call `TextFormatter.Draw(...)` to render through the driver

## Line drawing

- Use `Drawing.LineCanvas` to add lines and shapes
- Render via `LineCanvas.GetMap` or let `View` render it automatically (enables automatic line joining across views)

## When drawing occurs

- Call `View.SetNeedsDraw()` when content changes
- Call `View.SetNeedsLayout()` when viewport size changes
- MainLoop will check `NeedsDraw`/`SubViewNeedsDraw` and execute draw steps

Detailed draw steps per view during MainLoop (high level):

0. Check if `NeedsDraw`/`SubViewNeedsDraw` is set
1. Set clip to view's Frame
2. Draw `Border` and `Padding` (not Margin)
3. Set clip to view's Viewport
4. Set Normal color scheme
5. Call Draw on SubViews
6. Draw `Text`
7. Draw non-text content
8. Restore clip to Frame
9. Draw `LineCanvas`
10. Draw `Border` and other adornments
11. Cache clip for Margin rendering
12. Raise `DrawComplete`
13. Exclude Frame (not Margin) from current clip

Margins are rendered in a second pass using the cached clip so they can be transparent.

## Clipping

- Terminal.Gui supports non-rectangular clip regions via `Drawing.Region`.
- Use `View.SetClipToScreen`, `View.SetClip(Region)`, or `View.SetClipToFrame` to modify clip behavior.

## Cell

- `Drawing.Cell` represents a single screen cell (a `Rune` and an `Attribute`).
- Drivers manage the `Cell` array representing the screen.
- To draw a `Cell`: `View.Move(row, col)` then `View.AddRune(rune)`.

## Attribute

- `Drawing.Attribute` represents formatting attributes (foreground/background colors, text style).
- Set attributes with `View.SetAttribute(Attribute)` or `View.SetAttributeForRole(VisualRole)`.

Example:

```csharp
SetAttribute(new Attribute(Color.Red, Color.Black, Style.Underline));
AddStr("Red on Black Underlined.");
```

Or use scheme roles:

```csharp
SetScheme(new Scheme(Scheme) { Focus = new Attribute(Color.Red, Color.Black, Style.Underline) });
SetAttributeForRole(VisualRole.Focus);
AddStr("Red on Black Underlined.");
```

## Color

- Terminal.Gui supports true color via `Drawing.Color` (ARGB32).
- Standard W3C color names and classic terminal colors are supported (`StandardColor` enum).
- `Color.TryParse(string, out Color)` can parse color names.

### Alpha and transparency

- Alpha channel exists but terminals don't support alpha blending; alpha is used to indicate render/no-render and future features.
- Alpha is ignored for color name matching (opaque and semi-transparent map to same name).

### Legacy 16-color support

- For compatibility, Terminal.Gui can map true colors to 16-color equivalents when needed (`Application.Force16Colors`).

## VisualRole

`VisualRole` maps semantic roles to attributes (Normal, Focus, HotFocus, Active, Highlight, Disabled, Editable, ReadOnly, etc.).

## Schemes

- `Scheme` maps `VisualRole` to `Attribute` for how a View should look.
- `SchemeManager.Schemes` stores named schemes (Base, Dialog, Error, Menu, TopLevel).
- `ConfigurationManager` can override defaults and add schemes.

## Text Formatting

`Text.TextFormatter` supports:

- Horizontal alignment (Left, Center, Right)
- Vertical alignment (Top, Middle, Bottom)
- Word wrap
- Formatting hot keys

## Glyphs

- `Drawing.Glyphs` defines glyphs used for checkboxes, lines, borders, etc.
- Glyphs can be changed per theme scope via `ConfigurationManager`.

## Line Drawing

- `LineCanvas` auto-joins lines using appropriate box-drawing glyphs for intersections.

## Thickness

- `Drawing.Thickness` describes frame thickness per side; used by adornments.

## Diagnostics

- `View.Diagnostics.DrawIndicator` can animate a glyph in the `Border` when a View is redrawn.

## Accessing Application Drawing Context

Views can access drawing context via `View.App` and check driver capabilities (e.g., `SupportsTrueColor`) to choose drawing strategies.

Example custom view using application context:

```csharp
public class CustomView : View
{
    protected override bool OnDrawingContent()
    {
        if (App?.Driver?.SupportsTrueColor == true)
        {
            SetAttribute(new Attribute(Color.FromRgb(255, 0, 0), Color.FromRgb(0, 0, 255)));
        }
        else
        {
            SetAttributeForRole(VisualRole.Normal);
        }
        AddStr("Custom drawing with application context");
        return true;
    }
}
```

---

Generated from: https://gui-cs.github.io/Terminal.Gui/docs/drawing.html
