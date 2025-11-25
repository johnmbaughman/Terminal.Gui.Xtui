using System;
using System.Collections.Generic;
using System.Text;

namespace Terminal.Gui.Xaml.Generators;

internal interface IGeneratorFactory
{
    IGenerator GetGenerator(string elementName);
}

internal class GeneratorFactory : IGeneratorFactory
{
    private readonly Dictionary<string, Func<IGenerator>> _generators = new(
        StringComparer.OrdinalIgnoreCase)
    {
        { "Window", () => new WindowGenerator() },
        { "Label", () => new LabelGenerator() },
        { "Button", () => new ButtonGenerator() },
        // Add other generators here
    };

    public IGenerator GetGenerator(string elementName)
    {
        return _generators.TryGetValue(elementName, out var generatorFactory) 
            ? generatorFactory() 
            : new GenericGenerator();
    }
}

internal class GenericGenerator : IGenerator
{
    public string Template { get; }

    public string Generate(ElementNode node, IGeneratorFactory generators)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"// Generic generator for {node.Name}");
        sb.AppendLine($"var {node.Name.ToLower()} = new {node.Name}();");
        return sb.ToString();
    }
}

internal class ButtonGenerator : IGenerator
{
    public string Template { get; }

    public string Generate(ElementNode node, IGeneratorFactory generators)
    {
        throw new NotImplementedException();
    }
}

internal class LabelGenerator : IGenerator
{
    public string Template { get; }

    public string Generate(ElementNode node, IGeneratorFactory generators)
    {
        throw new NotImplementedException();
    }
}