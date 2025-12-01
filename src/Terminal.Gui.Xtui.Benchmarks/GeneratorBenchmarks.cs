using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Benchmarks;

/// <summary>
/// Benchmarks for code generation performance.
/// Tests WindowGenerator and other generators with varying child element counts.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class GeneratorBenchmarks
{
    private ElementNode _windowEmpty = null!;
    private ElementNode _window1Child = null!;
    private ElementNode _window10Children = null!;
    private ElementNode _window50Children = null!;
    private ElementNode _window100Children = null!;
    private WindowGenerator _windowGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _windowGenerator = new WindowGenerator();
        _factory = new GeneratorFactory();

        // Empty window
        _windowEmpty = new ElementNode { ElementTypeName = "Window" };
        _windowEmpty.Attributes["Title"] = "Empty";

        // Window with 1 child
        _window1Child = CreateWindowWithChildren(1);

        // Window with 10 children
        _window10Children = CreateWindowWithChildren(10);

        // Window with 50 children
        _window50Children = CreateWindowWithChildren(50);

        // Window with 100 children
        _window100Children = CreateWindowWithChildren(100);
    }

    [Benchmark(Baseline = true, Description = "Window Empty")]
    public string GenerateWindowEmpty()
    {
        return _windowGenerator.GenerateClass(_windowEmpty, "Benchmark", "EmptyWindow", _factory);
    }

    [Benchmark(Description = "Window 1 Child")]
    public string GenerateWindow1Child()
    {
        return _windowGenerator.GenerateClass(_window1Child, "Benchmark", "Window1", _factory);
    }

    [Benchmark(Description = "Window 10 Children")]
    public string GenerateWindow10Children()
    {
        return _windowGenerator.GenerateClass(_window10Children, "Benchmark", "Window10", _factory);
    }

    [Benchmark(Description = "Window 50 Children")]
    public string GenerateWindow50Children()
    {
        return _windowGenerator.GenerateClass(_window50Children, "Benchmark", "Window50", _factory);
    }

    [Benchmark(Description = "Window 100 Children")]
    public string GenerateWindow100Children()
    {
        return _windowGenerator.GenerateClass(_window100Children, "Benchmark", "Window100", _factory);
    }

    /// <summary>
    /// Creates a Window ElementNode with specified number of Label children.
    /// </summary>
    private static ElementNode CreateWindowWithChildren(int childCount)
    {
        var window = new ElementNode { ElementTypeName = "Window" };
        window.Attributes["Title"] = $"Window with {childCount} children";

        for (int i = 0; i < childCount; i++)
        {
            var label = new ElementNode { ElementTypeName = "Label" };
            label.Attributes["Text"] = $"Label {i}";
            label.Attributes["X"] = (i % 10).ToString();
            label.Attributes["Y"] = (i / 10).ToString();
            label.Attributes["Width"] = "20";
            label.Attributes["Height"] = "1";
            window.Children.Add(label);
        }

        return window;
    }
}

