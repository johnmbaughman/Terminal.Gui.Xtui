namespace Terminal.Gui.Xaml.Generators;

internal interface IGenerator
{
    string Template { get; }
    
    string Generate(ElementNode node, IGeneratorFactory generators);
}