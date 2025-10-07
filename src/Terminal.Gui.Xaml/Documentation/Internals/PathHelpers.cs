using System.IO;

namespace Terminal.Gui.Xaml.Documentation.Internals;

/// <summary>
/// Shared helpers for repository-relative path resolution and normalization.
/// </summary>
internal static class PathHelpers
{
    /// <summary>
    /// Attempts to locate the repository root by walking up from the starting directory
    /// and looking for the solution file 'Terminal.Gui.Xaml.sln'.
    /// </summary>
    /// <param name="startingDirectory">Optional starting directory; defaults to current directory.</param>
    /// <returns>Absolute path to the repository root if found; otherwise null.</returns>
    public static string? FindRepositoryRoot(string? startingDirectory = null)
    {
        var start = string.IsNullOrWhiteSpace(startingDirectory)
            ? new DirectoryInfo(Directory.GetCurrentDirectory())
            : new DirectoryInfo(Path.GetFullPath(startingDirectory!));

        var dir = start;
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
    /// Resolves a path to an absolute path, allowing repo-relative resolution when the path does not exist relative to CWD.
    /// </summary>
    public static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }
        if (Path.IsPathRooted(path))
        {
            return path;
        }
        var full = Path.GetFullPath(path);
        if (File.Exists(full) || Directory.Exists(full))
        {
            return full;
        }
        var root = FindRepositoryRoot();
        if (!string.IsNullOrEmpty(root))
        {
            return Path.Combine(root, path);
        }
        return full;
    }

    /// <summary>
    /// Normalizes path separators to forward slashes for stable comparisons and output.
    /// </summary>
    public static string NormalizeToForwardSlashes(string path) => path.Replace('\\', '/');
}
