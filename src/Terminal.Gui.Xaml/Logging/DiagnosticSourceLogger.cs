// <copyright file="DiagnosticSourceLogger.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Logger that emits events to DiagnosticSource for advanced tracing.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DiagnosticSourceLogger"/> class.
/// </remarks>
/// <param name="sourceName">The name of the diagnostic source.</param>
public sealed class DiagnosticSourceLogger (string sourceName) : IXamlLogger
{
    private readonly DiagnosticSource _diagnosticSource = new DiagnosticListener (sourceName);
    private readonly string _sourceName = sourceName;

    /// <inheritdoc/>
    public void LogTrace (string message, params object [] args) => Write ("Trace", message, args);

    /// <inheritdoc/>
    public void LogDebug (string message, params object [] args) => Write ("Debug", message, args);

    /// <inheritdoc/>
    public void LogInformation (string message, params object [] args) => Write ("Information", message, args);

    /// <inheritdoc/>
    public void LogWarning (string message, params object [] args) => Write ("Warning", message, args);

    /// <inheritdoc/>
    public void LogError (string message, params object [] args) => Write ("Error", message, args);
#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public void LogError (Exception exception, string message, params object [] args) => Write ("Error", message, args, exception);
#pragma warning restore CA1822 // Mark members as static
    /// <inheritdoc/>
    public void LogCritical (string message, params object [] args) => Write ("Critical", message, args);
#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public void LogCritical (Exception exception, string message, params object [] args) => Write ("Critical", message, args, exception);
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public IDisposable BeginScope (string operationName) => NullScope.Instance;
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public void LogPerformanceMetric (string operationName, double elapsedMs, long memoryUsedBytes = 0) => Write ("PerformanceMetric", $"{operationName} {elapsedMs}ms {memoryUsedBytes}bytes");
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    /// <inheritdoc/>
    public bool IsEnabled (LogLevel logLevel) => true;
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Writes a log event to the diagnostic source.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="args">Optional log arguments.</param>
    /// <param name="exception">Optional exception.</param>
    private void Write (string level, string message, object []? args = null, Exception? exception = null)
    {
        if (_diagnosticSource.IsEnabled (level))
        {
            _diagnosticSource.Write (level, new
            {
                Message = message,
                Args = args ?? [],
                Exception = exception,
                Source = _sourceName,
                Timestamp = DateTime.UtcNow,
            });
        }
    }

    /// <summary>
    /// Provides a null scope for BeginScope.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new ();

        private NullScope ()
        {
        }

        public void Dispose ()
        {
        }
    }
}
