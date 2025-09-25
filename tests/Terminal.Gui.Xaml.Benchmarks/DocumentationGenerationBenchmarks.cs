using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.Json;
using Terminal.Gui.Xaml.Documentation.Models;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;

namespace Terminal.Gui.Xaml.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount:1, iterationCount:5)]
public class DocumentationGenerationBenchmarks
{
    private SimpleDocumentationGeneratorService _service = null!;
    private DocFxConfiguration _config = null!;
    private string _outputFull = null!;

    [GlobalSetup]
    public void Setup()
    {
        _service = new SimpleDocumentationGeneratorService();
        _outputFull = Path.Combine(Path.GetTempPath(), "tgxaml-bench-"+Guid.NewGuid().ToString("n"));
        _config = new DocFxConfiguration
        {
            ProjectName = "Terminal.Gui.Xaml",
            Version = "1.0.0",
            OutputPath = _outputFull,
            SourcePaths = new[]{"src/Terminal.Gui.Xaml"},
            Metadata = new MetadataConfiguration
            {
                Src = new[]{ new SourceConfiguration{ Src = "src/Terminal.Gui.Xaml", Files = new[]{"**/*.cs"} } },
                Dest = "api"
            },
            Build = new BuildConfiguration{ Template = new[]{"default"} }
        };
        // Prime a full generation so incremental benchmark has baseline
        _service.GenerateDocumentationAsync(new GenerateDocumentationRequest{ Configuration = _config }).GetAwaiter().GetResult();
    }

    [Benchmark(Description="Full Generation")]
    public object FullGeneration()
    {
        var response = _service.GenerateDocumentationAsync(new GenerateDocumentationRequest{ Configuration = _config, IncrementalBuild = false }).GetAwaiter().GetResult();
        return response.GeneratedFiles.Count;
    }

    [Benchmark(Description="Incremental Generation")]
    public object IncrementalGeneration()
    {
        var response = _service.GenerateDocumentationAsync(new GenerateDocumentationRequest{ Configuration = _config, IncrementalBuild = true }).GetAwaiter().GetResult();
        return response.GeneratedFiles.Count;
    }
}

public static class Program
{
    private const string BenchDir = ".benchmarks"; // repo-relative (will resolve under current working directory)
    private const string LastResultsFile = "last-results.json";
    private const string HistoryFile = "history.csv";

    public static void Main(string[] args)
    {
        Directory.CreateDirectory(BenchDir);
        var prevPath = Path.Combine(BenchDir, LastResultsFile);
        Dictionary<string,double>? previousMeans = null;
        if (File.Exists(prevPath))
        {
            try
            {
                previousMeans = JsonSerializer.Deserialize<Dictionary<string,double>>(File.ReadAllText(prevPath));
            }
            catch { /* ignore corrupt */ }
        }

    var summary = BenchmarkRunner.Run<DocumentationGenerationBenchmarks>();
        // Extract mean (ns) per benchmark
        var currentMeans = new Dictionary<string,double>();
        foreach (var report in summary.Reports)
        {
            if (report.ResultStatistics != null)
            {
                currentMeans[report.BenchmarkCase.Descriptor.WorkloadMethodDisplayInfo] = report.ResultStatistics.Mean;
            }
        }

        File.WriteAllText(prevPath, JsonSerializer.Serialize(currentMeans, new JsonSerializerOptions{ WriteIndented = true }));

        // Append history CSV (timestamp, commit(optional), benchmark, meanNs)
        var historyPath = Path.Combine(BenchDir, HistoryFile);
        var timestamp = DateTime.UtcNow.ToString("o");
        var commit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? string.Empty;
        var headerNeeded = !File.Exists(historyPath);
        using (var writer = new StreamWriter(historyPath, append: true))
        {
            if (headerNeeded)
            {
                writer.WriteLine("timestamp,commit,benchmark,mean_ns");
            }
            foreach (var kvp in currentMeans)
            {
                writer.WriteLine($"{timestamp},{commit},{Escape(kvp.Key)},{kvp.Value:0.##}");
            }
        }

        // Optional regression detection (>20% slower). Use env var TGXAML_FAIL_ON_BENCH_REGRESSION=1 to fail with non-zero code.
        if (previousMeans != null && previousMeans.Count > 0)
        {
            var regressions = new List<string>();
            foreach (var kvp in currentMeans)
            {
                if (previousMeans.TryGetValue(kvp.Key, out var prev))
                {
                    if (prev > 0)
                    {
                        var ratio = kvp.Value / prev;
                        if (ratio > 1.20)
                        {
                            regressions.Add($"{kvp.Key}: mean {kvp.Value:n0} ns vs previous {prev:n0} ns ({(ratio-1)*100:n1}% slower)");
                        }
                    }
                }
            }
            if (regressions.Count > 0)
            {
                Console.WriteLine("PERFORMANCE REGRESSIONS DETECTED (>20% slower):");
                foreach (var r in regressions)
                {
                    Console.WriteLine(" - " + r);
                }
                if (Environment.GetEnvironmentVariable("TGXAML_FAIL_ON_BENCH_REGRESSION") == "1")
                {
                    Environment.ExitCode = 2; // signal regression to CI
                }
            }
        }
        static string Escape(string value) => value.Contains(',') ? '"'+value.Replace("\"","\"\"")+'"' : value;
    }
}
