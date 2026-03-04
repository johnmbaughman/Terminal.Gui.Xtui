# Minimal Xtui Public Interfaces

Purpose
- Provide a tiny, stable public interface surface that generated code can call without requiring a runtime helper class from the generator package.
- Generator emits the public interfaces (in a generated `Xtui.Interfaces.g.cs` or `AppXtui.g.cs`) and an internal implementation. Generated code calls the interfaces; the implementation is wired up by the generator during code emission.

Where to place
- Emit these public interfaces into the user's assembly (the generator should add a small `Xtui.Interfaces.g.cs` to the compilation). Keeping them `public` and tiny avoids any additional NuGet/runtime dependency.
- The generator also emits an `internal` implementation and a single public accessor holder the generated code uses (see `Xtui` below).

Minimal interfaces

public interface IXtuiAppHelpers
{
    void EnsureInitialized();
    void RegisterResource(string key, object value);
    bool TryGetResource(string key, out object? value);
    IReadOnlyDictionary<string, object> Resources { get; }
}

public interface IXtuiViewHelpers
{
    void SetResource(object view, string key, object value);
    bool TryGetResource(object view, string key, out object? value);
    void SetDataContext(object view, object? dataContext);
    object? GetDataContext(object view);
    void SetXtuiTag(object view, string? tag);
    string? GetXtuiTag(object view);
}

public interface IXtuiBindingHelpers
{
    void SetBinding(object target, string targetPropertyName, Binding binding);
    void NotifyDataContextChanged(object target, object? newDataContext);
}

Notes about `Binding` and converters
- `Binding` can be a small public type (emitted by the generator) or a public record/type declared alongside these interfaces. It does not need runtime library support beyond what the generated code emits.
- `IValueConverter` (see `Notes/ConverterInvestigation.md`) is used by the binding helpers; generator should emit converter-creation and registration code into the generated files and register them via `IXtuiAppHelpers.RegisterResource` or per-view `SetResource`.

Public accessor pattern (recommended)
- Emit a tiny public static accessor that exposes the interface instances. The generator wires these to its internal implementation.

public static class Xtui
{
    public static IXtuiAppHelpers App { get; } = /* assigned to internal impl by generated code */ throw new NotImplementedException();
    public static IXtuiViewHelpers View { get; } = /* assigned to internal impl by generated code */ throw new NotImplementedException();
    public static IXtuiBindingHelpers Binding { get; } = /* assigned to internal impl by generated code */ throw new NotImplementedException();
}

How the generator wires implementations
- Generator emits an internal implementation type(s) (for example `internal sealed class __Xtui_AppHelpersImpl : IXtuiAppHelpers`) and assigns them to the `Xtui` accessor  in the same generated assembly. Because both the interface and the implementation are emitted into the user's assembly, generated code can call `Xtui.App.EnsureInitialized()` without any runtime package.

Rationale and trade-offs
- Interfaces keep the public surface stable and mockable for tests.
- Keeping the implementation internal (and generated) ensures no runtime assembly dependency on the generator package and allows implementation changes without breaking consumers.
- Keep the interfaces minimal: add methods only when generated code requires them.

Example minimal file name: `Xtui.Interfaces.g.cs` (generator should emit into project root namespace).
