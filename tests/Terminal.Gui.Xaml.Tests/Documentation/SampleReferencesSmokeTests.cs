using System.Text.RegularExpressions;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Documentation;

public class SampleReferencesSmokeTests
{
    private static readonly Regex ProjectRunRegex = new(@"dotnet run --project\s+(?<path>[A-Za-z0-9_./\\-]+\.csproj)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    [Fact]
    public void AllReferencedSampleProjects_ShouldExist()
    {
        var root = FindRepoRoot();
        var examplesDir = Path.Combine(root, "docs", "articles", "examples");
        Assert.True(Directory.Exists(examplesDir), $"Examples directory not found: {examplesDir}");

        var missing = new List<string>();
        foreach (var file in Directory.GetFiles(examplesDir, "*.md", SearchOption.TopDirectoryOnly))
        {
            var text = File.ReadAllText(file);
            foreach (Match m in ProjectRunRegex.Matches(text))
            {
                var rel = m.Groups["path"].Value.Replace("/", Path.DirectorySeparatorChar.ToString());
                var full = Path.GetFullPath(Path.Combine(root, rel));
                if (!File.Exists(full))
                {
                    missing.Add($"{Path.GetFileName(file)} -> {rel}");
                }
            }
        }

        if (missing.Count > 0)
        {
            Assert.Fail("Missing sample project files referenced by examples:\n" + string.Join('\n', missing));
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Terminal.Gui.Xaml.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found");
    }
}
