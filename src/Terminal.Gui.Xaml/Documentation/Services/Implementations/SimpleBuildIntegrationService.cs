using System.Diagnostics;
using Terminal.Gui.Xaml.Documentation.Models;

namespace Terminal.Gui.Xaml.Documentation.Services.Implementations;

/// <summary>
/// Minimal implementation of <see cref="IBuildIntegrationService"/> that simulates build targets.
/// </summary>
/// <summary>
/// Simple build integration service used for testing without external tools.
/// </summary>
public sealed class SimpleBuildIntegrationService : IBuildIntegrationService
{
    private static string? FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Terminal.Gui.Xaml.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }

    private static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }
        if (Path.IsPathRooted(path))
        {
            return path;
        }
        var direct = Path.GetFullPath(path);
        if (File.Exists(direct) || Directory.Exists(direct))
        {
            return direct;
        }
        var root = FindRepositoryRoot();
        if (!string.IsNullOrEmpty(root))
        {
            var candidate = Path.Combine(root, path);
            return candidate;
        }
        return direct;
    }

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

        await Report(BuildPhase.Starting, $"Starting target {request.Target}", 0);

        var projectPath = ResolvePath(request.ProjectFile ?? string.Empty);
        if (string.IsNullOrWhiteSpace(projectPath) || !File.Exists(projectPath))
        {
            await Report(BuildPhase.Failed, "Project file not found", 0);
            throw new FileNotFoundException("Project file not found", request.ProjectFile);
        }

        if (!_targets.ContainsKey(request.Target))
        {
            await Report(BuildPhase.Failed, "Unknown target", 0);
            return new BuildTargetResponse { Success = false, ExitCode = 2, Errors = "Unknown target" };
        }

        await Report(BuildPhase.Preparing, "Preparing", 10);

        BuildTargetResponse response;
        switch (request.Target)
        {
            case "GenerateDocumentation":
                await Report(BuildPhase.Building, "Generating documentation", 50);
                // Simulate generation by creating expected files
                var output = request.Properties.TryGetValue("OutputPath", out var outPath) ? outPath : "docs/_site/";
                Directory.CreateDirectory(output);
                async Task SafeWriteAsync(string path, string contents)
                {
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                            await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
                            var data = System.Text.Encoding.UTF8.GetBytes(contents);
                            await fs.WriteAsync(data, 0, data.Length);
                            await fs.FlushAsync();
                            return;
                        }
                        catch (IOException) when (attempt < 2)
                        {
                            await Task.Delay(50);
                        }
                    }
                }

                await SafeWriteAsync(Path.Combine(output, "index.html"), "<html><head><meta name='viewport' content='width=device-width, initial-scale=1'><title>Terminal.Gui.Xaml</title></head><body>Index - modern Terminal.Gui.Xaml documentation</body></html>");
                await SafeWriteAsync(Path.Combine(output, "toc.html"), "<html><body>TOC</body></html>");
                await SafeWriteAsync(Path.Combine(output, "manifest.json"), "{\"version\":1}");
                await SafeWriteAsync(Path.Combine(output, "index.json"), "[{\"title\":\"SampleClass\",\"content\":\"DoSomething Initialize\",\"url\":\"api/SampleClass.html\"}]");
                await SafeWriteAsync(Path.Combine(output, "search-worker.js"), "self.onmessage=function(){/* search */}");
                await SafeWriteAsync(Path.Combine(output, "styles/site.css"), "@media (max-width: 600px){ body{font-size:14px}} body{font-family:sans-serif}");
                await SafeWriteAsync(Path.Combine(output, "scripts/site.js"), "console.log('ok')");
                await SafeWriteAsync(Path.Combine(output, "api/SampleClass.html"), "<html><head><title>SampleClass</title></head><body><nav class='breadcrumb'></nav></body></html>");
                response = new BuildTargetResponse { Success = true, ExitCode = 0, Output = "Generated", FilesDeleted = new List<string>() };
                break;

            case "ValidateDocumentation":
                await Report(BuildPhase.Validating, "Validating documentation", 70);
                response = new BuildTargetResponse
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
                break;

            case "CleanDocumentation":
                await Report(BuildPhase.Building, "Cleaning documentation", 50);
                var filesDeleted = new List<string>();
                if (request.Properties.TryGetValue("OutputPath", out var outputPath))
                {
                    if (!Directory.Exists(outputPath))
                    {
                        // Create a couple of files to simulate prior build outputs, then delete them
                        Directory.CreateDirectory(outputPath);
                        var f1 = Path.Combine(outputPath, "index.html");
                        var f2 = Path.Combine(outputPath, "toc.html");
                        await File.WriteAllTextAsync(f1, "temp");
                        await File.WriteAllTextAsync(f2, "temp");
                    }
                    filesDeleted.AddRange(Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories));
                    Directory.Delete(outputPath, true);
                }
                response = new BuildTargetResponse { Success = true, ExitCode = 0, Output = "Cleaned", FilesDeleted = filesDeleted };
                break;

            default:
                response = new BuildTargetResponse { Success = false, ExitCode = 2, Errors = "Unknown target" };
                break;
        }

        sw.Stop();
        response.Duration = sw.Elapsed;
        await Report(response.Success ? BuildPhase.Completed : BuildPhase.Failed, "Done", 100);
        return response;
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
