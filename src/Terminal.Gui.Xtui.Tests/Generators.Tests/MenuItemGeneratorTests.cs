using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class MenuItemGeneratorTests
{
    [Fact]
    public void MenuItemGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Open";
        node.Attributes["HelpText"] = "Open file";

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menuitem0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("MenuItem", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("Open", generated);
        Assert.Contains("HelpText=", generated);
    }

    [Fact]
    public void MenuItemGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("MenuItem");

        Assert.NotNull(generator);
        Assert.IsType<MenuItemGenerator>(generator);
    }

    [Fact]
    public void MenuItemGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Save";

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "saveItem", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varsaveItem", generated);
        Assert.Contains("newMenuItem", generated);
    }

    [Fact]
    public void MenuItemGenerator_GeneratesSimpleMenuItem()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Exit";

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "exitItem", factory);

        Assert.Single(statements); // Just the var declaration
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varexitItem", generated);
        Assert.Contains("newMenuItem", generated);
        Assert.Contains("Exit", generated);
    }

    [Fact]
    public void MenuItemGenerator_HandlesEmptyMenuItem()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "item", factory);

        Assert.Single(statements);
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varitem", generated);
        Assert.Contains("newMenuItem()", generated);
    }

    [Fact]
    public void MenuItemGenerator_HandlesMultipleProperties()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Copy";
        node.Attributes["HelpText"] = "Copy selection";
        node.Attributes["Enabled"] = "true";

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "copyItem", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Title=\"Copy\"", generated);
        Assert.Contains("HelpText=", generated);
        Assert.Contains("Enabled=true", generated);
    }

    [Fact]
    public void MenuItemGenerator_DoesNotProcessChildren()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Parent";

        // MenuItems don't have child controls
        var child = new ElementNode { ElementTypeName = "Label" };
        node.Children.Add(child);

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "item", factory);

        // Should only generate the MenuItem itself, not process children
        Assert.Single(statements);
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.DoesNotContain("Label", generated);
    }

    [Fact]
    public void MenuItemGenerator_HandlesDataProperty()
    {
        var node = new ElementNode { ElementTypeName = "MenuItem" };
        node.Attributes["Title"] = "Show Toolbar";
        node.Attributes["HelpText"] = "Toggle toolbar";
        node.Attributes["Enabled"] = "true";

        var generator = new MenuItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "toggleItem", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Title=", generated);
        Assert.Contains("HelpText=", generated);
        Assert.Contains("Enabled=true", generated);
    }
}
