// See https://aka.ms/new-console-template for more information

using System;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Terminal.Gui.Xaml.Generated;

class Program
{
	static void Main()
	{
		Application.Init();
		var top = Application.Top;

		var win = new Window()
		{
			Title = "XAML Example",
			X = 0,
			Y = 1,
			Width = Dim.Fill(),
			Height = Dim.Fill()
		};

		// Use generated view from Terminal.Gui.Xaml project (generated at build time)
		var view = Sample_Generated.Create();
		win.Add(view);

		top.Add(win);
		Application.Run();
	}
}
