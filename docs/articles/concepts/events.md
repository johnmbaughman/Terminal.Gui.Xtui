# Event Handling Concepts

## What are Events in Terminal.Gui.Xaml?

Events in Terminal.Gui.Xaml provide a way to respond to user interactions and system notifications. Unlike traditional event handling where you write code-behind, Terminal.Gui.Xaml supports both declarative event binding in XAML and command patterns for better separation of concerns.

## How Event Handling Works

### Event Types

#### User Input Events
- **Click**: Button presses, menu selections
- **KeyPress**: Keyboard input, shortcuts
- **Focus**: Control focus changes
- **Selection**: List item selection, text selection

#### System Events
- **Loaded**: Control initialization complete
- **SizeChanged**: Terminal or control resized
- **Timer**: Scheduled or repeated actions

#### Data Events
- **PropertyChanged**: Data binding notifications
- **CollectionChanged**: List modifications
- **Validation**: Input validation results

### Event Handling Approaches

#### 1. Command Binding (Recommended)
Use commands to separate UI from business logic:

```xml
<Button Text="Save" Command="{Binding SaveCommand}" />
<Button Text="Cancel" Command="{Binding CancelCommand}" />
```

```csharp
public class ViewModel
{
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    
    public ViewModel()
    {
        SaveCommand = new RelayCommand(Save, CanSave);
        CancelCommand = new RelayCommand(Cancel);
    }
    
    private void Save() { /* Save logic */ }
    private bool CanSave() => /* Validation logic */;
    private void Cancel() { /* Cancel logic */ }
}
```

#### 2. Event Binding
Bind events directly to view model methods:

```xml
<TextField TextChanged="{Binding OnTextChanged}" />
<ListView SelectionChanged="{Binding OnSelectionChanged}" />
```

#### 3. Code-Behind (When Necessary)
For UI-specific logic that doesn't belong in view models:

```xml
<Button Text="Browse" Click="BrowseButton_Click" />
```

```csharp
private void BrowseButton_Click(object sender, EventArgs e)
{
    // File dialog or UI-specific logic
}
```

## Why Use Commands Over Events?

### Testability
Commands can be tested without UI:

```csharp
[Test]
public void SaveCommand_WithValidData_CallsSaveService()
{
    // Arrange
    var viewModel = new DocumentViewModel();
    viewModel.Document = new Document { Title = "Test" };
    
    // Act
    viewModel.SaveCommand.Execute(null);
    
    // Assert
    mockSaveService.Verify(s => s.Save(It.IsAny<Document>()), Times.Once);
}
```

### Automatic Enable/Disable
Commands support `CanExecute` for automatic UI state management:

```xml
<!-- Button automatically disabled when CanSave returns false -->
<Button Text="Save" Command="{Binding SaveCommand}" />
```

### Keyboard Shortcuts
Commands work seamlessly with keyboard bindings:

```xml
<Window>
    <Window.KeyBindings>
        <KeyBinding Key="Ctrl+S" Command="{Binding SaveCommand}" />
        <KeyBinding Key="Ctrl+N" Command="{Binding NewCommand}" />
    </Window.KeyBindings>
</Window>
```

## Common Event Handling Patterns

### Form Validation
```xml
<StackView>
    <TextField Name="nameField" 
               Text="{Binding Name, Mode=TwoWay}"
               TextChanged="{Binding ValidateInput}" />
    
    <Label Text="{Binding ValidationMessage}" 
           ForegroundColor="Red" 
           Visible="{Binding HasValidationError}" />
    
    <Button Text="Submit" 
            Command="{Binding SubmitCommand}"
            Enabled="{Binding IsFormValid}" />
</StackView>
```

### Master-Detail Selection
```xml
<Window>
    <ListView ItemsSource="{Binding Items}"
              SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
              SelectionChanged="{Binding OnItemSelected}" />
    
    <FrameView DataContext="{Binding SelectedItem}">
        <TextField Text="{Binding Name, Mode=TwoWay}" />
        <Button Text="Delete" Command="{Binding DataContext.DeleteCommand, 
                                              RelativeSource={RelativeSource AncestorType=Window}}"
                CommandParameter="{Binding}" />
    </FrameView>
</Window>
```

### Confirmation Dialogs
```csharp
public class DeleteCommand : ICommand
{
    public bool CanExecute(object parameter) => parameter != null;
    
    public void Execute(object parameter)
    {
        var result = MessageBox.Query("Delete", "Are you sure?", "Yes", "No");
        if (result == 0) // Yes
        {
            // Perform delete
        }
    }
    
    public event EventHandler CanExecuteChanged;
}
```

### Progress Updates
```xml
<Window>
    <ProgressBar Value="{Binding Progress}" Maximum="100" />
    <Label Text="{Binding StatusMessage}" />
    <Button Text="Start" Command="{Binding StartProcessCommand}" />
    <Button Text="Cancel" Command="{Binding CancelProcessCommand}" />
</Window>
```

```csharp
public async Task StartProcess()
{
    for (int i = 0; i <= 100; i++)
    {
        Progress = i;
        StatusMessage = $"Processing... {i}%";
        await Task.Delay(50);
        
        if (cancellationToken.IsCancellationRequested)
            break;
    }
}
```

## Event Propagation and Bubbling

### Focus Events
```xml
<FrameView GotFocus="{Binding OnFrameFocused}" LostFocus="{Binding OnFrameUnfocused}">
    <TextField GotFocus="{Binding OnTextFieldFocused}" />
    <Button GotFocus="{Binding OnButtonFocused}" />
</FrameView>
```

### Key Events
```xml
<Window KeyDown="{Binding OnGlobalKeyDown}">
    <TextField KeyDown="{Binding OnTextFieldKeyDown}" />
    <!-- Event bubbles up if not handled -->
</Window>
```

### Handled Property
Stop event propagation when handled:

```csharp
private void OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    {
        Close();
        e.Handled = true; // Stop bubbling
    }
}
```

## Asynchronous Event Handling

### Async Commands
```csharp
public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool> _canExecute;
    private bool _isExecuting;
    
    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);
    
    public async void Execute(object parameter)
    {
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        
        try
        {
            await _executeAsync();
        }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public event EventHandler CanExecuteChanged;
}
```

### Background Operations
```csharp
public class LoadDataCommand : AsyncRelayCommand
{
    public LoadDataCommand(IDataService dataService) 
        : base(async () => await LoadDataAsync(dataService))
    {
    }
    
    private static async Task LoadDataAsync(IDataService dataService)
    {
        // Show loading indicator
        IsLoading = true;
        
        try
        {
            var data = await dataService.GetDataAsync();
            Items.Clear();
            foreach (var item in data)
                Items.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

## Performance Considerations

### Event Handler Cleanup
Always unsubscribe from events to prevent memory leaks:

```csharp
public class ViewModel : IDisposable
{
    private Timer _timer;
    
    public ViewModel()
    {
        _timer = new Timer(OnTimerTick, null, 1000, 1000);
    }
    
    public void Dispose()
    {
        _timer?.Dispose();
        // Unsubscribe from other events
    }
}
```

### Efficient Event Handling
Use weak event patterns for long-lived subscriptions:

```csharp
WeakEventManager.AddHandler(source, "PropertyChanged", OnPropertyChanged);
```

## See Also
- [Layout Concepts](layout.md) - Handle layout-related events
- [Binding Concepts](binding.md) - Event binding and commands
- [Handle Input Guide](../guides/handle-input.md) - Step-by-step input handlingvents (Concept)

Explain What/How/Why for events in Terminal.Gui.Xaml.
