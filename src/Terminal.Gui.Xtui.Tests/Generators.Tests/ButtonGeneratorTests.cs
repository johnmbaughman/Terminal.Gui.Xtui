using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class ButtonGeneratorTests
{
    [Fact]
    public void ButtonGenerator_GeneratesObjectInitializer_WithExpectedProperties ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };
        node.Attributes["Text"] = "Click Me";
        node.Attributes["Enabled"] = "true";
        node.Attributes["Visible"] = "false";

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "button0", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("newButton", generated);
        Assert.Contains ("Text=", generated);
        Assert.Contains ("Click Me", generated);
        Assert.Contains ("Enabled=true", generated);
        Assert.Contains ("Visible=false", generated);
    }

    [Fact]
    public void ButtonGenerator_IsRegisteredInFactory ()
    {
        var factory = new GeneratorFactory ();
        var generator = factory.GetGenerator ("Button");

        Assert.NotNull (generator);
        Assert.IsType<ButtonGenerator> (generator);
    }

    [Fact]
    public void ButtonGenerator_GeneratesVariableDeclaration ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };
        node.Attributes["Text"] = "Submit";

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "myButton", factory);
        
        Assert.Single (statements);
        var generated = statements[0].ToString ();
        Assert.Contains ("varmyButton", generated);
    }

    [Fact]
    public void ButtonGenerator_WithMultipleProperties_GeneratesAllProperties ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };
        node.Attributes["Text"] = "Save";
        node.Attributes["X"] = "10";
        node.Attributes["Y"] = "5";
        node.Attributes["Width"] = "20";
        node.Attributes["Height"] = "3";
        node.Attributes["Enabled"] = "true";

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "saveButton", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=\"Save\"", generated);
        Assert.Contains ("X=10", generated);
        Assert.Contains ("Y=5", generated);
        Assert.Contains ("Width=20", generated);
        Assert.Contains ("Height=3", generated);
        Assert.Contains ("Enabled=true", generated);
    }

    [Fact]
    public void ButtonGenerator_WithNoAttributes_GeneratesEmptyInitializer ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "emptyButton", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("newButton()", generated);
        Assert.Single (statements);
    }

    [Fact]
    public void ButtonGenerator_WithTextOnly_GeneratesTextProperty ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };
        node.Attributes["Text"] = "Click here";

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "button1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=", generated);
        Assert.Contains ("Click here", generated);
    }

    [Fact]
    public void ButtonGenerator_WithPosViewReference_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "Button" };
        node.Attributes["Text"] = "Login";
        node.Attributes["X"] = "{Right _usernameLabel + 1}";
        node.Attributes["Y"] = "{Center}";

        var generator = new ButtonGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "button1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Right(_usernameLabel)+1", generated);
        Assert.Contains ("Y=Pos.Center()", generated);
    }
}
