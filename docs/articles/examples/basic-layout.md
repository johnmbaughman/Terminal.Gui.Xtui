# Basic Layout Example

Demonstrates form layout with input controls, labels, and responsive sizing using Terminal.Gui.Xaml.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable  
> **Tested With**: .NET 8.0, Windows 11, PowerShell 7.4

## Related APIs

This example demonstrates these layout and input control APIs:

### Layout Controls
- **[StackView](../../api/Terminal.Gui.Xaml.StackView.yml)** - Vertical/horizontal layout container (`Orientation`, `Spacing`)
- **[FrameView](../../api/Terminal.Gui.Xaml.FrameView.yml)** - Bordered container with title (`Title`, `Border`)
- **[Window](../../api/Terminal.Gui.Xaml.Window.yml)** - Main window container (`Title`, `Width`, `Height`)

### Input Controls
- **[TextField](../../api/Terminal.Gui.Xaml.TextField.yml)** - Single-line text input (`Text`, `Width`)
- **[ComboBox](../../api/Terminal.Gui.Xaml.ComboBox.yml)** - Dropdown selection control (`ItemsSource`, `SelectedItem`)
- **[CheckBox](../../api/Terminal.Gui.Xaml.CheckBox.yml)** - Boolean checkbox control (`IsChecked`, `Text`)
- **[Button](../../api/Terminal.Gui.Xaml.Button.yml)** - Action button (`Text`, `Click` event, `IsEnabled`)

### Layout System
- **[Dim](../../api/Terminal.Gui.Xaml.Dim.yml)** - Dimension system (`Dim.Fill()`, `Dim.Sized()`)
- **[Pos](../../api/Terminal.Gui.Xaml.Pos.yml)** - Position system (`Pos.Left()`, `Pos.Center()`)

### Styling
- **[Color](../../api/Terminal.Gui.Xaml.Color.yml)** - Color system for styling (`ForegroundColor`, `BackgroundColor`)
- **[Label](../../api/Terminal.Gui.Xaml.Label.yml)** - Text display (`Text`, styling properties)

> **💡 Pro Tip**: Use `FrameView` to group related controls and `StackView` for consistent spacing. `Dim.Fill()` makes controls responsive to window resizing.

## What You'll Learn

- Form layout with labels and input controls
- Responsive sizing using `Dim.Fill()` and `Pos` helpers
- Container controls (`FrameView`, `StackView`)
- Basic input handling and validation
- Status display and user feedback

## Prerequisites

- Completed [Hello World Example](hello-world.md)
- .NET 8+ SDK installed
- Basic understanding of XAML layout concepts

## Complete Example

### Program.cs

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace BasicLayoutApp;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();
        
        try
        {
            var window = new BasicLayoutWindow();
            Application.Run(window);
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
```

### BasicLayoutWindow.xaml

```xml
<Window x:Class="BasicLayoutApp.BasicLayoutWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Basic Layout - Terminal.Gui.Xaml Demo"
        Width="70" Height="20">
    
    <StackView Orientation="Vertical" Spacing="1">
        
        <!-- Header -->
        <Label Text="User Registration Form"
               HorizontalAlignment="Center"
               ForegroundColor="Cyan" />
        
        <!-- Main form in a frame -->
        <FrameView Title="Personal Information" Height="12">
            <StackView Orientation="Vertical" Spacing="1" Margin="1">
                
                <!-- Name row -->
                <StackView Orientation="Horizontal">
                    <Label Text="Full Name:" Width="15" />
                    <TextField Name="nameField"
                               PlaceholderText="Enter your full name"
                               Width="Dim.Fill()"
                               Height="1" />
                </StackView>
                
                <!-- Email row -->
                <StackView Orientation="Horizontal">
                    <Label Text="Email Address:" Width="15" />
                    <TextField Name="emailField"
                               PlaceholderText="user@example.com"
                               Width="Dim.Fill()"
                               Height="1" />
                </StackView>
                
                <!-- Age row -->
                <StackView Orientation="Horizontal">
                    <Label Text="Age:" Width="15" />
                    <TextField Name="ageField"
                               PlaceholderText="18-120"
                               Width="10"
                               Height="1" />
                    <Label Text="years old" 
                           X="Pos.Right(ageField) + 1" />
                </StackView>
                
                <!-- Country selection -->
                <StackView Orientation="Horizontal">
                    <Label Text="Country:" Width="15" />
                    <ComboBox Name="countryCombo"
                              Width="20"
                              Height="1" />
                </StackView>
                
                <!-- Newsletter checkbox -->
                <CheckBox Name="newsletterCheck"
                          Text="Subscribe to newsletter"
                          X="15" />
                
                <!-- Terms checkbox -->
                <CheckBox Name="termsCheck"
                          Text="I agree to the terms and conditions"
                          X="15" />
                
            </StackView>
        </FrameView>
        
        <!-- Action buttons -->
        <StackView Orientation="Horizontal" 
                   HorizontalAlignment="Center" 
                   Spacing="3">
            <Button Name="submitButton"
                    Text="Submit"
                    Width="12"
                    Height="1"
                    IsDefault="true" />
            <Button Name="clearButton"
                    Text="Clear"
                    Width="12" 
                    Height="1" />
            <Button Name="cancelButton"
                    Text="Cancel"
                    Width="12"
                    Height="1" />
        </StackView>
        
        <!-- Status bar -->
        <Label Name="statusLabel"
               Text="Ready - Fill out the form above"
               HorizontalAlignment="Center"
               ForegroundColor="Yellow" />
        
    </StackView>
</Window>
```

### BasicLayoutWindow.xaml.cs

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using System.Text.RegularExpressions;

namespace BasicLayoutApp;

public partial class BasicLayoutWindow : Window
{
    private readonly string[] countries = new[]
    {
        "United States", "Canada", "United Kingdom", "Germany", 
        "France", "Japan", "Australia", "Brazil", "India", "Other"
    };
    
    public BasicLayoutWindow()
    {
        InitializeComponent();
        SetupForm();
        SetupEventHandlers();
    }
    
    private void SetupForm()
    {
        // Initialize country dropdown
        countryCombo.SetSource(countries);
        countryCombo.SelectedItem = 0; // Default to first item
        
        // Set initial status
        UpdateStatus("Ready - Fill out the form above", Color.Yellow);
    }
    
    private void SetupEventHandlers()
    {
        // Button event handlers
        submitButton.Clicked += OnSubmitClicked;
        clearButton.Clicked += OnClearClicked;
        cancelButton.Clicked += OnCancelClicked;
        
        // Real-time validation as user types
        nameField.TextChanged += (s, e) => ValidateForm();
        emailField.TextChanged += (s, e) => ValidateForm();
        ageField.TextChanged += (s, e) => ValidateForm();
        
        // Checkbox validation
        termsCheck.Toggled += (s, e) => ValidateForm();
        
        // Initial validation
        ValidateForm();
    }
    
    private void OnSubmitClicked(object sender, EventArgs e)
    {
        if (!IsFormValid())
        {
            UpdateStatus("Please fill out all required fields correctly", Color.Red);
            return;
        }
        
        // Simulate form submission
        var name = nameField.Text?.ToString() ?? "";
        var email = emailField.Text?.ToString() ?? "";
        var age = ageField.Text?.ToString() ?? "";
        var country = countries[countryCombo.SelectedItem];
        var newsletter = newsletterCheck.Checked;
        
        var message = $"Submitted: {name} ({email}), Age {age}, {country}";
        if (newsletter) message += " - Newsletter subscribed";
        
        UpdateStatus(message, Color.Green);
    }
    
    private void OnClearClicked(object sender, EventArgs e)
    {
        // Clear all form fields
        nameField.Text = "";
        emailField.Text = "";
        ageField.Text = "";
        countryCombo.SelectedItem = 0;
        newsletterCheck.Checked = false;
        termsCheck.Checked = false;
        
        UpdateStatus("Form cleared", Color.Yellow);
        ValidateForm();
    }
    
    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Close the application
        Application.RequestStop();
    }
    
    private void ValidateForm()
    {
        var isValid = IsFormValid();
        submitButton.Enabled = isValid;
        
        if (!isValid && !string.IsNullOrEmpty(GetValidationMessage()))
        {
            UpdateStatus(GetValidationMessage(), Color.Red);
        }
        else if (isValid)
        {
            UpdateStatus("Form is valid - ready to submit", Color.Green);
        }
    }
    
    private bool IsFormValid()
    {
        return IsNameValid() && 
               IsEmailValid() && 
               IsAgeValid() && 
               termsCheck.Checked;
    }
    
    private bool IsNameValid()
    {
        var name = nameField.Text?.ToString() ?? "";
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 2;
    }
    
    private bool IsEmailValid()
    {
        var email = emailField.Text?.ToString() ?? "";
        return !string.IsNullOrWhiteSpace(email) && 
               email.Contains('@') && 
               email.Contains('.');
    }
    
    private bool IsAgeValid()
    {
        var ageText = ageField.Text?.ToString() ?? "";
        return int.TryParse(ageText, out var age) && age >= 18 && age <= 120;
    }
    
    private string GetValidationMessage()
    {
        if (!IsNameValid()) return "Name must be at least 2 characters";
        if (!IsEmailValid()) return "Please enter a valid email address";
        if (!IsAgeValid()) return "Age must be between 18 and 120";
        if (!termsCheck.Checked) return "You must agree to the terms";
        return "";
    }
    
    private void UpdateStatus(string message, Color color)
    {
        statusLabel.Text = message;
        statusLabel.ForegroundColor = color;
    }
}
```

### Project File

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

```powershell
# Create and setup project
dotnet new console -n BasicLayoutApp
cd BasicLayoutApp
dotnet add package Terminal.Gui.Xaml

# Build and run
dotnet build
dotnet run
```

## Expected Output

You'll see a registration form with:

```
┌─ Basic Layout - Terminal.Gui.Xaml Demo ─────────────────────────┐
│                    User Registration Form                       │
│                                                                 │
│ ┌─ Personal Information ─────────────────────────────────────┐  │
│ │                                                           │  │
│ │ Full Name:      [Enter your full name              ]     │  │
│ │                                                           │  │
│ │ Email Address:  [user@example.com                  ]     │  │
│ │                                                           │  │
│ │ Age:            [18-120  ] years old                     │  │
│ │                                                           │  │
│ │ Country:        [United States     ▼]                    │  │
│ │                                                           │  │
│ │                 ☐ Subscribe to newsletter                │  │
│ │                                                           │  │
│ │                 ☐ I agree to the terms and conditions    │  │
│ │                                                           │  │
│ └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│            [  Submit  ]   [  Clear  ]   [  Cancel  ]            │
│                                                                 │
│               Ready - Fill out the form above                   │
└─────────────────────────────────────────────────────────────────┘
```

## Step-by-Step Explanation

### 1. Responsive Layout Structure
```xml
<StackView Orientation="Vertical" Spacing="1">
```
- Vertical arrangement of major form sections
- Consistent spacing between sections

### 2. Form Container
```xml
<FrameView Title="Personal Information" Height="12">
```
- Groups related form fields visually
- Fixed height ensures consistent layout

### 3. Input Row Layout
```xml
<StackView Orientation="Horizontal">
    <Label Text="Full Name:" Width="15" />
    <TextField Width="Dim.Fill()" />
</StackView>
```
- Fixed-width labels for alignment
- `Dim.Fill()` makes input fields expand to available width

### 4. Advanced Positioning
```xml
<Label Text="years old" X="Pos.Right(ageField) + 1" />
```
- Positions label relative to another control
- Adds spacing with `+ 1`

### 5. Button Group Layout
```xml
<StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="3">
```
- Centers button group horizontally
- Even spacing between buttons

### 6. Real-time Validation
```csharp
nameField.TextChanged += (s, e) => ValidateForm();
```
- Validates form as user types
- Provides immediate feedback

## Key Layout Concepts

### Flexible Sizing
- **Fixed Width**: `Width="15"` for consistent alignment
- **Fill Available**: `Width="Dim.Fill()"` for responsive sizing
- **Relative Position**: `X="Pos.Right(control) + offset"`

### Container Hierarchy
- **Window**: Root container
- **StackView**: Layout panels for arrangement
- **FrameView**: Visual grouping with borders

### Spacing and Alignment
- **Spacing**: Consistent gaps between elements
- **Alignment**: Center, left, right positioning
- **Margins**: Inner padding within containers

## Advanced Features Demonstrated

### Form Validation
```csharp
private bool IsEmailValid()
{
    var email = emailField.Text?.ToString() ?? "";
    return !string.IsNullOrWhiteSpace(email) && 
           email.Contains('@') && 
           email.Contains('.');
}
```

### Dynamic UI Updates
```csharp
private void ValidateForm()
{
    var isValid = IsFormValid();
    submitButton.Enabled = isValid; // Enable/disable based on validation
}
```

### Status Communication
```csharp
private void UpdateStatus(string message, Color color)
{
    statusLabel.Text = message;
    statusLabel.ForegroundColor = color; // Green/Yellow/Red feedback
}
```

## Platform Considerations

### Terminal Width
The form is designed for 70-character width terminals. On smaller terminals:
- Horizontal scrolling may occur
- Consider responsive breakpoints for mobile terminals

### Color Support
Status colors (Green/Yellow/Red) work on most modern terminals but may vary:
- **Windows Terminal**: Full color support
- **Legacy terminals**: May show different shades or fallback colors

## Next Steps

1. **Add Data Binding**: Convert to MVVM pattern with [Data Binding Guide](../guides/bind-data.md)
2. **Handle Events**: Learn advanced input handling with [Button Click Example](button-click.md)
3. **Navigation**: Connect multiple forms with [Navigation Guide](../guides/navigation.md)

## Troubleshooting

### Layout Issues

**Controls overlap or misalign**
```
Solution: Check container sizes and Width/Height properties
- Use explicit sizes for containers
- Ensure child controls fit within parent bounds
```

**Text gets cut off**
```
Solution: Increase container width or use Dim.Fill()
- Test with different terminal sizes
- Use responsive sizing where possible
```

### Validation Problems

**Form validation not working**
```
Solution: Verify event handlers are attached
- Check TextChanged events are wired up
- Ensure ValidateForm() is called on initialization
```

**Button states not updating**
```
Solution: Call ValidateForm() after state changes
- Update validation when checkboxes change
- Refresh button states in validation logic
```

## Related Topics

### Examples by Complexity
- **[Hello World](hello-world.md)** - Minimal application setup (start here!)
- **[Button Click](button-click.md)** - Event handling and user interaction
- **[Form Validation](form-validation.md)** - Advanced input validation
- **[Multi-Window App](multi-window.md)** - Complex application structure

### Learning Guides
- **[Create a Window](../guides/create-window.md)** - Step-by-step window creation
- **[Handle Input](../guides/handle-input.md)** - User interaction patterns  
- **[Navigation](../guides/navigation.md)** - Multi-form applications and flow

### API Reference
- **[StackView](../../api/Terminal.Gui.Xaml.StackView.yml)** - Layout container with orientation and spacing
- **[FrameView](../../api/Terminal.Gui.Xaml.FrameView.yml)** - Bordered grouping container
- **[TextField](../../api/Terminal.Gui.Xaml.TextField.yml)** - Text input control with validation
- **[ComboBox](../../api/Terminal.Gui.Xaml.ComboBox.yml)** - Dropdown selection with data binding
- **[CheckBox](../../api/Terminal.Gui.Xaml.CheckBox.yml)** - Boolean input with three-state support
- **[Dim](../../api/Terminal.Gui.Xaml.Dim.yml)** - Responsive dimension system

### Concepts  
- **[Layout System](../concepts/layout.md)** - Understanding positioning and sizing
- **[Control Hierarchy](../concepts/controls.md)** - Container and child relationships
- **[Event Handling](../concepts/events.md)** - User interaction patterns
