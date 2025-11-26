# Terminal.Gui.Xtui

A Roslyn source generator that enables XML-based UI design, similar to XAML, for [Terminal.Gui v2](https://github.com/gui-cs/Terminal.Gui) applications. Write your terminal UIs declaratively using XTUI (XAML Terminal User Interface) syntax, and let the generator produce clean, efficient C# code at compile time.

## Overview

Terminal.Gui.Xtui brings declarative UI design to Terminal.Gui through a compile-time source generator. Inspired by Microsoft's XAML implementation, this project allows you to define terminal-based user interfaces using familiar XML syntax while maintaining Terminal.Gui's performance characteristics.

### Key Features

- **Compile-Time Code Generation**: `.xtui` files are transformed into C# code during compilation using Roslyn incremental source generators
- **Zero Runtime Overhead**: No parsing at runtime—generated code directly instantiates Terminal.Gui controls
- **Type-Safe**: Leverages C#'s type system with compile-time validation
- **InitializeComponent Pattern**: Follows the familiar WPF/WinForms pattern with partial classes
- **Rich Layout Support**: Full support for Terminal.Gui's `Pos` and `Dim` positioning system including operators
- **Automatic Discovery**: `.xtui` files are automatically detected and processed when Terminal.Gui is referenced
- **IDE Integration**: Generated files visible in your IDE under `obj/Generated` folder

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Terminal.Gui v2 (referenced as submodule or package)
- Any IDE with C# support (Visual Studio, VS Code, Rider)

### Installation

1. **Clone the repository with submodules:**

```bash
git clone --recurse-submodules https://github.com/johnmbaughman/Terminal.Gui.Xtui.git
cd Terminal.Gui.Xtui
```

If you already cloned without `--recurse-submodules`:

```bash
git submodule update --init --recursive
```

2. **Build the solution:**

```bash
dotnet build src/Terminal.Gui.Xtui.sln
```

3. **Run the example:**

```bash
dotnet run --project src/Examples/Xtui
```

## Project Structure

```
Terminal.Gui.Xtui/
├── src/
│   ├── Terminal.Gui/                    # Git submodule (Terminal.Gui v2)
│   ├── Terminal.Gui.Xtui/               # Source generator library
│   │   ├── CodeGenerator.cs             # Roslyn incremental source generator
│   │   ├── XtuiLoader.cs                # XTUI parser (using System.Xml.Linq)
│   │   ├── ElementNode.cs               # Internal XTUI tree representation
│   │   ├── Generators/                  # Control-specific code generators
│   │   │   ├── WindowGenerator.cs       # Generates Window with InitializeComponent
│   │   │   ├── LabelGenerator.cs        # Label generator
│   │   │   ├── ButtonGenerator.cs       # Button generator
│   │   │   ├── GenericGenerator.cs      # Fallback for other controls
│   │   │   ├── GeneratorFactory.cs      # Factory for selecting generators
│   │   │   └── ObjectParsingHelpers.cs  # Pos/Dim expression parsing
│   │   ├── Mappers/                     # Type mappers
│   │   │   └── EnumMapper.cs            # Enum value mapping
│   │   ├── Terminal.Gui.Xtui.xsd        # XML Schema for IntelliSense
│   │   └── buildTransitive/             # MSBuild integration
│   │       └── Terminal.Gui.Xtui.targets # Automatic .xtui file discovery
│   ├── Examples/
│   │   ├── Xtui/                        # Basic example
│   │   │   ├── MyWindow.xtui            # XTUI UI definition
│   │   │   ├── MyWindow.cs              # Partial class with constructor
│   │   │   └── Program.cs               # Application entry point
│   │   └── Xtui.Mvvm/                   # MVVM pattern example
│   │       ├── MyWindow.xtui
│   │       ├── MyWindow.cs
│   │       └── Program.cs               # Includes MainViewModel
│   └── Terminal.Gui.Xtui.Tests/         # Unit tests (future)
├── .gitignore
├── .gitmodules
├── LICENSE
└── README.md
```

## Usage

### Basic Example

The best reference is the [Xtui example project](src/Examples/Xtui). Here's how it works:

**1. Create a `.xtui` file (`MyWindow.xtui`):**

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

**2. Create a partial class matching the `.xtui` filename (`MyWindow.cs`):**

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
    <ProjectReference Include="..\..\Terminal.Gui.Xtui\Terminal.Gui.Xtui.csproj"
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
  </ItemGroup>

  <!-- Import the targets that auto-discover .xtui files -->
  <Import Project="..\..\Terminal.Gui.Xtui\buildTransitive\Terminal.Gui.Xtui.targets" />
</Project>
```

**4. Use your window in the application (`Program.cs`):**

```csharp
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace Xtui;

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

namespace Xtui
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

See the [Xtui.Mvvm example project](src/Examples/Xtui.Mvvm) for MVVM pattern usage with CommunityToolkit.Mvvm:

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

## XTUI Syntax

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

XTUI comments are automatically stripped during parsing:

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

1. **XTUI Discovery** (`Terminal.Gui.Xtui.targets`):
   - MSBuild target automatically adds `*.xtui` files as `AdditionalFiles`
   - Only activates when Terminal.Gui is referenced

2. **Incremental Source Generator** (`CodeGenerator.cs`):
   - Implements `IIncrementalGenerator` for efficient compilation
   - Monitors `AdditionalFiles` for `.xtui` files
   - Verifies Terminal.Gui references in compilation
   - Generates one `.g.cs` file per `.xtui` file

3. **XTUI Parsing** (`XtuiLoader.cs`):
   - Uses `System.Xml.Linq` to parse XTUI (XML-based format)
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

For an XTUI file named `MyWindow.xtui`, the generator:

1. Searches for a partial class named `MyWindow` in the compilation
2. Extracts the namespace from the partial class declaration
3. Generates a matching partial class that inherits from `Window`
4. Creates `InitializeComponent()` method that:
   - Instantiates controls using object initializer syntax
   - Sets properties directly in initializers
   - Adds controls to the window via `this.Add()`

### Type System

The generator uses a property type dictionary in `ObjectParsingHelpers.cs` to:
- Map XTUI attributes to C# types
- Validate property names at compile time
- Generate type-appropriate expressions (string literals, bool values, Pos/Dim calls)
- Provide helpful error messages for unknown properties

## Development

### Building from Source

```bash
# Build entire solution
dotnet build src/Terminal.Gui.Xtui.sln

# Build release configuration
dotnet build src/Terminal.Gui.Xtui.sln -c Release

# Run example applications
dotnet run --project src/Examples/Xtui
dotnet run --project src/Examples/Xtui.Mvvm
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

Generated files are placed in `obj/Generated/Terminal.Gui.Xtui/Terminal.Gui.Xtui.CodeGenerator/` and are:
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

- **XTUI001**: XTUI parsing errors (invalid XML, unknown elements)
- **XTUI002**: Code generation errors (unexpected exceptions)

Errors appear in Visual Studio Error List with:
- Error message describing the issue
- Path to the problematic XTUI file
- Suggested fixes when possible

## Testing

Unit tests are planned in the `Terminal.Gui.Xtui.Tests` project. Testing strategy:

- **Parser tests**: Verify XTUI parsing and ElementNode tree construction
- **Generator tests**: Verify generated C# code syntax trees
- **Integration tests**: Compile and run generated code
- **Error case tests**: Verify diagnostic messages for invalid XTUI

## Roadmap

### Short Term
- [x] IntelliSense support via XSD schema
- [ ] Support for more controls (TextField, TextView, ListView, etc.)
- [ ] Event handler syntax (`Button.Accept="OnButtonClick"`)
- [ ] Named elements with `x:Name` attribute
- [ ] Property access from partial class constructor

### Medium Term
- [ ] Data binding syntax (`Text="{Binding PropertyName}"`)
- [ ] Attached properties for advanced layouts
- [ ] XTUI resources and styles
- [ ] Custom control support
- [ ] Improved error messages with line numbers

### Long Term
- [ ] NuGet package distribution
- [ ] Visual Studio XTUI designer support
- [ ] Live preview tooling
- [ ] Code-behind auto-generation

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
dotnet build src/Terminal.Gui.Xtui/Terminal.Gui.Xtui.csproj

# Test with example projects
dotnet clean src/Examples/Xtui
dotnet build src/Examples/Xtui
dotnet run --project src/Examples/Xtui
```

Check the generated files in `src/Examples/Xtui/obj/Generated/` to verify your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **[Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)** - The excellent terminal UI toolkit this project builds upon
- **Microsoft XAML** - Inspiration for the declarative UI markup approach and InitializeComponent pattern
- **[Roslyn](https://github.com/dotnet/roslyn)** - The .NET compiler platform that enables source generation
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** - MVVM helpers used in examples

## IntelliSense Support

XTUI files support IntelliSense/code completion in Visual Studio, Visual Studio Code, and JetBrains Rider through the included XML Schema Definition (XSD) file.

### Visual Studio

#### Automatic Setup (When Using NuGet Package)

When you reference the `Terminal.Gui.Xtui` NuGet package, the schema is automatically included and your `.xtui` files should get IntelliSense support.

#### Manual Setup (For Development)

1. Ensure your `.xtui` files include the XML namespace declaration at the top:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://schemas.terminal.gui/xtui ../../Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd">
    <!-- Your UI elements here -->
</Window>
```

2. The `xsi:schemaLocation` attribute should point to the relative path of the `Terminal.Gui.Xtui.xsd` file.

3. Visual Studio will automatically provide IntelliSense for:
   - Element names (Window, Label, Button, etc.)
   - Attribute names (Text, X, Y, Width, Height, etc.)
   - Documentation tooltips for elements and attributes

#### Configuring XML Editor Association

If Visual Studio doesn't automatically recognize `.xtui` files as XML:

1. Right-click a `.xtui` file in Solution Explorer
2. Select "Open With..."
3. Choose "XML (Text) Editor"
4. Click "Set as Default" (optional)
5. Click OK

### Visual Studio Code

#### Prerequisites

Install the **XML Language Support by Red Hat** extension:
1. Open VS Code
2. Press `Ctrl+Shift+X` (or `Cmd+Shift+X` on Mac) to open Extensions
3. Search for "XML"
4. Install **XML** by Red Hat (extension ID: `redhat.vscode-xml`)

#### Workspace Setup

For the best experience, add these files to your workspace root (this repo already includes them in `.vscode/`):

**`.vscode/settings.json`:**
```json
{
  "xml.fileAssociations": [
    {
      "pattern": "**/*.xtui",
      "systemId": "Terminal.Gui.Xtui.xsd"
    }
  ],
  "files.associations": {
    "*.xtui": "xml"
  }
}
```

**`.vscode/extensions.json`:**
```json
{
  "recommendations": [
    "redhat.vscode-xml"
  ]
}
```

#### Schema Configuration

**Option 1: Using schemaLocation (Recommended)**

Ensure your `.xtui` files include the XML declaration with schema location:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://schemas.terminal.gui/xtui ../../Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd">
    <!-- Your UI elements here -->
</Window>
```

The relative path in `xsi:schemaLocation` should point to where the `Terminal.Gui.Xtui.xsd` file is located.

**Option 2: Using Workspace Settings**

The workspace `.vscode/settings.json` can map `.xtui` files to the schema:

```json
{
  "xml.fileAssociations": [
    {
      "pattern": "**/*.xtui",
      "systemId": "path/to/Terminal.Gui.Xtui.xsd"
    }
  ]
}
```

Replace `path/to/` with the actual path to the schema file (e.g., `Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd`).

#### Verifying It Works

1. Open a `.xtui` file in VS Code
2. Check the bottom-right corner - it should show "XML" as the language mode
3. Start typing `<` inside the Window element - you should see completion suggestions for `Label`, `Button`, etc.
4. Inside an element, start typing an attribute name - you should see suggestions like `Text`, `X`, `Y`, etc.
5. Hover over element or attribute names to see documentation tooltips

#### Troubleshooting VS Code

**No IntelliSense appearing:**
- Verify the XML extension is installed and enabled
- Check that the file is recognized as XML (bottom-right corner should say "XML")
- Ensure the schema path in `xsi:schemaLocation` or settings is correct
- Try reloading the window: `Ctrl+Shift+P` → "Developer: Reload Window"

**Schema not found errors:**
- Check the relative path to the XSD file
- Verify the XSD file exists at that location
- Try using an absolute path temporarily to verify it works

**Extension not working:**
- Check Output panel: View → Output → Select "XML Support" from dropdown
- Look for any error messages about schema loading

### JetBrains Rider

#### Automatic Setup (When Using NuGet Package)

Rider will automatically recognize the schema when you reference the `Terminal.Gui.Xtui` NuGet package.

#### Manual Setup (For Development)

1. Ensure your `.xtui` files include the XML declaration as shown above.

2. If Rider doesn't automatically detect the schema:
   - Go to **Settings** → **Languages & Frameworks** → **Schemas and DTDs** → **XML Schemas**
   - Click **+** to add a new schema
   - Browse to `Terminal.Gui.Xtui.xsd`
   - Set the namespace to `http://schemas.terminal.gui/xtui`
   - Click OK

3. Associate `.xtui` extension with XML files:
   - Go to **Settings** → **Editor** → **File Types**
   - Find "XML files" in the list
   - Add `*.xtui` to the file name patterns
   - Click OK

### IntelliSense Features

Once configured, you'll get:

#### 1. Element Completion
Start typing `<` and you'll see a list of available elements:
- `Window`
- `Label`
- `Button`
- More controls as they're added

#### 2. Attribute Completion
Inside an element, start typing and you'll see available attributes:
- `Text` - The text content (string)
- `X`, `Y` - Position (number, percentage, or expression like `{Center}`)
- `Width`, `Height` - Dimensions (number, percentage, or expression like `{Fill}`)
- `Visible`, `Enabled`, `CanFocus` - Boolean properties
- And many more...

#### 3. Documentation
Hover over any element or attribute to see documentation describing its purpose.

#### 4. Validation
The IDE will highlight errors if you:
- Use invalid element names
- Use invalid attribute names
- Have malformed XML

### Supported Attributes

Common attributes available on most controls:

#### String Properties
- `Text` - Display text
- `Title` - Title text (Window, Dialog)
- `Id` - Identifier

#### Position (Pos type)
- `X` - Horizontal position
  - Number: `X="10"`
  - Percentage: `X="50%"`
  - Expression: `X="{Center}"`, `X="{Center + 10}"`, `X="{AnchorEnd - 5}"`
- `Y` - Vertical position (same formats as X)

#### Dimensions (Dim type)
- `Width` - Width
  - Number: `Width="20"`
  - Percentage: `Width="80%"`
  - Expression: `Width="{Fill}"`, `Width="{Fill - 5}"`, `Width="{Auto}"`
- `Height` - Height (same formats as Width)

#### Boolean Properties
- `Visible` - Visibility state
- `Enabled` - Enabled state
- `CanFocus` - Can receive focus
- `HasFocus` - Currently has focus
- And more...

### IntelliSense Troubleshooting

#### IntelliSense Not Working

1. **Check XML Declaration**: Ensure your file starts with `<?xml version="1.0" encoding="utf-8"?>`
2. **Check Namespace**: Verify the `xmlns` attribute is set to `http://schemas.terminal.gui/xtui`
3. **Check Schema Location**: Ensure `xsi:schemaLocation` points to the correct relative path to `Terminal.Gui.Xtui.xsd`
4. **Restart IDE**: Sometimes a restart helps the IDE pick up schema changes
5. **Clear Caches**: 
   - **Visual Studio**: Delete `.vs` folder in solution directory
   - **Rider**: File → Invalidate Caches / Restart

#### Schema Not Found Errors

If you see errors about the schema not being found:

1. Verify the relative path in `xsi:schemaLocation` is correct
2. Check that `Terminal.Gui.Xtui.xsd` exists at that location
3. For NuGet package users, ensure the package is properly restored

### Adding New Elements

As new Terminal.Gui controls are supported in XTUI, the `Terminal.Gui.Xtui.xsd` file is updated to include them. After updating the package, IntelliSense will automatically reflect the new elements and attributes.

## Related Resources

- [Terminal.Gui Documentation](https://gui-cs.github.io/Terminal.GuiV2Docs/)
- [Terminal.Gui GitHub Repository](https://github.com/gui-cs/Terminal.Gui)
- [Roslyn Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md)
- [C# Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)

## Support

- **Issues**: Report bugs or request features on [GitHub Issues](https://github.com/johnmbaughman/Terminal.Gui.Xtui/issues)
- **Discussions**: Ask questions or share ideas in [GitHub Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xtui/discussions)
- **Examples**: Check the `src/Examples/` folder for working examples

---

**Note**: This project targets Terminal.Gui v2 and is under active development. APIs and features may change as the project evolves.
