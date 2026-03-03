using System;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Generator.Helpers;

internal class GeneratorFactory : IGeneratorFactory
{
    private readonly Dictionary<string, Func<Generator>> _generators = new (
        StringComparer.OrdinalIgnoreCase)
    {
        { "Window", () => new Generators.WindowGenerator() },
        { "Runnable", () => new Generators.RunnableGenerator() },
        { "Label", () => new Generators.LabelGenerator() },
        { "Button", () => new Generators.ButtonGenerator() },
        { "CheckBox", () => new Generators.CheckBoxGenerator() },
        { "TextField", () => new Generators.TextFieldGenerator() },
        { "ListView", () => new Generators.ListViewGenerator() },
        { "MenuBar", () => new Generators.MenuBarGenerator() },
        { "StatusBar", () => new Generators.StatusBarGenerator() },
        { "MenuBarItem", () => new Generators.MenuBarItemGenerator() },
        { "MenuItem", () => new Generators.MenuItemGenerator() },
        { "Shortcut", () => new Generators.ShortcutGenerator() },
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
