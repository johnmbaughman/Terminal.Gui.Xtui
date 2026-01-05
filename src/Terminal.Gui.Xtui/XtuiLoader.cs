using System;
using System.Collections.Generic;
using System.Linq;
using XamlX;
using XamlX.Ast;
using XamlX.Parsers;

namespace Terminal.Gui.Xtui;

/// <summary>
/// XamlX-backed loader that produces the same <see cref="ElementNode"/> model as the legacy XML loader.
/// </summary>
public static class XtuiLoader
{
    public static ElementNode LoadFromString(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml))
        {
            throw new ArgumentException("XTUI string cannot be null or empty.", nameof(xaml));
        }

        XamlDocument doc;
        try
        {
            doc = XDocumentXamlParser.Parse(xaml);
        }
        catch (XamlParseException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        if (doc.Root is not XamlAstObjectNode root)
        {
            throw new InvalidOperationException("XTUI document has no root object.");
        }

        var namespaces = new Dictionary<string, string>(doc.NamespaceAliases);
        if (!namespaces.ContainsKey(string.Empty))
        {
            namespaces[string.Empty] = "http://schemas.terminal.gui/xtui";
        }

        return ToElementNode(root, namespaces);
    }

    private static ElementNode ToElementNode(XamlAstObjectNode obj, Dictionary<string, string> namespaces)
    {
        var node = new ElementNode
        {
            Namespaces = namespaces,
            ElementTypeName = ResolveTypeName(obj.Type, namespaces)
        };

        var textBuffer = new List<string>();

        foreach (var child in obj.Children)
        {
            switch (child)
            {
                case XamlAstXamlPropertyValueNode propNode:
                    HandleProperty(node, propNode, namespaces);
                    break;
                case XamlAstTextNode textNode:
                    textBuffer.Add(textNode.Text);
                    break;
                case XamlAstObjectNode childObj:
                    node.Children.Add(ToElementNode(childObj, namespaces));
                    break;
                case XamlAstXmlDirective:
                    // Ignore directives in the ElementNode projection for now.
                    break;
            }
        }

        var innerText = string.Concat(textBuffer).Trim();
        if (!string.IsNullOrEmpty(innerText))
        {
            node.InnerText = innerText;
        }

        return node;
    }

    private static void HandleProperty(ElementNode node, XamlAstXamlPropertyValueNode propNode, Dictionary<string, string> namespaces)
    {
        if (propNode.Property is not XamlAstNamePropertyReference nameProp)
        {
            return;
        }

        var propertyName = nameProp.Name;

        foreach (var value in propNode.Values)
        {
            switch (value)
            {
                case XamlAstTextNode text:
                    node.Attributes[propertyName] = text.Text;
                    break;
                case XamlAstObjectNode objNode:
                    node.Children.Add(ToElementNode(objNode, namespaces));
                    break;
            }
        }
    }

    private static string ResolveTypeName(IXamlAstTypeReference typeReference, Dictionary<string, string> namespaces)
    {
        if (typeReference is XamlAstXmlTypeReference xmlType)
        {
            var ns = xmlType.XmlNamespace ?? namespaces.GetValueOrDefault(string.Empty) ?? string.Empty;
            var mapped = MapNamespaceUri(ns);
            return mapped + "." + xmlType.Name;
        }

        return MapNamespaceUri(string.Empty) + "." + typeReference.ToString();
    }

    /// <summary>
    /// Maps an XML namespace URI to a C# namespace.
    /// Supports XAML-style clr-namespace syntax: clr-namespace:Namespace.Name or clr-namespace:Namespace.Name;assembly=AssemblyName
    /// </summary>
    private static string MapNamespaceUri(string? uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return "Terminal.Gui.Views";
        }

        if (uri == "http://schemas.terminal.gui/xtui")
        {
            return "Terminal.Gui.Views";
        }

        if (uri.StartsWith("clr-namespace:", StringComparison.Ordinal))
        {
            var nsDeclaration = uri.Substring("clr-namespace:".Length);
            var assemblyIndex = nsDeclaration.IndexOf(';');
            if (assemblyIndex > 0)
            {
                return nsDeclaration.Substring(0, assemblyIndex);
            }
            return nsDeclaration;
        }

        return uri;
    }
}