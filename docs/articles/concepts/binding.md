# Data Binding Concepts

## What is Data Binding in Terminal.Gui.Xaml?

Data binding automatically synchronizes data between your UI elements and your application's data model. Instead of manually updating UI controls when data changes, binding creates a live connection that keeps everything in sync automatically.

## How Data Binding Works

### Binding Sources
Data can come from various sources:
- **Properties**: Object properties that implement `INotifyPropertyChanged`
- **Collections**: Lists that implement `INotifyCollectionChanged`
- **Resources**: Static data defined in XAML
- **Other UI Elements**: Binding to properties of other controls

### Binding Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `OneWay` | Source → Target only | Display read-only data |
| `TwoWay` | Source ↔ Target | Input controls (TextBox, CheckBox) |
| `OneTime` | Source → Target once | Static initialization |
| `OneWayToSource` | Target → Source only | Custom controls |

### Basic Binding Syntax
```xml
<TextField Text="{Binding UserName}" />
<Label Text="{Binding FullName, Mode=OneWay}" />
<CheckBox Checked="{Binding IsEnabled, Mode=TwoWay}" />
```

### Property Paths
Navigate object hierarchies with dot notation:
```xml
<Label Text="{Binding User.Address.City}" />
<TextField Text="{Binding Items[0].Name}" />
```

## Why Use Data Binding?

### Automatic Updates
When your data changes, the UI updates automatically:

```csharp
public class PersonViewModel : INotifyPropertyChanged
{
    private string _name;
    public string Name 
    { 
        get => _name; 
        set { _name = value; OnPropertyChanged(); }
    }
    
    // PropertyChanged event implementation
}
```

```xml
<!-- Updates automatically when Name property changes -->
<Label Text="{Binding Name}" />
```

### Reduced Code
Eliminate manual UI update code:

```csharp
// Without binding - manual updates
nameLabel.Text = person.Name;
person.PropertyChanged += (s, e) => {
    if (e.PropertyName == "Name") 
        nameLabel.Text = person.Name;
};

// With binding - automatic
// <Label Text="{Binding Name}" />
```

### Testability
Separate UI from business logic:
- Test view models without UI
- Mock data sources easily
- Clear separation of concerns

## Common Binding Scenarios

### Form Input Binding
```xml
<Window DataContext="{Binding PersonViewModel}">
    <StackView Orientation="Vertical">
        <Label Text="Name:" />
        <TextField Text="{Binding Name, Mode=TwoWay}" />
        
        <Label Text="Age:" />
        <TextField Text="{Binding Age, Mode=TwoWay}" />
        
        <CheckBox Text="Active" Checked="{Binding IsActive, Mode=TwoWay}" />
        
        <Button Text="Save" Command="{Binding SaveCommand}" />
    </StackView>
</Window>
```

### List Binding
```xml
<ListView ItemsSource="{Binding People}">
    <ListView.ItemTemplate>
        <DataTemplate>
            <Label Text="{Binding FullName}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### Master-Detail Binding
```xml
<Window>
    <ListView Name="peopleList" 
              ItemsSource="{Binding People}" 
              SelectedItem="{Binding SelectedPerson, Mode=TwoWay}"
              Width="Dim.Percent(40)" />
    
    <FrameView Title="Details" X="Pos.Right(peopleList)">
        <StackView DataContext="{Binding SelectedPerson}">
            <Label Text="{Binding Name}" />
            <Label Text="{Binding Email}" />
            <TextField Text="{Binding Notes, Mode=TwoWay}" />
        </StackView>
    </FrameView>
</Window>
```

### Conditional Binding
```xml
<!-- Show different content based on data -->
<Label Text="{Binding Status}" 
       ColorScheme="{Binding IsOnline, Converter={StaticResource BoolToColorConverter}}" />

<!-- Enable/disable based on data -->
<Button Text="Submit" 
        Enabled="{Binding CanSubmit}" 
        Command="{Binding SubmitCommand}" />
```

## Value Converters
Transform data during binding:

```csharp
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Online" : "Offline";
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == "Online";
    }
}
```

```xml
<Label Text="{Binding IsOnline, Converter={StaticResource BoolToStatusConverter}}" />
```

## Binding Context and DataContext

### Window-Level Context
```xml
<Window DataContext="{Binding MainViewModel}">
    <!-- All child controls inherit this context -->
    <Label Text="{Binding Title}" />
    <Button Command="{Binding CloseCommand}" />
</Window>
```

### Control-Level Context
```xml
<FrameView DataContext="{Binding UserProfile}">
    <Label Text="{Binding Name}" />
    <Label Text="{Binding Email}" />
</FrameView>
```

### Relative Binding
```xml
<!-- Bind to parent window's property -->
<Label Text="{Binding RelativeSource={RelativeSource AncestorType=Window}, Path=Title}" />

<!-- Bind to another control -->
<Label Text="{Binding ElementName=nameField, Path=Text}" />
```

## Performance Considerations

### Efficient Collections
Use `ObservableCollection<T>` for automatic collection change notifications:

```csharp
public ObservableCollection<Person> People { get; set; } = new();

// Automatically notifies UI of changes
People.Add(new Person { Name = "John" });
People.RemoveAt(0);
```

### Property Change Notifications
Implement `INotifyPropertyChanged` efficiently:

```csharp
public class BaseViewModel : INotifyPropertyChanged
{
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) 
            return false;
            
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
}
```

## See Also
- [Layout Concepts](layout.md) - Position bound controls effectively
- [Events Guide](events.md) - Handle binding-related events
- [Bind Data Guide](../guides/bind-data.md) - Step-by-step binding implementation
