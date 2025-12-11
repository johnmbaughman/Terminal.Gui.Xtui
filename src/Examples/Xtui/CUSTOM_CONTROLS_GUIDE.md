# Custom Controls and MVVM Integration Guide

This guide demonstrates how to use custom controls and MVVM viewmodels with Terminal.Gui.Xtui's expandable namespace handling.

## Overview

The XTUI namespace system allows you to:
- Reference custom control libraries using XML namespace prefixes
- Integrate MVVM viewmodels seamlessly
- Mix built-in Terminal.Gui controls with custom controls
- Maintain clean separation of concerns in your UI code

## Namespace Declaration

Declare custom namespaces in your XTUI file using `xmlns:prefix="Namespace"`:

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:my="MyApp.Controls"
        xmlns:vm="MyApp.ViewModels">
    <!-- Your UI here -->
</Window>
```

### Built-in Namespaces

- **Default namespace** (`xmlns="http://schemas.terminal.gui/xtui"`): Maps to `Terminal.Gui.Views`
  - All standard Terminal.Gui controls (Window, Label, Button, etc.)
  - No prefix required: `<Label Text="Hello" />`

### Custom Namespaces

- **Custom control namespace** (e.g., `xmlns:my="MyApp.Controls"`):
  - Maps directly to the C# namespace
  - Use with prefix: `<my:CustomButton Text="Click Me" />`
  - Generator resolves to: `MyApp.Controls.CustomButton`

- **Viewmodel namespace** (e.g., `xmlns:vm="MyApp.ViewModels"`):
  - Reference viewmodel types in your UI
  - Use with prefix: `<vm:LoginForm />`
  - Generator resolves to: `MyApp.ViewModels.LoginForm`

## How It Works

### 1. Namespace Resolution

The `XtuiLoader` class handles namespace resolution:

```csharp
// In XtuiLoader.cs
private static string ResolveElementTypeName(
    string elementName, 
    string defaultNamespaceUri, 
    Dictionary<string, string> namespaces)
{
    // Handles prefixed elements: vm:LoginViewModel
    // Handles default namespace: Label
    // Returns fully qualified type: MyApp.ViewModels.LoginViewModel
}
```

### 2. Code Generation

The generator creates proper C# code with using directives:

```csharp
// Generated InitializeComponent method
#nullable enable
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using MyApp.Controls;      // Custom control namespace
using MyApp.ViewModels;    // Viewmodel namespace

namespace MyApp
{
    public partial class MyWindow : Window
    {
        private CustomButton _loginButton;
        
        private void InitializeComponent()
        {
            var custombutton0 = new CustomButton() { 
                Text = "Login",
                ButtonStyle = "Primary"
            };
            _loginButton = custombutton0;
            this.Add(_loginButton);
        }
    }
}
```

## Creating Custom Controls

### Example 1: Simple Custom Control

```csharp
// MyApp.Controls.cs
using Terminal.Gui;
using Terminal.Gui.Views;

namespace MyApp.Controls;

public class CustomButton : Button
{
    public CustomButton()
    {
        BorderStyle = BorderStyle.Double;
    }

    public string ButtonStyle { get; set; } = "Default";
}
```

**Usage in XTUI:**

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:my="MyApp.Controls">
    <my:CustomButton Id="_loginButton" 
                     Text="Login" 
                     ButtonStyle="Primary"
                     X="10" Y="5" />
</Window>
```

### Example 2: Composite Custom Control

```csharp
namespace MyApp.Controls;

public class StatusPanel : View
{
    private Label _titleLabel;
    private Label _messageLabel;
    private ProgressBar _progressBar;

    public StatusPanel()
    {
        // Initialize child controls
        _titleLabel = new Label { X = 1, Y = 0, Width = Dim.Fill(1) };
        _messageLabel = new Label { X = 1, Y = 1, Width = Dim.Fill(1) };
        _progressBar = new ProgressBar { X = 1, Y = 2, Width = Dim.Fill(1) };
        
        Add(_titleLabel, _messageLabel, _progressBar);
    }

    public string StatusTitle { get; set; } = "Status";
    public string StatusMessage { get; set; } = "Ready";
    public float Progress { get; set; } = 0.0f;
}
```

**Usage in XTUI:**

```xml
<my:StatusPanel Id="_statusPanel" 
                StatusTitle="Loading"
                StatusMessage="Please wait..."
                Progress="0.5"
                X="5" Y="10" Width="40" Height="5" />
```

## MVVM Integration

### Creating ViewModels

```csharp
// MyApp.ViewModels.cs
using System.ComponentModel;

namespace MyApp.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class LoginViewModel : ViewModelBase
{
    private string _username = string.Empty;
    
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }
    
    public void Login()
    {
        // Login logic
    }
}
```

### Future Enhancement: Data Binding

While the current generator supports referencing viewmodel types, future enhancements could include:

```xml
<!-- Future syntax example -->
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:vm="MyApp.ViewModels"
        DataContext="{vm:LoginViewModel}">
    
    <TextField Id="_usernameField" 
               Text="{Binding Username}"
               X="10" Y="5" />
    
    <Button Text="Login" 
            Command="{Binding LoginCommand}"
            X="10" Y="7" />
</Window>
```

## Best Practices

### 1. Organize Namespaces

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"          <!-- Built-in controls -->
        xmlns:my="MyApp.Controls"                          <!-- Custom controls -->
        xmlns:vm="MyApp.ViewModels"                        <!-- ViewModels -->
        xmlns:common="MyCompany.Common.Controls">          <!-- Shared library -->
```

### 2. Use Meaningful Prefixes

- `my` - Your application's custom controls
- `vm` - ViewModels
- `common` - Common/shared controls
- `third` - Third-party control libraries

### 3. Keep Controls Simple

Custom controls should:
- Have parameterless constructors
- Expose properties that map to XTUI attributes
- Follow Terminal.Gui conventions
- Be self-contained and reusable

### 4. Namespace Inheritance

Child elements inherit parent namespaces:

```xml
<Window xmlns:my="MyApp.Controls">
    <my:CustomPanel>
        <!-- my: prefix available here too -->
        <my:CustomButton Text="OK" />
    </my:CustomPanel>
</Window>
```

## Technical Details

### Namespace Resolution Algorithm

1. **Parse xmlns attributes**: Extract namespace declarations from element
2. **Inherit parent namespaces**: Child elements get parent's namespace dictionary
3. **Resolve element name**:
   - If prefixed (`vm:LoginForm`): Look up prefix in namespace dictionary
   - If not prefixed (`Label`): Use default namespace
4. **Map to C# namespace**: Convert XML namespace URI to C# namespace
5. **Generate qualified type**: Combine namespace + local name

### Code Generation Flow

1. **XtuiLoader**: Parse XTUI → ElementNode tree with qualified type names
2. **GeneratorFactory**: Look up generator using local type name
3. **Generators**: Generate code using local names for variables/objects
4. **WindowGenerator**: Collect all namespaces, emit using directives

### Type Name Handling

Throughout the pipeline:
- `ElementNode.ElementTypeName`: Stores fully qualified name (e.g., `MyApp.Controls.CustomButton`)
- Code generation: Extracts local name for variable/object creation (e.g., `CustomButton`)
- Using directives: Generated from collected namespaces

## Examples in This Repository

See these files for working examples:

- `CustomControlExample.xtui` - Example XTUI file with custom namespaces
- `MyApp.Controls.cs` - Example custom control implementations
- `MyApp.ViewModels.cs` - Example MVVM viewmodel implementations

## Extensibility

The namespace system is fully extensible:

1. **Add new namespace mappings**: Simply declare `xmlns:prefix="Your.Namespace"`
2. **No generator changes needed**: Existing generators handle all qualified names
3. **Mix and match**: Use multiple custom namespaces in one file
4. **Framework agnostic**: Any .NET types with parameterless constructors work

## Troubleshooting

### "Namespace prefix 'X' is not defined"

Ensure you declared the namespace:
```xml
<Window xmlns:custom="My.Namespace">
    <custom:MyControl />  <!-- Prefix must be declared above -->
</Window>
```

### "Type 'X' not found"

- Verify the namespace in `xmlns:prefix="Namespace"` is correct
- Ensure the type exists in that namespace
- Check that your project references the assembly containing the type

### Generated code doesn't compile

- Check that using directives are generated for custom namespaces
- Verify custom control types have parameterless constructors
- Ensure property types match XTUI attribute values

## Future Enhancements

Planned improvements:
- **Data binding syntax**: `{Binding PropertyName}`
- **Command binding**: `{Command MethodName}`
- **Resource references**: `{StaticResource Key}`
- **Template support**: Define reusable control templates
- **Style system**: CSS-like styling for controls
