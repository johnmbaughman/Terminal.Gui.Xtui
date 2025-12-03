using System.Linq;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace ExampleLogin;

public partial class ExampleLogin : Window
{
    public static string? UserName { get; set; }

    public ExampleLogin ()
    {
        Title = $"Example App ({Application.QuitKey} to quit)";
        
        InitializeComponent ();

        // Setup ListView data
        if (_listView != null)
        {
            _listView.SetSource (["One", "Two", "Three", "Four"]);
        }

        // Attach login button event handler
        if (_btnLogin != null)
        {
            _btnLogin.Accepting += OnLoginButtonAccepting;
        }
    }

    public override void EndInit ()
    {
        base.EndInit ();
        // Set the theme to "Anders" if it exists, otherwise use "Default"
        ThemeManager.Theme = ThemeManager.GetThemeNames ().FirstOrDefault (x => x == "Anders") ?? "Default";
    }

    private void OnLoginButtonAccepting (object? sender, CommandEventArgs e)
    {
        e.Handled = true;

        if (_userNameText != null && _passwordText != null)
        {
            if (_userNameText.Text == "admin" && _passwordText.Text == "password")
            {
                MessageBox.Query ("Logging In", "Login Successful", "Ok");
                UserName = _userNameText.Text;
                Application.RequestStop ();
            }
            else
            {
                MessageBox.ErrorQuery ("Logging In", "Incorrect username or password", "Ok");
            }
        }
    }
}
