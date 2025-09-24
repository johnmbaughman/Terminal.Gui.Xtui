# Navigation and Multi-Window Management

This guide covers navigation patterns, window management, and creating multi-view applications in Terminal.Gui.Xaml.

## Related APIs

This guide demonstrates these key navigation and window management APIs:

### Window Management
- **[Window](../../api/Terminal.Gui.Xaml.Window.yml)** - Primary window container for views
- **[Dialog](../../api/Terminal.Gui.Xaml.Dialog.yml)** - Modal dialog windows
- **[WindowManager](../../api/Terminal.Gui.Xaml.WindowManager.yml)** - Multi-window coordination
- **[Application](../../api/Terminal.Gui.Xaml.Application.yml)** - Application-level window lifecycle

### Navigation Controls
- **[MenuBar](../../api/Terminal.Gui.Xaml.MenuBar.yml)** - Top-level navigation menu
- **[MenuItem](../../api/Terminal.Gui.Xaml.MenuItem.yml)** - Individual menu actions
- **[TabView](../../api/Terminal.Gui.Xaml.TabView.yml)** - Tabbed content navigation
- **[TreeView](../../api/Terminal.Gui.Xaml.TreeView.yml)** - Hierarchical content navigation

### View Management
- **[ContentPresenter](../../api/Terminal.Gui.Xaml.ContentPresenter.yml)** - Dynamic content display
- **[Frame](../../api/Terminal.Gui.Xaml.Frame.yml)** - Navigation container for views
- **[UserControl](../../api/Terminal.Gui.Xaml.UserControl.yml)** - Reusable view components
- **[ViewManager](../../api/Terminal.Gui.Xaml.ViewManager.yml)** - View lifecycle management

### Commands and Events
- **[NavigationCommand](../../api/Terminal.Gui.Xaml.Commands.NavigationCommand.yml)** - Navigation-specific commands
- **[WindowClosingEventArgs](../../api/Terminal.Gui.Xaml.WindowClosingEventArgs.yml)** - Window close event handling
- **[NavigationEventArgs](../../api/Terminal.Gui.Xaml.NavigationEventArgs.yml)** - Navigation event data
- **[INavigationService](../../api/Terminal.Gui.Xaml.Services.INavigationService.yml)** - Navigation service interface

> **💡 Pro Tip**: Use `Dialog` for modal interactions and `Frame` with `UserControl` for complex view hierarchies. Always handle `WindowClosing` events for data persistence.

## Prerequisites

- Completed [Create a Window Guide](create-window.md)
- Understanding of [Events Concepts](../concepts/events.md)
- Familiarity with MVVM patterns

## Step 1: Create a Main Navigation Window

Create `NavigationMainWindow.xaml`:

```xml
<Window x:Class="MyTerminalApp.NavigationMainWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="Terminal.Gui.Xaml - Navigation Demo"
        Width="100" Height="30">
    
    <StackView Orientation="Vertical">
        <!-- Menu bar for navigation -->
        <MenuBar>
            <MenuBarItem Title="File">
                <MenuItem Title="New Document" Command="{Binding NewDocumentCommand}" />
                <MenuItem Title="Open Document" Command="{Binding OpenDocumentCommand}" />
                <MenuSeparator />
                <MenuItem Title="Settings" Command="{Binding ShowSettingsCommand}" />
                <MenuSeparator />
                <MenuItem Title="Exit" Command="{Binding ExitCommand}" />
            </MenuBarItem>
            <MenuBarItem Title="View">
                <MenuItem Title="Dashboard" Command="{Binding ShowDashboardCommand}" />
                <MenuItem Title="Data Entry" Command="{Binding ShowDataEntryCommand}" />
                <MenuItem Title="Reports" Command="{Binding ShowReportsCommand}" />
            </MenuBarItem>
            <MenuBarItem Title="Window">
                <MenuItem Title="Cascade Windows" Command="{Binding CascadeWindowsCommand}" />
                <MenuItem Title="Tile Horizontal" Command="{Binding TileHorizontalCommand}" />
                <MenuItem Title="Close All" Command="{Binding CloseAllWindowsCommand}" />
            </MenuBarItem>
        </MenuBar>
        
        <!-- Main content area with tabs -->
        <TabView Name="mainTabs" 
                 SelectedTab="{Binding SelectedTabIndex, Mode=TwoWay}"
                 Height="Dim.Fill(3)">
            
            <!-- Dashboard tab -->
            <Tab Text="Dashboard">
                <StackView Orientation="Vertical" Spacing="1" Margin="1">
                    <Label Text="Application Dashboard" 
                           HorizontalAlignment="Center" 
                           ForegroundColor="Cyan" />
                    
                    <!-- Quick action buttons -->
                    <FrameView Title="Quick Actions" Height="8">
                        <StackView Orientation="Vertical" Spacing="1" Margin="1">
                            <Button Text="Create New Document" 
                                    Command="{Binding NewDocumentCommand}" 
                                    Width="Dim.Fill()" />
                            <Button Text="Open Existing Document" 
                                    Command="{Binding OpenDocumentCommand}" 
                                    Width="Dim.Fill()" />
                            <Button Text="View Reports" 
                                    Command="{Binding ShowReportsCommand}" 
                                    Width="Dim.Fill()" />
                            <Button Text="Application Settings" 
                                    Command="{Binding ShowSettingsCommand}" 
                                    Width="Dim.Fill()" />
                        </StackView>
                    </FrameView>
                    
                    <!-- Recent items -->
                    <FrameView Title="Recent Documents">
                        <ListView ItemsSource="{Binding RecentDocuments}"
                                  SelectedItem="{Binding SelectedDocument, Mode=TwoWay}">
                            <ListView.ItemTemplate>
                                <DataTemplate>
                                    <Label Text="{Binding DisplayName}" />
                                </DataTemplate>
                            </ListView.ItemTemplate>
                        </ListView>
                    </FrameView>
                </StackView>
            </Tab>
            
            <!-- Data entry tab -->
            <Tab Text="Data Entry">
                <ContentPresenter Content="{Binding DataEntryView}" />
            </Tab>
            
            <!-- Reports tab -->
            <Tab Text="Reports">
                <ContentPresenter Content="{Binding ReportsView}" />
            </Tab>
            
        </TabView>
        
        <!-- Status bar -->
        <StackView Orientation="Horizontal" Y="Pos.Bottom()" Height="1">
            <Label Text="{Binding StatusMessage}" />
            <Label Text="{Binding ActiveWindowsCount, StringFormat='Windows: {0}'}" 
                   X="Pos.Right()" />
        </StackView>
        
    </StackView>
</Window>
```

## Step 2: Create Supporting Services

Create `NavigationService.cs`:

```csharp
using Terminal.Gui;

namespace MyTerminalApp.Services;

public interface INavigationService
{
    Task<Window> CreateDocumentWindowAsync(DocumentViewModel document = null);
    Window CreateSettingsWindow();
    View CreateDataEntryView();
    View CreateReportsView();
}

public class NavigationService : INavigationService
{
    public async Task<Window> CreateDocumentWindowAsync(DocumentViewModel document = null)
    {
        var viewModel = new DocumentWindowViewModel(document);
        var window = new DocumentWindow { DataContext = viewModel };
        return window;
    }
    
    public Window CreateSettingsWindow()
    {
        var viewModel = new SettingsViewModel();
        return new SettingsWindow { DataContext = viewModel };
    }
    
    public View CreateDataEntryView()
    {
        var viewModel = new DataEntryViewModel();
        return new DataEntryView { DataContext = viewModel };
    }
    
    public View CreateReportsView()
    {
        var viewModel = new ReportsViewModel();
        return new ReportsView { DataContext = viewModel };
    }
}
```

Create `WindowManager.cs`:

```csharp
using Terminal.Gui;

namespace MyTerminalApp.Services;

public interface IWindowManager
{
    void ShowWindow(Window window);
    void ShowModal(Window window);
    void CloseWindow(Window window);
    void CascadeWindows();
    void TileHorizontal();
}

public class WindowManager : IWindowManager
{
    private readonly List<Window> _openWindows = new();
    
    public void ShowWindow(Window window)
    {
        _openWindows.Add(window);
        
        // Position new windows slightly offset
        var offset = _openWindows.Count * 2;
        window.X = offset;
        window.Y = offset;
        
        Application.Run(window);
    }
    
    public void ShowModal(Window window)
    {
        Application.Run(window);
    }
    
    public void CloseWindow(Window window)
    {
        if (_openWindows.Contains(window))
        {
            _openWindows.Remove(window);
            window.RequestStop();
        }
    }
    
    public void CascadeWindows()
    {
        for (int i = 0; i < _openWindows.Count; i++)
        {
            _openWindows[i].X = i * 3;
            _openWindows[i].Y = i * 2;
        }
    }
    
    public void TileHorizontal()
    {
        if (_openWindows.Count == 0) return;
        
        var availableHeight = Application.Top.Frame.Height / _openWindows.Count;
        for (int i = 0; i < _openWindows.Count; i++)
        {
            var window = _openWindows[i];
            window.Y = i * availableHeight;
            window.Height = availableHeight;
        }
    }
}
```

## Step 3: Test Navigation

Run the application:

```powershell
dotnet build
dotnet run
```

Test these navigation features:
- **Tab navigation**: Switch between Dashboard, Data Entry, Reports
- **Window management**: Open multiple document windows, cascade/tile them
- **Modal dialogs**: Settings window opens modally
- **Recent documents**: Click items to reopen them
- **Menu navigation**: Access all features through menus

## Understanding Navigation Patterns

### Tab-Based Navigation
```xml
<TabView SelectedTab="{Binding SelectedTabIndex, Mode=TwoWay}">
    <Tab Text="Dashboard"><!-- Content --></Tab>
    <Tab Text="Data Entry"><!-- Content --></Tab>
</TabView>
```

### Service-Based Navigation
```csharp
public interface INavigationService
{
    Window CreateSettingsWindow();
    void NavigateTo<T>() where T : Window, new();
}
```

### Window Management
```csharp
public void CascadeWindows()
{
    for (int i = 0; i < _openWindows.Count; i++)
    {
        _openWindows[i].X = i * 3;
        _openWindows[i].Y = i * 2;
    }
}
```

## Next Steps

1. **Add State Management**: Preserve navigation state across sessions
2. **Implement Breadcrumbs**: Show navigation hierarchy  
3. **Add Keyboard Navigation**: Full keyboard-only navigation support
4. **Create Custom Views**: Build reusable navigation components

## Related Topics

### API Reference
- **[Window](../../api/Terminal.Gui.Xaml.Window.yml)** - Primary window container
- **[Dialog](../../api/Terminal.Gui.Xaml.Dialog.yml)** - Modal dialog handling
- **[MenuBar](../../api/Terminal.Gui.Xaml.MenuBar.yml)** - Menu navigation control
- **[TabView](../../api/Terminal.Gui.Xaml.TabView.yml)** - Tabbed content navigation
- **[Frame](../../api/Terminal.Gui.Xaml.Frame.yml)** - Content navigation container
- **[NavigationCommand](../../api/Terminal.Gui.Xaml.Commands.NavigationCommand.yml)** - Navigation commands

### Concepts
- **[Window Lifecycle](../concepts/window-lifecycle.md)** - Window creation and management
- **[Event Handling](../concepts/events.md)** - Navigation event patterns
- **[View Management](../concepts/views.md)** - View composition and lifecycle
- **[Application Architecture](../concepts/architecture.md)** - Multi-window app patterns

### Examples
- **[Multi-Window App](../examples/multi-window.md)** - Complete navigation example
- **[Dialog Usage](../examples/dialogs.md)** - Modal and non-modal dialogs
- **[Menu Navigation](../examples/menu-navigation.md)** - Menu-driven applications

### Related Guides
- **[Create a Window](create-window.md)** - Basic window creation and setup
- **[Handle Input](handle-input.md)** - Input handling across multiple views
- **[Bind Data](bind-data.md)** - Data context management in navigation

### External Resources
- **[MVVM Navigation](https://docs.microsoft.com/en-us/dotnet/architecture/maui/mvvm)** - Navigation in MVVM applications
- **[Window Management Patterns](https://docs.microsoft.com/en-us/windows/apps/design/layout/)** - UI navigation best practices
