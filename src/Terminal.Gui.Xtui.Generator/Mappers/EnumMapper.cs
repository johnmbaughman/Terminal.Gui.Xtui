using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Generator.Mappers;

/// <summary>
/// Provides mappings from short enum names/values used in .xtui files to
/// fully-qualified enum member expressions in the <c>Terminal.Gui</c>
/// namespace.
///
/// This helper is used by generators to translate attribute values such as
/// <c>TextAlignment="Center"</c> into the correct C# expression
/// <c>Terminal.Gui.TextAlignment.Centered</c>.
/// </summary>
internal static class EnumMapper
{
    private static readonly Dictionary<string, Dictionary<string, string>> EnumMappings = new ()
    {
        ["TextAlignment"] = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = "Terminal.Gui.TextAlignment.Left",
            ["Right"] = "Terminal.Gui.TextAlignment.Right",
            ["Center"] = "Terminal.Gui.TextAlignment.Centered",
            ["Justified"] = "Terminal.Gui.TextAlignment.Justified"
        },
        ["BorderStyle"] = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase)
        {
            ["None"] = "Terminal.Gui.BorderStyle.None",
            ["Single"] = "Terminal.Gui.BorderStyle.Single",
            ["Double"] = "Terminal.Gui.BorderStyle.Double",
            ["Rounded"] = "Terminal.Gui.BorderStyle.Rounded"
        },
        ["CheckState"] = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase)
        {
            ["UnChecked"] = "Terminal.Gui.Views.CheckState.UnChecked",
            ["Checked"] = "Terminal.Gui.Views.CheckState.Checked",
            ["None"] = "Terminal.Gui.Views.CheckState.None"
        }
    };

    /// <summary>
    /// Returns a fully-qualified enum member expression for the provided
    /// <paramref name="enumType"/> and <paramref name="value"/>.
    /// </summary>
    /// <param name="enumType">The enum type name (for example, <c>TextAlignment</c>).</param>
    /// <param name="value">The enum member name as used in .xtui files (for example, <c>Center</c>).</param>
    /// <returns>
    /// A fully-qualified enum expression (for example, <c>Terminal.Gui.TextAlignment.Centered</c>).
    /// If no mapping is found, returns <c>Terminal.Gui.{enumType}.{value}</c> as a fallback.
    /// </returns>
    public static string GetEnumValue (string enumType, string value)
    {
        if (EnumMappings.TryGetValue (enumType, out Dictionary<string, string>? mapping) &&
            mapping.TryGetValue (value, out string? enumValue))
        {
            return enumValue;
        }

        // Special-case a few enums that live in different namespaces or have
        // non-standard naming. For example, `Command` lives in `Terminal.Gui.Input`.
        if (string.Equals (enumType, "Command", StringComparison.OrdinalIgnoreCase))
        {
            return $"Terminal.Gui.Input.Command.{value}";
        }

        // AlignmentModes lives in Terminal.Gui.ViewBase
        if (string.Equals (enumType, "AlignmentModes", StringComparison.OrdinalIgnoreCase))
        {
            return $"Terminal.Gui.ViewBase.AlignmentModes.{value}";
        }

        // MouseState lives in Terminal.Gui.ViewBase
        if (string.Equals (enumType, "MouseState", StringComparison.OrdinalIgnoreCase))
        {
            return $"Terminal.Gui.ViewBase.MouseState.{value}";
        }

        // Fallback: assume fully qualified name in the `Terminal.Gui` root namespace
        return $"Terminal.Gui.{enumType}.{value}";
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="propertyName"/> corresponds to
    /// a known enum mapping supported by this mapper. Generators use this to
    /// decide whether to call <see cref="GetEnumValue"/> for a property.
    /// </summary>
    /// <param name="propertyName">The property name to check (for example, <c>TextAlignment</c>).</param>
    /// <returns><c>true</c> if the property is a recognized enum type; otherwise <c>false</c>.</returns>
    public static bool IsEnumProperty (string propertyName)
    {
        return EnumMappings.ContainsKey (propertyName);
    }
}
