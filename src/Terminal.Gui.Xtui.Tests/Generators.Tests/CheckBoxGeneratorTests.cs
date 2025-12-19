using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class CheckBoxGeneratorTests
{
    [Fact]
    public void CheckBoxGenerator_GeneratesObjectInitializer_WithExpectedProperties ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Accept Terms";
        node.Attributes["Enabled"] = "true";
        node.Attributes["Visible"] = "false";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "checkBox0", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("newCheckBox", generated);
        Assert.Contains ("Text=", generated);
        Assert.Contains ("Accept Terms", generated);
        Assert.Contains ("Enabled=true", generated);
        Assert.Contains ("Visible=false", generated);
    }

    [Fact]
    public void CheckBoxGenerator_IsRegisteredInFactory ()
    {
        var factory = new GeneratorFactory ();
        var generator = factory.GetGenerator ("CheckBox");

        Assert.NotNull (generator);
        Assert.IsType<CheckBoxGenerator> (generator);
    }

    [Fact]
    public void CheckBoxGenerator_GeneratesVariableDeclaration ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Option 1";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "myCheckBox", factory);
        
        Assert.Single (statements);
        var generated = statements[0].ToString ();
        Assert.Contains ("varmyCheckBox", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithCheckedState_GeneratesEnumExpression ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "I agree";
        node.Attributes["CheckedState"] = "Checked";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "checkbox1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("CheckedState=", generated);
        Assert.Contains ("Terminal.Gui.Views.CheckState.Checked", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithUncheckedState_GeneratesEnumExpression ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Unchecked Option";
        node.Attributes["CheckedState"] = "UnChecked";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "checkbox2", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("CheckedState=", generated);
        Assert.Contains ("Terminal.Gui.Views.CheckState.UnChecked", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithNoneState_GeneratesEnumExpression ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Tristate";
        node.Attributes["CheckedState"] = "None";
        node.Attributes["AllowCheckStateNone"] = "true";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "checkbox3", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("CheckedState=", generated);
        Assert.Contains ("Terminal.Gui.Views.CheckState.None", generated);
        Assert.Contains ("AllowCheckStateNone=true", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithRadioStyle_GeneratesProperty ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Radio Option";
        node.Attributes["RadioStyle"] = "true";
        node.Attributes["CheckedState"] = "Checked";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "radio1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("RadioStyle=true", generated);
        Assert.Contains ("Terminal.Gui.Views.CheckState.Checked", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithAllProperties_GeneratesComplete ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Full Featured";
        node.Attributes["CheckedState"] = "Checked";
        node.Attributes["AllowCheckStateNone"] = "true";
        node.Attributes["RadioStyle"] = "false";
        node.Attributes["Enabled"] = "true";
        node.Attributes["Visible"] = "true";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "fullCheckBox", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Text=", generated);
        Assert.Contains ("Full Featured", generated);
        Assert.Contains ("CheckedState=Terminal.Gui.Views.CheckState.Checked", generated);
        Assert.Contains ("AllowCheckStateNone=true", generated);
        Assert.Contains ("RadioStyle=false", generated);
        Assert.Contains ("Enabled=true", generated);
        Assert.Contains ("Visible=true", generated);
    }

    [Fact]
    public void CheckBoxGenerator_WithPosViewReference_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "CheckBox" };
        node.Attributes["Text"] = "Option";
        node.Attributes["X"] = "{Right _label + 2}";
        node.Attributes["Y"] = "{Bottom _prevControl - 1}";

        var generator = new CheckBoxGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "checkBox1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Right(_label)+2", generated);
        Assert.Contains ("Y=Pos.Bottom(_prevControl)-1", generated);
    }
}
