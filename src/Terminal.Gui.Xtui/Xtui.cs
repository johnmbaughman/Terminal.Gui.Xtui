using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui;

/// <summary>
/// Represents a binding between a source property and a target property.
/// </summary>
public record Binding(string Path, object? Source = null, IValueConverter? Converter = null, object? ConverterParameter = null, string? ElementName = null);

/// <summary>
/// Provides application-level helpers for Xtui.
/// </summary>
public interface IXtuiAppHelpers
{
    /// <summary>
    /// Ensures the Xtui system is initialized.
    /// </summary>
    void EnsureInitialized();

    /// <summary>
    /// Registers a resource with the given key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The resource value.</param>
    void RegisterResource(string key, object value);

    /// <summary>
    /// Tries to get a resource with the given key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The resource value if found.</param>
    /// <returns>True if the resource was found; otherwise, false.</returns>
    bool TryGetResource(string key, out object? value);

    /// <summary>
    /// Gets the resources dictionary.
    /// </summary>
    IReadOnlyDictionary<string, object> Resources { get; }
}

/// <summary>
/// Provides view-level helpers for Xtui.
/// </summary>
public interface IXtuiViewHelpers
{
    /// <summary>
    /// Sets a resource on a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The resource value.</param>
    void SetResource(object view, string key, object value);

    /// <summary>
    /// Tries to get a resource from a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The resource value if found.</param>
    /// <returns>True if the resource was found; otherwise, false.</returns>
    bool TryGetResource(object view, string key, out object? value);

    /// <summary>
    /// Sets the data context on a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="dataContext">The data context.</param>
    void SetDataContext(object view, object? dataContext);

    /// <summary>
    /// Gets the data context from a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <returns>The data context.</returns>
    object? GetDataContext(object view);

    /// <summary>
    /// Sets the Xtui tag on a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="tag">The tag.</param>
    void SetXtuiTag(object view, string? tag);

    /// <summary>
    /// Gets the Xtui tag from a view.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <returns>The tag.</returns>
    string? GetXtuiTag(object view);
}

/// <summary>
/// Provides binding helpers for Xtui.
/// </summary>
public interface IXtuiBindingHelpers
{
    /// <summary>
    /// Sets a binding on a target.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="targetPropertyName">The target property name.</param>
    /// <param name="binding">The binding.</param>
    void SetBinding(object target, string targetPropertyName, Binding binding);

    /// <summary>
    /// Notifies that the data context has changed.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="newDataContext">The new data context.</param>
    void NotifyDataContextChanged(object target, object? newDataContext);
}

/// <summary>
/// Static accessors for Xtui helpers.
/// </summary>
public static class Xtui
{
    /// <summary>
    /// Gets the application helpers.
    /// </summary>
    public static IXtuiAppHelpers App => throw new NotImplementedException();

    /// <summary>
    /// Gets the view helpers.
    /// </summary>
    public static IXtuiViewHelpers View => throw new NotImplementedException();

    /// <summary>
    /// Gets the binding helpers.
    /// </summary>
    public static IXtuiBindingHelpers Binding => throw new NotImplementedException();
}