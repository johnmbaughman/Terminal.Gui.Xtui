using System.IO;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Documentation.Utilities;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class DocumentationUtilitiesTests
{
    [Theory]
    [InlineData("C:/absolute/path/file.txt")]
    [InlineData("./relative/path")]
    public void ResolvePath_ShouldReturnAbsoluteOrAttemptRepoResolve(string input)
    {
        var result = DocumentationUtilities.ResolvePath(input);
        Assert.False(string.IsNullOrWhiteSpace(result));
        if (Path.IsPathRooted(input))
        {
            Assert.Equal(Path.GetFullPath(input), result);
        }
        else
        {
            // For relative paths we expect a non-empty result; can't assert exact repo root in tests
            Assert.NotNull(result);
        }
    }

    [Theory]
    [InlineData("some\\path\\file.txt", "some/path/file.txt")]
    [InlineData("another/path/file.txt", "another/path/file.txt")]
    public void NormalizePath_ShouldUseForwardSlashes(string input, string expected)
    {
        var norm = DocumentationUtilities.NormalizePath(input);
        Assert.Equal(expected, norm);
    }

    [Fact]
    public async Task SafeWriteAllTextAsync_CreatesFileWithContents()
    {
        var temp = Path.Combine(Path.GetTempPath(), "docutils_test_" + System.Guid.NewGuid().ToString("n"));
        try
        {
            var file = Path.Combine(temp, "out.txt");
            var content = "hello world";
            await DocumentationUtilities.SafeWriteAllTextAsync(file, content);
            Assert.True(File.Exists(file));
            var read = await File.ReadAllTextAsync(file);
            Assert.Equal(content, read);
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, true);
            }
        }
    }
}


