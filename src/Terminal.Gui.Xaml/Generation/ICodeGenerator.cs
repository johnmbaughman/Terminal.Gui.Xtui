// <copyright file="ICodeGenerator.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Generation;

/// <summary>
/// Defines the contract for code generation services.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Generates code from XAML asynchronously.
    /// </summary>
    /// <param name="xaml">The XAML content.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated code.</returns>
    Task<string> GenerateAsync(string xaml);
    
    /// <summary>
    /// Generates classes from multiple XAML documents asynchronously.
    /// </summary>
    /// <param name="xamlDocuments">The XAML documents.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated classes.</returns>
    Task<IEnumerable<GeneratedClass>> GenerateClassesAsync(IEnumerable<string> xamlDocuments);
    
    /// <summary>
    /// Validates whether the XAML can be used for code generation.
    /// </summary>
    /// <param name="xaml">The XAML content.</param>
    /// <returns>True if the XAML is valid for code generation; otherwise, false.</returns>
    bool ValidateGeneration(string xaml);

    /// <summary>
    /// Registers a custom template for code generation.
    /// </summary>
    /// <param name="templateName">The template name.</param>
    /// <param name="templateContent">The template content.</param>
    void RegisterTemplate(string templateName, string templateContent);
}

/// <summary>
/// Represents a generated class.
/// </summary>
public class GeneratedClass
{
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    public string ClassName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the generated code.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;
}