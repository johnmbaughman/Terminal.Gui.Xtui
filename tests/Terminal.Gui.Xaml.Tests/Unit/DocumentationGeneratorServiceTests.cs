using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class DocumentationGeneratorServiceTests
{
    private static DocFxConfiguration ValidConfig() => new()
    {
        ProjectName = "Terminal.Gui.Xaml",
        Version = "1.0.0",
        OutputPath = Path.Combine(Path.GetTempPath(), "tgxaml-docs-" + Guid.NewGuid().ToString("n")),
        SourcePaths = new[]{ Directory.GetCurrentDirectory() }
    };

    [Fact]
    public async Task Generate_Fails_When_Config_Null()
    {
        var svc = new SimpleDocumentationGeneratorService();
        var resp = await svc.GenerateDocumentationAsync(new GenerateDocumentationRequest
        {
            Configuration = null,
            SourcePaths = new[]{ Directory.GetCurrentDirectory() }
        });
        Assert.False(resp.Success);
        Assert.Contains("configuration is null", resp.Errors.First());
    }

    [Fact]
    public async Task Generate_ValidateOnly_Skips_File_Writes()
    {
        var cfg = ValidConfig();
        var svc = new SimpleDocumentationGeneratorService();
        var resp = await svc.GenerateDocumentationAsync(new GenerateDocumentationRequest
        {
            Configuration = cfg,
            SourcePaths = cfg.SourcePaths,
            ValidateOnly = true
        });
        Assert.True(resp.Success);
        Assert.Empty(resp.GeneratedFiles);
        Assert.NotNull(resp.ValidationResult);
    }

    [Fact]
    public async Task Generate_Full_Writes_Core_Files()
    {
        var cfg = ValidConfig();
        var svc = new SimpleDocumentationGeneratorService();
        var resp = await svc.GenerateDocumentationAsync(new GenerateDocumentationRequest
        {
            Configuration = cfg,
            SourcePaths = cfg.SourcePaths,
            OutputPath = cfg.OutputPath
        });
        Assert.True(resp.Success);
        Assert.Contains(resp.GeneratedFiles, f => f.EndsWith("index.html"));
        Assert.Contains(resp.GeneratedFiles, f => f.EndsWith("index.json"));
        Assert.NotNull(resp.ValidationResult);
    }

    [Fact]
    public async Task Generate_Incremental_Skips_Static_Files()
    {
        var cfg = ValidConfig();
        var svc = new SimpleDocumentationGeneratorService();
        var resp1 = await svc.GenerateDocumentationAsync(new GenerateDocumentationRequest
        {
            Configuration = cfg,
            SourcePaths = cfg.SourcePaths,
            OutputPath = cfg.OutputPath
        });
        Assert.True(resp1.Success);

        var resp2 = await svc.GenerateDocumentationAsync(new GenerateDocumentationRequest
        {
            Configuration = cfg,
            SourcePaths = cfg.SourcePaths,
            OutputPath = cfg.OutputPath,
            IncrementalBuild = true
        });
        Assert.True(resp2.Success);
        // In incremental mode we still expect search index but not necessarily a second index.html entry
        Assert.DoesNotContain(resp2.GeneratedFiles, f => f.EndsWith("api/SampleClass.html"));
        Assert.Contains(resp2.GeneratedFiles, f => f.EndsWith("index.json"));
    }

    [Fact]
    public async Task Validate_Fails_When_No_Source_Paths()
    {
        var svc = new SimpleDocumentationGeneratorService();
        var resp = await svc.ValidateDocumentationAsync(new ValidateDocumentationRequest
        {
            SourcePaths = Array.Empty<string>()
        });
        Assert.False(resp.PassesRequirements);
        Assert.Equal(ValidationStatus.Error, resp.ValidationResult.Status);
        Assert.Contains(resp.ValidationResult.Issues, i => i.Message.Contains("Source paths"));
    }

    [Fact]
    public async Task Validate_Warns_When_Coverage_Below_Minimum()
    {
        var svc = new SimpleDocumentationGeneratorService();
        var resp = await svc.ValidateDocumentationAsync(new ValidateDocumentationRequest
        {
            SourcePaths = new[]{ Directory.GetCurrentDirectory() },
            MinimumCoverage = 95.0,
            CheckLinks = false,
            CheckExamples = false
        });
    Assert.Contains(resp.ValidationResult.Issues, i => i.IssueType == IssueType.MissingDocumentation);
    }
}
