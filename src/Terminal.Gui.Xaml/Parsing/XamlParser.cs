// <copyright file="XamlParser.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui.Xaml.Exceptions;
using Terminal.Gui.Xaml.Model;

namespace Terminal.Gui.Xaml.Parsing;

/// <summary>
/// XML parsing service for XAML documents with namespace and validation support.
/// </summary>
public sealed class XamlParser : IXamlParser
{
    private readonly Dictionary<string, Type> _controls = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<XamlParsingError> _errors = new();
    /// <summary>
    /// Parses XAML text asynchronously and returns a XamlDocument.
    /// </summary>
    /// <param name="xaml">The XAML text to parse.</param>
    /// <returns>A task that represents the asynchronous parse operation.</returns>
    public async Task<XamlDocument> ParseAsync(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml))
        {
            throw new XamlParseException("XAML content is empty.");
        }
        try
        {
            var doc = await Task.Run(() => XDocument.Parse(xaml));
            return ParseDocument(doc);
        }
        catch (Exception ex)
        {
            throw new XamlParseException("Failed to parse XAML.", ex);
        }
    }
    /// <summary>
    /// Parses a XAML file asynchronously and returns a XamlDocument.
    /// </summary>
    /// <param name="filePath">The path to the XAML file.</param>
    /// <returns>A task that represents the asynchronous parse operation.</returns>
    public async Task<XamlDocument> ParseFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new XamlParseException($"File not found: {filePath}");
        }
        var xaml = await File.ReadAllTextAsync(filePath);
        return await ParseAsync(xaml);
    }
    /// <summary>
    /// Validates the XAML text for syntax correctness.
    /// </summary>
    /// <param name="xaml">The XAML text to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public bool Validate(string xaml)
    {
        return ValidateSyntax(xaml);
    }

    /// <summary>
    /// Validates the XAML syntax using XDocument.Parse.
    /// </summary>
    /// <param name="xaml">The XAML text to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public bool ValidateSyntax(string xaml)
    {
        try
        {
            XDocument.Parse(xaml);
            return true;
        }
        catch (Exception ex)
        {
            _errors.Add(new XamlParsingError
            {
                Message = ex.Message,
                LineNumber = 0,
                ColumnNumber = 0,
                Severity = XamlErrorSeverity.Error
            });
            return false;
        }
    }
    
    /// <summary>
    /// Registers a control type by name for use in parsing.
    /// </summary>
    /// <param name="controlName">The name of the control.</param>
    /// <param name="controlType">The type of the control.</param>
    public void RegisterControl(string controlName, Type controlType)
    {
        _controls[controlName] = controlType;
    }

    /// <summary>
    /// Gets the collection of parsing errors encountered during validation.
    /// </summary>
    /// <returns>An enumerable of XamlParsingError.</returns>
    public IEnumerable<XamlParsingError> GetParsingErrors()
    {
        return _errors;
    }

    private static XamlDocument ParseDocument(XDocument doc)
    {
        // TODO: Map XDocument to XamlDocument, support namespaces, validation, caching
        return new XamlDocument
        {
            FilePath = string.Empty,
            RootNamespace = null,
            XamlText = null
        };
    }
}

