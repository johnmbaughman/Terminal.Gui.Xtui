// <copyright file="TestLogger.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Logging;

namespace Terminal.Gui.Xaml.Tests.Infrastructure;

/// <summary>
/// Test logger implementation for capturing log output in tests.
/// </summary>
/// <summary>
/// Simple in-memory logger used by tests.
/// </summary>
public static class TestLogger
{
    /// <summary>
    /// Gets a list of captured log entries.
    /// </summary>
    public static IList<string> LogEntries { get; } = new List<string>();

    /// <summary>
    /// Logs at trace level.
    /// </summary>
    public static void LogTrace(string message, params object[] args) => Log("Trace", message, args);
    /// <summary>
    /// Logs at debug level.
    /// </summary>
    public static void LogDebug(string message, params object[] args) => Log("Debug", message, args);
    /// <summary>
    /// Logs at information level.
    /// </summary>
    public static void LogInformation(string message, params object[] args) => Log("Information", message, args);
    /// <summary>
    /// Logs at warning level.
    /// </summary>
    public static void LogWarning(string message, params object[] args) => Log("Warning", message, args);
    /// <summary>
    /// Logs at error level.
    /// </summary>
    public static void LogError(string message, params object[] args) => Log("Error", message, args);
    /// <summary>
    /// Logs an exception at error level.
    /// </summary>
    public static void LogError(Exception exception, string message, params object[] args) => Log("Error", $"{message}: {exception}", args);
    /// <summary>
    /// Logs at critical level.
    /// </summary>
    public static void LogCritical(string message, params object[] args) => Log("Critical", message, args);
    /// <summary>
    /// Logs an exception at critical level.
    /// </summary>
    public static void LogCritical(Exception exception, string message, params object[] args) => Log("Critical", $"{message}: {exception}", args);
    /// <summary>
    /// Begins a logging scope.
    /// </summary>
    public static IDisposable BeginScope(string operationName) => new NoopDisposable();
    /// <summary>
    /// Logs a performance metric.
    /// </summary>
    public static void LogPerformanceMetric(string operationName, double elapsedMs, long memoryUsedBytes = 0) => Log("PerformanceMetric", $"{operationName} {elapsedMs}ms {memoryUsedBytes}bytes", Array.Empty<object>());
    /// <summary>
    /// Returns whether a given level is enabled.
    /// </summary>
    public static bool IsEnabled(LogLevel logLevel) => true;

    private static void Log(string level, string message, object[]? args)
    {
        var safeArgs = args ?? Array.Empty<object>();
        LogEntries.Add($"[{level}] {string.Format(message, safeArgs)}");
    }

    private sealed class NoopDisposable : IDisposable
    {
        public void Dispose()
        {
            // no-op
        }
    }
}
