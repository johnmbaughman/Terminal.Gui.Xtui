// <copyright file="DocumentationGeneratorServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for IDocumentationGeneratorService.GenerateDocumentationAsync.
/// These tests define the expected behavior and must FAIL until implementation is complete.
/// </summary>
public class DocumentationGeneratorServiceContractTests
{
    [Fact]
    public async Task GenerateDocumentationAsync_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Incremental,
            ValidateOnly = false
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.False(string.IsNullOrEmpty(response.OutputPath));
    Assert.NotNull(response.GeneratedDocuments);
    Assert.NotNull(response.ValidationResult);
    Assert.True(response.GenerationTime < TimeSpan.FromMinutes(5));
    Assert.Empty(response.Errors);
    }

    [Fact]
    public async Task GenerateDocumentationAsync_WithInvalidConfiguration_ShouldFail()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = null, // Invalid configuration
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Full
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.False(response.Success);
    Assert.NotEmpty(response.Errors);
    Assert.Contains(response.Errors, e => e.Contains("configuration", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GenerateDocumentationAsync_WithNonExistentSourcePaths_ShouldFail()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "non/existent/path/" },
            BuildMode = DocumentationBuildMode.Full
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.False(response.Success);
    Assert.NotEmpty(response.Errors);
    Assert.Contains(response.Errors, e => e.Contains("source path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GenerateDocumentationAsync_WithValidateOnly_ShouldNotGenerateFiles()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.MetadataOnly,
            ValidateOnly = true
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.True(string.IsNullOrEmpty(response.OutputPath)); // No files generated in validate-only mode
        Assert.NotNull(response.ValidationResult);
        Assert.NotEqual(ValidationStatus.Error, response.ValidationResult.Status);
    }

    [Fact]
    public async Task GenerateDocumentationAsync_WithIncrementalBuild_ShouldBeFaster()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var fullBuildRequest = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Full
        };
        var incrementalBuildRequest = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Incremental
        };

        // Act
        var fullBuildResponse = await service.GenerateDocumentationAsync(fullBuildRequest);
        var incrementalBuildResponse = await service.GenerateDocumentationAsync(incrementalBuildRequest);

        // Assert
        Assert.True(fullBuildResponse.Success);
        Assert.True(incrementalBuildResponse.Success);
        Assert.True(incrementalBuildResponse.GenerationTime <= fullBuildResponse.GenerationTime);
    }

    [Fact]
    public async Task GenerateDocumentationAsync_ShouldValidateXmlDocumentationCoverage()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Full
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.ValidationResult);
        Assert.NotNull(response.ValidationResult.CoverageMetrics);
        Assert.True(response.ValidationResult.CoverageMetrics.CoveragePercentage >= 80.0);
    }

    [Fact]
    public async Task GenerateDocumentationAsync_ShouldGenerateSearchableOutput()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new GenerateDocumentationRequest
        {
            Configuration = CreateValidDocFxConfiguration(),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            BuildMode = DocumentationBuildMode.Full
        };

        // Act
        var response = await service.GenerateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Contains(response.GeneratedDocuments, d => d.GeneratedPath.Contains("search", StringComparison.OrdinalIgnoreCase) || d.GeneratedPath.Contains("index", StringComparison.OrdinalIgnoreCase));
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleDocumentationGeneratorService();
    }

    private static DocFxConfiguration CreateValidDocFxConfiguration()
    {
        return new DocFxConfiguration
        {
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = Path.Combine("temp", "docs", "_site"),
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            Build = new BuildConfiguration
            {
                Dest = "_site/",
                Template = new[] { "default" },
                GlobalMetadata = new Dictionary<string, object>
                {
                    ["_appTitle"] = "Terminal.Gui.Xaml"
                }
            },
            Metadata = new MetadataConfiguration
            {
                Src = new[]
                {
                    new SourceConfiguration
                    {
                        Files = new [] { "**/*.cs" },
                        Src = "src/Terminal.Gui.Xaml/"
                    }
                },
                Dest = "api/",
                Properties = new Dictionary<string, string> { ["TargetFramework"] = "net8.0" }
            }
        };
    }
}

// End of tests
