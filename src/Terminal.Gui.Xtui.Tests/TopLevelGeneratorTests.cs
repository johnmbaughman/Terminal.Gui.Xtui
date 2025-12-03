using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

public class TopLevelGeneratorTests
{
    [Fact]
    public void TopLevelGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };
        node.Attributes["Title"] = "Main App";
        node.Attributes["Visible"] = "true";

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "toplevel0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Toplevel", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("Main App", generated);
        Assert.Contains("Visible=true", generated);
    }

    [Fact]
    public void TopLevelGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("Toplevel");

        Assert.NotNull(generator);
        Assert.IsType<TopLevelGenerator>(generator);
    }

    [Fact]
    public void TopLevelGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };
        node.Attributes["Title"] = "App";

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "myTop", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varmyTop", generated);
        Assert.Contains("newToplevel", generated);
    }

    [Fact]
    public void TopLevelGenerator_GeneratesChildren()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "Hello";
        node.Children.Add(childLabel);

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "toplevel0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newToplevel", generated);
        Assert.Contains("newLabel", generated);
        Assert.Contains("toplevel0.Add(label0)", generated);
    }

    [Fact]
    public void TopLevelGenerator_GeneratesClass_WithInitializeComponent()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };
        var childButton = new ElementNode { ElementTypeName = "Button" };
        childButton.Attributes["Text"] = "Click";
        childButton.Attributes["Id"] = "_myButton";
        node.Children.Add(childButton);

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "MainWindow", factory);
        
        Assert.Contains("InitializeComponent()", code);
        Assert.Contains("private Button? _myButton;", code);
        Assert.Contains("var _myButton = new Button()", code);
        Assert.Contains("this._myButton = _myButton;", code);
        Assert.DoesNotContain("this.Add(", code); // TopLevel doesn't auto-add children with Id
    }

    [Fact]
    public void TopLevelGenerator_GeneratesClass_WithoutId()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "No ID";
        node.Children.Add(childLabel);

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "MainWindow", factory);

        Assert.Contains("InitializeComponent()", code);
        Assert.Contains("var label0", code);
        Assert.Contains("new Label()", code);
        Assert.DoesNotContain("private Label?", code); // No field since no Id
        Assert.DoesNotContain("this.Add(", code); // No Add since child has no Id
    }

    [Fact]
    public void TopLevelGenerator_HandlesEmptyTopLevel()
    {
        var node = new ElementNode { ElementTypeName = "Toplevel" };

        var generator = new TopLevelGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "empty", factory);

        Assert.Single(statements); // Just the var declaration
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varempty", generated);
        Assert.Contains("newToplevel()", generated);
    }

    [Fact]
    public void TopLevelGenerator_CaseInsensitiveTypeName()
    {
        var factory = new GeneratorFactory();
        
        var generator1 = factory.GetGenerator("Toplevel");
        var generator2 = factory.GetGenerator("TopLevel");

        Assert.NotNull(generator1);
        Assert.NotNull(generator2);
        Assert.IsType<TopLevelGenerator>(generator1);
        Assert.IsType<TopLevelGenerator>(generator2);
    }
}
