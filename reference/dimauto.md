# Dim.Auto Deep Dive

This is an offline conversion of the "Dim.Auto" deep dive from the Terminal.Gui docs.

## Table of Contents

- Overview
- Using Dim.Auto
  - Specifying minimum/maximum
  - API
- Technical details
  - Calculation logic
  - Handling subviews
  - Adornments
- Limitations
- Behavior of other Pos/Dim types
- Building Dim.Auto friendly views
  - Best practices
- Debugging Dim.Auto issues
- See Also

---

# Overview

The `Dim.Auto` type is a specialized `Dim` class in Terminal.Gui v2 that enables automatic sizing of a `View` based on its content. It calculates dimensions at runtime using one of the `DimAutoStyle` strategies (`Text`, `Content`, or `Auto`). `Dim.Auto` is typically used for `Width` or `Height` and can be combined with other `Dim` types.

- `Text`: size based on `Text` and `TextFormatter` settings
- `Content`: size based on `View.GetContentSize()` or largest subview plus position
- `Auto`: takes the larger of `Text` and `Content`

## Using Dim.Auto

### Specifying a minimum size

You can provide a `minimumContentDim` (a `Dim` or numeric value) to ensure a view does not shrink below a threshold.

```csharp
View view = new ()
{
    Text = "Hello, World!",
    Width = Dim.Auto(DimAutoStyle.Text, minimumContentDim: Dim.Absolute(10)),
    Height = Dim.Auto(DimAutoStyle.Text),
};
```

You can use `Dim.Func` for dynamic minimums:

```csharp
View view = new ()
{
    Width = Dim.Auto(DimAutoStyle.Content, minimumContentDim: Dim.Func(GetDynamicMinSize)),
    Height = Dim.Auto(DimAutoStyle.Text),
};

int GetDynamicMinSize() => someDynamicInt;
```

### Specifying a maximum size

Use `maximumContentDim` to constrain growth (also a `Dim`):

```csharp
View dialog = new ()
{
    Width = Dim.Auto(DimAutoStyle.Content, maximumContentDim: Dim.Percent(90)),
    Height = Dim.Auto(DimAutoStyle.Content, maximumContentDim: Dim.Percent(90)),
};
```

### API

`Dim.Auto` signature:

```csharp
public static Dim Auto (DimAutoStyle style = DimAutoStyle.Auto, Dim minimumContentDim = null, Dim maximumContentDim = null)
```

Examples:

```csharp
View view = new ()
{
    Width = Dim.Auto(DimAutoStyle.Text),
    Height = Dim.Auto(DimAutoStyle.Text),
};

View view2 = new ()
{
    Width = Dim.Auto(DimAutoStyle.Content),
    Height = Dim.Auto(DimAutoStyle.Content),
};
view2.Add(new Label() { Text = "Hello, World!" });
```

## Technical details

### Calculation logic

- `Text` style: uses `TextFormatter` (ConstrainToWidth / ConstrainToHeight) and respects maximum dims.
- `Content` style: if `ContentSizeTracksViewport` is `false` and no subviews, uses explicit `GetContentSize()`; otherwise iterates subviews to compute required dimension based on positions and sizes.
- `Auto` style: computes both and uses the larger dimension.

Final size respects `minimumContentDim` and `maximumContentDim`, and adds adornment thickness (margin, border, padding) to compute the frame.

### Handling subviews

- Subviews are categorized by their `Pos`/`Dim` types to manage dependencies and avoid circular calculations.
- Absolute-positioned/ sized subviews are processed first; more complex dependencies (PosAnchorEnd, DimView) handled afterwards.

### Adornments consideration

Adornment thickness is added to computed content size so frame includes margin, border, and padding.

## Limitations

- Performance overhead for dynamic calculations with many subviews or complex text formatting.
- Not ideal for full-screen layouts (`Dim.Fill()` is better for that use case).
- Complex dependencies (subviews using `Dim.Auto`) can require multiple layout iterations.

## Behavior of other Pos/Dim types when used within a Dim.Auto-sized view

Summary table (impacts on Dim.Auto dimension):

- `PosAlign`: Yes
- `PosView`: Yes
- `PosCombine`: Yes (if includes impacting Pos)
- `PosAnchorEnd`: Yes
- `PosCenter`: No
- `PosPercent`: No (unless combined)
- `PosAbsolute`: Yes
- `PosFunc`: Yes (if function returns large value)
- `DimView`: Yes
- `DimCombine`: Yes (if includes impacting Dim)
- `DimFill`: No
- `DimPercent`: No
- `DimAuto`: Yes
- `DimAbsolute`: Yes
- `DimFunc`: Yes

## Building Dim.Auto friendly views

### Best practices

- Choose appropriate `DimAutoStyle` (`Text`, `Content`, `Auto`)
- Update content size via `SetContentSize()` or update `Text` when content changes
- Use `minimumContentDim` and `maximumContentDim` to constrain sizing
- Account for adornments in custom views

Example `LinearRange` usage (sized automatically):

```csharp
List<object> options = new() { "Option 1", "Option 2", "Option 3" };
LinearRange slider = new(options)
{
    Orientation = Orientation.Vertical,
    Type = SliderType.Multiple,
};
view.Add(slider);
```

## Debugging Dim.Auto issues

- Enable `ValidatePosDim` on the view to validate `Pos`/`Dim` settings at runtime
- Verify `ContentSizeTracksViewport` and call `SetContentSize()` when needed
- Inspect subview dependencies for Pos/Dim types that affect dimensions
- Check `TextFormatter` constraints for text-based sizing

## See Also

- Lexicon & Taxonomy: https://gui-cs.github.io/Terminal.Gui/docs/lexicon.html
- Layout Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/layout.html

---

Generated from: https://gui-cs.github.io/Terminal.Gui/docs/dimauto.html
