using System;
using System.IO;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

/// <summary>
/// Integration tests for baseline parity validation (T015).
/// Ensures generated code exactly matches the authoritative baseline.
/// Zero-tolerance policy: Any diff is a bug that must be fixed before merge.
/// </summary>
public class BaselineGenerationTests
{
    /// <summary>
    /// Path to the baseline XTUI input file.
    /// </summary>
    private const string BaselineXtuiPath = "Examples/UICatalogXtui/Views/UICatalogTop.xtui";

    /// <summary>
    /// Path to the authoritative baseline output file.
    /// </summary>
    private const string BaselineOutputPath = "refactor/generated-baseline.cs";

    [Fact]
    public void GenerateClass_ForUICatalogTop_MatchesBaselineExactly()
    {
        // Arrange: Load the XTUI markup
        string xtuiPath = Path.Combine(GetRepositoryRoot(), BaselineXtuiPath);
        if (!File.Exists(xtuiPath))
        {
            throw new FileNotFoundException($"Baseline XTUI file not found: {xtuiPath}");
        }

        string xtuiContent = File.ReadAllText(xtuiPath);
        
        // Parse XTUI
        ElementNode root = XtuiLoader.LoadFromString(xtuiContent);
        
        // Get the appropriate generator for Toplevel
        var generatorFactory = new GeneratorFactory();
        Generator generator = generatorFactory.GetGenerator(root.ElementTypeName);
        
        // Act: Generate the code
        string generatedCode = generator.GenerateClass(root, "UICatalogXtui", "UICatalogTop", generatorFactory);

        // Assert: Load the baseline and compare
        string baselinePath = Path.Combine(GetRepositoryRoot(), BaselineOutputPath);
        if (!File.Exists(baselinePath))
        {
            // If baseline doesn't exist, write the generated code for review
            string tempPath = Path.Combine(Path.GetTempPath(), "generated-baseline-candidate.cs");
            File.WriteAllText(tempPath, generatedCode);
            throw new FileNotFoundException(
                $"Baseline file not found: {baselinePath}\n" +
                $"Generated code written to: {tempPath}\n" +
                "Review the generated code and create the baseline file if correct.");
        }

        string baseline = File.ReadAllText(baselinePath);

        // Normalize line endings for cross-platform compatibility
        string normalizedGenerated = NormalizeLineEndings(generatedCode);
        string normalizedBaseline = NormalizeLineEndings(baseline);

        // Assert exact match
        if (normalizedGenerated != normalizedBaseline)
        {
            // Write both files for diff analysis
            string tempDir = Path.Combine(Path.GetTempPath(), "xtui-baseline-diff");
            Directory.CreateDirectory(tempDir);
            
            string generatedPath = Path.Combine(tempDir, "generated.cs");
            string baselineCopyPath = Path.Combine(tempDir, "baseline.cs");
            
            File.WriteAllText(generatedPath, generatedCode);
            File.WriteAllText(baselineCopyPath, baseline);

            Assert.Fail(
                $"Generated code does not match baseline!\n" +
                $"Baseline: {baselinePath}\n" +
                $"Generated output saved to: {generatedPath}\n" +
                $"Baseline copy saved to: {baselineCopyPath}\n" +
                $"Run a diff tool to compare:\n" +
                $"  git diff --no-index \"{baselineCopyPath}\" \"{generatedPath}\"\n" +
                $"\n" +
                $"Zero-tolerance policy: Any diff is a bug, not a rebaseline scenario.\n" +
                $"Fix the generator to match the baseline output.");
        }

        // Success: Generated code matches baseline exactly
        Assert.Equal(normalizedBaseline, normalizedGenerated);
    }

    [Fact]
    public void BaselineFiles_Exist()
    {
        // Verify the baseline files are present in the repository
        string repoRoot = GetRepositoryRoot();
        
        string xtuiPath = Path.Combine(repoRoot, BaselineXtuiPath);
        string baselinePath = Path.Combine(repoRoot, BaselineOutputPath);

        Assert.True(File.Exists(xtuiPath), 
            $"Baseline XTUI input file not found: {xtuiPath}");
        
        Assert.True(File.Exists(baselinePath), 
            $"Baseline output file not found: {baselinePath}");
    }

    [Fact]
    public void Baseline_CanBeParsed()
    {
        // Verify the baseline XTUI file can be parsed without errors
        string xtuiPath = Path.Combine(GetRepositoryRoot(), BaselineXtuiPath);
        string xtuiContent = File.ReadAllText(xtuiPath);

        // This should not throw
        ElementNode root = XtuiLoader.LoadFromString(xtuiContent);

        Assert.NotNull(root);
        Assert.Equal("Terminal.Gui.Views.Toplevel", root.ElementTypeName);
    }

    /// <summary>
    /// Normalizes line endings to \n for consistent comparison.
    /// </summary>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    /// <summary>
    /// Gets the repository root directory by walking up from the test assembly location.
    /// Assumes the tests are running from: repo/src/Terminal.Gui.Xtui.Tests/bin/Debug/net8.0/
    /// </summary>
    private static string GetRepositoryRoot()
    {
        string assemblyDir = Path.GetDirectoryName(typeof(BaselineGenerationTests).Assembly.Location)
            ?? throw new InvalidOperationException("Could not determine assembly location");

        // Walk up to find src/ directory
        DirectoryInfo? current = new DirectoryInfo(assemblyDir);
        while (current != null)
        {
            // Check if we're at the src/ level
            if (current.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find repository root from assembly location: {assemblyDir}");
    }

    /// <summary>
    /// Generates baseline code for CI validation.
    /// This test is designed to be called from CI to regenerate the baseline output.
    /// Set environment variable GENERATE_BASELINE=true to enable output generation.
    /// </summary>
    [Fact]
    public void GenerateBaseline_ForCI()
    {
        // Only run if explicitly requested via environment variable
        var shouldGenerate = Environment.GetEnvironmentVariable("GENERATE_BASELINE");
        if (shouldGenerate != "true")
        {
            return; // Skip silently
        }

        var srcRoot = GetRepositoryRoot();
        var repoRoot = Path.GetDirectoryName(srcRoot) ?? throw new InvalidOperationException("Cannot determine repo root");
        
        var xtuiPath = Path.Combine(srcRoot, "Examples", "UICatalogXtui", "Views", "UICatalogTop.xtui");
        Assert.True(File.Exists(xtuiPath), $"XTUI file not found: {xtuiPath}");

        var xtuiContent = File.ReadAllText(xtuiPath);
        var rootNode = XtuiLoader.LoadFromString(xtuiContent);
        Assert.NotNull(rootNode);

        var generator = new GeneratorFactory().GetGenerator(rootNode.ElementTypeName);
        Assert.NotNull(generator);

        var generated = generator.GenerateClass(rootNode, "UICatalogXtui", "UICatalogTop", new GeneratorFactory());

        // Write to artifacts directory (repo root level)
        var artifactsDir = Path.Combine(repoRoot, "artifacts", "generated");
        Directory.CreateDirectory(artifactsDir);
        var outputPath = Path.Combine(artifactsDir, "generated-baseline.cs");
        File.WriteAllText(outputPath, generated);

        Console.WriteLine($"Generated baseline written to: {outputPath}");
    }
}

