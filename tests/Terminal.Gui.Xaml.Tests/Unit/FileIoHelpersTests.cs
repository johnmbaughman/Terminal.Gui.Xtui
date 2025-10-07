using System.Text;
using Terminal.Gui.Xaml.Documentation.Internals;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class FileIoHelpersTests
{
    [Fact]
    public async Task SafeWriteTextAsync_ShouldCreateDirectoryAndWriteFile()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(root, "a", "b", "c.txt");

        await FileIoHelpers.SafeWriteTextAsync(file, "hello world");

        Assert.True(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.Equal("hello world", content);
    }

    [Fact]
    public async Task SafeReadTextAsync_ShouldReadWhatWasWritten()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "text.txt");
        await File.WriteAllTextAsync(file, "abc123");

        var text = await FileIoHelpers.SafeReadTextAsync(file);
        Assert.Equal("abc123", text);
    }

    [Fact]
    public async Task SafeReplaceTextAsync_ShouldAtomicallyReplace()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "swap.txt");

        await File.WriteAllTextAsync(file, "v1");
        await FileIoHelpers.SafeReplaceTextAsync(file, "v2");

        // Verify replaced content
        var text = await File.ReadAllTextAsync(file);
        Assert.Equal("v2", text);
    }

    [Fact]
    public async Task SafeReplaceTextAsync_ShouldCreateWhenMissing()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(root, "new.txt");

        await FileIoHelpers.SafeReplaceTextAsync(file, "newcontent");

        Assert.True(File.Exists(file));
        var text = await File.ReadAllTextAsync(file);
        Assert.Equal("newcontent", text);
    }

    [Fact]
    public async Task SafeWriteBytesAsync_And_SafeReadBytesAsync_ShouldRoundTrip()
    {
        var root = Path.Combine("temp", "unit", "iohelpers", Guid.NewGuid().ToString("N"));
        var file = Path.Combine(root, "bin.dat");
        var data = Encoding.UTF8.GetBytes("payload");

        await FileIoHelpers.SafeWriteBytesAsync(file, data);
        var bytes = await FileIoHelpers.SafeReadBytesAsync(file);

        Assert.Equal(data, bytes);
    }
}
