// <copyright file="DocumentationGeneratorServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.OutputPath.Should().NotBeNullOrEmpty();
        response.GeneratedDocuments.Should().NotBeNull();
        response.ValidationResult.Should().NotBeNull();
        response.GenerationTime.Should().BeLessThan(TimeSpan.FromMinutes(5));
        response.Errors.Should().BeEmpty();
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
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Should().Contain(error => error.Contains("configuration"));
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
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Should().Contain(error => error.Contains("source path"));
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.OutputPath.Should().BeNullOrEmpty(); // No files generated in validate-only mode
        response.ValidationResult.Should().NotBeNull();
        response.ValidationResult.ValidationStatus.Should().NotBe(ValidationStatus.Error);
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
        fullBuildResponse.Success.Should().BeTrue();
        incrementalBuildResponse.Success.Should().BeTrue();
        incrementalBuildResponse.GenerationTime.Should().BeLessOrEqualTo(fullBuildResponse.GenerationTime);
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
        response.Should().NotBeNull();
        response.ValidationResult.Should().NotBeNull();
        response.ValidationResult.CoverageMetrics.Should().NotBeNull();
        response.ValidationResult.CoverageMetrics.CoveragePercentage.Should().BeGreaterOrEqualTo(80.0);
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.GeneratedDocuments.Should().Contain(doc =>
            doc.GeneratedPath.Contains("search") || doc.GeneratedPath.Contains("index"));
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IDocumentationGeneratorService not implemented yet");
    }

    private static DocFxConfiguration CreateValidDocFxConfiguration()
    {
        // This will fail until DocFxConfiguration is implemented
        throw new NotImplementedException("DocFxConfiguration not implemented yet");
    }
}

// These types will fail to compile until implemented in Phase 3.3
public class GenerateDocumentationRequest
{
    public DocFxConfiguration Configuration { get; set; }
    public string[] SourcePaths { get; set; }
    public DocumentationBuildMode BuildMode { get; set; }
    public bool ValidateOnly { get; set; } = false;
}

public class GenerateDocumentationResponse
{
    public bool Success { get; set; }
    public string OutputPath { get; set; }
    public ApiDocumentation[] GeneratedDocuments { get; set; }
    public DocumentationValidationResult ValidationResult { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public string[] Errors { get; set; }
}

public enum DocumentationBuildMode
{
    Full,
    Incremental,
    MetadataOnly
}
