// <copyright file="DocumentationGenerationIntegrationTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.GeneratedFiles.Should().NotBeEmpty();
        response.Duration.Should().BeLessThan(TimeSpan.FromMinutes(5));

        // Verify key files were generated
        response.GeneratedFiles.Should().Contain(file =>
            file.EndsWith("index.html"));
        response.GeneratedFiles.Should().Contain(file =>
            file.Contains("api/") && file.EndsWith(".html"));
        response.GeneratedFiles.Should().Contain(file =>
            file.EndsWith("toc.html"));

        // Verify structure
        Directory.Exists("temp/docs/_site/").Should().BeTrue();
        Directory.Exists("temp/docs/_site/api/").Should().BeTrue();
        File.Exists("temp/docs/_site/index.html").Should().BeTrue();
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
        response.Should().NotBeNull();
        response.ValidationResult.Should().NotBeNull();
        response.Coverage.Should().NotBeNull();

        // Verify coverage metrics
        response.Coverage.CoveragePercentage.Should().BeGreaterOrEqualTo(0);
        response.Coverage.TotalMembers.Should().BeGreaterThan(0);
        response.Coverage.DocumentedMembers.Should().BeGreaterOrEqualTo(0);
        response.Coverage.UndocumentedMembers.Should().BeGreaterOrEqualTo(0);

        // Verify issue reporting
        response.Issues.Should().NotBeNull();
        response.Issues.Where(i => i.Severity == IssueSeverity.Error)
            .Should().AllSatisfy(issue =>
            {
                issue.FilePath.Should().NotBeNullOrEmpty();
                issue.LineNumber.Should().BeGreaterThan(0);
                issue.MemberName.Should().NotBeNullOrEmpty();
                issue.Message.Should().NotBeNullOrEmpty();
            });
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
        incrementalResponse.Should().NotBeNull();
        incrementalResponse.Success.Should().BeTrue();
        incrementalResponse.GeneratedFiles.Count.Should().BeLessThan(fullBuildFileCount);
        incrementalResponse.Duration.Should().BeLessThan(fullBuildResponse.Duration);
        incrementalResponse.Metadata.Should().ContainKey("IncrementalBuild");
        incrementalResponse.Metadata["IncrementalBuild"].Should().Be("true");
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
        generateResponse.Should().NotBeNull();
        generateResponse.Success.Should().BeTrue();
        generateResponse.ExitCode.Should().Be(0);
        generateResponse.Output.Should().NotBeNullOrEmpty();
        generateResponse.Duration.Should().BeLessThan(TimeSpan.FromMinutes(5));

        // Verify documentation was generated
        Directory.Exists("temp/docs/_site/").Should().BeTrue();
        File.Exists("temp/docs/_site/index.html").Should().BeTrue();

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
        validateResponse.Should().NotBeNull();
        validateResponse.Success.Should().BeTrue();
        validateResponse.ValidationResults.Should().NotBeNull();
    }

    [Fact]
    public async Task EndToEnd_SearchFunctionality_ShouldGenerateSearchableContent()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();

        // Verify search functionality was generated
        response.GeneratedFiles.Should().Contain(file =>
            file.Contains("search") && file.EndsWith(".js"));
        File.Exists("temp/docs/_site/search-worker.js").Should().BeTrue();

        // Verify search index was created
        response.Metadata.Should().ContainKey("SearchEnabled");
        response.Metadata["SearchEnabled"].Should().Be("true");
    }

    [Fact]
    public async Task EndToEnd_MultipleTemplates_ShouldGenerateWithCustomStyling()
    {
        // Arrange
        var generatorService = CreateDocumentationGeneratorService();
        var configService = CreateDocumentationConfigurationService();

        var config = await configService.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();

        // Verify template assets were generated
        response.GeneratedFiles.Should().Contain(file =>
            file.Contains("styles/") && file.EndsWith(".css"));
        response.GeneratedFiles.Should().Contain(file =>
            file.Contains("scripts/") && file.EndsWith(".js"));

        // Verify custom styling was applied
        var indexContent = await File.ReadAllTextAsync("temp/docs/_site/index.html");
        indexContent.Should().Contain("modern");
        indexContent.Should().Contain("Terminal.Gui.Xaml");
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
        totalDuration.Should().BeLessThan(TimeSpan.FromMinutes(5));
        response.Duration.Should().BeLessThan(TimeSpan.FromMinutes(5));

        // Performance should be reasonable for typical project size
        if (response.GeneratedFiles.Count > 0)
        {
            var avgTimePerFile = response.Duration.TotalMilliseconds / response.GeneratedFiles.Count;
            avgTimePerFile.Should().BeLessThan(1000); // Less than 1 second per file on average
        }
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IDocumentationGeneratorService not implemented yet");
    }

    private static IDocumentationConfigurationService CreateDocumentationConfigurationService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IDocumentationConfigurationService not implemented yet");
    }

    private static IBuildIntegrationService CreateBuildIntegrationService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IBuildIntegrationService not implemented yet");
    }
}
