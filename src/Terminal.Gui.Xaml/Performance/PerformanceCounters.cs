// <copyright file="PerformanceCounters.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace Terminal.Gui.Xaml.Performance;

/// <summary>
/// Provides performance counters for constitutional compliance monitoring.
/// </summary>
public static class PerformanceCounters
{
    /// <summary>
    /// Measures the elapsed time of an operation in milliseconds.
    /// </summary>
    /// <summary>
    /// Measures the elapsed time in milliseconds for the specified action.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <returns>The elapsed time in milliseconds.</returns>
    public static double MeasureElapsedMs (Action action)
    {
        Stopwatch sw = Stopwatch.StartNew ();
        action ();
        sw.Stop ();
        return sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Calculates the difference in memory usage before and after executing the specified action, in bytes.
    /// </summary>
    /// <summary>
    /// Measures the memory usage of an operation in bytes.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <returns>The memory usage in bytes.</returns>
    public static long MeasureMemoryUsage (Action action)
    {
        long before = GC.GetTotalMemory (true);
        action ();
        long after = GC.GetTotalMemory (true);
        return Math.Max (0, after - before);
    }
}
