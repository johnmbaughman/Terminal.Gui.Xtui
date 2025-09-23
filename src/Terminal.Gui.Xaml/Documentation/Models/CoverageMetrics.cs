// <copyright file="CoverageMetrics.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Models;

/// <summary>
/// Represents documentation coverage statistics.
/// </summary>
public class CoverageMetrics
{
    /// <summary>
    /// Gets or sets the total number of members.
    /// </summary>
    public int TotalMembers { get; set; }

    /// <summary>
    /// Gets or sets the number of documented members.
    /// </summary>
    public int DocumentedMembers { get; set; }

    /// <summary>
    /// Gets or sets the number of undocumented members.
    /// </summary>
    public int UndocumentedMembers { get; set; }

    /// <summary>
    /// Gets the documentation coverage percentage.
    /// </summary>
    public double CoveragePercentage => TotalMembers == 0 ? 0 : (double)DocumentedMembers / TotalMembers * 100.0;
}
