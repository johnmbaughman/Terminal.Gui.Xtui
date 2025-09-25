using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

Console.WriteLine("LayoutSample starting.");
_ = new SimpleBuildIntegrationService();
Application.Init();
var win = new Window("Layout Sample") { Width = Dim.Fill(), Height = Dim.Fill() };
win.Add(new Label("(Layout demo placeholder)") { X = 1, Y = 1 });
Application.Run(win);
Application.Shutdown();
