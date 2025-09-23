// <copyright file="DocumentationMetadataModelsTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
using Terminal.Gui.Xaml.Documentation.Models;
using System.ComponentModel.DataAnnotations;

namespace Terminal.Gui.Xaml.Tests.Models;

/// <summary>
/// Tests for DocumentationMetadata model classes.
/// These tests verify model validation and must FAIL until implementation is complete.
/// </summary>
public class DocumentationMetadataModelsTests
{
    [Fact]
    public void DocFxConfiguration_WithValidData_ShouldValidateSuccessfully()
    {
        // Arrange
        var config = new DocFxConfiguration
        {
            Metadata = new MetadataConfiguration
            {
                Src = new[]
                {
                    new SourceConfiguration
                    {
                        Files = new[] { "**/*.cs" },
                        Src = "src/"
                    }
                },
                Dest = "api/",
                Properties = new Dictionary<string, object>
                {
                    ["TargetFramework"] = "net8.0"
                }
            },
            Build = new BuildConfiguration
            {
                Content = new[]
                {
                    new ContentConfiguration
                    {
                        Files = new[] { "**/*.yml", "**/*.md" },
                        Src = ".",
                        Dest = "."
                    }
                },
                Resource = new[]
                {
                    new ResourceConfiguration
                    {
                        Files = new[] { "images/**" }
                    }
                },
                Dest = "_site/",
                Template = new[] { "default" },
                GlobalMetadata = new Dictionary<string, object>
                {
                    ["_appTitle"] = "Terminal.Gui.Xaml Documentation",
                    ["_enableSearch"] = true
                }
            }
        };

        // Act
        var validationResults = ValidateModel(config);

        // Assert
        validationResults.Should().BeEmpty();
        config.Metadata.Should().NotBeNull();
        config.Build.Should().NotBeNull();
        config.Metadata.Src.Should().NotBeEmpty();
        config.Build.Dest.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MetadataConfiguration_WithEmptySourcePaths_ShouldFailValidation()
    {
        // Arrange
        var metadata = new MetadataConfiguration
        {
            Src = Array.Empty<SourceConfiguration>(),
            Dest = "api/"
        };

        // Act
        var validationResults = ValidateModel(metadata);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(result =>
            result.ErrorMessage.Contains("Src") && result.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void SourceConfiguration_WithInvalidFiles_ShouldFailValidation()
    {
        // Arrange
        var source = new SourceConfiguration
        {
            Files = Array.Empty<string>(),
            Src = "src/"
        };

        // Act
        var validationResults = ValidateModel(source);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(result =>
            result.ErrorMessage.Contains("Files") && result.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void BuildConfiguration_WithEmptyDestination_ShouldFailValidation()
    {
        // Arrange
        var build = new BuildConfiguration
        {
            Content = new[]
            {
                new ContentConfiguration
                {
                    Files = new[] { "**/*.md" },
                    Src = "."
                }
            },
            Dest = string.Empty
        };

        // Act
        var validationResults = ValidateModel(build);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(result =>
            result.ErrorMessage.Contains("Dest") && result.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void TemplateConfiguration_WithValidSettings_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var template = new TemplateConfiguration
        {
            Name = "modern",
            Path = "templates/modern/",
            CustomCss = "custom.css",
            CustomJs = "custom.js",
            Variables = new Dictionary<string, object>
            {
                ["primaryColor"] = "#007acc",
                ["fontFamily"] = "Segoe UI"
            }
        };

        // Assert
        template.Name.Should().Be("modern");
        template.Path.Should().Be("templates/modern/");
        template.CustomCss.Should().Be("custom.css");
        template.CustomJs.Should().Be("custom.js");
        template.Variables.Should().ContainKey("primaryColor");
        template.Variables["primaryColor"].Should().Be("#007acc");
    }

    [Fact]
    public void CoverageMetrics_WithValidCounts_ShouldCalculatePercentageCorrectly()
    {
        // Arrange & Act
        var coverage = new CoverageMetrics
        {
            TotalMembers = 100,
            DocumentedMembers = 85,
            UndocumentedMembers = 15
        };

        // Assert
        coverage.TotalMembers.Should().Be(100);
        coverage.DocumentedMembers.Should().Be(85);
        coverage.UndocumentedMembers.Should().Be(15);
        coverage.CoveragePercentage.Should().Be(85.0);
        coverage.DocumentedMembers + coverage.UndocumentedMembers.Should().Be(coverage.TotalMembers);
    }

    [Fact]
    public void ValidationIssue_WithCompleteInformation_ShouldFormatCorrectly()
    {
        // Arrange & Act
        var issue = new ValidationIssue
        {
            IssueType = IssueType.MissingDocumentation,
            Severity = IssueSeverity.Warning,
            FilePath = "src/Terminal.Gui.Xaml/Controls/XamlView.cs",
            LineNumber = 42,
            MemberName = "LoadXaml",
            Message = "Method LoadXaml is missing XML documentation",
            Suggestion = "Add XML documentation with <summary>, <param>, and <returns> tags"
        };

        // Assert
        issue.IssueType.Should().Be(IssueType.MissingDocumentation);
        issue.Severity.Should().Be(IssueSeverity.Warning);
        issue.FilePath.Should().Contain("XamlView.cs");
        issue.LineNumber.Should().Be(42);
        issue.MemberName.Should().Be("LoadXaml");
        issue.Message.Should().Contain("missing XML documentation");
        issue.Suggestion.Should().Contain("XML documentation");
    }

    [Fact]
    public void DocumentationValidationResult_WithIssues_ShouldCategorizeCorrectly()
    {
        // Arrange
        var issues = new[]
        {
            new ValidationIssue { Severity = IssueSeverity.Error },
            new ValidationIssue { Severity = IssueSeverity.Warning },
            new ValidationIssue { Severity = IssueSeverity.Warning },
            new ValidationIssue { Severity = IssueSeverity.Information }
        };

        // Act
        var result = new DocumentationValidationResult
        {
            Status = ValidationStatus.Warning,
            Issues = issues,
            Summary = "4 issues found: 1 error, 2 warnings, 1 informational"
        };

        // Assert
        result.Status.Should().Be(ValidationStatus.Warning);
        result.Issues.Count(i => i.Severity == IssueSeverity.Error).Should().Be(1);
        result.Issues.Count(i => i.Severity == IssueSeverity.Warning).Should().Be(2);
        result.Issues.Count(i => i.Severity == IssueSeverity.Information).Should().Be(1);
        result.Summary.Should().Contain("4 issues");
    }

    [Fact]
    public void GenerateDocumentationResponse_WithMetadata_ShouldTrackBuildInformation()
    {
        // Arrange & Act
        var response = new GenerateDocumentationResponse
        {
            Success = true,
            GeneratedFiles = new[] { "index.html", "api/Terminal.Gui.Xaml.html" },
            Duration = TimeSpan.FromMinutes(2),
            OutputPath = "docs/_site/",
            Metadata = new Dictionary<string, object>
            {
                ["DocFxVersion"] = "2.75.3",
                ["GeneratedAt"] = DateTime.UtcNow,
                ["IncrementalBuild"] = false,
                ["TemplatesUsed"] = new[] { "default", "modern" }
            }
        };

        // Assert
        response.Success.Should().BeTrue();
        response.GeneratedFiles.Should().HaveCount(2);
        response.Duration.Should().Be(TimeSpan.FromMinutes(2));
        response.OutputPath.Should().Be("docs/_site/");
        response.Metadata.Should().ContainKey("DocFxVersion");
        response.Metadata["DocFxVersion"].Should().Be("2.75.3");
        response.Metadata.Should().ContainKey("IncrementalBuild");
        response.Metadata["IncrementalBuild"].Should().Be(false);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void SourceConfiguration_WithInvalidSrc_ShouldFailValidation(string invalidSrc)
    {
        // Arrange
        var source = new SourceConfiguration
        {
            Files = new[] { "**/*.cs" },
            Src = invalidSrc
        };

        // Act
        var validationResults = ValidateModel(source);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(result =>
            result.ErrorMessage.Contains("Src"));
    }

    [Theory]
    [InlineData(ValidationStatus.Success, 0)]
    [InlineData(ValidationStatus.Warning, 2)]
    [InlineData(ValidationStatus.Error, 5)]
    public void DocumentationValidationResult_WithDifferentStatuses_ShouldReflectCorrectState(
        ValidationStatus status, int issueCount)
    {
        // Arrange
        var issues = Enumerable.Range(0, issueCount)
            .Select(i => new ValidationIssue
            {
                Message = $"Issue {i}",
                Severity = status == ValidationStatus.Error ? IssueSeverity.Error : IssueSeverity.Warning
            })
            .ToArray();

        // Act
        var result = new DocumentationValidationResult
        {
            Status = status,
            Issues = issues
        };

        // Assert
        result.Status.Should().Be(status);
        result.Issues.Should().HaveCount(issueCount);

        if (status == ValidationStatus.Success)
        {
            result.Issues.Should().BeEmpty();
        }
        else
        {
            result.Issues.Should().NotBeEmpty();
        }
    }

    private static IList<ValidationResult> ValidateModel<T>(T model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}

// These types will fail to compile until implemented in Phase 3.3
public class ValidationIssue
{
    public IssueType IssueType { get; set; }
    public IssueSeverity Severity { get; set; }
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string MemberName { get; set; }
    public string Message { get; set; }
    public string Suggestion { get; set; }
}

public class DocumentationValidationResult
{
    public ValidationStatus Status { get; set; }
    public ValidationIssue[] Issues { get; set; } = Array.Empty<ValidationIssue>();
    public string Summary { get; set; }
}
