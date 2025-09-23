// <copyright file="MicrosoftLoggerAdapter.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.Logging;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Adapter to bridge IXamlLogger with Microsoft.Extensions.Logging.
/// </summary>
public sealed class MicrosoftLoggerAdapter : IXamlLogger
{
    private readonly ILogger _logger;

    public MicrosoftLoggerAdapter(ILogger logger)
    {
        _logger = logger;
    }

    public void LogTrace(string message, params object[] args) => _logger.LogTrace(message, args);
    public void LogDebug(string message, params object[] args) => _logger.LogDebug(message, args);
    public void LogInformation(string message, params object[] args) => _logger.LogInformation(message, args);
    public void LogWarning(string message, params object[] args) => _logger.LogWarning(message, args);
    public void LogError(string message, params object[] args) => _logger.LogError(message, args);
    public void LogError(Exception exception, string message, params object[] args) => _logger.LogError(exception, message, args);
    public void LogCritical(string message, params object[] args) => _logger.LogCritical(message, args);
    public void LogCritical(Exception exception, string message, params object[] args) => _logger.LogCritical(exception, message, args);
    public IDisposable BeginScope(string operationName) => _logger.BeginScope(operationName);
    public void LogPerformanceMetric(string operationName, double elapsedMs, long memoryUsedBytes = 0)
    {
        _logger.LogInformation("PerformanceMetric: {Operation} {ElapsedMs}ms {MemoryUsedBytes}bytes", operationName, elapsedMs, memoryUsedBytes);
    }
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled((Microsoft.Extensions.Logging.LogLevel)logLevel);
}
