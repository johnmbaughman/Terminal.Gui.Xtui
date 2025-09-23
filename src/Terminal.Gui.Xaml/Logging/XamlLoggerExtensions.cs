// <copyright file="XamlLoggerExtensions.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Provides extension methods for <see cref="IXamlLogger"/> to simplify logging operations.
/// </summary>
public static class XamlLoggerExtensions
{
    /// <summary>
    /// Logs the start of a XAML parsing operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="filePath">The XAML file path being parsed.</param>
    /// <param name="caller">The calling method name (auto-filled).</param>
    public static void LogXamlParsingStart(this IXamlLogger logger, string filePath,
        [CallerMemberName] string caller = "")
    {
        logger.LogDebug("[{Caller}] Starting XAML parsing: {FilePath}", caller, filePath);
    }

    /// <summary>
    /// Logs the completion of a XAML parsing operation with performance metrics.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="filePath">The XAML file path that was parsed.</param>
    /// <param name="elapsedMs">The parsing time in milliseconds.</param>
    /// <param name="elementCount">The number of elements parsed.</param>
    /// <param name="caller">The calling method name (auto-filled).</param>
    public static void LogXamlParsingComplete(this IXamlLogger logger, string filePath,
        double elapsedMs, int elementCount, [CallerMemberName] string caller = "")
    {
        logger.LogInformation("[{Caller}] XAML parsing completed: {FilePath} ({ElementCount} elements, {ElapsedMs:F2}ms)",
            caller, filePath, elementCount, elapsedMs);

        // Log performance metric for constitutional compliance monitoring
        logger.LogPerformanceMetric("XamlParsing", elapsedMs);

        // Warn if parsing time exceeds constitutional requirement
        if (elapsedMs > 100)
        {
            logger.LogWarning("XAML parsing time ({ElapsedMs:F2}ms) exceeds constitutional requirement (100ms) for {FilePath}",
                elapsedMs, filePath);
        }
    }

    /// <summary>
    /// Logs the start of a code generation operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="className">The class name being generated.</param>
    /// <param name="caller">The calling method name (auto-filled).</param>
    public static void LogCodeGenerationStart(this IXamlLogger logger, string className,
        [CallerMemberName] string caller = "")
    {
        logger.LogDebug("[{Caller}] Starting code generation: {ClassName}", caller, className);
    }

    /// <summary>
    /// Logs the completion of a code generation operation with performance metrics.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="className">The class name that was generated.</param>
    /// <param name="elapsedMs">The generation time in milliseconds.</param>
    /// <param name="linesOfCode">The number of lines of code generated.</param>
    /// <param name="caller">The calling method name (auto-filled).</param>
    public static void LogCodeGenerationComplete(this IXamlLogger logger, string className,
        double elapsedMs, int linesOfCode, [CallerMemberName] string caller = "")
    {
        logger.LogInformation("[{Caller}] Code generation completed: {ClassName} ({LinesOfCode} LOC, {ElapsedMs:F2}ms)",
            caller, className, linesOfCode, elapsedMs);

        // Log performance metric for constitutional compliance monitoring
        logger.LogPerformanceMetric("CodeGeneration", elapsedMs);

        // Warn if generation time exceeds reasonable limits
        if (elapsedMs > 200)
        {
            logger.LogWarning("Code generation time ({ElapsedMs:F2}ms) exceeds recommended limit (200ms) for {ClassName}",
                elapsedMs, className);
        }
    }

    /// <summary>
    /// Logs a data binding operation.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="sourceProperty">The source property being bound.</param>
    /// <param name="targetProperty">The target property being bound to.</param>
    /// <param name="bindingMode">The binding mode (OneWay, TwoWay, etc.).</param>
    public static void LogDataBinding(this IXamlLogger logger, string sourceProperty,
        string targetProperty, string bindingMode)
    {
        logger.LogTrace("Data binding established: {SourceProperty} -> {TargetProperty} ({BindingMode})",
            sourceProperty, targetProperty, bindingMode);
    }

    /// <summary>
    /// Logs a data binding error.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="bindingExpression">The binding expression that failed.</param>
    /// <param name="exception">The exception that occurred.</param>
    public static void LogDataBindingError(this IXamlLogger logger, string bindingExpression,
        Exception exception)
    {
        logger.LogError(exception, "Data binding failed for expression: {BindingExpression}",
            bindingExpression);
    }

    /// <summary>
    /// Logs memory usage for constitutional compliance monitoring.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="memoryUsedMB">The memory used in megabytes.</param>
    public static void LogMemoryUsage(this IXamlLogger logger, string operationName,
        double memoryUsedMB)
    {
        logger.LogTrace("{OperationName} memory usage: {MemoryUsedMB:F2} MB", operationName, memoryUsedMB);

        // Log performance metric
        logger.LogPerformanceMetric($"{operationName}_Memory", 0, (long)(memoryUsedMB * 1024 * 1024));

        // Warn if memory usage exceeds constitutional requirement
        if (memoryUsedMB > 50)
        {
            logger.LogWarning("{OperationName} memory usage ({MemoryUsedMB:F2} MB) exceeds constitutional requirement (50 MB)",
                operationName, memoryUsedMB);
        }
    }

    /// <summary>
    /// Creates a performance tracking scope that automatically logs elapsed time.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">The name of the operation to track.</param>
    /// <returns>A disposable scope that logs performance on disposal.</returns>
    public static IDisposable BeginPerformanceScope(this IXamlLogger logger, string operationName)
    {
        return new PerformanceScope(logger, operationName);
    }

    /// <summary>
    /// Logs constitutional compliance check results.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="checkName">The name of the constitutional check.</param>
    /// <param name="passed">Whether the check passed.</param>
    /// <param name="details">Additional details about the check.</param>
    public static void LogConstitutionalCheck(this IXamlLogger logger, string checkName,
        bool passed, string? details = null)
    {
        var status = passed ? "PASSED" : "FAILED";
        var message = string.IsNullOrEmpty(details)
            ? "Constitutional check {CheckName}: {Status}"
            : "Constitutional check {CheckName}: {Status} - {Details}";

        if (passed)
        {
            logger.LogInformation(message, checkName, status, details);
        }
        else
        {
            logger.LogError(message, checkName, status, details);
        }
    }

    /// <summary>
    /// A disposable scope for automatic performance tracking.
    /// </summary>
    private sealed class PerformanceScope : IDisposable
    {
        private readonly IXamlLogger _logger;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private readonly long _startMemory;
        private bool _disposed;

        public PerformanceScope(IXamlLogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _startMemory = GC.GetTotalMemory(false);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _stopwatch.Stop();
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsed = Math.Max(0, endMemory - _startMemory);

            _logger.LogPerformanceMetric(_operationName, _stopwatch.Elapsed.TotalMilliseconds, memoryUsed);
            _disposed = true;
        }
    }
}
