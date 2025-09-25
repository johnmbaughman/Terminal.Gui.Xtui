namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents the result of a validation operation, including errors and status.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the validation was successful.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The collection of validation error messages.
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyCollection<string> Errors { get; set; } = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new List<string>());
}
