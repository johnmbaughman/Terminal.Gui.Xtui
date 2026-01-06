using System;
using System.Linq;

namespace Terminal.Gui.Xtui.Helpers;

/// <summary>
/// Helper methods for manipulating type names in code generation.
/// Provides utilities for extracting simple type names from fully qualified names,
/// converting to variable naming conventions, and checking type name formats.
/// </summary>
internal static class TypeNameHelpers
{
    /// <summary>
    /// Extracts the simple type name from a fully qualified type name.
    /// If the type name contains dots (namespace qualifiers), returns only the last segment.
    /// Otherwise, returns the type name unchanged.
    /// </summary>
    /// <param name="fullTypeName">The fully qualified type name (e.g., "Terminal.Gui.Label" or "Button")</param>
    /// <returns>The simple type name (e.g., "Label" or "Button")</returns>
    /// <exception cref="ArgumentNullException">Thrown when fullTypeName is null</exception>
    /// <example>
    /// ExtractLocalTypeName("Terminal.Gui.Label") returns "Label"
    /// ExtractLocalTypeName("Button") returns "Button"
    /// </example>
    public static string ExtractLocalTypeName(string fullTypeName)
    {
        if (fullTypeName == null)
        {
            throw new ArgumentNullException(nameof(fullTypeName));
        }

        if (string.IsNullOrEmpty(fullTypeName))
        {
            return string.Empty;
        }

        return fullTypeName.Contains('.') 
            ? fullTypeName.Split('.').Last() 
            : fullTypeName;
    }

    /// <summary>
    /// Converts a type name to camelCase variable naming convention.
    /// The first character is converted to lowercase, subsequent characters remain unchanged.
    /// </summary>
    /// <param name="typeName">The type name to convert (e.g., "Label", "TextField")</param>
    /// <returns>The camelCase variable name (e.g., "label", "textField")</returns>
    /// <exception cref="ArgumentNullException">Thrown when typeName is null</exception>
    /// <example>
    /// ToVariableName("Label") returns "label"
    /// ToVariableName("TextField") returns "textField"
    /// ToVariableName("MyView") returns "myView"
    /// </example>
    public static string ToVariableName(string typeName)
    {
        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        if (string.IsNullOrEmpty(typeName))
        {
            return string.Empty;
        }

        // Convert first character to lowercase, keep rest as-is
        return char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);
    }

    /// <summary>
    /// Creates an indexed variable name by extracting the simple type name,
    /// converting to camelCase, and appending the index.
    /// </summary>
    /// <param name="fullTypeName">The fully qualified type name (e.g., "Terminal.Gui.Label")</param>
    /// <param name="index">The zero-based index to append</param>
    /// <returns>The indexed variable name (e.g., "label0", "button1")</returns>
    /// <exception cref="ArgumentNullException">Thrown when fullTypeName is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is negative</exception>
    /// <example>
    /// CreateIndexedVariableName("Terminal.Gui.Label", 0) returns "label0"
    /// CreateIndexedVariableName("Button", 5) returns "button5"
    /// </example>
    public static string CreateIndexedVariableName(string fullTypeName, int index)
    {
        if (fullTypeName == null)
        {
            throw new ArgumentNullException(nameof(fullTypeName));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative");
        }

        string localTypeName = ExtractLocalTypeName(fullTypeName);
        string variableName = ToVariableName(localTypeName);
        return $"{variableName}{index}";
    }

    /// <summary>
    /// Checks whether a type name is simple (no namespace qualifiers) or fully qualified.
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <returns>True if the type name contains no dots (is simple), false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when typeName is null</exception>
    /// <example>
    /// IsSimpleTypeName("Label") returns true
    /// IsSimpleTypeName("Terminal.Gui.Label") returns false
    /// IsSimpleTypeName("") returns false
    /// </example>
    public static bool IsSimpleTypeName(string typeName)
    {
        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        if (string.IsNullOrEmpty(typeName))
        {
            return false;
        }

        return !typeName.Contains('.');
    }
}
