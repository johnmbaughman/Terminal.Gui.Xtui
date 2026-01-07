using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator;using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class TextFieldGeneratorTests
{
    [Fact]
    public void TextFieldGenerator_GeneratesObjectInitializer_WithExpectedProperties ()
    {
        // Arrange
        var node = new ElementNode { ElementTypeName = "TextField" };
        node.Attributes["Text"] = "Enter text";
        node.Attributes["Secret"] = "false";

        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        // Act
        var statements = generator.GenerateStatements (node, "textField1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        // Assert
        Assert.Contains ("newTextField", generated);
        Assert.Contains ("Text=", generated);
        Assert.Contains ("Enter text", generated);
        Assert.Contains ("Secret=false", generated);
    }

    [Fact]
    public void TextFieldGenerator_IsRegisteredInFactory ()
    {
        var factory = new GeneratorFactory ();
        var generator = factory.GetGenerator ("TextField");

        Assert.NotNull (generator);
        Assert.IsType<TextFieldGenerator> (generator);
    }

    [Fact]
    public void TextFieldGenerator_GeneratesVariableDeclaration ()
    {
        var node = new ElementNode { ElementTypeName = "TextField" };
        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "myTextField", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("varmyTextField", generated);
        Assert.Contains ("newTextField", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithMultipleProperties_GeneratesAllProperties ()
    {
        var node = new ElementNode { ElementTypeName = "TextField" };
        node.Attributes["Text"] = "Username";
        node.Attributes["Secret"] = "true";
        node.Attributes["Enabled"] = "true";

        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "textField1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=", generated);
        Assert.Contains ("Username", generated);
        Assert.Contains ("Secret=true", generated);
        Assert.Contains ("Enabled=true", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithNoAttributes_GeneratesEmptyInitializer ()
    {
        var node = new ElementNode { ElementTypeName = "TextField" };
        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "textField1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("vartextField1", generated);
        Assert.Contains ("newTextField()", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithSecretTrue_GeneratesSecretProperty ()
    {
        var node = new ElementNode { ElementTypeName = "TextField" };
        node.Attributes["Secret"] = "true";

        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "passwordField", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Secret=true", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithPosViewReference_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "TextField" };
        node.Attributes["X"] = "{Right _usernameLabel + 1}";
        node.Attributes["Y"] = "{Top _prevField}";
        node.Attributes["Width"] = "{Fill}";

        var generator = new TextFieldGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "textField1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Right(_usernameLabel)+1", generated);
        Assert.Contains ("Y=Pos.Top(_prevField)", generated);
        Assert.Contains ("Width=Dim.Fill()", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithDimExpressions_GeneratesDimCode()
    {
        string xtui = @"<TextField xmlns=""http://schemas.terminal.gui/xtui"" 
                                   Width=""{Fill}"" 
                                   Height=""{Auto}"" />";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new TextFieldGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "textField1", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Width=Dim.Fill()", generated);
        Assert.Contains("Height=Dim.Auto()", generated);
    }

    [Fact]
    public void TextFieldGenerator_WithPositioning_GeneratesAllCode()
    {
        string xtui = @"<TextField xmlns=""http://schemas.terminal.gui/xtui"" 
                                   Text=""Enter password"" 
                                   Secret=""true"" 
                                   X=""5"" 
                                   Y=""10"" 
                                   Width=""30"" />";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new TextFieldGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "passwordField", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Text=", generated);
        Assert.Contains("Enter password", generated);
        Assert.Contains("Secret=true", generated);
        Assert.Contains("X=5", generated);
        Assert.Contains("Y=10", generated);
        Assert.Contains("Width=30", generated);
    }
}
