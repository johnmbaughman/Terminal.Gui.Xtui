using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator;using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class StatusBarGeneratorTests
{
    [Fact]
    public void StatusBarGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Attributes["Id"] = "statusBar";
        node.Attributes["Visible"] = "true";
        node.Attributes["AlignmentModes"] = "IgnoreFirstOrLast";
        node.Attributes["CanFocus"] = "false";

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("var statusBar", generated);
        Assert.Contains("new StatusBar", generated);
        Assert.Contains("Visible", generated);
        Assert.Contains("AlignmentModes", generated);
        Assert.Contains("CanFocus", generated);
    }

    [Fact]
    public void StatusBarGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("StatusBar");

        Assert.NotNull(generator);
        Assert.IsType<StatusBarGenerator>(generator);
    }

    [Fact]
    public void StatusBarGenerator_GeneratesClassWithInitializeComponent()
    {
        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Attributes["Id"] = "statusBar";
        node.Attributes["Visible"] = "true";

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "TestNamespace", "TestStatusBar", factory);

        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public partial class TestStatusBar : StatusBar", code);
        Assert.Contains("private void InitializeComponent()", code);
        Assert.Contains("this.Visible", code);
    }

    [Fact]
    public void StatusBarGenerator_WithChildren_GeneratesAddStatements()
    {
        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Attributes["Id"] = "statusBar";

        var child = new ElementNode { ElementTypeName = "Shortcut" };
        child.Attributes["Title"] = "Status";
        node.Children.Add(child);

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains(".Add(", generated);
        Assert.Contains("shortcut0", generated);
    }

    [Fact]
    public void StatusBarGenerator_ParsesFromXtui_GeneratesCorrectCode()
    {
        string xtui = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<StatusBar xmlns=\"http://schemas.terminal.gui/xtui\" Id=\"statusBar\" Visible=\"true\" AlignmentModes=\"IgnoreFirstOrLast\" CanFocus=\"false\" />";

        var root = XtuiLoader.LoadFromString(xtui);
        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(root, "TestApp", "MainStatusBar", factory);

        Assert.Contains("namespace TestApp", code);
        Assert.Contains("public partial class MainStatusBar : StatusBar", code);
        Assert.Contains("this.Visible", code);
        Assert.Contains("this.AlignmentModes", code);
    }

    [Fact]
    public void StatusBarGenerator_WithBindingDirective_PreservesBindingSyntax()
    {
        string xtui = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<StatusBar xmlns=\"http://schemas.terminal.gui/xtui\" Id=\"statusBar\" Visible=\"{Binding ShowStatusBar}\" />";
        var root = XtuiLoader.LoadFromString(xtui);
        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(root, "TestApp", "MainStatusBar", factory);

        Assert.Contains("namespace TestApp", code);
        Assert.Contains("this.Visible", code);
    }

    [Fact]
    public void StatusBarGenerator_WithShortcutsContainer_GeneratesShortcuts()
    {
        var shortcutNode = new ElementNode { ElementTypeName = "Shortcut" };
        shortcutNode.Attributes["Title"] = "Quit";
        shortcutNode.Attributes["Key"] = "F10";
        shortcutNode.Attributes["CanFocus"] = "false";

        var shortcutsContainer = new ElementNode { ElementTypeName = "Shortcuts" };
        shortcutsContainer.Children.Add(shortcutNode);

        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Attributes["Id"] = "statusBar";
        node.Children.Add(shortcutsContainer);

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("var statusBar", generated);
        Assert.Contains("var shortcut0", generated);
        Assert.Contains("statusBar.Add(shortcut0)", generated);
    }

    [Fact]
    public void StatusBarGenerator_WithMultipleShortcuts_GeneratesAllShortcuts()
    {
        var shortcut1 = new ElementNode { ElementTypeName = "Shortcut" };
        shortcut1.Attributes["Title"] = "Quit";
        shortcut1.Attributes["Key"] = "QuitKey";

        var shortcut2 = new ElementNode { ElementTypeName = "Shortcut" };
        shortcut2.Attributes["Title"] = "Help";
        shortcut2.Attributes["Key"] = "F1";

        var shortcutsContainer = new ElementNode { ElementTypeName = "Shortcuts" };
        shortcutsContainer.Children.Add(shortcut1);
        shortcutsContainer.Children.Add(shortcut2);

        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Children.Add(shortcutsContainer);

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("var shortcut0", generated);
        Assert.Contains("var shortcut1", generated);
        Assert.Contains("Add(", generated);
        Assert.Contains("shortcut0", generated);
        Assert.Contains("shortcut1", generated);
    }

    [Fact]
    public void StatusBarGenerator_GeneratesClass_WithShortcutsInline()
    {
        var shortcutNode = new ElementNode { ElementTypeName = "Shortcut" };
        shortcutNode.Attributes["Title"] = "Quit";
        shortcutNode.Attributes["Key"] = "F10";

        var shortcutsContainer = new ElementNode { ElementTypeName = "Shortcuts" };
        shortcutsContainer.Children.Add(shortcutNode);

        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Children.Add(shortcutsContainer);

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "TestNamespace", "TestStatusBar", factory);

        Assert.Contains("var shortcut0", code);
        Assert.Contains("Quit", code);
        Assert.Contains("Key.F10", code);
        Assert.Contains("this.Add(shortcut0)", code);
    }

    [Fact]
    public void StatusBarGenerator_WithShortcutsContainer_GeneratesAddStatements()
    {
        string xtui = @"<StatusBar xmlns=""http://schemas.terminal.gui/xtui""><Shortcuts><Shortcut Title=""Quit"" Key=""F10"" /></Shortcuts></StatusBar>";
        var node = XtuiLoader.LoadFromString(xtui);
        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "status", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("new StatusBar", generated);
        Assert.Contains("shortcut0", generated);
        Assert.Contains("Add(", generated);
    }

    [Fact]
    public void StatusBarGenerator_GenerateClass_IncludesInitializeComponentAndFields()
    {
        string xtui = @"<StatusBar xmlns=""http://schemas.terminal.gui/xtui""><Shortcut Id=""quitShortcut"" Title=""Quit"" /></StatusBar>";
        var node = XtuiLoader.LoadFromString(xtui);
        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();
        var code = generator.GenerateClass(node, "TestNamespace", "TestStatusBar", factory);

        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public partial class TestStatusBar : StatusBar", code);
        Assert.Contains("private void InitializeComponent()", code);
        Assert.Contains("quitShortcut", code);
        Assert.Contains("Add(", code);
    }
}
