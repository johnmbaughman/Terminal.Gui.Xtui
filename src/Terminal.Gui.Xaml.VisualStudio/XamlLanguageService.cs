using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xaml.VisualStudio;

/// <summary>
/// Provides XAML IntelliSense, syntax highlighting, and navigation for Terminal.Gui.Xaml in Visual Studio.
/// </summary>
public class XamlLanguageService
{
    /// <summary>
    /// Gets completion items for XAML IntelliSense.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public IEnumerable<string> GetCompletions(string xaml, int position)
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Implement real IntelliSense logic
        return new[] { "Window", "StackView", "Label", "Button", "TextField" };
    }

    /// <summary>
    /// Gets navigation targets for "Go to Definition" in XAML.
    /// </summary>
#pragma warning disable CA1822 // Mark members as static
    public string? GetDefinition(string xaml, int position)
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Implement navigation logic
        return null;
    }
}
