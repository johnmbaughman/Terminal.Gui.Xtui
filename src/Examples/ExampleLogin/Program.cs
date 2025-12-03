using System;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;

namespace ExampleLogin;

class Program
{
    static void Main ()
    {
        // Enable configuration
        ConfigurationManager.Enable (ConfigLocations.All);

        // Run the application
        Application.Run<ExampleLogin> ().Dispose ();

        // Shutdown Terminal.Gui for clean exit
        Application.Shutdown ();

        // Display the username after shutdown
        Console.WriteLine ($@"Username: {ExampleLogin.UserName}");
    }
}
