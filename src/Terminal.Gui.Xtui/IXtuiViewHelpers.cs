namespace Terminal.Gui.Xtui;

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
