# Glossary

Definitions of key terms, concepts, and acronyms used in Terminal.Gui.Xaml development.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Quick Navigation**: [A-C](#a-c) • [D-G](#d-g) • [H-M](#h-m) • [N-R](#n-r) • [S-Z](#s-z)

---

## A-C

### Access Key
A keyboard shortcut that allows quick navigation to a control. Defined using the `AccessKey` property, typically activated with Alt+Key.
```xml
<Button Text="Save" AccessKey="S" />
<!-- Activated with Alt+S -->
```

### Attached Property
A property that can be applied to any element, even if the element's class doesn't define it. Common in layout scenarios.
```xml
<Label Grid.Row="1" Grid.Column="2" />
<!-- Grid.Row and Grid.Column are attached properties -->
```

### Binding
The connection between UI elements and data sources that automatically synchronizes values. See [Data Binding](concepts/binding.md).

### Binding Context
The object that provides data for binding expressions. Typically set via the `DataContext` property.

### Binding Mode
Specifies how data flows between source and target in a binding:
- **OneWay**: Source to target only
- **TwoWay**: Bidirectional synchronization  
- **OneTime**: Single initial value transfer
- **OneWayToSource**: Target to source only

### Code-Behind
The C# code file (.xaml.cs) associated with a XAML file that contains event handlers and logic.

### Command
An object implementing `ICommand` that encapsulates application logic and can be bound to UI elements for MVVM scenarios.

### Control
A UI element that users can interact with, such as buttons, text fields, or lists. Inherits from the `View` class in Terminal.Gui.

### Converter
A class implementing `IValueConverter` that transforms data between binding source and target formats.

---

## D-G

### DataContext
The object that serves as the default source for data binding within an element and its children.

### Data Template
A XAML template that defines how data objects should be displayed in data-bound controls like ListView.

### Dependency Property
A special type of property that supports data binding, styling, and other WPF-like features.

### Design Time
The period when you're editing XAML in a designer or IDE, as opposed to runtime when the application is executing.

### Dim
Terminal.Gui's dimension system for character-based sizing:
```xml
<FrameView Width="Dim.Fill(2)" Height="Dim.Percent(50)" />
```

### Element Tree
The hierarchical structure of XAML elements in your user interface, similar to DOM in web development.

### Event Handler
A method in code-behind that responds to user interface events like button clicks or text changes.

### Focus
The state of a control that indicates it will receive keyboard input. Only one control can have focus at a time.

---

## H-M

### Hot Reload
A development feature that applies XAML changes to a running application without restarting it.

### INotifyPropertyChanged
An interface that classes implement to notify the binding system when property values change, enabling automatic UI updates.

### IntelliSense
IDE feature providing code completion, syntax highlighting, and error detection for XAML and C#.

### Layout
The system for positioning and sizing controls within their containers. See [Layout Concepts](concepts/layout.md).

### Markup Extension
XAML syntax that provides values for properties using special syntax, such as `{Binding}` or `{StaticResource}`.

### MVVM (Model-View-ViewModel)
A design pattern that separates UI (View) from business logic (Model) using a ViewModel as an intermediary. Common in XAML applications.

---

## N-R

### Namespace
XML namespace declarations in XAML that map prefixes to .NET namespaces:
```xml
xmlns:local="clr-namespace:MyApp.Views"
```

### Navigation
Moving between different views or windows in an application. See [Navigation Guide](guides/navigation.md).

### One-Way Binding
A binding mode where data flows from source to target but changes in the target don't affect the source.

### Pos
Terminal.Gui's position system for character-based positioning:
```xml
<Button X="Pos.Center()" Y="Pos.Bottom(view) - 1" />
```

### Property Element
XAML syntax for setting complex properties using elements instead of attributes:
```xml
<Button>
    <Button.Content>
        <StackPanel>
            <Label Text="Line 1" />
            <Label Text="Line 2" />
        </StackPanel>
    </Button.Content>
</Button>
```

### Resource
A reusable object defined in XAML that can be referenced throughout your application, such as styles or data templates.

### Routed Event
An event that can travel up or down the element tree, allowing multiple elements to handle the same event occurrence.

---

## S-Z

### Screen Reader
Assistive technology that reads UI elements aloud for visually impaired users. Terminal.Gui.Xaml supports screen readers through accessibility properties.

### Setter
Part of a style that sets a property value:
```xml
<Style TargetType="Button">
    <Setter Property="ForegroundColor" Value="Blue" />
</Style>
```

### Style
A collection of property setters that can be applied to controls to provide consistent appearance and behavior.

### Tab Index
A numeric value that determines the order in which controls receive focus when the user presses the Tab key.

### Template
A reusable definition of a control's visual structure. Data templates define how data appears, while control templates define control appearance.

### Terminal.Gui
The underlying cross-platform terminal UI framework that Terminal.Gui.Xaml is built upon.

### Trigger
A mechanism that automatically applies property changes or actions when specified conditions are met.

### Two-Way Binding
A binding mode where changes in either the source or target are automatically synchronized to the other.

### UI Thread
The main thread where all UI operations must occur. Terminal applications typically run single-threaded.

### User Control
A reusable custom control created by combining existing controls, typically with associated XAML and code-behind files.

### Value Converter
A class that transforms data between different types or formats during binding operations.

### View
In MVVM pattern, the user interface layer that displays data and captures user input. In Terminal.Gui, the base class for all UI elements.

### ViewModel
In MVVM pattern, the layer between View and Model that exposes data and commands for the UI to bind to.

### Visual Tree
The runtime hierarchy of visual elements that make up the user interface.

### XAML (eXtensible Application Markup Language)
A declarative markup language used to define user interfaces. Pronounced "zammel."

### XML Namespace
A mechanism to avoid name conflicts in XML documents by qualifying element and attribute names.

---

## Acronyms and Abbreviations

| Acronym | Full Form | Description |
|---------|-----------|-------------|
| **API** | Application Programming Interface | Set of functions and methods available to developers |
| **CLI** | Command Line Interface | Text-based user interface for operating systems |
| **DI** | Dependency Injection | Design pattern for providing dependencies to objects |
| **IDE** | Integrated Development Environment | Software for code editing, debugging, and building |
| **MVVM** | Model-View-ViewModel | Architectural pattern for separating UI from business logic |
| **NuGet** | .NET Package Manager | Package management system for .NET |
| **REPL** | Read-Eval-Print Loop | Interactive programming environment |
| **TUI** | Text User Interface | Character-based user interface in terminal applications |
| **UI** | User Interface | The means by which users interact with applications |
| **UWP** | Universal Windows Platform | Windows application platform |
| **WPF** | Windows Presentation Foundation | Microsoft's UI framework for desktop applications |
| **XAML** | eXtensible Application Markup Language | Declarative markup language for defining UIs |

---

## Common File Extensions

| Extension | Description | Example |
|-----------|-------------|---------|
| **.xaml** | XAML markup files containing UI definitions | `MainWindow.xaml` |
| **.xaml.cs** | Code-behind files with event handlers and logic | `MainWindow.xaml.cs` |
| **.cs** | C# source code files | `PersonViewModel.cs` |
| **.csproj** | MSBuild project files | `MyApp.csproj` |
| **.sln** | Visual Studio solution files | `MyApplication.sln` |
| **.json** | Configuration files | `appsettings.json` |

---

## Symbol Meanings

### XAML Syntax
- `{}` - Markup extension syntax: `{Binding Path=Name}`
- `x:` - XAML namespace prefix for XAML-specific features
- `xmlns:` - XML namespace declaration
- `<!--  -->` - XAML comments
- `<![CDATA[  ]]>` - Literal text that won't be parsed as XML

### Terminal Positioning
- `Pos.Center()` - Center position within parent
- `Pos.AnchorEnd()` - Position at end of parent
- `Dim.Fill()` - Fill remaining space
- `Dim.Percent(50)` - 50% of parent dimension

### Binding Syntax
- `{Binding}` - Bind to entire DataContext
- `{Binding Name}` - Bind to Name property
- `{Binding Path=Name}` - Explicit path syntax
- `{Binding Name, Mode=TwoWay}` - Specify binding mode

---

## Related Documentation

- [Getting Started](getting-started.md) - Basic concepts and your first application
- XAML concepts and syntax (coming soon)  
- [Data Binding](concepts/binding.md) - Comprehensive binding guide
- [Layout System](concepts/layout.md) - Positioning and sizing concepts
- [API Reference](../api/index.md) - Complete technical reference

---

## Contributing to the Glossary

Found a term that should be added or needs clarification?

1. **Check existing documentation** to see if the term is explained elsewhere
2. **Search the codebase** to understand current usage
3. **Submit a pull request** with your additions following the format above
4. **Include examples** where helpful to illustrate the concept

See our [Contributing Guide](contributing/index.md) for the contribution process.

---

> **Quick Navigation**: [A-C](#a-c) • [D-G](#d-g) • [H-M](#h-m) • [N-R](#n-r) • [S-Z](#s-z) • [Top](#glossary)
