// <copyright file="IDataBindingEngine.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Binding;

/// <summary>
/// Defines the contract for data binding engine services.
/// </summary>
public interface IDataBindingEngine
{
    /// <summary>
    /// Binds a source property to a target property with a given mode.
    /// </summary>
    /// <param name="sourceProperty">The source property path.</param>
    /// <param name="targetProperty">The target property path.</param>
    /// <param name="mode">The binding mode.</param>
    void Bind(string sourceProperty, string targetProperty, BindingMode mode);

    /// <summary>
    /// Binds data to a control.
    /// </summary>
    /// <param name="control">The control to bind data to.</param>
    /// <param name="dataSource">The data source.</param>
    /// <param name="propertyPath">The property path for binding.</param>
    void Bind(object control, object dataSource, string propertyPath);
    
    /// <summary>
    /// Unbinds data from a control.
    /// </summary>
    /// <param name="control">The control to unbind data from.</param>
    void Unbind(object control);
    
    /// <summary>
    /// Updates the binding for a control.
    /// </summary>
    /// <param name="control">The control to update binding for.</param>
    void UpdateBinding(object control);
    
    /// <summary>
    /// Validates a binding expression.
    /// </summary>
    /// <param name="expression">The binding expression.</param>
    /// <returns>True if the binding expression is valid; otherwise, false.</returns>
    bool ValidateBinding(string expression);

    /// <summary>
    /// Binds an event on a control to a handler name.
    /// </summary>
    /// <param name="eventPath">The event path (e.g., Button.Clicked).</param>
    /// <param name="handlerName">The handler method name.</param>
    void BindEvent(string eventPath, string handlerName);
}

/// <summary>
/// Specifies binding modes.
/// </summary>
public enum BindingMode
{
    /// <summary>
    /// One-way binding from source to target.
    /// </summary>
    OneWay,

    /// <summary>
    /// Two-way binding between source and target.
    /// </summary>
    TwoWay,
}