// <copyright file="BuildConfiguration.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents build-specific documentation settings.
/// </summary>
public class BuildConfiguration
{
    /// <summary>
    /// Gets or sets the build mode.
    /// </summary>
    public DocumentationBuildMode BuildMode { get; set; } = DocumentationBuildMode.Full;

    /// <summary>
    /// Gets or sets a value indicating whether to enable parallel processing.
    /// </summary>
    public bool ParallelBuild { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed build logging.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to treat documentation warnings as build errors.
    /// </summary>
    public bool WarningsAsErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum parallel operations.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "MaxConcurrency must be between 1 and {2}")]
    public int MaxConcurrency { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets a value indicating whether to enable incremental build caching.
    /// </summary>
    public bool CacheEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the content configurations.
    /// </summary>
    public ContentConfiguration[] Content { get; set; } = Array.Empty<ContentConfiguration>();

    /// <summary>
    /// Gets or sets the resource configurations.
    /// </summary>
    public ResourceConfiguration[] Resource { get; set; } = Array.Empty<ResourceConfiguration>();

    /// <summary>
    /// Gets or sets the destination path.
    /// </summary>
    [Required(ErrorMessage = "Dest is required")]
    public string Dest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the templates to use.
    /// </summary>
    public string[] Template { get; set; } = new[] { "default" };

    /// <summary>
    /// Gets or sets the global metadata.
    /// </summary>
    public Dictionary<string, object> GlobalMetadata { get; set; } = new();
}

/// <summary>
/// Specifies the documentation build mode.
/// </summary>
public enum DocumentationBuildMode
{
    /// <summary>
    /// Full documentation build.
    /// </summary>
    Full,

    /// <summary>
    /// Incremental build (only changed files).
    /// </summary>
    Incremental,

    /// <summary>
    /// Metadata only build.
    /// </summary>
    MetadataOnly
}
