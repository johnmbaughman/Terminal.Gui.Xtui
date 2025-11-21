// See https://aka.ms/new-console-template for more information

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Xaml.Mvvm
{
	public partial class MainViewModel : ObservableObject
	{
		[ObservableProperty]
		private string message = "Hello MVVM";

		public IRelayCommand ClickCommand { get; }

		public MainViewModel()
		{
			ClickCommand = new RelayCommand(() => Message = "Clicked at " + DateTime.Now);
		}
	}

	class Program
	{
		static void Main()
		{
			Application.Init();
			var top = Application.Top;

			var win = new Window()
			{
				Title = "XAML MVVM Example",
				X = 0,
				Y = 1,
				Width = Dim.Fill(),
				Height = Dim.Fill()
			};

			var vm = new MainViewModel();

			// var label = new Label() { Text = vm.Message, X = 0, Y = 0 };
			// vm.PropertyChanged += (s, e) =>
			// {
			// 	if (e.PropertyName == nameof(Example.MainViewModel.Message))
			// 		label.Text = vm.Message;
			// };

			var button = new Button() { Text = "Click", X = 0, Y = 2 };
			button.Accepting += (s, e) =>
			{
				vm.ClickCommand.Execute(null);
				e.Handled = true;
			};

			// win.Add(label, button);
			top.Add(win);
			Application.Run();
		}
	}
}
