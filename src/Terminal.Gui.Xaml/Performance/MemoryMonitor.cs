// <copyright file="MemoryMonitor.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace Terminal.Gui.Xaml.Performance;

/// <summary>
/// Monitors memory usage for constitutional compliance.
/// </summary>
public static class MemoryMonitor
{
    /// <summary>
    /// Gets the current process memory usage in megabytes.
    /// </summary>
    public static double CurrentMemoryUsageMB
    {
        get
        {
            using var proc = Process.GetCurrentProcess();
            return proc.WorkingSet64 / (1024.0 * 1024.0);
        }
    }

    /// <summary>
    /// Checks if current memory usage exceeds the constitutional limit (50MB).
    /// </summary>
    public static bool IsMemoryUsageCompliant() => CurrentMemoryUsageMB <= 50.0;
}
