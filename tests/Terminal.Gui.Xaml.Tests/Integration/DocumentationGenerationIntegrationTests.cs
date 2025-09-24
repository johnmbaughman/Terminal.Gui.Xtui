// <copyright file="DocumentationGenerationIntegrationTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Tests.Integration;

/// <summary>
/// Integration tests for end-to-end documentation generation.
/// These tests verify the complete workflow and must FAIL until implementation is complete.
/// </summary>
public class DocumentationGenerationIntegrationTests
{
    [Fact]
    public async Task EndToEnd_GenerateDocumentation_ShouldProduceCompleteDocumentationSite()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
        var request = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = false,
            OutputPath = "temp/docs/_site/"
        };

        // Act
        var response = await generatorService.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.True(response.GeneratedFiles.Any());
    Assert.True(response.Duration < TimeSpan.FromMinutes(5));

        // Verify key files were generated
        Assert.Contains(response.GeneratedFiles, f => f.EndsWith("index.html", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(response.GeneratedFiles, f => f.Contains("api/", StringComparison.OrdinalIgnoreCase) && f.EndsWith(".html", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(response.GeneratedFiles, f => f.EndsWith("toc.html", StringComparison.OrdinalIgnoreCase));

        // Verify structure
    Assert.True(Directory.Exists("temp/docs/_site/"));
    Assert.True(Directory.Exists("temp/docs/_site/api/"));
    Assert.True(File.Exists("temp/docs/_site/index.html"));
    }

    [Fact]
    public async Task EndToEnd_ValidateDocumentation_ShouldAnalyzeCodeCoverage()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            CheckLinks = true,
            CheckExamples = true,
            MinimumCoverage = 80.0
        };

        // Act
        var response = await generatorService.ValidateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.NotNull(response.ValidationResult);
    Assert.NotNull(response.Coverage);

        // Verify coverage metrics
    Assert.True(response.Coverage.CoveragePercentage >= 0);
    Assert.True(response.Coverage.TotalMembers > 0);
    Assert.True(response.Coverage.DocumentedMembers >= 0);
    Assert.True(response.Coverage.UndocumentedMembers >= 0);

        // Verify issue reporting
        Assert.NotNull(response.Issues);
        foreach (var issue in response.Issues.Where(i => i.Severity == IssueSeverity.Error))
        {
            Assert.False(string.IsNullOrEmpty(issue.FilePath));
            Assert.True(issue.LineNumber > 0);
            Assert.False(string.IsNullOrEmpty(issue.MemberName));
            Assert.False(string.IsNullOrEmpty(issue.Message));
        }
    }

    [Fact]
    public async Task EndToEnd_IncrementalBuild_ShouldOnlyRegenerateChangedFiles()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");

        // First build - full generation
        var fullBuildRequest = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = false,
            OutputPath = "temp/docs/_site/"
        };

        var fullBuildResponse = await generatorService.GenerateDocumentationAsync(fullBuildRequest);
        var fullBuildFileCount = fullBuildResponse.GeneratedFiles.Count;

        // Second build - incremental (no changes)
        var incrementalRequest = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = true,
            OutputPath = "temp/docs/_site/"
        };

        // Act
        var incrementalResponse = await generatorService.GenerateDocumentationAsync(incrementalRequest);

        // Assert
    Assert.NotNull(incrementalResponse);
    Assert.True(incrementalResponse.Success);
    Assert.True(incrementalResponse.GeneratedFiles.Count < fullBuildFileCount);
    Assert.True(incrementalResponse.Duration < fullBuildResponse.Duration);
    Assert.True(incrementalResponse.Metadata.ContainsKey("IncrementalBuild"));
    Assert.Equal("true", incrementalResponse.Metadata["IncrementalBuild"].ToString());
    }

    [Fact]
    public async Task EndToEnd_MSBuildIntegration_ShouldExecuteDocumentationTargets()
    {
        // Arrange
        var buildService = CreateBuildIntegrationService();
        var generateRequest = new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string, string>
            {
                ["DocFxConfigPath"] = "docs/docfx.json",
                ["OutputPath"] = "temp/docs/_site/",
                ["LogLevel"] = "Info"
            }
        };

        // Act
        var generateResponse = await buildService.ExecuteBuildTargetAsync(generateRequest);

        // Assert
    Assert.NotNull(generateResponse);
    Assert.True(generateResponse.Success);
    Assert.Equal(0, generateResponse.ExitCode);
    Assert.False(string.IsNullOrEmpty(generateResponse.Output));
    Assert.True(generateResponse.Duration < TimeSpan.FromMinutes(5));

        // Verify documentation was generated
    Assert.True(Directory.Exists("temp/docs/_site/"));
    Assert.True(File.Exists("temp/docs/_site/index.html"));

        // Test validation target
        var validateRequest = new BuildTargetRequest
        {
            Target = "ValidateDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string, string>
            {
                ["MinimumCoverage"] = "80.0",
                ["CheckLinks"] = "true"
            }
        };

        var validateResponse = await buildService.ExecuteBuildTargetAsync(validateRequest);
    Assert.NotNull(validateResponse);
    Assert.True(validateResponse.Success);
    Assert.NotNull(validateResponse.ValidationResults);
    }

    [Fact]
    public async Task EndToEnd_SearchFunctionality_ShouldGenerateSearchableContent()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
    config.Build ??= new BuildConfiguration();
    config.Build.GlobalMetadata["_enableSearch"] = true;

        var request = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = false,
            OutputPath = "temp/docs/_site/"
        };

        // Act
        var response = await generatorService.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);

        // Verify search functionality was generated
        Assert.Contains(response.GeneratedFiles, f => f.Contains("search", StringComparison.OrdinalIgnoreCase) && f.EndsWith(".js", StringComparison.OrdinalIgnoreCase));
        Assert.True(File.Exists("temp/docs/_site/search-worker.js"));

        // Verify search index was created
    Assert.True(response.Metadata.ContainsKey("SearchEnabled"));
    Assert.Equal("true", response.Metadata["SearchEnabled"].ToString());
    }

    [Fact]
    public async Task EndToEnd_MultipleTemplates_ShouldGenerateWithCustomStyling()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
    config.Build ??= new BuildConfiguration();
    config.Build.Template = new[] { "default", "modern" };

        var request = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = false,
            OutputPath = "temp/docs/_site/"
        };

        // Act
        var response = await generatorService.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);

        // Verify template assets were generated
        Assert.Contains(response.GeneratedFiles, f => f.Contains("styles/", StringComparison.OrdinalIgnoreCase) && f.EndsWith(".css", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(response.GeneratedFiles, f => f.Contains("scripts/", StringComparison.OrdinalIgnoreCase) && f.EndsWith(".js", StringComparison.OrdinalIgnoreCase));

        // Verify custom styling was applied (retry in case file handle not yet released)
        string indexContent = string.Empty;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                indexContent = await File.ReadAllTextAsync("temp/docs/_site/index.html");
                break;
            }
            catch (IOException) when (attempt < 2)
            {
                await Task.Delay(50);
            }
        }
    Assert.Contains("modern", indexContent, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("Terminal.Gui.Xaml", indexContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EndToEnd_PerformanceRequirements_ShouldMeetTimingConstraints()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
        var request = new GenerateDocumentationRequest
        {
            Configuration = config,
            ValidateOnly = false,
            IncrementalBuild = false,
            OutputPath = "temp/docs/_site/"
        };

        var startTime = DateTime.UtcNow;

        // Act
        var response = await generatorService.GenerateDocumentationAsync(request);

        // Assert
        var totalDuration = DateTime.UtcNow - startTime;

        // Constitutional requirement: Must complete within 5 minutes
    Assert.True(totalDuration < TimeSpan.FromMinutes(5));
    Assert.True(response.Duration < TimeSpan.FromMinutes(5));

        // Performance should be reasonable for typical project size
        if (response.GeneratedFiles.Count > 0)
        {
            var avgTimePerFile = response.Duration.TotalMilliseconds / response.GeneratedFiles.Count;
            Assert.True(avgTimePerFile < 1000, $"Expected avgTimePerFile < 1000ms, actual {avgTimePerFile}"); // Less than 1 second per file on average
        }
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleDocumentationGeneratorService();
    }

    private static IDocumentationConfigurationService CreateDocumentationConfigurationService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleDocumentationConfigurationService();
    }

    private static IBuildIntegrationService CreateBuildIntegrationService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleBuildIntegrationService();
    }
}
