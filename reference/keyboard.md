# Keyboard Deep Dive

## See Also

- Cancellable Work Pattern
- Command Deep Dive
- Mouse Deep Dive
- Lexicon & Taxonomy

---

## Tenets for Terminal.Gui Keyboard Handling (Unless you know better ones...)

Tenets higher in the list have precedence over tenets lower in the list.

- **Users Have Control** - Terminal.Gui provides default key bindings consistent with these tenets, but those defaults are configurable by the user (for example via `Terminal.Gui.Configuration.ConfigurationManager`).
- **More Editor than Command Line** - Once a Terminal.Gui app starts, the user is no longer using the command line. Users expect keyboard idioms in TUI apps to be consistent with GUI apps (such as VS Code, Vim, and Emacs). For example, `Ctrl+V` is `Paste` by default in Terminal.Gui.
- **Be Consistent With the User's Platform** - Respect platform conventions (e.g., `Ctrl+Backspace` on Windows vs `Ctrl+W` on Linux to erase a word).
- **The Source of Truth is Wikipedia** - The project uses the Wikipedia article as a guide for default key bindings.
- **If It's Hot, It Works** - If a `View` has a visible `HotKey`, pressing it should invoke the defined behavior even when other UI elements (like modal views) are present.

---

## Keyboard APIs

### Key Bindings

Key Bindings is the preferred way of handling keyboard input in `View` implementations. The `View` calls `View.AddCommand(Command, Func<bool?>)` to declare it supports a particular command and then uses `KeyBindings` to indicate which key presses will invoke the command. Example:

```
public MyView : View
{
  AddCommand (Command.ScrollUp, () => ScrollVertical (-1));
  KeyBindings.Add (Key.CursorUp, Command.ScrollUp);
}

```

The `Character Map` scenario includes a `CharMap` view that demonstrates the Key Bindings API.

The `Command` enum lists generic operations implemented by views (for example `Command.Accept`). Use `View.GetSupportedCommands()` to determine which commands a `View` implements.

The default key for activating a button is `Space`. You can change this using `KeyBindings.ReplaceKey()`:

```
var btn = new Button () { Title = "Press me" };
btn.KeyBindings.ReplaceKey (btn.KeyBindings.GetKeyFromCommands (Command.Accept));

```

Key Bindings can be added at the `Application` or `View` level. Application-scoped Key Bindings have two categories:

1. Application Command Key Bindings - for `Command`s supported by `App.Application` (e.g., `Application.QuitKey`).
2. Application Key Bindings - for `Command`s supported on arbitrary `Views` that should be invoked regardless of what part of the application is active.

Use `Application.Keyboard.KeyBindings` to add or modify Application-scoped key bindings. For backward compatibility, `Application.KeyBindings` delegates to the same bindings.

View-scoped Key Bindings also have two categories:

1. **HotKey Bindings** - invoked regardless of whether the `View` has focus (common use-case: `View.HotKey`).
2. **Focused Bindings** - invoked only when the `View` has focus. Use `View.KeyBindings` for focused bindings and `View.HotKeyBindings` for hotkeys.

---

### HotKey

A HotKey is a key press that selects a visible UI item. To select items across `View`s (e.g., a `Button` in a `Dialog`) the key press must have the `Alt` modifier. For selecting items within a `View` that are not `View`s themselves, the key press can be without `Alt`.

By default, the `Text` of a `View` is scanned for the `HotKeySpecifier` (underscore `_` by default); the character following the underscore is the `HotKey`. If no specifier is found, the first character of `Text` is used.

---

### Shortcut

A `Shortcut` is an opinionated view for displaying a command, help text, and the key press that invokes a `Command`. Shortcuts can be any key press (e.g., `Key.A`, `Key.A.WithCtrl`, `Key.Del`, `Key.F1`).

`MenuBar`, `PopoverMenu`, and `StatusBar` support `Shortcut`s.

---

### Key Events

Note: Most drivers/platforms do not support distinct KeyUp events; Terminal.Gui does not support KeyUp.

`Application.RaiseKeyDownEvent` raises `Application.KeyDown` and calls `View.NewKeyDownEvent` on runnable views. If no view handles the key event, Application-scoped key bindings will be invoked.

When a view is enabled, `View.NewKeyDownEvent` does the following:

1. If the view has a focused subview, `NewKeyDown` is called recursively on the focused view. If it handles the key, processing stops.
2. `OnKeyDown` is called. If it handles the key, processing stops.
3. `KeyDown` is raised on the view.
4. If the view does not handle the event, any bindings for the key will be invoked. If a bound command handler returns `true`, processing stops.
5. If the key is not bound or handlers did not handle it, `OnKeyDownNotHandled` is called.

Keyboard events are retrieved from `Drivers` each iteration of the `Application` main loop. The driver raises the `IDriver.KeyDown` event which invokes `Application.RaiseKeyDownEvent`.

Terminal.Gui provides these APIs for handling keyboard input:

- `Key` - platform-independent abstraction for keyboard operations.
- `KeyBindings` - declarative handling of keyboard input via commands.
- `Key Events` - subscribe to `View.KeyDown` and related events for cases like capturing arbitrary typing.

---

## General input model

### Application

- Implements support for `KeyBindingScope.Application`.
- Keyboard functionality is encapsulated in `IKeyboard`, accessed via `Application.Keyboard`.
- `Application.Keyboard` provides access to `KeyBindings`, key binding configuration (`QuitKey`, `ArrangeKey`, navigation keys), and keyboard event handling.
- For backward compatibility, `Application` still exposes static properties/methods that delegate to `Application.Keyboard`.
- `RaiseKeyDownEvent` is public and can be used to simulate keyboard input (prefer `InputInjector` for testing).

### View

- Views implement `KeyBindings` and `HotKeyBindings`.
- Exposes cancelable `NewKeyDownEvent` and virtual `OnKeyDown` methods.

---

## IKeyboard Architecture

### Key Features

1. **Decoupled State** - Keyboard state (bindings, navigation keys, events) is in `IKeyboard` separate from `Application`.
2. **Dependency Injection** - `Keyboard` implementation receives an `IApplication` reference.
3. **Testability** - Unit tests can create isolated `IKeyboard` instances with mock `IApplication`.
4. **Backward Compatibility** - Existing `Application` keyboard APIs remain available and delegate to `Application.Keyboard`.

### Usage Examples

```
// Modern approach - using IKeyboard
App.Keyboard.KeyBindings.Add(Key.F1, Command.HotKey);
App.Keyboard.RaiseKeyDownEvent(Key.Enter);
App.Keyboard.QuitKey = Key.Q.WithCtrl;

// Legacy approach - still works (delegates to Application.Keyboard)
Application.KeyBindings.Add(Key.F1, Command.HotKey);
Application.RaiseKeyDownEvent(Key.Enter);
Application.QuitKey = Key.Q.WithCtrl;

```

Testing with isolated keyboard instances:

```
var keyboard1 = new Keyboard();
keyboard1.QuitKey = Key.Q.WithCtrl;
keyboard1.KeyBindings.Add(Key.F1, Command.HotKey);

var keyboard2 = new Keyboard();
keyboard2.QuitKey = Key.X.WithCtrl;
keyboard2.KeyBindings.Add(Key.F2, Command.Accept);

Assert.Equal(Key.Q.WithCtrl, keyboard1.QuitKey);
Assert.Equal(Key.X.WithCtrl, keyboard2.QuitKey);

```

Accessing application context from views:

```
public class MyView : View
{
    protected override bool OnKeyDown(Key key)
    {
        // Use View.App instead of static Application
        if (key == Key.F1)
        {
            App?.Keyboard?.KeyBindings.Add(Key.F2, Command.Accept);
            return true;
        }
        return base.OnKeyDown(key);
    }
}

```

### Architecture Benefits

- Parallel testing, dependency inversion, cleaner code, mockability.

### Implementation Details

The `Keyboard` class implements `IKeyboard` and maintains:

- `KeyBindings`
- Navigation Keys: `QuitKey`, `ArrangeKey`, `NextTabKey`, `PrevTabKey`, `NextTabGroupKey`, `PrevTabGroupKey`
- Events: `KeyDown` for application-level keyboard monitoring
- Command implementations for application-scoped commands (Quit, Suspend, Navigation, Refresh, Arrange)

---

## Testing Keyboard Input

### Quick Test Example

```
VirtualTimeProvider time = new();
using IApplication app = Application.Create(time);
app.Init(DriverRegistry.Names.ANSI);

// Subscribe to key events
app.Keyboard.KeyDown += (s, e) => Console.WriteLine($"Key: {e}");

// Inject keys
app.InjectKey(Key.A);
app.InjectKey(Key.Enter);
app.InjectKey(Key.Esc);

```

### Testing Key Commands

```
VirtualTimeProvider time = new();
using IApplication app = Application.Create(time);
app.Init(DriverRegistry.Names.ANSI);

Button button = new() { Text = "_Click Me" };
bool acceptingCalled = false;
button.Accepting += (s, e) => acceptingCalled = true;

IRunnable runnable = new Runnable();
(runnable as View)?.Add(button);
app.Begin(runnable);

// Inject hotkey (Alt+C)
app.InjectKey(Key.C.WithAlt);

Assert.True(acceptingCalled);

```

### Testing Escape Sequences with Pipeline Mode

```
VirtualTimeProvider time = new();
using IApplication app = Application.Create(time);
app.Init(DriverRegistry.Names.ANSI);

IInputInjector injector = app.GetInputInjector();
InputInjectionOptions options = new() { Mode = InputInjectionMode.Pipeline };

// This encodes Key.F1 => "\x1b[OP", injects chars, parses back to Key.F1
injector.InjectKey(Key.F1, options);

```

Key testing features: virtual time control, single-call injection, pipeline/direct modes, escape sequence handling.

---

## Additional Links

- Terminal.Gui v2
- Documentation index
- API Reference
- Source (GitHub)
- Overview, Getting Started, Showcase, What's new in v2

---

*(Extracted from https://gui-cs.github.io/Terminal.Gui/docs/keyboard.html)*
