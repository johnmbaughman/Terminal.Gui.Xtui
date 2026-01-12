# Popovers Deep Dive

Popovers are transient UI elements that appear above other content to display contextual information, such as menus, tooltips, autocomplete suggestions, and dialog boxes. Terminal.Gui's popover system provides a flexible, non-modal way to present temporary UI without blocking the rest of the application.

## Overview

Normally, `View`s cannot draw outside of their `Viewport`. To display content that appears to "pop over" other views, Terminal.Gui provides the popover system via `Application.Popover`. Popovers differ from alternatives like modifying `Border` or `Margin` behavior because they:

- Are managed centrally by the application
- Support focus and keyboard event routing
- Automatically hide in response to user actions
- Can receive global hotkeys even when not visible

---

## Creating a Popover

### Using `PopoverMenu`

The easiest way to create a popover is to use `Terminal.Gui.Views.PopoverMenu`, which provides a cascading menu implementation:

```
// Create a popover menu with menu items
PopoverMenu contextMenu = new ([
    new MenuItem ("Cut", Command.Cut),
    new MenuItem ("Copy", Command.Copy),
    new MenuItem ("Paste", Command.Paste),
    new MenuItem ("Select All", Command.SelectAll)
]);

// IMPORTANT: Register before showing
Application.Popover?.Register (contextMenu);

// Show at mouse position or specific location
contextMenu.MakeVisible (); // Uses current mouse position
// OR
contextMenu.MakeVisible (new Point (10, 5)); // Specific location

```

### Creating a Custom Popover

Inherit from `Terminal.Gui.App.PopoverBaseImpl` for custom popovers:

```
public class MyCustomPopover : PopoverBaseImpl
{
    public MyCustomPopover ()
    {
        // PopoverBaseImpl already sets up required defaults:
        // - ViewportSettings with Transparent and TransparentMouse flags
        // - Command.Quit binding to hide the popover
        // - Width/Height set to Dim.Fill()
        
        // Add your custom content
        Label label = new () { Text = "Custom Popover Content" };
        Add (label);
        
        // Optionally override size
        Width = 40;
        Height = 10;
    }
}

// Usage:
MyCustomPopover myPopover = new ();
Application.Popover?.Register (myPopover);
Application.Popover?.Show (myPopover);

```

---

## Popover Requirements

A `View` qualifies as a popover if it:

1. Implements `Terminal.Gui.App.IPopover` (provides the `Current` property for runnable association)
2. Is focusable (`CanFocus = true`)
3. Is transparent — `ViewportSettings` includes `Transparent` and `TransparentMouse`
4. Handles Quit — binds `Application.QuitKey` to `Command.Quit` and sets `Visible = false`

`PopoverBaseImpl` provides these defaults.

---

## Registration and Lifecycle

### Registration (REQUIRED)

All popovers must be registered before they can be shown:

```
PopoverMenu popover = new ([...]);

// REQUIRED: Register with the application
Application.Popover?.Register (popover);

// Now you can show it
Application.Popover?.Show (popover);
// OR
popover.MakeVisible (); // For PopoverMenu

```

Registration enables keyboard routing, hotkeys when hidden, and lifecycle management.

### Showing and Hiding

Show a popover:

```
Application.Popover?.Show (popover);

```

Hide a popover:

```
// Method 1: Via ApplicationPopover
Application.Popover?.Hide (popover);

// Method 2: Set Visible property
popover.Visible = false;

// Automatic hiding occurs when:
// - User presses Application.QuitKey (typically Esc)
// - User clicks outside the popover (not on a subview)
// - Another popover is shown

```

### Lifecycle Management

Registered popovers have their lifetime managed by the application and are disposed on `Application.Shutdown()`. To manage lifetime manually, deregister and dispose yourself:

```
Application.Popover?.DeRegister (popover);
popover.Dispose ();

```

---

## Keyboard Event Routing

### Global Hotkeys

Registered popovers receive keyboard events even when not visible, enabling global hotkey support:

```
PopoverMenu menu = new ([...]);
menu.Key = Key.F10.WithShift; // Default hotkey

Application.Popover?.Register (menu);

// Now pressing Shift+F10 anywhere in the app will show the menu

```

### Runnable Association

`IPopover.Current` associates a popover with a specific `IRunnable`:

- If `null`: popover receives all application keyboard events
- If set: popover only receives events when the associated runnable is active
- Automatically set to `Application.TopRunnableView` during registration

```
// Associate with a specific runnable
myPopover.Current = myWindow; // Only active when myWindow is the top runnable

```

---

## Focus and Input

When visible:

- Popovers receive focus automatically
- All keyboard input goes to the popover until hidden
- Mouse clicks on subviews are captured
- Mouse clicks outside subviews pass through (due to `TransparentMouse`)

When hidden:

- Only registered hotkeys are processed
- Other keyboard input is not captured

---

## Layout and Positioning

### Default Layout

`PopoverBaseImpl` sets `Width = Dim.Fill()` and `Height = Dim.Fill()`, making the popover fill the screen by default. Transparent viewport settings allow content beneath to remain visible.

### Custom Sizing

Override `Width` and `Height` to customize size:

```
public class MyPopover : PopoverBaseImpl
{
    public MyPopover ()
    {
        Width = 40;  // Fixed width
        Height = Dim.Auto (); // Auto height based on content
    }
}

```

### Positioning with `PopoverMenu`

`PopoverMenu` provides positioning helpers:

```
// Position at specific screen coordinates
menu.SetPosition (new Point (10, 5));

// Show and position in one call
menu.MakeVisible (new Point (10, 5));

// Uses mouse position if null
menu.MakeVisible (); // Uses Application.Mouse.LastMousePosition

```

The menu automatically adjusts position to ensure it remains fully visible on screen.

---

## Built-in Popover Types

### PopoverMenu

`Terminal.Gui.Views.PopoverMenu` is a cascading menu implementation used for context menus, `MenuBar` drop-downs, and custom menu scenarios. Key features:

- Cascading submenus with automatic positioning
- Keyboard navigation (arrow keys, hotkeys)
- Automatic key binding from `Command`s
- Mouse support
- Separator lines via `new Line()`

Example with submenus:

```
PopoverMenu fileMenu = new ([
    new MenuItem ("New", Command.New),
    new MenuItem ("Open", Command.Open),
    new MenuItem {
        Title = "Recent",
        SubMenu = new Menu ([
            new MenuItem ("File1.txt", Command.Open),
            new MenuItem ("File2.txt", Command.Open)
        ])
    },
    new Line (),
    new MenuItem ("Exit", Command.Quit)
]);

Application.Popover?.Register (fileMenu);
fileMenu.MakeVisible ();

```

---

## Mouse Event Handling

Popovers use `ViewportSettings.TransparentMouse`, which means:

- Clicks on popover subviews: captured and handled normally
- Clicks outside subviews: pass through to views beneath
- Clicks on background: automatically hide the popover

---

## Best Practices

1. Always register first:

```
// WRONG - Will throw InvalidOperationException
PopoverMenu menu = new ([...]);
menu.MakeVisible ();

// CORRECT
PopoverMenu menu = new ([...]);
Application.Popover?.Register (menu);
menu.MakeVisible ();

```

2. Use `PopoverMenu` for menus — don't reinvent the wheel for standard menu scenarios.
3. Manage lifecycle appropriately — let the application manage disposal for long-lived popovers; deregister and dispose short-lived popovers.
4. Test global hotkeys for conflicts and consider configuration for custom hotkeys.
5. Handle edge cases: positioning near screen edges, multiple runnables, and keyboard-only navigation.

---

## Common Scenarios

### Context Menu on Right-Click

```
PopoverMenu contextMenu = new ([...]);
contextMenu.MouseFlags = MouseFlags.Button3Clicked; // Right-click
Application.Popover?.Register (contextMenu);

myView.MouseClick += (s, e) =>
{
    if (e.MouseEvent.Flags == MouseFlags.Button3Clicked)
    {
        contextMenu.MakeVisible (myView.ScreenToViewport (e.MouseEvent.Position));
        e.Handled = true;
    }
};

```

### Autocomplete Popup

```
public class AutocompletePopover : PopoverBaseImpl
{
    private ListView _listView;
    
    public AutocompletePopover ()
    {
        Width = 30;
        Height = 10;
        
        _listView = new ListView
        {
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        Add (_listView);
    }
    
    public void ShowSuggestions (IEnumerable<string> suggestions, Point position)
    {
        _listView.SetSource (suggestions.ToList ());
        // Position below the text entry field
        X = position.X;
        Y = position.Y + 1;
        Visible = true;
    }
}

```

### Global Command Palette

```
PopoverMenu commandPalette = new (GetAllCommands ());
commandPalette.Key = Key.P.WithCtrl; // Ctrl+P to show

Application.Popover?.Register (commandPalette);

// Now Ctrl+P anywhere in the app shows the command palette

```

---

## API Reference

- `Terminal.Gui.App.IPopover` - Interface for popover views
- `Terminal.Gui.App.PopoverBaseImpl` - Abstract base class for custom popovers
- `Terminal.Gui.Views.PopoverMenu` - Cascading menu implementation
- `Terminal.Gui.App.ApplicationPopover` - Popover manager (accessed via `Application.Popover`)

---

## See Also

- Keyboard Deep Dive - Understanding keyboard event routing
- Mouse Deep Dive - Mouse event handling
- MenuBar Overview - Using PopoverMenu with MenuBar

---

*(Extracted from https://gui-cs.github.io/Terminal.Gui/docs/Popovers.html)*
