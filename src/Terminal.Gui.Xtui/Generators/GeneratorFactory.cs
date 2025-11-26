using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Generators;

internal class GeneratorFactory : IGeneratorFactory
{
    private readonly Dictionary<string, Func<Generator>> _generators = new(
        StringComparer.OrdinalIgnoreCase)
    {
        { "Window", () => new WindowGenerator() },
        { "Label", () => new LabelGenerator() },
        { "Button", () => new ButtonGenerator() },
        // Add other generators here
    };

    public Generator GetGenerator(string elementName)
    {
        return _generators.TryGetValue(elementName, out var generatorFactory) 
            ? generatorFactory() 
            : new GenericGenerator();
    }
}