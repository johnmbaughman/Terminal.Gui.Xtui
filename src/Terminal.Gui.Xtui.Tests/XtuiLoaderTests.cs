using Terminal.Gui.Xtui.Generator;

namespace Terminal.Gui.Xtui.Tests;

public class XtuiLoaderTests
{
    [Fact]
    public void LoadXtui_ParsesResourcesWithXKey()
    {
        // Arrange
        string xtuiContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Runnable xmlns=""http://schemas.terminal.gui/xtui""
    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
    xmlns:Converters=""clr-namespace:TestNamespace;assembly=TestAssembly""
    Title=""Test"">
    <Runnable.Resources>
        <Converters:TestConverter x:Key=""MyConverter"" />
    </Runnable.Resources>
    <MenuBar Title=""Test"" />
</Runnable>";

        // Act
        var result = XtuiLoader.LoadFromString(xtuiContent);

        // Assert  
        Assert.NotNull(result);
        Assert.Contains("Runnable", result.ElementTypeName);
        
        // Check children - should have MenuBar and TestConverter
        // First, let's see what we actually have
        var childInfo = string.Join("\n", result.Children.Select((c, i) => 
            $"  Child {i}: Type={c.ElementTypeName}, Attributes={string.Join(",", c.Attributes.Keys)}"));
        Assert.True(result.Children.Count >= 2, $"Expected at least 2 children (converter + MenuBar), got {result.Children.Count}:\n{childInfo}");
        
        // Find the converter child
        var converter = result.Children.FirstOrDefault(c => c.ElementTypeName.Contains("TestConverter"));
        Assert.NotNull(converter);
        
        // Check for x:Key attribute
        Assert.True(converter.Attributes.ContainsKey("x:Key"), "Converter should have x:Key attribute");
        Assert.Equal("MyConverter", converter.Attributes["x:Key"]);
    }
}
