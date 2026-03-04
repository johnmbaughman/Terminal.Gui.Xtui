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
        Application.Init ();
        var top = new Runnable();
        var vm = new MainViewModel ();
        top.Add (new MyWindow (vm));
        Application.Run (top);
        top.Dispose ();
        Application.Shutdown ();
    }
}
