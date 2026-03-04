using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.Views;

namespace Xtui;

public partial class MyWindow
{
    public MyWindow ()
    {
        InitializeComponent ();
        
        // Attach event handler to the generated field
        if (_closeButton != null)
        {
            _closeButton.Accepting += OnCloseButtonAccepting;
        }
    }

    private void OnCloseButtonAccepting (object? sender, CommandEventArgs e)
    {
        e.Handled = true;
        // Find the Runnable in the SuperView chain and request stop
        var view = this.SuperView;
        while (view != null)
        {
            if (view is Runnable runnable)
            {
                Application.RequestStop (runnable);
                return;
            }
            view = view.SuperView;
        }
    }
}