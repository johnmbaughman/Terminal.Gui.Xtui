// <copyright file="XamlParseException.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace Terminal.Gui.Xaml.Exceptions;

/// <summary>
/// The exception that is thrown when an error occurs during XAML parsing.
/// </summary>
[Serializable]
public class XamlParseException : XamlException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class.
    /// </summary>
    public XamlParseException()
        : base("An error occurred while parsing XAML.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public XamlParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public XamlParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class with location information.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="filePath">The XAML file path where the error occurred.</param>
    /// <param name="lineNumber">The line number where the error occurred.</param>
    /// <param name="columnNumber">The column number where the error occurred.</param>
    public XamlParseException(string message, string filePath, int lineNumber, int columnNumber)
        : base(message)
    {
        XamlFilePath = filePath;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        ErrorCode = "XAML0001";
        LocalizedMessage = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class with location information and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="filePath">The XAML file path where the error occurred.</param>
    /// <param name="lineNumber">The line number where the error occurred.</param>
    /// <param name="columnNumber">The column number where the error occurred.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public XamlParseException(string message, string filePath, int lineNumber, int columnNumber, Exception innerException)
        : base(message, innerException)
    {
        XamlFilePath = filePath;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        ErrorCode = "XAML0001";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlParseException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected XamlParseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Gets or sets the XAML element name where the parsing error occurred, if applicable.
    /// </summary>
    public string? ElementName { get; set; }

    /// <summary>
    /// Gets or sets the attribute name where the parsing error occurred, if applicable.
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// Gets or sets the expected token or syntax that was expected during parsing.
    /// </summary>
    public string? Expected { get; set; }

    /// <summary>
    /// Gets or sets the actual token or syntax that was encountered during parsing.
    /// </summary>
    public string? Actual { get; set; }

    /// <summary>
    /// Creates a new <see cref="XamlParseException"/> for an invalid element error.
    /// </summary>
    /// <param name="elementName">The invalid element name.</param>
    /// <param name="filePath">The XAML file path.</param>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>A new <see cref="XamlParseException"/> instance.</returns>
    public static XamlParseException InvalidElement(string elementName, string filePath, int lineNumber, int columnNumber)
    {
        var message = $"The element '{elementName}' is not recognized or is not a valid Terminal.Gui control.";
        return new XamlParseException(message, filePath, lineNumber, columnNumber)
        {
            ElementName = elementName,
            ErrorCode = "XAML1001"
        };
    }

    /// <summary>
    /// Creates a new <see cref="XamlParseException"/> for an invalid attribute error.
    /// </summary>
    /// <param name="attributeName">The invalid attribute name.</param>
    /// <param name="elementName">The element name containing the invalid attribute.</param>
    /// <param name="filePath">The XAML file path.</param>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>A new <see cref="XamlParseException"/> instance.</returns>
    public static XamlParseException InvalidAttribute(string attributeName, string elementName, string filePath, int lineNumber, int columnNumber)
    {
        var message = $"The attribute '{attributeName}' is not valid for the element '{elementName}'.";
        return new XamlParseException(message, filePath, lineNumber, columnNumber)
        {
            ElementName = elementName,
            AttributeName = attributeName,
            ErrorCode = "XAML1002"
        };
    }

    /// <summary>
    /// Creates a new <see cref="XamlParseException"/> for a malformed XAML syntax error.
    /// </summary>
    /// <param name="expected">The expected syntax.</param>
    /// <param name="actual">The actual syntax encountered.</param>
    /// <param name="filePath">The XAML file path.</param>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>A new <see cref="XamlParseException"/> instance.</returns>
    public static XamlParseException MalformedSyntax(string expected, string actual, string filePath, int lineNumber, int columnNumber)
    {
        var message = $"Invalid XAML syntax. Expected '{expected}' but found '{actual}'.";
        return new XamlParseException(message, filePath, lineNumber, columnNumber)
        {
            Expected = expected,
            Actual = actual,
            ErrorCode = "XAML1003"
        };
    }

    /// <summary>
    /// Sets serialization data for the exception.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination for this serialization.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        info.AddValue(nameof(ElementName), ElementName);
        info.AddValue(nameof(AttributeName), AttributeName);
        info.AddValue(nameof(Expected), Expected);
        info.AddValue(nameof(Actual), Actual);

        base.GetObjectData(info, context);
    }
}
