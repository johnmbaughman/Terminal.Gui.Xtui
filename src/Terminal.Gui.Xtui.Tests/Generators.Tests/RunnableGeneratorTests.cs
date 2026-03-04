using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator;
using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class RunnableGeneratorTests
{
    [Fact]
    public void RunnableGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };
        node.Attributes["Title"] = "Main App";
        node.Attributes["Visible"] = "true";

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "runnable0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Runnable", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("Main App", generated);
        Assert.Contains("Visible=true", generated);
    }

    [Fact]
    public void RunnableGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("Runnable");

        Assert.NotNull(generator);
        Assert.IsType<RunnableGenerator>(generator);
    }

    [Fact]
    public void RunnableGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };
        node.Attributes["Title"] = "App";

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "myRunnable", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varmyRunnable", generated);
        Assert.Contains("newRunnable", generated);
    }

    [Fact]
    public void RunnableGenerator_GeneratesChildren()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "Hello";
        node.Children.Add(childLabel);

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "runnable0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newRunnable", generated);
        Assert.Contains("newLabel", generated);
        Assert.Contains("runnable0.Add(label0)", generated);
    }

    [Fact]
    public void RunnableGenerator_GeneratesClass_WithInitializeComponent()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };
        var childButton = new ElementNode { ElementTypeName = "Button" };
        childButton.Attributes["Text"] = "Click";
        childButton.Attributes["Id"] = "_myButton";
        node.Children.Add(childButton);

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "MainWindow", factory);

        Assert.Contains("InitializeComponent()", code);
        Assert.Contains("private Button? _myButton;", code);
        Assert.DoesNotContain("var _myButton", code);
        Assert.Contains("this._myButton = new Button()", code);
        Assert.DoesNotContain("this.Add(", code);
    }

    [Fact]
    public void RunnableGenerator_GeneratesClass_WithoutId()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "No ID";
        node.Children.Add(childLabel);

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "MainWindow", factory);

        Assert.Contains("InitializeComponent()", code);
        Assert.DoesNotContain("var label0", code);
        Assert.Contains("new Label()", code);
        Assert.Contains("private Label?", code);
        Assert.Contains("this.label0 = new Label()", code);
        Assert.DoesNotContain("this.Add(", code);
    }

    [Fact]
    public void RunnableGenerator_HandlesEmptyRunnable()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "empty", factory);

        Assert.Single(statements);
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varempty", generated);
        Assert.Contains("newRunnable()", generated);
    }

    [Fact]
    public void RunnableGenerator_CaseInsensitiveTypeName()
    {
        var factory = new GeneratorFactory();

        var generator1 = factory.GetGenerator("Runnable");
        var generator2 = factory.GetGenerator("runnable");

        Assert.NotNull(generator1);
        Assert.NotNull(generator2);
        Assert.IsType<RunnableGenerator>(generator1);
        Assert.IsType<RunnableGenerator>(generator2);
    }

    [Fact]
    public void RunnableGenerator_GeneratesClassWithGivenName()
    {
        var node = new ElementNode { ElementTypeName = "Runnable" };

        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyNamespace", "CustomRunnableClass", factory);

        Assert.Contains("public partial class CustomRunnableClass", code);
    }

    [Fact]
    public void RunnableGenerator_WithChildren_GeneratesAddStatements()
    {
        string xtui = "<Runnable xmlns=\"http://schemas.terminal.gui/xtui\"><Label Text=\"Hi\" /></Runnable>";
        var node = XtuiLoader.LoadFromString(xtui);
        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "mainRunnable", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Runnable", generated);
        Assert.Contains("label0", generated);
    }

    [Fact]
    public void RunnableGenerator_GenerateClass_IncludesInitializeComponentAndFields()
    {
        string xtui = "<Runnable xmlns=\"http://schemas.terminal.gui/xtui\"><Label Id=\"lbl1\" Text=\"Hello\" /></Runnable>";
        var node = XtuiLoader.LoadFromString(xtui);
        var generator = new RunnableGenerator();
        var factory = new GeneratorFactory();
        var code = generator.GenerateClass(node, "TestNamespace", "TestRunnable", factory);

        Assert.Contains("namespace TestNamespace", code);
        Assert.Contains("public partial class TestRunnable : Runnable", code);
        Assert.Contains("private void InitializeComponent()", code);
        Assert.Contains("lbl1", code);
    }
}
