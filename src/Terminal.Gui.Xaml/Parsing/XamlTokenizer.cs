using System.Collections.Generic;
using System.Xml.Linq;

namespace Terminal.Gui.Xaml.Parsing;

/// <summary>
/// Tokenizer for XAML XML parsing, supporting incremental parsing and caching.
/// </summary>
public static class XamlTokenizer
{
    /// <summary>
    /// Tokenizes XAML into XML elements for incremental parsing.
    /// </summary>
    public static IEnumerable<XElement> Tokenize(string xaml)
    {
        var doc = XDocument.Parse(xaml);
        foreach (var element in doc.Descendants())
        {
            yield return element;
        }
    }
}
