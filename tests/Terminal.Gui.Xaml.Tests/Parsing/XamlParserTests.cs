using System.Threading.Tasks;
using Terminal.Gui.Xaml.Parsing;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Parsing;

public class XamlParserTests
{
    [Fact]
    public async Task ParseAsync_ValidXaml_ReturnsDocument()
    {
        var parser = new XamlParser();
        var doc = await parser.ParseAsync("<Window Title=\"Hello\" />");
        Assert.NotNull(doc);
    }

    [Fact]
    public async Task ParseAsync_EmptyXaml_Throws()
    {
        var parser = new XamlParser();
        await Assert.ThrowsAsync<Terminal.Gui.Xaml.Exceptions.XamlParseException>(() => parser.ParseAsync("   "));
    }

    [Fact]
    public void Validate_ValidXaml_ReturnsTrue()
    {
        var parser = new XamlParser();
        Assert.True(parser.Validate("<Window />"));
    }

    [Fact]
    public void Validate_InvalidXaml_ReturnsFalse()
    {
        var parser = new XamlParser();
        Assert.False(parser.Validate("<Window>"));
    }
}
