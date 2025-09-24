// <copyright file="GenerationAndValidationModels.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Request model for generating documentation.
/// </summary>
public class GenerateDocumentationRequest
{
    /// <summary>
    /// Gets or sets the DocFX configuration to use.
    /// </summary>
    public DocFxConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the source paths.
    /// </summary>
    public string[] SourcePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the build mode.
    /// </summary>
    public DocumentationBuildMode BuildMode { get; set; } = DocumentationBuildMode.Full;

    /// <summary>
    /// Gets or sets a value indicating whether to only validate and not generate files.
    /// </summary>
    public bool ValidateOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to perform incremental build.
    /// </summary>
    public bool IncrementalBuild { get; set; }

    /// <summary>
    /// Gets or sets the explicit output path to generate to.
    /// </summary>
    public string? OutputPath { get; set; }
}

/// <summary>
/// Response model for generating documentation.
/// </summary>
public class GenerateDocumentationResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether generation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the output path where generated files reside.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the list of generated documents.
    /// </summary>
    public IList<ApiDocumentation> GeneratedDocuments { get; set; } = new List<ApiDocumentation>();

    /// <summary>
    /// Gets or sets the validation result.
    /// </summary>
    public DocumentationValidationResult? ValidationResult { get; set; }

    /// <summary>
    /// Gets or sets the time it took to generate.
    /// </summary>
    public TimeSpan GenerationTime { get; set; }

    /// <summary>
    /// Gets or sets the total duration (alias for GenerationTime) for convenience.
    /// </summary>
    public TimeSpan Duration
    {
        get => GenerationTime;
        set => GenerationTime = value;
    }

    /// <summary>
    /// Gets or sets any errors that occurred.
    /// </summary>
    public string[] Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets additional metadata about the generation run.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of generated files (paths).
    /// </summary>
    public IList<string> GeneratedFiles { get; set; } = new List<string>();
}

/// <summary>
/// Request model for validating documentation against requirements.
/// </summary>
public class ValidateDocumentationRequest
{
    /// <summary>
    /// Gets or sets the documentation source paths.
    /// </summary>
    public string[] SourcePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether to check links.
    /// </summary>
    public bool CheckLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to compile examples.
    /// </summary>
    public bool CheckExamples { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum required documentation coverage percentage.
    /// </summary>
    [Range(0.0, 100.0)]
    public double MinimumCoverage { get; set; } = 80.0;
}

/// <summary>
/// Response model for documentation validation containing aggregate results.
/// </summary>
public class ValidateDocumentationResponse
{
    /// <summary>
    /// Gets or sets the validation result object.
    /// </summary>
    public DocumentationValidationResult ValidationResult { get; set; } = new();

    /// <summary>
    /// Gets or sets the coverage metrics.
    /// </summary>
    public CoverageMetrics Coverage { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of validation issues found.
    /// </summary>
    public ValidationIssue[] Issues { get; set; } = Array.Empty<ValidationIssue>();

    /// <summary>
    /// Gets or sets a value indicating whether the documentation passes minimal requirements.
    /// </summary>
    public bool PassesRequirements { get; set; }
}

/// <summary>
/// Represents a configuration validation result with errors and warnings.
/// </summary>
public class ConfigurationValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the configuration is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages.
    /// </summary>
    public IList<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the list of warning messages.
    /// </summary>
    public IList<string> Warnings { get; set; } = new List<string>();
}
