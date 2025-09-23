// <copyright file="IXamlLogger.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Provides structured logging capabilities for XAML framework operations.
/// </summary>
public interface IXamlLogger
{
    /// <summary>
    /// Logs a trace message for detailed diagnostic information.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogTrace(string message, params object[] args);

    /// <summary>
    /// Logs a debug message for development diagnostic information.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a critical error message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogCritical(string message, params object[] args);

    /// <summary>
    /// Logs a critical error message with exception details.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional message arguments.</param>
    void LogCritical(Exception exception, string message, params object[] args);

    /// <summary>
    /// Begins a logical operation scope for performance tracking.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <returns>A disposable scope for the operation.</returns>
    IDisposable BeginScope(string operationName);

    /// <summary>
    /// Logs performance metrics for constitutional compliance monitoring.
    /// </summary>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="memoryUsedBytes">The memory used in bytes.</param>
    void LogPerformanceMetric(string operationName, double elapsedMs, long memoryUsedBytes = 0);

    /// <summary>
    /// Gets a value indicating whether the specified log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if the log level is enabled; otherwise, false.</returns>
    bool IsEnabled(LogLevel logLevel);
}

/// <summary>
/// Defines log levels for filtering log output.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Trace level for very detailed diagnostic information.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Debug level for diagnostic information useful during development.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Information level for general informational messages.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Warning level for potentially harmful situations.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error level for error events that might allow the application to continue.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Critical level for very serious error events.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// None level to disable all logging.
    /// </summary>
    None = 6,
}
