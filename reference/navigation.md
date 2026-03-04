# Navigation Deep Dive

This deep dive covers Terminal.Gui's navigation system: visual cues for focus, how users change focus, keyboard and mouse navigation, programmatic navigation, and accessibility considerations.

## Lexicon & Taxonomy

- Cursor: visual indicator of where keyboard input will be applied.
- Focus: the state where a View receives input.
- Focus Chain: ordered sequence of Views from the focused element up the SuperView chain.
- TabGroup: a container for focusable views; navigated with Application.NextTabGroupKey / PrevTabGroupKey.
- TabStop: an ultimate stop-point for keyboard navigation (Tab/Shift+Tab).

## Tenets for UI Navigation

- One Focus Per App: exactly one view receives keyboard input at a time.
- There's Always a Way With The Keyboard: built-in views expose navigation keys and hotkeys.
- Flexible Overrides: navigation behavior can be changed via properties or by overriding focus events.
- Decouple Concepts: `CanFocus`, `TabStop`, and other properties are intentionally decoupled in v2.

## Answering the Key Navigation Questions

### Visual Cues for Focus

- Focused views render using `ColorScheme.Focus`.
- Focused views may display a cursor for text input views.
- HotKeys are shown by underlined characters in labels/buttons.
- Focus indicators (highlight rectangles, color) show which view will receive input.

### Changing Focus

Keyboard methods:
- `Tab` / `Shift+Tab` — navigate between TabStop views
- `F6` / `Shift+F6` — navigate between TabGroup containers
- Arrow keys — navigate within containers or between adjacent views
- HotKeys (Alt+letter) — direct navigation to specific views
- `Enter` / `Space` — activate the focused view

Mouse methods:
- Click a focusable view to give it focus
- Focus behavior may `RestoreFocus` to a previously focused subview or `AdvanceFocus` when appropriate

### Navigation Order

- TabStop Views: navigated with Tab/Shift+Tab in layout order
- TabGroup Views: navigated with F6/Shift+F6 across containers
- NoStop Views: skipped in keyboard tabbing but can receive mouse focus

## Keyboard Navigation

Terminal.Gui registers application-scoped key bindings during `app.Init()`:

- `IKeyboard.NextTabStopKey` — `Key.Tab`
- `IKeyboard.PrevTabStopKey` — `Key.Tab.WithShift`
- `Key.CursorRight` / `Key.CursorDown` — act as `NextTabStop`
- `Key.CursorLeft` / `Key.CursorUp` — act as `PrevTabStop`
- `IKeyboard.NextTabGroupKey` — `Key.F6`
- `IKeyboard.PrevTabGroupKey` — `Key.F6.WithShift`

Views may override these behaviors (e.g. `TextView` overrides `Key.Tab`). Unit tests ensure built-in views have navigation keys that advance focus.

### HotKeys

HotKeys are defined through the `HotKey` property and activated with `Alt+<key>`. They work independently of focus and can trigger actions anywhere in the view hierarchy.

```csharp
var saveButton = new Button() { Text = "_Save", HotKey = Key.S };
var exitButton = new Button() { Text = "E_xit", HotKey = Key.X };
// Alt+S -> save, Alt+X -> exit
```

## Mouse Navigation

- Clicking a focusable view sets focus (if `CanFocus == true`).
- If the clicked view is a container with focusable subviews, behavior depends on prior focus: either `RestoreFocus()` or `AdvanceFocus()` is used.
- Developers can use `MouseEvent` handlers to customize click/focus behavior.

Example:

```csharp
view.MouseEvent += (sender, e) => {
    if (e.Flags.HasFlag(MouseFlags.LeftButtonClicked) && view.CanFocus) {
        view.SetFocus();
        e.Handled = true;
    }
};
```

## Application-Level Navigation

`Application.Navigation` encapsulates navigation helpers and events (`FocusedChanged`, `FocusedChanging`, `AdvanceFocus`, `GetFocused`). Use `App?.Current` when accessing from a `View`.

Programmatic examples:

```csharp
var app = Application.Create().Init();
app.Navigation.FocusedChanged += (s,e) => {
    var focused = app.Navigation.GetFocused();
    StatusBar.Text = $"Focused: {focused?.GetType().Name ?? "None"}";
};

Application.Navigation.AdvanceFocus(NavigationDirection.Forward, TabBehavior.TabStop);
```

## View-Level Navigation

`View.AdvanceFocus` is the core method to change focus for a view. Important events: `HasFocusChanging`, `HasFocusChanged`.

Override examples:

```csharp
protected override void OnHasFocusChanging(CancelEventArgs<bool> e) {
    if (SomeCondition) { e.Cancel = true; return; }
    base.OnHasFocusChanging(e);
}
```

## What Makes a View Focusable?

Requirements for keyboard focus:
1. `Visible == true`
2. `Enabled == true`
3. `CanFocus == true`
4. `TabStop != TabBehavior.NoStop` (for keyboard navigation)

A view can still be focused programmatically or by mouse even if `TabStop == NoStop`.

Example:

```csharp
var view = new Label() {
  Text = "Focusable Label",
  Visible = true,
  Enabled = true,
  CanFocus = true,
  TabStop = TabBehavior.TabStop
};
```

## Determining the Most-Focused View

Use `Application.Navigation.GetFocused()` to obtain the most-focused view in the app. `View.HasFocus` indicates whether a view is in the focus chain.

## Programmatic Focus Control

```csharp
if (myButton.SetFocus()) {
  Console.WriteLine("Button now has focus");
}
else {
  Console.WriteLine("Could not focus button");
}
// or
myButton.HasFocus = true;
```

## Built-In Views Interactivity

A summary table (omitted here) describes how built-in views respond to input methods (e.g., Button, ListView, TextField). Key points:

- Many controls use `OnAccept`, `OnSelect`, or `Focus` behaviors for activation.
- `ListView` and `TextView` have behaviors to support selection/scrolling and context menus.

## Common Navigation Patterns

Dialog navigation:

```csharp
var dialog = new Dialog() { Title = "Settings", CanFocus = true, TabStop = TabBehavior.TabGroup };
var okButton = new Button() { Text = "OK", IsDefault = true };
var cancelButton = new Button() { Text = "Cancel" };
dialog.Add(okButton, cancelButton);
```

Container navigation (panels):

```csharp
var leftPanel = new FrameView { Title = "Options", TabStop = TabBehavior.TabGroup, X = 0, Width = Dim.Percent(50) };
var rightPanel = new FrameView { Title = "Preview", TabStop = TabBehavior.TabGroup, X = Pos.Right(leftPanel), Width = Dim.Fill() };
```

## Accessibility Considerations

- Ensure all functionality is accessible by keyboard.
- Maintain logical tab order and clear focus indicators.
- Use HotKeys for direct access to important functions.
- Provide meaningful labels and screen-reader-friendly content.

Examples and best practices are included in the original docs.

## Additional Links

- Keyboard Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/keyboard.html
- Mouse Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/mouse.html
- Lexicon & Taxonomy: https://gui-cs.github.io/Terminal.Gui/docs/lexicon.html

Generated from: https://gui-cs.github.io/Terminal.Gui/docs/navigation.html
