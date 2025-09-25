# Tutorial: Build a Mini App

An end-to-end walkthrough building a complete Terminal.Gui.Xaml application from scratch.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Duration**: 30-45 minutes  
> **Level**: Intermediate  
> **Prerequisites**: [Getting Started](../getting-started.md) completed

## What You'll Build

In this tutorial, you'll create a complete file manager application that demonstrates:

- **Multi-window navigation** with proper focus management
- **Data binding** with live updates from file system
- **Custom commands** for file operations
- **Error handling** with user-friendly messages
- **Responsive layout** that adapts to terminal size
- **Keyboard shortcuts** and accessibility patterns

## Learning Objectives

By the end of this tutorial, you'll understand how to:

1. **Structure a multi-file XAML application**
2. **Implement MVVM pattern** with ViewModels and Commands
3. **Handle asynchronous operations** in terminal UI
4. **Create reusable custom controls** 
5. **Test and debug** Terminal.Gui.Xaml applications

## Prerequisites

- **Terminal.Gui.Xaml basics** from [Getting Started](../getting-started.md)
- **C# knowledge** including async/await and MVVM concepts
- **Understanding of data binding** from [Binding Guide](../guides/bind-data.md)
- **.NET 8+ SDK** installed and working

## Application Overview

### Features
- **File Browser**: Navigate directories with keyboard and mouse
- **File Operations**: Copy, move, delete files with confirmation dialogs
- **Search**: Find files by name with live filtering
- **Properties**: View file details in a separate pane
- **Status Bar**: Show current directory and file count
- **Menu System**: Access all functions via keyboard shortcuts

### Architecture
```
FileManagerApp/
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── FileItemViewModel.cs
│   └── PropertiesViewModel.cs
├── Views/
│   ├── MainWindow.xaml
│   ├── FileListView.xaml
│   └── PropertiesPanel.xaml
├── Services/
│   ├── IFileService.cs
│   └── FileService.cs
├── Models/
│   └── FileItem.cs
└── Program.cs
```

## Step 1: Project Setup

### Create the Project Structure

```powershell
# Create new solution
dotnet new sln -n FileManagerApp
cd FileManagerApp

# Create main application project
dotnet new console -n FileManagerApp
dotnet sln add FileManagerApp/FileManagerApp.csproj

# Add Terminal.Gui.Xaml package
cd FileManagerApp
dotnet add package Terminal.Gui.Xaml

# Create folder structure
New-Item -ItemType Directory -Path "ViewModels", "Views", "Services", "Models"
```

### Configure Project File

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

## Step 2: Core Models and Services

### File Item Model

```csharp
// Models/FileItem.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileManagerApp.Models;

public class FileItem : INotifyPropertyChanged
{
    private bool _isSelected;
    
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime Modified { get; set; }
    public bool IsDirectory { get; set; }
    public string Icon => IsDirectory ? "📁" : "📄";
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string DisplaySize => IsDirectory ? "---" : FormatBytes(Size);
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:N1} {suffixes[counter]}";
    }
}
```

### File Service

```csharp
// Services/IFileService.cs
using FileManagerApp.Models;

namespace FileManagerApp.Services;

public interface IFileService
{
    Task<IEnumerable<FileItem>> GetFilesAsync(string path);
    Task<FileItem?> GetFileInfoAsync(string path);
    Task<bool> DeleteFileAsync(string path);
    Task<bool> CopyFileAsync(string sourcePath, string destinationPath);
    Task<bool> MoveFileAsync(string sourcePath, string destinationPath);
}
```

```csharp
// Services/FileService.cs
using FileManagerApp.Models;

namespace FileManagerApp.Services;

public class FileService : IFileService
{
    public async Task<IEnumerable<FileItem>> GetFilesAsync(string path)
    {
        try
        {
            var items = new List<FileItem>();
            
            // Add parent directory if not root
            if (Directory.GetParent(path) != null)
            {
                items.Add(new FileItem
                {
                    Name = "..",
                    FullPath = Directory.GetParent(path)!.FullName,
                    IsDirectory = true,
                    Modified = DateTime.Now
                });
            }
            
            // Add directories
            var directories = Directory.GetDirectories(path);
            foreach (var dir in directories)
            {
                var info = new DirectoryInfo(dir);
                items.Add(new FileItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsDirectory = true,
                    Modified = info.LastWriteTime
                });
            }
            
            // Add files
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                items.Add(new FileItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsDirectory = false,
                    Size = info.Length,
                    Modified = info.LastWriteTime
                });
            }
            
            return items.OrderBy(x => !x.IsDirectory).ThenBy(x => x.Name);
        }
        catch
        {
            return Enumerable.Empty<FileItem>();
        }
    }

    public async Task<FileItem?> GetFileInfoAsync(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);
                return new FileItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsDirectory = true,
                    Modified = info.LastWriteTime
                };
            }
            else if (File.Exists(path))
            {
                var info = new FileInfo(path);
                return new FileItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsDirectory = false,
                    Size = info.Length,
                    Modified = info.LastWriteTime
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CopyFileAsync(string sourcePath, string destinationPath)
    {
        try
        {
            File.Copy(sourcePath, destinationPath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MoveFileAsync(string sourcePath, string destinationPath)
    {
        try
        {
            File.Move(sourcePath, destinationPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Step 3: ViewModels with Commands

### Main View Model

```csharp
// ViewModels/MainViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileManagerApp.Models;
using FileManagerApp.Services;
using Terminal.Gui;

namespace FileManagerApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IFileService _fileService;
    private string _currentPath;
    private FileItem? _selectedItem;
    private string _statusText;
    private bool _isLoading;

    public MainViewModel(IFileService fileService)
    {
        _fileService = fileService;
        _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _statusText = "Ready";
        
        Files = new ObservableCollection<FileItem>();
        
        // Initialize commands
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        NavigateCommand = new AsyncRelayCommand<FileItem>(NavigateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync);
        CopyCommand = new AsyncRelayCommand(CopySelectedAsync);
        
        // Load initial directory
        _ = RefreshAsync();
    }
    
    public ObservableCollection<FileItem> Files { get; }
    
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WindowTitle));
        }
    }
    
    public FileItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }
    
    public string StatusText
    {
        get
        {
            if (SelectedItem != null)
                return $"{SelectedItem.Name} - {SelectedItem.DisplaySize}";
            return $"{Files.Count} items in {Path.GetFileName(CurrentPath)}";
        }
    }
    
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }
    
    public string WindowTitle => $"File Manager - {Path.GetFileName(CurrentPath)}";
    
    public ICommand RefreshCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CopyCommand { get; }
    
    private async Task RefreshAsync()
    {
        IsLoading = true;
        try
        {
            var files = await _fileService.GetFilesAsync(CurrentPath);
            Files.Clear();
            foreach (var file in files)
            {
                Files.Add(file);
            }
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(StatusText));
        }
    }
    
    private async Task NavigateAsync(FileItem? item)
    {
        if (item?.IsDirectory == true)
        {
            CurrentPath = item.FullPath;
            await RefreshAsync();
        }
    }
    
    private async Task DeleteSelectedAsync()
    {
        if (SelectedItem == null) return;
        
        var result = MessageBox.Query("Confirm Delete", 
            $"Delete {SelectedItem.Name}?", "Yes", "No");
            
        if (result == 0)
        {
            var success = await _fileService.DeleteFileAsync(SelectedItem.FullPath);
            if (success)
            {
                await RefreshAsync();
            }
            else
            {
                MessageBox.ErrorQuery("Error", "Failed to delete file.", "OK");
            }
        }
    }
    
    private async Task CopySelectedAsync()
    {
        // Implementation for copy dialog
        MessageBox.Query("Copy", "Copy functionality - implement dialog", "OK");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Helper class for async commands
public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter) => await _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class AsyncRelayCommand<T> : ICommand
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    public async void Execute(object? parameter) => await _execute((T?)parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

## Step 4: XAML Views

### Main Window

```xml
<!-- Views/MainWindow.xaml -->
<Window x:Class="FileManagerApp.Views.MainWindow"
        xmlns="http://terminal.gui.xaml/winfx/2006/xaml/presentation"
        xmlns:x="http://www.w3.org/1999/xlink"
        Title="{Binding WindowTitle}"
        Width="100" Height="30">
    
    <Window.KeyBindings>
        <KeyBinding Key="F5" Command="{Binding RefreshCommand}" />
        <KeyBinding Key="Delete" Command="{Binding DeleteCommand}" />
        <KeyBinding Key="F4" Command="{Binding CopyCommand}" />
    </Window.KeyBindings>
    
    <StackView Orientation="Vertical">
        
        <!-- Menu Bar -->
        <MenuBar>
            <MenuBarItem Title="File">
                <MenuItem Title="Refresh" Shortcut="F5" Command="{Binding RefreshCommand}" />
                <MenuItem Title="Copy" Shortcut="F4" Command="{Binding CopyCommand}" />
                <MenuSeparator />
                <MenuItem Title="Delete" Shortcut="Del" Command="{Binding DeleteCommand}" />
                <MenuSeparator />
                <MenuItem Title="Exit" Shortcut="Alt+F4" Click="OnExitClicked" />
            </MenuBarItem>
            <MenuBarItem Title="View">
                <MenuItem Title="Refresh" Command="{Binding RefreshCommand}" />
            </MenuBarItem>
        </MenuBar>
        
        <!-- Toolbar -->
        <StackView Orientation="Horizontal" Height="1">
            <Label Text="Path: " />
            <Label Text="{Binding CurrentPath}" Width="Dim.Fill()" />
        </StackView>
        
        <!-- Main Content Area -->
        <StackView Orientation="Horizontal" Height="Dim.Fill(2)">
            
            <!-- File List -->
            <FrameView Title="Files" Width="Dim.Percent(60)">
                <ListView Name="fileListView"
                          ItemsSource="{Binding Files}"
                          SelectedItem="{Binding SelectedItem, Mode=TwoWay}"
                          AccessibleName="File list">
                    <ListView.ItemTemplate>
                        <DataTemplate>
                            <StackView Orientation="Horizontal">
                                <Label Text="{Binding Icon}" Width="2" />
                                <Label Text="{Binding Name}" Width="30" />
                                <Label Text="{Binding DisplaySize}" Width="10" HorizontalAlignment="Right" />
                            </StackView>
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>
            </FrameView>
            
            <!-- Properties Panel -->
            <FrameView Title="Properties" Width="Dim.Fill()">
                <StackView Orientation="Vertical" Margin="1">
                    <Label Text="Name:" />
                    <Label Text="{Binding SelectedItem.Name}" ForegroundColor="Cyan" />
                    
                    <Label Text="Size:" />
                    <Label Text="{Binding SelectedItem.DisplaySize}" ForegroundColor="Yellow" />
                    
                    <Label Text="Modified:" />
                    <Label Text="{Binding SelectedItem.Modified}" ForegroundColor="Green" />
                    
                    <Label Text="Type:" />
                    <Label ForegroundColor="Magenta">
                        <Label.Text>
                            <Binding Path="SelectedItem.IsDirectory">
                                <Binding.Converter>
                                    <ValueConverter>
                                        <ValueConverter.ConvertMethod>
                                            <![CDATA[
                                            (bool isDir) => isDir ? "Directory" : "File"
                                            ]]>
                                        </ValueConverter.ConvertMethod>
                                    </ValueConverter>
                                </Binding.Converter>
                            </Binding>
                        </Label.Text>
                    </Label>
                </StackView>
            </FrameView>
            
        </StackView>
        
        <!-- Status Bar -->
        <Label Name="statusBar" 
               Text="{Binding StatusText}" 
               Height="1" 
               ForegroundColor="Black" 
               BackgroundColor="Gray" />
               
    </StackView>
</Window>
```

### Main Window Code-Behind

```csharp
// Views/MainWindow.xaml.cs
using FileManagerApp.ViewModels;
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace FileManagerApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Handle double-click for navigation
        fileListView.OpenSelectedItem += OnFileDoubleClick;
    }
    
    private void OnFileDoubleClick(object sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel && viewModel.SelectedItem != null)
        {
            viewModel.NavigateCommand.Execute(viewModel.SelectedItem);
        }
    }
    
    private void OnExitClicked(object sender, EventArgs e)
    {
        Application.RequestStop();
    }
}
```

## Step 5: Application Entry Point

```csharp
// Program.cs
using FileManagerApp.Services;
using FileManagerApp.ViewModels;
using FileManagerApp.Views;
using Terminal.Gui;

namespace FileManagerApp;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();

        try
        {
            // Setup dependency injection (simple)
            var fileService = new FileService();
            var mainViewModel = new MainViewModel(fileService);
            
            // Create and configure main window
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // Run the application
            Application.Run(mainWindow);
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Fatal Error", 
                $"Application error: {ex.Message}", "OK");
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
```

## Step 6: Testing and Debugging

### Manual Testing

```powershell
# Build and run
dotnet build
dotnet run

# Test key scenarios:
# 1. Navigate directories with Enter key
# 2. Select files with arrow keys
# 3. Use F5 to refresh
# 4. Try Delete key on a test file
# 5. Test menu access with Alt keys
```

### Expected Behavior

**On startup:**
- Shows current user home directory
- File list populated with directories and files
- Properties panel shows selected item details
- Status bar shows item count

**Navigation:**
- Double-click or Enter navigates into directories
- ".." entry navigates to parent directory
- Arrow keys select different files
- Properties panel updates with selection

**File Operations:**
- F5 refreshes the current directory
- Delete key prompts for confirmation
- Menu items work via keyboard shortcuts

## Step 7: Enhancements

### Add Search Functionality

```xml
<!-- Add to toolbar section -->
<StackView Orientation="Horizontal" Height="1">
    <Label Text="Search: " />
    <TextField Name="searchBox" 
               Text="{Binding SearchText, Mode=TwoWay}" 
               Width="20" />
    <Button Text="Clear" Command="{Binding ClearSearchCommand}" />
</StackView>
```

### Implement File Filtering

```csharp
// Add to MainViewModel
private string _searchText = string.Empty;
public string SearchText
{
    get => _searchText;
    set
    {
        _searchText = value;
        OnPropertyChanged();
        _ = FilterFilesAsync();
    }
}

private async Task FilterFilesAsync()
{
    // Filter files based on search text
    // Implementation details...
}
```

### Add Keyboard Shortcuts

```xml
<Window.KeyBindings>
    <KeyBinding Key="Ctrl+C" Command="{Binding CopyCommand}" />
    <KeyBinding Key="Ctrl+X" Command="{Binding CutCommand}" />
    <KeyBinding Key="Ctrl+V" Command="{Binding PasteCommand}" />
    <KeyBinding Key="F2" Command="{Binding RenameCommand}" />
    <KeyBinding Key="Alt+Enter" Command="{Binding PropertiesCommand}" />
</Window.KeyBindings>
```

## Troubleshooting

### Common Issues

**Files don't load on startup**
```
- Check CurrentPath is valid
- Verify file service permissions
- Add error handling in RefreshAsync()
```

**ListView doesn't respond to selection**
```
- Ensure SelectedItem binding is TwoWay
- Check DataContext is properly set
- Verify INotifyPropertyChanged implementation
```

**Commands don't execute**
```
- Check command binding syntax
- Verify CanExecute returns true
- Add debugging to command methods
```

**XAML parsing errors**
```
- Validate XML namespace declarations
- Check property names and types
- Use DocFX build warnings to identify issues
```

## Next Steps

Congratulations! You've built a complete file manager application. Consider these enhancements:

1. **Add Configuration**: Settings for default directory, colors, etc.
2. **Implement Plugins**: Extensible architecture for custom file types
3. **Add Tests**: Unit tests for ViewModels and Services
4. **Improve UI**: Icons, colors, better layouts
5. **Add Logging**: Structured logging for debugging

## Related Documentation

- [Data Binding Guide](../guides/bind-data.md) - Advanced binding scenarios
- Custom Controls Guide (coming soon)  
- Performance Guide (coming soon)
- [Testing Guide](../../tests/TestStrategy.md) - Unit and integration testing

---

> **Completed Tutorial**: You now have experience building production-ready Terminal.Gui.Xaml applications with MVVM, commands, and proper architecture patterns.
