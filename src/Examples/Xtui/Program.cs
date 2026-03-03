using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace Xtui;

class Program
{
    static void Main ()
    {
        var app = Application.Create ();
        app.Init ();
        var top = new Runnable ();
        top.Add (new MyWindow ());
        app.Run (top);
        top.Dispose ();
    }
}