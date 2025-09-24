# Layout Concepts

Understanding positioning, sizing, and responsive design patterns in Terminal.Gui.Xaml applications.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Applies To**: All Terminal.Gui.Xaml applications

## What is Layout in Terminal.Gui.Xaml?

Layout in Terminal.Gui.Xaml defines how UI elements (views) are positioned and sized within their container. Unlike traditional GUI frameworks that use pixel-based positioning, Terminal.Gui works with character cells in a terminal window, making layout both simpler and more constrained.

## How Layout Works

### Coordinate System
Terminal.Gui uses a character-based coordinate system:
- **Origin (0,0)**: Top-left corner of the container
- **X-axis**: Horizontal position (columns), increases left to right
- **Y-axis**: Vertical position (rows), increases top to bottom
- **Bounds**: Rectangle defining position and size in character cells

### Layout Types

#### Absolute Layout
Position views at specific coordinates within their parent:

```xml
<Window>
    <Label Text="Hello" X="5" Y="2" Width="10" Height="1" />
    <Button Text="OK" X="5" Y="4" Width="8" Height="1" />
</Window>
```

#### Computed Layout
Use relative positioning with `Pos` and `Dim` helpers:

```xml
<Window>
    <Label Text="Name:" X="Pos.Left" Y="Pos.Top" />
    <TextField X="Pos.Right(nameLabel) + 1" Y="Pos.Top" Width="Dim.Fill(1)" />
</Window>
```

#### Container-Based Layout
Let container views manage child positioning:

```xml
<StackView Orientation="Vertical" Spacing="1">
    <Label Text="First item" />
    <Label Text="Second item" />
    <Button Text="Third item" />
</StackView>
```

### Layout Properties

| Property | Description | Example |
|----------|-------------|---------|
| `X`, `Y` | Absolute position | `X="5" Y="10"` |
| `Width`, `Height` | Fixed size | `Width="20" Height="3"` |
| `Pos.Left`, `Pos.Top` | Relative to parent | `X="Pos.Left"` |
| `Pos.Right`, `Pos.Bottom` | Relative to other views | `X="Pos.Right(button)"` |
| `Dim.Fill` | Expand to available space | `Width="Dim.Fill()"` |
| `Dim.Percent` | Percentage of parent | `Width="Dim.Percent(50)"` |

## Why Layout Matters

### Responsive Design
Good layout ensures your application works across different terminal sizes:

```xml
<!-- Adapts to terminal width -->
<Window>
    <MenuBar Width="Dim.Fill()" />
    <ContentView Y="1" Width="Dim.Fill()" Height="Dim.Fill(2)" />
    <StatusBar Y="Pos.Bottom()" Width="Dim.Fill()" Height="1" />
</Window>
```

### Accessibility
Proper layout improves keyboard navigation and screen reader support:
- Logical tab order follows visual layout
- Related controls grouped together
- Consistent spacing and alignment

### Maintainability
Declarative layout in XAML separates presentation from logic:
- Easy to modify without code changes
- Clear visual structure
- Reusable layout patterns

## Common Layout Patterns

### Form Layout
```xml
<TableView>
    <TableView.Columns>
        <Column Header="Field" />
        <Column Header="Value" />
    </TableView.Columns>
    <Row>
        <Label Text="Name:" />
        <TextField Name="nameField" />
    </Row>
    <Row>
        <Label Text="Email:" />
        <TextField Name="emailField" />
    </Row>
</TableView>
```

### Master-Detail Layout
```xml
<Window>
    <ListView Name="masterList" X="0" Y="0" Width="Dim.Percent(30)" Height="Dim.Fill()" />
    <FrameView Title="Details" X="Pos.Right(masterList)" Y="0" Width="Dim.Fill()" Height="Dim.Fill()">
        <TextView Name="detailView" Width="Dim.Fill(2)" Height="Dim.Fill(2)" />
    </FrameView>
</Window>
```

### Toolbar Layout
```xml
<Window>
    <ToolBar Y="0" Width="Dim.Fill()">
        <Button Text="New" />
        <Button Text="Open" />
        <Button Text="Save" />
        <Separator />
        <Button Text="Exit" />
    </ToolBar>
    <ContentView Y="1" Width="Dim.Fill()" Height="Dim.Fill(1)" />
</Window>
```

## See Also
- [Binding Concepts](binding.md) - Connect data to layout elements
- [Events Guide](events.md) - Handle layout-related events
- [Create a Window Guide](../guides/create-window.md) - Step-by-step window creation
