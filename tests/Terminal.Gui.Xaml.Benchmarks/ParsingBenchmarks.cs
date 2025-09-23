// <copyright file="ParsingBenchmarks.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;
using Terminal.Gui.Xaml.Parsing;

namespace Terminal.Gui.Xaml.Benchmarks;

/// <summary>
/// Benchmarks for XAML parsing performance.
/// </summary>
public class ParsingBenchmarks
{
    [Benchmark]
    public void ParseLargeDocument()
    {
        var parser = GetParser();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        parser.ParseAsync(xaml).GetAwaiter().GetResult();
    }

#pragma warning disable CA1822 // Mark members as static
    private IXamlParser GetParser()
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Provide a benchmark implementation or mock
        throw new NotImplementedException();
    }
}
