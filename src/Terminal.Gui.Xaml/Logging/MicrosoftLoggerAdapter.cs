// <copyright file="MicrosoftLoggerAdapter.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.Logging;

namespace Terminal.Gui.Xaml.Logging;

/// <summary>
/// Adapter to bridge IXamlLogger with Microsoft.Extensions.Logging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MicrosoftLoggerAdapter"/> class.
/// </remarks>
/// <param name="logger">The Microsoft logger instance.</param>
public sealed class MicrosoftLoggerAdapter (ILogger logger) : IXamlLogger
{
    private readonly ILogger _logger = logger;

    /// <inheritdoc/>
    public void LogTrace (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogTrace ("{Message}", message);
        }
        else
        {
            _logger.LogTrace ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogDebug (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogDebug ("{Message}", message);
        }
        else
        {
            _logger.LogDebug ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogInformation (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogInformation ("{Message}", message);
        }
        else
        {
            _logger.LogInformation ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogWarning (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogWarning ("{Message}", message);
        }
        else
        {
            _logger.LogWarning ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogError (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogError ("{Message}", message);
        }
        else
        {
            _logger.LogError ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogError (Exception exception, string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogError (exception, "{Message}", message);
        }
        else
        {
            _logger.LogError (exception, "{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogCritical (string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogCritical ("{Message}", message);
        }
        else
        {
            _logger.LogCritical ("{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public void LogCritical (Exception exception, string message, params object [] args)
    {
        if (args.Length == 0)
        {
            _logger.LogCritical (exception, "{Message}", message);
        }
        else
        {
            _logger.LogCritical (exception, "{Message} | Args: {Args}", message, args);
        }
    }

    /// <inheritdoc/>
    public IDisposable BeginScope (string operationName)
    {
        // Microsoft.Extensions.Logging.ILogger.BeginScope may return null, but IDisposable is not nullable in the interface.
        // Suppress possible null reference warning as per interface contract.
        return _logger.BeginScope (operationName)!;
    }

    /// <inheritdoc/>
    public void LogPerformanceMetric (string operationName, double elapsedMs, long memoryUsedBytes = 0)
    {
        _logger.LogInformation ("PerformanceMetric: {Operation} {ElapsedMs}ms {MemoryUsedBytes}bytes", operationName, elapsedMs, memoryUsedBytes);
    }

    /// <inheritdoc/>
    public bool IsEnabled (LogLevel logLevel)
    {
        return _logger.IsEnabled ((Microsoft.Extensions.Logging.LogLevel)logLevel);
    }
}
