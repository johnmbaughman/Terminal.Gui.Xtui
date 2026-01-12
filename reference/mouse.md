# Mouse Deep Dive

## Quick Reference

### The Pipeline (TL;DR)

```
ANSI Input → AnsiMouseParser → MouseInterpreter → MouseImpl → View → Commands
   (1-based)     (0-based screen)   (click synthesis)   (routing)  (viewport)  (Activate/Accept)

```

### Pipeline Stages

```
ANSI -> Parser -> Interpreter -> MouseImpl -> View -> Commands
```

### Coordinate Systems

```
ANSI   : 1-based, (1,1) = top-left
Screen : 0-based, (0,0) = top-left of terminal
Viewport: 0-based, relative to view's content area
```

### Common Patterns

Handle mouse clicks:

```csharp
view.Activating += (s, e) =>
{
    if (e.Context is CommandContext<MouseBinding> { Binding.MouseEventArgs: { } mouse })
    {
        Point position = mouse.Position;  // Viewport-relative
        HandleClick(position);
        e.Handled = true;
    }
};
```

Enable visual feedback and auto-grab:

```csharp
view.MouseHighlightStates = MouseState.In | MouseState.Pressed;
```

Continuous button press (scrollbar arrows, spin buttons):

```csharp
view.MouseHoldRepeat = MouseFlags.LeftButtonReleased;
view.Activating += (s, e) => { DoRepeatAction(); e.Handled = true; };
```

## Tenets for Terminal.Gui Mouse Handling

- Keyboard Required; Mouse Optional — ensure keyboard parity.
- Be Consistent With the User's Platform — follow platform conventions (e.g., right-click for context menus on Windows).

## Mouse Behavior — End User's Perspective

### Button Behavior

- Single click: press + release inside → standard click.
- Hold with `MouseHoldRepeat = false`: press → stay → release → one accept on release.
- Hold with `MouseHoldRepeat = true`: repeated accepts while held (~500ms initial, ~50ms intervals) + one final on release.
- Drag outside → release outside: canceled (no accept).
- Double-click: two separate accepts (or selection behavior depending on control).

### ListView Behavior

- Single click: item selected on click.
- Double-click: typical activate behavior; first click selects, second activates.

## Mouse APIs

Terminal.Gui provides:

- Mouse Bindings (`MouseBindings`) — declarative mapping of `MouseFlags` to `Command`.
- Mouse Events — direct `MouseEvent` handling for complex scenarios.
- Mouse State — `MouseState` property for visual feedback.
- `Terminal.Gui.Input.Mouse` abstraction for platform-independent events.

## Mouse Bindings

Default bindings (examples):

```csharp
MouseBindings.Add (MouseFlags.LeftButtonPressed, Command.Activate);
MouseBindings.Add (MouseFlags.LeftButtonPressed | MouseFlags.Ctrl, Command.Context);
```

Recommended pattern: use `AddCommand` with `MouseBindings` to wire view commands to mouse actions.

Example:

```csharp
public class MyView : View
{
    public MyView()
    {
        AddCommand (Command.ScrollUp, () => ScrollVertical (-1));
        MouseBindings.Add (MouseFlags.WheelUp, Command.ScrollUp);

        AddCommand (Command.ScrollDown, () => ScrollVertical (1));
        MouseBindings.Add (MouseFlags.WheelDown, Command.ScrollDown);

        AddCommand (Command.Activate, () => {
            SelectItem();
            return true;
        });
    }
}
```

## Mouse Events

Mouse events follow the Cancellable Work Pattern. High-level flow:

1. Driver Level: platform-specific events → `Mouse`
2. Application Level: `IMouse.RaiseMouseEvent` routes event to target view
3. View Level: `View.NewMouseEvent()` performs validation, grab handling, low-level `MouseEvent`, and command invocation via `MouseBindings`.

### Handling Mouse Events Directly

```csharp
public class CustomView : View
{
    public CustomView()
    {
        MouseEvent += OnMouseEventHandler;
    }
    
    private void OnMouseEventHandler(object sender, Mouse e)
    {
        if (e.Flags.HasFlag(MouseFlags.LeftButtonPressed))
        {
            // Handle drag start
            e.Handled = true;
        }
    }
    
    protected override bool OnMouseEvent(Mouse mouse)
    {
        if (mouse.Flags.HasFlag(MouseFlags.LeftButtonPressed))
        {
            return true; // Handled
        }
        return base.OnMouseEvent(mouse);
    }
}
```

### Handling Mouse Clicks (recommended)

Use `Activating` with `CommandContext<MouseBinding>` to get `Mouse` info:

```csharp
public class ClickableView : View
{
    public ClickableView()
    {
        Activating += OnActivating;
    }
    
    private void OnActivating(object sender, CommandEventArgs e)
    {
        if (e.Context is CommandContext<MouseBinding> { Binding.MouseEventArgs: { } mouse })
        {
            Point clickPosition = mouse.Position; // Viewport-relative
            if (mouse.Flags.HasFlag(MouseFlags.LeftButtonPressed))
            {
                HandleLeftClick(clickPosition);
            }
            else if (mouse.Flags.HasFlag(MouseFlags.RightButtonPressed))
            {
                ShowContextMenu(clickPosition);
            }
            e.Handled = true;
        }
    }
}
```

To customize bindings:

```csharp
MouseBindings.Clear();
MouseBindings.Add(MouseFlags.LeftButtonPressed, Command.Activate);
MouseBindings.Add(MouseFlags.RightButtonPressed, Command.Context);

AddCommand(Command.Context, HandleContextMenu);
```

## Mouse State and Mouse Grab

### MouseState

`MouseState` values: `None`, `In`, `Pressed`, `PressedOutside`.

Configure highlight states:

```csharp
view.MouseHighlightStates = MouseState.In | MouseState.Pressed;

view.MouseStateChanged += (sender, e) => 
{
    switch (e.Value)
    {
        case MouseState.In:
            // Hover appearance
            break;
        case MouseState.Pressed:
            // Pressed appearance
            break;
    }
};
```

### Mouse Grab

Views with `MouseHighlightStates` or `MouseHoldRepeat` will auto-grab the mouse on press.
Grab lifecycle:

1. Press inside → Auto-grab, set focus if `CanFocus`, set `MouseState |= Pressed`
2. Move outside → `MouseState |= PressedOutside` (unless `MouseHoldRepeat`)
3. Release → Ungrab and invoke commands if inside

Grabbed view receives all mouse events (even outside viewport) with coordinates converted to viewport-relative, and `mouse.View` set to the grabbed view.

## Coordinate Systems

Use conversion helpers:

```csharp
Point viewportPos = view.ScreenToViewport(screenPos);
Point screenPos = view.ViewportToScreen(viewportPos);

Point contentPos = view.ScreenToContent(screenPos);
Point screenFromContent = view.ContentToScreen(contentPos);

Point framePos = view.ScreenToFrame(screenPos);
Rectangle screenRect = view.FrameToScreen();
```

Mouse coordinate summary:

- `Mouse.ScreenPosition` — screen (terminal) coords (0,0 top-left)
- `Mouse.Position` — viewport-relative coords (0,0 top-left of content area)

## Complete Mouse Event Pipeline

### Stage 1: Terminal Input (ANSI Escape Sequences)

Input Format: SGR Extended Mouse Mode (`ESC[<button;x;yM/m`)

Example — single click at column 10, row 5:

```
Press:   ESC[<0;10;5M
Release: ESC[<0;10;5m
```

Key points: ANSI coordinates are 1-based; `M` = press, `m` = release; button codes and modifiers encode button and modifiers.

### Stage 2: ANSI Parsing (AnsiMouseParser)

Responsibilities:

1. Parse the sequence
2. Extract button/x/y/terminator
3. Convert to 0-based coordinates
4. Map to `MouseFlags`
5. Create `Mouse` instance

### Stage 3: Click Synthesis (MouseInterpreter)

Tracks press/release pairs to synthesize click events and detect multi-clicks based on timing and proximity.

### Stage 4: Application Routing (MouseImpl)

Find target view:

```csharp
List<View?> viewsUnderMouse = App.TopRunnableView.GetViewsUnderLocation(
    mouse.ScreenPosition, 
    ViewportSettingsFlags.TransparentMouse
);
View? deepestView = viewsUnderMouse?.LastOrDefault();
```

Popover dismissal example:

```csharp
if (mouse.IsPressed && 
    App.Popover?.GetActivePopover() is {} popover &&
    !View.IsInHierarchy(popover, deepestView, true))
{
    ApplicationPopover.HideWithQuitCommand(popover);
    RaiseMouseEvent(mouse); // Recurse to handle event below popover
}
```

Mouse grab handling (forward to grabbed view):

```csharp
if (MouseGrabView is {})
{
    Point viewportLoc = MouseGrabView.ScreenToViewport(mouse.ScreenPosition);
    MouseGrabView.NewMouseEvent(new Mouse { 
        Position = viewportLoc, 
        ScreenPosition = mouse.ScreenPosition,
        View = MouseGrabView 
    });
}
```

Convert to view coordinates and send:

```csharp
Point viewportLocation = deepestView.ScreenToViewport(mouse.ScreenPosition);
Mouse viewMouseEvent = new() {
    Position = viewportLocation,
    Flags = mouse.Flags,
    ScreenPosition = mouse.ScreenPosition,
    View = deepestView
};

deepestView.NewMouseEvent(viewMouseEvent);
```

### Stage 5: View Processing (View.NewMouseEvent)

Pre-conditions:

```csharp
if (!Enabled) return false;
if (!CanBeVisible(this)) return false;
if (!MousePositionTracking && mouse.Flags == MouseFlags.PositionReport) 
    return false;
```

Low-level mouse handling and grab behavior occur here. On press, views may grab the mouse and set focus; on release they ungrab and synthesize click events. Command invocation uses `MouseBindings`.

Invoke commands example:

```csharp
if (MouseBindings.TryGet(mouse.Flags, out binding))
{
    binding.MouseEventArgs = mouse;
    InvokeCommands(binding.Commands, binding);
}
```

Default binding examples: `LeftButtonPressed` → `Command.Activate`.

## Driver Architecture

- Windows: `WindowsInputProcessor` uses `ReadConsoleInput()` and produces `Mouse` objects directly.
- Unix/ANSI: ANSI escape sequence parsing pipeline used.

Platform API → InputProcessorImpl → AnsiResponseParser → MouseInterpreter → Application

## Best Practices

- Use Mouse Bindings and Commands for simple interactions.
- Use `Activating` event to handle clicks and get `Mouse` via `CommandContext`.
- Use `MouseHighlightStates` for automatic grab and visual feedback.
- Use `MouseHoldRepeat` for repeating actions (e.g., scroll buttons).
- Respect platform conventions and provide keyboard alternatives.

## Testing Mouse Input

Quick test example (injecting mouse events):

```csharp
VirtualTimeProvider time = new();
using IApplication app = Application.Create(time);
app.Init(DriverRegistry.Names.ANSI);

// Inject click
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonPressed 
});
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonReleased 
});
```

Testing double-click with virtual time:

```csharp
VirtualTimeProvider time = new();
using IApplication app = Application.Create(time);
app.Init(DriverRegistry.Names.ANSI);

// First click
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonPressed,
    Timestamp = time.Now 
});
time.Advance(TimeSpan.FromMilliseconds(50));
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonReleased,
    Timestamp = time.Now 
});

// Second click within threshold
time.Advance(TimeSpan.FromMilliseconds(250));
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonPressed,
    Timestamp = time.Now 
});
time.Advance(TimeSpan.FromMilliseconds(50));
app.InjectMouse(new() { 
    ScreenPosition = new(10, 5), 
    Flags = MouseFlags.LeftButtonReleased,
    Timestamp = time.Now 
});
// Double-click detected!
```

Key testing features: virtual time control, single-call injection (`app.InjectMouse`), deterministic multi-click timing.

## Limitations and Considerations

- Terminal support varies — not all terminals support mouse input.
- Mouse wheel and additional buttons support varies by platform and terminal.
- Coordinate precision is limited to character cells.
- Excessive mouse move tracking can impact performance; prefer enter/leave events where possible.

## Global Mouse Handling

Handle mouse events application-wide before views:

```csharp
App.Mouse.MouseEvent += (sender, e) => 
{
    if (e.Flags.HasFlag(MouseFlags.RightButtonClicked))
    {
        ShowGlobalContextMenu(e.Position);
        e.Handled = true;
    }
};
```

## Mouse Enter/Leave Events

```csharp
view.MouseEnter += (sender, e) => 
{
    UpdateTooltip("Hovering");
};

view.MouseLeave += (sender, e) => 
{
    HideTooltip();
};
```

## See Also

- [Command System](https://gui-cs.github.io/Terminal.Gui/docs/command.html)
- [Input Injection](https://gui-cs.github.io/Terminal.Gui/docs/input-injection.html)
- [View Layout](https://gui-cs.github.io/Terminal.Gui/docs/layout.html)
- [Cancellable Work Pattern](https://gui-cs.github.io/Terminal.Gui/docs/cancellable-work-pattern.html)
