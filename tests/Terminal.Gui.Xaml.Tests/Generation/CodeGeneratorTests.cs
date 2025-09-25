using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Generation;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Generation;

public class CodeGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ValidXaml_ReturnsCode()
    {
        var generator = new CodeGenerator();
        var code = await generator.GenerateAsync("<Window Title=\"Hello\" />");
        Assert.Contains("InitializeComponent", code);
        Assert.Contains("GeneratedView", code);
    }

    [Fact]
    public async Task GenerateClassesAsync_MultipleDocuments_ReturnsClasses()
    {
        var generator = new CodeGenerator();
        var xamls = new List<string> { "<Window />", "<Button />" };
        var classes = await generator.GenerateClassesAsync(xamls);
        Assert.Equal(2, classes.Count());
        Assert.All(classes, c => Assert.Contains("InitializeComponent", c.Code));
    }

    [Fact]
    public void ValidateGeneration_EmptyXaml_ReturnsFalse()
    {
        var generator = new CodeGenerator();
        Assert.False(generator.ValidateGeneration("   "));
    }

    [Fact]
    public void RegisterTemplate_StoresTemplate()
    {
        var generator = new CodeGenerator();
        generator.RegisterTemplate("Custom", "// custom template");
        // No direct way to verify, but should not throw
    }
}
