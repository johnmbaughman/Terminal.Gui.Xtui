using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui.Benchmarks;

/// <summary>
/// Benchmarks for code generation performance.
/// Tests WindowGenerator and other generators with varying child element counts.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class GeneratorBenchmarks
{
    private ElementNode _windowEmpty = null!;
    private ElementNode _window1Child = null!;
    private ElementNode _window10Children = null!;
    private ElementNode _window50Children = null!;
    private ElementNode _window100Children = null!;
    private ElementNode _checkBoxWindow = null!;
    private ElementNode _mixedControlWindow = null!;
    private ElementNode _button1 = null!;
    private ElementNode _button10Properties = null!;
    private ElementNode _button50Batch = null!;
    private ElementNode _label1 = null!;
    private ElementNode _label10Properties = null!;
    private ElementNode _label50Batch = null!;
    private WindowGenerator _windowGenerator = null!;
    private CheckBoxGenerator _checkBoxGenerator = null!;
    private ButtonGenerator _buttonGenerator = null!;
    private LabelGenerator _labelGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _windowGenerator = new WindowGenerator ();
        _checkBoxGenerator = new CheckBoxGenerator ();
        _buttonGenerator = new ButtonGenerator ();
        _labelGenerator = new LabelGenerator ();
        _factory = new GeneratorFactory ();

        // Empty window
        _windowEmpty = new ElementNode { ElementTypeName = "Window" };
        _windowEmpty.Attributes ["Title"] = "Empty";

        // Window with 1 child
        _window1Child = CreateWindowWithChildren (1);

        // Window with 10 children
        _window10Children = CreateWindowWithChildren (10);

        // Window with 50 children
        _window50Children = CreateWindowWithChildren (50);

        // Window with 100 children
        _window100Children = CreateWindowWithChildren (100);

        // Window with CheckBox controls
        _checkBoxWindow = CreateCheckBoxWindow (50);

        // Window with mixed controls
        _mixedControlWindow = CreateMixedControlWindow (50);

        // Button benchmarks
        _button1 = CreateButton ("Click Me", 5, 10);
        _button10Properties = CreateButtonWithManyProperties ();
        _button50Batch = CreateButtonBatch (50);

        // Label benchmarks
        _label1 = CreateLabel ("Username:", 0, 5);
        _label10Properties = CreateLabelWithManyProperties ();
        _label50Batch = CreateLabelBatch (50);
    }

    [Benchmark (Baseline = true, Description = "Window Empty")]
    public string GenerateWindowEmpty ()
    {
        return _windowGenerator.GenerateClass (_windowEmpty, "Benchmark", "EmptyWindow", _factory);
    }

    [Benchmark (Description = "Window 1 Child")]
    public string GenerateWindow1Child ()
    {
        return _windowGenerator.GenerateClass (_window1Child, "Benchmark", "Window1", _factory);
    }

    [Benchmark (Description = "Window 10 Children")]
    public string GenerateWindow10Children ()
    {
        return _windowGenerator.GenerateClass (_window10Children, "Benchmark", "Window10", _factory);
    }

    [Benchmark (Description = "Window 50 Children")]
    public string GenerateWindow50Children ()
    {
        return _windowGenerator.GenerateClass (_window50Children, "Benchmark", "Window50", _factory);
    }

    [Benchmark (Description = "Window 100 Children")]
    public string GenerateWindow100Children ()
    {
        return _windowGenerator.GenerateClass (_window100Children, "Benchmark", "Window100", _factory);
    }

    [Benchmark (Description = "CheckBox Window (50 CheckBoxes)")]
    public string GenerateCheckBoxWindow ()
    {
        return _windowGenerator.GenerateClass (_checkBoxWindow, "Benchmark", "CheckBoxWindow", _factory);
    }

    [Benchmark (Description = "Mixed Control Window (50 controls)")]
    public string GenerateMixedControlWindow ()
    {
        return _windowGenerator.GenerateClass (_mixedControlWindow, "Benchmark", "MixedWindow", _factory);
    }

    [Benchmark (Description = "Button Simple")]
    public string GenerateButtonSimple ()
    {
        var statements = _buttonGenerator.GenerateStatements (_button1, "btn1", _factory);
        return string.Concat (statements.Select (s => s.ToString ()));
    }

    [Benchmark (Description = "Button With 10 Properties")]
    public string GenerateButtonWithProperties ()
    {
        var statements = _buttonGenerator.GenerateStatements (_button10Properties, "btn2", _factory);
        return string.Concat (statements.Select (s => s.ToString ()));
    }

    [Benchmark (Description = "Button Batch (50 buttons)")]
    public string GenerateButtonBatch ()
    {
        string result = "";
        for (int i = 0; i < 50; i++)
        {
            var statements = _buttonGenerator.GenerateStatements (_button50Batch, $"button{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }

    [Benchmark (Description = "Label Simple")]
    public string GenerateLabelSimple ()
    {
        var statements = _labelGenerator.GenerateStatements (_label1, "label1", _factory);
        return string.Concat (statements.Select (s => s.ToString ()));
    }

    [Benchmark (Description = "Label With Properties")]
    public string GenerateLabelWithProperties ()
    {
        var statements = _labelGenerator.GenerateStatements (_label10Properties, "label1", _factory);
        return string.Concat (statements.Select (s => s.ToString ()));
    }

    [Benchmark (Description = "Label Batch (50 labels)")]
    public string GenerateLabelBatch ()
    {
        string result = "";
        for (int i = 0; i < 50; i++)
        {
            var statements = _labelGenerator.GenerateStatements (_label50Batch, $"label{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }

    /// <summary>
    /// Creates a Window ElementNode with specified number of Label children.
    /// </summary>
    private static ElementNode CreateWindowWithChildren (int childCount)
    {
        var window = new ElementNode { ElementTypeName = "Window" };
        window.Attributes ["Title"] = $"Window with {childCount} children";

        for (int i = 0; i < childCount; i++)
        {
            var label = new ElementNode { ElementTypeName = "Label" };
            label.Attributes ["Text"] = $"Label {i}";
            label.Attributes ["X"] = (i % 10).ToString ();
            label.Attributes ["Y"] = (i / 10).ToString ();
            label.Attributes ["Width"] = "20";
            label.Attributes ["Height"] = "1";
            window.Children.Add (label);
        }

        return window;
    }

    /// <summary>
    /// Creates a Window ElementNode with CheckBox children.
    /// </summary>
    private static ElementNode CreateCheckBoxWindow (int childCount)
    {
        var window = new ElementNode { ElementTypeName = "Window" };
        window.Attributes ["Title"] = $"CheckBox Window with {childCount} checkboxes";

        for (int i = 0; i < childCount; i++)
        {
            var checkBox = new ElementNode { ElementTypeName = "CheckBox" };
            checkBox.Attributes ["Text"] = $"Option {i}";
            checkBox.Attributes ["CheckedState"] = (i % 2) == 0 ? "Checked" : "UnChecked";
            checkBox.Attributes ["X"] = (i % 10).ToString ();
            checkBox.Attributes ["Y"] = (i / 10).ToString ();
            
            if (i % 3 == 0)
            {
                checkBox.Attributes ["AllowCheckStateNone"] = "true";
            }
            
            if (i % 5 == 0)
            {
                checkBox.Attributes ["RadioStyle"] = "true";
            }
            
            window.Children.Add (checkBox);
        }

        return window;
    }

    /// <summary>
    /// Creates a Window ElementNode with mixed control types.
    /// </summary>
    private static ElementNode CreateMixedControlWindow (int childCount)
    {
        var window = new ElementNode { ElementTypeName = "Window" };
        window.Attributes ["Title"] = $"Mixed Window with {childCount} controls";

        for (int i = 0; i < childCount; i++)
        {
            ElementNode control;
            
            switch (i % 3)
            {
                case 0:
                    control = new ElementNode { ElementTypeName = "Label" };
                    control.Attributes ["Text"] = $"Label {i}";
                    break;
                case 1:
                    control = new ElementNode { ElementTypeName = "Button" };
                    control.Attributes ["Text"] = $"Button {i}";
                    break;
                default:
                    control = new ElementNode { ElementTypeName = "CheckBox" };
                    control.Attributes ["Text"] = $"CheckBox {i}";
                    control.Attributes ["CheckedState"] = (i % 2) == 0 ? "Checked" : "UnChecked";
                    break;
            }
            
            control.Attributes ["X"] = (i % 10).ToString ();
            control.Attributes ["Y"] = (i / 10).ToString ();
            window.Children.Add (control);
        }

        return window;
    }

    /// <summary>
    /// Creates a simple Button ElementNode.
    /// </summary>
    private static ElementNode CreateButton (string text, int x, int y)
    {
        var button = new ElementNode { ElementTypeName = "Button" };
        button.Attributes ["Text"] = text;
        button.Attributes ["X"] = x.ToString ();
        button.Attributes ["Y"] = y.ToString ();
        return button;
    }

    /// <summary>
    /// Creates a Button ElementNode with many properties.
    /// </summary>
    private static ElementNode CreateButtonWithManyProperties ()
    {
        var button = new ElementNode { ElementTypeName = "Button" };
        button.Attributes ["Text"] = "Submit Form";
        button.Attributes ["X"] = "10";
        button.Attributes ["Y"] = "20";
        button.Attributes ["Width"] = "25";
        button.Attributes ["Height"] = "3";
        button.Attributes ["Enabled"] = "true";
        button.Attributes ["Visible"] = "true";
        button.Attributes ["CanFocus"] = "true";
        return button;
    }

    /// <summary>
    /// Creates a Button ElementNode for batch testing.
    /// </summary>
    private static ElementNode CreateButtonBatch (int count)
    {
        var button = new ElementNode { ElementTypeName = "Button" };
        button.Attributes ["Text"] = "Action";
        button.Attributes ["X"] = "5";
        button.Attributes ["Y"] = "5";
        return button;
    }

    /// <summary>
    /// Creates a simple Label ElementNode.
    /// </summary>
    private static ElementNode CreateLabel (string text, int x, int y)
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes ["Text"] = text;
        label.Attributes ["X"] = x.ToString ();
        label.Attributes ["Y"] = y.ToString ();
        return label;
    }

    /// <summary>
    /// Creates a Label ElementNode with many properties.
    /// </summary>
    private static ElementNode CreateLabelWithManyProperties ()
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes ["Text"] = "Form Label:";
        label.Attributes ["X"] = "0";
        label.Attributes ["Y"] = "10";
        label.Attributes ["Width"] = "{Fill}";
        label.Attributes ["Height"] = "{Auto}";
        label.Attributes ["Enabled"] = "true";
        label.Attributes ["Visible"] = "true";
        label.Attributes ["CanFocus"] = "false";
        return label;
    }

    /// <summary>
    /// Creates a Label ElementNode for batch testing.
    /// </summary>
    private static ElementNode CreateLabelBatch (int count)
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes ["Text"] = "Item";
        label.Attributes ["X"] = "0";
        label.Attributes ["Y"] = "0";
        return label;
    }
}


