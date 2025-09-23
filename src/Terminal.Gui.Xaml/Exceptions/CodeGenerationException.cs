// <copyright file="CodeGenerationException.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Runtime.Serialization;

namespace Terminal.Gui.Xaml.Exceptions;

/// <summary>
/// The exception that is thrown when an error occurs during code generation.
/// </summary>
[Serializable]
public class CodeGenerationException : XamlException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeGenerationException"/> class.
    /// </summary>
    public CodeGenerationException()
        : base("An error occurred during code generation.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeGenerationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CodeGenerationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeGenerationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CodeGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeGenerationException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    protected CodeGenerationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the class being generated when the error occurred.
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the name of the method being generated when the error occurred.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Gets or sets the error code for localization and diagnostics.
    /// </summary>
    public string ErrorCode { get; set; } = "XAML2001";

    /// <summary>
    /// Gets or sets a localized error message, if available.
    /// </summary>
    public string? LocalizedMessage { get; set; }
    public string? MethodName { get; set; }

    /// <summary>
    /// Gets or sets the generated code that caused the error, if available.
    /// </summary>
    public string? GeneratedCode { get; set; }

    /// <summary>
    /// Creates a new <see cref="CodeGenerationException"/> for an invalid type error.
    /// </summary>
    /// <param name="typeName">The invalid type name.</param>
    /// <param name="className">The class being generated.</param>
    /// <returns>A new <see cref="CodeGenerationException"/> instance.</returns>
    public static CodeGenerationException InvalidType(string typeName, string className)
    {
        var message = $"Cannot generate code for type '{typeName}'. The type is not supported or cannot be found.";
        return new CodeGenerationException(message)
        {
            ClassName = className,
            ErrorCode = "CODEGEN0001"
        };
    }

    /// <summary>
    /// Creates a new <see cref="CodeGenerationException"/> for a compilation error.
    /// </summary>
    /// <param name="compilationError">The compilation error message.</param>
    /// <param name="className">The class being generated.</param>
    /// <param name="generatedCode">The generated code that failed to compile.</param>
    /// <returns>A new <see cref="CodeGenerationException"/> instance.</returns>
    public static CodeGenerationException CompilationError(string compilationError, string className, string generatedCode)
    {
        var message = $"Generated code for class '{className}' failed to compile: {compilationError}";
        return new CodeGenerationException(message)
        {
            ClassName = className,
            GeneratedCode = generatedCode,
            ErrorCode = "CODEGEN0002"
        };
    }

    /// <summary>
    /// Creates a new <see cref="CodeGenerationException"/> for a template error.
    /// </summary>
    /// <param name="templateName">The name of the template that failed.</param>
    /// <param name="className">The class being generated.</param>
    /// <param name="templateError">The template error details.</param>
    /// <returns>A new <see cref="CodeGenerationException"/> instance.</returns>
    public static CodeGenerationException TemplateError(string templateName, string className, string templateError)
    {
        var message = $"Code generation template '{templateName}' failed for class '{className}': {templateError}";
        return new CodeGenerationException(message)
        {
            ClassName = className,
            ErrorCode = "CODEGEN0003"
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

        info.AddValue(nameof(ClassName), ClassName);
        info.AddValue(nameof(MethodName), MethodName);
        info.AddValue(nameof(GeneratedCode), GeneratedCode);

        base.GetObjectData(info, context);
    }
}
