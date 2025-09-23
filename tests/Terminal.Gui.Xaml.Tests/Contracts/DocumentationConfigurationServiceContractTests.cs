// <copyright file="DocumentationConfigurationServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for IDocumentationConfigurationService.
/// These tests define the expected behavior and must FAIL until implementation is complete.
/// </summary>
public class DocumentationConfigurationServiceContractTests
{
    [Fact]
    public async Task LoadConfigurationAsync_WithValidPath_ShouldReturnConfiguration()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
        var configPath = "docs/docfx.json";

        // Act
        var config = await service.LoadConfigurationAsync(configPath);

        // Assert
        config.Should().NotBeNull();
        config.Metadata.Should().NotBeNull();
        config.Build.Should().NotBeNull();
        config.Metadata.Src.Should().NotBeEmpty();
        config.Build.Dest.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoadConfigurationAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
        var configPath = "non/existent/docfx.json";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            service.LoadConfigurationAsync(configPath));
    }

    [Fact]
    public async Task SaveConfigurationAsync_WithValidConfiguration_ShouldPersist()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
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
                Template = new[] { "default", "modern" }
            }
        };
        var configPath = "temp/docfx.json";

        // Act
        await service.SaveConfigurationAsync(config, configPath);

        // Assert - Verify file was created and can be loaded back
        var loadedConfig = await service.LoadConfigurationAsync(configPath);
        loadedConfig.Should().BeEquivalentTo(config);
    }

    [Fact]
    public async Task CreateDefaultConfigurationAsync_ShouldReturnValidConfiguration()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
        var projectPath = "src/Terminal.Gui.Xaml/";

        // Act
        var config = await service.CreateDefaultConfigurationAsync(projectPath);

        // Assert
        config.Should().NotBeNull();
        config.Metadata.Should().NotBeNull();
        config.Build.Should().NotBeNull();
        config.Metadata.Src.Should().NotBeEmpty();
        config.Metadata.Src[0].Src.Should().Be(projectPath);
        config.Build.Template.Should().Contain("default");
        config.Build.GlobalMetadata.Should().ContainKey("_appTitle");
        config.Build.GlobalMetadata.Should().ContainKey("_appFooter");
    }

    [Fact]
    public async Task ValidateConfigurationAsync_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
        var config = await service.CreateDefaultConfigurationAsync("src/Terminal.Gui.Xaml/");

        // Act
        var validation = await service.ValidateConfigurationAsync(config);

        // Assert
        validation.Should().NotBeNull();
        validation.IsValid.Should().BeTrue();
        validation.Errors.Should().BeEmpty();
        validation.Warnings.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateConfigurationAsync_WithInvalidPaths_ShouldFail()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();
        var config = new DocFxConfiguration
        {
            Metadata = new MetadataConfiguration
            {
                Src = new[]
                {
                    new SourceConfiguration
                    {
                        Files = new[] { "**/*.cs" },
                        Src = "non/existent/path/"
                    }
                },
                Dest = "api/"
            },
            Build = new BuildConfiguration
            {
                Content = new[]
                {
                    new ContentConfiguration
                    {
                        Files = new[] { "**/*.md" },
                        Src = "non/existent/docs/",
                        Dest = "."
                    }
                },
                Dest = "_site/"
            }
        };

        // Act
        var validation = await service.ValidateConfigurationAsync(config);

        // Assert
        validation.Should().NotBeNull();
        validation.IsValid.Should().BeFalse();
        validation.Errors.Should().NotBeEmpty();
        validation.Errors.Should().Contain(error =>
            error.Contains("non/existent/path"));
    }

    [Fact]
    public void GetSupportedTemplates_ShouldReturnAvailableTemplates()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();

        // Act
        var templates = service.GetSupportedTemplates();

        // Assert
        templates.Should().NotBeNull();
        templates.Should().NotBeEmpty();
        templates.Should().Contain("default");
        templates.Should().Contain("modern");
        templates.Should().AllSatisfy(template => template.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetDefaultGlobalMetadata_ShouldIncludeRequiredProperties()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();

        // Act
        var metadata = service.GetDefaultGlobalMetadata();

        // Assert
        metadata.Should().NotBeNull();
        metadata.Should().ContainKey("_appTitle");
        metadata.Should().ContainKey("_appFooter");
        metadata.Should().ContainKey("_enableSearch");
        metadata.Should().ContainKey("_enableNewTab");
        metadata["_enableSearch"].Should().Be(true);
        metadata["_enableNewTab"].Should().Be(true);
    }

    private static IDocumentationConfigurationService CreateDocumentationConfigurationService()
    {
        // This will fail until the interface and implementation are created
        throw new NotImplementedException("IDocumentationConfigurationService not implemented yet");
    }
}

// These types will fail to compile until implemented in Phase 3.3
public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
