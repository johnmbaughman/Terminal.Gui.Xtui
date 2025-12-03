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
            <Toplevel>
                <Label Text="Hello Toplevel" />
            </Toplevel>
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
        // TopLevel creates child but doesn't add it (no Id specified)
        Assert.Contains ("var label0 = new Label", generatedCode);
        Assert.Contains ("Text = \"Hello Toplevel\"", generatedCode);
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

    [Fact]
    public void Generator_WithMenuBar_GeneratesMenuBar ()
    {
        var xtuiSource = """
            <Window Title="Menu Test">
                <MenuBar Id="_menuBar" />
                <Label Text="Content" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MenuWindow
                {
                    public MenuWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MenuWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify MenuBar is generated
        Assert.Contains ("MenuBar", generatedCode);
        Assert.Contains ("private MenuBar? _menuBar;", generatedCode);
        
        // Verify the Label is also added
        Assert.Contains ("Content", generatedCode);
    }

    [Fact]
    public void Generator_WithTextField_GeneratesSecretProperty ()
    {
        var xtuiSource = """
            <Window Title="Login">
                <Label Text="Username:" />
                <TextField Id="_usernameField" />
                <Label Text="Password:" />
                <TextField Id="_passwordField" Secret="true" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class LoginWindow
                {
                    public LoginWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("LoginWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify TextField elements
        Assert.Contains ("new TextField", generatedCode);
        Assert.Contains ("Secret = true", generatedCode);
        
        // Verify fields are declared for controls with Id
        Assert.Contains ("private TextField? _usernameField;", generatedCode);
        Assert.Contains ("private TextField? _passwordField;", generatedCode);
        
        // Verify field assignments
        Assert.Contains ("_usernameField = new TextField", generatedCode);
        Assert.Contains ("_passwordField = new TextField", generatedCode);
    }

    [Fact]
    public void Generator_WithListView_GeneratesCorrectly ()
    {
        var xtuiSource = """
            <Window Title="List Window">
                <ListView Id="_listView" Width="40" Height="10" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class ListWindow
                {
                    public ListWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("ListWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify ListView is generated
        Assert.Contains ("new ListView", generatedCode);
        Assert.Contains ("Width = 40", generatedCode);
        Assert.Contains ("Height = 10", generatedCode);
        
        // Verify field declaration
        Assert.Contains ("private ListView? _listView;", generatedCode);
    }

    [Fact]
    public void Generator_WithControlsWithIds_GeneratesPrivateFields ()
    {
        var xtuiSource = """
            <Window Title="Fields Test">
                <Label Id="_statusLabel" Text="Ready" />
                <Button Id="_okButton" Text="OK" />
                <Button Id="_cancelButton" Text="Cancel" />
                <Label Text="No ID here" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class FieldsWindow
                {
                    public FieldsWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("FieldsWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify fields are declared only for controls with Id
        Assert.Contains ("private Label? _statusLabel;", generatedCode);
        Assert.Contains ("private Button? _okButton;", generatedCode);
        Assert.Contains ("private Button? _cancelButton;", generatedCode);
        
        // Verify field assignments
        Assert.Contains ("_statusLabel = new Label", generatedCode);
        Assert.Contains ("_okButton = new Button", generatedCode);
        Assert.Contains ("_cancelButton = new Button", generatedCode);
        
        // Verify all controls are added
        var addCount = System.Text.RegularExpressions.Regex.Matches (generatedCode, @"this\.Add\(").Count;
        Assert.Equal (4, addCount);
    }

    [Fact]
    public void Generator_WithPosAndDimExpressions_GeneratesCorrectSyntax ()
    {
        var xtuiSource = """
            <Window Title="Layout Test">
                <Label Id="_label1" Text="First" X="0" Y="0" Width="20" Height="1" />
                <Label Id="_label2" Text="Second" X="{Right _label1 + 2}" Y="0" Width="{Fill - 5}" Height="1" />
                <Button Id="_button" Text="Centered" X="{Center}" Y="{Center}" Width="10" Height="1" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class LayoutWindow
                {
                    public LayoutWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("LayoutWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify Pos expressions are generated
        Assert.Contains ("Pos.Right(_label1)", generatedCode);
        Assert.Contains ("Pos.Center()", generatedCode);
        
        // Verify Dim expressions are generated
        Assert.Contains ("Dim.Fill()", generatedCode);
        
        // Verify field declarations for controls with IDs
        Assert.Contains ("private Label? _label1;", generatedCode);
        Assert.Contains ("private Label? _label2;", generatedCode);
        Assert.Contains ("private Button? _button;", generatedCode);
    }

    [Fact]
    public void Generator_WithPercentageExpressions_GeneratesCorrectly ()
    {
        var xtuiSource = """
            <Window Title="Percentage Test">
                <Label Text="50% wide" X="0" Y="0" Width="50%" Height="1" />
                <Button Text="Centered" X="25%" Y="50%" Width="50%" Height="3" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class PercentWindow
                {
                    public PercentWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("PercentWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify percentage expressions are converted
        Assert.Contains ("Width = Dim.Percent(50)", generatedCode);
        Assert.Contains ("X = Pos.Percent(25)", generatedCode);
        Assert.Contains ("Y = Pos.Percent(50)", generatedCode);
    }

    [Fact]
    public void Generator_WithBooleanProperties_GeneratesCorrectValues ()
    {
        var xtuiSource = """
            <Window Title="Boolean Test">
                <Label Text="Visible" Visible="true" Enabled="false" />
                <Button Text="Can Focus" CanFocus="TRUE" IsDefault="true" />
                <CheckBox Text="Check" AllowCheckStateNone="false" RadioStyle="False" />
            </Window>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class BoolWindow
                {
                    public BoolWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("BoolWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify boolean values are lowercased (C# convention)
        Assert.Contains ("Visible = true", generatedCode);
        Assert.Contains ("Enabled = false", generatedCode);
        Assert.Contains ("CanFocus = true", generatedCode);
        Assert.Contains ("IsDefault = true", generatedCode);
        Assert.Contains ("AllowCheckStateNone = false", generatedCode);
        Assert.Contains ("RadioStyle = false", generatedCode);
    }

    [Fact]
    public void Generator_WithToplevelAndMenuBar_AddsMenuBarAutomatically ()
    {
        var xtuiSource = """
            <Toplevel>
                <MenuBar>
                    <MenuBarItem Title="_File">
                        <MenuItem Title="_Exit" />
                    </MenuBarItem>
                </MenuBar>
                <Label Text="Main Content" />
            </Toplevel>
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class MainApp
                {
                    public MainApp()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("MainApp.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Verify MenuBar is created
        Assert.Contains ("new MenuBar", generatedCode);
        
        // TopLevel should have special handling for MenuBar (auto-add)
        // Verify MenuBar is added to the Toplevel
        var menuBarAddMatch = System.Text.RegularExpressions.Regex.Match (generatedCode, @"this\.Add\(menubar\d+\)");
        Assert.True (menuBarAddMatch.Success, "MenuBar should be added to Toplevel");
    }

    [Fact]
    public void Generator_WithEmptyWindow_GeneratesEmptyInitializeComponent ()
    {
        var xtuiSource = """
            <Window Title="Empty" />
            """;

        var userCode = """
            namespace MyApp
            {
                public partial class EmptyWindow
                {
                    public EmptyWindow()
                    {
                        InitializeComponent();
                    }
                }
            }
            """;

        var compilation = CreateCompilation (userCode, includeTerminalGui: true);
        var driver = CreateDriver (compilation, ("EmptyWindow.xtui", xtuiSource));

        driver = driver.RunGeneratorsAndUpdateCompilation (compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult ();

        Assert.Single (runResult.GeneratedTrees);

        var generatedCode = runResult.GeneratedTrees.First ().ToString ();

        // Should generate InitializeComponent but with no statements
        Assert.Contains ("private void InitializeComponent()", generatedCode);
        
        // Should not have any Add calls
        Assert.DoesNotContain ("this.Add(", generatedCode);
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

