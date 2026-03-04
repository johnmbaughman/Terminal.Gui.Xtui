using System.Collections.Generic;

namespace Terminal.Gui.Xtui;

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
