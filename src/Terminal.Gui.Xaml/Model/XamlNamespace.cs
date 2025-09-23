// <copyright file="XamlNamespace.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents a XAML namespace.
/// </summary>
public class XamlNamespace
{
    public string Uri { get; }

    public XamlNamespace(string uri)
    {
        Uri = uri;
    }
}
