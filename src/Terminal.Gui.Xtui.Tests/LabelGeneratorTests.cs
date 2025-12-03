using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

public class LabelGeneratorTests
{
    [Fact]
    public void LabelGenerator_GeneratesObjectInitializer_WithExpectedProperties ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Hello World";
        node.Attributes["Visible"] = "true";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label0", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("newLabel", generated);
        Assert.Contains ("Text=", generated);
        Assert.Contains ("Hello World", generated);
        Assert.Contains ("Visible=true", generated);
    }

    [Fact]
    public void LabelGenerator_IsRegisteredInFactory ()
    {
        var factory = new GeneratorFactory ();
        var generator = factory.GetGenerator ("Label");

        Assert.NotNull (generator);
        Assert.IsType<LabelGenerator> (generator);
    }

    [Fact]
    public void LabelGenerator_GeneratesVariableDeclaration ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Label Text";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "myLabel", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("varmyLabel", generated);
        Assert.Contains ("newLabel", generated);
    }

    [Fact]
    public void LabelGenerator_WithMultipleProperties_GeneratesAllProperties ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Username:";
        node.Attributes["Enabled"] = "true";
        node.Attributes["Visible"] = "true";
        node.Attributes["X"] = "0";
        node.Attributes["Y"] = "5";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=", generated);
        Assert.Contains ("Username:", generated);
        Assert.Contains ("Enabled=true", generated);
        Assert.Contains ("Visible=true", generated);
        Assert.Contains ("X=", generated);
        Assert.Contains ("Y=", generated);
    }

    [Fact]
    public void LabelGenerator_WithNoAttributes_GeneratesEmptyInitializer ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("varlabel1", generated);
        Assert.Contains ("newLabel()", generated);
    }

    [Fact]
    public void LabelGenerator_WithTextOnly_GeneratesTextProperty ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Click here";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=", generated);
        Assert.Contains ("Click here", generated);
    }

    [Fact]
    public void LabelGenerator_WithPosExpressions_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Label";
        node.Attributes["X"] = "{Center}";
        node.Attributes["Y"] = "{AnchorEnd - 5}";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Center()", generated);
        Assert.Contains ("Y=Pos.AnchorEnd()-5", generated);
    }

    [Fact]
    public void LabelGenerator_WithDimExpressions_GeneratesDimCode ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Label";
        node.Attributes["Width"] = "{Fill}";
        node.Attributes["Height"] = "{Auto}";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Width=Dim.Fill()", generated);
        Assert.Contains ("Height=Dim.Auto()", generated);
    }

    [Fact]
    public void LabelGenerator_WithPosViewReference_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "Label" };
        node.Attributes["Text"] = "Password:";
        node.Attributes["X"] = "{Left _usernameLabel}";
        node.Attributes["Y"] = "{Bottom _usernameLabel + 1}";

        var generator = new LabelGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "label1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Left(_usernameLabel)", generated);
        Assert.Contains ("Y=Pos.Bottom(_usernameLabel)+1", generated);
    }
}
