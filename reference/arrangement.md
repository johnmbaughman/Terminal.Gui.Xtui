# View Arrangement Deep Dive

This is an offline conversion of the "View Arrangement" deep dive from the Terminal.Gui docs.

## Table of Contents

- Overview
- Arrangement Modes
- Arrange Mode (Interactive)
- Tiled vs Overlapped Layouts
- Movable Views
- Resizable Views
- Creating Resizable Splitters
- Modal Views
- Runnable Views
- Examples
- Advanced Topics
- See Also

---

# View Arrangement Deep Dive

Terminal.Gui provides an Arrangement system that enables users to interactively move and resize views using the keyboard and mouse. The system supports both Tiled and Overlapped layout modes.

See the Layout Deep Dive for the broader layout system: https://gui-cs.github.io/Terminal.Gui/docs/layout.html

## Overview

### Arrangement Lexicon

- Arrange Mode — Interactive mode (default key `Ctrl+F5`) displaying indicators on arrangeable views for keyboard-based arrangement.
- Arrangement — Layout feature controlling how users can arrange views (Tiled or Overlapped).
- Modal — A view run via `Application.Run` where `Modal == true` (constrained z-order).
- Movable — A View that can be moved by the user (`ViewArrangement.Movable`).
- Overlapped — Layout where SubViews have overlapping Frames with Z-order determined by SubViews order (`ViewArrangement.Overlapped`).
- Resizable — A View that can be resized by the user (`ViewArrangement.Resizable`).
- Runnable — A view type run via `Application.Run(Toplevel)` with its own `RunState`.
- Tiled — Layout where SubViews typically do not overlap (default, `ViewArrangement.Fixed`).

### ViewArrangement Flags

The `ViewArrangement` enum supports these flags (combinable):

- `Fixed (0)` — View cannot be moved or resized (default)
- `Movable (1)` — View can be moved by the user
- `LeftResizable (2)` — Left edge resizable
- `RightResizable (4)` — Right edge resizable
- `TopResizable (8)` — Top edge resizable
- `BottomResizable (16)` — Bottom edge resizable
- `Resizable (30)` — All edges resizable
- `Overlapped (32)` — View overlaps other views (enables Z-order)

The `View.Arrangement` property controls arrangement behavior for a View.

## Arrangement Modes

### Fixed (Default)

Views with `ViewArrangement.Fixed` cannot be moved or resized by the user.

```csharp
var view = new View { Arrangement = ViewArrangement.Fixed }; // Default
```

### Movable

Views with `ViewArrangement.Movable` can be dragged or moved with keyboard.

```csharp
var window = new Window { Title = "Movable Window", Arrangement = ViewArrangement.Movable };
```

Interaction:

- Mouse: Drag the top Border
- Keyboard: Press `Ctrl+F5` to enter Arrange Mode, then use arrow keys

### Resizable

Views with `ViewArrangement.Resizable` can be resized by the user.

```csharp
var window = new Window { Title = "Resizable Window", Arrangement = ViewArrangement.Resizable };
```

Interaction:

- Mouse: Drag border edges
- Keyboard: Enter Arrange Mode and press `Tab` to cycle resize handles

### Movable and Resizable

Combine flags for both behaviors:

```csharp
var window = new Window { Title = "Movable and Resizable", Arrangement = ViewArrangement.Movable | ViewArrangement.Resizable };
```

Note: When both are set, Movable may take precedence over resizing certain edges.

### Individual Edge Resizing

```csharp
// Only bottom edge resizable
var view = new View { Arrangement = ViewArrangement.BottomResizable };

// Left and right edges resizable
var view2 = new View { Arrangement = ViewArrangement.LeftResizable | ViewArrangement.RightResizable };
```

## Arrange Mode (Interactive)

Enter with `Ctrl+F5` (configurable via `Application.Keyboard.ArrangeKey`).

Entering Arrange Mode:

1. Visual indicators appear on arrangeable views
2. Move indicator (`◊`) for Movable in top-left
3. Resize indicators appear; `Tab` cycles handles
4. Arrow keys move/resize
5. Press `Esc` or `Ctrl+F5` to exit

Arrange Mode indicators (glyphs):

- Movable: ◊ (top-left)
- Resizable: ⇲ (bottom-right)
- Left/Right: ↔ (edge centered)
- Top/Bottom: ↕ (edge centered)

Keyboard controls:

- Arrow Keys — Move/resize
- Tab / Shift+Tab — Cycle modes
- Esc / Ctrl+F5 — Exit

Requirements for arrangement:

1. Must be part of a SuperView
2. Position/size must be independent of other SubViews
3. Must have `View.Arrangement` flags set
4. Typically needs a Border for mouse interaction

## Tiled vs Overlapped Layouts

### Tiled Layout

SubViews typically do not overlap; no Z-order. Uses `Pos` and `Dim` for positioning. Example:

```csharp
var container = new View { Arrangement = ViewArrangement.Fixed };
var view1 = new View { X = 0, Y = 0, Width = 20, Height = 10 };
var view2 = new View { X = 21, Y = 0, Width = 20, Height = 10 };
container.Add(view1, view2);
```

### Overlapped Layout

SubViews overlap and Z-order determines stacking. Enable with `ViewArrangement.Overlapped`.

```csharp
var container = new View { Arrangement = ViewArrangement.Overlapped };
// Add overlapping windows with Movable | Overlapped
```

Characteristics:

- Z-order from `View.SubViews` ordering
- Later views appear above earlier
- Use `Ctrl+Tab` to switch overlapped views

## Movable Views

Enable movable:

```csharp
var window = new Window { Title = "Drag Me!", X = 10, Y = 5, Width = 40, Height = 15, Arrangement = ViewArrangement.Movable, BorderStyle = LineStyle.Single };
```

Moving with mouse: drag top Border. With keyboard: enter Arrange Mode and use arrows.

## Resizable Views

All-edges resizable:

```csharp
var window = new Window { Title = "Resize Me!", Arrangement = ViewArrangement.Resizable, BorderStyle = LineStyle.Single };
```

Specific-edge example:

```csharp
var view = new View { Arrangement = ViewArrangement.RightResizable | ViewArrangement.BottomResizable, BorderStyle = LineStyle.Single };
```

Mouse resizing: drag enabled edges. Keyboard: Arrange Mode → `Tab` → arrow keys.

## Creating Resizable Splitters

Horizontal splitter (left/right panes):

```csharp
View leftPane = new () { X = 0, Y = 0, Width = Dim.Fill(Dim.Func(_ => rightPane.Frame.Width)), Height = Dim.Fill(), BorderStyle = LineStyle.Single };

View rightPane = new () { X = Pos.Right(leftPane) - 1, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), Arrangement = ViewArrangement.LeftResizable, BorderStyle = LineStyle.Single, SuperViewRendersLineCanvas = true };
rightPane.Border.Thickness = new Thickness(1, 0, 0, 0);

container.Add(leftPane, rightPane);
```

Vertical splitter (top/bottom) similar, using `TopResizable` on bottom pane and `Dim.Fill(Dim.Func(...))` for height.

## Modal Views

A view is modal when run via `Application.Run` and `Runnable.Modal = true`.

Modal characteristics:

- Exclusive input
- Constrained Z-order
- Blocks execution until `Application.RequestStop()`
- Own `RunState`

Modal types: `Dialog`, `MessageBox`, `Wizard`.

Modal example:

```csharp
var dialog = new Dialog { Title = "Confirm", Width = 40, Height = 10 };
var label = new Label { Text = "Are you sure?", X = Pos.Center(), Y = 2 };
dialog.Add(label);
var ok = new Button { Text = "OK" };
ok.Accepting += (s, e) => Application.RequestStop();
dialog.AddButton(ok);
Application.Run(dialog);
```

## Runnable Views

Runnable views run via `Application.Run` and have their own `RunState`. Non-modal runnables are possible.

## Examples

See examples for movable/resizable windows, splitters, overlapped windows, and custom arrange key samples.

## Advanced Topics

### Constraints and Limitations

Arrangement requires a SuperView, independent position/size (complex `Dim`/`Pos` may prevent arrangement), and typically a Border for mouse arrangement.

### SuperViewRendersLineCanvas

Set `SuperViewRendersLineCanvas = true` on panes when creating splitters to handle line intersections.

### Z-Order Management

Use `container.BringSubviewToFront(view)` / `SendSubviewToBack(view)` and inspect `container.SubViews` index to manage Z-order.

### Arrangement Events

Monitor `view.FrameChanged` and `view.LayoutComplete` to react to moves/resizes.

## See Also

- [Layout Deep Dive](https://gui-cs.github.io/Terminal.Gui/docs/layout.html)
- [View Deep Dive](https://gui-cs.github.io/Terminal.Gui/docs/View.html)
- [Multitasking Deep Dive](https://gui-cs.github.io/Terminal.Gui/docs/multitasking.html)
- [Drawing Deep Dive](https://gui-cs.github.io/Terminal.Gui/docs/drawing.html)

---

Generated from: https://gui-cs.github.io/Terminal.Gui/docs/arrangement.html
