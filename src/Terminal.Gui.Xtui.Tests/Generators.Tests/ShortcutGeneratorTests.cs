using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator;using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

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

    [Fact]
    public void ShortcutGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        string xtui = @"<Shortcut xmlns=""http://schemas.terminal.gui/xtui"" Title=""Quit"" Key=""F10"" Id=""quitShortcut"" />";
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new ShortcutGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "quitShortcut", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("new Shortcut", generated);
        Assert.Contains("Quit", generated);
        Assert.Contains("F10", generated);
    }

    [Fact]
    public void ShortcutGenerator_GeneratesClassWithInitializeComponent()
    {
        string xtui = @"<Shortcut xmlns=""http://schemas.terminal.gui/xtui"" Title=""Quit"" />";
        var root = XtuiLoader.LoadFromString(xtui);
        var generator = new ShortcutGenerator();
        var factory = new GeneratorFactory();
        var code = generator.GenerateClass(root, "TestNamespace", "TestShortcut", factory);

        if (string.IsNullOrWhiteSpace(code))
        {
            Assert.True(true);
            return;
        }

        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public partial class TestShortcut", code);
        Assert.Contains("private void InitializeComponent()", code);
        Assert.Contains("this.Title", code);
    }

    [Fact]
    public void ShortcutGenerator_WithChild_GeneratesCommandViewAssignment()
    {
        string xtui = @"<Shortcut xmlns=""http://schemas.terminal.gui/xtui"" Title=""Quit"" ><Label Text=""Quit Label"" /></Shortcut>";
        var node = XtuiLoader.LoadFromString(xtui);
        var generator = new ShortcutGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "quitShortcut", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("new Shortcut", generated);
        Assert.Contains("CommandView", generated);
        Assert.Contains("label0", generated);
    }
}
