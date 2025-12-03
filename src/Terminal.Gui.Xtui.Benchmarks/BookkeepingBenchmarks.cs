using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Terminal.Gui.Xtui;

namespace Terminal.Gui.Xtui.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class BookkeepingBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Ensure clean state
        GeneratedFileBookkeeping.Clear();
    }

    [Benchmark(Description = "Record 100 distinct generated files")]
    public void RecordManyDistinct()
    {
        for (int i = 0; i < 100; i++)
        {
            string input = $"File{i}.xtui";
            string gen = $"File{i}.g.cs";
            GeneratedFileBookkeeping.Record(input, gen, "Benchmark", $"Class{i}");
        }
    }

    [Benchmark(Description = "Lookup 100 existing entries")]
    public void LookupManyExisting()
    {
        for (int i = 0; i < 100; i++)
        {
            string input = $"File{i}.xtui";
            var info = GeneratedFileBookkeeping.Get(input);
            // Use info to avoid dead code elimination
            if (info != null && info.ClassName.Length == 0)
            {
                throw new System.Exception("unexpected");
            }
        }
    }
}
