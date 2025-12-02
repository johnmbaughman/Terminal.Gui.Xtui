using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

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
}
