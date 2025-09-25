namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Describes a control's metadata, type, and properties for runtime instantiation.
/// </summary>
public class ControlInfo
{
    /// <summary>
    /// The name of the control.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of the control (e.g., Button, Label).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The property dictionary for the control.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}
