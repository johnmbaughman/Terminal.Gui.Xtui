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
    private ElementNode _statusBarWithShortcuts = null!;
    private ElementNode _statusBarWithManyShortcuts = null!;
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

        // StatusBar with Shortcuts container
        var shortcut1 = new ElementNode { ElementTypeName = "Shortcut" };
        shortcut1.Attributes["Title"] = "Quit";
        shortcut1.Attributes["Key"] = "QuitKey";
        shortcut1.Attributes["CanFocus"] = "false";

        var shortcut2 = new ElementNode { ElementTypeName = "Shortcut" };
        shortcut2.Attributes["Title"] = "Help";
        shortcut2.Attributes["Key"] = "F1";
        shortcut2.Attributes["CanFocus"] = "false";

        var shortcutsContainer = new ElementNode { ElementTypeName = "Shortcuts" };
        shortcutsContainer.Children.Add(shortcut1);
        shortcutsContainer.Children.Add(shortcut2);

        _statusBarWithShortcuts = new ElementNode { ElementTypeName = "StatusBar" };
        _statusBarWithShortcuts.Attributes["Visible"] = "true";
        _statusBarWithShortcuts.Attributes["AlignmentModes"] = "IgnoreFirstOrLast";
        _statusBarWithShortcuts.Children.Add(shortcutsContainer);

        // StatusBar with many shortcuts
        var manyShortcutsContainer = new ElementNode { ElementTypeName = "Shortcuts" };
        for (int i = 0; i < 10; i++)
        {
            var shortcut = new ElementNode { ElementTypeName = "Shortcut" };
            shortcut.Attributes["Title"] = $"Command{i}";
            shortcut.Attributes["Key"] = $"F{i + 1}";
            shortcut.Attributes["CanFocus"] = "false";
            manyShortcutsContainer.Children.Add(shortcut);
        }

        _statusBarWithManyShortcuts = new ElementNode { ElementTypeName = "StatusBar" };
        _statusBarWithManyShortcuts.Attributes["Visible"] = "{Binding ShowStatusBar}";
        _statusBarWithManyShortcuts.Children.Add(manyShortcutsContainer);
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

    [Benchmark(Description = "StatusBar With Shortcuts")]
    public string GenerateStatusBarWithShortcuts()
    {
        return _generator.GenerateClass(_statusBarWithShortcuts, "Benchmark", "StatusBarShortcuts", _factory);
    }

    [Benchmark(Description = "StatusBar With Many Shortcuts")]
    public string GenerateStatusBarWithManyShortcuts()
    {
        return _generator.GenerateClass(_statusBarWithManyShortcuts, "Benchmark", "StatusBarManyShortcuts", _factory);
    }

    [Benchmark(Description = "StatusBar With Shortcuts Statements")]
    public StatementSyntax[] GenerateStatusBarShortcutsStatements()
    {
        return _generator.GenerateStatements(_statusBarWithShortcuts, "statusBar", _factory);
    }
}
