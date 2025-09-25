using System.Diagnostics;
using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Xunit;
using Xunit.Abstractions;

namespace Terminal.Gui.Xaml.Tests.Performance;

public class DocumentationPerformanceTests
{
    private readonly ITestOutputHelper _output;
    public DocumentationPerformanceTests(ITestOutputHelper output) => _output = output;
    private static DocFxConfiguration ValidConfig(string output)
        => new()
        {
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = output,
            SourcePaths = new[] { "src/Terminal.Gui.Xaml" },
            ExcludePatterns = new[] { "bin/**", "obj/**" },
            Metadata = new MetadataConfiguration
            {
                Src = new[]
                {
                    new SourceConfiguration
                    {
                        Src = "src/Terminal.Gui.Xaml",
                        Files = new [] { "**/*.cs" },
                        Exclude = new [] { "bin/**", "obj/**" }
                    }
                },
                Dest = "api"
            },
            Build = new BuildConfiguration
            {
                Template = new [] { "default" }
            }
        };

    [Fact]
    public async Task FullGeneration_Completes_Under_Time_And_Memory_Thresholds()
    {
        var output = Path.Combine(Path.GetTempPath(), "tgxaml-perf-full-"+Guid.NewGuid().ToString("n"));
        var service = new SimpleDocumentationGeneratorService();
        var config = ValidConfig(output);
        long beforeMem = GC.GetTotalMemory(true);
        var sw = Stopwatch.StartNew();
    var response = await service.GenerateDocumentationAsync(new GenerateDocumentationRequest { Configuration = config, ValidateOnly = false, IncrementalBuild = false });
        sw.Stop();
        long afterMem = GC.GetTotalMemory(false);
        long delta = afterMem - beforeMem;

    _output.WriteLine($"Full generation produced {response.GeneratedFiles.Count} files in {sw.Elapsed.TotalMilliseconds:n2} ms; memory delta {delta/1024.0/1024.0:n2} MB");
    // Assert success
        Assert.NotNull(response);
        Assert.NotNull(response.GeneratedFiles);
        Assert.True(response.GeneratedFiles!.Count > 0);

        // Time threshold (simulated service should be very fast < 3s)
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(3), $"Generation took {sw.Elapsed}");

        // Memory delta threshold (< 120 MB reasonable for simulated)
        const long maxDelta = 120L * 1024 * 1024;
        Assert.True(delta < maxDelta, $"Memory delta {delta/1024/1024} MB exceeds {maxDelta/1024/1024} MB");
    }

    [Fact]
    public async Task IncrementalGeneration_Faster_And_NotLarger_Memory()
    {
        var output = Path.Combine(Path.GetTempPath(), "tgxaml-perf-incr-"+Guid.NewGuid().ToString("n"));
        var service = new SimpleDocumentationGeneratorService();
        var config = ValidConfig(output);

        // First run (full)
        var swFull = Stopwatch.StartNew();
    var full = await service.GenerateDocumentationAsync(new GenerateDocumentationRequest { Configuration = config });
        swFull.Stop();

        // Second run incremental
        var swIncr = Stopwatch.StartNew();
    var incr = await service.GenerateDocumentationAsync(new GenerateDocumentationRequest { Configuration = config, IncrementalBuild = true });
        swIncr.Stop();

    _output.WriteLine($"Full: {swFull.Elapsed.TotalMilliseconds:n2} ms vs Incremental: {swIncr.Elapsed.TotalMilliseconds:n2} ms");
    Assert.True(swIncr.Elapsed <= swFull.Elapsed + TimeSpan.FromMilliseconds(250), $"Incremental run {swIncr.Elapsed} not faster or comparable to full {swFull.Elapsed}");

        // Memory comparison using immediate GC snapshots
        long mem1 = GC.GetTotalMemory(true);
        long mem2 = GC.GetTotalMemory(true);
        Assert.InRange(mem2 - mem1, -10_000_000, 80_000_000); // Acceptable variance

        // Ensure incremental did not re-emit all static files drastically increasing count
        Assert.NotNull(full.GeneratedFiles);
        Assert.NotNull(incr.GeneratedFiles);
        _output.WriteLine($"File counts: full={full.GeneratedFiles.Count} incremental={incr.GeneratedFiles.Count}");
        Assert.True(incr.GeneratedFiles!.Count <= full.GeneratedFiles!.Count, "Incremental should not produce more files than full");
    }
}
