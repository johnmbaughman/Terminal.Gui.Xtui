using Terminal.Gui.Xtui.Generators;
using Terminal.Gui.Xtui.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class WindowGeneratorTests
{
    [Fact]
    public void WindowGenerator_GeneratesObjectInitializer_WithExpectedProperties()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        node.Attributes["Title"] = "My Window";
        node.Attributes["Visible"] = "true";
        node.Attributes["Enabled"] = "false";

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "window0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Window", generated);
        Assert.Contains("Title=", generated);
        Assert.Contains("My Window", generated);
        Assert.Contains("Visible=true", generated);
        Assert.Contains("Enabled=false", generated);
    }

    [Fact]
    public void WindowGenerator_IsRegisteredInFactory()
    {
        var factory = new GeneratorFactory();
        var generator = factory.GetGenerator("Window");

        Assert.NotNull(generator);
        Assert.IsType<WindowGenerator>(generator);
    }

    [Fact]
    public void WindowGenerator_GeneratesVariableDeclaration()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        node.Attributes["Title"] = "Dialog";

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "myWindow", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("varmyWindow", generated);
        Assert.Contains("newWindow", generated);
    }

    [Fact]
    public void WindowGenerator_GeneratesChildren()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "Hello";
        node.Children.Add(childLabel);

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "window0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newWindow", generated);
        Assert.Contains("newLabel", generated);
        Assert.Contains("window0.Add(label0)", generated);
    }

    [Fact]
    public void WindowGenerator_GeneratesClass_WithInitializeComponent()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        var childButton = new ElementNode { ElementTypeName = "Button" };
        childButton.Attributes["Text"] = "OK";
        childButton.Attributes["Id"] = "_okButton";
        node.Children.Add(childButton);

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "MainWindow", factory);
        
        Assert.Contains("InitializeComponent()", code);
        Assert.Contains("using Terminal.Gui.Views;", code);
        Assert.Contains("using Terminal.Gui.ViewBase;", code);
        Assert.Contains("public partial class MainWindow : Window", code);
        Assert.Contains("private Button? _okButton;", code);
        Assert.Contains("var button0 = new Button", code);
        Assert.Contains("this._okButton = button0", code);
        Assert.Contains("this.Add(button0)", code);
    }

    [Fact]
    public void WindowGenerator_GeneratesClass_WithoutId()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        var childLabel = new ElementNode { ElementTypeName = "Label" };
        childLabel.Attributes["Text"] = "Status";
        node.Children.Add(childLabel);

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "StatusWindow", factory);

        Assert.Contains("InitializeComponent()", code);
        Assert.Contains("public partial class StatusWindow : Window", code);
        Assert.Contains("var label0 = new Label", code); // Window generates local variables for all children
        Assert.Contains("this.Add(label0)", code); // Window adds children via local variables
        Assert.Contains("private Label?", code); // Field is declared for children (generator now declares fields)
    }

    [Fact]
    public void WindowGenerator_HandlesEmptyWindow()
    {
        var node = new ElementNode { ElementTypeName = "Window" };

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var statements = generator.GenerateStatements(node, "empty", factory);

        Assert.Single(statements); // Just the var declaration
        var generated = string.Concat(statements.Select(s => s.ToString()));
        Assert.Contains("varempty", generated);
        Assert.Contains("newWindow()", generated);
    }

    [Fact]
    public void WindowGenerator_RemovesIdFromAttributes()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        var childTextField = new ElementNode { ElementTypeName = "TextField" };
        childTextField.Attributes["Id"] = "_textField";
        childTextField.Attributes["Text"] = "Input";
        node.Children.Add(childTextField);

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "InputWindow", factory);

        // Id should not appear in object initializer
        Assert.DoesNotContain("Id=", code);
        // But the field should be declared
        Assert.Contains("private TextField? _textField;", code);
    }

    [Fact]
    public void WindowGenerator_GeneratesMultipleChildren()
    {
        var node = new ElementNode { ElementTypeName = "Window" };
        
        var label1 = new ElementNode { ElementTypeName = "Label" };
        label1.Attributes["Text"] = "First";
        label1.Attributes["Id"] = "_label1";
        
        var label2 = new ElementNode { ElementTypeName = "Label" };
        label2.Attributes["Text"] = "Second";
        label2.Attributes["Id"] = "_label2";
        
        node.Children.Add(label1);
        node.Children.Add(label2);

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyApp", "TwoLabels", factory);

        Assert.Contains("private Label? _label1;", code);
        Assert.Contains("private Label? _label2;", code);
        Assert.Contains("var label0 = new Label", code);
        Assert.Contains("var label1 = new Label", code);
        Assert.Contains("this._label1 = label0", code);
        Assert.Contains("this._label2 = label1", code);
        Assert.Contains("this.Add(label0)", code);
        Assert.Contains("this.Add(label1)", code);
    }

    [Fact]
    public void WindowGenerator_GeneratesClassWithGivenName()
    {
        var node = new ElementNode { ElementTypeName = "Window" };

        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();

        var code = generator.GenerateClass(node, "MyNamespace", "CustomWindowClass", factory);

        Assert.Contains("public partial class CustomWindowClass", code);
    }

    [Fact]
    public void WindowGenerator_WithPosExpressions_GeneratesPosCode()
    {
        string xtui = @"<Window xmlns=""http://schemas.terminal.gui/xtui"" 
                                X=""{Center}"" 
                                Y=""{Center}"" 
                                Width=""50"" 
                                Height=""20"" />";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "window1", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("X=Pos.Center()", generated);
        Assert.Contains("Y=Pos.Center()", generated);
        Assert.Contains("Width=50", generated);
        Assert.Contains("Height=20", generated);
    }

    [Fact]
    public void WindowGenerator_WithMultipleChildren_GeneratesAllChildren()
    {
        string xtui = @"<Window xmlns=""http://schemas.terminal.gui/xtui"" Title=""Form"">
                            <Label Text=""Username:"" />
                            <TextField Text="""" />
                            <Button Text=""Submit"" />
                        </Window>";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new WindowGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "window0", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("newLabel", generated);
        Assert.Contains("newTextField", generated);
        Assert.Contains("newButton", generated);
        Assert.Contains("window0.Add(label0)", generated);
        Assert.Contains("window0.Add(textfield1)", generated);
        Assert.Contains("window0.Add(button2)", generated);
    }
}
