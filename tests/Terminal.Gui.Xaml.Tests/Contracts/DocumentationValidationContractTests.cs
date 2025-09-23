// <copyright file="DocumentationValidationContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
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
        response.Should().NotBeNull();
        response.ValidationResult.Should().NotBeNull();
        response.Coverage.Should().NotBeNull();
        response.Issues.Should().NotBeNull();
        response.PassesRequirements.Should().BeTrue();
        response.Coverage.CoveragePercentage.Should().BeGreaterOrEqualTo(80.0);
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
        response.Should().NotBeNull();
        response.PassesRequirements.Should().BeFalse();
        response.Issues.Should().Contain(issue =>
            issue.IssueType == IssueType.MissingDocumentation);
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
        response.Should().NotBeNull();
        response.Issues.Should().NotContain(issue =>
            issue.IssueType == IssueType.InvalidLink && issue.Severity == IssueSeverity.Error);
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
        response.Should().NotBeNull();
        response.Issues.Should().NotContain(issue =>
            issue.IssueType == IssueType.InvalidExample && issue.Severity == IssueSeverity.Error);
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
        duration.Should().BeLessThan(TimeSpan.FromMinutes(1));
        response.Should().NotBeNull();
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
        response.Should().NotBeNull();
        response.Issues.Where(issue => issue.Severity >= IssueSeverity.Warning)
            .Should().AllSatisfy(issue =>
            {
                issue.Message.Should().NotBeNullOrEmpty();
                issue.FilePath.Should().NotBeNullOrEmpty();
                issue.MemberName.Should().NotBeNullOrEmpty();
            });
    }

    private static IDocumentationGeneratorService CreateDocumentationGeneratorService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IDocumentationGeneratorService not implemented yet");
    }
}

// These types will fail to compile until implemented in Phase 3.3
public class ValidateDocumentationRequest
{
    public string[] SourcePaths { get; set; }
    public bool CheckLinks { get; set; } = true;
    public bool CheckExamples { get; set; } = true;
    public double MinimumCoverage { get; set; } = 80.0;
}

public class ValidateDocumentationResponse
{
    public DocumentationValidationResult ValidationResult { get; set; }
    public CoverageMetrics Coverage { get; set; }
    public ValidationIssue[] Issues { get; set; }
    public bool PassesRequirements { get; set; }
}

public enum IssueType
{
    MissingDocumentation,
    InvalidLink,
    InvalidExample,
    MalformedXml,
    MissingReturnDoc,
    MissingParameterDoc
}

public enum IssueSeverity
{
    Error,
    Warning,
    Information
}

public enum ValidationStatus
{
    Success,
    Warning,
    Error
}
