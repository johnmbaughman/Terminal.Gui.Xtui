// <copyright file="TemplateConfiguration.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents DocFX template customization settings.
/// </summary>
public class TemplateConfiguration
{
    /// <summary>
    /// Gets or sets the name of DocFX template to use.
    /// </summary>
    public string Name { get; set; } = "default";

    /// <summary>
    /// Gets or sets the path to the template.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to custom CSS files.
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Gets or sets the path to custom JavaScript files.
    /// </summary>
    public string? CustomJs { get; set; }

    /// <summary>
    /// Gets or sets the path to project logo.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// Gets or sets the path to favicon.
    /// </summary>
    public string? FaviconPath { get; set; }

    /// <summary>
    /// Gets or sets the primary branding color (hex).
    /// </summary>
    public string? BrandingColor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable full-text search.
    /// </summary>
    public bool EnableSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable dark mode toggle.
    /// </summary>
    public bool EnableDarkMode { get; set; } = true;

    /// <summary>
    /// Gets or sets custom template variables.
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = [];
}
