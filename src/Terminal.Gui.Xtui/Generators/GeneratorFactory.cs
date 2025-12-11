using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Generators;

internal class GeneratorFactory : IGeneratorFactory
{
    private readonly Dictionary<string, Func<Generator>> _generators = new (
        StringComparer.OrdinalIgnoreCase)
    {
        { "Window", () => new WindowGenerator() },
        { "Toplevel", () => new TopLevelGenerator() },
        { "Label", () => new LabelGenerator() },
        { "Button", () => new ButtonGenerator() },
        { "CheckBox", () => new CheckBoxGenerator() },
        { "TextField", () => new TextFieldGenerator() },
        { "ListView", () => new ListViewGenerator() },
        { "MenuBar", () => new MenuBarGenerator() },
        { "StatusBar", () => new StatusBarGenerator() },
        { "MenuBarItem", () => new MenuBarItemGenerator() },
        { "MenuItem", () => new MenuItemGenerator() },
        // Add other generators here
    };

    public Generator GetGenerator (string elementName)
    {
        // Extract the local type name (after the last dot) to find the generator
        int lastDot = elementName.LastIndexOf('.');
        string localName = lastDot >= 0 ? elementName.Substring(lastDot + 1) : elementName;

        return _generators.TryGetValue (localName, out Func<Generator>? generatorFactory)
            ? generatorFactory ()
            : new GenericGenerator ();
    }
}