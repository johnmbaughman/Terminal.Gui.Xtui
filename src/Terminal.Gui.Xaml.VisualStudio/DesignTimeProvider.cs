using System;

namespace Terminal.Gui.Xaml.VisualStudio;

/// <summary>
/// Provides design-time metadata and error highlighting for Terminal.Gui.Xaml in Visual Studio.
/// </summary>
public class DesignTimeProvider
{
    /// <summary>
    /// Gets design-time errors for a given XAML document.
    /// </summary>
    public IEnumerable<string> GetErrors(string xaml)
    {
        // TODO: Implement error highlighting logic
        return Array.Empty<string>();
    }

    /// <summary>
    /// Gets property grid metadata for a given XAML element.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public IDictionary<string, object> GetPropertyMetadata(string elementName)
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Implement property grid integration
        return new Dictionary<string, object>();
    }
}
