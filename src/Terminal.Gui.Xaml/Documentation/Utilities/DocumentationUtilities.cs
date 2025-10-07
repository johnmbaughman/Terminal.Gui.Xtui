using System.Diagnostics;

namespace Terminal.Gui.Xaml.Documentation.Utilities;

/// <summary>
/// Helper utilities shared across documentation services (path resolution, safe IO, constants).
/// </summary>
public static class DocumentationUtilities
{
    /// <summary>
    /// Standardized prefix used in documentation-related log messages for CI parsing.
    /// </summary>
    public const string DocsPrefix = "DOCS:";

    /// <summary>
    /// Normalizes a filesystem path to use forward slashes. Useful for consistent
    /// comparisons across platforms and for tests that parse generated file lists.
    /// </summary>
    /// <param name="path">The input path to normalize.</param>
    /// <returns>The path with forward slashes.</returns>
    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string? FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Terminal.Gui.Xaml.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// Resolves a path which may be relative to the repository root. If the provided path
    /// is rooted it is returned as-is. If the path does not exist, the method attempts to
    /// resolve it relative to the repository root (detected by locating the solution file).
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>An absolute path candidate to use for file/directory checks.</returns>
    public static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path ?? string.Empty;
        }
        if (Path.IsPathRooted(path))
        {
            // Return the canonical absolute path for rooted inputs and ensure system
            // directory separators are used (tests may compare returned strings).
            var full = Path.GetFullPath(path);
            return full.Replace('/', Path.DirectorySeparatorChar);
        }

        var direct = Path.GetFullPath(path);
        if (File.Exists(direct) || Directory.Exists(direct))
        {
            return direct.Replace('/', Path.DirectorySeparatorChar);
        }
        var root = FindRepositoryRoot();
        if (!string.IsNullOrEmpty(root))
        {
            var candidate = Path.Combine(root, path);
            return candidate.Replace('/', Path.DirectorySeparatorChar);
        }
        return direct;
    }

    /// <summary>
    /// Writes text to a file asynchronously with a small retry loop to reduce flakes
    /// from transient IO contention. The method ensures the destination directory exists.
    /// </summary>
    /// <param name="path">The destination file path.</param>
    /// <param name="contents">The contents to persist.</param>
    public static async Task SafeWriteAllTextAsync(string path, string contents)
    {
        for (var i = 0; i < 3; i++)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
                var data = System.Text.Encoding.UTF8.GetBytes(contents);
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
                return;
            }
            catch (IOException) when (i < 2)
            {
                await Task.Delay(75).ConfigureAwait(false);
            }
        }
    }
}
