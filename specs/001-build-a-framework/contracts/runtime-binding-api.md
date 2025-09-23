# Runtime Binding API Contract

## IDataBindingEngine Interface

```csharp
/// <summary>
/// Runtime data binding engine for Terminal.Gui XAML applications
/// </summary>
public interface IDataBindingEngine
{
    /// <summary>
    /// Create binding between view property and data source
    /// Performance requirement: Must complete within 50ms
    /// </summary>
    /// <param name="target">Target Terminal.Gui view</param>
    /// <param name="targetProperty">Property name on target view</param>
    /// <param name="source">Data source object</param>
    /// <param name="binding">Binding configuration</param>
    /// <returns>Active binding instance</returns>
    IActiveBinding CreateBinding(View target, string targetProperty, object source, BindingExpression binding);
    
    /// <summary>
    /// Establish two-way data binding
    /// </summary>
    /// <param name="target">Target Terminal.Gui view</param>
    /// <param name="targetProperty">Property name on target view</param>
    /// <param name="source">Data source object</param>
    /// <param name="binding">Binding configuration</param>
    /// <returns>Active two-way binding instance</returns>
    ITwoWayBinding CreateTwoWayBinding(View target, string targetProperty, object source, BindingExpression binding);
    
    /// <summary>
    /// Remove and cleanup binding
    /// </summary>
    /// <param name="binding">Active binding to remove</param>
    void RemoveBinding(IActiveBinding binding);
    
    /// <summary>
    /// Update all bindings for specific data source
    /// </summary>
    /// <param name="source">Data source that changed</param>
    void RefreshBindings(object source);
    
    /// <summary>
    /// Register custom value converter
    /// </summary>
    /// <param name="converter">Value converter implementation</param>
    void RegisterConverter(IValueConverter converter);
}
```

## Active Binding Management

```csharp
public interface IActiveBinding : IDisposable
{
    /// <summary>
    /// Unique binding identifier
    /// </summary>
    Guid BindingId { get; }
    
    /// <summary>
    /// Target view for binding
    /// </summary>
    View Target { get; }
    
    /// <summary>
    /// Source object for binding
    /// </summary>
    object Source { get; }
    
    /// <summary>
    /// Current binding status
    /// </summary>
    BindingStatus Status { get; }
    
    /// <summary>
    /// Update binding value from source
    /// Performance requirement: Must complete within 10ms
    /// </summary>
    void UpdateFromSource();
    
    /// <summary>
    /// Update source value from target (for two-way bindings)
    /// </summary>
    void UpdateFromTarget();
    
    /// <summary>
    /// Evaluate binding expression
    /// </summary>
    /// <returns>Evaluated value or binding error</returns>
    object? EvaluateExpression();
}
```

## Value Conversion Contract

```csharp
public interface IValueConverter
{
    /// <summary>
    /// Converter name for XAML registration
    /// </summary>
    string ConverterName { get; }
    
    /// <summary>
    /// Convert value from source to target
    /// </summary>
    /// <param name="value">Source value</param>
    /// <param name="targetType">Target property type</param>
    /// <param name="parameter">Converter parameter</param>
    /// <param name="culture">Current culture</param>
    /// <returns>Converted value</returns>
    object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture);
    
    /// <summary>
    /// Convert value from target back to source (two-way binding)
    /// </summary>
    /// <param name="value">Target value</param>
    /// <param name="targetType">Source property type</param>
    /// <param name="parameter">Converter parameter</param>
    /// <param name="culture">Current culture</param>
    /// <returns>Converted value</returns>
    object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture);
}
```

## Performance Requirements

- **Binding Creation**: Must complete within 50ms per binding
- **Value Updates**: Must complete within 10ms per update
- **Memory Management**: Must properly dispose of event subscriptions
- **Change Notifications**: Must efficiently handle INotifyPropertyChanged events
- **Error Handling**: Must gracefully handle binding failures without crashing

## Event Handling Contract

```csharp
public interface IEventBindingManager
{
    /// <summary>
    /// Bind XAML event to code-behind method
    /// </summary>
    /// <param name="target">Target Terminal.Gui view</param>
    /// <param name="eventName">Event name (e.g., "Clicked")</param>
    /// <param name="handler">Code-behind instance</param>
    /// <param name="methodName">Handler method name</param>
    void BindEvent(View target, string eventName, object handler, string methodName);
    
    /// <summary>
    /// Remove event binding
    /// </summary>
    /// <param name="target">Target view</param>
    /// <param name="eventName">Event name</param>
    void UnbindEvent(View target, string eventName);
    
    /// <summary>
    /// Validate that event and handler are compatible
    /// </summary>
    /// <param name="eventInfo">Terminal.Gui event information</param>
    /// <param name="handlerMethod">Code-behind method information</param>
    /// <returns>True if compatible</returns>
    bool ValidateEventHandler(EventInfo eventInfo, MethodInfo handlerMethod);
}
```

## Constitutional Compliance

- **User Experience**: Bindings must maintain >30 FPS during updates
- **Error Handling**: Must provide clear error messages for binding failures
- **Performance Monitoring**: Must track binding performance metrics
- **Memory Usage**: Must stay within 50MB memory constraint
- **Thread Safety**: Must be safe for Terminal.Gui's threading model