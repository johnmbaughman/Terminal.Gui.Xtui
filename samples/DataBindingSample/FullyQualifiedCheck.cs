// Temporary diagnostic file to test fully qualified reference
public static class FullyQualifiedCheck
{
    public static void Touch()
    {
        Terminal.Gui.Application.MainLoopIteration += (_, _) => { };
    }
}
