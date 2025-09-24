// <copyright file="BuildIntegrationServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for IBuildIntegrationService.
/// These tests define the expected behavior and must FAIL until implementation is complete.
/// </summary>
public class BuildIntegrationServiceContractTests
{
    [Fact]
    public async Task ExecuteBuildTargetAsync_WithGenerateDocumentationTarget_ShouldSucceed()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var request = new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string, string>
            {
                ["DocFxConfigPath"] = "docs/docfx.json",
                ["OutputPath"] = "docs/_site/"
            }
        };

        // Act
        var response = await service.ExecuteBuildTargetAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.Equal(0, response.ExitCode);
    Assert.False(string.IsNullOrEmpty(response.Output));
    Assert.True(response.Errors == null || response.Errors.Length == 0);
    Assert.True(response.Duration < TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task ExecuteBuildTargetAsync_WithValidateDocumentationTarget_ShouldValidate()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var request = new BuildTargetRequest
        {
            Target = "ValidateDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string, string>
            {
                ["MinimumCoverage"] = "80.0",
                ["CheckLinks"] = "true",
                ["CheckExamples"] = "true"
            }
        };

        // Act
        var response = await service.ExecuteBuildTargetAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.NotNull(response.ValidationResults);
    Assert.NotNull(response.ValidationResults.Coverage);
    Assert.NotNull(response.ValidationResults.Issues);
    }

    [Fact]
    public async Task ExecuteBuildTargetAsync_WithCleanDocumentationTarget_ShouldCleanOutput()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var request = new BuildTargetRequest
        {
            Target = "CleanDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string, string>
            {
                ["OutputPath"] = "docs/_site/",
                ["IntermediatePath"] = "docs/obj/"
            }
        };

        // Act
        var response = await service.ExecuteBuildTargetAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.True(response.Success);
    Assert.Equal(0, response.ExitCode);
    Assert.NotNull(response.FilesDeleted);
    Assert.True(response.FilesDeleted!.Count > 0);
    }

    [Fact]
    public async Task ExecuteBuildTargetAsync_WithInvalidProjectFile_ShouldFail()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var request = new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = "non/existent/project.csproj"
        };

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            service.ExecuteBuildTargetAsync(request));
    }

    [Fact]
    public async Task ExecuteBuildTargetAsync_WithInvalidTarget_ShouldFail()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var request = new BuildTargetRequest
        {
            Target = "NonExistentTarget",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj"
        };

        // Act
        var response = await service.ExecuteBuildTargetAsync(request);

        // Assert
    Assert.NotNull(response);
    Assert.False(response.Success);
    Assert.NotEqual(0, response.ExitCode);
    Assert.NotNull(response.Errors);
    Assert.True(response.Errors!.Length > 0);
    }

    [Fact]
    public void GetAvailableTargets_ShouldReturnDocumentationTargets()
    {
        // Arrange
        var service = CreateBuildIntegrationService();

        // Act
        var targets = service.GetAvailableTargets();

        // Assert
    Assert.NotNull(targets);
    Assert.True(targets.Any());
    Assert.Contains("GenerateDocumentation", targets);
    Assert.Contains("ValidateDocumentation", targets);
    Assert.Contains("CleanDocumentation", targets);
    Assert.All(targets, t => Assert.False(string.IsNullOrEmpty(t)));
    }

    [Fact]
    public void GetTargetProperties_ForGenerateDocumentationTarget_ShouldReturnRequiredProperties()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var target = "GenerateDocumentation";

        // Act
        var properties = service.GetTargetProperties(target);

        // Assert
    Assert.NotNull(properties);
    Assert.True(properties.Any());
    Assert.True(properties.ContainsKey("DocFxConfigPath"));
    Assert.True(properties.ContainsKey("OutputPath"));
    Assert.True(properties.ContainsKey("LogLevel"));
    Assert.Equal("docs/docfx.json", properties["DocFxConfigPath"].DefaultValue);
    Assert.Equal("docs/_site/", properties["OutputPath"].DefaultValue);
    }

    [Fact]
    public async Task RegisterCustomTarget_WithValidTargetDefinition_ShouldSucceed()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var targetDefinition = new BuildTargetDefinition
        {
            Name = "CustomDocumentationTarget",
            Command = "docfx",
            Arguments = "--custom-args",
            WorkingDirectory = "docs/",
            Properties = new Dictionary<string, BuildPropertyDefinition>
            {
                ["CustomProperty"] = new BuildPropertyDefinition
                {
                    Description = "Custom documentation property",
                    DefaultValue = "default",
                    Required = false
                }
            }
        };

        // Act
        await service.RegisterCustomTargetAsync(targetDefinition);

        // Assert
    var targets = service.GetAvailableTargets();
    Assert.Contains("CustomDocumentationTarget", targets);
    }

    [Fact]
    public async Task ExecuteBuildTargetAsync_ShouldProvideProgressReporting()
    {
        // Arrange
        var service = CreateBuildIntegrationService();
        var progressReports = new List<BuildProgress>();
        var request = new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj"
        };

        // Act
        var response = await service.ExecuteBuildTargetAsync(request, progress =>
        {
            progressReports.Add(progress);
            return Task.CompletedTask;
        });

        // Assert
    Assert.NotNull(response);
    Assert.True(progressReports.Any());
    Assert.Contains(progressReports, p => p.Phase == BuildPhase.Starting);
    Assert.Contains(progressReports, p => p.Phase == BuildPhase.Completed || p.Phase == BuildPhase.Failed);
    Assert.All(progressReports, p => Assert.False(string.IsNullOrEmpty(p.Message)));
    }

    private static IBuildIntegrationService CreateBuildIntegrationService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleBuildIntegrationService();
    }
}
