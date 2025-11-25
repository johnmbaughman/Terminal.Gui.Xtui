# Terminal.Gui.Xaml

A Roslyn source generator that enables XAML-based UI design for [Terminal.Gui v2](https://github.com/gui-cs/Terminal.Gui) applications. Write your terminal UIs declaratively using XAML syntax, and let the generator produce clean, efficient C# code at compile time.

## Overview

Terminal.Gui.Xaml brings declarative UI design to Terminal.Gui through a compile-time source generator. Inspired by Microsoft's XAML implementation, this project allows you to define terminal-based user interfaces using familiar XAML syntax while maintaining Terminal.Gui's performance characteristics.

### Key Features

- **Compile-Time Code Generation**: XAML files are transformed into C# code during compilation using Roslyn incremental source generators
- **Zero Runtime Overhead**: No XAML parsing at runtime—generated code directly instantiates Terminal.Gui controls
- **Type-Safe**: Leverages C#'s type system with compile-time validation
- **InitializeComponent Pattern**: Follows the familiar WPF/WinForms pattern with partial classes
- **Rich Layout Support**: Full support for Terminal.Gui's `Pos` and `Dim` positioning system including operators
- **Automatic Discovery**: XAML files are automatically detected and processed when Terminal.Gui is referenced
- **IDE Integration**: Generated files visible in your IDE under `obj/Generated` folder

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Terminal.Gui v2 (referenced as submodule or package)
- Any IDE with C# support (Visual Studio, VS Code, Rider)

### Installation

1. **Clone the repository with submodules:**

```bash
git clone --recurse-submodules https://github.com/johnmbaughman/Terminal.Gui.Xaml.git
cd Terminal.Gui.Xaml
```

If you already cloned without `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

2. **Build the solution:**

```bash
dotnet build src/Terminal.Gui.Xaml.sln
```

3. **Run the example:**

```bash
dotnet run --project src/Examples/Xaml
```

## Project Structure

```
Terminal.Gui.Xaml/
├── src/
│   ├── Terminal.Gui/                    # Git submodule (Terminal.Gui v2)
│   ├── Terminal.Gui.Xaml/               # Source generator library
│   │   ├── CodeGenerator.cs             # Roslyn incremental source generator
│   │   ├── XamlLoader.cs                # XAML parser (using System.Xml.Linq)
│   │   ├── ElementNode.cs               # Internal XAML tree representation
│   │   ├── Generators/                  # Control-specific code generators
│   │   │   ├── WindowGenerator.cs       # Generates Window with InitializeComponent
│   │   │   ├── LabelGenerator.cs        # Label generator
│   │   │   ├── ButtonGenerator.cs       # Button generator
│   │   │   ├── GenericGenerator.cs      # Fallback for other controls
│   │   │   ├── GeneratorFactory.cs      # Factory for selecting generators
│   │   │   └── ObjectParsingHelpers.cs  # Pos/Dim expression parsing
│   │   ├── Mappers/                     # Type mappers
│   │   │   └── EnumMapper.cs            # Enum value mapping
│   │   └── buildTransitive/             # MSBuild integration
│   │       └── Terminal.Gui.Xaml.targets # Automatic XAML file discovery
│   ├── Examples/
│   │   ├── Xaml/                        # Basic example
│   │   │   ├── MyWindow.xaml            # XAML UI definition
│   │   │   ├── MyWindow.cs              # Partial class with constructor
│   │   │   └── Program.cs               # Application entry point
│   │   └── Xaml.Mvvm/                   # MVVM pattern example
│   │       ├── MyWindow.xaml
│   │       ├── MyWindow.cs
│   │       └── Program.cs               # Includes MainViewModel
│   └── Terminal.Gui.Xaml.Tests/         # Unit tests (future)
├── .gitignore
├── .gitmodules
├── LICENSE
└── README.md
```

## Usage

### Basic Example

The best reference is the [Xaml example project](src/Examples/Xaml). Here's how it works:

**1. Create a XAML file (`MyWindow.xaml`):**

```xml
<Window>
    <!-- Simple label with absolute positioning -->
    <Label Text="Hello" X="10" Y="5" Width="20" Height="1" />
    
    <!-- Button with centered positioning and percentage sizing -->
    <Button Text="Click Me" X="{Center}" Y="50%" Width="80%" Height="3" />
    
    <!-- Label using AnchorEnd and Auto/Fill dimensions -->
    <Label Text="Anchored" X="{AnchorEnd}" Y="{AnchorEnd 5}" Width="{Auto}" Height="{Fill}" />
</Window>
```

**2. Create a partial class matching the XAML filename (`MyWindow.cs`):**

```csharp
namespace Xaml;

public partial class MyWindow 
{
    public MyWindow()
    {
        InitializeComponent();  // Generated method
    }
}
```

**3. Reference the generator in your `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- Reference Terminal.Gui -->
    <ProjectReference Include="..\..\Terminal.Gui\Terminal.Gui\Terminal.Gui.csproj" />
    
    <!-- Reference the generator (OutputItemType="Analyzer" is critical) -->
    <ProjectReference Include="..\..\Terminal.Gui.Xaml\Terminal.Gui.Xaml.csproj"
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <!-- Import the targets that auto-discover XAML files -->
  <Import Project="..\..\Terminal.Gui.Xaml\buildTransitive\Terminal.Gui.Xaml.targets" />
</Project>
```

**4. Use your window in the application (`Program.cs`):**

```csharp
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace Xaml;

class Program
{
    static void Main()
    {
        var app = Application.Create();
        app.Init();
        var top = new Toplevel();
        top.Add(new MyWindow());
        app.Run(top);
        top.Dispose();
        app.Shutdown();
    }
}
```

**Generated Code (`MyWindow.g.cs`):**

The generator produces code like this (visible in `obj/Generated/`):

```csharp
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace Xaml
{
    public partial class MyWindow : Window
    {
        private void InitializeComponent()
        {
            this.Add(new Label() { Text = "Hello", X = 10, Y = 5, Width = 20, Height = 1 });
            this.Add(new Button() { Text = "Click Me", X = Pos.Center(), Y = Pos.Percent(50), Width = Dim.Percent(80), Height = 3 });
            this.Add(new Label() { Text = "Anchored", X = Pos.AnchorEnd(), Y = Pos.AnchorEnd(5), Width = Dim.Auto(), Height = Dim.Fill() });
        }
    }
}
```

### MVVM Example

See the [Xaml.Mvvm example project](src/Examples/Xaml.Mvvm) for MVVM pattern usage with CommunityToolkit.Mvvm:

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "Hello MVVM";

    public IRelayCommand ClickCommand { get; }

    public MainViewModel()
    {
        ClickCommand = new RelayCommand(() => 
            Message = "Clicked at " + DateTime.Now);
    }
}

public partial class MyWindow
{
    private readonly MainViewModel _viewModel;

    public MyWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        // Bind properties to controls after InitializeComponent
    }
}
```

## XAML Syntax

### Positioning (Pos)

Terminal.Gui's `Pos` type supports multiple positioning modes:

```xml
<!-- Absolute positioning -->
<Label X="10" Y="5" />

<!-- Percentage positioning -->
<Label X="50%" Y="25%" />

<!-- Named positioning methods -->
<Label X="{Center}" Y="{AnchorEnd}" />
<Label X="{AnchorEnd 10}" />

<!-- Arithmetic expressions -->
<Label X="{Center - 10}" />
<Label X="{Center + 5}" />
<Label X="{AnchorEnd - 25}" />
```

**Supported Pos methods:** `Absolute`, `Percent`, `Center`, `AnchorEnd`, `Left`, `Right`, `Top`, `Bottom`, `X`, `Y`, `Func`, `Align`

### Sizing (Dim)

Terminal.Gui's `Dim` type supports dynamic sizing:

```xml
<!-- Absolute size -->
<Label Width="20" Height="1" />

<!-- Percentage sizing -->
<Label Width="80%" Height="50%" />

<!-- Dynamic sizing -->
<Label Width="{Fill}" Height="{Auto}" />

<!-- Arithmetic expressions -->
<Label Width="{Fill - 5}" />
<Label Width="{Auto + 2}" />
```

**Supported Dim methods:** `Absolute`, `Percent`, `Fill`, `Auto`, `Width`, `Height`, `Func`

### Property Types

The generator recognizes these property types:

- **String properties:** `Text`, `Title`, `Id`
- **Boolean properties:** `Visible`, `Enabled`, `CanFocus`, `HasFocus`, etc.
- **Position properties:** `X`, `Y` (Pos type)
- **Dimension properties:** `Width`, `Height` (Dim type)

### Comments

XAML comments are automatically stripped during parsing:

```xml
<Window>
    <!-- This is a single-line comment -->
    <Label Text="Hello" />
    
    <!-- 
        Multi-line comment
        Spans multiple lines
    -->
    <Button Text="Click Me" />
    
    <!-- Commented-out controls are not generated:
    <Label Text="This won't appear" />
    -->
</Window>
```

### Supported Controls

Currently supported Terminal.Gui controls:

- **Window** - Top-level window with title bar and border
- **Label** - Text display control
- **Button** - Clickable button
- **View** - Generic container (via GenericGenerator)

Additional controls can be added by implementing new generators in the `Generators/` folder.

## Architecture

### Source Generator Pipeline

1. **XAML Discovery** (`Terminal.Gui.Xaml.targets`):
   - MSBuild target automatically adds `*.xaml` files as `AdditionalFiles`
   - Only activates when Terminal.Gui is referenced

2. **Incremental Source Generator** (`CodeGenerator.cs`):
   - Implements `IIncrementalGenerator` for efficient compilation
   - Monitors `AdditionalFiles` for `.xaml` files
   - Verifies Terminal.Gui references in compilation
   - Generates one `.g.cs` file per `.xaml` file

3. **XAML Parsing** (`XamlLoader.cs`):
   - Uses `System.Xml.Linq` to parse XAML
   - Converts XML to `ElementNode` tree structure
   - Automatically filters out XML comments

4. **Code Generation** (`Generators/`):
   - `GeneratorFactory` selects appropriate generator per control type
   - `WindowGenerator` creates partial class with `InitializeComponent()` method
   - `ObjectParsingHelpers` parses Pos/Dim expressions with operator support
   - Generates Roslyn `SyntaxTree` nodes for type-safe C# output

5. **Output**:
   - Generated `.g.cs` files written to `obj/Generated/`
   - Visible in IDE solution explorer
   - Compiled with rest of project

### Generated Code Pattern

For a XAML file named `MyWindow.xaml`, the generator:

1. Searches for a partial class named `MyWindow` in the compilation
2. Extracts the namespace from the partial class declaration
3. Generates a matching partial class that inherits from `Window`
4. Creates `InitializeComponent()` method that:
   - Instantiates controls using object initializer syntax
   - Sets properties directly in initializers
   - Adds controls to the window via `this.Add()`

### Type System

The generator uses a property type dictionary in `ObjectParsingHelpers.cs` to:
- Map XAML attributes to C# types
- Validate property names at compile time
- Generate type-appropriate expressions (string literals, bool values, Pos/Dim calls)
- Provide helpful error messages for unknown properties

## Development

### Building from Source

```bash
# Build entire solution
dotnet build src/Terminal.Gui.Xaml.sln

# Build release configuration
dotnet build src/Terminal.Gui.Xaml.sln -c Release

# Run example applications
dotnet run --project src/Examples/Xaml
dotnet run --project src/Examples/Xaml.Mvvm
```

### Submodule Management

Terminal.Gui is included as a git submodule at `src/Terminal.Gui`:

```bash
# Update submodule to latest on tracked branch (v2_develop)
git submodule update --remote

# Switch to a different branch
cd src/Terminal.Gui
git checkout v2_develop
cd ../..
git add src/Terminal.Gui
git commit -m "Updated Terminal.Gui submodule"
```

**Important:** Never modify files in `src/Terminal.Gui`. All Terminal.Gui changes should be contributed to the upstream repository.

### Generated Files Location

Generated files are placed in `obj/Generated/Terminal.Gui.Xaml/Terminal.Gui.Xaml.CodeGenerator/` and are:
- **Not committed** to source control (excluded by `.gitignore`)
- **Automatically regenerated** on each build
- **Visible in IDE** for debugging and verification

### Adding New Control Generators

To add support for a new Terminal.Gui control:

1. Create a new generator class in `Generators/` (e.g., `TextFieldGenerator.cs`)
2. Inherit from `Generator` base class
3. Override `GenerateStatements()` or `GenerateClass()` as needed
4. Register in `GeneratorFactory.cs`

Example:

```csharp
internal sealed class TextFieldGenerator : Generator
{
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Generate code for TextField instantiation
    }
}
```

### Error Handling

The generator provides detailed error diagnostics:

- **XAML001**: XAML parsing errors (invalid XML, unknown elements)
- **XAML002**: Code generation errors (unexpected exceptions)

Errors appear in Visual Studio Error List with:
- Error message describing the issue
- Path to the problematic XAML file
- Suggested fixes when possible

## Testing

Unit tests are planned in the `Terminal.Gui.Xaml.Tests` project. Testing strategy:

- **Parser tests**: Verify XAML parsing and ElementNode tree construction
- **Generator tests**: Verify generated C# code syntax trees
- **Integration tests**: Compile and run generated code
- **Error case tests**: Verify diagnostic messages for invalid XAML

## Roadmap

### Short Term
- [ ] Support for more controls (TextField, TextView, ListView, etc.)
- [ ] Event handler syntax (`Button.Accept="OnButtonClick"`)
- [ ] Named elements with `x:Name` attribute
- [ ] Property access from partial class constructor

### Medium Term
- [ ] Data binding syntax (`Text="{Binding PropertyName}"`)
- [ ] Attached properties for advanced layouts
- [ ] XAML resources and styles
- [ ] Custom control support
- [ ] Improved error messages with line numbers

### Long Term
- [ ] NuGet package distribution
- [ ] Visual Studio XAML designer support
- [ ] Live preview tooling
- [ ] Code-behind auto-generation
- [ ] XAML IntelliSense improvements

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Code Style

- Follow standard C# conventions
- Use nullable reference types (`#nullable enable`)
- Add XML documentation comments for public APIs
- Keep generator code focused and testable

### Testing Your Changes

```bash
# Build the generator
dotnet build src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj

# Test with example projects
dotnet clean src/Examples/Xaml
dotnet build src/Examples/Xaml
dotnet run --project src/Examples/Xaml
```

Check the generated files in `src/Examples/Xaml/obj/Generated/` to verify your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **[Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)** - The excellent terminal UI toolkit this project builds upon
- **Microsoft XAML** - Inspiration for the declarative UI markup approach and InitializeComponent pattern
- **[Roslyn](https://github.com/dotnet/roslyn)** - The .NET compiler platform that enables source generation
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** - MVVM helpers used in examples

## Related Resources

- [Terminal.Gui Documentation](https://gui-cs.github.io/Terminal.GuiV2Docs/)
- [Terminal.Gui GitHub Repository](https://github.com/gui-cs/Terminal.Gui)
- [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md)
- [C# Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)

## Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues)
- **Discussions**: Ask questions or share ideas in [GitHub Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xaml/discussions)
- **Examples**: Check the `src/Examples/` folder for working examples

---

**Note**: This project targets Terminal.Gui v2 and is under active development. APIs and features may change as the project evolves.
