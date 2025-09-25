using System.Text;

namespace Terminal.Gui.Xaml.Documentation.Internals;

/// <summary>
/// Internal file I/O helpers for resilient writes used in documentation services.
/// </summary>
internal static class FileIoHelpers
{
    /// <summary>
    /// Writes text to a file with retries and shared read/write to minimize contention.
    /// Ensures the directory for the file exists.
    /// </summary>
    public static async Task SafeWriteTextAsync(
        string path,
        string contents,
        Encoding? encoding = null,
        int retries = 3,
        int delayMs = 75,
        FileShare share = FileShare.ReadWrite)
    {
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                EnsureDirectoryForFile(path);
                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, share, 4096, useAsync: true);
                var enc = encoding ?? Encoding.UTF8;
                var data = enc.GetBytes(contents);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
                return;
            }
            catch (IOException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Writes bytes to a file with retries and shared read/write to minimize contention.
    /// Ensures the directory for the file exists.
    /// </summary>
    public static async Task SafeWriteBytesAsync(
        string path,
        ReadOnlyMemory<byte> data,
        int retries = 3,
        int delayMs = 75,
        FileShare share = FileShare.ReadWrite)
    {
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                EnsureDirectoryForFile(path);
                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, share, 4096, useAsync: true);
                await stream.WriteAsync(data);
                await stream.FlushAsync();
                return;
            }
            catch (IOException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Reads all text from a file with retries and shared read/write to minimize contention.
    /// </summary>
    public static async Task<string> SafeReadTextAsync(
        string path,
        Encoding? encoding = null,
        int retries = 3,
        int delayMs = 75,
        FileShare share = FileShare.ReadWrite)
    {
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, share, 4096, useAsync: true);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                var enc = encoding ?? Encoding.UTF8;
                return enc.GetString(ms.ToArray());
            }
            catch (IOException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
        // Last attempt without catch to bubble the exception if it still fails
        await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, share, 4096, useAsync: true))
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            var enc = encoding ?? Encoding.UTF8;
            return enc.GetString(ms.ToArray());
        }
    }

    /// <summary>
    /// Reads all bytes from a file with retries and shared read/write to minimize contention.
    /// </summary>
    public static async Task<byte[]> SafeReadBytesAsync(
        string path,
        int retries = 3,
        int delayMs = 75,
        FileShare share = FileShare.ReadWrite)
    {
        for (var attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, share, 4096, useAsync: true);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            }
            catch (IOException) when (attempt < retries - 1)
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
        await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, share, 4096, useAsync: true))
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// Atomically replaces the target file with the specified text content, using a temporary file and File.Replace when possible.
    /// </summary>
    public static async Task SafeReplaceTextAsync(
        string path,
        string contents,
        Encoding? encoding = null,
        string? backupPath = null,
        int retries = 3,
        int delayMs = 75)
    {
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir))
        {
            dir = ".";
        }
        EnsureDirectoryForFile(path);
        var temp = Path.Combine(dir!, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            // Write new content to a temp file in the same directory/volume for atomic replace
            await SafeWriteTextAsync(temp, contents, encoding, retries, delayMs).ConfigureAwait(false);

            for (var attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Replace(temp, path, backupPath, ignoreMetadataErrors: true);
                    }
                    else
                    {
                        // No existing file: a simple move is sufficient
                        File.Move(temp, path);
                    }
                    temp = string.Empty; // mark consumed so we don't try to delete below
                    break;
                }
                catch (IOException) when (attempt < retries - 1)
                {
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(temp) && File.Exists(temp))
            {
                try { File.Delete(temp); } catch { /* best-effort cleanup */ }
            }
        }
    }

    /// <summary>
    /// Atomically replaces the target file with the specified bytes, using a temporary file and File.Replace when possible.
    /// </summary>
    public static async Task SafeReplaceBytesAsync(
        string path,
        ReadOnlyMemory<byte> data,
        string? backupPath = null,
        int retries = 3,
        int delayMs = 75)
    {
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir))
        {
            dir = ".";
        }
        EnsureDirectoryForFile(path);
        var temp = Path.Combine(dir!, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await SafeWriteBytesAsync(temp, data, retries, delayMs).ConfigureAwait(false);

            for (var attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Replace(temp, path, backupPath, ignoreMetadataErrors: true);
                    }
                    else
                    {
                        File.Move(temp, path);
                    }
                    temp = string.Empty;
                    break;
                }
                catch (IOException) when (attempt < retries - 1)
                {
                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            if (!string.IsNullOrEmpty(temp) && File.Exists(temp))
            {
                try { File.Delete(temp); } catch { }
            }
        }
    }

    /// <summary>
    /// Ensures the directory for the given file path exists.
    /// </summary>
    public static void EnsureDirectoryForFile(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
