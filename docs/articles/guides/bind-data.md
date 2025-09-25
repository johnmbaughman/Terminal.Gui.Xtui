# Guide: Bind Data

# Bind Data to Controls

This guide shows how to connect your UI controls to data using Terminal.Gui.Xaml's data binding features.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable

## Related APIs

This guide demonstrates these key data binding APIs and concepts:

### Core Binding System
### Core Binding System
- `Binding` - Primary data binding mechanism
- `BindingExpression` - Runtime binding evaluation and management
- `BindingMode` - One-way, two-way, and one-time binding modes
- `IValueConverter` - Custom data transformation interface

### MVVM Support
### MVVM Support
- `INotifyPropertyChanged` - Property change notification
- `ICommand` - Command pattern for user actions
- `RelayCommand` - Simple command implementation
- `ObservableCollection<T>` - Collection change notifications

### Control Data Properties
### Control Data Properties
- `TextField.Text` - Bindable text content
- `Label.Text` - Display bound data
- `CheckBox.Checked` - Boolean binding
- `Button.Command` - Command binding

> **💡 Pro Tip**: Most Terminal.Gui.Xaml controls support two-way data binding. Always implement `INotifyPropertyChanged` in your view models for automatic UI updates.

## Prerequisites

## Related Resources
- Data binding implementation and concepts
- Value converters for transforming data
- Observable collections for list UIs
## Step 1: Create a View Model

Create `PersonViewModel.cs`:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyTerminalApp.ViewModels;

public class PersonViewModel : INotifyPropertyChanged
{
    private string _firstName = "";
    private string _lastName = "";
    private string _email = "";
    private bool _isSubscribed = false;
    private string _statusMessage = "";
    
    // Properties with change notification
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }
    
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
    
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
    
    public bool IsSubscribed
    {
        get => _isSubscribed;
        set => SetProperty(ref _isSubscribed, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }
    
    // Computed property
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Commands
    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }
    
    public PersonViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        ClearCommand = new RelayCommand(Clear);
        
        // Update FullName when names change
        PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(FirstName) || e.PropertyName == nameof(LastName))
                OnPropertyChanged(nameof(FullName));
                
            // Refresh command state when validation changes
            if (e.PropertyName is nameof(FirstName) or nameof(LastName) or nameof(Email))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        };
    }
    
    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               !string.IsNullOrWhiteSpace(Email) &&
               Email.Contains('@');
    }
    
    private void Save()
    {
        StatusMessage = $"Saved: {FullName} ({Email}) - Newsletter: {(IsSubscribed ? "Yes" : "No")}";
    }
    
    private void Clear()
    {
        FirstName = "";
        LastName = "";
        Email = "";
        IsSubscribed = false;
        StatusMessage = "Form cleared";
    }
    
    // Helper method for property change notification
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

## Step 2: Create the RelayCommand Helper

Create `RelayCommand.cs`:

```csharp
using System.Windows.Input;

namespace MyTerminalApp.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
    
    public void Execute(object parameter) => _execute();
    
    public event EventHandler CanExecuteChanged;
    
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
```

## Step 3: Create Data-Bound XAML Window

Create `PersonWindow.xaml`:

```xml
<Window x:Class="MyTerminalApp.PersonWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Person Information - Data Binding Demo"
        Width="70" Height="20">
    
    <StackView Orientation="Vertical" Spacing="1">
        <!-- Header with computed property -->
        <Label Text="{Binding FullName, StringFormat='Editing: {0}'}" 
               HorizontalAlignment="Center" 
               ForegroundColor="Cyan" 
               Visible="{Binding FullName, Converter={StaticResource StringNotEmptyConverter}}" />
        
        <!-- Input form with two-way binding -->
        <FrameView Title="Personal Information" Height="10">
            <StackView Orientation="Vertical" Spacing="1" Margin="1">
                <StackView Orientation="Horizontal">
                    <Label Text="First Name:" Width="12" />
                    <TextField Text="{Binding FirstName, Mode=TwoWay}" 
                               Width="Dim.Fill()" />
                </StackView>
                
                <StackView Orientation="Horizontal">
                    <Label Text="Last Name:" Width="12" />
                    <TextField Text="{Binding LastName, Mode=TwoWay}" 
                               Width="Dim.Fill()" />
                </StackView>
                
                <StackView Orientation="Horizontal">
                    <Label Text="Email:" Width="12" />
                    <TextField Text="{Binding Email, Mode=TwoWay}" 
                               Width="Dim.Fill()" />
                </StackView>
                
                <CheckBox Text="Subscribe to newsletter" 
                          Checked="{Binding IsSubscribed, Mode=TwoWay}" />
            </StackView>
        </FrameView>
        
        <!-- Command buttons -->
        <StackView Orientation="Horizontal" HorizontalAlignment="Center" Spacing="2">
            <Button Text="Save" 
                    Command="{Binding SaveCommand}" 
                    Width="10" />
            <Button Text="Clear" 
                    Command="{Binding ClearCommand}" 
                    Width="10" />
        </StackView>
        
        <!-- Status display -->
        <Label Text="{Binding StatusMessage}" 
               HorizontalAlignment="Center" 
               ForegroundColor="Yellow" 
               Visible="{Binding StatusMessage, Converter={StaticResource StringNotEmptyConverter}}" />
        
        <!-- Real-time preview -->
        <FrameView Title="Preview" Height="4">
            <StackView Orientation="Vertical" Margin="1">
                <Label Text="{Binding FullName, StringFormat='Name: {0}'}" />
                <Label Text="{Binding Email, StringFormat='Email: {0}'}" />
                <Label Text="{Binding IsSubscribed, Converter={StaticResource BoolToSubscriptionConverter}}" />
            </StackView>
        </FrameView>
    </StackView>
</Window>
```

## Step 4: Create the Code-Behind

Create `PersonWindow.xaml.cs`:

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using MyTerminalApp.ViewModels;

namespace MyTerminalApp;

public partial class PersonWindow : Window
{
    public PersonViewModel ViewModel { get; }
    
    public PersonWindow()
    {
        ViewModel = new PersonViewModel();
        DataContext = ViewModel;
        
        InitializeComponent();
        SetupConverters();
    }
    
    private void SetupConverters()
    {
        // Add value converters to resources
        Resources.Add("StringNotEmptyConverter", new StringNotEmptyConverter());
        Resources.Add("BoolToSubscriptionConverter", new BoolToSubscriptionConverter());
    }
}

// Value converters for data transformation
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value?.ToString());
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToSubscriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Newsletter: Subscribed ✓" : "Newsletter: Not subscribed";
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

## Step 5: Update Program.cs

Update `Program.cs` to use the new data-bound window:

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;
using MyTerminalApp;

Application.Init();

try
{
    var personWindow = new PersonWindow();
    Application.Run(personWindow);
}
finally
{
    Application.Shutdown();
}
```

## Step 6: Test the Data Binding

Run the application:

```powershell
dotnet build
dotnet run
```

You should see:
- **Automatic UI updates**: As you type, the preview section updates in real-time
- **Command state management**: Save button is disabled until all required fields are valid
- **Two-way binding**: Checkbox state is preserved and reflected in the preview
- **Value conversion**: Boolean subscription status is converted to readable text

## Understanding Data Binding Features

### Property Change Notification
```csharp
public string FirstName
{
    get => _firstName;
    set => SetProperty(ref _firstName, value); // Notifies UI of changes
}
```

### Computed Properties
```csharp
public string FullName => $"{FirstName} {LastName}".Trim();

// Notify when dependencies change
PropertyChanged += (s, e) => 
{
    if (e.PropertyName == nameof(FirstName) || e.PropertyName == nameof(LastName))
        OnPropertyChanged(nameof(FullName));
};
```

### Two-Way Binding

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)

```xml
<TextField Text="{Binding FirstName, Mode=TwoWay}" />
```
- Changes in UI update the view model
- Changes in view model update the UI

> 💡 **v1.2+**: Two-way binding validation support added. Binding errors are automatically displayed with visual indicators.

### Command Binding with CanExecute
```csharp
SaveCommand = new RelayCommand(Save, CanSave);

private bool CanSave()
{
    return !string.IsNullOrWhiteSpace(FirstName) && /* other validation */;
}
```
- Button automatically enables/disables based on `CanSave()`

### Value Converters
```xml
<Label Text="{Binding IsSubscribed, Converter={StaticResource BoolToSubscriptionConverter}}" />
```
- Transforms data for display (bool → text)
- Keeps business logic separate from presentation

## Advanced Binding Scenarios

### Collection Binding
```csharp
public ObservableCollection<Person> People { get; set; } = new();
```

```xml
<ListView ItemsSource="{Binding People}"
          SelectedItem="{Binding SelectedPerson, Mode=TwoWay}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding FullName}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### Validation Binding
```csharp
public string FirstNameError => 
    string.IsNullOrWhiteSpace(FirstName) ? "First name is required" : null;
```

```xml
<TextField Text="{Binding FirstName, Mode=TwoWay}" />
<Label Text="{Binding FirstNameError}" 
       ForegroundColor="Red" 
       Visible="{Binding FirstNameError, Converter={StaticResource StringNotEmptyConverter}}" />
```

### Multi-Binding (Custom)
```csharp
public string FormSummary => $"{People.Count} people, {People.Count(p => p.IsSubscribed)} subscribers";
```

## Next Steps

1. **Add Validation**: Implement `IDataErrorInfo` for robust validation
   - See [Handle Input Guide](handle-input.md)

2. **Master-Detail Views**: Bind collections with selection
   - See [Navigation Guide](navigation.md)

3. **Async Commands**: Handle long-running operations
   - See [Events Guide](../concepts/events.md)

## Troubleshooting

### Data Not Updating

**Property changes not reflected**
- Ensure `INotifyPropertyChanged` is implemented
- Check `SetProperty()` is called in property setters
- Verify `PropertyChanged` event is raised

**Commands not working**
- Check command is bound: `Command="{Binding SaveCommand}"`
- Ensure `CanExecute` logic is correct
- Call `RaiseCanExecuteChanged()` when validation state changes

### Binding Errors

**Null reference exceptions**
- Initialize collections: `new ObservableCollection<T>()`
- Check `DataContext` is set before `InitializeComponent()`
- Handle null values in converters

**Performance issues**
- Avoid complex calculations in property getters
- Use `OneWay` binding for read-only data
- Implement virtual collections for large datasets

## Related Topics

### Concepts
- [Data Binding Concepts](../concepts/binding.md) - Understanding binding fundamentals
- [Layout System](../concepts/layout.md) - UI layout and data display
- [Event Handling](../concepts/events.md) - Commands and event binding

### Examples
- [Basic Layout](../examples/basic-layout.md) - Data binding in practice
- [Button Click](../examples/button-click.md) - Command binding examples

### Related Guides
- [Creating Windows](create-window.md) - Setting up the UI structure
- [Handling Input](handle-input.md) - Advanced input scenarios
- [Navigation](navigation.md) - Passing data between views

### External Resources
- [INotifyPropertyChanged Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged)
- [ObservableCollection<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1)
- [MVVM Pattern Overview](https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
