using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace Xtui;

class Program
{
    static void Main ()
    {
        Application.Init();
        var top = new Runnable();
        top.Add(new MyWindow());
        Application.Run(top);
        top.Dispose();
        Application.Shutdown();
    }
}