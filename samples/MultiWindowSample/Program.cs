using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

Console.WriteLine("MultiWindowSample starting.");
_ = new SimpleDocumentationGeneratorService();
Application.Init();
var main = new Window("Main") { Width = Dim.Fill(), Height = Dim.Fill() };
var open = new Button(1,1,"Open Secondary");
open.Clicked += () => {
	var secondary = new Window("Secondary") { X = Pos.Percent(10), Y = Pos.Percent(10), Width = 40, Height = 10 };
	secondary.Add(new Button(1,1,"Close") { Clicked = () => Application.RequestStop(secondary) });
	Application.Top.Add(secondary);
	secondary.SetNeedsDisplay();
};
main.Add(open);
Application.Run(main);
Application.Shutdown();
