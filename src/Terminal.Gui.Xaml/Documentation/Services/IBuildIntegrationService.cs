// <copyright file="IBuildIntegrationService.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Defines the contract for build integration services.
/// </summary>
public interface IBuildIntegrationService
{
    /// <summary>
    /// Executes a build target asynchronously.
    /// </summary>
    /// <param name="request">The build target request.</param>
    /// <param name="progressReporter">Optional progress reporter.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the build target response.</returns>
    Task<BuildTargetResponse> ExecuteBuildTargetAsync(BuildTargetRequest request, Func<BuildProgress, Task>? progressReporter = null);
    
    /// <summary>
    /// Gets available build targets.
    /// </summary>
    /// <returns>A collection of available build targets.</returns>
    Task<IEnumerable<BuildTargetDefinition>> GetAvailableTargetsAsync();

    /// <summary>
    /// Gets available build targets synchronously for quick discovery.
    /// </summary>
    /// <returns>Collection of available target names.</returns>
    IEnumerable<string> GetAvailableTargets();

    /// <summary>
    /// Gets property definitions for a specific target.
    /// </summary>
    /// <param name="target">The target name.</param>
    /// <returns>Dictionary of property definitions.</returns>
    IReadOnlyDictionary<string, BuildPropertyDefinition> GetTargetProperties(string target);

    /// <summary>
    /// Registers a custom build target at runtime.
    /// </summary>
    /// <param name="definition">The target definition.</param>
    Task RegisterCustomTargetAsync(BuildTargetDefinition definition);
}