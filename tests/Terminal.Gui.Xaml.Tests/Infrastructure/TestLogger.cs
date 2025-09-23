// <copyright file="TestLogger.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Logging;

namespace Terminal.Gui.Xaml.Tests.Infrastructure;

/// <summary>
/// Test logger implementation for capturing log output in tests.
/// </summary>
public static class TestLogger
{
    public static List<string> LogEntries { get; } = new();

    public static void LogTrace(string message, params object[] args) => Log("Trace", message, args);
    public static void LogDebug(string message, params object[] args) => Log("Debug", message, args);
    public static void LogInformation(string message, params object[] args) => Log("Information", message, args);
    public static void LogWarning(string message, params object[] args) => Log("Warning", message, args);
    public static void LogError(string message, params object[] args) => Log("Error", message, args);
    public static void LogError(Exception exception, string message, params object[] args) => Log("Error", $"{message}: {exception}", args);
    public static void LogCritical(string message, params object[] args) => Log("Critical", message, args);
    public static void LogCritical(Exception exception, string message, params object[] args) => Log("Critical", $"{message}: {exception}", args);
    public static IDisposable BeginScope(string operationName) => null;
    public static void LogPerformanceMetric(string operationName, double elapsedMs, long memoryUsedBytes = 0) => Log("PerformanceMetric", $"{operationName} {elapsedMs}ms {memoryUsedBytes}bytes");
    public static bool IsEnabled(LogLevel logLevel) => true;

    private static void Log(string level, string message, object[] args)
    {
        LogEntries.Add($"[{level}] {string.Format(message, args)}");
    }
}
