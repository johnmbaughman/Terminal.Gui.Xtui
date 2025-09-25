# Multi-Window Application Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml`

## What You'll Learn

- Creating multiple top-level windows
- Switching focus programmatically
- Handling window closing events
- Using XAML to define secondary windows

## Primary Window XAML

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Main" Width="60" Height="18">
	<StackView Orientation="Vertical">
		<Button x:Name="OpenSettings" Text="Open Settings" Clicked="OnOpenSettings" />
		<Button x:Name="OpenLogs" Text="Open Logs" Clicked="OnOpenLogs" />
	</StackView>
</Window>
```

## Secondary (Settings) Window XAML

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Settings" Width="50" Height="12">
	<StackView Orientation="Vertical">
		<Label Text="Theme" />
		<ComboBox x:Name="ThemeSelect" />
		<Button Text="Close" Clicked="OnClose" />
	</StackView>
</Window>
```

## Code-Behind: Managing Windows

```csharp
using Terminal.Gui;

public partial class MainWindow : Window
{
		private SettingsWindow? _settings;
		private LogsWindow? _logs;

		public MainWindow()
		{
				InitializeComponent();
		}

		private void OnOpenSettings(object? sender, EventArgs e)
		{
				if (_settings == null)
				{
						_settings = new SettingsWindow();
						_settings.Closed += (_) => _settings = null;
						Application.Top.Add(_settings);
				}
				_settings.FocusFirst();
		}

		private void OnOpenLogs(object? sender, EventArgs e)
		{
				if (_logs == null)
				{
						_logs = new LogsWindow();
						_logs.Closed += (_) => _logs = null;
						Application.Top.Add(_logs);
				}
				_logs.FocusFirst();
		}
}
```

## Run

```powershell
dotnet run --project samples/MultiWindowSample/MultiWindowSample.csproj
```

Status: Example — initial content (enhance with window layout persistence later)
