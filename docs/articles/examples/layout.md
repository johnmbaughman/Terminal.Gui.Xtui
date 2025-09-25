# Layout Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml`

## What You'll Learn

- Difference between absolute and computed layout
- Using `StackView` and `GridView` containers
- Anchoring controls to resize with the terminal
- Mixing XAML layout with code-behind adjustments

## Basic Window Definition

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Layout Demo" Width="80" Height="20">
	<GridView x:Name="RootGrid" Rows="3" Columns="2">
		<Label Grid.Row="0" Grid.Column="0" Text="Header" />
		<StackView Grid.Row="1" Grid.Column="0" Orientation="Vertical">
			<Label Text="Name" />
			<TextField x:Name="NameField" Width="40" />
			<Label Text="Description" />
			<TextView x:Name="DescriptionField" Width="60" Height="5" />
		</StackView>
		<FrameView Grid.RowSpan="2" Grid.Column="1" Title="Preview" x:Name="PreviewPane" />
	</GridView>
</Window>
```

## Code-Behind Adjustment

```csharp
using Terminal.Gui;

public partial class LayoutDemoWindow : Window
{
		public LayoutDemoWindow()
		{
				InitializeComponent();

				// Demonstrate dynamic resizing logic
				PreviewPane.Width = Dim.Fill();
				PreviewPane.Height = Dim.Fill();

				this.LayoutComplete += (_) => PreviewPane.SetNeedsDisplay();
		}
}
```

## Run

```powershell
dotnet run --project samples/LayoutSample/LayoutSample.csproj
```

Status: Example — initial content (enhance with grid spanning and relative dimensions later)
