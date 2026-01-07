using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Terminal.Gui.Xtui.Generator;
using Xunit.Abstractions;

namespace Terminal.Gui.Xtui.RoslynTests;

public class SmokeTests
{
    private readonly ITestOutputHelper _output;

    public SmokeTests (ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Generator_CanBeInstantiated ()
    {
        var generator = new XtuiGenerator ();
        Assert.NotNull (generator);
    }

    [Fact]
    public void Generator_CanBeWrappedAsSourceGenerator ()
    {
        var generator = new XtuiGenerator ();
        var sourceGenerator = generator.AsSourceGenerator ();
        Assert.NotNull (sourceGenerator);
    }

    [Fact]
    public void XtuiLoader_CanParseSimpleXml ()
    {
        var xml = "<Window Title=\"Test\"><Label Text=\"Hello\" /></Window>";
        var node = XtuiLoader.LoadFromString (xml);

        Assert.Equal ("Terminal.Gui.Views.Window", node.ElementTypeName);
        Assert.Equal ("Test", node.Attributes ["Title"]);
        Assert.Single (node.Children);
        Assert.Equal ("Terminal.Gui.Views.Label", node.Children [0].ElementTypeName);
    }

    [Fact]
    public void GeneratorDriver_CanRunGenerator ()
    {
        // Create a minimal compilation
        var source = "class Test { }";
        var syntaxTree = CSharpSyntaxTree.ParseText (source);

        var references = new []
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        };

        var compilation = CSharpCompilation.Create (
            "TestCompilation",
            new [] { syntaxTree },
            references,
            new CSharpCompilationOptions (OutputKind.DynamicallyLinkedLibrary));

        // Create generator
        var generator = new XtuiGenerator ().AsSourceGenerator ();
        GeneratorDriver driver = CSharpGeneratorDriver.Create (new [] { generator });

        // Run the driver
        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        // Just verify it runs without crashing
        Assert.NotNull (outputCompilation);

        var result = driver.GetRunResult ();
        _output.WriteLine ($"Generated {result.GeneratedTrees.Length} files");
        _output.WriteLine ($"Diagnostics: {result.Diagnostics.Length}");

        foreach (var diag in result.Diagnostics)
        {
            _output.WriteLine ($"  {diag.Id}: {diag.GetMessage ()}");
        }
    }
}

