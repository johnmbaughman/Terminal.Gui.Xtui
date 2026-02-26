using Terminal.Gui.Xtui.Generator;using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class GenericGeneratorTests
{
    [Fact]
    public void GenericGenerator_GeneratesObjectInitializer_ForUnknownType()
    {
        var node = new ElementNode { ElementTypeName = "CustomView" };
        node.Attributes["Text"] = "Custom";
        node.Attributes["Enabled"] = "true";

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "custom0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("CustomView", generated);
        Assert.Contains("Text=", generated);
        Assert.Contains("Custom", generated);
        Assert.Contains("Enabled=true", generated);
    }

    [Fact]
    public void GenericGenerator_IsUsedForUnknownTypes()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("UnknownControl");

        Assert.NotNull(generator);
        Assert.IsType<GenericGenerator>(generator);
    }

    [Fact]
    public void GenericGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "TextView" };
        node.Attributes["Text"] = "Sample";

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "textView", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("vartextView", generated);
        Assert.Contains("newTextView", generated);
    }

    [Fact]
    public void GenericGenerator_GeneratesChildren()
    {
        var node = new ElementNode { ElementTypeName = "FrameView" };
        node.Attributes["Title"] = "Container";

        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "Inside Frame";
        node.Children.Add(childLabel);

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "frame0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newFrameView", generated);
        Assert.Contains("newLabel", generated);
        Assert.Contains("frame0.Add(label0)", generated);
    }

    [Fact]
    public void GenericGenerator_HandlesEmptyControl()
    {
        var node = new ElementNode { ElementTypeName = "Panel" };

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "panel", factory);

        Assert.Single(statements); // Just the var declaration
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varpanel", generated);
        Assert.Contains("newPanel()", generated);
    }

    [Fact]
    public void GenericGenerator_GeneratesMultipleChildren()
    {
        var node = new ElementNode { ElementTypeName = "Dialog" };

        var button1 = new ElementNode { ElementTypeName = "Button" };
        button1.Attributes["Text"] = "OK";

        var button2 = new ElementNode { ElementTypeName = "Button" };
        button2.Attributes["Text"] = "Cancel";

        node.Children.Add(button1);
        node.Children.Add(button2);

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "dialog0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("dialog0.Add(button0)", generated);
        Assert.Contains("dialog0.Add(button1)", generated);
    }

    [Fact]
    public void GenericGenerator_PreservesTypeName()
    {
        var node = new ElementNode { ElementTypeName = "StatusBar" };
        node.Attributes["Visible"] = "true";

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "statusBar", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        // Should use exact type name from node
        Assert.Contains("newStatusBar", generated);
        Assert.DoesNotContain("GenericView", generated);
    }

    [Fact]
    public void GenericGenerator_HandlesNestedChildren()
    {
        var node = new ElementNode { ElementTypeName = "Container" };

        var child1 = new ElementNode { ElementTypeName = "Panel" };
        var grandchild = new ElementNode { ElementTypeName = "Label" };
        grandchild.Attributes["Text"] = "Nested";
        child1.Children.Add(grandchild);
        node.Children.Add(child1);

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "container0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newContainer", generated);
        Assert.Contains("newPanel", generated);
        Assert.Contains("newLabel", generated);
        Assert.Contains("panel0.Add(label0)", generated);
        Assert.Contains("container0.Add(panel0)", generated);
    }

    [Fact]
    public void GenericGenerator_UsesObjectParsingHelpers()
    {
        var node = new ElementNode { ElementTypeName = "CustomControl" };
        node.Attributes["X"] = "10";
        node.Attributes["Y"] = "{Center}";
        node.Attributes["Width"] = "50%";

        var generator = new GenericGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "custom", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        // Should use ObjectParsingHelpers for position/dimension parsing
        Assert.Contains("X=10", generated);
        Assert.Contains("Y=Pos.Center()", generated);
        Assert.Contains("Width=Dim.Percent(50", generated);
    }
}
