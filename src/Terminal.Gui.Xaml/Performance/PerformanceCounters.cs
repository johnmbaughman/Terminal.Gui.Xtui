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
    public static double MeasureElapsedMs(Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return sw.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Measures the memory usage of an operation in bytes.
    /// </summary>
    public static long MeasureMemoryUsage(Action action)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var before = GC.GetTotalMemory(true);
        action();
        var after = GC.GetTotalMemory(true);
        return Math.Max(0, after - before);
    }
}
