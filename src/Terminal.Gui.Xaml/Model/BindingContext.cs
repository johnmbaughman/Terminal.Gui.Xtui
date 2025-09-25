namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Provides a context for data binding, including source and scope.
/// </summary>
public class BindingContext
{
    /// <summary>
    /// The data source for binding.
    /// </summary>
    public object? Source { get; set; }

    /// <summary>
    /// The scope of the binding context.
    /// </summary>
    public string Scope { get; set; } = string.Empty;
}
