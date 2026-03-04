using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Terminal.Gui.Xtui.Generator;
using Xunit.Abstractions;

namespace Terminal.Gui.Xtui.RoslynTests;

public class SmokeTests (ITestOutputHelper output)
{
    [Fact]
    public void Generator_CanBeInstantiated ()
    {
        XtuiGenerator generator = new XtuiGenerator ();
        Assert.NotNull (generator);
    }

    [Fact]
    public void Generator_CanBeWrappedAsSourceGenerator ()
    {
        XtuiGenerator generator = new XtuiGenerator ();
        ISourceGenerator sourceGenerator = generator.AsSourceGenerator ();
        Assert.NotNull (sourceGenerator);
    }

    [Fact]
    public void XtuiLoader_CanParseSimpleXml ()
    {
        const string xml = "<Window Title=\"Test\"><Label Text=\"Hello\" /></Window>";
        ElementNode node = XtuiLoader.LoadFromString (xml);

        Assert.Equal ("Terminal.Gui.Views.Window", node.ElementTypeName);
        Assert.Equal ("Test", node.Attributes ["Title"]);
        Assert.Single (node.Children);
        Assert.Equal ("Terminal.Gui.Views.Label", node.Children [0].ElementTypeName);
    }

    [Fact]
    public void GeneratorDriver_CanRunGenerator ()
    {
        // Create a minimal compilation
        const string source = "class Test { }";
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText (source);

        PortableExecutableReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        ];

        CSharpCompilation compilation = CSharpCompilation.Create (
            "TestCompilation",
            new [] { syntaxTree },
            references,
            new CSharpCompilationOptions (OutputKind.DynamicallyLinkedLibrary));

        // Create generator
        ISourceGenerator generator = new XtuiGenerator ().AsSourceGenerator ();
        GeneratorDriver driver = CSharpGeneratorDriver.Create (generator);

        // Run the driver
        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

        // Just verify it runs without crashing
        Assert.NotNull (outputCompilation);

        GeneratorDriverRunResult result = driver.GetRunResult ();
        output.WriteLine ($"Generated {result.GeneratedTrees.Length} files");
        output.WriteLine ($"Diagnostics: {result.Diagnostics.Length}");

        foreach (Diagnostic? diag in result.Diagnostics)
        {
            output.WriteLine ($"  {diag.Id}: {diag.GetMessage ()}");
        }
    }
}

