using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Terminal.Gui.Xtui;

/// <summary>
/// Utility for loading XTUI markup into the generator model.
///
/// Parses an XTUI string into an <see cref="ElementNode"/> tree, preserving
/// element names, attributes and inner text while ignoring XML comments.
/// Generators consume the resulting <see cref="ElementNode"/> instances to
/// produce C# code.
/// </summary>
public static class XtuiLoaderXml
{
    /// <summary>
    /// Maps an XML namespace URI to a C# namespace.
    /// Supports XAML-style clr-namespace syntax: clr-namespace:Namespace.Name or clr-namespace:Namespace.Name;assembly=AssemblyName
    /// </summary>
    /// <param name="uri">The XML namespace URI to map.</param>
    /// <returns>The corresponding C# namespace.</returns>
    private static string MapNamespaceUri(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return "Terminal.Gui.Views";
        }
        if (uri == "http://schemas.terminal.gui/xtui")
        {
            return "Terminal.Gui.Views";
        }
        
        // Parse XAML-style clr-namespace declarations
        // Format: clr-namespace:MyApp.ViewModels or clr-namespace:MyApp.ViewModels;assembly=MyAssembly
        if (uri.StartsWith("clr-namespace:"))
        {
            string nsDeclaration = uri.Substring("clr-namespace:".Length);
            int assemblyIndex = nsDeclaration.IndexOf(";");
            if (assemblyIndex > 0)
            {
                // Extract namespace before assembly reference
                return nsDeclaration.Substring(0, assemblyIndex);
            }
            return nsDeclaration;
        }
        
        // Legacy support: plain namespace strings are used as-is
        return uri;
    }

    /// <summary>
    /// Resolves an element name with optional namespace prefix to a fully qualified type name.
    /// </summary>
    /// <param name="elementName">The element name, possibly with a namespace prefix (e.g., "vm:LoginViewModel").</param>
    /// <param name="defaultNamespaceUri">The default namespace URI (xmlns without prefix).</param>
    /// <param name="namespaces">Dictionary of namespace prefixes to URIs.</param>
    /// <returns>Fully qualified type name.</returns>
    private static string ResolveElementTypeName(string elementName, string defaultNamespaceUri, Dictionary<string, string> namespaces)
    {
        string nsUri;
        string localName;

        if (elementName.Contains(':'))
        {
            // Element has a namespace prefix: vm:LoginViewModel
            int colonIndex = elementName.IndexOf(':');
            string prefix = elementName.Substring(0, colonIndex);
            localName = elementName.Substring(colonIndex + 1);

            if (namespaces.TryGetValue(prefix, out string? prefixNs))
            {
                nsUri = prefixNs;
            }
            else
            {
                throw new InvalidOperationException($"Namespace prefix '{prefix}' is not defined for element '{elementName}'.");
            }
        }
        else
        {
            // No prefix, use default namespace
            localName = elementName;
            nsUri = defaultNamespaceUri;
        }

        string csNamespace = MapNamespaceUri(nsUri);
        return csNamespace + "." + localName;
    }
    public static ElementNode LoadFromString (string xaml)
    {
        if (string.IsNullOrWhiteSpace (xaml))
        {
            throw new System.ArgumentException ("XTUI string cannot be null or empty.", nameof (xaml));
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse (xaml);
        }
        catch (System.Xml.XmlException ex)
        {
            // Wrap XML parsing errors as InvalidOperationException so callers
            // can treat them as XTUI parsing/validation errors.
            throw new System.InvalidOperationException (ex.Message, ex);
        }
        if (doc.Root is null)
        {
            throw new System.InvalidOperationException ("XTUI document has no root element.");
        }

        return FromXElement (doc.Root);
    }

    /// <summary>
    /// Recursively converts an XElement to an ElementNode.
    /// Only processes XElement nodes and XText nodes; XComment nodes are automatically ignored.
    /// </summary>
    private static ElementNode FromXElement (XElement el, Dictionary<string, string>? parentNamespaces = null)
    {
        var namespaces = parentNamespaces != null ? new Dictionary<string, string>(parentNamespaces) : new Dictionary<string, string>();

        // Parse xmlns attributes
        foreach (XAttribute? attr in el.Attributes ())
        {
            if (attr.Name.LocalName == "xmlns")
            {
                // Default namespace: xmlns="namespace"
                namespaces[""] = attr.Value;
            }
            else if (attr.Name.NamespaceName == "http://www.w3.org/2000/xmlns/" && attr.Name.LocalName != "xmlns")
            {
                // Prefixed namespace: xmlns:prefix="namespace"
                namespaces[attr.Name.LocalName] = attr.Value;
            }
        }

        ElementNode node = new ElementNode { Namespaces = namespaces };

        // Resolve element type name with namespace support
        // XElement.Name.NamespaceName contains the resolved namespace URI from xmlns declarations
        // This automatically handles prefixed elements like <mah:MetroWindow xmlns:mah="...">
        // The XML parser resolves the prefix to the full namespace URI, so we just need to:
        // 1. Get the namespace URI (already resolved by XML parser)
        // 2. Map it to a C# namespace using MapNamespaceUri (handles clr-namespace: syntax)
        // 3. Combine with the local element name
        string nsUri = el.Name.NamespaceName;
        string localName = el.Name.LocalName;
        string csNamespace = MapNamespaceUri(nsUri);
        node.ElementTypeName = csNamespace + "." + localName;

        // Process other attributes (skip xmlns)
        foreach (XAttribute? attr in el.Attributes ())
        {
            if (attr.Name.LocalName == "xmlns" || attr.Name.NamespaceName == "http://www.w3.org/2000/xmlns/")
            {
                continue;
            }
            node.Attributes[attr.Name.LocalName] = attr.Value;
        }

        // Only process text nodes; comments are ignored
        string txt = string.Concat (el.Nodes ().OfType<XText> ().Select (t => t.Value)).Trim ();
        if (!string.IsNullOrEmpty (txt))
        {
            node.InnerText = txt;
        }

        // Only process element nodes; comments are ignored
        foreach (XElement? child in el.Elements ())
        {
            node.Children.Add (FromXElement (child, namespaces));
        }

        return node;
    }
}
