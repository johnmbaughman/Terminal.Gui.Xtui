// <copyright file="GenerationBenchmarks.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;
using Terminal.Gui.Xaml.Generation;

namespace Terminal.Gui.Xaml.Benchmarks;

/// <summary>
/// Benchmarks for code generation performance.
/// </summary>
public class GenerationBenchmarks
{
    [Benchmark]
    public void GenerateLargeClass()
    {
        var generator = GetGenerator();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        generator.GenerateAsync(xaml).GetAwaiter().GetResult();
    }

#pragma warning disable CA1822 // Mark members as static
    private ICodeGenerator GetGenerator()
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Provide a benchmark implementation or mock
        throw new NotImplementedException();
    }
}
