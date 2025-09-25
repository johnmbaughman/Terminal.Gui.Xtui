using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.Xaml.Build;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Smoke;

public class QuickstartSmokeTests
{
    [Fact]
    public async Task Quickstart_EndToEnd_Smoke()
    {
        // Generate
        var gen = new SimpleDocumentationGeneratorService();
        var output = Path.Combine(Path.GetTempPath(), "tgxaml-smoke-" + Guid.NewGuid().ToString("n"));
        var req = new GenerateDocumentationRequest
        {
            Configuration = new DocFxConfiguration { ProjectName = "Terminal.Gui.Xaml", OutputPath = output, Build = new BuildConfiguration { Template = new[]{"modern"} } },
            SourcePaths = new[]{"docs"},
            OutputPath = output,
            BuildMode = DocumentationBuildMode.Full
        };
        var genResp = await gen.GenerateDocumentationAsync(req);
        Assert.True(genResp.Success);
        Assert.True(File.Exists(Path.Combine(output, "index.html")));
        Assert.True(File.Exists(Path.Combine(output, "index.json")));
    Assert.True(File.Exists(Path.Combine(output, "toc.html")));
    Assert.True(File.Exists(Path.Combine(output, "manifest.json")));

        // Validate
        var val = await gen.ValidateDocumentationAsync(new ValidateDocumentationRequest
        {
            SourcePaths = new[]{"docs"},
            MinimumCoverage = 50.0,
            CheckExamples = true,
            CheckLinks = true
        });
        Assert.True(val.PassesRequirements);
        Assert.NotNull(val.ValidationResult);

        // Build integration (simulate GenerateDocumentation & Clean)
        var build = new BuildIntegrationService(new SimpleBuildIntegrationService());
        var progress = new List<BuildProgress>();
        var buildOutput = Path.Combine(Path.GetTempPath(), "tgxaml-smoke-build-"+Guid.NewGuid().ToString("n"));
        var buildResp = await build.ExecuteBuildTargetAsync(new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories).FirstOrDefault() ?? "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string,string>{{"OutputPath", buildOutput}}
        }, p => { progress.Add(p); return Task.CompletedTask; });
        Assert.True(buildResp.Success);
        Assert.Contains(progress, p => p.Phase == BuildPhase.Completed);
        Assert.True(File.Exists(Path.Combine(buildOutput, "toc.html")));
        Assert.True(File.Exists(Path.Combine(buildOutput, "manifest.json")));
    }
}
