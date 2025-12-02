using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace Xtui.Mvvm;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "Hello MVVM";

    public IRelayCommand ClickCommand { get; }

    public MainViewModel ()
    {
        ClickCommand = new RelayCommand (() => Message = "Clicked at " + DateTime.Now);
    }
}

class Program
{
    static void Main ()
    {
        var vm = new MainViewModel ();
        var app = Application.Create ();
        app.Init ();
        var top = new Toplevel ();
        top.Add (new MyWindow (vm));
        app.Run (top);
        top.Dispose ();
        app.Shutdown ();
    }
}
