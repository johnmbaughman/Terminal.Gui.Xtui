# Terminal.Gui Event Deep Dive

This document provides a comprehensive overview of how events work in Terminal.Gui. For the conceptual overview of the Cancellable Work Pattern, see the Cancellable Work Pattern documentation.

## Lexicon and Taxonomy

- Action: A delegate type that represents a method that can be called with specific parameters but returns no value. Used for simple callbacks in Terminal.Gui.
- Cancel/Cancelling/Cancelled: Applies to scenarios where something can be cancelled. Changing the Orientation of a Slider is cancelable.
- Cancellation: Mechanisms to halt a phase or workflow in the Cancellable Work Pattern, such as setting Cancel/Handled properties in event arguments or returning bool from virtual methods.
- Command: A pattern that encapsulates a request as an object, allowing for parameterization and queuing of requests.
- Context: Data passed to observers for informed decision-making in the Cancellable Work Pattern (e.g., DrawContext, Key, ICommandContext, CancelEventArgs<T>).
- Default Behavior: Standard implementations for phases in the Cancellable Work Pattern (e.g., DrawText, InvokeCommands, RaiseActivating).
- Event: A notification mechanism used extensively in Terminal.Gui for UI interactions.
- Handle/Handling/Handled: Whether an event has been handled by a listener or override.
- Invoke: Calling or triggering an event, action, or method.
- Listen: Subscribing for notifications when an event occurs.
- Notifications: Events (DrawingText, KeyDown, Activating, OrientationChanging) and virtual methods (OnDrawingText, OnKeyDown, OnActivating, OnOrientationChanging).
- Raise: Triggering an event to notify registered handlers.
- Workflow: A sequence of phases in the Cancellable Work Pattern (multi-phase, linear, per-unit, or event-driven).

## Event Categories

Terminal.Gui uses several types of events:

1. UI Interaction Events: Events triggered by user input (keyboard, mouse).
2. View Lifecycle Events: Events related to view creation, activation, and disposal.
3. Property Change Events: Events for property value changes.
4. Drawing Events: Events related to view rendering.
5. Command Events: Events for command execution and workflow control.

## Event Patterns

### 1. Cancellable Work Pattern (CWP)

The Cancellable Work Pattern (CWP) is a core pattern in Terminal.Gui that provides a consistent way to handle cancellable operations. An "event" typically has two components:

1. Protected virtual method: `protected virtual OnMethod()` which can be overridden by subclasses.
2. Public event: `public event EventHandler<>` which allows external subscribers to participate.

The virtual method is called first (giving subclasses priority), then the event is invoked. Helper classes exist to simplify implementing CWP.

#### Manual CWP Implementation

```csharp
public class MyView : View
{
    // Public event
    public event EventHandler<MouseEventArgs>? MouseEvent;

    // Protected virtual method
    protected virtual bool OnMouseEvent(MouseEventArgs args)
    {
        // Return true to handle the event and stop propagation
        return false;
    }

    // Internal method to raise the event
    internal bool RaiseMouseEvent(MouseEventArgs args)
    {
        // Call virtual method first
        if (OnMouseEvent(args) || args.Handled)
        {
            return true;
        }

        // Then raise the event
        MouseEvent?.Invoke(this, args);
        return args.Handled;
    }
}
```

#### CWP with Helper Classes

Terminal.Gui provides static helper classes to implement the pattern consistently.

#### Property Changes

For property changes, `CWPPropertyHelper.ChangeProperty` can be used. Example:

```csharp
public class MyView : View
{
    private string _text = string.Empty;
    public event EventHandler<ValueChangingEventArgs<string>>? TextChanging;
    public event EventHandler<ValueChangedEventArgs<string>>? TextChanged;

    public string Text
    {
        get => _text;
        set
        {
            if (CWPPropertyHelper.ChangeProperty(
                currentValue: _text,
                newValue: value,
                onChanging: args => OnTextChanging(args),
                changingEvent: TextChanging,
                onChanged: args => OnTextChanged(args),
                changedEvent: TextChanged,
                out string finalValue))
            {
                _text = finalValue;
            }
        }
    }

    // Virtual method called before the change
    protected virtual bool OnTextChanging(ValueChangingEventArgs<string> args)
    {
        // Return true to cancel the change
        return false;
    }

    // Virtual method called after the change
    protected virtual void OnTextChanged(ValueChangedEventArgs<string> args)
    {
        // React to the change
    }
}
```

#### Workflows

For general workflows, `CWPWorkflowHelper` is available. Example:

```csharp
public class MyView : View
{
    public bool? ExecuteWorkflow()
    {
        ResultEventArgs<bool> args = new();
        return CWPWorkflowHelper.Execute(
            onMethod: args => OnExecuting(args),
            eventHandler: Executing,
            args: args,
            defaultAction: () =>
            {
                // Main execution logic
                DoWork();
                args.Result = true;
            });
    }

    // Virtual method called before execution
    protected virtual bool OnExecuting(ResultEventArgs<bool> args)
    {
        // Return true to cancel execution
        return false;
    }

    public event EventHandler<ResultEventArgs<bool>>? Executing;
}
```

### 2. Action Callbacks

For simple callbacks without cancellation, use `Action`. Example from `Shortcut`:

```csharp
public class Shortcut : View
{
    /// <summary>
    ///     Gets or sets the action to be invoked when the shortcut key is pressed or the shortcut is clicked on with the
    ///     mouse.
    /// </summary>
    /// <remarks>
    ///     Note, the <see cref="View.Accepting"/> event is fired first, and if cancelled, the event will not be invoked.
    /// </remarks>
    public Action? Action { get; set; }

    internal virtual bool? DispatchCommand(ICommandContext? commandContext)
    {
        bool cancel = base.DispatchCommand(commandContext) == true;

        if (cancel)
        {
            return true;
        }

        if (Action is { })
        {
            Logging.Debug($"{Title} ({commandContext?.Source?.Title}) - Invoke Action...");
            Action.Invoke();

            // Assume if there's a subscriber to Action, it's handled.
            cancel = true;
        }

        return cancel;
    }
}
```

### 3. Property Change Notifications

Implement `INotifyPropertyChanged` for property change notifications. Example `Aligner`:

```csharp
public class Aligner : INotifyPropertyChanged
{
    private Alignment _alignment;
    public event PropertyChangedEventHandler? PropertyChanged;

    public Alignment Alignment
    {
        get => _alignment;
        set
        {
            _alignment = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Alignment)));
        }
    }
}
```

### 4. Event Propagation

Events often propagate through the view hierarchy. Example from `Button`:

```csharp
private bool? HandleHotKeyCommand (ICommandContext commandContext)
{
    bool cachedIsDefault = IsDefault; // Supports "Swap Default" in Buttons scenario where IsDefault changes

    if (RaiseActivating (commandContext) is true)
    {
        return true;
    }

    bool? handled = RaiseAccepting (commandContext);

    if (handled == true)
    {
        return true;
    }

    SetFocus ();

    // If Accept was not handled...
    if (cachedIsDefault && SuperView is { })
    {
        return SuperView.InvokeCommand (Command.Accept);
    }

    return false;
}
```

This shows how `Button` raises `Activating`, then `Accepting`, and may propagate `Accept` to the `SuperView`.

## Event Context

### Event Arguments

Terminal.Gui provides rich event argument types. Example `CommandEventArgs`:

```csharp
public class CommandEventArgs : EventArgs
{
    public ICommandContext? Context { get; set; }
    public bool Handled { get; set; }
    public bool Cancel { get; set; }
}
```

### Command Context

Command execution includes context via `ICommandContext`:

```csharp
public interface ICommandContext
{
    View Source { get; }
    object? Parameter { get; }
    IDictionary<string, object> State { get; }
}
```

## Best Practices

1. Event Naming:
   - Use past tense for completed events (e.g., `Clicked`, `Changed`).
   - Use present tense for ongoing events (e.g., `Clicking`, `Changing`).
   - Use "ing" suffix for cancellable events.
2. Event Handler Implementation:
   - Keep handlers short and focused.
   - Use `async/await` for long-running tasks.
   - Unsubscribe from events in `Dispose`.
   - Use weak event patterns for long-lived subscriptions.
3. Event Context:
   - Provide rich context in event args (source view, binding details, view-specific state).
4. Event Propagation:
   - Use appropriate propagation mechanisms and avoid unnecessary bubbling.
   - Consider `PropagatedCommands` for hierarchical views.

## Common Pitfalls

1. Memory Leaks

```csharp
// BAD: Potential memory leak
view.Activating += OnActivating;

// GOOD: Unsubscribe in Dispose
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        view.Activating -= OnActivating;
    }
    base.Dispose(disposing);
}
```

2. Incorrect Event Cancellation

```csharp
// BAD: Using Cancel for event handling
args.Cancel = true;  // Wrong for MouseEventArgs

// GOOD: Using Handled for event handling
args.Handled = true;  // Correct for MouseEventArgs

// GOOD: Using Cancel for operation cancellation
args.Cancel = true;  // Correct for CancelEventArgs
```

3. Missing Context

```csharp
// BAD: Missing context
Activating?.Invoke(this, new CommandEventArgs());

// GOOD: Including context
Activating?.Invoke(this, new CommandEventArgs { Context = ctx });
```

## Known Issues & Proposed Enhancements

### Proposed Enhancement: Command Propagation

The Cancellable Work Pattern in `View.Command` currently supports local `Command.Activate` and propagating `Command.Accept`. A proposed enhancement is to add `PropagatedCommands` to `View` to enable hierarchical coordination (Issue #4050). Example:

```csharp
public IReadOnlyList<Command> PropagatedCommands { get; set; } = new List<Command> { Command.Accept };
protected bool? RaiseAccepting(ICommandContext? ctx)
{
    CommandEventArgs args = new() { Context = ctx };
    if (OnAccepting(args) || args.Handled)
    {
        return true;
    }
    Accepting?.Invoke(this, args);

    if (!args.Handled && SuperView?.PropagatedCommands.Contains(Command.Accept) == true)
    {
        return SuperView.InvokeCommand(Command.Accept, ctx);
    }
    return Accepting is null ? null : args.Handled;
}
```

### Conflation in FlagSelector

Issue: `CheckBox.Activating` triggers `Accepting`, conflating state change and confirmation.
Recommendation: Separate `Activating` and `Accepting` handlers.

### Complexity in Multi-Phase Workflows

Issue: `View.Draw` multi-phase workflows can be complex. Recommendation: clearer phase-specific docs and examples.

## Useful External Documentation

- .NET Naming Guidelines - Names of Events: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members
- .NET Design for Extensibility - Events and Callbacks: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/events-and-callbacks
- C# Event Implementation Fundamentals, Best Practices and Conventions: https://www.codeproject.com/Articles/20550/C-Event-Implementation-Fundamentals-Best-Practices

## Additional Links

- Terminal.Gui docs: https://gui-cs.github.io/Terminal.Gui/docs/index.html
- Cancellable Work Pattern: https://gui-cs.github.io/Terminal.Gui/docs/cancellable-work-pattern.html
- Command Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/command.html
- View Deep Dive: https://gui-cs.github.io/Terminal.Gui/docs/View.html

