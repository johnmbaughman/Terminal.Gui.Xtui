namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents a parsed XAML template for instantiation and reuse.
/// </summary>
public class ParsedTemplate
{
    /// <summary>
    /// The key identifying the template.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The content of the parsed template.
    /// </summary>
    public object? Content { get; set; }
}
