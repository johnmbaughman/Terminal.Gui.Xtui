// <copyright file="DocumentationMetadataModelsTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
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
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = "docs/_site/",
            SourcePaths = new[] { "src/Terminal.Gui.Xaml/" },
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
                Properties = new Dictionary<string, string>
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
    Assert.Empty(validationResults);
    Assert.NotNull(config.Metadata);
    Assert.NotNull(config.Build);
    Assert.NotEmpty(config.Metadata.Src);
    Assert.False(string.IsNullOrWhiteSpace(config.Build.Dest));
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
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r =>
            (r.ErrorMessage ?? string.Empty).Contains("Src") &&
            ((r.ErrorMessage ?? string.Empty).Contains("required") || (r.ErrorMessage ?? string.Empty).Contains("least one")));
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
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r =>
            (r.ErrorMessage ?? string.Empty).Contains("Files") &&
            ((r.ErrorMessage ?? string.Empty).Contains("required") || (r.ErrorMessage ?? string.Empty).Contains("least one")));
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
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r =>
            (r.ErrorMessage ?? string.Empty).Contains("Dest") && (r.ErrorMessage ?? string.Empty).Contains("required"));
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
    Assert.Equal("modern", template.Name);
    Assert.Equal("templates/modern/", template.Path);
    Assert.Equal("custom.css", template.CustomCss);
    Assert.Equal("custom.js", template.CustomJs);
    Assert.True(template.Variables.ContainsKey("primaryColor"));
    Assert.Equal("#007acc", template.Variables["primaryColor"]);
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
        Assert.Equal(100, coverage.TotalMembers);
        Assert.Equal(85, coverage.DocumentedMembers);
        Assert.Equal(15, coverage.UndocumentedMembers);
        Assert.Equal(85.0, coverage.CoveragePercentage);
        Assert.Equal(coverage.TotalMembers, coverage.DocumentedMembers + coverage.UndocumentedMembers);
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
    Assert.Equal(IssueType.MissingDocumentation, issue.IssueType);
    Assert.Equal(IssueSeverity.Warning, issue.Severity);
    Assert.Contains("XamlView.cs", issue.FilePath);
    Assert.Equal(42, issue.LineNumber);
    Assert.Equal("LoadXaml", issue.MemberName);
    Assert.Contains("missing XML documentation", issue.Message);
    Assert.Contains("XML documentation", issue.Suggestion);
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
    Assert.Equal(ValidationStatus.Warning, result.Status);
    Assert.Equal(1, result.Issues.Count(i => i.Severity == IssueSeverity.Error));
    Assert.Equal(2, result.Issues.Count(i => i.Severity == IssueSeverity.Warning));
    Assert.Equal(1, result.Issues.Count(i => i.Severity == IssueSeverity.Information));
    Assert.Contains("4 issues", result.Summary);
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
    Assert.True(response.Success);
    Assert.Equal(2, response.GeneratedFiles.Count);
    Assert.Equal(TimeSpan.FromMinutes(2), response.Duration);
    Assert.Equal("docs/_site/", response.OutputPath);
    Assert.True(response.Metadata.ContainsKey("DocFxVersion"));
    Assert.Equal("2.75.3", response.Metadata["DocFxVersion"]);
    Assert.True(response.Metadata.ContainsKey("IncrementalBuild"));
    Assert.Equal(false, response.Metadata["IncrementalBuild"]);
    }

    [Theory]
    [InlineData("")]
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
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, r => (r.ErrorMessage ?? string.Empty).Contains("Src"));
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
        Assert.Equal(status, result.Status);
        Assert.Equal(issueCount, result.Issues.Length);
        if (status == ValidationStatus.Success)
        {
            Assert.Empty(result.Issues);
        }
        else
        {
            Assert.NotEmpty(result.Issues);
        }
    }

    private static IList<ValidationResult> ValidateModel<T>(T model) where T : notnull
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model!);
        Validator.TryValidateObject(model!, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}
 
