# Resources & Theming

Version: 1.0.0-beta1  
Audience: Intermediate  
Prerequisites: Basic understanding of XAML markup, Terminal.Gui color attributes

## Goals

- Centralize reusable visual resources (color schemes, style presets, strings)
- Support runtime theme switching without reloading the application
- Keep XAML declarative while enabling dynamic overrides

## Resource Strategies

| Strategy | Use When | Pros | Cons |
|----------|----------|------|------|
| Inline attributes | One-off tweak | Fast | Repetition |
| Resource dictionary (single file) | Small app | Organized | Can grow noisy |
| Layered dictionaries | Large / modular app | Isolation, override order | Slightly more plumbing |
| Dynamic provider service | Theme switching | Central control | Some indirection |

## Example: Resource Dictionary

```xml
<Resources xmlns="http://schemas.terminal-gui.org/xaml">
  <ColorScheme x:Key="LightScheme"
               NormalForeground="Black"
               NormalBackground="Gray"
               FocusForeground="Black"
               FocusBackground="BrightYellow" />
  <ColorScheme x:Key="DarkScheme"
               NormalForeground="Gray"
               NormalBackground="Black"
               FocusForeground="Yellow"
               FocusBackground="DarkGray" />
</Resources>
```

## Applying a Resource by Key

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Res Demo" Width="50" Height="12" ColorScheme="{StaticResource DarkScheme}">
  <Label Text="Themed Window" />
</Window>
```

## Runtime Theme Switching Pattern

```csharp
public interface IThemeService
{
    event Action? ThemeChanged;
    ColorScheme Current { get; }
    void Use(string themeName);
}

public class ThemeService : IThemeService
{
    private readonly Dictionary<string, ColorScheme> _themes;
    public event Action? ThemeChanged;
    public ColorScheme Current { get; private set; }

    public ThemeService()
    {
        _themes = new()
        {
            ["light"] = new ColorScheme { Normal = new Attribute(Color.Black, Color.Gray) },
            ["dark"] = new ColorScheme { Normal = new Attribute(Color.Gray, Color.Black) }
        };
        Current = _themes["dark"]; // default
    }

    public void Use(string themeName)
    {
        if (_themes.TryGetValue(themeName, out var scheme))
        {
            Current = scheme;
            ThemeChanged?.Invoke();
        }
    }
}
```

## Hooking Theme Changes in a Window

```csharp
public partial class ThemedWindow : Window
{
    private readonly IThemeService _themeService;

    public ThemedWindow(IThemeService themeService)
    {
        _themeService = themeService;
        InitializeComponent();
        ApplyTheme();
        _themeService.ThemeChanged += () => { ApplyTheme(); SetNeedsDisplay(); };
    }

    private void ApplyTheme() => ColorScheme = _themeService.Current;
}
```

## Best Practices

- Keep color names semantic (e.g., PrimaryForeground) if you add abstraction.
- Avoid mutating `ColorScheme` instances shared across windows; clone if adjusting.
- Batch redraws after theme change by deferring heavy operations until `ThemeChanged` completes.
- Provide a fall-back scheme to avoid null scheme states during transitions.

## Diagnostics

| Symptom | Cause | Fix |
|---------|-------|-----|
| Theme not applied | Key typo in XAML | Verify resource key casing |
| Flicker on switch | Multiple redraws | Debounce or batch `SetNeedsDisplay` |
| Colors washed out | Terminal palette mismatch | Restrict to standard 16-color attribute set |

## Next Steps

- Add high contrast scheme
- Externalize resource dictionary to a shared `*.xaml` file
- Persist last chosen theme in user settings

Status: Guide — initial content (expand with style inheritance & symbol resources)
