// <copyright file="DocumentationGeneratorService.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Utilities;

namespace Terminal.Gui.Xaml.Documentation.Services;

/// <summary>
/// Implements documentation generation and validation logic.
/// </summary>
public class DocumentationGeneratorService : IDocumentationGeneratorService
{
    private readonly IDocumentationGeneratorService? _innerGenerator;

    /// <summary>
    /// Creates a new instance of <see cref="DocumentationGeneratorService"/>.
    /// If no inner generator is provided, a test-friendly <see cref="Implementations.SimpleDocumentationGeneratorService"/>
    /// will be used so library consumers and unit tests can run without DocFX installed.
    /// </summary>
    /// <param name="innerGenerator">Optional inner generator implementation to delegate to.</param>
    public DocumentationGeneratorService(IDocumentationGeneratorService? innerGenerator = null)
    {
        _innerGenerator = innerGenerator ?? new Implementations.SimpleDocumentationGeneratorService();
    }
#pragma warning disable CA1822 // Mark members as static
    public async Task<GenerateDocumentationResponse> GenerateDocumentationAsync(GenerateDocumentationRequest request)
#pragma warning restore CA1822 // Mark members as static
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Delegate to an inner generator implementation. This keeps the high-level service
        // thin and makes it easy to swap in a DocFX-based implementation later.
        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await _innerGenerator!.GenerateDocumentationAsync(request).ConfigureAwait(false);
            sw.Stop();
            result.GenerationTime = result.GenerationTime == default ? sw.Elapsed : result.GenerationTime;
            return result;
        }
        catch (OperationCanceledException)
        {
            return new GenerateDocumentationResponse
            {
                Success = false,
                Errors = new[] { $"{DocumentationUtilities.DocsPrefix} Operation cancelled during documentation generation." }
            };
        }
        catch (Exception ex)
        {
            return new GenerateDocumentationResponse
            {
                Success = false,
                Errors = new[] { $"{DocumentationUtilities.DocsPrefix} Unhandled exception during generation: {ex.GetType().Name}: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Validates documentation for the provided source paths and returns validation results
    /// including issues and coverage metrics. This method currently returns a stubbed
    /// <see cref="ValidateDocumentationResponse"/> intended for TDD until the real implementation
    /// is provided.
    /// </summary>
    /// <param name="request">The validation request containing source paths and rules.</param>
    /// <returns>A <see cref="ValidateDocumentationResponse"/> containing validation details.</returns>
    public async Task<ValidateDocumentationResponse> ValidateDocumentationAsync(ValidateDocumentationRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Delegate to the inner generator's validation if available. The simple generator
        // provides a stable, testable validation implementation so we reuse that logic here.
        try
        {
            var validationRequest = new Implementations.SimpleDocumentationGeneratorService.ValidateDocumentationRequestWrapper(request);
            var validateResponse = await _innerGenerator!.ValidateDocumentationAsync(request).ConfigureAwait(false);
            // Ensure Coverage populated
            validateResponse.Coverage ??= new CoverageMetrics();
            validateResponse.PassesRequirements = validateResponse.Coverage.CoveragePercentage >= request.MinimumCoverage && (validateResponse.ValidationResult?.Status ?? ValidationStatus.Success) != ValidationStatus.Error;
            return validateResponse;
        }
        catch (OperationCanceledException)
        {
            return new ValidateDocumentationResponse
            {
                ValidationResult = new DocumentationValidationResult
                {
                    Status = ValidationStatus.Error,
                    Issues = new[] { new ValidationIssue { IssueType = IssueType.MissingDocumentation, Severity = IssueSeverity.Error, Message = $"{DocumentationUtilities.DocsPrefix} Validation cancelled." } },
                    CoverageMetrics = new CoverageMetrics()
                },
                Coverage = new CoverageMetrics(),
                Issues = new[] { new ValidationIssue { IssueType = IssueType.MissingDocumentation, Severity = IssueSeverity.Error, Message = $"{DocumentationUtilities.DocsPrefix} Validation cancelled." } },
                PassesRequirements = false
            };
        }
        catch (Exception ex)
        {
            return new ValidateDocumentationResponse
            {
                ValidationResult = new DocumentationValidationResult
                {
                    Status = ValidationStatus.Error,
                    Issues = new[] { new ValidationIssue { IssueType = IssueType.MalformedXml, Severity = IssueSeverity.Error, Message = $"{DocumentationUtilities.DocsPrefix} Unhandled validation exception: {ex.Message}" } },
                    CoverageMetrics = new CoverageMetrics()
                },
                Coverage = new CoverageMetrics(),
                Issues = new[] { new ValidationIssue { IssueType = IssueType.MalformedXml, Severity = IssueSeverity.Error, Message = $"{DocumentationUtilities.DocsPrefix} Unhandled validation exception: {ex.Message}" } },
                PassesRequirements = false
            };
        }
    }
}
