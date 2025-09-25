using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

Console.WriteLine("FormValidationSample starting.");
_ = new SimpleDocumentationGeneratorService();
Application.Init();
var win = new Window("Form Validation Sample") { Width = Dim.Fill(), Height = Dim.Fill() };
win.Add(new Label("(Form validation demo placeholder)") { X = 1, Y = 1 });
Application.Run(win);
Application.Shutdown();
