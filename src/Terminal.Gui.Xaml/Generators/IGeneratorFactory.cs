namespace Terminal.Gui.Xaml.Generators;

internal interface IGeneratorFactory
{
    Generator GetGenerator(string elementName);
}