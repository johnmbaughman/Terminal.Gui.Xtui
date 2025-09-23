// <copyright file="DiagnosticSourceLogger.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Logger that emits events to DiagnosticSource for advanced tracing.
/// </summary>
public sealed class DiagnosticSourceLogger : IXamlLogger
{
    private readonly DiagnosticSource _diagnosticSource;
    private readonly string _sourceName;

    public DiagnosticSourceLogger(string sourceName)
    {
        _sourceName = sourceName;
        _diagnosticSource = new DiagnosticListener(sourceName);
    }

    public void LogTrace(string message, params object[] args) => Write("Trace", message, args);
    public void LogDebug(string message, params object[] args) => Write("Debug", message, args);
    public void LogInformation(string message, params object[] args) => Write("Information", message, args);
    public void LogWarning(string message, params object[] args) => Write("Warning", message, args);
    public void LogError(string message, params object[] args) => Write("Error", message, args);
#pragma warning disable CA1822 // Mark members as static
    public void LogError(Exception exception, string message, params object[] args) => Write("Error", message, args, exception);
#pragma warning restore CA1822 // Mark members as static
    public void LogCritical(string message, params object[] args) => Write("Critical", message, args);
#pragma warning disable CA1822 // Mark members as static
    public void LogCritical(Exception exception, string message, params object[] args) => Write("Critical", message, args, exception);
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    public IDisposable BeginScope(string operationName) => null;
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    public void LogPerformanceMetric(string operationName, double elapsedMs, long memoryUsedBytes = 0) => Write("PerformanceMetric", $"{operationName} {elapsedMs}ms {memoryUsedBytes}bytes");
#pragma warning restore CA1822 // Mark members as static
#pragma warning disable CA1822 // Mark members as static
    public bool IsEnabled(LogLevel logLevel) => true;
#pragma warning restore CA1822 // Mark members as static

    private void Write(string level, string message, object[] args, Exception? exception = null)
    {
        if (_diagnosticSource.IsEnabled(level))
        {
            _diagnosticSource.Write(level, new
            {
                Message = message,
                Args = args,
                Exception = exception,
                Source = _sourceName,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
