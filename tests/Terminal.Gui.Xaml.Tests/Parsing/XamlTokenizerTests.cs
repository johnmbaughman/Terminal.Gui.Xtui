using System.Linq;
using Terminal.Gui.Xaml.Parsing;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Parsing;

public class XamlTokenizerTests
{
    [Fact]
    public void Tokenize_ValidXaml_ReturnsElements()
    {
        var elements = XamlTokenizer.Tokenize("<Window><Button /></Window>").ToList();
        Assert.True(elements.Count > 0);
        Assert.Equal("Window", elements[0].Name.LocalName);
    }

    [Fact]
    public void Tokenize_InvalidXaml_Throws()
    {
        Assert.Throws<System.Xml.XmlException>(() => XamlTokenizer.Tokenize("<Window>").ToList());
    }
}
