using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

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

        var child = new ElementNode { ElementTypeName = "Label" };
        child.Attributes["Text"] = "Status";
        node.Children.Add(child);

        var generator = new StatusBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains(".Add(", generated);
        Assert.Contains("label0", generated);
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
}
