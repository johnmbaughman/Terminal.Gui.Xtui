using System.Diagnostics;
using System.Text;
using Terminal.Gui.Xaml.Documentation.Internals;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class FileIoHelpersContentionTests
{
    [Fact]
    public async Task SafeWriteTextAsync_ShouldRetry_WhenFileLockedForRead()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "contention-write.txt");
        await File.WriteAllTextAsync(file, "initial");

        // Lock the file for read with no write sharing, so writes fail while this is open
        await using var readLock = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

        var sw = Stopwatch.StartNew();
        var writerTask = FileIoHelpers.SafeWriteTextAsync(file, "updated");

        // Hold the lock long enough to force at least one retry but not long enough to exhaust all retries
        await Task.Delay(120);
        await readLock.DisposeAsync();

        await writerTask; // should complete successfully after lock released
        sw.Stop();

        var content = await File.ReadAllTextAsync(file);
        Assert.Equal("updated", content);
        Assert.True(sw.ElapsedMilliseconds >= 75, "Expected at least one retry delay to occur");
    }

    [Fact]
    public async Task SafeReplaceTextAsync_ShouldRetryAndReplace_WhenFileLockedForRead()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "contention-replace.txt");
        await File.WriteAllTextAsync(file, "v1");

        // Lock file with read-only access and no write share
        await using var readLock = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);

        var sw = Stopwatch.StartNew();
        var replacerTask = FileIoHelpers.SafeReplaceTextAsync(file, "v2");

        await Task.Delay(120);
        await readLock.DisposeAsync();

        await replacerTask;
        sw.Stop();

        var content = await File.ReadAllTextAsync(file);
        Assert.Equal("v2", content);
        Assert.True(sw.ElapsedMilliseconds >= 75);
    }

    [Fact]
    public async Task SafeReadTextAsync_ShouldRetry_WhenFileLockedForWrite()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "contention-read.txt");
        await File.WriteAllTextAsync(file, "payload");

        // Open a write lock with no sharing to block reads
        await using var writeLock = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var sw = Stopwatch.StartNew();
        var readerTask = FileIoHelpers.SafeReadTextAsync(file);

        await Task.Delay(120);
        await writeLock.DisposeAsync();

        var text = await readerTask;
        sw.Stop();

        Assert.Equal("payload", text);
        Assert.True(sw.ElapsedMilliseconds >= 75);
    }
}
