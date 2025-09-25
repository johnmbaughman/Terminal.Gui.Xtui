using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Terminal.Gui.Xaml.Parsing;
using Terminal.Gui.Xaml.Model;

namespace Terminal.Gui.Xaml.Runtime;

/// <summary>
/// Loads XAML files at runtime, instantiates controls, and manages resources.
/// </summary>
public class RuntimeLoader
{
    private readonly Dictionary<string, object> _cache = new();

    /// <summary>
    /// Initializes a new instance of RuntimeLoader.
    /// </summary>
    public RuntimeLoader() { }

    /// <summary>
    /// Loads and instantiates controls from a XAML file at runtime.
    /// </summary>
    /// <param name="xamlPath">Absolute path to the XAML file.</param>
    /// <param name="viewModel">Optional ViewModel for dependency injection.</param>
    /// <returns>Instantiated control object.</returns>
    public object Load(string xamlPath, object? viewModel = null)
    {
        if (_cache.TryGetValue(xamlPath, out var cached))
        {
            return cached;
        }

        var xaml = File.ReadAllText(xamlPath);
        var parser = new Parsing.XamlParser();
        var xamlDoc = parser.ParseAsync(xaml).GetAwaiter().GetResult();
        var control = ControlFactory.Create(xamlDoc, viewModel);
        _cache[xamlPath] = control;
        return control;
    }

    /// <summary>
    /// Clears cached controls and resources.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}
