// <copyright file="IXamlParser.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Model;

namespace Terminal.Gui.Xaml.Parsing;

/// <summary>
/// Defines the contract for XAML parsing services.
/// </summary>
public interface IXamlParser
{
    /// <summary>
    /// Parses XAML content asynchronously.
    /// </summary>
    /// <param name="xaml">The XAML content to parse.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the parsed XAML document.</returns>
    Task<XamlDocument> ParseAsync(string xaml);

    /// <summary>
    /// Parses a XAML file asynchronously.
    /// </summary>
    /// <param name="filePath">The XAML file path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the parsed XAML document.</returns>
    Task<XamlDocument> ParseFileAsync(string filePath);
    
    /// <summary>
    /// Validates XAML syntax.
    /// </summary>
    /// <param name="xaml">The XAML content to validate.</param>
    /// <returns>True if the XAML syntax is valid; otherwise, false.</returns>
    bool ValidateSyntax(string xaml);

    /// <summary>
    /// Validates the provided XAML and returns true if valid.
    /// </summary>
    /// <param name="xaml">The XAML to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool Validate(string xaml);

    /// <summary>
    /// Registers a custom control type by name for use in XAML.
    /// </summary>
    /// <param name="controlName">The control name.</param>
    /// <param name="controlType">The control type.</param>
    void RegisterControl(string controlName, Type controlType);
    
    /// <summary>
    /// Gets parsing errors from the last parsing operation.
    /// </summary>
    /// <returns>A collection of parsing errors.</returns>
    IEnumerable<XamlParsingError> GetParsingErrors();
}

/// <summary>
/// Represents a XAML parsing error.
/// </summary>
public class XamlParsingError
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the line number where the error occurred.
    /// </summary>
    public int LineNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the column number where the error occurred.
    /// </summary>
    public int ColumnNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the error severity.
    /// </summary>
    public XamlErrorSeverity Severity { get; set; }
}

/// <summary>
/// Represents the severity of a XAML parsing error.
/// </summary>
public enum XamlErrorSeverity
{
    /// <summary>
    /// Information message.
    /// </summary>
    Info,
    
    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error message.
    /// </summary>
    Error,
    
    /// <summary>
    /// Fatal error message.
    /// </summary>
    Fatal
}