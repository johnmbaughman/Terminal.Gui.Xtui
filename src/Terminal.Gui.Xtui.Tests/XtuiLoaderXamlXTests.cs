using Terminal.Gui.Xtui;

namespace Terminal.Gui.Xtui.Tests;

public class XtuiLoaderXamlXTests
{
    [Fact]
    public void XamlX_Loader_Matches_Xml_For_Simple_Tree()
    {
        var xtui = "<Window Title=\"Main\" Width=\"80\">\n  <Label Text=\"Hello\" />\n</Window>";

        var expected = XtuiLoaderXml.LoadFromString(xtui);
        var actual = XtuiLoader.LoadFromString(xtui);

        AssertElementEqual(actual, expected);
    }

    [Fact]
    public void XamlX_Loader_Matches_Xml_For_Xmlns()
    {
        var xtui = "<Window xmlns:my=\"MyNamespace\" Title=\"Main\"><my:CustomControl Text=\"Hello\" /></Window>";

        var expected = XtuiLoaderXml.LoadFromString(xtui);
        var actual = XtuiLoader.LoadFromString(xtui);

        AssertElementEqual(actual, expected);
    }

    private static void AssertElementEqual(ElementNode actual, ElementNode expected)
    {
        Assert.Equal(expected.ElementTypeName, actual.ElementTypeName);
        Assert.Equal(expected.InnerText, actual.InnerText);
        Assert.Equal(expected.Namespaces, actual.Namespaces);
        Assert.Equal(expected.Attributes, actual.Attributes);
        Assert.Equal(expected.Children.Count, actual.Children.Count);

        for (var i = 0; i < expected.Children.Count; i++)
        {
            AssertElementEqual(actual.Children[i], expected.Children[i]);
        }
    }
}
