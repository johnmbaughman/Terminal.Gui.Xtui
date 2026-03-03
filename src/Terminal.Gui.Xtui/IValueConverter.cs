using System;

namespace Terminal.Gui.Xtui;

/// <summary>
/// Interface for value conversion in bindings.
/// </summary>
public interface IValueConverter
{
    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The culture to use in the converter.</param>
    /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
    object? Convert(object? value, Type targetType, object? parameter, string? language);

    /// <summary>
    /// Converts a value back.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The culture to use in the converter.</param>
    /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
    object? ConvertBack(object? value, Type targetType, object? parameter, string? language);
}