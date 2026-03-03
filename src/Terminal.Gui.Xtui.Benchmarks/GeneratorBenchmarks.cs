using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Terminal.Gui.Xtui.Generator;
using Terminal.Gui.Xtui.Generator.Generators;
using Terminal.Gui.Xtui.Generator.Helpers;

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
    private ElementNode _runnableEmpty = null!;
    private ElementNode _runnable50Children = null!;
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
    private RunnableGenerator _runnableGenerator = null!;
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
        _windowEmpty.Attributes["Title"] = "Empty";

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

        // Runnable benchmarks
        _runnableGenerator = new RunnableGenerator ();
        _runnableEmpty = new ElementNode { ElementTypeName = "Runnable" };
        _runnableEmpty.Attributes["Modal"] = "false";
        // Runnable with 50 children for benchmarks
        _runnable50Children = CreateWindowWithChildren (50);
        // Update element type names in children set if necessary (already uses Window children)
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

    [Benchmark (Description = "Runnable Empty")]
    public string GenerateRunnableEmpty ()
    {
        return _runnableGenerator.GenerateClass (_runnableEmpty, "Benchmark", "EmptyRunnable", _factory);
    }

    [Benchmark (Description = "Runnable 50 Children")]
    public string GenerateRunnable50Children ()
    {
        return _runnableGenerator.GenerateClass (_runnable50Children, "Benchmark", "Runnable50", _factory);
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
        window.Attributes["Title"] = $"Window with {childCount} children";

        for (int i = 0; i < childCount; i++)
        {
            var label = new ElementNode { ElementTypeName = "Label" };
            label.Attributes["Text"] = $"Label {i}";
            label.Attributes["X"] = (i % 10).ToString ();
            label.Attributes["Y"] = (i / 10).ToString ();
            label.Attributes["Width"] = "20";
            label.Attributes["Height"] = "1";
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
        window.Attributes["Title"] = $"CheckBox Window with {childCount} checkboxes";

        for (int i = 0; i < childCount; i++)
        {
            var checkBox = new ElementNode { ElementTypeName = "CheckBox" };
            checkBox.Attributes["Text"] = $"Option {i}";
            checkBox.Attributes["CheckedState"] = (i % 2) == 0 ? "Checked" : "UnChecked";
            checkBox.Attributes["X"] = (i % 10).ToString ();
            checkBox.Attributes["Y"] = (i / 10).ToString ();

            if (i % 3 == 0)
            {
                checkBox.Attributes["AllowCheckStateNone"] = "true";
            }

            if (i % 5 == 0)
            {
                checkBox.Attributes["RadioStyle"] = "true";
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
        window.Attributes["Title"] = $"Mixed Window with {childCount} controls";

        for (int i = 0; i < childCount; i++)
        {
            ElementNode control;

            switch (i % 3)
            {
                case 0:
                    control = new ElementNode { ElementTypeName = "Label" };
                    control.Attributes["Text"] = $"Label {i}";
                    break;
                case 1:
                    control = new ElementNode { ElementTypeName = "Button" };
                    control.Attributes["Text"] = $"Button {i}";
                    break;
                default:
                    control = new ElementNode { ElementTypeName = "CheckBox" };
                    control.Attributes["Text"] = $"CheckBox {i}";
                    control.Attributes["CheckedState"] = (i % 2) == 0 ? "Checked" : "UnChecked";
                    break;
            }

            control.Attributes["X"] = (i % 10).ToString ();
            control.Attributes["Y"] = (i / 10).ToString ();
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
        button.Attributes["Text"] = text;
        button.Attributes["X"] = x.ToString ();
        button.Attributes["Y"] = y.ToString ();
        return button;
    }

    /// <summary>
    /// Creates a Button ElementNode with many properties.
    /// </summary>
    private static ElementNode CreateButtonWithManyProperties ()
    {
        var button = new ElementNode { ElementTypeName = "Button" };
        button.Attributes["Text"] = "Submit Form";
        button.Attributes["X"] = "10";
        button.Attributes["Y"] = "20";
        button.Attributes["Width"] = "25";
        button.Attributes["Height"] = "3";
        button.Attributes["Enabled"] = "true";
        button.Attributes["Visible"] = "true";
        button.Attributes["CanFocus"] = "true";
        return button;
    }

    /// <summary>
    /// Creates a Button ElementNode for batch testing.
    /// </summary>
    private static ElementNode CreateButtonBatch (int count)
    {
        var button = new ElementNode { ElementTypeName = "Button" };
        button.Attributes["Text"] = "Action";
        button.Attributes["X"] = "5";
        button.Attributes["Y"] = "5";
        return button;
    }

    /// <summary>
    /// Creates a simple Label ElementNode.
    /// </summary>
    private static ElementNode CreateLabel (string text, int x, int y)
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes["Text"] = text;
        label.Attributes["X"] = x.ToString ();
        label.Attributes["Y"] = y.ToString ();
        return label;
    }

    /// <summary>
    /// Creates a Label ElementNode with many properties.
    /// </summary>
    private static ElementNode CreateLabelWithManyProperties ()
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes["Text"] = "Form Label:";
        label.Attributes["X"] = "0";
        label.Attributes["Y"] = "10";
        label.Attributes["Width"] = "{Fill}";
        label.Attributes["Height"] = "{Auto}";
        label.Attributes["Enabled"] = "true";
        label.Attributes["Visible"] = "true";
        label.Attributes["CanFocus"] = "false";
        return label;
    }

    /// <summary>
    /// Creates a Label ElementNode for batch testing.
    /// </summary>
    private static ElementNode CreateLabelBatch (int count)
    {
        var label = new ElementNode { ElementTypeName = "Label" };
        label.Attributes["Text"] = "Item";
        label.Attributes["X"] = "0";
        label.Attributes["Y"] = "0";
        return label;
    }
}

/// <summary>
/// Benchmarks for MenuBar code generation performance.
/// Tests MenuBarGenerator with varying numbers of MenuBarItems and properties.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class MenuBarGeneratorBenchmarks
{
    private ElementNode _menuBarEmpty = null!;
    private ElementNode _menuBar1Item = null!;
    private ElementNode _menuBar5Items = null!;
    private ElementNode _menuBar10Items = null!;
    private ElementNode _menuBarWithProperties = null!;
    private MenuBarGenerator _menuBarGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _menuBarGenerator = new MenuBarGenerator ();
        _factory = new GeneratorFactory ();

        // Empty MenuBar
        _menuBarEmpty = new ElementNode { ElementTypeName = "MenuBar" };
        _menuBarEmpty.Attributes["Id"] = "menuBar";

        // MenuBar with 1 item
        _menuBar1Item = CreateMenuBarWithItems (1);

        // MenuBar with 5 items
        _menuBar5Items = CreateMenuBarWithItems (5);

        // MenuBar with 10 items
        _menuBar10Items = CreateMenuBarWithItems (10);

        // MenuBar with many properties
        _menuBarWithProperties = CreateMenuBarWithManyProperties ();
    }

    [Benchmark (Baseline = true, Description = "MenuBar Empty")]
    public string GenerateMenuBarEmpty ()
    {
        return _menuBarGenerator.GenerateClass (_menuBarEmpty, "Benchmark", "EmptyMenuBar", _factory);
    }

    [Benchmark (Description = "MenuBar 1 Item")]
    public string GenerateMenuBar1Item ()
    {
        return _menuBarGenerator.GenerateClass (_menuBar1Item, "Benchmark", "MenuBar1", _factory);
    }

    [Benchmark (Description = "MenuBar 5 Items")]
    public string GenerateMenuBar5Items ()
    {
        return _menuBarGenerator.GenerateClass (_menuBar5Items, "Benchmark", "MenuBar5", _factory);
    }

    [Benchmark (Description = "MenuBar 10 Items")]
    public string GenerateMenuBar10Items ()
    {
        return _menuBarGenerator.GenerateClass (_menuBar10Items, "Benchmark", "MenuBar10", _factory);
    }

    [Benchmark (Description = "MenuBar With Properties")]
    public string GenerateMenuBarWithProperties ()
    {
        return _menuBarGenerator.GenerateClass (_menuBarWithProperties, "Benchmark", "MenuBarProps", _factory);
    }

    [Benchmark (Description = "MenuBar Statements Only")]
    public object GenerateMenuBarStatementsOnly ()
    {
        return _menuBarGenerator.GenerateStatements (_menuBar5Items, "menuBar", _factory);
    }

    /// <summary>
    /// Creates a MenuBar ElementNode with specified number of MenuBarItem children.
    /// </summary>
    private static ElementNode CreateMenuBarWithItems (int count)
    {
        var menuBar = new ElementNode { ElementTypeName = "MenuBar" };
        menuBar.Attributes["Title"] = "Main Menu";
        menuBar.Attributes["Id"] = "menuBar";

        for (int i = 0; i < count; i++)
        {
            var menuBarItem = new ElementNode { ElementTypeName = "MenuBarItem" };
            menuBarItem.Attributes["Title"] = $"_Menu{i}";
            menuBar.Children.Add (menuBarItem);
        }

        return menuBar;
    }

    /// <summary>
    /// Creates a MenuBar ElementNode with many properties.
    /// </summary>
    private static ElementNode CreateMenuBarWithManyProperties ()
    {
        var menuBar = new ElementNode { ElementTypeName = "MenuBar" };
        menuBar.Attributes["Title"] = "Application Menu";
        menuBar.Attributes["Id"] = "mainMenuBar";
        menuBar.Attributes["X"] = "0";
        menuBar.Attributes["Y"] = "0";
        menuBar.Attributes["Width"] = "{Fill}";
        menuBar.Attributes["Height"] = "1";
        menuBar.Attributes["Enabled"] = "true";
        menuBar.Attributes["Visible"] = "true";
        menuBar.Attributes["CanFocus"] = "true";

        // Add a few items
        for (int i = 0; i < 3; i++)
        {
            var menuBarItem = new ElementNode { ElementTypeName = "MenuBarItem" };
            menuBarItem.Attributes["Title"] = $"_Item{i}";
            menuBar.Children.Add (menuBarItem);
        }

        return menuBar;
    }
}

/// <summary>
/// Benchmarks for TextField code generation performance.
/// Tests TextFieldGenerator with varying properties including Secret property.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class TextFieldGeneratorBenchmarks
{
    private ElementNode _textFieldSimple = null!;
    private ElementNode _textFieldWithSecret = null!;
    private ElementNode _textFieldWithManyProperties = null!;
    private ElementNode _textFieldBatch50 = null!;
    private TextFieldGenerator _textFieldGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _textFieldGenerator = new TextFieldGenerator ();
        _factory = new GeneratorFactory ();

        // Simple TextField
        _textFieldSimple = new ElementNode { ElementTypeName = "TextField" };
        _textFieldSimple.Attributes["Text"] = "Default Value";

        // TextField with Secret property
        _textFieldWithSecret = new ElementNode { ElementTypeName = "TextField" };
        _textFieldWithSecret.Attributes["Id"] = "_passwordField";
        _textFieldWithSecret.Attributes["Secret"] = "true";
        _textFieldWithSecret.Attributes["X"] = "10";
        _textFieldWithSecret.Attributes["Y"] = "5";

        // TextField with many properties
        _textFieldWithManyProperties = new ElementNode { ElementTypeName = "TextField" };
        _textFieldWithManyProperties.Attributes["Id"] = "_usernameField";
        _textFieldWithManyProperties.Attributes["Text"] = "Initial";
        _textFieldWithManyProperties.Attributes["X"] = "{Right _label + 2}";
        _textFieldWithManyProperties.Attributes["Y"] = "0";
        _textFieldWithManyProperties.Attributes["Width"] = "30";
        _textFieldWithManyProperties.Attributes["Height"] = "1";
        _textFieldWithManyProperties.Attributes["Enabled"] = "true";
        _textFieldWithManyProperties.Attributes["CanFocus"] = "true";

        // TextField for batch testing
        _textFieldBatch50 = new ElementNode { ElementTypeName = "TextField" };
        _textFieldBatch50.Attributes["Text"] = "Input";
    }

    [Benchmark (Baseline = true, Description = "TextField Simple")]
    public object GenerateTextFieldSimple ()
    {
        return _textFieldGenerator.GenerateStatements (_textFieldSimple, "field1", _factory);
    }

    [Benchmark (Description = "TextField With Secret")]
    public object GenerateTextFieldWithSecret ()
    {
        return _textFieldGenerator.GenerateStatements (_textFieldWithSecret, "_passwordField", _factory);
    }

    [Benchmark (Description = "TextField With Many Properties")]
    public object GenerateTextFieldWithManyProperties ()
    {
        return _textFieldGenerator.GenerateStatements (_textFieldWithManyProperties, "_usernameField", _factory);
    }

    [Benchmark (Description = "TextField Batch (50 fields)")]
    public string GenerateTextFieldBatch ()
    {
        string result = "";
        for (int i = 0; i < 50; i++)
        {
            var statements = _textFieldGenerator.GenerateStatements (_textFieldBatch50, $"field{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }
}

/// <summary>
/// Benchmarks for ListView code generation performance.
/// Tests ListViewGenerator with varying properties.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class ListViewGeneratorBenchmarks
{
    private ElementNode _listViewSimple = null!;
    private ElementNode _listViewWithDimensions = null!;
    private ElementNode _listViewWithManyProperties = null!;
    private ElementNode _listViewBatch20 = null!;
    private ListViewGenerator _listViewGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _listViewGenerator = new ListViewGenerator ();
        _factory = new GeneratorFactory ();

        // Simple ListView
        _listViewSimple = new ElementNode { ElementTypeName = "ListView" };
        _listViewSimple.Attributes["Id"] = "_listView";

        // ListView with dimensions
        _listViewWithDimensions = new ElementNode { ElementTypeName = "ListView" };
        _listViewWithDimensions.Attributes["Id"] = "_listView";
        _listViewWithDimensions.Attributes["Width"] = "40";
        _listViewWithDimensions.Attributes["Height"] = "10";
        _listViewWithDimensions.Attributes["X"] = "0";
        _listViewWithDimensions.Attributes["Y"] = "0";

        // ListView with many properties
        _listViewWithManyProperties = new ElementNode { ElementTypeName = "ListView" };
        _listViewWithManyProperties.Attributes["Id"] = "_itemsListView";
        _listViewWithManyProperties.Attributes["X"] = "{Center}";
        _listViewWithManyProperties.Attributes["Y"] = "2";
        _listViewWithManyProperties.Attributes["Width"] = "{Fill - 5}";
        _listViewWithManyProperties.Attributes["Height"] = "{Fill - 3}";
        _listViewWithManyProperties.Attributes["Enabled"] = "true";
        _listViewWithManyProperties.Attributes["CanFocus"] = "true";
        _listViewWithManyProperties.Attributes["Visible"] = "true";

        // ListView for batch testing
        _listViewBatch20 = new ElementNode { ElementTypeName = "ListView" };
        _listViewBatch20.Attributes["Width"] = "30";
        _listViewBatch20.Attributes["Height"] = "8";
    }

    [Benchmark (Baseline = true, Description = "ListView Simple")]
    public object GenerateListViewSimple ()
    {
        return _listViewGenerator.GenerateStatements (_listViewSimple, "_listView", _factory);
    }

    [Benchmark (Description = "ListView With Dimensions")]
    public object GenerateListViewWithDimensions ()
    {
        return _listViewGenerator.GenerateStatements (_listViewWithDimensions, "_listView", _factory);
    }

    [Benchmark (Description = "ListView With Many Properties")]
    public object GenerateListViewWithManyProperties ()
    {
        return _listViewGenerator.GenerateStatements (_listViewWithManyProperties, "_itemsListView", _factory);
    }

    [Benchmark (Description = "ListView Batch (20 lists)")]
    public string GenerateListViewBatch ()
    {
        string result = "";
        for (int i = 0; i < 20; i++)
        {
            var statements = _listViewGenerator.GenerateStatements (_listViewBatch20, $"list{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }
}

/// <summary>
/// Benchmarks for MenuBarItem and MenuItem code generation performance.
/// Tests MenuBarItemGenerator and MenuItemGenerator with varying numbers of children and properties.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class MenuItemGeneratorBenchmarks
{
    private ElementNode _menuItemSimple = null!;
    private ElementNode _menuItemWithProperties = null!;
    private ElementNode _menuItemBatch30 = null!;
    private ElementNode _menuBarItemEmpty = null!;
    private ElementNode _menuBarItemWith5MenuItems = null!;
    private ElementNode _menuBarItemWith10MenuItems = null!;
    private MenuItemGenerator _menuItemGenerator = null!;
    private MenuBarItemGenerator _menuBarItemGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _menuItemGenerator = new MenuItemGenerator ();
        _menuBarItemGenerator = new MenuBarItemGenerator ();
        _factory = new GeneratorFactory ();

        // Simple MenuItem
        _menuItemSimple = new ElementNode { ElementTypeName = "MenuItem" };
        _menuItemSimple.Attributes["Title"] = "_Open";

        // MenuItem with properties
        _menuItemWithProperties = new ElementNode { ElementTypeName = "MenuItem" };
        _menuItemWithProperties.Attributes["Title"] = "_Save As...";
        _menuItemWithProperties.Attributes["HelpText"] = "Save file with new name";
        _menuItemWithProperties.Attributes["Enabled"] = "true";

        // MenuItem for batch testing
        _menuItemBatch30 = new ElementNode { ElementTypeName = "MenuItem" };
        _menuItemBatch30.Attributes["Title"] = "_Action";

        // Empty MenuBarItem
        _menuBarItemEmpty = new ElementNode { ElementTypeName = "MenuBarItem" };
        _menuBarItemEmpty.Attributes["Title"] = "_File";

        // MenuBarItem with 5 MenuItems
        _menuBarItemWith5MenuItems = CreateMenuBarItemWithMenuItems (5);

        // MenuBarItem with 10 MenuItems
        _menuBarItemWith10MenuItems = CreateMenuBarItemWithMenuItems (10);
    }

    [Benchmark (Baseline = true, Description = "MenuItem Simple")]
    public object GenerateMenuItemSimple ()
    {
        return _menuItemGenerator.GenerateStatements (_menuItemSimple, "menuItem1", _factory);
    }

    [Benchmark (Description = "MenuItem With Properties")]
    public object GenerateMenuItemWithProperties ()
    {
        return _menuItemGenerator.GenerateStatements (_menuItemWithProperties, "saveAsItem", _factory);
    }

    [Benchmark (Description = "MenuItem Batch (30 items)")]
    public string GenerateMenuItemBatch ()
    {
        string result = "";
        for (int i = 0; i < 30; i++)
        {
            var statements = _menuItemGenerator.GenerateStatements (_menuItemBatch30, $"item{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }

    [Benchmark (Description = "MenuBarItem Empty")]
    public object GenerateMenuBarItemEmpty ()
    {
        return _menuBarItemGenerator.GenerateStatements (_menuBarItemEmpty, "fileMenu", _factory);
    }

    [Benchmark (Description = "MenuBarItem With 5 MenuItems")]
    public object GenerateMenuBarItemWith5MenuItems ()
    {
        return _menuBarItemGenerator.GenerateStatements (_menuBarItemWith5MenuItems, "editMenu", _factory);
    }

    [Benchmark (Description = "MenuBarItem With 10 MenuItems")]
    public object GenerateMenuBarItemWith10MenuItems ()
    {
        return _menuBarItemGenerator.GenerateStatements (_menuBarItemWith10MenuItems, "viewMenu", _factory);
    }

    /// <summary>
    /// Creates a MenuBarItem ElementNode with specified number of MenuItem children.
    /// </summary>
    private static ElementNode CreateMenuBarItemWithMenuItems (int count)
    {
        var menuBarItem = new ElementNode { ElementTypeName = "MenuBarItem" };
        menuBarItem.Attributes["Title"] = $"_Menu ({count} items)";

        for (int i = 0; i < count; i++)
        {
            var menuItem = new ElementNode { ElementTypeName = "MenuItem" };
            menuItem.Attributes["Title"] = $"_Item{i}";
            if (i % 3 == 0)
            {
                menuItem.Attributes["HelpText"] = $"Help for item {i}";
            }
            menuBarItem.Children.Add (menuItem);
        }

        return menuBarItem;
    }
}

/// <summary>
/// Benchmarks for GenericGenerator code generation performance.
/// Tests GenericGenerator with unknown control types and varying properties.
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class GenericGeneratorBenchmarks
{
    private ElementNode _customControlSimple = null!;
    private ElementNode _customControlWithProperties = null!;
    private ElementNode _customControlWithChildren = null!;
    private ElementNode _customControlBatch25 = null!;
    private GenericGenerator _genericGenerator = null!;
    private GeneratorFactory _factory = null!;

    [GlobalSetup]
    public void Setup ()
    {
        _genericGenerator = new GenericGenerator ();
        _factory = new GeneratorFactory ();

        // Simple custom control
        _customControlSimple = new ElementNode { ElementTypeName = "CustomView" };
        _customControlSimple.Attributes["Title"] = "My Custom View";

        // Custom control with properties
        _customControlWithProperties = new ElementNode { ElementTypeName = "AdvancedWidget" };
        _customControlWithProperties.Attributes["Id"] = "_widget";
        _customControlWithProperties.Attributes["X"] = "{Center}";
        _customControlWithProperties.Attributes["Y"] = "5";
        _customControlWithProperties.Attributes["Width"] = "{Fill - 10}";
        _customControlWithProperties.Attributes["Height"] = "20";
        _customControlWithProperties.Attributes["Enabled"] = "true";
        _customControlWithProperties.Attributes["Visible"] = "true";
        _customControlWithProperties.Attributes["CanFocus"] = "true";

        // Custom control with children
        _customControlWithChildren = new ElementNode { ElementTypeName = "ContainerControl" };
        _customControlWithChildren.Attributes["Title"] = "Container";
        for (int i = 0; i < 5; i++)
        {
            var child = new ElementNode { ElementTypeName = "ChildControl" };
            child.Attributes["Text"] = $"Child {i}";
            _customControlWithChildren.Children.Add (child);
        }

        // Custom control for batch testing
        _customControlBatch25 = new ElementNode { ElementTypeName = "SimpleCustomControl" };
        _customControlBatch25.Attributes["Name"] = "Control";
    }

    [Benchmark (Baseline = true, Description = "Generic Simple")]
    public object GenerateGenericSimple ()
    {
        return _genericGenerator.GenerateStatements (_customControlSimple, "custom1", _factory);
    }

    [Benchmark (Description = "Generic With Properties")]
    public object GenerateGenericWithProperties ()
    {
        return _genericGenerator.GenerateStatements (_customControlWithProperties, "_widget", _factory);
    }

    [Benchmark (Description = "Generic With Children")]
    public object GenerateGenericWithChildren ()
    {
        return _genericGenerator.GenerateStatements (_customControlWithChildren, "container", _factory);
    }

    [Benchmark (Description = "Generic Batch (25 controls)")]
    public string GenerateGenericBatch ()
    {
        string result = "";
        for (int i = 0; i < 25; i++)
        {
            var statements = _genericGenerator.GenerateStatements (_customControlBatch25, $"control{i}", _factory);
            result = string.Concat (statements.Select (s => s.ToString ()));
        }
        return result;
    }
}



