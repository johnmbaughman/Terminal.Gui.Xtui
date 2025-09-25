# Form Validation Example

Version: 1.0.0-beta1  
Prerequisites: .NET 8 SDK, Terminal.Gui v2+, `Terminal.Gui.Xaml`

## What You'll Learn

- Binding input fields to a view model
- Performing synchronous validation
- Displaying inline error messages
- Preventing submission until fields are valid

## View Model with Validation

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class RegistrationViewModel : INotifyPropertyChanged, IDataErrorInfo
{
	private string _email = string.Empty;
	private string _password = string.Empty;

	public string Email
	{
		get => _email; 
		set { _email = value; OnPropertyChanged(); }
	}

	public string Password
	{
		get => _password; 
		set { _password = value; OnPropertyChanged(); }
	}

	public string Error => string.Empty;

	public string this[string columnName] => columnName switch
	{
		nameof(Email) => string.IsNullOrWhiteSpace(Email) || !Email.Contains('@') ? "Email is required and must contain '@'" : string.Empty,
		nameof(Password) => Password.Length < 8 ? "Password must be at least 8 characters" : string.Empty,
		_ => string.Empty
	};

	public event PropertyChangedEventHandler? PropertyChanged;
	private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

## XAML Form Layout

```xml
<Window xmlns="http://schemas.terminal-gui.org/xaml" Title="Register" Width="70" Height="18">
  <StackView Orientation="Vertical">
	<Label Text="Email" />
	<TextField x:Name="EmailField" Text="{Binding Email}" />
	<Label x:Name="EmailError" Text="{Binding [Email]}" />
	<Label Text="Password" />
	<TextField x:Name="PasswordField" Secret=true Text="{Binding Password}" />
	<Label x:Name="PasswordError" Text="{Binding [Password]}" />
	<Button x:Name="SubmitButton" Text="Submit" Clicked="OnSubmit" />
  </StackView>
</Window>
```

## Handling Submit

```csharp
using Terminal.Gui;

public partial class RegisterWindow : Window
{
	private readonly RegistrationViewModel _vm = new();

	public RegisterWindow()
	{
		InitializeComponent();
		DataContext = _vm;
	}

	private void OnSubmit(object? sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(_vm[nameof(_vm.Email)]) || !string.IsNullOrEmpty(_vm[nameof(_vm.Password)]))
		{
			MessageBox.ErrorQuery("Invalid", "Fix validation errors first.", "OK");
			return;
		}
		MessageBox.Query("Success", "Registration complete!", "OK");
	}
}
```

## Run

```powershell
dotnet run --project samples/FormValidationSample/FormValidationSample.csproj
```

Status: Example — initial content (extend with async validation and summary panel later)
