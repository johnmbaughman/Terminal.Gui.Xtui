# Application Architecture

Terminal.Gui v2 uses an instance-based application architecture that decouples views from the global application state, enabling multiple application contexts, providing type-safe result handling, enabling testability.

## Key Features

- Instance-Based: Use `Application.Create()` to get an `IApplication` instance instead of static methods
- IRunnable Interface: Views implement `IRunnable<TResult>` to participate in session management without inheriting from `Runnable`
- Fluent API: Chain `Init()` and `Run()` for elegant, concise code
- `IDisposable` Pattern: Proper resource cleanup with `Dispose()` or `using` statements
- Automatic Disposal: Framework-created runnables are automatically disposed
- Type-Safe Results: Generic `TResult` parameter provides compile-time type safety
- CWP Compliance: All lifecycle events follow the Cancellable Work Pattern

## View Hierarchy and Run Stack

(This section in the original contains a compact diagram of the view hierarchy / run stack.)

## Usage Example Flow

The following Mermaid diagram captures the usage/session flow shown on the original page.

```mermaid
flowchart LR
  A[Initially empty SessionStack] --> B[Run(mainWindow)]
  B --> C[SessionStack: [Main]\nTopRunnable: Main]
  C --> D[Run(dialog)]
  D --> E[SessionStack: [Dialog, Main]\nTopRunnable: Dialog]
  E --> F[RequestStop()]
  F --> G[SessionStack: [Main]\nTopRunnable: Main]
  G --> H[RequestStop()]
  H --> I[SessionStack: []\nTopRunnable: null]
```

## Key Concepts

### Instance-Based vs Static

Terminal.Gui v2 supports both static and instance-based patterns. The static `Application` class is marked obsolete but still functional for backward compatibility. The recommended pattern is to use `Application.Create()` to get an `IApplication` instance:

```csharp
// RECOMMENDED (v2 - instance-based with using statement):
using (IApplication app = Application.Create ().Init ())
{
    Window top = new ();
    top.Add (myView);
    app.Run (top);
    top.Dispose ();
} // app.Dispose() called automatically

// WITH IRunnable (fluent API with automatic disposal):
using (IApplication app = Application.Create ().Init ())
{
    app.Run<ColorPickerDialog> ();
    Color? result = app.GetResult<Color> ();
}

// ALTERNATIVE (manual disposal):
IApplication app = Application.Create ().Init ();
app.Run<ColorPickerDialog> ();
Color? result = app.GetResult<Color> ();
app.Dispose ();

// OLD (v1 / early v2 - obsolete, avoid in new code):
Application.Init ();
Window top = new ();
...
```

Note: The static `Application` class delegates to a singleton instance accessible via `Application.Instance`. `Application.Create()` creates a new application instance, enabling multiple application contexts and better testability.

### `View.App` Property

Every view now has an `App` property that references its application context:

```csharp
public class View
{
    /// <summary>
    /// Gets the application context for this view.
    /// </summary>
    public IApplication? App { get; internal set; }
    
    /// <summary>
    /// Gets the application context, checking parent hierarchy if needed.
    /// Override to customize application resolution.
    /// </summary>
    public virtual IApplication? GetApp () => App ?? SuperView?.GetApp ();
}
```

Benefits:

- Views can be tested without `Application.Init()`
- Multiple applications can coexist
- Clear ownership: views know their context
- Reduced global state dependencies

### Accessing Application from Views

```csharp
public class MyView : View
{
    public override void OnEnter (View view)
    {
        // Use View.App instead of obsolete static Application
        IApplication? app = App;
        app?.TopRunnable?.SetNeedsDraw ();
        
        // Access SessionStack
        if (app?.SessionStack?.Count > 0)
        {
            // Work with sessions
        }
    }
}
```

## IRunnable Architecture

### Key Benefits

- Interface-Based: No forced inheritance from `Runnable`
- Type-Safe Results: Generic `TResult` parameter provides compile-time type safety
- Fluent API: Method chaining for elegant, concise code
- Automatic Disposal: Framework manages lifecycle of created runnables
- CWP Compliance: All lifecycle events follow the Cancellable Work Pattern

### Fluent API Pattern

```csharp
// Recommended: using statement with GetResult
using (IApplication app = Application.Create ().Init ())
{
    app.Run<ColorPickerDialog> ();
    Color? result = app.GetResult<Color> ();
    
    if (result is { })
    {
        ApplyColor (result);
    }
}
```

Key Methods:

- `Init()` - Returns `IApplication` for chaining
- `Run<TRunnable>()` - Creates and runs runnable, returns `IApplication`
- `GetResult()` / `GetResult<T>()` - Extract typed result after run
- `Dispose()` - Release all resources (called automatically with `using`)

### Disposal Semantics

"Whoever creates it, owns it":

```csharp
// Framework ownership - automatic disposal
using (IApplication app = Application.Create ().Init ())
{
    app.Run<MyDialog> ();
    MyResultType? result = app.GetResult<MyResultType> ();
}

// Caller ownership - manual disposal
using (IApplication app = Application.Create ().Init ())
{
    MyDialog dialog = new ();
    app.Run (dialog);
    MyResultType? result = dialog.Result;
    dialog.Dispose ();  // Caller must dispose
}
```

### Creating Runnable Views

Derive from `Runnable<TResult>` or implement `IRunnable<TResult>`:

```csharp
public class FileDialog : Runnable<string?>
{
    private TextField _pathField;
    
    public FileDialog ()
    {
        Title = "Select File";
        
        _pathField = new () { X = 1, Y = 1, Width = Dim.Fill (1) };
        
        Button okButton = new () { Text = "OK", IsDefault = true };
        okButton.Accepting += (s, e) =>
        {
            Result = _pathField.Text;
            App?.RequestStop ();
        };
        
        Add (_pathField, okButton);
    }
    
    protected override bool OnIsRunningChanging (bool oldValue, bool newValue)
    {
        if (!newValue)  // Stopping - extract result before disposal
        {
            Result = _pathField?.Text;
        }
        return base.OnIsRunningChanging (oldValue, newValue);
    }
}
```

## SessionStack

The `SessionStack` manages all running `IRunnable` sessions. Typical operations:

- Push: `Begin(IRunnable)` adds to top of stack
- Pop: `End(SessionToken)` removes from stack
- Peek: `TopRunnable` returns current modal runnable
- All: `SessionStack` enumerates all running sessions

## IApplication Interface

Key members include `TopRunnable`, `TopRunnableView`, `SessionStack`, `Driver`, `Coordinator`, `Init()`, `Dispose()`, `Begin()`, `Run()`, `RequestStop()`, `End()`, `GetResult()` / `GetResult<T>()`.

## Terminology Changes & Migration

The docs explain renamings such as `TopRunnable` (formerly "Current/Top") and `SessionStack` (formerly "Runnables"). Migration strategies include using `View.App`, passing `IApplication` as a dependency, or storing `IApplication` references.

## Resource Management and Disposal

- Use the `using` statement to ensure `Dispose()` is called and input threads are stopped cleanly.
- Manual disposal patterns are shown for cases that need them.

### Input Thread Lifecycle

Calling `Init()` starts a dedicated input thread which must be stopped by `Dispose()` to avoid thread leaks (important in tests).

## Driver Management

- Discover drivers via `Application.GetRegisteredDriverNames()` or `Application.GetRegisteredDrivers()`.
- Prefer `DriverRegistry.Names` for type-safe constants.
- `ForceDriver` allows forcing a specific driver.

## View.Driver Property

Views have a `Driver` property to access driver functions rather than using obsolete static `Application.Driver`.

## Testing

Documentation shows testing patterns for both mock applications and real `IApplication` instances.

## Best Practices

- DO: Use `View.App` and instance-based patterns.
- DON'T: Use the obsolete static `Application` in new code.

## Advanced Scenarios

- Multiple applications can coexist using `Application.Create()` for each instance.

## See Also

Links to related docs: Navigation, Keyboard, Mouse, Drivers, Multitasking, Layout, etc.

---

*Content converted from https://gui-cs.github.io/Terminal.Gui/docs/application.html (main body).*