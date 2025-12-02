using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Terminal.Gui.Xtui;

namespace Terminal.Gui.Xtui.Benchmarks;

/// <summary>
/// Benchmarks for XTUI parsing performance (XtuiLoader.LoadFromString).
/// Tests parsing speed and memory allocations for various XTUI file sizes.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class XtuiLoaderBenchmarks
{
    private string _tiny = null!;
    private string _small = null!;
    private string _medium = null!;
    private string _large = null!;
    private string _xlarge = null!;
    private string _nested = null!;
    private string _withComments = null!;
    private string _checkBoxes = null!;
    private string _mixedControls = null!;

    [GlobalSetup]
    public void Setup ()
    {
        // Tiny: Single control (5 elements total)
        _tiny = GenerateXtui (5);

        // Small: 10 controls (typical small dialog)
        _small = GenerateXtui (10);

        // Medium: 50 controls (typical application window)
        _medium = GenerateXtui (50);

        // Large: 100 controls (complex form)
        _large = GenerateXtui (100);

        // XLarge: 500 controls (stress test)
        _xlarge = GenerateXtui (500);

        // Nested: Deep hierarchy (10 levels)
        _nested = GenerateNestedXtui (10);

        // With comments: 50 controls with XML comments
        _withComments = GenerateXtuiWithComments (50);

        // CheckBoxes: 50 CheckBox controls with various states
        _checkBoxes = GenerateCheckBoxXtui (50);

        // Mixed controls: Labels, Buttons, and CheckBoxes
        _mixedControls = GenerateMixedControlsXtui (50);
    }

    [Benchmark (Baseline = true, Description = "Tiny (5 elements)")]
    public ElementNode ParseTiny () => XtuiLoader.LoadFromString (_tiny);

    [Benchmark (Description = "Small (10 elements)")]
    public ElementNode ParseSmall () => XtuiLoader.LoadFromString (_small);

    [Benchmark (Description = "Medium (50 elements)")]
    public ElementNode ParseMedium () => XtuiLoader.LoadFromString (_medium);

    [Benchmark (Description = "Large (100 elements)")]
    public ElementNode ParseLarge () => XtuiLoader.LoadFromString (_large);

    [Benchmark (Description = "XLarge (500 elements)")]
    public ElementNode ParseXLarge () => XtuiLoader.LoadFromString (_xlarge);

    [Benchmark (Description = "Nested (10 levels deep)")]
    public ElementNode ParseNested () => XtuiLoader.LoadFromString (_nested);

    [Benchmark (Description = "With Comments (50 elements)")]
    public ElementNode ParseWithComments () => XtuiLoader.LoadFromString (_withComments);

    [Benchmark (Description = "CheckBoxes (50 elements)")]
    public ElementNode ParseCheckBoxes () => XtuiLoader.LoadFromString (_checkBoxes);

    [Benchmark (Description = "Mixed Controls (50 elements)")]
    public ElementNode ParseMixedControls () => XtuiLoader.LoadFromString (_mixedControls);

    /// <summary>
    /// Generates XTUI markup with specified number of Label elements.
    /// </summary>
    private static string GenerateXtui (int elementCount)
    {
        var sb = new System.Text.StringBuilder ();
        sb.AppendLine (@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine (@"<Window Title=""Benchmark Window"">");

        for (int i = 0; i < elementCount; i++)
        {
            sb.AppendLine ($@"    <Label Text=""Label {i}"" X=""{i % 10}"" Y=""{i / 10}"" Width=""20"" Height=""1"" />");
        }

        sb.AppendLine ("</Window>");
        return sb.ToString ();
    }

    /// <summary>
    /// Generates deeply nested XTUI markup.
    /// </summary>
    private static string GenerateNestedXtui (int depth)
    {
        var sb = new System.Text.StringBuilder ();
        sb.AppendLine (@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine (@"<Window Title=""Nested Benchmark"">");

        // Create nested Views
        for (int i = 0; i < depth; i++)
        {
            sb.AppendLine ($@"{new string (' ', (i + 1) * 4)}<View Id=""Level{i}"">");
        }

        // Add a label at the deepest level
        sb.AppendLine ($@"{new string (' ', (depth + 1) * 4)}<Label Text=""Deep"" />");

        // Close all nested Views
        for (int i = depth - 1; i >= 0; i--)
        {
            sb.AppendLine ($@"{new string (' ', (i + 1) * 4)}</View>");
        }

        sb.AppendLine ("</Window>");
        return sb.ToString ();
    }

    /// <summary>
    /// Generates XTUI markup with XML comments interspersed.
    /// </summary>
    private static string GenerateXtuiWithComments (int elementCount)
    {
        var sb = new System.Text.StringBuilder ();
        sb.AppendLine (@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine (@"<Window Title=""Commented Benchmark"">");
        sb.AppendLine ("    <!-- Header section -->");

        for (int i = 0; i < elementCount; i++)
        {
            if (i % 5 == 0)
            {
                sb.AppendLine ($"    <!-- Group {i / 5} -->");
            }

            sb.AppendLine ($@"    <Label Text=""Label {i}"" X=""{i % 10}"" Y=""{i / 10}"" Width=""20"" Height=""1"" />");

            if (i % 10 == 9)
            {
                sb.AppendLine ("    <!-- End of group -->");
            }
        }

        sb.AppendLine ("    <!-- Footer section -->");
        sb.AppendLine ("</Window>");
        return sb.ToString ();
    }

    /// <summary>
    /// Generates XTUI markup with CheckBox elements with various states.
    /// </summary>
    private static string GenerateCheckBoxXtui (int elementCount)
    {
        var sb = new System.Text.StringBuilder ();
        sb.AppendLine (@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine (@"<Window Title=""CheckBox Benchmark"">");

        for (int i = 0; i < elementCount; i++)
        {
            string checkedState = (i % 3) switch
            {
                0 => "Checked",
                1 => "UnChecked",
                _ => "None"
            };

            bool allowNone = (i % 3) == 2;
            bool radioStyle = (i % 5) == 0;

            sb.Append ($@"    <CheckBox Text=""Option {i}"" CheckedState=""{checkedState}""");
            
            if (allowNone)
            {
                sb.Append (" AllowCheckStateNone=\"true\"");
            }
            
            if (radioStyle)
            {
                sb.Append (" RadioStyle=\"true\"");
            }
            
            sb.AppendLine (" />");
        }

        sb.AppendLine ("</Window>");
        return sb.ToString ();
    }

    /// <summary>
    /// Generates XTUI markup with mixed control types (Labels, Buttons, CheckBoxes).
    /// </summary>
    private static string GenerateMixedControlsXtui (int elementCount)
    {
        var sb = new System.Text.StringBuilder ();
        sb.AppendLine (@"<?xml version=""1.0"" encoding=""utf-8""?>");
        sb.AppendLine (@"<Window Title=""Mixed Controls Benchmark"">");

        for (int i = 0; i < elementCount; i++)
        {
            switch (i % 3)
            {
                case 0:
                    sb.AppendLine ($@"    <Label Text=""Label {i}"" X=""{i % 10}"" Y=""{i / 10}"" />");
                    break;
                case 1:
                    sb.AppendLine ($@"    <Button Text=""Button {i}"" X=""{i % 10}"" Y=""{i / 10}"" />");
                    break;
                case 2:
                    string state = (i % 2) == 0 ? "Checked" : "UnChecked";
                    sb.AppendLine ($@"    <CheckBox Text=""CheckBox {i}"" CheckedState=""{state}"" X=""{i % 10}"" Y=""{i / 10}"" />");
                    break;
            }
        }

        sb.AppendLine ("</Window>");
        return sb.ToString ();
    }
}

