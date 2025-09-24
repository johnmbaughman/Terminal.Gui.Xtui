// <copyright file="XamlException.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace Terminal.Gui.Xaml.Exceptions;

/// <summary>
/// The base exception class for all Terminal.Gui XAML framework exceptions.
/// </summary>
[Serializable]
public class XamlException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XamlException"/> class.
    /// </summary>
    public XamlException ()
        : base ("An error occurred in the XAML framework.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public XamlException (string message)
        : base (message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public XamlException (string message, Exception innerException)
        : base (message, innerException)
    {
    }

    /// <summary>
    /// Gets or sets the XAML file path where the error occurred, if applicable.
    /// </summary>
    public string? XamlFilePath { get; set; }

    /// <summary>
    /// Gets or sets the line number where the error occurred, if applicable.
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the column number where the error occurred, if applicable.
    /// </summary>
    public int? ColumnNumber { get; set; }

    /// <summary>
    /// Gets or sets the error code for localization and diagnostics.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets a localized error message, if available.
    /// </summary>
    public string? LocalizedMessage { get; set; }

    /// <summary>
    /// Gets a formatted error message that includes location information if available.
    /// </summary>
    public string DetailedMessage
    {
        get
        {
            string message = Message;

            if (!string.IsNullOrEmpty (XamlFilePath))
            {
                message += $" File: {XamlFilePath}";
                if (LineNumber.HasValue)
                {
                    message += $", Line: {LineNumber}";
                    if (ColumnNumber.HasValue)
                    {
                        message += $", Column: {ColumnNumber}";
                    }
                }
            }

            if (!string.IsNullOrEmpty (ErrorCode))
            {
                message += $" (Error Code: {ErrorCode})";
            }

            return message;
        }
    }

    /// <summary>
    /// Serializes this exception to JSON using System.Text.Json.
    /// </summary>
    /// <returns>A JSON string representing the exception.</returns>
    public string ToJson ()
    {
        return System.Text.Json.JsonSerializer.Serialize (this);
    }
}
