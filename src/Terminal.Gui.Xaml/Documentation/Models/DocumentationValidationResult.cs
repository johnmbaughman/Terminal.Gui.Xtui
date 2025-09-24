// <copyright file="DocumentationValidationResult.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents validation results for documentation quality.
/// </summary>
public class DocumentationValidationResult
{
    /// <summary>
    /// Gets or sets what was validated (file path or identifier).
    /// </summary>
    public string ValidationTarget { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation status.
    /// </summary>
    public ValidationStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the list of validation issues found.
    /// </summary>
    public ValidationIssue [] Issues { get; set; } = [];

    /// <summary>
    /// Gets or sets the documentation coverage statistics.
    /// </summary>
    public CoverageMetrics? CoverageMetrics { get; set; }

    /// <summary>
    /// Gets or sets when validation was performed.
    /// </summary>
    public DateTime ValidationTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the validation summary.
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// Specifies the validation status.
/// </summary>
public enum ValidationStatus
{
    /// <summary>
    /// Validation completed successfully with no issues.
    /// </summary>
    Success,

    /// <summary>
    /// Validation completed with warnings.
    /// </summary>
    Warning,

    /// <summary>
    /// Validation failed with errors.
    /// </summary>
    Error,
}
