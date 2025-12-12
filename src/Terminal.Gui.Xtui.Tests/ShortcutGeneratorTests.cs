using Xunit;
using Terminal.Gui.Xtui.Generators;
using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Tests;

public class ShortcutGeneratorTests
{
    private readonly GeneratorFactory _generatorFactory = new();

    [Fact]
    public void GenerateStatements_BasicShortcut_CreatesValidCode()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };
        node.Attributes["Title"] = "Quit";
        node.Attributes["CanFocus"] = "false";

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut0", _generatorFactory);

        // Assert
        Assert.NotNull(statements);
        Assert.Single(statements); // Should only create the variable declaration
        
        var code = statements[0].ToFullString();
        Assert.Contains("var shortcut0", code);
        Assert.Contains("new Shortcut", code);
        Assert.Contains("Title = \"Quit\"", code);
        Assert.Contains("CanFocus = false", code);
    }

    [Fact]
    public void GenerateStatements_ShortcutWithSimpleKeyEnum_GeneratesKeyDotNotation()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };
        node.Attributes["Title"] = "Help";
        node.Attributes["Key"] = "F10";

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut0", _generatorFactory);

        // Assert
        var code = statements[0].ToFullString();
        Assert.Contains("Key = Key.F10", code);
    }

    [Fact]
    public void GenerateStatements_ShortcutWithQuitKey_GeneratesApplicationQuitKey()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };
        node.Attributes["Title"] = "Quit";
        node.Attributes["Key"] = "QuitKey";

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut0", _generatorFactory);

        // Assert
        var code = statements[0].ToFullString();
        Assert.Contains("Key = Application.QuitKey", code);
    }

    [Fact]
    public void GenerateStatements_ShortcutWithFullyQualifiedKey_PreservesFormat()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };
        node.Attributes["Title"] = "Copy";
        node.Attributes["Key"] = "Key.C.WithCtrl";

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut0", _generatorFactory);

        // Assert
        var code = statements[0].ToFullString();
        Assert.Contains("Key = Key.C.WithCtrl", code);
    }

    [Fact]
    public void GenerateStatements_ShortcutWithMultipleProperties_GeneratesAllProperties()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };
        node.Attributes["Title"] = "Version Info";
        node.Attributes["CanFocus"] = "false";
        node.Attributes["HelpText"] = "Show version";

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut1", _generatorFactory);

        // Assert
        var code = statements[0].ToFullString();
        Assert.Contains("Title = \"Version Info\"", code);
        Assert.Contains("CanFocus = false", code);
        Assert.Contains("HelpText = \"Show version\"", code);
    }

    [Fact]
    public void GenerateStatements_EmptyShortcut_GeneratesBasicObject()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "Shortcut" };

        var generator = new ShortcutGenerator();

        // Act
        var statements = generator.GenerateStatements(node, "shortcut0", _generatorFactory);

        // Assert
        Assert.NotNull(statements);
        Assert.Single(statements);
        
        var code = statements[0].ToFullString();
        Assert.Contains("var shortcut0", code);
        Assert.Contains("new Shortcut()", code);
    }
}
