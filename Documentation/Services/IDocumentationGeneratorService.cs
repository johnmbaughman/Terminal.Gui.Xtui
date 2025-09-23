// <copyright file="IDocumentationGeneratorService.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Provides methods for generating and validating documentation.
/// </summary>
public interface IDocumentationGeneratorService
{
    /// <summary>
    /// Generates documentation for the framework.
    /// </summary>
    /// <param name="request">The documentation generation request.</param>
    /// <returns>The documentation generation response.</returns>
    Task<GenerateDocumentationResponse> GenerateDocumentationAsync(GenerateDocumentationRequest request);

    /// <summary>
    /// Validates documentation quality without generation.
    /// </summary>
    /// <param name="request">The documentation validation request.</param>
    /// <returns>The documentation validation response.</returns>
    Task<ValidateDocumentationResponse> ValidateDocumentationAsync(ValidateDocumentationRequest request);
}

public class GenerateDocumentationRequest
{
    public DocFxConfiguration Configuration { get; set; }
    public string[] SourcePaths { get; set; }
    public DocumentationBuildMode BuildMode { get; set; }
    public bool ValidateOnly { get; set; } = false;
}

public class GenerateDocumentationResponse
{
    public bool Success { get; set; }
    public string OutputPath { get; set; }
    public ApiDocumentation[] GeneratedDocuments { get; set; }
    public DocumentationValidationResult ValidationResult { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public string[] Errors { get; set; }
}

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
