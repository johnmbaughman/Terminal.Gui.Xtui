using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Documentation;

/// <summary>
/// Contains tests that validate the structure and syntax of documentation code examples for the Terminal.Gui.Xaml framework.
/// </summary>
public class ExampleValidationTests
{
    /// <summary>
    /// Gets the path to the documentation examples directory.
    /// </summary>
    private static readonly string ExamplesPath = Path.Combine(
        Path.GetDirectoryName(typeof(ExampleValidationTests).Assembly.Location)!,
        "..", "..", "..", "..", "..", "docs", "articles", "examples"
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleValidationTests"/> class and verifies the examples directory exists.
    /// </summary>
    public ExampleValidationTests()
    {
        // Ensure examples directory exists
        Assert.True(Directory.Exists(ExamplesPath), 
            $"Examples directory not found: {Path.GetFullPath(ExamplesPath)}");
    }

    /// <summary>
    /// Validates that all expected example files exist in the documentation.
    /// </summary>
    [Fact]
    public void ExampleFiles_ShouldExist()
    {
        // Arrange & Act
        var exampleFiles = Directory.GetFiles(ExamplesPath, "*.md")
            .Where(f => !Path.GetFileName(f).Equals("index.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // Assert
        Assert.True(exampleFiles.Length > 0, "No example files found");
        
        // Expected examples based on current documentation
        var expectedExamples = new[] { "hello-world.md", "basic-layout.md", "button-click.md" };
        
        foreach (var expected in expectedExamples)
        {
            var expectedPath = Path.Combine(ExamplesPath, expected);
            Assert.True(File.Exists(expectedPath), $"Expected example file not found: {expected}");
        }
    }

    /// <summary>
    /// Validates that each example file contains at least one code block and has proper section headers.
    /// </summary>
    [Fact]
    public void ExampleFiles_ShouldContainCodeBlocks()
    {
        // Arrange
        var exampleFiles = Directory.GetFiles(ExamplesPath, "*.md")
            .Where(f => !Path.GetFileName(f).Equals("index.md", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        foreach (var file in exampleFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            
            // Check for various types of code blocks
            var csharpBlocks = Regex.Matches(content, @"```csharp.*?```", RegexOptions.Singleline).Count;
            var powershellBlocks = Regex.Matches(content, @"```powershell.*?```", RegexOptions.Singleline).Count;
            var xmlBlocks = Regex.Matches(content, @"```xml.*?```", RegexOptions.Singleline).Count;
            
            var totalBlocks = csharpBlocks + powershellBlocks + xmlBlocks;
            
            Assert.True(totalBlocks > 0, $"Example file {fileName} should contain at least one code block");
            
            // Basic validation - ensure examples have expected structure
            Assert.True(content.Contains("## What You'll Learn") || content.Contains("# "), 
                $"Example file {fileName} should have proper section headers");
            
            if (csharpBlocks > 0)
            {
                Assert.True(content.Contains("using"), 
                    $"C# examples in {fileName} should contain using statements");
            }
        }
    }

    /// <summary>
    /// Validates that each example file contains required metadata and prerequisites sections.
    /// </summary>
    [Fact]
    public void ExampleFiles_ShouldHaveConsistentStructure()
    {
        // Arrange
        var exampleFiles = Directory.GetFiles(ExamplesPath, "*.md")
            .Where(f => !Path.GetFileName(f).Equals("index.md", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        foreach (var file in exampleFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            
            // Check for version metadata
            Assert.True(content.Contains("Version") || content.Contains("version"), 
                $"Example file {fileName} should contain version information");
            
            // Check for prerequisites section
            Assert.True(content.Contains("Prerequisites") || content.Contains("prerequisite"), 
                $"Example file {fileName} should contain prerequisites section");
            
            // Check for .NET 8 requirement
            Assert.True(content.Contains(".NET 8") || content.Contains("net8.0"), 
                $"Example file {fileName} should specify .NET 8 requirement");
        }
    }

    /// <summary>
    /// Validates that all code blocks in example files are well-formed and match expected language patterns.
    /// </summary>
    [Fact]
    public void ExampleCodeBlocks_ShouldBeWellFormed()
    {
        // Arrange
        var exampleFiles = Directory.GetFiles(ExamplesPath, "*.md")
            .Where(f => !Path.GetFileName(f).Equals("index.md", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        foreach (var file in exampleFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            
            // Find all code blocks
            var codeBlockPattern = @"```(\w+)\r?\n(.*?)\r?\n```";
            var matches = Regex.Matches(content, codeBlockPattern, RegexOptions.Singleline);
            
            Assert.True(matches.Count > 0, $"Example file {fileName} should contain properly formatted code blocks");
            
            foreach (Match match in matches)
            {
                var language = match.Groups[1].Value;
                var code = match.Groups[2].Value;
                
                Assert.False(string.IsNullOrWhiteSpace(code), 
                    $"Code block in {fileName} should not be empty");
                
                // Basic language-specific validation
                switch (language.ToLowerInvariant())
                {
                    case "csharp":
                        // Should not contain obvious placeholder text
                        Assert.False(code.Contains("TODO") || code.Contains("PLACEHOLDER"), 
                            $"C# code in {fileName} should not contain placeholder text");
                        break;
                        
                    case "xml":
                        // Should look like valid XML structure
                        Assert.True(code.TrimStart().StartsWith("<") && code.TrimEnd().EndsWith(">"), 
                            $"XML code in {fileName} should have proper XML structure");
                        break;
                        
                    case "powershell":
                        // Should contain valid PowerShell patterns
                        Assert.True(code.Contains("dotnet") || code.Contains("$") || code.Contains("-"), 
                            $"PowerShell code in {fileName} should contain valid PowerShell syntax");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Validates that example code compiles successfully (future enhancement).
    /// </summary>
    [Fact(Skip = "Future enhancement - requires actual compilation")]
    public void ExampleCode_ShouldCompileSuccessfully()
    {
        // This test is marked as FutureEnhancement and will be skipped until the framework is implemented
        // Assert.Inconclusive("Example compilation validation will be implemented once Terminal.Gui.Xaml framework has working code");
        
        // Future implementation will:
        // 1. Extract C# code blocks from examples
        // 2. Create temporary project files
        // 3. Add Terminal.Gui.Xaml package reference
        // 4. Attempt compilation
        // 5. Report any compilation errors
    }

    /// <summary>
    /// Validates that example code runs without runtime errors (future enhancement).
    /// </summary>
    [Fact(Skip = "Future enhancement - requires actual runtime execution")]
    public void ExampleCode_ShouldRunWithoutRuntimeErrors()
    {
        // This test is marked as FutureEnhancement and will be skipped until the framework is implemented
        // Assert.Inconclusive("Example runtime validation will be implemented once Terminal.Gui.Xaml framework has working code");
        
        // Future implementation will:
        // 1. Compile examples (from previous test)
        // 2. Execute them in isolated process
        // 3. Check for runtime exceptions
        // 4. Validate expected output patterns
    }
}