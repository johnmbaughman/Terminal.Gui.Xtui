using System;
using System.IO;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class DocumentationGeneratorServiceTests
{
    [Fact]
    public async Task GenerateDocumentationAsync_DelegatesToSimpleGenerator_ReturnsSuccess()
    {
        var temp = Path.Combine(Path.GetTempPath(), "tgx-docgen-test", Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(temp);

        var config = new DocFxConfiguration
        {
            ProjectName = "TestProject",
            OutputPath = temp
        };

        var request = new GenerateDocumentationRequest
        {
            Configuration = config,
            SourcePaths = new[] { "." },
            BuildMode = DocumentationBuildMode.Full
        };

    var service = new SimpleDocumentationGeneratorService();
    var resp = await service.GenerateDocumentationAsync(request);

        Assert.NotNull(resp);
        Assert.True(resp.Success);
        Assert.False(string.IsNullOrWhiteSpace(resp.OutputPath));
        Assert.NotEmpty(resp.GeneratedDocuments);

        // cleanup
        try { Directory.Delete(temp, true); } catch { }
    }

    [Fact]
    public async Task ValidateDocumentationAsync_ReturnsCoverageAndIssues()
    {
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "." },
            CheckLinks = true,
            CheckExamples = true,
            MinimumCoverage = 50.0
        };

    var service = new SimpleDocumentationGeneratorService();
    var resp = await service.ValidateDocumentationAsync(request);

        Assert.NotNull(resp);
        Assert.NotNull(resp.ValidationResult);
        Assert.NotNull(resp.Coverage);
        Assert.NotNull(resp.Issues);
        Assert.IsType<double>(resp.Coverage.CoveragePercentage);
    }
}
