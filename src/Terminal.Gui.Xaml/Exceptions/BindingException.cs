// <copyright file="BindingException.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Terminal.Gui.Xaml.Exceptions;

/// <summary>
/// The exception that is thrown when a data binding error occurs in the XAML framework.
/// </summary>
[Serializable]
public class BindingException : XamlException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindingException"/> class.
    /// </summary>
    public BindingException ()
        : base ("A data binding error occurred.")
    {
        ErrorCode = "XAML1001";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BindingException (string message)
        : base (message)
    {
        ErrorCode = "XAML1001";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BindingException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BindingException (string message, Exception innerException)
        : base (message, innerException)
    {
        ErrorCode = "XAML1001";
    }

    /// <summary>
    /// Gets or sets the binding expression that caused the error.
    /// </summary>
    public string? BindingExpression { get; set; }
}
