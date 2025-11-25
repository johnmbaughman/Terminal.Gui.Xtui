using System.Linq;
using System.Xml.Linq;

namespace Terminal.Gui.Xaml;

public static class XamlLoader
{
    /// <summary>
    /// Loads XAML from a string and converts it to an ElementNode tree.
    /// XML comments are automatically ignored during parsing.
    /// </summary>
    /// <param name="xaml">The XAML string to parse.</param>
    /// <returns>An ElementNode representing the root element, or null if the XAML is empty or invalid.</returns>
    public static ElementNode LoadFromString(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml)) return null;
        var doc = XDocument.Parse(xaml);
        return doc.Root is null ? null : FromXElement(doc.Root);
    }

    /// <summary>
    /// Recursively converts an XElement to an ElementNode.
    /// Only processes XElement nodes and XText nodes; XComment nodes are automatically ignored.
    /// </summary>
    private static ElementNode FromXElement(XElement el)
    {
        var node = new ElementNode { Name = el.Name.LocalName };

        foreach (var attr in el.Attributes())
            node.Attributes[attr.Name.LocalName] = attr.Value;

        // Only process text nodes; comments are ignored
        var txt = string.Concat(el.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
        if (!string.IsNullOrEmpty(txt))
            node.InnerText = txt;

        // Only process element nodes; comments are ignored
        foreach (var child in el.Elements())
            node.Children.Add(FromXElement(child));

        return node;
    }
}