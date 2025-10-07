// <copyright file="IDocumentationGeneratorService.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Defines the contract for documentation generation services.
/// </summary>
public interface IDocumentationGeneratorService
{
    /// <summary>
    /// Generates documentation asynchronously.
    /// </summary>
    /// <param name="request">The generate documentation request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generation response.</returns>
    Task<GenerateDocumentationResponse> GenerateDocumentationAsync(GenerateDocumentationRequest request);

    /// <summary>
    /// Validates documentation asynchronously.
    /// </summary>
    /// <param name="request">The validate documentation request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the validation response.</returns>
    Task<ValidateDocumentationResponse> ValidateDocumentationAsync(ValidateDocumentationRequest request);
}