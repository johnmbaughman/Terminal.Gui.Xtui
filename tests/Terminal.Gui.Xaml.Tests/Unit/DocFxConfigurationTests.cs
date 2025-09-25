using System.ComponentModel.DataAnnotations;
using Terminal.Gui.Xaml.Documentation.Models;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class DocFxConfigurationTests
{
    private static IList<ValidationResult> Validate(object instance)
    {
        var ctx = new ValidationContext(instance);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Invalid_When_Required_Fields_Missing()
    {
        var cfg = new DocFxConfiguration();
        var results = Validate(cfg);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("ProjectName"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Version"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("OutputPath"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("SourcePaths"));
    }

    [Fact]
    public void Valid_With_Minimum_Required_Fields()
    {
        var cfg = new DocFxConfiguration
        {
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = "out",
            SourcePaths = new []{"src"}
        };
        var results = Validate(cfg);
        Assert.Empty(results);
    }

    [Fact]
    public void MetadataConfiguration_Validates_Src_And_Dest()
    {
        var meta = new MetadataConfiguration();
        var results = Validate(meta);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Src"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Dest"));

        meta = new MetadataConfiguration
        {
            Src = new[]{ new SourceConfiguration{ Files = new[]{"**/*.cs"}, Src = "src" }},
            Dest = "api"
        };
        results = Validate(meta);
        Assert.Empty(results);
    }

    [Fact]
    public void SourceConfiguration_Validates_Required_Collections()
    {
        var src = new SourceConfiguration();
        var results = Validate(src);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Files"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Src"));

        src = new SourceConfiguration
        {
            Files = new[]{"**/*.cs"},
            Src = "src"
        };
        results = Validate(src);
        Assert.Empty(results);
    }
}
