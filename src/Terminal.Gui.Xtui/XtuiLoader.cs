using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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

        var namespaces = new Dictionary<string, string>();
        var xmlRoot = XDocument.Parse(xaml).Root;
        if (xmlRoot is not null)
        {
            foreach (var attr in xmlRoot.Attributes())
            {
                if (!attr.IsNamespaceDeclaration)
                {
                    continue;
                }

                var prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
                namespaces[prefix] = attr.Value;
            }
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
                    if (TryConvertMarkupExtension(objNode, out var markupText))
                    {
                        node.Attributes[propertyName] = markupText;
                    }
                    else
                    {
                        node.Children.Add(ToElementNode(objNode, namespaces));
                    }
                    break;
            }
        }
    }

    private static bool TryConvertMarkupExtension(XamlAstObjectNode objNode, out string markup)
    {
        markup = string.Empty;

        if (objNode.Type is not XamlAstXmlTypeReference xmlType)
        {
            return false;
        }

        var parts = new List<string> { xmlType.Name };
        IEnumerable<IXamlAstValueNode> args = objNode.Arguments != null
            ? objNode.Arguments
            : Array.Empty<IXamlAstValueNode>();

        foreach (var arg in args)
        {
            switch (arg)
            {
                case XamlAstTextNode textArg:
                    parts.Add(textArg.Text);
                    break;
                default:
                    return false;
            }
        }

        markup = "{" + string.Join(" ", parts) + "}";
        return true;
    }

    private static string ResolveTypeName(IXamlAstTypeReference typeReference, Dictionary<string, string> namespaces)
    {
        if (typeReference is XamlAstXmlTypeReference xmlType)
        {
            var ns = xmlType.XmlNamespace ?? (namespaces.TryGetValue(string.Empty, out var defaultNs) ? defaultNs : string.Empty);
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

        var nonNullUri = uri!;

        if (nonNullUri == "http://schemas.terminal.gui/xtui")
        {
            return "Terminal.Gui.Views";
        }

        if (nonNullUri.StartsWith("clr-namespace:", StringComparison.Ordinal))
        {
            var nsDeclaration = nonNullUri.Substring("clr-namespace:".Length);
            var assemblyIndex = nsDeclaration.IndexOf(';');
            if (assemblyIndex > 0)
            {
                return nsDeclaration.Substring(0, assemblyIndex);
            }
            return nsDeclaration;
        }

        return nonNullUri;
    }
}