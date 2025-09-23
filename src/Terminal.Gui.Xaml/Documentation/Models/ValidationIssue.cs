// <copyright file="ValidationIssue.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents a specific documentation validation issue.
/// </summary>
public class ValidationIssue
{
    /// <summary>
    /// Gets or sets the type of issue.
    /// </summary>
    public IssueType IssueType { get; set; }

    /// <summary>
    /// Gets or sets the severity of the issue.
    /// </summary>
    public IssueSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the human-readable issue description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file where issue was found.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number of issue (if applicable).
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the member that has the issue (if applicable).
    /// </summary>
    public string? MemberName { get; set; }

    /// <summary>
    /// Gets or sets the suggested fix for the issue.
    /// </summary>
    public string? Suggestion { get; set; }
}

/// <summary>
/// Specifies the type of documentation issue.
/// </summary>
public enum IssueType
{
    /// <summary>
    /// Missing XML documentation.
    /// </summary>
    MissingDocumentation,

    /// <summary>
    /// Invalid or broken link.
    /// </summary>
    InvalidLink,

    /// <summary>
    /// Invalid or non-compiling code example.
    /// </summary>
    InvalidExample,

    /// <summary>
    /// Malformed XML documentation.
    /// </summary>
    MalformedXml,

    /// <summary>
    /// Missing return documentation.
    /// </summary>
    MissingReturnDoc,

    /// <summary>
    /// Missing parameter documentation.
    /// </summary>
    MissingParameterDoc
}

/// <summary>
/// Specifies the severity of a documentation issue.
/// </summary>
public enum IssueSeverity
{
    /// <summary>
    /// Critical error that must be fixed.
    /// </summary>
    Error,

    /// <summary>
    /// Warning that should be addressed.
    /// </summary>
    Warning,

    /// <summary>
    /// Informational notice.
    /// </summary>
    Information
}
