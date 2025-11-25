using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xaml.Mappers;

internal static class EnumMapper
{
    private static readonly Dictionary<string, Dictionary<string, string>> _enumMappings = new()
    {
        ["TextAlignment"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = "Terminal.Gui.TextAlignment.Left",
            ["Right"] = "Terminal.Gui.TextAlignment.Right",
            ["Center"] = "Terminal.Gui.TextAlignment.Centered",
            ["Justified"] = "Terminal.Gui.TextAlignment.Justified"
        },
        ["BorderStyle"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["None"] = "Terminal.Gui.BorderStyle.None",
            ["Single"] = "Terminal.Gui.BorderStyle.Single",
            ["Double"] = "Terminal.Gui.BorderStyle.Double",
            ["Rounded"] = "Terminal.Gui.BorderStyle.Rounded"
        }
    };

    public static string GetEnumValue(string enumType, string value)
    {
        if (_enumMappings.TryGetValue(enumType, out var mapping) &&
            mapping.TryGetValue(value, out var enumValue))
        {
            return enumValue;
        }
        
        // Fallback: assume fully qualified name
        return $"Terminal.Gui.{enumType}.{value}";
    }

    public static bool IsEnumProperty(string propertyName)
    {
        return _enumMappings.ContainsKey(propertyName);
    }
}
