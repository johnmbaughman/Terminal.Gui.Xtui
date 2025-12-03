using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

public class MenuBarItemGeneratorTests
{
    [Fact]
    public void MenuBarItemGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "File";

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "menubaritem0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("MenuBarItem", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("File", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("MenuBarItem");

        Assert.NotNull(generator);
        Assert.IsType<MenuBarItemGenerator>(generator);
    }

    [Fact]
    public void MenuBarItemGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "Edit";

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "fileMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varfileMenu", generated);
        Assert.Contains("newMenuBarItem", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_GeneratesMenuItems_WithMenuItemsContainer()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "File";

        var menuItemsContainer = new ElementNode { ElementTypeName = "MenuItems" };
        var menuItem = new ElementNode { ElementTypeName = "MenuItem" };
        menuItem.Attributes["Title"] = "Open";
        menuItemsContainer.Children.Add(menuItem);
        node.Children.Add(menuItemsContainer);

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "fileMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newMenuBarItem", generated);
        Assert.Contains("fileMenu.PopoverMenu=newPopoverMenu(newMenu())", generated);
        Assert.Contains("newMenuItem", generated);
        Assert.Contains("fileMenu.PopoverMenu.Root.Add(menuitem0)", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_GeneratesMenuItems_WithoutContainer()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "Edit";

        var menuItem = new ElementNode { ElementTypeName = "MenuItem" };
        menuItem.Attributes["Title"] = "Cut";
        node.Children.Add(menuItem);

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "editMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newMenuBarItem", generated);
        Assert.Contains("editMenu.PopoverMenu=newPopoverMenu(newMenu())", generated);
        Assert.Contains("newMenuItem", generated);
        Assert.Contains("editMenu.PopoverMenu.Root.Add(menuitem0)", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_HandlesEmptyMenuBarItem()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "Help";

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "helpMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varhelpMenu", generated);
        Assert.Contains("newMenuBarItem", generated);
        // Should not create PopoverMenu if no children
        Assert.DoesNotContain("PopoverMenu", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_GeneratesMultipleMenuItems()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "File";

        var openItem = new ElementNode { ElementTypeName = "MenuItem" };
        openItem.Attributes["Title"] = "Open";
        
        var saveItem = new ElementNode { ElementTypeName = "MenuItem" };
        saveItem.Attributes["Title"] = "Save";
        
        node.Children.Add(openItem);
        node.Children.Add(saveItem);

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "fileMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("fileMenu.PopoverMenu.Root.Add(menuitem0)", generated);
        Assert.Contains("fileMenu.PopoverMenu.Root.Add(menuitem1)", generated);
    }

    [Fact]
    public void MenuBarItemGenerator_IgnoresNonMenuItemChildren()
    {
        var node = new ElementNode { ElementTypeName = "MenuBarItem" };
        node.Attributes["Title"] = "File";

        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes["Text"] = "Not a menu item";
        node.Children.Add(label);

        var generator = new MenuBarItemGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "fileMenu", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        // Should not process non-MenuItem children
        Assert.DoesNotContain("PopoverMenu", generated);
    }
}
