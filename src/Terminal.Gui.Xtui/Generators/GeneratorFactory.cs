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
        { "MenuBarItem", () => new MenuBarItemGenerator() },
        { "MenuItem", () => new MenuItemGenerator() },
        // Add other generators here
    };

    public Generator GetGenerator (string elementName)
    {
        return _generators.TryGetValue (elementName, out Func<Generator>? generatorFactory)
            ? generatorFactory ()
            : new GenericGenerator ();
    }
}