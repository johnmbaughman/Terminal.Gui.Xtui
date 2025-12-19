using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class MenuBarGeneratorTests
{
    [Fact]
    public void MenuBarGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["Title"] = "menuBar";
        node.Attributes["Id"] = "menuBar";

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newMenuBar", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("menuBar", generated);
        Assert.Contains("Id=", generated);
    }

    [Fact]
    public void MenuBarGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("MenuBar");

        Assert.NotNull(generator);
        Assert.IsType<MenuBarGenerator>(generator);
    }

    [Fact]
    public void MenuBarGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["Title"] = "Main Menu";

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "myMenuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varmyMenuBar", generated);
        Assert.Contains("newMenuBar", generated);
    }

    [Fact]
    public void MenuBarGenerator_WithMultipleProperties_GeneratesAllProperties()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["Title"] = "menuBar";
        node.Attributes["Id"] = "menuBar";
        node.Attributes["Visible"] = "true";
        node.Attributes["Enabled"] = "true";

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar1", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Title=", generated);
        Assert.Contains("Id=", generated);
        Assert.Contains("Visible=true", generated);
        Assert.Contains("Enabled=true", generated);
    }

    [Fact]
    public void MenuBarGenerator_WithNoAttributes_GeneratesEmptyInitializer()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varmenuBar", generated);
        Assert.Contains("newMenuBar()", generated);
    }

    [Fact]
    public void MenuBarGenerator_GeneratesClassWithInitializeComponent()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["Title"] = "Main Menu";

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "TestNamespace", "TestMenuBar", factory);

        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public partial class TestMenuBar : MenuBar", code);
        Assert.Contains("private void InitializeComponent()", code);
        Assert.Contains("this.Title", code);
    }

    [Fact]
    public void MenuBarGenerator_GeneratesUsingsForTerminalGui()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "TestNamespace", "TestMenuBar", factory);

        Assert.Contains("using Terminal.Gui.Views;", code);
        Assert.Contains("using Terminal.Gui.ViewBase;", code);
    }

    [Fact]
    public void MenuBarGenerator_WithChildren_GeneratesAddStatements()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["Title"] = "Main Menu";

        // Add a child MenuBarItem (using generic generator for now)
        var childNode = new ElementNode { ElementTypeName = "MenuBarItem" };
        childNode.Attributes["Title"] = "_File";
        node.Children.Add(childNode);

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newMenuBar", generated);
        Assert.Contains(".Add(", generated);
        Assert.Contains("menubaritem0", generated);
    }

    [Fact]
    public void MenuBarGenerator_WithMultipleChildren_GeneratesMultipleAddStatements()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        
        // Add multiple child MenuBarItems
        for (int i = 0; i < 3; i++)
        {
            var childNode = new ElementNode { ElementTypeName = "MenuBarItem" };
            childNode.Attributes["Title"] = $"Menu{i}";
            node.Children.Add(childNode);
        }

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        // MenuBar now emits a single param-array Add call containing all items
        Assert.Equal(1, generated.Split(new[] { ".Add(" }, System.StringSplitOptions.None).Length - 1);
        Assert.Contains("menubaritem0", generated);
        Assert.Contains("menubaritem1", generated);
        Assert.Contains("menubaritem2", generated);
    }

    [Fact]
    public void MenuBarGenerator_ParsesFromXtui_GeneratesCorrectCode()
    {
        string xtui = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MenuBar Title=""Main Menu"" Id=""menuBar"">
</MenuBar>";

        var root = XtuiLoader.LoadFromString(xtui);
        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(root, "TestApp", "MainMenu", factory);

        Assert.Contains("namespace TestApp", code);
        Assert.Contains("public partial class MainMenu : MenuBar", code);
        Assert.Contains("this.Title", code);
        Assert.Contains("Main Menu", code);
    }

    [Fact]
    public void MenuBarGenerator_WithPositionalProperties_GeneratesCorrectSyntax()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };
        node.Attributes["X"] = "0";
        node.Attributes["Y"] = "0";
        node.Attributes["Width"] = "{Fill}";
        node.Attributes["Height"] = "1";

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("X=0", generated);
        Assert.Contains("Y=0", generated);
        Assert.Contains("Width=Dim.Fill()", generated);
        Assert.Contains("Height=1", generated);
    }

    [Fact]
    public void MenuBarGenerator_GeneratesClassWithGivenName()
    {
        var node = new ElementNode { ElementTypeName = "MenuBar" };

        var generator = new MenuBarGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyNamespace", "CustomMenuBarClass", factory);

        Assert.Contains("public partial class CustomMenuBarClass", code);
    }
}
