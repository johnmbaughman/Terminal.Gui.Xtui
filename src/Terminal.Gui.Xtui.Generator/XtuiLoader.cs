using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XamlX;
using XamlX.Ast;
using XamlX.Parsers;

namespace Terminal.Gui.Xtui.Generator;

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

        Dictionary<string, string> namespaces = new ();
        XElement? xmlRoot = XDocument.Parse(xaml).Root;

        if (xmlRoot is null)
        {
            return ToElementNode (root, namespaces);
        }

        foreach (XAttribute? attr in xmlRoot.Attributes())
        {
            if (!attr.IsNamespaceDeclaration)
            {
                continue;
            }

            string prefix = attr.Name.LocalName == "xmlns" ? string.Empty : attr.Name.LocalName;
            namespaces[prefix] = attr.Value;
        }

        return ToElementNode(root, namespaces);
    }

    private static ElementNode ToElementNode(XamlAstObjectNode obj, Dictionary<string, string> namespaces)
    {
        ElementNode node = new()
        {
            Namespaces = namespaces,
            ElementTypeName = ResolveTypeName(obj.Type, namespaces)
        };

        List<string> textBuffer = new List<string>();

        foreach (IXamlAstNode? child in obj.Children)
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
                case XamlAstXmlDirective directive:
                    // Handle directives like x:Key by adding them to Attributes
                    HandleDirective(node, directive);
                    break;
            }
        }

        string innerText = string.Concat(textBuffer).Trim();
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

        string propertyName = nameProp.Name;

        foreach (IXamlAstValueNode? value in propNode.Values)
        {
            switch (value)
            {
                case XamlAstTextNode text:
                    node.Attributes[propertyName] = text.Text;
                    break;
                case XamlAstObjectNode objNode:
                    // Don't convert Resources to markup extensions - they should be children
                    if (propertyName != "Resources" && TryConvertMarkupExtension(objNode, out string markupText))
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

    private static void HandleDirective(ElementNode node, XamlAstXmlDirective directive)
    {
        // For x:Key and other XAML directives, use simplified name format
        // The namespace might be "x" (prefix) or the full namespace URI
        string attributeName;
        if (directive.Namespace == "x" || directive.Namespace == "http://schemas.microsoft.com/winfx/2006/xaml")
        {
            attributeName = "x:" + directive.Name;
        }
        else if (!string.IsNullOrEmpty(directive.Namespace))
        {
            attributeName = directive.Namespace + ":" + directive.Name;
        }
        else
        {
            attributeName = directive.Name;
        }
        
        // Get the directive value
        foreach (IXamlAstValueNode? value in directive.Values)
        {
            switch (value)
            {
                case XamlAstTextNode text:
                    node.Attributes[attributeName] = text.Text;
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

        List<string> parts = new List<string> { xmlType.Name };
        IEnumerable<IXamlAstValueNode> args = objNode.Arguments != null
            ? objNode.Arguments
            : Array.Empty<IXamlAstValueNode>();

        foreach (IXamlAstValueNode? arg in args)
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
            string? ns = xmlType.XmlNamespace ?? (namespaces.TryGetValue(string.Empty, out string? defaultNs) ? defaultNs : string.Empty);
            string mapped = MapNamespaceUri(ns);
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

        string nonNullUri = uri!;

        if (nonNullUri == "http://schemas.terminal.gui/xtui")
        {
            return "Terminal.Gui.Views";
        }

        if (nonNullUri.StartsWith("clr-namespace:", StringComparison.Ordinal))
        {
            string nsDeclaration = nonNullUri.Substring("clr-namespace:".Length);
            int assemblyIndex = nsDeclaration.IndexOf(';');
            if (assemblyIndex > 0)
            {
                return nsDeclaration.Substring(0, assemblyIndex);
            }
            return nsDeclaration;
        }

        return nonNullUri;
    }
}
