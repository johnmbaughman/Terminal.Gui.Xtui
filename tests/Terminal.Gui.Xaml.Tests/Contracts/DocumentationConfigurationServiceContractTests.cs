// <copyright file="DocumentationConfigurationServiceContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
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
    Assert.NotNull(config);
    Assert.NotNull(config.Metadata);
    Assert.NotNull(config.Build);
    Assert.NotEmpty(config.Metadata!.Src);
    Assert.False(string.IsNullOrEmpty(config.Build!.Dest));
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
                Template = new[] { "default", "modern" }
            }
        };
        var configPath = "temp/docfx.json";

        // Act
        await service.SaveConfigurationAsync(config, configPath);

        // Assert - Verify file was created and can be loaded back
        var loadedConfig = await service.LoadConfigurationAsync(configPath);
    Assert.Equal(config.Metadata!.Dest, loadedConfig.Metadata!.Dest);
    Assert.Equal(config.Build!.Dest, loadedConfig.Build!.Dest);
    Assert.Equal(config.Build.Template, loadedConfig.Build.Template);
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
    Assert.NotNull(config);
    Assert.NotNull(config.Metadata);
    Assert.NotNull(config.Build);
    Assert.NotEmpty(config.Metadata!.Src);
    Assert.Equal(projectPath, config.Metadata.Src[0].Src);
    Assert.Contains("default", config.Build!.Template);
    Assert.True(config.Build.GlobalMetadata.ContainsKey("_appTitle"));
    Assert.True(config.Build.GlobalMetadata.ContainsKey("_appFooter"));
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
    Assert.NotNull(validation);
    Assert.True(validation.IsValid);
    Assert.Empty(validation.Errors);
    Assert.NotNull(validation.Warnings);
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
        Assert.NotNull(validation);
        Assert.False(validation.IsValid);
        Assert.NotEmpty(validation.Errors);
        Assert.Contains(validation.Errors, e => e.Contains("non/existent/path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetSupportedTemplates_ShouldReturnAvailableTemplates()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();

        // Act
        var templates = service.GetSupportedTemplates();

        // Assert
    Assert.NotNull(templates);
    Assert.True(templates.Any());
    Assert.Contains("default", templates);
    Assert.Contains("modern", templates);
    Assert.All(templates, t => Assert.False(string.IsNullOrEmpty(t)));
    }

    [Fact]
    public void GetDefaultGlobalMetadata_ShouldIncludeRequiredProperties()
    {
        // Arrange
        var service = CreateDocumentationConfigurationService();

        // Act
        var metadata = service.GetDefaultGlobalMetadata();

        // Assert
    Assert.NotNull(metadata);
    Assert.True(metadata.ContainsKey("_appTitle"));
    Assert.True(metadata.ContainsKey("_appFooter"));
    Assert.True(metadata.ContainsKey("_enableSearch"));
    Assert.True(metadata.ContainsKey("_enableNewTab"));
    Assert.True((bool)metadata["_enableSearch"]);
    Assert.True((bool)metadata["_enableNewTab"]);
    }

    private static IDocumentationConfigurationService CreateDocumentationConfigurationService()
    {
        return new Terminal.Gui.Xaml.Documentation.Services.Implementations.SimpleDocumentationConfigurationService();
    }
}
 
