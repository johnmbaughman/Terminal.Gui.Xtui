using BenchmarkDotNet.Attributes;
using Terminal.Gui.Xtui.Generators;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Benchmarks;

[MemoryDiagnoser]
public class ShortcutGeneratorBenchmarks
{
    private readonly GeneratorFactory _generatorFactory = new();
    private readonly ShortcutGenerator _generator = new();
    private ElementNode _simpleShortcut = null!;
    private ElementNode _shortcutWithKey = null!;
    private ElementNode _complexShortcut = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleShortcut = new ElementNode { ElementTypeName = "Shortcut" };
        _simpleShortcut.Attributes["Title"] = "Quit";

        _shortcutWithKey = new ElementNode { ElementTypeName = "Shortcut" };
        _shortcutWithKey.Attributes["Title"] = "Help";
        _shortcutWithKey.Attributes["Key"] = "F1";
        _shortcutWithKey.Attributes["CanFocus"] = "false";

        _complexShortcut = new ElementNode { ElementTypeName = "Shortcut" };
        _complexShortcut.Attributes["Title"] = "Save";
        _complexShortcut.Attributes["HelpText"] = "Save current file";
        _complexShortcut.Attributes["Key"] = "Key.S.WithCtrl";
        _complexShortcut.Attributes["CanFocus"] = "false";
    }

    [Benchmark]
    public void GenerateStatements_SimpleShortcut()
    {
        _ = _generator.GenerateStatements(_simpleShortcut, "shortcut0", _generatorFactory);
    }

    [Benchmark]
    public void GenerateStatements_ShortcutWithKey()
    {
        _ = _generator.GenerateStatements(_shortcutWithKey, "shortcut0", _generatorFactory);
    }

    [Benchmark]
    public void GenerateStatements_ComplexShortcut()
    {
        _ = _generator.GenerateStatements(_complexShortcut, "shortcut0", _generatorFactory);
    }

    [Benchmark]
    public void GenerateStatements_MultipleShortcuts()
    {
        for (int i = 0; i < 10; i++)
        {
            _ = _generator.GenerateStatements(_shortcutWithKey, $"shortcut{i}", _generatorFactory);
        }
    }
}
