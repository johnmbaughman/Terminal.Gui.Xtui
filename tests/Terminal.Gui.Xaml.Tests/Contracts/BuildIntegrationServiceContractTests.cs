// <copyright file="BuildIntegrationServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.ExitCode.Should().Be(0);
        response.Output.Should().NotBeNullOrEmpty();
        response.Errors.Should().BeNullOrEmpty();
        response.Duration.Should().BeLessThan(TimeSpan.FromMinutes(5));
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.ValidationResults.Should().NotBeNull();
        response.ValidationResults.Coverage.Should().NotBeNull();
        response.ValidationResults.Issues.Should().NotBeNull();
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
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.ExitCode.Should().Be(0);
        response.FilesDeleted.Should().NotBeNull();
        response.FilesDeleted.Should().NotBeEmpty();
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
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.ExitCode.Should().NotBe(0);
        response.Errors.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetAvailableTargets_ShouldReturnDocumentationTargets()
    {
        // Arrange
        var service = CreateBuildIntegrationService();

        // Act
        var targets = service.GetAvailableTargets();

        // Assert
        targets.Should().NotBeNull();
        targets.Should().NotBeEmpty();
        targets.Should().Contain("GenerateDocumentation");
        targets.Should().Contain("ValidateDocumentation");
        targets.Should().Contain("CleanDocumentation");
        targets.Should().AllSatisfy(target => target.Should().NotBeNullOrEmpty());
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
        properties.Should().NotBeNull();
        properties.Should().NotBeEmpty();
        properties.Should().ContainKey("DocFxConfigPath");
        properties.Should().ContainKey("OutputPath");
        properties.Should().ContainKey("LogLevel");
        properties["DocFxConfigPath"].DefaultValue.Should().Be("docs/docfx.json");
        properties["OutputPath"].DefaultValue.Should().Be("docs/_site/");
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
        targets.Should().Contain("CustomDocumentationTarget");
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
        response.Should().NotBeNull();
        progressReports.Should().NotBeEmpty();
        progressReports.Should().Contain(p => p.Phase == BuildPhase.Starting);
        progressReports.Should().Contain(p => p.Phase == BuildPhase.Completed || p.Phase == BuildPhase.Failed);
        progressReports.Should().AllSatisfy(p => p.Message.Should().NotBeNullOrEmpty());
    }

    private static IBuildIntegrationService CreateBuildIntegrationService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IBuildIntegrationService not implemented yet");
    }
}

// These types will fail to compile until implemented in Phase 3.3
public class BuildTargetRequest
{
    public string Target { get; set; }
    public string ProjectFile { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}

public class BuildTargetResponse
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Output { get; set; }
    public string Errors { get; set; }
    public TimeSpan Duration { get; set; }
    public ValidateDocumentationResponse ValidationResults { get; set; }
    public List<string> FilesDeleted { get; set; } = new();
}

public class BuildTargetDefinition
{
    public string Name { get; set; }
    public string Command { get; set; }
    public string Arguments { get; set; }
    public string WorkingDirectory { get; set; }
    public Dictionary<string, BuildPropertyDefinition> Properties { get; set; } = new();
}

public class BuildPropertyDefinition
{
    public string Description { get; set; }
    public string DefaultValue { get; set; }
    public bool Required { get; set; }
}

public class BuildProgress
{
    public BuildPhase Phase { get; set; }
    public string Message { get; set; }
    public double PercentComplete { get; set; }
}

public enum BuildPhase
{
    Starting,
    Preparing,
    Building,
    Validating,
    Completing,
    Completed,
    Failed
}
