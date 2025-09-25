# Guide: Handle Input

# Handle User Input

This guide covers input handling, validation, keyboard shortcuts, and user interaction patterns in Terminal.Gui.Xaml.

## Related APIs

This guide demonstrates these key input handling APIs and patterns:

### Input Events and Handling
- `KeyEventArgs` - Keyboard input event data
- `MouseEventArgs` - Mouse input event data
- `TextChangedEventArgs` - Text input change notifications
- `FocusEventArgs` - Focus change event data

### Keyboard and Shortcuts
- `KeyBinding` - Keyboard shortcut definitions
- `Key` - Key enumeration and modifiers
- `KeyGesture` - Complex key combinations
- `InputGesture` - Base input gesture handling

### Commands and Actions
- `ICommand` - Command pattern interface
- `RelayCommand` - Simple command implementation
- `CommandBinding` - Connect commands to handlers
- `RoutedCommand` - Commands with routing support

### Validation
- `IDataErrorInfo` - Object-level validation interface
- `ValidationRule` - Custom validation logic
- `Validation` - Validation helper methods
- `BindingValidationError` - Validation error details

> **💡 Pro Tip**: Use command binding for complex logic and direct event handlers for simple UI interactions. Keyboard shortcuts work globally within a window.

## Prerequisites

- Completed [Create a Window Guide](create-window.md)
- Understanding of [Events Concepts](../concepts/events.md)
- Familiarity with command patterns

## Step 1: Basic Input Handling

Create `InputDemoWindow.xaml`:

```xml
<Window x:Class="MyTerminalApp.InputDemoWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Input Handling Demo"
        Width="80" Height="25">
    
    <!-- Global key bindings -->
    <Window.KeyBindings>
        <KeyBinding Key="Ctrl+N" Command="{Binding NewCommand}" />
        <KeyBinding Key="Ctrl+S" Command="{Binding SaveCommand}" />
        <KeyBinding Key="F1" Command="{Binding ShowHelpCommand}" />
        <KeyBinding Key="Escape" Command="{Binding CancelCommand}" />
    </Window.KeyBindings>
    
    <StackView Orientation="Vertical" Spacing="1">
        <!-- Menu bar for quick access -->
        <MenuBar>
            <MenuBarItem Title="File">
                <MenuItem Title="New" Shortcut="Ctrl+N" Command="{Binding NewCommand}" />
                <MenuItem Title="Save" Shortcut="Ctrl+S" Command="{Binding SaveCommand}" />
                <MenuSeparator />
                <MenuItem Title="Exit" Shortcut="Alt+F4" Command="{Binding ExitCommand}" />
            </MenuBarItem>
            <MenuBarItem Title="Help">
                <MenuItem Title="About" Shortcut="F1" Command="{Binding ShowHelpCommand}" />
            </MenuBarItem>
        </MenuBar>
        
        <!-- Text input with validation -->
        <FrameView Title="Text Input" Height="8">
            <StackView Orientation="Vertical" Spacing="1" Margin="1">
                <StackView Orientation="Horizontal">
                    <Label Text="Name:" Width="15" />
                    <TextField Name="nameField"
                               Text="{Binding Name, Mode=TwoWay}"
                               PlaceholderText="Enter your name..."
                               Width="Dim.Fill()" />
                </StackView>
                
                <StackView Orientation="Horizontal">
                    <Label Text="Age (18-120):" Width="15" />
                    <TextField Name="ageField"
                               Text="{Binding AgeText, Mode=TwoWay}"
                               Width="10" />
                    <Label Text="{Binding AgeValidationMessage}" 
                           ForegroundColor="Red" 
                           Visible="{Binding HasAgeError}" />
                </StackView>
                
                <StackView Orientation="Horizontal">
                    <Label Text="Password:" Width="15" />
                    <TextField Name="passwordField"
                               Text="{Binding Password, Mode=TwoWay}"
                               IsPassword="true"
                               Width="20" />
                    <Label Text="Strength:" Width="10" />
                    <Label Text="{Binding PasswordStrength}" 
                           ForegroundColor="{Binding PasswordStrengthColor}" />
                </StackView>
            </StackView>
        </FrameView>
        
        <!-- Choice controls -->
        <FrameView Title="Choices" Height="6">
            <StackView Orientation="Vertical" Margin="1">
                <RadioGroup Name="genderGroup" 
                            SelectedIndex="{Binding GenderIndex, Mode=TwoWay}">
                    <RadioButton Text="Male" />
                    <RadioButton Text="Female" />
                    <RadioButton Text="Other" />
                </RadioGroup>
                
                <CheckBox Text="I agree to the terms and conditions"
                          Checked="{Binding AcceptTerms, Mode=TwoWay}" />
            </StackView>
        </FrameView>
        
        <!-- Action buttons -->
        <StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="2">
            <Button Text="Submit" 
                    Command="{Binding SubmitCommand}"
                    IsDefault="true" 
                    Width="12" />
            <Button Text="Reset" 
                    Command="{Binding ResetCommand}" 
                    Width="12" />
            <Button Text="Cancel" 
                    Command="{Binding CancelCommand}"
                    IsCancel="true" 
                    Width="12" />
        </StackView>
        
        <!-- Status and help -->
        <Label Text="{Binding StatusMessage}" 
               HorizontalAlignment="Center"
               ForegroundColor="{Binding StatusColor}" />
        
        <Label Text="Shortcuts: Ctrl+N=New, Ctrl+S=Save, F1=Help, Esc=Cancel" 
               HorizontalAlignment="Center" 
               ForegroundColor="DarkGray" />
    </StackView>
</Window>
```

## Step 2: Create the Input View Model

Create `InputDemoViewModel.cs`:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Terminal.Gui;

namespace MyTerminalApp.ViewModels;

public class InputDemoViewModel : INotifyPropertyChanged
{
    private string _name = "";
    private string _ageText = "";
    private string _password = "";
    private int _genderIndex = 0;
    private bool _acceptTerms = false;
    private string _statusMessage = "Ready";
    private Color _statusColor = Color.Green;
    
    // Basic properties
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public string AgeText
    {
        get => _ageText;
        set 
        { 
            SetProperty(ref _ageText, value);
            OnPropertyChanged(nameof(Age));
            OnPropertyChanged(nameof(IsAgeValid));
            OnPropertyChanged(nameof(AgeValidationMessage));
            OnPropertyChanged(nameof(HasAgeError));
        }
    }
    
    public string Password
    {
        get => _password;
        set 
        { 
            SetProperty(ref _password, value);
            OnPropertyChanged(nameof(PasswordStrength));
            OnPropertyChanged(nameof(PasswordStrengthColor));
        }
    }
    
    public int GenderIndex
    {
        get => _genderIndex;
        set => SetProperty(ref _genderIndex, value);
    }
    
    public bool AcceptTerms
    {
        get => _acceptTerms;
        set => SetProperty(ref _acceptTerms, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    public Color StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }
    
    // Computed validation properties
    public int? Age => int.TryParse(AgeText, out var age) ? age : null;
    
    public bool IsAgeValid => Age is >= 18 and <= 120;
    
    public bool HasAgeError => !string.IsNullOrEmpty(AgeText) && !IsAgeValid;
    
    public string AgeValidationMessage => HasAgeError 
        ? "Age must be between 18 and 120" 
        : "";
    
    public string PasswordStrength
    {
        get
        {
            if (string.IsNullOrEmpty(Password)) return "";
            
            var score = 0;
            if (Password.Length >= 8) score++;
            if (Regex.IsMatch(Password, @"[A-Z]")) score++;
            if (Regex.IsMatch(Password, @"[a-z]")) score++;
            if (Regex.IsMatch(Password, @"\d")) score++;
            if (Regex.IsMatch(Password, @"[^a-zA-Z0-9]")) score++;
            
            return score switch
            {
                0 or 1 => "Weak",
                2 or 3 => "Medium",
                4 => "Strong",
                5 => "Very Strong",
                _ => "Unknown"
            };
        }
    }
    
    public Color PasswordStrengthColor
    {
        get
        {
            return PasswordStrength switch
            {
                "Weak" => Color.Red,
                "Medium" => Color.Yellow,
                "Strong" => Color.Green,
                "Very Strong" => Color.Cyan,
                _ => Color.Gray
            };
        }
    }
    
    public bool CanSubmit => 
        !string.IsNullOrWhiteSpace(Name) &&
        IsAgeValid &&
        !string.IsNullOrEmpty(Password) &&
        AcceptTerms;
    
    // Commands
    public ICommand SubmitCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ShowHelpCommand { get; }
    public ICommand ExitCommand { get; }
    
    public InputDemoViewModel()
    {
        SubmitCommand = new RelayCommand(Submit, () => CanSubmit);
        ResetCommand = new RelayCommand(Reset);
        CancelCommand = new RelayCommand(Cancel);
        NewCommand = new RelayCommand(NewForm);
        SaveCommand = new RelayCommand(Save, () => CanSubmit);
        ShowHelpCommand = new RelayCommand(ShowHelp);
        ExitCommand = new RelayCommand(Exit);
        
        // Update command states when validation changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(Name) or nameof(IsAgeValid) or 
                nameof(Password) or nameof(AcceptTerms))
            {
                ((RelayCommand)SubmitCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        };
    }
    
    private void Submit()
    {
        if (!CanSubmit) return;
        
        var gender = GenderIndex switch
        {
            0 => "Male",
            1 => "Female", 
            2 => "Other",
            _ => "Unknown"
        };
        
        StatusMessage = $"Submitted: {Name}, Age {Age}, Gender: {gender}";
        StatusColor = Color.Green;
    }
    
    private void Reset()
    {
        Name = "";
        AgeText = "";
        Password = "";
        GenderIndex = 0;
        AcceptTerms = false;
        StatusMessage = "Form reset";
        StatusColor = Color.Yellow;
    }
    
    private void Cancel()
    {
        // In a real app, this might close the window or navigate back
        StatusMessage = "Operation cancelled";
        StatusColor = Color.Red;
    }
    
    private void NewForm()
    {
        Reset();
        StatusMessage = "New form created (Ctrl+N)";
        StatusColor = Color.Cyan;
    }
    
    private void Save()
    {
        if (!CanSubmit) return;
        
        StatusMessage = "Form saved (Ctrl+S)";
        StatusColor = Color.Blue;
    }
    
    private void ShowHelp()
    {
        var message = @"Input Demo Help (F1)

Keyboard Shortcuts:
• Ctrl+N - New form
• Ctrl+S - Save form  
• F1 - Show this help
• Esc - Cancel operation
• Enter - Submit (when valid)
• Tab - Navigate between fields

Validation Rules:
• Name: Required
• Age: Must be 18-120
• Password: Required for submission
• Terms: Must be accepted";

        MessageBox.Query("Help", message, "OK");
    }
    
    private void Exit()
    {
        Application.RequestStop();
    }
    
    // Property change notification
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
}
```

## Step 3: Create the Code-Behind with Advanced Input Handling

Create `InputDemoWindow.xaml.cs`:

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using MyTerminalApp.ViewModels;

namespace MyTerminalApp;

public partial class InputDemoWindow : Window
{
    public InputDemoViewModel ViewModel { get; }
    
    public InputDemoWindow()
    {
        ViewModel = new InputDemoViewModel();
        DataContext = ViewModel;
        
        InitializeComponent();
        SetupAdvancedInputHandling();
    }
    
    private void SetupAdvancedInputHandling()
    {
        // Custom validation on field focus changes
        nameField.Leave += ValidateNameField;
        ageField.Leave += ValidateAgeField;
        passwordField.Leave += ValidatePasswordField;
        
        // Input formatting and constraints
        ageField.TextChanging += FilterNumericInput;
        
        // Context menus
        SetupContextMenus();
        
        // Focus management
        SetupFocusManagement();
    }
    
    private void ValidateNameField(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameField.Text?.ToString()))
        {
            ViewModel.StatusMessage = "Name is required";
            ViewModel.StatusColor = Color.Red;
        }
    }
    
    private void ValidateAgeField(object sender, EventArgs e)
    {
        if (ViewModel.HasAgeError)
        {
            ViewModel.StatusMessage = ViewModel.AgeValidationMessage;
            ViewModel.StatusColor = Color.Red;
        }
    }
    
    private void ValidatePasswordField(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(passwordField.Text?.ToString()))
        {
            ViewModel.StatusMessage = "Password is required for submission";
            ViewModel.StatusColor = Color.Yellow;
        }
    }
    
    private void FilterNumericInput(object sender, TextChangingEventArgs e)
    {
        // Only allow numeric input for age field
        if (sender == ageField)
        {
            var newText = e.NewText?.ToString() ?? "";
            if (!string.IsNullOrEmpty(newText) && !newText.All(char.IsDigit))
            {
                e.Cancel = true;
                ViewModel.StatusMessage = "Age must be numeric";
                ViewModel.StatusColor = Color.Red;
            }
        }
    }
    
    private void SetupContextMenus()
    {
        // Right-click context menu for text fields
        var contextMenu = new ContextMenu();
        contextMenu.MenuItems.Add(new MenuItem("Cut", "", CutText, null, null, Key.CtrlMask | Key.X));
        contextMenu.MenuItems.Add(new MenuItem("Copy", "", CopyText, null, null, Key.CtrlMask | Key.C));
        contextMenu.MenuItems.Add(new MenuItem("Paste", "", PasteText, null, null, Key.CtrlMask | Key.V));
        contextMenu.MenuItems.Add(new MenuSeparator());
        contextMenu.MenuItems.Add(new MenuItem("Select All", "", SelectAllText, null, null, Key.CtrlMask | Key.A));
        
        nameField.ContextMenu = contextMenu;
        ageField.ContextMenu = contextMenu;
        passwordField.ContextMenu = contextMenu;
    }
    
    private void CutText()
    {
        if (Focused is TextField field)
        {
            Clipboard.Contents = field.SelectedText;
            field.DeleteSelectedText();
        }
    }
    
    private void CopyText()
    {
        if (Focused is TextField field)
        {
            Clipboard.Contents = field.SelectedText;
        }
    }
    
    private void PasteText()
    {
        if (Focused is TextField field)
        {
            field.InsertText(Clipboard.Contents);
        }
    }
    
    private void SelectAllText()
    {
        if (Focused is TextField field)
        {
            field.SelectAll();
        }
    }
    
    private void SetupFocusManagement()
    {
        // Auto-advance to next field on Enter for single-line inputs
        nameField.KeyDown += (s, e) =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                ageField.SetFocus();
                e.Handled = true;
            }
        };
        
        ageField.KeyDown += (s, e) =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                passwordField.SetFocus();
                e.Handled = true;
            }
        };
        
        // Auto-submit on Enter if form is valid
        passwordField.KeyDown += (s, e) =>
        {
            if (e.KeyEvent.Key == Key.Enter && ViewModel.CanSubmit)
            {
                ViewModel.SubmitCommand.Execute(null);
                e.Handled = true;
            }
        };
    }
}
```

## Step 4: Test the Input Handling

Run the application:

```powershell
dotnet build
dotnet run
```

Test these features:
- **Real-time validation**: Age field only accepts numbers, shows validation messages
- **Password strength**: Updates as you type
- **Keyboard shortcuts**: Ctrl+N, Ctrl+S, F1, Esc
- **Auto-focus**: Enter key moves to next field
- **Context menus**: Right-click on text fields
- **Command states**: Submit/Save buttons enable/disable based on form validity

## Understanding Input Handling Patterns

### Input Validation
```csharp
public bool IsAgeValid => Age is >= 18 and <= 120;
public bool HasAgeError => !string.IsNullOrEmpty(AgeText) && !IsAgeValid;
```

### Real-time Feedback
```csharp
public string PasswordStrength { get; }
public Color PasswordStrengthColor { get; }
```

### Input Filtering
```csharp
private void FilterNumericInput(object sender, TextChangingEventArgs e)
{
    if (!newText.All(char.IsDigit))
        e.Cancel = true; // Prevent non-numeric input
}
```

### Keyboard Shortcuts
```xml
<Window.KeyBindings>
    <KeyBinding Key="Ctrl+S" Command="{Binding SaveCommand}" />
</Window.KeyBindings>
```

### Context Menus
```csharp
nameField.ContextMenu = contextMenu;
```

## Advanced Input Scenarios

### Custom Input Controls
```csharp
public class DatePickerField : TextField
{
    protected override bool ProcessKey(KeyEvent keyEvent)
    {
        // Custom date input handling
        if (keyEvent.Key == Key.F4)
        {
            ShowDatePicker();
            return true;
        }
        return base.ProcessKey(keyEvent);
    }
}
```

### Input Masking
```csharp
private void ApplyPhoneMask(object sender, TextChangingEventArgs e)
{
    var text = e.NewText?.ToString() ?? "";
    var digits = new string(text.Where(char.IsDigit).ToArray());
    
    if (digits.Length <= 10)
    {
        e.NewText = digits.Length switch
        {
            >= 7 => $"({digits[..3]}) {digits[3..6]}-{digits[6..]}",
            >= 4 => $"({digits[..3]}) {digits[3..]}",
            >= 1 => $"({digits}",
            _ => ""
        };
    }
}
```

### Async Validation
```csharp
public async Task ValidateEmailAsync(string email)
{
    IsValidating = true;
    try
    {
        var isValid = await emailValidationService.ValidateAsync(email);
        EmailValidationResult = isValid ? "Valid" : "Invalid email address";
    }
    finally
    {
        IsValidating = false;
    }
}
```

## Next Steps

1. **Add Navigation**: Create multi-step forms or wizards
   - See [Navigation Guide](navigation.md)

2. **Improve Accessibility**: Screen reader support, high contrast
   - See [Accessibility Guide](../accessibility.md)

3. **Data Persistence**: Save and restore form state
   - See [Bind Data Guide](bind-data.md)

## Troubleshooting

### Input Not Working

**Keys not registering**
- Check `KeyBindings` are at the right level (Window, Control)
- Ensure event handlers return `true` for `Handled` property
- Verify focus is on the correct control

**Validation not updating**
- Check `PropertyChanged` notifications are fired
- Ensure computed properties update dependent properties
- Verify `CanExecute` logic includes all validation rules

### Performance Issues

**Slow validation**
- Avoid expensive operations in property getters
- Use async validation for network calls
- Debounce rapid input changes

**Memory leaks**
- Unsubscribe from events in `Dispose()`
- Use weak event patterns for long-lived objects
- Clear event handlers when removing controls

## Related Topics

### API Reference
- `KeyEventArgs` - Keyboard event handling
- `MouseEventArgs` - Mouse interaction events
- `KeyBinding` - Keyboard shortcuts
- `ICommand` - Command pattern interface
- `ValidationRule` - Custom validation
- `TextChangedEventArgs` - Text change notifications

### Concepts
- **[Event Handling](../concepts/events.md)** - Understanding event patterns
- Command pattern - see `ICommand` usage in guides
- Input system - keyboard and mouse handling is covered throughout this guide
- Validation concepts - see Bind Data guide for patterns

### Examples
- **[Button Click](../examples/button-click.md)** - Basic event handling
- Form validation example (coming soon)
- Keyboard shortcuts example (coming soon)

### Related Guides
- **[Create a Window](create-window.md)** - Basic UI setup and event handling
- **[Bind Data](bind-data.md)** - Connecting data to UI with validation
- **[Navigation](navigation.md)** - Managing focus and view transitions

### External Resources
- **[ICommand Interface](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand)** - .NET command pattern
- **[IDataErrorInfo Interface](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.idataerrorinfo)** - Validation interface
- **[Event Patterns](https://docs.microsoft.com/en-us/dotnet/csharp/events-overview)** - C# event handling
