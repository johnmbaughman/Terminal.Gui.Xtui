// <copyright file="DocumentationValidationContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for IDocumentationGeneratorService.ValidateDocumentationAsync.
/// These tests define the expected behavior and must FAIL until implementation is complete.
/// </summary>
public class DocumentationValidationContractTests
{
    [Fact]
    public async Task ValidateDocumentationAsync_WithValidSourcePaths_ShouldSucceed()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            CheckLinks = true,
            CheckExamples = true,
            MinimumCoverage = 80.0
        };

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.NotNull(response.ValidationResult);
    Assert.NotNull(response.Coverage);
    Assert.NotNull(response.Issues);
    Assert.True(response.PassesRequirements);
    Assert.True(response.Coverage.CoveragePercentage >= 80.0);
    }

    [Fact]
    public async Task ValidateDocumentationAsync_WithLowCoverage_ShouldFailRequirements()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            CheckLinks = false,
            CheckExamples = false,
            MinimumCoverage = 95.0 // Set very high threshold
        };

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.PassesRequirements);
        Assert.Contains(response.Issues, i => i.IssueType == IssueType.MissingDocumentation);
    }

    [Fact]
    public async Task ValidateDocumentationAsync_WithInvalidSourcePaths_ShouldFail()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "non/existent/path/" },
            MinimumCoverage = 80.0
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ValidateDocumentationAsync(request));
    }

    [Fact]
    public async Task ValidateDocumentationAsync_WithLinkValidation_ShouldCheckAllLinks()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            CheckLinks = true,
            CheckExamples = false,
            MinimumCoverage = 0.0 // Don't care about coverage for this test
        };

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.DoesNotContain(response.Issues, issue => issue.IssueType == IssueType.InvalidLink && issue.Severity == IssueSeverity.Error);
    }

    [Fact]
    public async Task ValidateDocumentationAsync_WithExampleValidation_ShouldCompileExamples()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            CheckLinks = false,
            CheckExamples = true,
            MinimumCoverage = 0.0
        };

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.DoesNotContain(response.Issues, issue => issue.IssueType == IssueType.InvalidExample && issue.Severity == IssueSeverity.Error);
    }

    [Fact]
    public async Task ValidateDocumentationAsync_ShouldCompleteWithin1Minute()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            MinimumCoverage = 80.0
        };
        var startTime = DateTime.UtcNow;

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
        var duration = DateTime.UtcNow - startTime;
    Assert.True(duration < TimeSpan.FromMinutes(1));
    Assert.NotNull(response);
    }

    [Fact]
    public async Task ValidateDocumentationAsync_ShouldProvideActionableErrorMessages()
    {
        // Arrange
        var service = CreateDocumentationGeneratorService();
        var request = new ValidateDocumentationRequest
        {
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
            MinimumCoverage = 100.0 // Force some missing documentation
        };

        // Act
        var response = await service.ValidateDocumentationAsync(request);

        // Assert
        Assert.NotNull(response);
        foreach (var issue in response.Issues.Where(i => i.Severity >= IssueSeverity.Warning))
        {
            Assert.False(string.IsNullOrEmpty(issue.Message));
            Assert.False(string.IsNullOrEmpty(issue.FilePath));
            Assert.False(string.IsNullOrEmpty(issue.MemberName));
        }
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleDocumentationGeneratorService();
    }
}

// End of tests
