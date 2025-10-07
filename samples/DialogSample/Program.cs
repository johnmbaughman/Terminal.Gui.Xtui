using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

Console.WriteLine("DialogSample starting.");
_ = new SimpleBuildIntegrationService();
Application.Init();
var win = new Window("Dialog Sample") { Width = Dim.Fill(), Height = Dim.Fill() };
win.Add(new Button(1,1,"Show Dialog") { Clicked = () => MessageBox.Query("Dialog","Example dialog","OK") });
Application.Run(win);
Application.Shutdown();
