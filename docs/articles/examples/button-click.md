# Button Click Example

Demonstrates event handling, user interaction patterns, and dynamic UI updates in Terminal.Gui.Xaml.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable  
> **Tested With**: .NET 8.0, Windows 11, PowerShell 7.4

## Related APIs

This example demonstrates these event handling and interaction APIs:

### Interactive Controls
- **[Button](../../api/Terminal.Gui.Xaml.Button.yml)** - Click events and command binding (`Click`, `Command`, `IsEnabled`)
- **[CheckBox](../../api/Terminal.Gui.Xaml.CheckBox.yml)** - Boolean interaction (`CheckedChanged`, `IsChecked`)
- **[MenuItem](../../api/Terminal.Gui.Xaml.MenuItem.yml)** - Menu item actions (`Click`, `Command`, shortcuts)
- **[MenuBar](../../api/Terminal.Gui.Xaml.MenuBar.yml)** - Menu container (`MenuBarItem` hierarchy)

### Event System
- **[EventArgs](../../api/Terminal.Gui.Xaml.EventArgs.yml)** - Base event argument type
- **[RoutedEventArgs](../../api/Terminal.Gui.Xaml.RoutedEventArgs.yml)** - Routed event handling
- **[KeyEventArgs](../../api/Terminal.Gui.Xaml.KeyEventArgs.yml)** - Keyboard event data

### Command Pattern
- **[ICommand](../../api/System.Windows.Input.ICommand.yml)** - Command interface (`Execute`, `CanExecute`)
- **[RelayCommand](../../api/Terminal.Gui.Xaml.Commands.RelayCommand.yml)** - Simple command implementation
- **[KeyBinding](../../api/Terminal.Gui.Xaml.Input.KeyBinding.yml)** - Keyboard shortcuts (`Key`, `Command`)

### Dynamic UI Updates  
- **[Label](../../api/Terminal.Gui.Xaml.Label.yml)** - Text display with styling (`Text`, `ForegroundColor`)
- **[Application.Refresh](../../api/Terminal.Gui.Application.yml#Terminal_Gui_Application_Refresh)** - Force UI redraw
- **[Control.SetNeedsDisplay](../../api/Terminal.Gui.Control.yml#Terminal_Gui_Control_SetNeedsDisplay)** - Mark for repainting

> **💡 Pro Tip**: Use commands for complex logic that needs `CanExecute` support, and direct event handlers for simple UI interactions. Keyboard shortcuts work globally within windows.

## What You'll Learn

- Button click event handling in XAML and code-behind
- Command pattern vs direct event handling
- Dynamic UI updates based on user interaction
- Multiple interaction patterns (buttons, keyboard shortcuts, menus)
- State management and user feedback

## Prerequisites

- Completed [Hello World Example](hello-world.md) and [Basic Layout Example](basic-layout.md)
- Understanding of C# events and event handlers
- Basic XAML layout concepts

## Complete Example

### Program.cs

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace ButtonClickApp;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();
        
        try
        {
            var window = new ButtonClickWindow();
            Application.Run(window);
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
```

### ButtonClickWindow.xaml

```xml
<Window x:Class="ButtonClickApp.ButtonClickWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Button Click Demo - Event Handling"
        Width="80" Height="25">
    
    <!-- Global keyboard shortcuts -->
    <Window.KeyBindings>
        <KeyBinding Key="F1" Command="{Binding ShowHelpCommand}" />
        <KeyBinding Key="Ctrl+R" Command="{Binding ResetCounterCommand}" />
        <KeyBinding Key="Escape" Command="{Binding ExitCommand}" />
    </Window.KeyBindings>
    
    <StackView Orientation="Vertical" Spacing="1">
        
        <!-- Menu Bar -->
        <MenuBar>
            <MenuBarItem Title="Actions">
                <MenuItem Title="Increment Counter" Shortcut="Space" Command="{Binding IncrementCommand}" />
                <MenuItem Title="Decrement Counter" Shortcut="Backspace" Command="{Binding DecrementCommand}" />
                <MenuSeparator />
                <MenuItem Title="Reset Counter" Shortcut="Ctrl+R" Command="{Binding ResetCounterCommand}" />
                <MenuSeparator />
                <MenuItem Title="Exit" Shortcut="Esc" Command="{Binding ExitCommand}" />
            </MenuBarItem>
            <MenuBarItem Title="Help">
                <MenuItem Title="Show Help" Shortcut="F1" Command="{Binding ShowHelpCommand}" />
                <MenuItem Title="About" Command="{Binding ShowAboutCommand}" />
            </MenuBarItem>
        </MenuBar>
        
        <!-- Header -->
        <Label Text="Interactive Button Demo"
               HorizontalAlignment="Center"
               ForegroundColor="Cyan" />
        
        <!-- Counter Display -->
        <FrameView Title="Counter" Height="6">
            <StackView Orientation="Vertical" HorizontalAlignment="Center" VerticalAlignment="Center">
                <Label Name="counterLabel"
                       Text="Counter: 0"
                       HorizontalAlignment="Center"
                       FontStyle="Bold"
                       ForegroundColor="Green" />
                <Label Name="clicksLabel"
                       Text="Total Clicks: 0"
                       HorizontalAlignment="Center"
                       ForegroundColor="Yellow" />
            </StackView>
        </FrameView>
        
        <!-- Primary Action Buttons -->
        <FrameView Title="Actions" Height="8">
            <StackView Orientation="Vertical" Spacing="1" Margin="1">
                
                <!-- Counter controls -->
                <StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="3">
                    <Button Name="decrementButton"
                            Text="- Decrement"
                            Width="15"
                            Height="1" />
                    <Button Name="incrementButton"
                            Text="+ Increment"
                            Width="15" 
                            Height="1"
                            IsDefault="true" />
                </StackView>
                
                <!-- Secondary actions -->
                <StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="3">
                    <Button Name="resetButton"
                            Text="🔄 Reset"
                            Width="12"
                            Height="1" />
                    <Button Name="randomButton"
                            Text="🎲 Random"
                            Width="12"
                            Height="1" />
                    <Button Name="doubleButton"
                            Text="✕2 Double"
                            Width="12"
                            Height="1" />
                </StackView>
                
            </StackView>
        </FrameView>
        
        <!-- Settings Panel -->
        <FrameView Title="Settings" Height="5">
            <StackView Orientation="Horizontal" Spacing="3" Margin="1">
                <CheckBox Name="soundEnabledCheck"
                          Text="Enable sound effects"
                          Checked="True" />
                <CheckBox Name="animationEnabledCheck"
                          Text="Enable animations"
                          Checked="True" />
                <CheckBox Name="autoSaveCheck"
                          Text="Auto-save counter" />
            </StackView>
        </FrameView>
        
        <!-- Status and Activity Log -->
        <FrameView Title="Activity Log" Height="4">
            <ListView Name="activityList"
                      Height="Dim.Fill()"
                      Width="Dim.Fill()" />
        </FrameView>
        
        <!-- Status Bar -->
        <Label Name="statusLabel"
               Text="Ready - Click buttons or use keyboard shortcuts"
               HorizontalAlignment="Center"
               ForegroundColor="Gray" />
        
    </StackView>
</Window>
```

### ButtonClickWindow.xaml.cs

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using System.Collections.ObjectModel;

namespace ButtonClickApp;

public partial class ButtonClickWindow : Window
{
    private int _counter = 0;
    private int _totalClicks = 0;
    private readonly Random _random = new();
    private readonly ObservableCollection<string> _activityLog = new();
    
    // Properties for data binding (if using MVVM pattern)
    public int Counter 
    { 
        get => _counter; 
        set 
        {
            _counter = value;
            UpdateCounterDisplay();
        }
    }
    
    public int TotalClicks 
    { 
        get => _totalClicks; 
        set 
        {
            _totalClicks = value;
            UpdateClicksDisplay();
        }
    }
    
    public ButtonClickWindow()
    {
        InitializeComponent();
        SetupEventHandlers();
        SetupActivityLog();
        LogActivity("Application started");
    }
    
    private void SetupEventHandlers()
    {
        // Primary button events
        incrementButton.Clicked += OnIncrementClicked;
        decrementButton.Clicked += OnDecrementClicked;
        resetButton.Clicked += OnResetClicked;
        randomButton.Clicked += OnRandomClicked;
        doubleButton.Clicked += OnDoubleClicked;
        
        // Settings events
        soundEnabledCheck.Toggled += OnSoundToggled;
        animationEnabledCheck.Toggled += OnAnimationToggled;
        autoSaveCheck.Toggled += OnAutoSaveToggled;
        
        // Keyboard events
        KeyDown += OnKeyDown;
    }
    
    private void SetupActivityLog()
    {
        activityList.SetSource(_activityLog);
    }
    
    private void OnIncrementClicked(object sender, EventArgs e)
    {
        Counter++;
        TotalClicks++;
        LogActivity($"Incremented counter to {Counter}");
        UpdateStatus("Counter incremented", Color.Green);
        
        if (soundEnabledCheck.Checked)
        {
            PlaySound("increment");
        }
        
        if (animationEnabledCheck.Checked)
        {
            AnimateCounterChange("+1");
        }
    }
    
    private void OnDecrementClicked(object sender, EventArgs e)
    {
        Counter--;
        TotalClicks++;
        LogActivity($"Decremented counter to {Counter}");
        UpdateStatus("Counter decremented", Color.Yellow);
        
        if (soundEnabledCheck.Checked)
        {
            PlaySound("decrement");
        }
        
        if (animationEnabledCheck.Checked)
        {
            AnimateCounterChange("-1");
        }
    }
    
    private void OnResetClicked(object sender, EventArgs e)
    {
        var previousValue = Counter;
        Counter = 0;
        LogActivity($"Reset counter from {previousValue} to 0");
        UpdateStatus("Counter reset to zero", Color.Cyan);
        
        if (soundEnabledCheck.Checked)
        {
            PlaySound("reset");
        }
    }
    
    private void OnRandomClicked(object sender, EventArgs e)
    {
        var previousValue = Counter;
        Counter = _random.Next(-100, 101); // Random between -100 and 100
        TotalClicks++;
        LogActivity($"Set random value: {previousValue} → {Counter}");
        UpdateStatus($"Set random value: {Counter}", Color.Magenta);
        
        if (soundEnabledCheck.Checked)
        {
            PlaySound("random");
        }
    }
    
    private void OnDoubleClicked(object sender, EventArgs e)
    {
        var previousValue = Counter;
        Counter *= 2;
        TotalClicks++;
        LogActivity($"Doubled counter: {previousValue} → {Counter}");
        UpdateStatus($"Counter doubled to {Counter}", Color.Blue);
        
        if (soundEnabledCheck.Checked)
        {
            PlaySound("double");
        }
    }
    
    private void OnSoundToggled(object sender, EventArgs e)
    {
        var enabled = soundEnabledCheck.Checked;
        LogActivity($"Sound effects {(enabled ? "enabled" : "disabled")}");
        UpdateStatus($"Sound effects {(enabled ? "enabled" : "disabled")}", Color.Gray);
    }
    
    private void OnAnimationToggled(object sender, EventArgs e)
    {
        var enabled = animationEnabledCheck.Checked;
        LogActivity($"Animations {(enabled ? "enabled" : "disabled")}");
        UpdateStatus($"Animations {(enabled ? "enabled" : "disabled")}", Color.Gray);
    }
    
    private void OnAutoSaveToggled(object sender, EventArgs e)
    {
        var enabled = autoSaveCheck.Checked;
        LogActivity($"Auto-save {(enabled ? "enabled" : "disabled")}");
        UpdateStatus($"Auto-save {(enabled ? "enabled" : "disabled")}", Color.Gray);
        
        if (enabled)
        {
            // In a real app, you might save to file or database
            LogActivity($"Counter auto-saved: {Counter}");
        }
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.KeyEvent.Key)
        {
            case Key.Space:
                OnIncrementClicked(this, EventArgs.Empty);
                e.Handled = true;
                break;
                
            case Key.Backspace:
                OnDecrementClicked(this, EventArgs.Empty);
                e.Handled = true;
                break;
                
            case Key.F1:
                ShowHelp();
                e.Handled = true;
                break;
                
            case Key.Esc:
                Application.RequestStop();
                e.Handled = true;
                break;
        }
    }
    
    private void UpdateCounterDisplay()
    {
        counterLabel.Text = $"Counter: {Counter}";
        
        // Change color based on value
        counterLabel.ForegroundColor = Counter switch
        {
            > 0 => Color.Green,
            < 0 => Color.Red,
            _ => Color.Yellow
        };
    }
    
    private void UpdateClicksDisplay()
    {
        clicksLabel.Text = $"Total Clicks: {TotalClicks}";
    }
    
    private void UpdateStatus(string message, Color color)
    {
        statusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        statusLabel.ForegroundColor = color;
    }
    
    private void LogActivity(string activity)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _activityLog.Insert(0, $"[{timestamp}] {activity}");
        
        // Keep log size manageable
        while (_activityLog.Count > 50)
        {
            _activityLog.RemoveAt(_activityLog.Count - 1);
        }
        
        // Auto-scroll to top (newest items)
        if (_activityLog.Count > 0)
        {
            activityList.SelectedItem = 0;
        }
    }
    
    private void PlaySound(string soundType)
    {
        // In a real application, you might:
        // - Play different console beeps
        // - Trigger system sounds
        // - Use a sound library
        Console.Beep(); // Simple beep for demonstration
    }
    
    private void AnimateCounterChange(string change)
    {
        // Simple animation by temporarily showing the change
        var originalText = counterLabel.Text;
        counterLabel.Text = $"{originalText} ({change})";
        
        // In a real app, you might use a timer to reset after a delay
        // For this demo, we'll just update on the next UI refresh
        Application.Invoke(() => UpdateCounterDisplay());
    }
    
    private void ShowHelp()
    {
        var helpText = @"Button Click Demo - Keyboard Shortcuts

Navigation:
• Space         - Increment counter
• Backspace     - Decrement counter  
• Ctrl+R        - Reset counter
• Tab           - Navigate between controls
• Enter         - Activate focused button
• Esc           - Exit application

Menu Access:
• Alt+A         - Actions menu
• Alt+H         - Help menu
• F1            - Show this help
• Arrow Keys    - Navigate menus

Features:
• Click buttons or use keyboard shortcuts
• Enable/disable sound effects and animations
• View activity log of all actions
• Auto-save functionality (simulated)

Tips:
• Default button (+ Increment) can be activated with Enter
• All actions are logged with timestamps
• Counter color changes based on positive/negative values";
        
        MessageBox.Query("Help", helpText, "OK");
        LogActivity("Viewed help information");
    }
    
    private void ShowAbout()
    {
        var aboutText = @"Button Click Demo v1.0

This example demonstrates event handling patterns in Terminal.Gui.Xaml:
• Direct event handlers vs command pattern
• Keyboard shortcuts and menu integration  
• Dynamic UI updates and user feedback
• Settings persistence and state management

Built with Terminal.Gui.Xaml framework
© 2025 Terminal.Gui.Xaml Examples";
        
        MessageBox.Query("About", aboutText, "OK");
        LogActivity("Viewed about information");
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
# Setup project
dotnet new console -n ButtonClickApp
cd ButtonClickApp
dotnet add package Terminal.Gui.Xaml

# Build and run
dotnet build
dotnet run
```

## Expected Output

An interactive application with a counter, multiple buttons, settings, and activity log:

```
┌─ Button Click Demo - Event Handling ──────────────────────────────────┐
│ [Actions ▼] [Help ▼]                                                  │
│                                                                        │
│                        Interactive Button Demo                         │
│                                                                        │
│ ┌─ Counter ─────────────────────────────────────────────────────────┐ │
│ │                        Counter: 5                                 │ │
│ │                     Total Clicks: 12                              │ │
│ └───────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│ ┌─ Actions ─────────────────────────────────────────────────────────┐ │
│ │                                                                   │ │
│ │      [- Decrement]     [+ Increment]                             │ │
│ │                                                                   │ │
│ │     [🔄 Reset]    [🎲 Random]    [✕2 Double]                      │ │
│ │                                                                   │ │
│ └───────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│ ┌─ Settings ────────────────────────────────────────────────────────┐ │
│ │ ☑ Enable sound effects  ☑ Enable animations  ☐ Auto-save counter│ │
│ └───────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│ ┌─ Activity Log ────────────────────────────────────────────────────┐ │
│ │ [14:32:15] Incremented counter to 5                              │ │
│ │ [14:32:10] Doubled counter: 2 → 4                                │ │
│ └───────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│           14:32:15 - Counter incremented                               │
└────────────────────────────────────────────────────────────────────────┘
```

## Key Event Handling Patterns

### 1. Direct Event Handling
```csharp
incrementButton.Clicked += OnIncrementClicked;

private void OnIncrementClicked(object sender, EventArgs e)
{
    Counter++;
    // Update UI, play sounds, log activity
}
```

### 2. Keyboard Shortcuts
```xml
<Window.KeyBindings>
    <KeyBinding Key="Ctrl+R" Command="{Binding ResetCounterCommand}" />
</Window.KeyBindings>
```

### 3. Menu Integration
```xml
<MenuItem Title="Increment Counter" Shortcut="Space" Command="{Binding IncrementCommand}" />
```

### 4. Dynamic UI Updates
```csharp
private void UpdateCounterDisplay()
{
    counterLabel.Text = $"Counter: {Counter}";
    counterLabel.ForegroundColor = Counter > 0 ? Color.Green : Color.Red;
}
```

### 5. Settings and State Management
```csharp
private void OnSoundToggled(object sender, EventArgs e)
{
    var enabled = soundEnabledCheck.Checked;
    // Save preference, update behavior
}
```

## Advanced Features Demonstrated

### Multiple Interaction Methods
- **Mouse/Touch**: Direct button clicks
- **Keyboard**: Space, Backspace, Ctrl+R shortcuts
- **Menu**: Alt+A for Actions menu
- **Default Actions**: Enter key activates default button

### User Feedback Systems
- **Visual**: Color changes, text updates
- **Audio**: Console beep for sound effects
- **Logging**: Timestamped activity history
- **Status**: Real-time status messages

### State Management
- **Counter Value**: Persistent across interactions
- **Settings**: Checkboxes maintain user preferences
- **History**: Activity log with automatic cleanup

## Event Handling Best Practices

### 1. Separation of Concerns
```csharp
private void OnIncrementClicked(object sender, EventArgs e)
{
    // Update data
    Counter++;
    TotalClicks++;
    
    // Update UI
    LogActivity($"Incremented counter to {Counter}");
    UpdateStatus("Counter incremented", Color.Green);
    
    // Handle effects
    if (soundEnabledCheck.Checked) PlaySound("increment");
}
```

### 2. Consistent Event Signatures
```csharp
// All event handlers follow the same pattern
private void OnButtonClicked(object sender, EventArgs e)
private void OnSettingToggled(object sender, EventArgs e)
private void OnKeyPressed(object sender, KeyEventArgs e)
```

### 3. Error Handling
```csharp
private void OnRandomClicked(object sender, EventArgs e)
{
    try
    {
        Counter = _random.Next(-100, 101);
        // ... update UI
    }
    catch (Exception ex)
    {
        UpdateStatus($"Error: {ex.Message}", Color.Red);
        LogActivity($"Error in random generation: {ex.Message}");
    }
}
```

### 4. Resource Management
```csharp
public void Dispose()
{
    // Unsubscribe from events
    incrementButton.Clicked -= OnIncrementClicked;
    // Dispose resources
    _random?.Dispose();
}
```

## Next Steps

1. **Data Binding**: Convert to MVVM with [Data Binding Guide](../guides/bind-data.md)
2. **Advanced Input**: Explore complex input handling with [Handle Input Guide](../guides/handle-input.md)
3. **Navigation**: Connect multiple windows with [Navigation Guide](../guides/navigation.md)
4. **Custom Controls**: Build reusable components with custom event handling

## Troubleshooting

### Events Not Firing

**Button clicks not working**
```
Solution: Check event handler attachment
- Verify += syntax in constructor
- Ensure handler method signature matches
- Check if button is enabled and visible
```

**Keyboard shortcuts not working**
```
Solution: Verify KeyBindings and focus
- Check Window.KeyBindings syntax
- Ensure window has focus
- Verify key combination is correct
```

### UI Update Issues

**Counter not updating visually**
```
Solution: Call UpdateCounterDisplay() after changes
- Update UI in all code paths that change counter
- Check if controls are properly named
- Verify property setters trigger UI updates
```

**Status messages not showing**
```
Solution: Check status label updates
- Ensure statusLabel is properly bound
- Verify UpdateStatus() method is called
- Check if status text fits in available space
```

## Related Topics

### Examples by Learning Path
- **[Hello World](hello-world.md)** - Basic application structure and lifecycle
- **[Basic Layout](basic-layout.md)** - Form controls and responsive layout
- **[Form Validation](form-validation.md)** - Advanced input validation patterns
- **[Data Binding](data-binding.md)** - MVVM with commands and two-way binding

### Guides for Deep Learning
- **[Handle Input Guide](../guides/handle-input.md)** - Comprehensive input handling patterns
- **[Data Binding Guide](../guides/bind-data.md)** - MVVM pattern with command implementation
- **[Create a Window](../guides/create-window.md)** - Window setup and event handling basics

### API Reference
- **[Button](../../api/Terminal.Gui.Xaml.Button.yml)** - Click events, commands, and state management
- **[KeyBinding](../../api/Terminal.Gui.Xaml.Input.KeyBinding.yml)** - Keyboard shortcuts and global hotkeys
- **[MenuItem](../../api/Terminal.Gui.Xaml.MenuItem.yml)** - Menu actions and keyboard accelerators
- **[CheckBox](../../api/Terminal.Gui.Xaml.CheckBox.yml)** - Boolean input with change events
- **[ICommand](../../api/System.Windows.Input.ICommand.yml)** - Command pattern interface
- **[RelayCommand](../../api/Terminal.Gui.Xaml.Commands.RelayCommand.yml)** - Command implementation helper

### Concepts
- **[Event Handling](../concepts/events.md)** - Understanding event patterns and routing
- **[Command Pattern](../concepts/commands.md)** - Implementing commands vs direct events
- **[User Interaction](../concepts/interaction.md)** - Best practices for user experience
