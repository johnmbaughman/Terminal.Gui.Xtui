# Data Binding Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml`

## What You'll Learn

- One-way and two-way binding basics
- Binding collections to list-based controls
- Raising `INotifyPropertyChanged` efficiently
- Simple validation integration with bindings

## View Model

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class InventoryViewModel : INotifyPropertyChanged
{
    private string _filter = string.Empty;
    private ItemViewModel? _selected;

    public ObservableCollection<ItemViewModel> Items { get; } = new();
    public ObservableCollection<ItemViewModel> Filtered { get; } = new();

    public string Filter
    {
        get => _filter;
        set { if (_filter != value) { _filter = value; OnPropertyChanged(); ApplyFilter(); } }
    }

    public ItemViewModel? Selected
    {
        get => _selected;
        set { if (_selected != value) { _selected = value; OnPropertyChanged(); } }
    }

    public InventoryViewModel()
    {
        Items.Add(new ItemViewModel { Name = "Widget", Quantity = 10 });
        Items.Add(new ItemViewModel { Name = "Gadget", Quantity = 4 });
        Items.Add(new ItemViewModel { Name = "Thing", Quantity = 25 });
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Filtered.Clear();
        var q = string.IsNullOrWhiteSpace(Filter) ? Items : Items.Where(i => i.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase));
        foreach (var item in q) Filtered.Add(item);
        OnPropertyChanged(nameof(Filtered));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ItemViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _quantity;

    public string Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(); } } }
    public int Quantity { get => _quantity; set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); } } }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

## XAML Layout

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Inventory" Width="80" Height="20">
  <GridView Rows="3" Columns="2">
    <Label Grid.Row="0" Grid.Column="0" Text="Filter" />
    <TextField Grid.Row="0" Grid.Column="1" Text="{Binding Filter}" Width="30" />
    <ListView Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2" Source="{Binding Filtered}" x:Name="ItemsList" />
    <FrameView Grid.Row="2" Grid.ColumnSpan="2" Title="Details" Height="6">
      <StackView Orientation="Vertical">
        <Label Text="Name" />
        <TextField Text="{Binding Selected.Name}" Width="40" />
        <Label Text="Quantity" />
        <TextField Text="{Binding Selected.Quantity}" Width="10" />
      </StackView>
    </FrameView>
  </GridView>
</Window>
```

## Code-Behind: Hooking Selection

```csharp
using Terminal.Gui;

public partial class InventoryWindow : Window
{
    private readonly InventoryViewModel _vm = new();

    public InventoryWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        ItemsList.SelectedItemChanged += _ => _vm.Selected = ItemsList.SelectedItem >= 0 ? _vm.Filtered[ItemsList.SelectedItem] : null;
    }
}
```

## Run

```powershell
dotnet run --project samples/DataBindingSample/DataBindingSample.csproj
```

Status: Example — initial content (enhance with async data refresh later)

## Async Refresh with Cancellation

Below we extend the view model to support an asynchronous reload that can be cancelled if a new request starts before the prior completes.

```csharp
using System.Threading;
using System.Threading.Tasks;

public partial class InventoryViewModel
{
  private CancellationTokenSource? _refreshCts;

  public bool IsRefreshing { get; private set; }

  public async Task RefreshAsync()
  {
    _refreshCts?.Cancel();
    var cts = new CancellationTokenSource();
    _refreshCts = cts;
    IsRefreshing = true;
    OnPropertyChanged(nameof(IsRefreshing));
    try
    {
      await Task.Delay(750, cts.Token); // simulate IO
      Items.Clear();
      // Simulated remote data
      Items.Add(new ItemViewModel { Name = "Widget", Quantity = Random.Shared.Next(1,20) });
      Items.Add(new ItemViewModel { Name = "Gadget", Quantity = Random.Shared.Next(1,20) });
      Items.Add(new ItemViewModel { Name = "Thing", Quantity = Random.Shared.Next(1,20) });
      ApplyFilter();
    }
    catch (OperationCanceledException) { }
    finally
    {
      if (_refreshCts == cts)
      {
        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));
      }
    }
  }
}
```

### UI Hook

```xml
<Button Text="Refresh" Clicked="OnRefresh" />
<Label Text="Loading..." Visible="{Binding IsRefreshing}" />
```

```csharp
public partial class InventoryWindow : Window
{
  private async void OnRefresh(object? sender, EventArgs e)
    => await _vm.RefreshAsync();
}
```

Key points:

- Cancels in-flight refresh when a new one starts.
- Exposes `IsRefreshing` for UI state binding.
- Uses `Random` to simulate dynamic data.

Status Extension: Added async pattern with cancellation (future: add debounce + progress bar)
