namespace Terminal.Gui.Xtui;

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
