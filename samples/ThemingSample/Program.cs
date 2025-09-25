using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

Console.WriteLine("ThemingSample starting.");
_ = new SimpleBuildIntegrationService();
Application.Init();
var win = new Window("Theming Sample") { Width = Dim.Fill(), Height = Dim.Fill() };
win.Add(new Label("(Theming demo placeholder)") { X = 1, Y = 1 });
Application.Run(win);
Application.Shutdown();
