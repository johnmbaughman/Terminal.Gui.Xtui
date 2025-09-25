using System.Runtime.Loader;

namespace Terminal.Gui.Xaml.Tests.Infrastructure;

/// <summary>
/// Assembly-level cleanup via process exit/unloading events.
/// Ensures transient directories created during tests are removed at the end of the run.
/// </summary>
public static class GlobalTestCleanup
{
    static GlobalTestCleanup()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, __) => Cleanup();
        try
        {
            AssemblyLoadContext.Default.Unloading += _ => Cleanup();
        }
        catch
        {
            // Best effort; some environments may not support this
        }
    }

    private static void Cleanup()
    {
        // Fire-and-forget best-effort cleanup
        try { DeleteDirectorySafe(Path.Combine("temp", "unit", "iohelpers")).GetAwaiter().GetResult(); } catch { }
        try { DeleteDirectorySafe(Path.Combine("temp", "docs", "_site")).GetAwaiter().GetResult(); } catch { }
    }

    private static async Task DeleteDirectorySafe(string path, int retries = 5, int delayMs = 100)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }
        if (!Directory.Exists(path))
        {
            return;
        }

        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }

        // Final attempt, let exceptions bubble if still failing for visibility
        Directory.Delete(path, recursive: true);
    }
}
