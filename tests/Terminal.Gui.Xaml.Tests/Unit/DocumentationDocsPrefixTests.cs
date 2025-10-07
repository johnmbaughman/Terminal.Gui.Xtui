using System;
using System.Threading.Tasks;
using System.IO;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.Xaml.Documentation.Utilities;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class DocumentationDocsPrefixTests
{
    [Fact]
    public async Task SimpleBuildIntegrationService_UnknownTarget_ReturnsDocsPrefixInErrors()
    {
        var svc = new SimpleBuildIntegrationService();

        var req = new Terminal.Gui.Xaml.Documentation.Services.BuildTargetRequest
        {
            Target = "NonExistent",
            ProjectFile = "does-not-exist.csproj",
            Properties = new System.Collections.Generic.Dictionary<string, string>()
        };

        var resp = await svc.ExecuteBuildTargetAsync(req);

        Assert.False(resp.Success);
        Assert.NotNull(resp.Errors);
        Assert.Contains(DocumentationUtilities.DocsPrefix, resp.Errors);
    }

    [Fact]
    public async Task SimpleBuildIntegrationService_MissingProject_ThrowsFileNotFound_WithDocsPrefix()
    {
        var svc = new SimpleBuildIntegrationService();

        var req = new Terminal.Gui.Xaml.Documentation.Services.BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = "this-project-file-does-not-exist.csproj",
        };

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () => await svc.ExecuteBuildTargetAsync(req));

        Assert.Contains(DocumentationUtilities.DocsPrefix, ex.Message);
    }

    [Fact]
    public async Task SimpleDocumentationConfigurationService_MissingConfig_Throws_FileNotFound_WithDocsPrefix()
    {
        var svc = new SimpleDocumentationConfigurationService();
        var missing = Path.Combine("temp", "docs", "does-not-exist-docfx.json");

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(async () => await svc.LoadConfigurationAsync(missing));

        Assert.Contains(DocumentationUtilities.DocsPrefix, ex.Message);
    }
}
