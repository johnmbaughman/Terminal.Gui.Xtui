// <copyright file="XamlDocument.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Represents a XAML document with namespace support and validation.
/// </summary>
public class XamlDocument
{
    public string RawContent { get; }
    public XDocument Xml { get; }
    public IReadOnlyDictionary<string, XamlNamespace> Namespaces { get; }

    public XamlDocument(string rawContent)
    {
        RawContent = rawContent ?? throw new ArgumentNullException(nameof(rawContent));
        Xml = XDocument.Parse(rawContent);
        Namespaces = ParseNamespaces(Xml);
    }

    private static Dictionary<string, XamlNamespace> ParseNamespaces(XDocument xml)
    {
        var nsDict = new Dictionary<string, XamlNamespace>();
        foreach (var attr in xml.Root.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                nsDict[attr.Name.LocalName] = new XamlNamespace(attr.Value);
            }
        }
        return nsDict;
    }

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1021 // Avoid out parameters
    public bool Validate(out List<string> errors)
#pragma warning restore CA1021 // Avoid out parameters
#pragma warning restore CA1822 // Mark members as static
    {
        errors = new List<string>();
        // TODO: Implement validation logic for Microsoft XAML standards
        return errors.Count == 0;
    }

    public override string ToString() => Xml.ToString();
}
