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
            using Process proc = Process.GetCurrentProcess ();
            return proc.WorkingSet64 / (1024.0 * 1024.0);
        }
    }

    /// <summary>
    /// Gets the current memory usage in bytes for the process.
    /// </summary>
    /// <returns>The total memory usage in bytes for the current process.</returns>
    public static long GetCurrentMemoryUsage ()
    {
        return GC.GetTotalMemory (false);
    }

    /// <summary>
    /// Checks if current memory usage is within the constitutional limit (50MB).
    /// </summary>
    /// <returns>True if memory usage is compliant; otherwise, false.</returns>
    public static bool IsMemoryUsageCompliant () => CurrentMemoryUsageMB <= 50.0;
}
