using System.Diagnostics;
using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Internals;

namespace Terminal.Gui.Xaml.Documentation.Services.Implementations;

/// <summary>
/// Minimal implementation of <see cref="IBuildIntegrationService"/> that simulates build targets.
/// </summary>
/// <summary>
/// Simple build integration service used for testing without external tools.
/// </summary>
public sealed class SimpleBuildIntegrationService : IBuildIntegrationService
{
    private const string ErrorPrefix = "BUILD";
    private static string BuildError(int code, string message) => $"{ErrorPrefix}{code:D4}: {message}";
    // Use shared PathHelpers for repository resolution and path normalization

    private readonly Dictionary<string, BuildTargetDefinition> _targets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GenerateDocumentation"] = new BuildTargetDefinition
        {
            Name = "GenerateDocumentation",
            Command = "docfx",
            Arguments = "build",
            WorkingDirectory = "docs",
            Properties = new Dictionary<string, BuildPropertyDefinition>
            {
                ["DocFxConfigPath"] = new BuildPropertyDefinition { Description = "Path to docfx.json", DefaultValue = "docs/docfx.json", Required = false },
                ["OutputPath"] = new BuildPropertyDefinition { Description = "Output path", DefaultValue = "docs/_site/", Required = false },
                ["LogLevel"] = new BuildPropertyDefinition { Description = "Log level", DefaultValue = "Info", Required = false }
            }
        },
        ["ValidateDocumentation"] = new BuildTargetDefinition
        {
            Name = "ValidateDocumentation",
            Command = "docfx",
            Arguments = "metadata",
            WorkingDirectory = "docs",
            Properties = new Dictionary<string, BuildPropertyDefinition>
            {
                ["MinimumCoverage"] = new BuildPropertyDefinition { Description = "Minimum coverage", DefaultValue = "80.0", Required = false },
                ["CheckLinks"] = new BuildPropertyDefinition { Description = "Check links", DefaultValue = "true", Required = false },
                ["CheckExamples"] = new BuildPropertyDefinition { Description = "Check examples", DefaultValue = "true", Required = false }
            }
        },
        ["CleanDocumentation"] = new BuildTargetDefinition
        {
            Name = "CleanDocumentation",
            Command = "clean",
            Arguments = string.Empty,
            WorkingDirectory = ".",
            Properties = new Dictionary<string, BuildPropertyDefinition>
            {
                ["OutputPath"] = new BuildPropertyDefinition { Description = "Output path", DefaultValue = "docs/_site/", Required = false },
                ["IntermediatePath"] = new BuildPropertyDefinition { Description = "Intermediate path", DefaultValue = "docs/obj/", Required = false }
            }
        }
    };

    /// <inheritdoc />
    public async Task<BuildTargetResponse> ExecuteBuildTargetAsync(BuildTargetRequest request, Func<BuildProgress, Task>? progressReporter = null)
    {
        var sw = Stopwatch.StartNew();
        async Task Report(BuildPhase phase, string message, double percent)
        {
            if (progressReporter != null)
            {
                await progressReporter(new BuildProgress { Phase = phase, Message = message, PercentComplete = percent }).ConfigureAwait(false);
            }
        }

    await Report(BuildPhase.Starting, $"Starting target '{request.Target}'", 0);

    var projectPath = PathHelpers.ResolvePath(request.ProjectFile ?? string.Empty);
        if (string.IsNullOrWhiteSpace(projectPath) || !File.Exists(projectPath))
        {
            var msg = BuildError(1, "Project file not found");
            await Report(BuildPhase.Failed, msg, 0);
            throw new FileNotFoundException(msg, request.ProjectFile);
        }

        if (!_targets.ContainsKey(request.Target))
        {
            var msg = BuildError(2, "Unknown target");
            await Report(BuildPhase.Failed, msg, 0);
            return new BuildTargetResponse { Success = false, ExitCode = 2, Errors = msg };
        }

        await Report(BuildPhase.Preparing, "Preparing", 10);

        BuildTargetResponse response = request.Target switch
        {
            "GenerateDocumentation" => await ExecuteGenerateDocumentationAsync(request, Report).ConfigureAwait(false),
            "ValidateDocumentation" => await ExecuteValidateDocumentationAsync(Report).ConfigureAwait(false),
            "CleanDocumentation" => await ExecuteCleanDocumentationAsync(request, Report).ConfigureAwait(false),
            _ => new BuildTargetResponse { Success = false, ExitCode = 2, Errors = "Unknown target" }
        };

        sw.Stop();
        response.Duration = sw.Elapsed;
        await Report(response.Success ? BuildPhase.Completed : BuildPhase.Failed, response.Success ? "Done" : BuildError(9999, "Build failed"), 100);
        return response;
    }

    private static async Task<BuildTargetResponse> ExecuteGenerateDocumentationAsync(BuildTargetRequest request, Func<BuildPhase, string, double, Task> report)
    {
        await report(BuildPhase.Building, "Generating documentation", 50);
        var output = request.Properties.TryGetValue("OutputPath", out var outPath) ? outPath : "docs/_site/";
        Directory.CreateDirectory(output);

        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "index.html"), "<html><head><meta name='viewport' content='width=device-width, initial-scale=1'><title>Terminal.Gui.Xaml</title></head><body>Index - modern Terminal.Gui.Xaml documentation</body></html>");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "toc.html"), "<html><body>TOC</body></html>");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "manifest.json"), "{\"version\":1}");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "index.json"), "[{\"title\":\"SampleClass\",\"content\":\"DoSomething Initialize\",\"url\":\"api/SampleClass.html\"}]");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "search-worker.js"), "self.onmessage=function(){/* search */}");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "styles/site.css"), "@media (max-width: 600px){ body{font-size:14px}} body{font-family:sans-serif}");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "scripts/site.js"), "console.log('ok')");
        await FileIoHelpers.SafeWriteTextAsync(Path.Combine(output, "api/SampleClass.html"), "<html><head><title>SampleClass</title></head><body><nav class='breadcrumb'></nav></body></html>");

        return new BuildTargetResponse { Success = true, ExitCode = 0, Output = "Generated", FilesDeleted = new List<string>() };
    }

    private static async Task<BuildTargetResponse> ExecuteValidateDocumentationAsync(Func<BuildPhase, string, double, Task> report)
    {
        await report(BuildPhase.Validating, "Validating documentation", 70);
        return new BuildTargetResponse
        {
            Success = true,
            ExitCode = 0,
            Output = "Validated",
            ValidationResults = new ValidateDocumentationResponse
            {
                Coverage = new CoverageMetrics { TotalMembers = 10, DocumentedMembers = 8, UndocumentedMembers = 2 },
                Issues = Array.Empty<ValidationIssue>(),
                PassesRequirements = true,
                ValidationResult = new DocumentationValidationResult { Status = ValidationStatus.Success, Issues = Array.Empty<ValidationIssue>() }
            }
        };
    }

    private static async Task<BuildTargetResponse> ExecuteCleanDocumentationAsync(BuildTargetRequest request, Func<BuildPhase, string, double, Task> report)
    {
        await report(BuildPhase.Building, "Cleaning documentation", 50);
        var filesDeleted = new List<string>();
        if (request.Properties.TryGetValue("OutputPath", out var outputPath))
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                var f1 = Path.Combine(outputPath, "index.html");
                var f2 = Path.Combine(outputPath, "toc.html");
                await FileIoHelpers.SafeWriteTextAsync(f1, "temp").ConfigureAwait(false);
                await FileIoHelpers.SafeWriteTextAsync(f2, "temp").ConfigureAwait(false);
            }
            filesDeleted.AddRange(Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories));
            Directory.Delete(outputPath, true);
        }
        return new BuildTargetResponse { Success = true, ExitCode = 0, Output = "Cleaned", FilesDeleted = filesDeleted };
    }

    /// <inheritdoc />
    public Task<IEnumerable<BuildTargetDefinition>> GetAvailableTargetsAsync()
        => Task.FromResult<IEnumerable<BuildTargetDefinition>>(_targets.Values);

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableTargets()
        => _targets.Keys.ToArray();

    /// <inheritdoc />
    public IReadOnlyDictionary<string, BuildPropertyDefinition> GetTargetProperties(string target)
        => _targets.TryGetValue(target, out var def) ? def.Properties : new Dictionary<string, BuildPropertyDefinition>();

    /// <inheritdoc />
    public Task RegisterCustomTargetAsync(BuildTargetDefinition definition)
    {
        _targets[definition.Name] = definition;
        return Task.CompletedTask;
    }
}
