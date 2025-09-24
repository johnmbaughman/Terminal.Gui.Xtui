// <copyright file="XamlDocument.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Model;

namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents a XAML document and provides validation logic.
/// </summary>
public class XamlDocument
{
    private readonly List<XamlNamespace> _namespaces = [];

    /// <summary>
    /// Gets or sets the XAML text.
    /// </summary>
    public string? XamlText { get; set; }

    /// <summary>
    /// Gets or sets the file path.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the root namespace.
    /// </summary>
    public XamlNamespace? RootNamespace { get; set; }

    /// <summary>
    /// Gets the collection of namespaces.
    /// </summary>
    public IReadOnlyList<XamlNamespace> Namespaces => _namespaces;

    /// <summary>
    /// Validates the XAML document against Microsoft XAML standards.
    /// </summary>
    /// <returns>
    /// A tuple containing a boolean indicating validity and a collection of error messages.
    /// </returns>
    public (bool IsValid, System.Collections.ObjectModel.Collection<string> Errors) Validate ()
    {
        System.Collections.ObjectModel.Collection<string> errors = [];

        if (string.IsNullOrWhiteSpace (XamlText))
        {
            errors.Add ("XAML text is empty.");
        }

        if (RootNamespace == null)
        {
            errors.Add ("Root namespace is not set.");
        }

        if (Namespaces.Count == 0)
        {
            errors.Add ("No namespaces defined.");
        }

        // Add more comprehensive validation logic as needed.
        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Returns the string representation of the XAML document.
    /// </summary>
    /// <returns>The XAML text.</returns>
    public override string ToString () => XamlText ?? string.Empty;
}

