using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class StatusBarGeneratorBenchmarks
{
    private ElementNode _statusBarEmpty = null!;
    private ElementNode _statusBarWithProperties = null!;
    private ElementNode _statusBarWithBinding = null!;
    private StatusBarGenerator _generator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup()
    {
        _generator = new StatusBarGenerator();
        _factory = new GeneratorFactory();

        _statusBarEmpty = new ElementNode { ElementTypeName = "StatusBar" };
        _statusBarEmpty.Attributes["Id"] = "statusBar";

        _statusBarWithProperties = new ElementNode { ElementTypeName = "StatusBar" };
        _statusBarWithProperties.Attributes["Id"] = "statusBar";
        _statusBarWithProperties.Attributes["Visible"] = "true";
        _statusBarWithProperties.Attributes["AlignmentModes"] = "IgnoreFirstOrLast";
        _statusBarWithProperties.Attributes["CanFocus"] = "false";

        _statusBarWithBinding = new ElementNode { ElementTypeName = "StatusBar" };
        _statusBarWithBinding.Attributes["Id"] = "statusBar";
        _statusBarWithBinding.Attributes["Visible"] = "{Binding ShowStatusBar}";
        _statusBarWithBinding.Attributes["AlignmentModes"] = "IgnoreFirstOrLast";
        _statusBarWithBinding.Attributes["Height"] = "{Dim Auto={style=Auto;minimumContentDim={Dim Func={_ => statusBar.Visible ? 1 : 0}};maximumContentDim={Dim func={_ => statusBar.Visible ? 1 : 0}}}}";
    }

    [Benchmark(Baseline = true, Description = "StatusBar Empty")]
    public string GenerateStatusBarEmpty()
    {
        return _generator.GenerateClass(_statusBarEmpty, "Benchmark", "EmptyStatusBar", _factory);
    }

    [Benchmark(Description = "StatusBar With Properties")]
    public string GenerateStatusBarWithProperties()
    {
        return _generator.GenerateClass(_statusBarWithProperties, "Benchmark", "StatusBarProps", _factory);
    }

    [Benchmark(Description = "StatusBar With Binding")]
    public string GenerateStatusBarWithBinding()
    {
        return _generator.GenerateClass(_statusBarWithBinding, "Benchmark", "StatusBarBinding", _factory);
    }

    [Benchmark(Description = "StatusBar Statements")]
    public StatementSyntax[] GenerateStatusBarStatements()
    {
        return _generator.GenerateStatements(_statusBarWithProperties, "statusBar", _factory);
    }
}

