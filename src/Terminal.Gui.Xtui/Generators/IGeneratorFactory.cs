namespace Terminal.Gui.Xtui.Generators;

internal interface IGeneratorFactory
{
    Generator GetGenerator (string elementName);
}