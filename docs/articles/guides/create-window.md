# Create a Window

This guide walks you through creating your first Terminal.Gui.Xaml window with basic layout and controls.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable

## Related APIs

Before diving into the implementation, familiarize yourself with these key APIs used in this guide:

### Core Components
- **[Window](../../api/Terminal.Gui.Xaml.Window.yml)** - Main container for your application interface
- **[StackView](../../api/Terminal.Gui.Xaml.StackView.yml)** - Layout container that arranges controls vertically or horizontally
- **[Label](../../api/Terminal.Gui.Xaml.Label.yml)** - Display text and provide user guidance
- **[TextField](../../api/Terminal.Gui.Xaml.TextField.yml)** - Single-line text input control
- **[Button](../../api/Terminal.Gui.Xaml.Button.yml)** - Interactive control for user actions
- **[CheckBox](../../api/Terminal.Gui.Xaml.CheckBox.yml)** - Boolean input control

### Layout and Sizing
- **[Dim](../../api/Terminal.Gui.Xaml.Dim.yml)** - Dimension system for responsive layouts (`Dim.Fill()`, `Dim.Sized()`)
- **[Pos](../../api/Terminal.Gui.Xaml.Pos.yml)** - Position system for control placement

### Styling and Appearance
- **[Color](../../api/Terminal.Gui.Xaml.Color.yml)** - Color system for foreground and background styling
- **[BorderStyle](../../api/Terminal.Gui.Xaml.BorderStyle.yml)** - Border appearance options

### Event Handling
- **[EventArgs](../../api/Terminal.Gui.Xaml.EventArgs.yml)** - Base event argument type
- **[Click Events](../../api/Terminal.Gui.Xaml.ClickEvent.yml)** - Button and control interaction events

> **💡 Pro Tip**: Keep the API reference open while following this guide to explore additional properties and methods for each control.

## Prerequisites

- .NET 8+ SDK installed
- Terminal.Gui.Xaml package referenced
- Basic familiarity with XAML syntax

## Step 1: Create the Project

Create a new console application:

```powershell
dotnet new console -n MyTerminalApp
cd MyTerminalApp
dotnet add package Terminal.Gui.Xaml
```

## Step 2: Create a Simple Window XAML

Create `MainWindow.xaml`:

```xml
<Window x:Class="MyTerminalApp.MainWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="My First Terminal Window"
        Width="80" Height="25">
    
    <StackView Orientation="Vertical" Spacing="1">
        <Label Text="Welcome to Terminal.Gui.Xaml!" 
               HorizontalAlignment="Center" 
               ForegroundColor="Green" />
        
        <FrameView Title="User Information" Height="8">
            <StackView Orientation="Vertical" Spacing="1" Margin="1">
                <StackView Orientation="Horizontal">
                    <Label Text="Name:" Width="10" />
                    <TextField Name="nameField" Width="Dim.Fill()" />
                </StackView>
                
                <StackView Orientation="Horizontal">
                    <Label Text="Email:" Width="10" />
                    <TextField Name="emailField" Width="Dim.Fill()" />
                </StackView>
                
                <CheckBox Name="subscribeCheck" Text="Subscribe to newsletter" />
            </StackView>
        </FrameView>
        
        <StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="2">
            <Button Name="saveButton" Text="Save" Width="8" />
            <Button Name="cancelButton" Text="Cancel" Width="8" />
        </StackView>
        
        <Label Name="statusLabel" Text="" 
               HorizontalAlignment="Center" 
               ForegroundColor="Yellow" />
    </StackView>
</Window>
```

## Step 3: Create the Code-Behind

Create `MainWindow.xaml.cs`:

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace MyTerminalApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetupEventHandlers();
    }
    
    private void SetupEventHandlers()
    {
        // Handle save button click
        saveButton.Click += OnSaveClick;
        
        // Handle cancel button click  
        cancelButton.Click += OnCancelClick;
        
        // Validate input as user types
        nameField.TextChanged += ValidateInput;
        emailField.TextChanged += ValidateInput;
    }
    
    private void OnSaveClick(object sender, EventArgs e)
    {
        if (IsInputValid())
        {
            var name = nameField.Text?.ToString() ?? "";
            var email = emailField.Text?.ToString() ?? "";
            var subscribe = subscribeCheck.Checked;
            
            statusLabel.Text = $"Saved: {name} ({email}) Subscribe: {subscribe}";
            statusLabel.ForegroundColor = Color.Green;
        }
        else
        {
            statusLabel.Text = "Please fill in all required fields";
            statusLabel.ForegroundColor = Color.Red;
        }
    }
    
    private void OnCancelClick(object sender, EventArgs e)
    {
        // Clear form
        nameField.Text = "";
        emailField.Text = "";
        subscribeCheck.Checked = false;
        statusLabel.Text = "Form cleared";
        statusLabel.ForegroundColor = Color.Yellow;
    }
    
    private void ValidateInput(object sender, EventArgs e)
    {
        saveButton.Enabled = IsInputValid();
    }
    
    private bool IsInputValid()
    {
        return !string.IsNullOrWhiteSpace(nameField.Text?.ToString()) &&
               !string.IsNullOrWhiteSpace(emailField.Text?.ToString());
    }
}
```

## Step 4: Update Program.cs

Update your `Program.cs` to launch the window:

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using MyTerminalApp;

// Initialize Terminal.Gui
Application.Init();

try
{
    // Create and show the main window
    var mainWindow = new MainWindow();
    Application.Run(mainWindow);
}
finally
{
    // Clean up
    Application.Shutdown();
}
```

## Step 5: Run the Application

Build and run your application:

```powershell
dotnet build
dotnet run
```

You should see a terminal window with:
- A welcome message
- A framed input form with name and email fields
- A checkbox for newsletter subscription
- Save and Cancel buttons
- A status label for feedback

## Understanding the Code

### Window Structure
```xml
<Window Title="..." Width="80" Height="25">
```
- Defines the main container with title and size
- Size is in character cells (80 columns × 25 rows)

### Layout with StackView
```xml
<StackView Orientation="Vertical" Spacing="1">
```
- Arranges child controls vertically
- Adds 1 character spacing between items

### Input Controls
```xml
<TextField Name="nameField" Width="Dim.Fill()" />
```
- Named controls can be accessed in code-behind
- `Dim.Fill()` expands to available width

### Event Handling
```csharp
saveButton.Click += OnSaveClick;
```
- Connects UI events to code methods
- Enables interactive behavior

## Next Steps

Now that you have a basic window, you can:

1. **Add Data Binding**: Connect controls to view models
   - See [Bind Data Guide](bind-data.md)

2. **Improve Layout**: Use advanced positioning
   - See [Layout Concepts](../concepts/layout.md)

3. **Handle Complex Input**: Add validation and commands
   - See [Handle Input Guide](handle-input.md)

4. **Add Navigation**: Multiple windows or views
   - See [Navigation Guide](navigation.md)

## Troubleshooting

### Common Issues

**Window doesn't appear**
- Check that `Application.Init()` is called before creating windows
- Ensure `Application.Run()` is called with your window

**Controls not responding**
- Verify event handlers are attached in `SetupEventHandlers()`
- Check control names match between XAML and code-behind

**Layout problems**
- Use `Width="Dim.Fill()"` for flexible sizing
- Set explicit widths for labels: `Width="10"`
- Add `Spacing` to StackView for better appearance

**Terminal display issues**
- Test in different terminal types (cmd, PowerShell, bash)
- Ensure terminal supports colors if using `ForegroundColor`
- Check terminal size is adequate for your window size

## Related Topics

### API Reference
- [Window Class](../../api/Terminal.Gui.Xaml.Window.yml) - Window API documentation
- [StackView Class](../../api/Terminal.Gui.Xaml.StackView.yml) - Stack layout container
- [Label Class](../../api/Terminal.Gui.Xaml.Label.yml) - Text display control
- [TextField Class](../../api/Terminal.Gui.Xaml.TextField.yml) - Text input control
- [Button Class](../../api/Terminal.Gui.Xaml.Button.yml) - Button control
- [IXamlParser Interface](../../api/Terminal.Gui.Xaml.IXamlParser.yml) - XAML parsing

### Concepts
- [Layout System](../concepts/layout.md) - Understanding layout principles
- [Event Handling](../concepts/events.md) - Working with events
- [Data Binding](../concepts/binding.md) - Connecting data to UI

### Examples
- [Hello World](../examples/hello-world.md) - Simplest possible window
- [Basic Layout](../examples/basic-layout.md) - Layout containers in action
- [Button Click](../examples/button-click.md) - Event handling example

### Next Steps
- [Data Binding Guide](bind-data.md) - Connect your UI to data
- [Handling Input](handle-input.md) - Advanced input processing
- [Navigation Guide](navigation.md) - Multiple windows and views
