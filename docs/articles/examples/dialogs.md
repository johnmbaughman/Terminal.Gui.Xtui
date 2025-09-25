# Dialogs Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml` package

## What You'll Learn

- Creating modal and modeless dialogs
- Passing results (OK / Cancel semantics)
- Using data binding for dialog state

## Basic Modal Dialog

```csharp
using Terminal.Gui;
using Terminal.Gui.Xaml;

public class DialogDemo
{
		public static void ShowAbout()
		{
				var ok = new Button("OK");
				var dialog = new Dialog("About", 60, 10, ok);
				dialog.Add(new Label("Terminal.Gui.Xaml Dialog Sample") { X = 1, Y = 1 });
				ok.Clicked += () => Application.RequestStop();
				Application.Run(dialog);
		}
}
```

## XAML-Defined Dialog

```xml
<Dialog xmlns="http://schemas.terminal-gui.org/xaml" Title="Confirm" Width="50" Height="12">
	<StackView Orientation="Vertical">
		<Label Text="Proceed with action?" />
		<StackView Orientation="Horizontal">
			<Button x:Name="OkButton" Text="OK" Clicked="OnOk" />
			<Button x:Name="CancelButton" Text="Cancel" Clicked="OnCancel" />
		</StackView>
	</StackView>
</Dialog>
```

## Launching

```powershell
dotnet run --project samples/DialogSample/DialogSample.csproj
```

Status: Example — initial content (expand with advanced scenarios later)
