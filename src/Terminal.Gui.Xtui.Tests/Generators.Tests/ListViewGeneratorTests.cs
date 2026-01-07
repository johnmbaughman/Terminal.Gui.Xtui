using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator;using Terminal.Gui.Xtui.Generator.Helpers;

namespace Terminal.Gui.Xtui.Tests.Generators.Tests;

public class ListViewGeneratorTests
{
    [Fact]
    public void ListViewGenerator_GeneratesObjectInitializer_WithExpectedProperties ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        node.Attributes["Height"] = "{Auto}";
        node.Attributes["Width"] = "{Auto}";

        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "listView1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("newListView", generated);
        Assert.Contains ("Height=", generated);
        Assert.Contains ("Width=", generated);
    }

    [Fact]
    public void ListViewGenerator_IsRegisteredInFactory ()
    {
        var factory = new GeneratorFactory ();
        var generator = factory.GetGenerator ("ListView");

        Assert.NotNull (generator);
        Assert.IsType<ListViewGenerator> (generator);
    }

    [Fact]
    public void ListViewGenerator_GeneratesVariableDeclaration ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "myListView", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("varmyListView", generated);
        Assert.Contains ("newListView", generated);
    }

    [Fact]
    public void ListViewGenerator_WithMultipleProperties_GeneratesAllProperties ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        node.Attributes["Height"] = "{Auto}";
        node.Attributes["Width"] = "{Auto}";
        node.Attributes["Enabled"] = "true";
        node.Attributes["Visible"] = "true";

        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "listView1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Height=", generated);
        Assert.Contains ("Width=", generated);
        Assert.Contains ("Enabled=true", generated);
        Assert.Contains ("Visible=true", generated);
    }

    [Fact]
    public void ListViewGenerator_WithNoAttributes_GeneratesEmptyInitializer ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "listView1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("varlistView1", generated);
        Assert.Contains ("newListView()", generated);
    }

    [Fact]
    public void ListViewGenerator_WithDimensions_GeneratesDimensionProperties ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        node.Attributes["Y"] = "{AnchorEnd}";
        node.Attributes["Height"] = "{Auto}";
        node.Attributes["Width"] = "{Auto}";

        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "listView1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("Y=", generated);
        Assert.Contains ("Height=", generated);
        Assert.Contains ("Width=", generated);
    }

    [Fact]
    public void ListViewGenerator_WithPosViewReference_GeneratesPosCode ()
    {
        var node = new ElementNode { ElementTypeName = "ListView" };
        node.Attributes["X"] = "{Left _container}";
        node.Attributes["Y"] = "{Bottom _header + 1}";
        node.Attributes["Width"] = "{Fill - 2}";
        node.Attributes["Height"] = "{Fill - 3}";

        var generator = new ListViewGenerator ();
        var factory = new GeneratorFactory ();

        var statements = generator.GenerateStatements (node, "listView1", factory);
        var generated = string.Concat (statements.Select (s => s.ToString ()));

        Assert.Contains ("X=Pos.Left(_container)", generated);
        Assert.Contains ("Y=Pos.Bottom(_header)+1", generated);
        Assert.Contains ("Width=Dim.Fill()-2", generated);
        Assert.Contains ("Height=Dim.Fill()-3", generated);
    }

    [Fact]
    public void ListViewGenerator_WithFillDimensions_GeneratesFillCode()
    {
        string xtui = @"<ListView xmlns=""http://schemas.terminal.gui/xtui"" 
                                  Width=""{Fill}"" 
                                  Height=""{Fill}"" />";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new ListViewGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "listView1", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("Width=Dim.Fill()", generated);
        Assert.Contains("Height=Dim.Fill()", generated);
    }

    [Fact]
    public void ListViewGenerator_WithPositioning_GeneratesAllCode()
    {
        string xtui = @"<ListView xmlns=""http://schemas.terminal.gui/xtui"" 
                                  X=""5"" 
                                  Y=""10"" 
                                  Width=""40"" 
                                  Height=""15"" />";
        
        ElementNode node = XtuiLoader.LoadFromString(xtui);
        var generator = new ListViewGenerator();
        var factory = new GeneratorFactory();
        var statements = generator.GenerateStatements(node, "listView1", factory);
        var generated = string.Concat(statements.Select(s => s.ToString()));

        Assert.Contains("X=5", generated);
        Assert.Contains("Y=10", generated);
        Assert.Contains("Width=40", generated);
        Assert.Contains("Height=15", generated);
    }
}
