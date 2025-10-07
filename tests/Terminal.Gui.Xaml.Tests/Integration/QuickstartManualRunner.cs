using System.IO;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.Xaml.Documentation.Models;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Integration;

// This test simulates steps from quickstart.md to exercise the simple generator.
public class QuickstartManualRunner
{
    [Fact]
    public async Task Quickstart_GenerateDocumentation_ProducesFiles ()
    {
        // Arrange
        var temp = Path.Combine (Path.GetTempPath (), "tgx_quickstart_" + System.Guid.NewGuid ().ToString ("n"));
        if (Directory.Exists (temp))
        {
            Directory.Delete (temp, true);
        }
        Directory.CreateDirectory (temp);

        var config = new DocFxConfiguration
        {
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = temp,
            SourcePaths = new [] { "src/Terminal.Gui.Xaml" },
            Build = new BuildConfiguration
            {
                Dest = "_site/",
                Template = new [] { "default", "modern" }
            }
        };

        var svc = new SimpleDocumentationGeneratorService ();
        var req = new GenerateDocumentationRequest
        {
            Configuration = config,
            OutputPath = temp,
            SourcePaths = config.SourcePaths,
            ValidateOnly = false
        };

        // Act
        var resp = await svc.GenerateDocumentationAsync (req);

        // Assert
        Assert.True (resp.Success, "Generation should succeed");
        Assert.False (string.IsNullOrWhiteSpace (resp.OutputPath));
        Assert.True (File.Exists (Path.Combine (temp, "index.html")), "index.html should be present");
        Assert.True (File.Exists (Path.Combine (temp, "index.json")), "search index should be present");
        Assert.Contains ("modern", System.IO.File.ReadAllText (Path.Combine (temp, "index.html")), System.StringComparison.OrdinalIgnoreCase);

        // Cleanup
        Directory.Delete (temp, true);
    }
}
