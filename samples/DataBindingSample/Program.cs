using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

// DataBindingSample entry point (Terminal.Gui sample UI).
Console.WriteLine("DataBindingSample starting.");
var svc = new SimpleDocumentationGeneratorService();
Console.WriteLine($"Service type: {svc.GetType().Name}");
Application.Init();
var win = new Window("Data Binding Sample") { Width = Dim.Fill(), Height = Dim.Fill() };
win.Add(new Label("(Data binding demo placeholder)") { X = 1, Y = 1 });
Application.Run(win);
Application.Shutdown ();
