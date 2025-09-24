# Getting Started with Terminal.Gui.Xaml

This guide helps you set up Terminal.Gui.Xaml and create a minimal UI using XAML.

## Prerequisites

- .NET 8+ SDK
- Terminal.Gui v2+
- Basic familiarity with XAML

## Install packages

From your app’s project folder, add the required packages:

```pwsh
dotnet add package Terminal.Gui
dotnet add package Terminal.Gui.Xaml
```

## Create your first XAML UI

1) Add a XAML file named `MainWindow.xaml` to your project:

```xml
<Window xmlns="https://schemas.terminal-gui.xaml.com/2023"
        Title="My First XAML App">
  <StackView Orientation="Vertical">
    <Label Text="Hello, XAML!" />
    <Button Text="Click Me" Click="OnButtonClick" />
  </StackView>
  
  <!-- Optional: Key bindings, layout hints, etc. -->
</Window>
```

2) Load and run the XAML window from `Program.cs`:

```csharp
using Terminal.Gui;

// using Terminal.Gui.Xaml; // Namespace for XAML loading utilities

Application.Init();

// Load a Window defined in XAML (API subject to change)
// var window = XamlLoader.Load<Window>("MainWindow.xaml");
// For now, create a window directly if loader API is not available in your version:
var window = new Window("My First XAML App")
{
    X = 0,
    Y = 1, // Leave space for the menu/status bar if any
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

window.Add(new Label("Hello, XAML!") { X = 1, Y = 1 });
window.Add(new Button("Click Me") { X = 1, Y = 3, Clicked = () => MessageBox.Query("Hello", "Button clicked!", "OK") });

Application.Top.Add(window);
Application.Run();
Application.Shutdown();
```

Expected result: A terminal window titled “My First XAML App” that shows a label and a “Click Me” button, which displays a message box when clicked.

Notes:
- When XAML loading APIs are available, prefer `XamlLoader.Load<T>(path)` to instantiate views from XAML.
- Keep UI minimal at first; add layout, binding, and events as you grow.

## Next steps

- Concepts: [Layout](../articles/concepts/layout.md), [Binding](../articles/concepts/binding.md), [Events](../articles/concepts/events.md)
- Guides: [Create a Window](../articles/guides/create-window.md), [Bind Data](../articles/guides/bind-data.md), [Handle Input](../articles/guides/handle-input.md), [Navigation](../articles/guides/navigation.md)
- Examples: [Hello World](../articles/examples/hello-world.md), [Basic Layout](../articles/examples/basic-layout.md)
- Reference: [API Reference](../api/index.md)
