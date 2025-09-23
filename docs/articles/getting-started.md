# Getting Started with Terminal.Gui.Xaml

This guide walks you through setting up and using the Terminal.Gui.Xaml framework for declarative UI development.

## Prerequisites

- .NET 8+ SDK
- Terminal.Gui v2+ compatible project
- Basic understanding of XAML concepts

## Installation

Add the Terminal.Gui.Xaml package to your project:

```bash
dotnet add package Terminal.Gui.Xaml
```

## Your First XAML UI

1. Create a XAML file (e.g., `MainWindow.xaml`):

```xml
<Window xmlns="https://schemas.terminal-gui.xaml.com/2023"
        Title="My First XAML App">
    <StackView Orientation="Vertical">
        <Label Text="Hello, XAML!" />
        <Button Text="Click Me" Click="OnButtonClick" />
    </StackView>
</Window>
```

2. The framework will automatically generate a code-behind class.

3. Implement your event handlers:

```csharp
partial void OnButtonClick()
{
    MessageBox.Query("Hello", "Button clicked!", "OK");
}
```

## Next Steps

- Learn about [data binding](data-binding.md)
- Explore [custom controls](custom-controls.md)  
- See [sample applications](samples.md)
- Review the [API reference](../api/index.md)
