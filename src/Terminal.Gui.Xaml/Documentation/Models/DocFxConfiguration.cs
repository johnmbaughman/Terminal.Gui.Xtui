// <copyright file="DocFxConfiguration.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents the DocFX project configuration settings.
/// </summary>
public class DocFxConfiguration
{
    /// <summary>
    /// Gets or sets the name of the documentation project.
    /// </summary>
    [Required(ErrorMessage = "ProjectName is required")]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the documentation version (synced with assembly version).
    /// </summary>
    [Required(ErrorMessage = "Version is required")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path where documentation is generated.
    /// </summary>
    [Required(ErrorMessage = "OutputPath is required")]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source code paths to document.
    /// </summary>
    [Required(ErrorMessage = "SourcePaths must contain at least one valid path")]
    [MinLength(1, ErrorMessage = "SourcePaths must contain at least one valid path")]
    public string[] SourcePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the files/patterns to exclude from documentation.
    /// </summary>
    public string[] ExcludePatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the custom template settings.
    /// </summary>
    public TemplateConfiguration? TemplateSettings { get; set; }

    /// <summary>
    /// Gets or sets the build-specific settings.
    /// </summary>
    public BuildConfiguration? BuildSettings { get; set; }

    /// <summary>
    /// Gets or sets the DocFX metadata configuration.
    /// </summary>
    public MetadataConfiguration? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the DocFX build configuration.
    /// </summary>
    public BuildConfiguration? Build { get; set; }
}

/// <summary>
/// Represents DocFX metadata configuration.
/// </summary>
public class MetadataConfiguration
{
    /// <summary>
    /// Gets or sets the source configurations.
    /// </summary>
    [Required(ErrorMessage = "Src is required")]
    [MinLength(1, ErrorMessage = "Src must contain at least one source configuration")]
    public SourceConfiguration[] Src { get; set; } = Array.Empty<SourceConfiguration>();

    /// <summary>
    /// Gets or sets the destination path for generated metadata.
    /// </summary>
    [Required(ErrorMessage = "Dest is required")]
    public string Dest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Represents a source configuration for DocFX.
/// </summary>
public class SourceConfiguration
{
    /// <summary>
    /// Gets or sets the file patterns to include.
    /// </summary>
    [Required(ErrorMessage = "Files is required")]
    [MinLength(1, ErrorMessage = "Files must contain at least one pattern")]
    public string[] Files { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the source path.
    /// </summary>
    [Required(ErrorMessage = "Src is required")]
    public string Src { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the exclude patterns.
    /// </summary>
    public string[]? Exclude { get; set; }
}

/// <summary>
/// Represents DocFX build configuration content.
/// </summary>
public class ContentConfiguration
{
    /// <summary>
    /// Gets or sets the file patterns to include.
    /// </summary>
    public string[] Files { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the source path.
    /// </summary>
    public string Src { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination path.
    /// </summary>
    public string Dest { get; set; } = string.Empty;
}

/// <summary>
/// Represents DocFX resource configuration.
/// </summary>
public class ResourceConfiguration
{
    /// <summary>
    /// Gets or sets the file patterns to include.
    /// </summary>
    public string[] Files { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the source path.
    /// </summary>
    public string? Src { get; set; }
}
