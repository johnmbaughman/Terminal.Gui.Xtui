// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Production-facing implementation of <see cref="IDocumentationConfigurationService"/>.
/// Wraps the tested <see cref="SimpleDocumentationConfigurationService"/> to allow
/// future extension (caching, telemetry, advanced validation) without changing
/// contract surface or test expectations.
/// </summary>
public sealed class DocumentationConfigurationService : IDocumentationConfigurationService
{
    private readonly SimpleDocumentationConfigurationService _inner = new();

    /// <inheritdoc />
    public Task<DocFxConfiguration> LoadConfigurationAsync(string configPath) => _inner.LoadConfigurationAsync(configPath);

    /// <inheritdoc />
    public Task SaveConfigurationAsync(DocFxConfiguration configuration, string configPath) => _inner.SaveConfigurationAsync(configuration, configPath);

    /// <inheritdoc />
    public DocumentationValidationResult ValidateConfiguration(DocFxConfiguration configuration) => _inner.ValidateConfiguration(configuration);

    /// <inheritdoc />
    public Task<DocFxConfiguration> CreateDefaultConfigurationAsync(string projectPath) => _inner.CreateDefaultConfigurationAsync(projectPath);

    /// <inheritdoc />
    public Task<ConfigurationValidationResult> ValidateConfigurationAsync(DocFxConfiguration configuration) => _inner.ValidateConfigurationAsync(configuration);

    /// <inheritdoc />
    public IReadOnlyList<string> GetSupportedTemplates() => _inner.GetSupportedTemplates();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> GetDefaultGlobalMetadata() => _inner.GetDefaultGlobalMetadata();
}
