# Theming Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml`

## What You'll Learn

- Applying a global color scheme
- Dynamically switching themes at runtime
- Styling controls in XAML
- Isolating theme resources

## Theme Resource XAML

```xml
<Resources xmlns="http://schemas.terminal-gui.org/xaml">
  <ColorScheme x:Key="DarkScheme"
               NormalForeground="Gray"
               NormalBackground="Black"
               FocusForeground="Yellow"
               FocusBackground="DarkGray"
               HotNormalForeground="BrightBlue"
               HotFocusForeground="BrightYellow" />
</Resources>
```

## Applying Theme in Window XAML

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Theming" Width="60" Height="15">
  <StackView Orientation="Vertical">
    <Label Text="Choose Theme:" />
    <ComboBox x:Name="ThemeSelector" />
    <Button Text="Apply" Clicked="OnApplyTheme" />
  </StackView>
</Window>
```

## Code-Behind: Theme Switching

```csharp
using Terminal.Gui;

public partial class ThemingWindow : Window
{
    private readonly Dictionary<string, ColorScheme> _schemes;

    public ThemingWindow()
    {
        InitializeComponent();
        _schemes = new Dictionary<string, ColorScheme>
        {
            ["Dark"] = new ColorScheme { Normal = new Attribute(Color.Gray, Color.Black), Focus = new Attribute(Color.Yellow, Color.DarkGray) },
            ["Light"] = new ColorScheme { Normal = new Attribute(Color.Black, Color.Gray), Focus = new Attribute(Color.Black, Color.BrightYellow) }
        };
        ThemeSelector.SetSource(_schemes.Keys.ToList());
    }

    private void OnApplyTheme(object? sender, EventArgs e)
    {
        if (ThemeSelector.SelectedItem < 0) return;
        var key = ThemeSelector.Text.ToString();
        if (_schemes.TryGetValue(key, out var scheme))
        {
            // Apply to this window and descendants
            this.ColorScheme = scheme;
            SetNeedsDisplay();
        }
    }
}
```

## Run

```powershell
dotnet run --project samples/ThemingSample/ThemingSample.csproj
```

Status: Example — initial content (enhance with resource dictionary merging later)
