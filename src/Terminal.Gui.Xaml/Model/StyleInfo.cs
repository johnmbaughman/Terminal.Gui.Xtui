namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents style information for controls, including visual properties and triggers.
/// </summary>
public class StyleInfo
{
    /// <summary>
    /// The selector for the style (e.g., control type or class).
    /// </summary>
    public string Selector { get; set; } = string.Empty;

    /// <summary>
    /// The property setters for the style.
    /// </summary>
    public Dictionary<string, object> Setters { get; set; } = new();
}
