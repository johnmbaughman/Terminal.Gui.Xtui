using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Terminal.Gui.Xtui.RoslynTests;

public class IncrementalGeneratorTests
{
    private static CSharpCompilation CreateCompilation (string source, bool includeTerminalGui = false, params MetadataReference [] references)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText (source);

        var defaultReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        };

        // Add Terminal.Gui reference if requested
        if (includeTerminalGui)
        {
            var terminalGuiReference = GetTerminalGuiReference ();
            if (terminalGuiReference != null)
            {
                defaultReferences.Add (terminalGuiReference);
            }
        }

        return CSharpCompilation.Create (
            "TestCompilation",
            new [] { syntaxTree },
            defaultReferences.Concat (references),
            new CSharpCompilationOptions (OutputKind.DynamicallyLinkedLibrary));
    }

    private static MetadataReference? GetTerminalGuiReference ()
    {
        // Try to locate Terminal.Gui.dll from the solution
        // Start from the test project directory and navigate up to find Terminal.Gui project
        var testProjectDir = Directory.GetCurrentDirectory ();

        // Navigate up to src directory
        var srcDir = testProjectDir;
        while (!string.IsNullOrEmpty (srcDir) && Path.GetFileName (srcDir) != "src")
        {
            srcDir = Path.GetDirectoryName (srcDir);
        }

        if (string.IsNullOrEmpty (srcDir))
        {
            return null;
        }

        // Look for Terminal.Gui assembly in the Terminal.Gui project's bin directory
        // Try multiple potential paths (different configurations and frameworks)
        var potentialPaths = new []
        {
            Path.Combine(srcDir, "Terminal.Gui", "Terminal.Gui", "bin", "Debug", "net8.0", "Terminal.Gui.dll"),
            Path.Combine(srcDir, "Terminal.Gui", "Terminal.Gui", "bin", "Release", "net8.0", "Terminal.Gui.dll"),
            Path.Combine(srcDir, "Terminal.Gui", "Terminal.Gui", "bin", "Debug", "net9.0", "Terminal.Gui.dll"),
            Path.Combine(srcDir, "Terminal.Gui", "Terminal.Gui", "bin", "Release", "net9.0", "Terminal.Gui.dll"),
        };

        foreach (var path in potentialPaths)
        {
            if (File.Exists (path))
            {
                return MetadataReference.CreateFromFile (path);
            }
        }

        return null;
    }

    private static GeneratorDriver CreateDriver (CSharpCompilation compilation, params (string fileName, string content) [] additionalTexts)
    {
        var generator = new XtuiGenerator ().AsSourceGenerator ();

        var driver = CSharpGeneratorDriver.Create (
            generators: new [] { generator },
            additionalTexts: additionalTexts.Select (t =>
                (AdditionalText)new InMemoryAdditionalText (t.fileName, t.content)).ToImmutableArray (),
            parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First ().Options);

        return driver;
    }

    [Fact]
    public void Generator_WithSimpleWindow_GeneratesInitializeComponent ()
    {
        var xtuiSource = """
            <Window Title="Main Window">
                <Label Text="Hello World" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyWindow
                {
                    public MyWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MyWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // Should have no errors
        Assert.Empty (diagnostics.Where (d => d.Severity == DiagnosticSeverity.Error));

        // Should have generated one file
        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify key elements in generated code
        Assert.Contains ("InitializeComponent()", generatedCode);
        Assert.Contains ("using Terminal.Gui.Views;", generatedCode);
        Assert.Contains ("using Terminal.Gui.ViewBase;", generatedCode);
        Assert.Contains ("public partial class MyWindow : Window", generatedCode);
        Assert.Contains ("this.Add(new Label", generatedCode);
        Assert.Contains ("Text = \"Hello World\"", generatedCode);
    }

    [Fact]
    public void Generator_WithSimpleToplevel_GeneratesInitializeComponent ()
    {
        var xtuiSource = """
            <TopLevel>
                <Label Text="Hello TopLevel" />
            </TopLevel>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyTop
                {
                    public MyTop()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MyTop.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // Should have no errors
        Assert.Empty (diagnostics.Where (d => d.Severity == DiagnosticSeverity.Error));

        // Should have generated one file
        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify key elements in generated code
        Assert.Contains ("InitializeComponent()", generatedCode);
        Assert.Contains ("using Terminal.Gui.Views;", generatedCode);
        Assert.Contains ("public partial class MyTop : Toplevel", generatedCode);
        Assert.Contains ("this.Add(new Label", generatedCode);
        Assert.Contains ("Text = \"Hello TopLevel\"", generatedCode);
    }

    [Fact]
    public void Generator_RecordsGeneratedFileBookkeeping ()
    {
        var xtuiSource = """
            <Window Title="Main Window">
                <Label Text="Hello World" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyWindow
                {
                    public MyWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MyWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // Ensure generation ran
        Assert.Single (runResult.GeneratedTrees);

        // Use reflection to inspect the bookkeeping helper
        var genAssembly = typeof (XtuiGenerator).Assembly;
        var bookkeepingType = genAssembly.GetType ("Terminal.Gui.Xtui.GeneratedFileBookkeeping");
        Assert.NotNull (bookkeepingType);

        var countProp = bookkeepingType.GetProperty ("Count", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        Assert.NotNull (countProp);

        int count = (int)countProp.GetValue (null)!;
        Assert.True (count >= 1, "Bookkeeping should contain at least one recorded generated file");

        var getMethod = bookkeepingType.GetMethod ("Get", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        Assert.NotNull (getMethod);

        var info = getMethod.Invoke (null, new object?[] { "MyWindow.xtui" });
        Assert.NotNull (info);

        var infoType = info.GetType ();
        var classNameProp = infoType.GetProperty ("ClassName");
        Assert.NotNull (classNameProp);

        var className = (string)classNameProp.GetValue (info)!;
        Assert.Equal ("MyWindow", className);
    }

    [Fact]
    public void Generator_EmitsCollisionDiagnostic_ForDuplicateGeneratedIdentity ()
    {
        // Two different input paths with the same filename will generate the same class name
        var xtuiSourceA = """
            <Window Title="A">
                <Label Text="A" />
            </Window>
            """;

        var xtuiSourceB = """
            <Window Title="B">
                <Label Text="B" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyWindow
                {
                    public MyWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);

        // Provide two additional texts with same filename but different directories
        var driver = CreateDriver (compilation,
            ("dirA/MyWindow.xtui", xtuiSourceA),
            ("dirB/MyWindow.xtui", xtuiSourceB));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // There should be a diagnostic with id XTUI003
        var diag = runResult.Diagnostics.FirstOrDefault (d => d.Id == "XTUI003");
        Assert.NotNull (diag);
        Assert.Equal (DiagnosticSeverity.Warning, diag.Severity);
    }

    [Fact]
    public void Generator_WithoutTerminalGuiReference_DoesNotGenerate ()
    {
        var xtuiSource = """
            <Window Title="Main">
                <Label Text="Test" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyWindow
                {
                }
            }
            """;

        var compilation = CreateCompilation (userCode); // No Terminal.Gui reference
        var driver = CreateDriver (compilation, ("MyWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // Should not generate any files (no Terminal.Gui reference)
        Assert.Empty (runResult.GeneratedTrees);
    }

    [Fact]
    public void Generator_WithInvalidXtui_ReportsDiagnostic ()
    {
        var xtuiSource = """
            <Window Title="Main"
                <Label Text="Unclosed tag"
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MyWindow
                {
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MyWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        // Should have a diagnostic error
        var generatorDiagnostics = runResult.Diagnostics;
        // The generator should report at least one diagnostic for the invalid XTUI input.
        Assert.NotEmpty (generatorDiagnostics);
    }

    [Fact]
    public void Generator_WithMultipleChildren_GeneratesMultipleAddCalls ()
    {
        var xtuiSource = """
            <Window Title="Multi">
                <Label Text="First" />
                <Button Text="Click Me" />
                <Label Text="Second" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MultiWindow
                {
                    public MultiWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MultiWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify all three children are added
        Assert.Contains ("this.Add(new Label", generatedCode);
        Assert.Contains ("Text = \"First\"", generatedCode);
        Assert.Contains ("this.Add(new Button", generatedCode);
        Assert.Contains ("Text = \"Click Me\"", generatedCode);
        Assert.Contains ("Text = \"Second\"", generatedCode);

        // Count the number of Add calls (should be 3)
        var addCount = System.Text.RegularExpressions.Regex.Matches (generatedCode, @"this\.Add\(").Count;
        Assert.Equal (3, addCount);
    }

    [Fact]
    public void Generator_WithNoPartialClass_UsesDefaultNamespace ()
    {
        var xtuiSource = """
            <Window Title="Standalone">
            </Window>
            """;

        // No user code with partial class - just empty compilation
        var compilation = CreateCompilation ("// empty", includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("Standalone.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Should use default namespace "Generated"
        Assert.Contains ("namespace Generated", generatedCode);
        Assert.Contains ("public partial class Standalone : Window", generatedCode);
    }

    [Fact]
    public void Generator_WithCommentsInXtui_IgnoresComments ()
    {
        var xtuiSource = """
            <Window Title="Main">
                <!-- This is a comment -->
                <Label Text="Visible" />
                <!-- <Label Text="Commented Out" /> -->
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class CommentTest
                {
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("CommentTest.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Should only have one Add call for the visible label
        Assert.Contains ("Text = \"Visible\"", generatedCode);
        Assert.DoesNotContain ("Text = \"Commented Out\"", generatedCode);
        Assert.DoesNotContain ("This is a comment", generatedCode);

        var addCount = System.Text.RegularExpressions.Regex.Matches (generatedCode, @"this\.Add\(").Count;
        Assert.Equal (1, addCount);
    }

    [Fact]
    public void Generator_WithCheckBox_GeneratesWithCheckedState ()
    {
        var xtuiSource = """
            <Window Title="CheckBox Test">
                <CheckBox Text="I Accept" CheckedState="Checked" />
                <CheckBox Text="Optional" CheckedState="UnChecked" AllowCheckStateNone="true" />
                <CheckBox Text="Radio Style" RadioStyle="true" CheckedState="Checked" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class CheckBoxWindow
                {
                    public CheckBoxWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("CheckBoxWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify CheckBox elements are generated
        Assert.Contains ("new CheckBox", generatedCode);
        Assert.Contains ("Text = \"I Accept\"", generatedCode);
        Assert.Contains ("CheckedState = Terminal.Gui.Views.CheckState.Checked", generatedCode);
        Assert.Contains ("CheckedState = Terminal.Gui.Views.CheckState.UnChecked", generatedCode);
        Assert.Contains ("AllowCheckStateNone = true", generatedCode);
        Assert.Contains ("RadioStyle = true", generatedCode);

        // Count the number of Add calls (should be 3)
        var addCount = System.Text.RegularExpressions.Regex.Matches (generatedCode, @"this\.Add\(").Count;
        Assert.Equal (3, addCount);
    }

    /// <summary>
    /// Helper class for providing additional text files to the generator during testing.
    /// </summary>
    private class InMemoryAdditionalText : AdditionalText
    {
        private readonly string _text;

        public InMemoryAdditionalText (string path, string text)
        {
            Path = path;
            _text = text;
        }

        public override string Path { get; }

        public override SourceText GetText (CancellationToken cancellationToken = default)
        {
            return SourceText.From (_text, Encoding.UTF8);
        }
    }
}

