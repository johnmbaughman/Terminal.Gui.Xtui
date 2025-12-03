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
public static class XtuiLoader
{
    /// <summary>
    /// Loads XTUI from a string and converts it to an ElementNode tree.
    /// XML comments are automatically ignored during parsing.
    /// </summary>
    /// <param name="xaml">The XTUI string to parse.</param>
    /// <returns>An ElementNode representing the root element.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the XTUI string is null or empty.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when the XTUI document has no root element.</exception>
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
    private static ElementNode FromXElement (XElement el)
    {
        ElementNode node = new ElementNode { ElementTypeName = el.Name.LocalName };

        foreach (XAttribute? attr in el.Attributes ())
        {
            node.Attributes [attr.Name.LocalName] = attr.Value;
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
            node.Children.Add (FromXElement (child));
        }

        return node;
    }
}