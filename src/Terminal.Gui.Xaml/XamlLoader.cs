using System.Linq;
using System.Xml.Linq;

namespace Terminal.Gui.Xaml;

public static class XamlLoader
{
    public static ElementNode LoadFromString(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml)) return null;
        var doc = XDocument.Parse(xaml);
        return doc.Root is null ? null : FromXElement(doc.Root);
    }

    private static ElementNode FromXElement(XElement el)
    {
        var node = new ElementNode { Name = el.Name.LocalName };

        foreach (var attr in el.Attributes())
            node.Attributes[attr.Name.LocalName] = attr.Value;

        var txt = string.Concat(el.Nodes().OfType<XText>().Select(t => t.Value)).Trim();
        if (!string.IsNullOrEmpty(txt))
            node.InnerText = txt;

        foreach (var child in el.Elements())
            node.Children.Add(FromXElement(child));

        return node;
    }
}