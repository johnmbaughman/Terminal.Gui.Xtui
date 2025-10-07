# Frequently Asked Questions

Common questions and answers about Terminal.Gui.Xaml development, troubleshooting, and best practices.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Quick Links**: [Getting Started](getting-started.md) • [Troubleshooting](#troubleshooting) • [Performance](#performance)

## General Questions

### What is Terminal.Gui.Xaml?

**Q**: What is Terminal.Gui.Xaml and how does it relate to Terminal.Gui?

**A**: Terminal.Gui.Xaml is a declarative UI framework that brings XAML-based development to terminal applications. It's built on top of Terminal.Gui v2+ and allows you to define your terminal UI using familiar XAML syntax instead of imperative C# code.

Key benefits:
- **Declarative UI**: Define layouts in XAML instead of code
- **Data Binding**: Two-way binding with INotifyPropertyChanged
- **Design-Time Support**: IntelliSense and validation in IDEs
- **Familiar Patterns**: Similar to WPF/UWP for existing developers

### How does it compare to other UI frameworks?

**Q**: How does Terminal.Gui.Xaml compare to WPF, Avalonia, or MAUI?

**A**: Terminal.Gui.Xaml targets terminal/console applications specifically:

| Framework | Target | Rendering | Deployment |
|-----------|--------|-----------|------------|
| **Terminal.Gui.Xaml** | Terminal/Console | Text-based | Cross-platform CLI |
| **WPF** | Windows Desktop | Graphics | Windows only |
| **Avalonia** | Cross-platform Desktop | Graphics | Desktop apps |
| **MAUI** | Mobile/Desktop | Graphics | Multiple platforms |

Choose Terminal.Gui.Xaml for:
- Command-line tools with rich UIs
- Server administration interfaces
- Development tools and utilities
- Applications that need to run in SSH/remote sessions

### What .NET versions are supported?

**Q**: What versions of .NET can I use with Terminal.Gui.Xaml?

**A**: Terminal.Gui.Xaml requires **.NET 8.0 or later**. This ensures:
- Latest C# language features
- Performance optimizations
- Long-term support (LTS) compatibility
- Modern API surfaces

For older .NET versions, consider using Terminal.Gui directly.

## Getting Started

### How do I create my first application?

**Q**: What's the quickest way to get started with Terminal.Gui.Xaml?

**A**: Follow these steps:

```powershell
# Create new console application
dotnet new console -n MyTerminalApp
cd MyTerminalApp

# Add Terminal.Gui.Xaml package
dotnet add package Terminal.Gui.Xaml

# Run the app
dotnet run
```

See our complete [Getting Started Guide](getting-started.md) for detailed instructions.

### Do I need Visual Studio?

**Q**: Can I develop Terminal.Gui.Xaml applications without Visual Studio?

**A**: Yes! Terminal.Gui.Xaml works with:
- **Visual Studio 2022**: Full IntelliSense and debugging
- **VS Code**: With C# extension for syntax highlighting
- **JetBrains Rider**: Full IDE support
- **Command Line**: Using `dotnet` CLI tools

XAML IntelliSense is available in Visual Studio and VS Code with appropriate extensions.

### Can I use existing Terminal.Gui code?

**Q**: I have existing Terminal.Gui applications. Can I migrate to XAML?

**A**: Yes, you can migrate incrementally:

1. **Hybrid Approach**: Use XAML for new windows, keep existing code
2. **Control-by-Control**: Replace individual controls with XAML equivalents
3. **Full Migration**: Convert entire application to XAML

See our [Migration Overview](migration/v1-to-v2.md) for high-level changes and strategies.

## XAML and Syntax

### Is this "real" XAML?

**Q**: Is Terminal.Gui.Xaml compatible with WPF/UWP XAML?

**A**: Terminal.Gui.Xaml uses XAML syntax but targets terminal controls:

**Similar Concepts**:
- Element hierarchy and nesting
- Property setting via attributes
- Data binding syntax `{Binding}`
- Resource dictionaries and styles

**Terminal-Specific**:
- Character-based positioning (`Pos`, `Dim`)
- Terminal colors and attributes
- Keyboard-focused navigation
- Text-based rendering

You can leverage existing XAML knowledge while learning terminal-specific concepts.

### Why doesn't my WPF XAML work directly?

**Q**: I copied XAML from my WPF app but it doesn't work. Why?

**A**: While the syntax is similar, the control libraries are different:

```xml
<!-- WPF -->
<Grid>
    <TextBox Text="{Binding Name}" />
    <Button Content="OK" Click="OnClick" />
</Grid>

<!-- Terminal.Gui.Xaml -->
<StackView Orientation="Vertical">
    <TextField Text="{Binding Name}" />
    <Button Text="OK" Clicked="OnClick" />
</StackView>
```

Key differences:
- Different control names (`TextBox` → `TextField`)
- Different properties (`Content` → `Text`)
- Different events (`Click` → `Clicked`)
- Different layout containers (`Grid` → `StackView`)

### How do I handle complex layouts?

**Q**: How do I create complex, responsive layouts in character-based terminals?

**A**: Use Terminal.Gui's positioning system:

```xml
<Window Width="80" Height="25">
    <!-- Responsive sizing -->
    <StackView Orientation="Vertical">
        <FrameView Title="Header" Height="3" Width="Dim.Fill()" />
        
        <!-- Split content area -->
        <StackView Orientation="Horizontal" Height="Dim.Fill(2)">
            <FrameView Title="Sidebar" Width="20" />
            <FrameView Title="Content" Width="Dim.Fill()" />
        </StackView>
        
        <Label Text="Status" Height="1" Width="Dim.Fill()" />
    </StackView>
</Window>
```

Learn more in our [Layout Concepts](concepts/layout.md) guide.

## Data Binding

### How does data binding work?

**Q**: How do I set up data binding in Terminal.Gui.Xaml?

**A**: Implement `INotifyPropertyChanged` in your view models:

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

Bind in XAML:
```xml
<TextField Text="{Binding Name, Mode=TwoWay}" />
```

See our [Data Binding Guide](guides/bind-data.md) for complete examples.

### Why isn't my binding updating?

**Q**: My bound data changes but the UI doesn't update. What's wrong?

**A**: Common issues:

1. **Missing INotifyPropertyChanged**:
   ```csharp
   // ❌ Bad - no notifications
   public string Name { get; set; }
   
   // ✅ Good - raises PropertyChanged
   public string Name
   {
       get => _name;
       set { _name = value; OnPropertyChanged(); }
   }
   ```

2. **Incorrect DataContext**:
   ```csharp
   // Ensure DataContext is set
   window.DataContext = new PersonViewModel();
   ```

3. **Wrong binding mode**:
   ```xml
   <!-- For input controls, use TwoWay -->
   <TextField Text="{Binding Name, Mode=TwoWay}" />
   ```

## Performance

### How fast is Terminal.Gui.Xaml?

**Q**: What's the performance impact of using XAML vs. code-only Terminal.Gui?

**A**: Terminal.Gui.Xaml performance characteristics:

| Metric | Target | Notes |
|--------|--------|-------|
| **XAML Parse Time** | < 100ms | One-time cost at startup |
| **Render Performance** | > 30 FPS | Same as Terminal.Gui |
| **Memory Overhead** | < 50MB | For typical applications |
| **Startup Time** | < 50ms | Additional overhead |

The XAML parsing happens once at startup, so runtime performance is equivalent to hand-coded Terminal.Gui.

### How can I optimize performance?

**Q**: My application feels slow. How can I optimize it?

**A**: Common optimization strategies:

1. **Minimize Bindings**: Only bind what changes frequently
2. **Lazy Loading**: Load complex UIs on demand
3. **Virtual Lists**: Use virtualization for large data sets
4. **Efficient Updates**: Batch property changes

```csharp
// ✅ Batch updates efficiently
using (var suspension = viewModel.SuspendNotifications())
{
    viewModel.Property1 = value1;
    viewModel.Property2 = value2;
    viewModel.Property3 = value3;
} // Single update notification
```

Performance guidance is coming soon; in the meantime, prefer simple layouts and minimize unnecessary bindings.

## Troubleshooting

### Common Build Errors

**Q**: My project won't build. What are common issues?

**A**: Check these common problems:

1. **Missing Package Reference**:
   ```xml
   <PackageReference Include="Terminal.Gui.Xaml" Version="1.0.0" />
   ```

2. **Wrong Target Framework**:
   ```xml
   <TargetFramework>net8.0</TargetFramework>
   ```

3. **Missing XAML Build Action**:
   ```xml
   <ItemGroup>
     <EmbeddedResource Include="**/*.xaml" />
   </ItemGroup>
   ```

4. **Namespace Issues**:
   ```xml
   xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
   xmlns:x="http://www.w3.org/1999/xlink"
   ```

### Runtime Exceptions

**Q**: My app crashes at runtime. How do I debug XAML issues?

**A**: Common runtime issues:

1. **XAML Parse Errors**:
   ```
   XamlParseException: Cannot find control 'MyControl'
   ```
   **Solution**: Check control names and namespaces

2. **Binding Errors**:
   ```
   BindingException: Property 'Name' not found on type 'PersonViewModel'
   ```
   **Solution**: Verify property names and DataContext

3. **Type Conversion**:
   ```
   ArgumentException: Cannot convert 'string' to 'int'
   ```
   **Solution**: Use appropriate data types or converters

Enable debugging output:
```csharp
// Add to Program.cs for detailed binding information
XamlConfiguration.DebugBindings = true;
```

### XAML IntelliSense Not Working

**Q**: I don't get IntelliSense for XAML in my editor. How do I fix this?

**A**: Solutions by editor:

**Visual Studio 2022**:
1. Install latest VS 2022 (17.7+)
2. Ensure C# workload is installed
3. Restart VS after adding Terminal.Gui.Xaml package

**VS Code**:
1. Install "C# for Visual Studio Code" extension
2. Install "XAML" extension by Microsoft
3. Restart VS Code and reload workspace

**Rider**:
1. Enable XAML support in settings
2. Invalidate caches and restart if needed

## Platform-Specific

### Does it work on Linux?

**Q**: Can I run Terminal.Gui.Xaml applications on Linux?

**A**: Yes! Terminal.Gui.Xaml is cross-platform:

**Supported Platforms**:
- Windows (PowerShell, Command Prompt, Windows Terminal)
- Linux (bash, zsh, most terminal emulators)
- macOS (Terminal.app, iTerm2)

**Platform Considerations**:
- Colors may vary between terminals
- Some Unicode symbols might not display correctly
- Keyboard shortcuts may differ
- Font rendering varies by terminal

Test your application on target platforms for best results.

### What about macOS?

**Q**: Are there any macOS-specific considerations?

**A**: macOS works well with some considerations:

**✅ Works Great**:
- Terminal.app with default settings
- iTerm2 with Unicode support
- VS Code integrated terminal

**⚠️ Consider**:
- Some Unicode symbols may not render
- Terminal color schemes vary
- Keyboard shortcuts (Cmd vs Ctrl)

## Advanced Scenarios

### Can I create custom controls?

**Q**: How do I create reusable custom controls in Terminal.Gui.Xaml?

**A**: Yes, you can create custom controls:

1. **User Controls**: Combine existing controls
2. **Custom Controls**: Inherit from base classes
3. **Templated Controls**: Define control templates

Example custom control:
```csharp
public partial class PersonEditor : UserControl
{
    public PersonEditor()
    {
        InitializeComponent(); // Loads PersonEditor.xaml
    }
}
```

Custom controls guidance is coming soon; for now, you can compose `UserControl` from existing controls.

### How do I handle validation?

**Q**: What's the best way to handle input validation?

**A**: Multiple validation strategies:

1. **Property Validation**: Validate in property setters
2. **IDataErrorInfo**: Implement validation interface  
3. **Validation Rules**: Create reusable validation logic
4. **Commands**: Validate before executing actions

```csharp
public class PersonViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    public string this[string propertyName]
    {
        get
        {
            return propertyName switch
            {
                nameof(Email) when !IsValidEmail(Email) => "Invalid email format",
                nameof(Age) when Age < 0 || Age > 120 => "Age must be between 0 and 120",
                _ => null
            };
        }
    }
}
```

### Can I use dependency injection?

**Q**: How do I integrate with dependency injection containers?

**A**: Yes, Terminal.Gui.Xaml works with DI:

```csharp
// Setup DI container
var services = new ServiceCollection();
services.AddTransient<IDataService, DataService>();
services.AddTransient<MainWindowViewModel>();

var provider = services.BuildServiceProvider();

// Use in application
var viewModel = provider.GetService<MainWindowViewModel>();
var window = new MainWindow { DataContext = viewModel };
```

This enables MVVM patterns with proper separation of concerns.

## Getting Help

### Where can I get help?

**Q**: I'm stuck and need help. What are my options?

**A**: Multiple support channels:

**Free Support**:
- [GitHub Issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues) - Bug reports and feature requests
- [GitHub Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xaml/discussions) - Q&A and community help
- [Stack Overflow](https://stackoverflow.com/questions/tagged/terminal-gui-xaml) - Use tag `terminal-gui-xaml`

**Documentation**:
- [Getting Started Guide](getting-started.md)
- [API Reference](../api/index.md)
- [Examples Collection](examples/index.md)

**Community**:
- Share your projects and get feedback
- Contribute examples and documentation
- Help other developers

### How do I report bugs?

**Q**: I found a bug. How should I report it?

**A**: Please report bugs on [GitHub Issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues):

**Include**:
- Terminal.Gui.Xaml version
- .NET version and OS
- Minimal reproduction case
- Expected vs actual behavior
- Stack trace if applicable

**Template**:
```
**Version**: Terminal.Gui.Xaml 1.0.0, .NET 8.0, Windows 11
**Issue**: Button click handler not called

**Reproduction**:
1. Create button with Clicked event
2. Run application
3. Click button
4. Handler never executes

**Expected**: Handler should be called
**Actual**: No response to button clicks
```

### How can I contribute?

**Q**: I want to contribute to Terminal.Gui.Xaml. How do I start?

**A**: Contributions are welcome! See our [Contributing Guide](contributing/index.md) for:

- Code contributions and pull requests
- Documentation improvements  
- Example submissions
- Testing and bug reports
- Feature suggestions and feedback

Start with [good first issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) to get familiar with the codebase.

---

## Still Have Questions?

If your question isn't answered here:

1. **Search existing documentation** using the search box
2. **Check [GitHub Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xaml/discussions)** for community Q&A
3. **Create a new discussion** if you can't find an answer
4. **Report documentation gaps** to help improve this FAQ

---

> **Quick Links**: [Getting Started](getting-started.md) • [API Reference](../api/index.md) • [Examples](examples/index.md) • [Contributing](contributing/index.md)
