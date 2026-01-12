# View Deep Dive

This document is an offline conversion of the "View" deep dive from the Terminal.Gui docs.

## Table of Contents

- View Hierarchy
- View Composition
- Core Concepts
- View Lifecycle
- Subsystems
- Common View Patterns
- Modal Views
- Advanced Topics
- See Also

---

# View Deep Dive

`View` is the base class for all visible UI elements in Terminal.Gui. View provides core functionality for layout, drawing, input handling, navigation, and scrolling. All interactive controls, windows, and dialogs derive from `View`.

See the Views Overview for a catalog of all built-in View subclasses: https://gui-cs.github.io/Terminal.Gui/docs/views.html

## View Hierarchy

### Terminology

- `View` - The base class for all visible UI elements
- SubView - A View that is contained in another View and rendered as part of the containing View's content area. SubViews are added via `View.Add`.
- SuperView - The View that contains SubViews. Each View has a `View.SuperView` property that references its container.
- Child View - A view that holds a reference to another view in a parent/child relationship (used sparingly; generally SubView/SuperView is preferred)
- Parent View - A view that holds a reference to another view but is NOT a SuperView (used sparingly)

### Key Properties

- `View.SubViews` - Read-only list of all SubViews added to this View
- `View.SuperView` - The View's container (null if the View has no container)
- `View.Id` - Unique identifier for the View (should be unique among siblings)
- `View.Data` - Arbitrary data attached to the View
- `View.App` - The application context this View belongs to
- `View.Driver` - The driver used for rendering (shortcut to `App.Driver`)


## View Composition

Views are composed of several nested layers that define how they are positioned, drawn, and scrolled.

### The Layers

1. `Frame` - The outermost rectangle defining the View's location and size relative to the SuperView's content area
2. `Margin` - Adornment that provides spacing between the View and other SubViews
3. `Border` - Adornment that draws the visual border and title
4. `Padding` - Adornment that provides spacing between the border and the viewport
5. `Viewport` - Rectangle describing the visible portion of the content area
6. `Content Area` - The total area where content can be drawn (defined by `View.GetContentSize`)

See the Layout Deep Dive for complete details on View composition and layout: https://gui-cs.github.io/Terminal.Gui/docs/layout.html

## Core Concepts

### Frame vs. Viewport

- Frame - The View's location and size in SuperView-relative coordinates. Frame includes all adornments (Margin, Border, Padding)
- Viewport - The visible "window" into the View's content, located inside the adornments. Viewport coordinates are always relative to (0,0) of the content area.

```csharp
// Frame is SuperView-relative
view.Frame = new Rectangle(10, 5, 50, 20);

// Viewport is content-relative (the visible portal)
view.Viewport = new Rectangle(0, 0, 45, 15); // Adjusted for adornments
```

### Content Area and Scrolling

The Content Area is where the View's content is drawn. By default, the content area size matches the Viewport size. To enable scrolling:

1. Call `View.SetContentSize` with a size larger than the Viewport
2. Change `Viewport.Location` to scroll the content

See the Scrolling Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/scrolling.html

### Adornments

Adornments are special Views that surround the content:

- `Margin` - Transparent spacing outside the Border
- `Border` - Visual frame with LineStyle, title, and arrangement UI
- `Padding` - Spacing inside the Border, outside the Viewport

Each adornment has a `Thickness` that defines the width of each side (Top, Right, Bottom, Left).


## View Lifecycle

Views implement `ISupportInitializeNotification`.

### Initialization

1. Constructor - Creates the View and sets up default state
2. `BeginInit` - Signals initialization is starting
3. `EndInit` - Signals initialization is complete; raises `View.Initialized` event
4. `IsInitialized` - Property indicating if initialization is complete

During initialization, `View.App` is set to reference the application context, enabling views to access application services like the driver and current session.

### Disposal

Views are `IDisposable`:

- Call `View.Dispose()` to clean up resources
- The `View.Disposing` event is raised when disposal begins
- Automatically disposes SubViews, adornments, and scroll bars

### Layout

Layout happens automatically when needed:

1. `View.SetNeedsLayout` marks View as needing layout
2. `View.Layout` calculates position and size
3. `LayoutStarted` event is raised
4. Frame and Viewport are calculated based on X, Y, Width, Height
5. SubViews are laid out
6. `LayoutComplete` event is raised

### Drawing

Drawing happens automatically when needed:

1. `View.SetNeedsDraw` marks View as needing redraw
2. `View.Draw` renders the View
3. `DrawingContent` event is raised
4. `View.OnDrawingContent` is called (override to draw custom content)
5. `DrawingContentComplete` event is raised
6. Adornments and SubViews are drawn

Example drawing override:

```csharp
protected override bool OnDrawingContent()
{
    // Draw at viewport coordinates (0,0)
    Move(0, 0);
    SetAttribute(new Attribute(Color.White, Color.Blue));
    AddStr("Hello, Terminal.Gui!");
    
    return true;
}
```

### Input Processing Order

1. Keyboard: Key → KeyBindings → Command → Command Handlers → Events
2. Mouse: MouseEvent → MouseBindings → Command → Command Handlers → Events


## Subsystems

### Commands

- `View.AddCommand` - Declares commands the View supports
- `View.InvokeCommand` - Invokes a command

See Command Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/command.html

### Input Handling

#### Keyboard
- `View.KeyBindings` maps keys to Commands
- `View.HotKey` the hot key for the View
- Events: `KeyDown`, `InvokingKeyBindings`

#### Mouse
- `View.MouseBindings` maps mouse events to Commands
- Events: `MouseEnter`, `MouseLeave`, `MouseEvent`

### Layout and Arrangement

- `View.X`, `View.Y` - Position using `Pos`
- `View.Width`, `View.Height` - Size using `Dim`
- Layout features: `Dim.Auto`, `Pos.AnchorEnd`, `Pos.Align`, `Dim.Fill`
- `View.Arrangement` controls move/resize behavior
- Events: `LayoutStarted`, `LayoutComplete`, `FrameChanged`, `ViewportChanged`

### Drawing

- `View.Draw`, `View.AddRune`, `View.AddStr`, `View.Move`, `View.Clear`

WARNING: `Move()` sets the Draw Cursor (where next character renders), NOT the Terminal Cursor (visible cursor indicator). To position the Terminal Cursor, use the `View.Cursor` property. See Cursor Management: https://gui-cs.github.io/Terminal.Gui/docs/cursor.html

### Navigation

- `View.CanFocus`, `View.HasFocus`, `View.TabStop`, `View.TabIndex` and `View.SetFocus`
- Events: `HasFocusChanging`, `HasFocusChanged`, `Accepting`, `Accepted`, `Activating`, `Activated`

### Scrolling

- `View.Viewport`, `View.GetContentSize`, `View.SetContentSize`
- `View.ScrollVertical`, `View.ScrollHorizontal`
- Built-in scrollbars: `View.VerticalScrollBar`, `View.HorizontalScrollBar`

### Text

- `View.Text`, `View.Title`, `View.TextFormatter`, `View.TextDirection`, `View.TextAlignment`, `View.VerticalTextAlignment`


## Common View Patterns

### Creating a Custom View

```csharp
public class MyCustomView : View
{
    public MyCustomView()
    {
        // Set up default size
        Width = Dim.Auto();
        Height = Dim.Auto();
        
        // Can receive focus
        CanFocus = true;
        
        // Add supported commands
        AddCommand(Command.Accept, HandleAccept);
        
        // Configure key bindings
        KeyBindings.Add(Key.Enter, Command.Accept);
    }
    
    protected override bool OnDrawingContent()
    {
        // Draw custom content using viewport coordinates
        Move(0, 0);
        SetAttributeForRole(VisualRole.Normal);
        AddStr("My custom content");
        
        return true; // Handled
    }
    
    private bool HandleAccept()
    {
        // Handle the Accept command
        return true; // Handled
    }
}
```

### Adding SubViews

```csharp
var container = new View
{
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

var LeftButton = new Button { Text = "OK", X = 2, Y = 2 };
var MiddleButton = new Button { Text = "Cancel", X = Pos.Right(LeftButton) + 2, Y = 2 };

container.Add(LeftButton, MiddleButton);
```

### Using Adornments

```csharp
var view = new View
{
    BorderStyle = LineStyle.Double,
    Title = "My View"
};

// Configure border
view.Border.Thickness = new Thickness(1);
view.Border.Settings = BorderSettings.Title;

// Add padding
view.Padding.Thickness = new Thickness(1);

// Add margin
view.Margin.Thickness = new Thickness(2);
```

### Implementing Scrolling

```csharp
var view = new View
{
    Width = 40,
    Height = 20
};

// Set content larger than viewport
view.SetContentSize(new Size(100, 100));

// Scroll the content
view.Viewport = view.Viewport with { Location = new Point(10, 10) };

// Enable scrollbars
view.VerticalScrollBar.AutoShow = true;
view.HorizontalScrollBar.AutoShow = true;

// Add key bindings for scrolling
view.KeyBindings.Add(Key.CursorUp, Command.ScrollUp);
view.KeyBindings.Add(Key.CursorDown, Command.ScrollDown);

// Add command handlers
view.AddCommand(Command.ScrollUp, () => { view.ScrollVertical(-1); return true; });
view.AddCommand(Command.ScrollDown, () => { view.ScrollVertical(1); return true; });
```


## Runnable Views (IRunnable)

The `IRunnable` pattern provides an interface-based runnable with typed results and fluent lifecycle methods.

### Creating a Runnable View

```csharp
public class ColorPickerDialog : Runnable<Color?>
{
    private ColorPicker16 _colorPicker;
    
    public ColorPickerDialog()
    {
        Title = "Select a Color";
        
        _colorPicker = new ColorPicker16 { X = Pos.Center(), Y = 2 };
        
        var okButton = new Button { Text = "OK", IsDefault = true };
        okButton.Accepting += (s, e) => {
            Result = _colorPicker.SelectedColor;
            Application.RequestStop();
        };
        
        Add(_colorPicker, okButton);
    }
}
```

### Running with Fluent API

```csharp
Color? result = Application.Create()
                           .Init()
                           .Run<ColorPickerDialog>()
                           .Shutdown() as Color?;
```

### Running with Explicit Control

```csharp
var app = Application.Create();
app.Init();

var dialog = new ColorPickerDialog();
app.Run(dialog);

// Extract result after Run returns
Color? result = dialog.Result;

// Caller is responsible for disposal
dialog.Dispose();

app.Shutdown();
```


## Modal Views (Legacy)

### Running a View Modally (Legacy)

```csharp
var dialog = new Dialog
{
    Title = "Confirmation",
    Width = Dim.Percent(50),
    Height = Dim.Percent(50)
};

var label = new Label { Text = "Are you sure?", X = Pos.Center(), Y = 1 };
dialog.Add(label);

aApplication.Run(dialog);

dialog.Dispose();
```

### Dialog Example (Legacy)

```csharp
bool okPressed = false;
var ok = new Button { Text = "Ok" };
ok.Accepting += (s, e) => { okPressed = true; Application.RequestStop(); };

var cancel = new Button { Text = "Cancel" };
cancel.Accepting += (s, e) => Application.RequestStop();

var dialog = new Dialog 
{ 
    Title = "Quit",
    Width = 50,
    Height = 10
};
dialog.Add(new Label { Text = "Are you sure you want to quit?", X = Pos.Center(), Y = 2 });
dialog.AddButton(ok);
dialog.AddButton(cancel);

Application.Run(dialog);

if (okPressed)
{
    // User clicked OK
}
```

Which displays an ASCII-ish dialog box when rendered.

### Wizard Example

```csharp
var wizard = new Wizard { Title = "Setup Wizard" };

var step1 = new WizardStep { Title = "Welcome" };
step1.Add(new Label { Text = "Welcome to the wizard!", X = 1, Y = 1 });

var step2 = new WizardStep { Title = "Configuration" };
step2.Add(new TextField { X = 1, Y = 1, Width = 30 });

wizard.AddStep(step1);
wizard.AddStep(step2);

Application.Run(wizard);
```


## Advanced Topics

### View Diagnostics

`View.Diagnostics` - `ViewDiagnosticFlags` for debugging like `Ruler`, `DrawIndicator`, `FramePadding`.

### View States

- `View.Enabled`, `View.Visible`, `View.CanFocus`, `View.HasFocus`.

### Shadow Effects

`View.ShadowStyle` controls drop shadows.


## See Also

- Application Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/application.html
- Views Overview: https://gui-cs.github.io/Terminal.Gui/docs/views.html
- Layout Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/layout.html
- Drawing Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/drawing.html
- Keyboard Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/keyboard.html
- Scrolling Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/scrolling.html
- Command Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/command.html

---

Generated from: https://gui-cs.github.io/Terminal.Gui/docs/View.html

