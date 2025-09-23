// <copyright file="ApiDocumentation.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents generated API documentation metadata.
/// </summary>
public class ApiDocumentation
{
    /// <summary>
    /// Gets or sets the assembly being documented.
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace being documented.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type being documented.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the member being documented (optional).
    /// </summary>
    public string? MemberName { get; set; }

    /// <summary>
    /// Gets or sets the percentage of XML documentation coverage.
    /// </summary>
    public double DocumentationCoverage { get; set; }

    /// <summary>
    /// Gets or sets the path to generated documentation file.
    /// </summary>
    public string GeneratedPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of last generation.
    /// </summary>
    public DateTime LastGenerated { get; set; }
}
