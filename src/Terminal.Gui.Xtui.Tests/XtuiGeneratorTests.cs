using System;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

public class XtuiGeneratorTests
{
    [Fact]
    public void XtuiLoader_Parses_SimpleRootAndAttributes ()
    {
        var xtui = "<Window Title=\"Main\" Width=\"80\">\n  <Label Text=\"Hello\" />\n</Window>";

        var root = XtuiLoader.LoadFromString (xtui);

        Assert.Equal ("Window", root.ElementTypeName);
        Assert.True (root.Attributes.ContainsKey ("Title"));
        Assert.Equal ("Main", root.Attributes ["Title"]);
        Assert.Single (root.Children);
        Assert.Equal ("Label", root.Children [0].ElementTypeName);
        Assert.Equal ("Hello", root.Children [0].Attributes ["Text"]);
    }

    [Fact]
    public void GeneratorFactory_Returns_SpecificGenerators_And_GenericFallback ()
    {
        var factory = new GeneratorFactory ();

        var windowGen = factory.GetGenerator ("Window");
        var labelGen = factory.GetGenerator ("Label");
        var buttonGen = factory.GetGenerator ("Button");
        var unknownGen = factory.GetGenerator ("FooBar");

        Assert.NotNull (windowGen);
        Assert.NotNull (labelGen);
        Assert.NotNull (buttonGen);
        Assert.NotNull (unknownGen);

        Assert.Equal (typeof (WindowGenerator), windowGen.GetType ());
        Assert.Equal (typeof (LabelGenerator), labelGen.GetType ());
        Assert.Equal (typeof (ButtonGenerator), buttonGen.GetType ());
        Assert.Equal (typeof (GenericGenerator), unknownGen.GetType ());
    }

    [Fact]
    public void WindowGenerator_GenerateClass_ProducesInitializeComponentAndUsings ()
    {
        var root = new ElementNode { ElementTypeName = "Window" };
        var label = new ElementNode
        {
            ElementTypeName = "Label",
            Attributes =
            {
                ["Text"] = "Hello"
            }
        };
        root.Children.Add (label);

        var generator = new WindowGenerator ();
        var factory = new GeneratorFactory ();

        var code = generator.GenerateClass (root, "MyNamespace", "MyWindow", factory);

        Assert.Contains ("InitializeComponent()", code);
        Assert.Contains ("using Terminal.Gui.Views;", code);
        Assert.Contains ("using Terminal.Gui.ViewBase;", code);
        Assert.Contains ("public partial class MyWindow : Window", code);
        Assert.Contains ("this.Add(new Label", code);
    }
}
