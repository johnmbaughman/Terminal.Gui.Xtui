// <copyright file="XamlNamespace.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents a XAML namespace.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XamlNamespace"/> class.
/// </remarks>
/// <param name="uri">The namespace URI.</param>
public class XamlNamespace (System.Uri uri)
{

    /// <summary>
    /// Gets the URI of the XAML namespace.
    /// </summary>
    public System.Uri Uri { get; } = uri;
}
