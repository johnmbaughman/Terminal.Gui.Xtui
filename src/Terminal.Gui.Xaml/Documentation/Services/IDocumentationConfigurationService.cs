// <copyright file="IDocumentationConfigurationService.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Defines the contract for documentation configuration services.
/// </summary>
public interface IDocumentationConfigurationService
{
    /// <summary>
    /// Loads documentation configuration asynchronously.
    /// </summary>
    /// <param name="configPath">The configuration file path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the DocFx configuration.</returns>
    Task<DocFxConfiguration> LoadConfigurationAsync(string configPath);
    
    /// <summary>
    /// Saves documentation configuration asynchronously.
    /// </summary>
    /// <param name="configuration">The DocFx configuration to save.</param>
    /// <param name="configPath">The configuration file path.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveConfigurationAsync(DocFxConfiguration configuration, string configPath);
    
    /// <summary>
    /// Validates documentation configuration.
    /// </summary>
    /// <param name="configuration">The DocFx configuration to validate.</param>
    /// <returns>A validation result.</returns>
    DocumentationValidationResult ValidateConfiguration(DocFxConfiguration configuration);

    /// <summary>
    /// Creates a default DocFX configuration from a project path.
    /// </summary>
    /// <param name="projectPath">The project path to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the default configuration.</returns>
    Task<DocFxConfiguration> CreateDefaultConfigurationAsync(string projectPath);

    /// <summary>
    /// Validates a configuration asynchronously with additional checks like path existence.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a validation result.</returns>
    Task<ConfigurationValidationResult> ValidateConfigurationAsync(DocFxConfiguration configuration);

    /// <summary>
    /// Gets the list of supported templates.
    /// </summary>
    /// <returns>A list of template names.</returns>
    IReadOnlyList<string> GetSupportedTemplates();

    /// <summary>
    /// Gets default global metadata key/value pairs.
    /// </summary>
    /// <returns>A dictionary of default metadata items.</returns>
    IReadOnlyDictionary<string, object> GetDefaultGlobalMetadata();
}