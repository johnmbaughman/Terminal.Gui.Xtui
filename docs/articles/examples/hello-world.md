# Hello World Example

A minimal Terminal.Gui.Xaml application showing the basic window structure and XAML rendering.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable  
> **Tested With**: .NET 8.0, Windows 11, PowerShell 7.4

## Related APIs

This example demonstrates these fundamental APIs:

### Application Framework
- **`Application`** - Framework initialization (`Init`, `Run`, `Shutdown`)
- **`Application.Top`** - Top-level application container
- **`Application.MainLoop`** - Event loop management

### Window and Layout
- **`Window`** - Primary container with title and sizing
- **`StackView`** - Vertical layout container
- **`Label`** - Text display control

### XAML Integration
- **`XamlLoader`** - XAML parsing and loading
- **[IXamlParser](../../api/Terminal.Gui.Xaml.Parsing.IXamlParser.yml)** - XAML parsing interface

> **💡 Pro Tip**: This is the minimum viable Terminal.Gui.Xaml application. Always call `Application.Init()` before creating windows and `Application.Shutdown()` in a `finally` block.

## What You'll Learn

- Basic Terminal.Gui.Xaml project setup
- XAML window definition with simple controls
- Application lifecycle management (Init/Run/Shutdown)
- Cross-platform terminal compatibility

## Prerequisites

- .NET 8+ SDK installed
- Terminal.Gui.Xaml package (latest version)
- Basic understanding of console applications
- No prior XAML experience required

## Complete Example

### Project Setup

Create a new console application:

```powershell
# Create new project
dotnet new console -n HelloWorldApp
cd HelloWorldApp

# Add Terminal.Gui.Xaml package
dotnet add package Terminal.Gui.Xaml

# Verify project structure
dir
```

### Program.cs

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace HelloWorldApp;

class Program
{
    static void Main(string[] args)
    {
        // Initialize Terminal.Gui framework
        Application.Init();
        
        try
        {
            // Create and show the main window
            var window = new HelloWorldWindow();
            Application.Run(window);
        }
        finally
        {
            // Clean up resources
            Application.Shutdown();
        }
    }
}
```

### HelloWorldWindow.xaml

```xml
<Window x:Class="HelloWorldApp.HelloWorldWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Hello World - Terminal.Gui.Xaml"
        Width="50" Height="10">
    
    <StackView Orientation="Vertical" 
               HorizontalAlignment="Center" 
               VerticalAlignment="Center"
               Spacing="1">
        
        <Label Text="Hello, Terminal.Gui.Xaml!"
               HorizontalAlignment="Center"
               ForegroundColor="Green" />
        
        <Label Text="Welcome to declarative terminal UI development"
               HorizontalAlignment="Center"
               ForegroundColor="Cyan" />
        
        <Label Text="Press Ctrl+C to exit"
               HorizontalAlignment="Center"
               ForegroundColor="Gray" />
        
    </StackView>
</Window>
```

### HelloWorldWindow.xaml.cs

```csharp
using Terminal.Gui.Xaml;

namespace HelloWorldApp;

public partial class HelloWorldWindow : Window
{
    public HelloWorldWindow()
    {
        InitializeComponent();
    }
}
```

### Project File

Ensure your `HelloWorldApp.csproj` includes the necessary configuration:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Terminal.Gui.Xaml" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="**/*.xaml" />
  </ItemGroup>

</Project>
```

## Running the Example

### Windows (PowerShell)
```powershell
# Build the application
dotnet build

# Run the application  
dotnet run
```

### Linux/macOS (Bash)
```bash
# Build the application
dotnet build

# Run the application
dotnet run
```

## Expected Output

When you run the application, you should see:

```
┌─ Hello World - Terminal.Gui.Xaml ─────────┐
│                                           │
│                                           │
│          Hello, Terminal.Gui.Xaml!       │
│                                           │
│    Welcome to declarative terminal UI     │
│           development                     │
│                                           │
│           Press Ctrl+C to exit            │
│                                           │
│                                           │
└───────────────────────────────────────────┘
```

## Step-by-Step Explanation

### 1. Application Initialization
```csharp
Application.Init();
```
- Initializes Terminal.Gui framework
- Sets up terminal display and input handling
- Must be called before creating any UI elements

### 2. Window Definition
```xml
<Window Title="..." Width="50" Height="10">
```
- Defines a bordered window container
- Size in character cells (50 columns × 10 rows)
- Title appears in the window's top border

### 3. Layout Container
```xml
<StackView Orientation="Vertical" Spacing="1">
```
- Arranges child controls vertically
- Centers content both horizontally and vertically
- Adds 1 character space between items

### 4. Text Display
```xml
<Label Text="Hello, Terminal.Gui.Xaml!" ForegroundColor="Green" />
```
- Displays static text
- Supports color styling for terminal compatibility
- Automatically handles text alignment

### 5. Resource Cleanup
```csharp
Application.Shutdown();
```
- Restores terminal to original state
- Releases framework resources
- Always call in a finally block

## Key Concepts

### XAML Structure
- **Declarative UI**: Define layout and controls in XAML markup
- **Namespaces**: Standard XAML namespaces for Terminal.Gui.Xaml
- **Properties**: Set control properties as XML attributes

### Terminal Compatibility
- **Character-based**: All sizes and positions in character cells
- **Color Support**: Uses terminal color capabilities
- **Cross-platform**: Works on Windows, Linux, macOS terminals

### Performance
- **Fast Parsing**: XAML parsing completes in milliseconds
- **Efficient Rendering**: Optimized for terminal display updates
- **Low Memory**: Minimal memory footprint for simple applications

## Platform Differences

### Terminal Emulators
Different terminal applications may render colors slightly differently:

- **Windows Terminal**: Full color support, Unicode characters
- **PowerShell**: Good color support, basic Unicode
- **Command Prompt**: Limited color support
- **Linux terminals**: Varies by terminal (gnome-terminal, xterm, etc.)
- **macOS Terminal**: Good color support, Unicode characters

### Font Considerations
- Use standard ASCII characters for maximum compatibility
- Unicode characters may not display consistently across terminals
- Test in your target deployment environment

## Next Steps

Now that you have a basic window working:

1. **Add Interaction**: Try the [Button Click Example](button-click.md)
2. **Learn Layout**: Explore [Basic Layout Example](basic-layout.md)
3. **Understand Concepts**: Read [Layout Concepts](../concepts/layout.md)
4. **Build More**: Follow the [Create a Window Guide](../guides/create-window.md)

## Troubleshooting

### Application Doesn't Start

**Issue**: Nothing appears when running `dotnet run`
```
Solution: Check that Application.Init() is called before creating windows
```

**Issue**: Exception on startup
```
Solution: Verify Terminal.Gui.Xaml package is installed and referenced
```

### Display Problems

**Issue**: Window appears but controls are missing
```
Solution: Ensure XAML files are marked as EmbeddedResource in the project file
```

**Issue**: Colors don't display correctly
```
Solution: Test in different terminal applications; some have limited color support
```

### Build Errors

**Issue**: XAML compilation errors
```
Solution: Check XAML syntax, ensure proper namespace declarations
```

**Issue**: InitializeComponent() not found
```
Solution: Verify code-behind class inherits from Window and uses partial class modifier
```

### Performance Issues

**Issue**: Slow startup or high memory usage
```
Solution: This should not occur with this simple example. Check for:
- Correct Terminal.Gui.Xaml version
- No infinite loops in constructor
- Proper resource disposal
```

## Related Topics

### More Examples
- **[Basic Layout](basic-layout.md)** - Form controls and layout containers
- **[Button Click](button-click.md)** - Event handling and user interaction
- **[Multi-Window App](multi-window.md)** - Complex application structure

### Learning Path
- **[Create a Window Guide](../guides/create-window.md)** - Detailed window creation tutorial
- **[Handle Input Guide](../guides/handle-input.md)** - User interaction patterns
- **[Data Binding Guide](../guides/bind-data.md)** - MVVM pattern implementation

### API Reference
- `Application` - Framework initialization and lifecycle
- `Window` - Main container with properties and events
- `StackView` - Layout container for vertical/horizontal arrangement  
- `Label` - Text display control with styling options
- `XamlLoader` - XAML parsing and object creation

### Concepts
- [Layout](../concepts/layout.md) - Positioning and sizing basics
- [Binding](../concepts/binding.md) - Data binding fundamentals
- [Events](../concepts/events.md) - Event model and patterns
