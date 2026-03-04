namespace Terminal.Gui.Xtui;

/// <summary>
/// Represents a binding between a source property and a target property.
/// </summary>
public record Binding(string Path, object? Source = null, IValueConverter? Converter = null, object? ConverterParameter = null, string? ElementName = null);
