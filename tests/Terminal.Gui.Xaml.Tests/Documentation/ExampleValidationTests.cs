using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Terminal.Gui.Xaml.Tests.Documentation;

[TestClass]
public class ExampleValidationTests
{
    private static readonly string ExamplesPath = Path.Combine(
        Path.GetDirectoryName(typeof(ExampleValidationTests).Assembly.Location)!,
        "..", "..", "..", "..", "..", "docs", "articles", "examples"
    );

    [TestInitialize]
    public void TestInitialize()
    {
        // Ensure examples directory exists
        Assert.IsTrue(Directory.Exists(ExamplesPath), 
            $"Examples directory not found: {Path.GetFullPath(ExamplesPath)}");
    }

    [TestMethod]
    public void ExampleFiles_ShouldExist()
    {
        // Arrange & Act
        var exampleFiles = Directory.GetFiles(ExamplesPath, "*.md")
            .Where(f => !Path.GetFileName(f).Equals("index.md", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // Assert
        Assert.IsTrue(exampleFiles.Length > 0, "No example files found");
        
        // Expected examples based on current documentation
        var expectedExamples = new[] { "hello-world.md", "basic-layout.md", "button-click.md" };
        
        foreach (var expected in expectedExamples)
        {
            var expectedPath = Path.Combine(ExamplesPath, expected);
            Assert.IsTrue(File.Exists(expectedPath), $"Expected example file not found: {expected}");
        }
    }

    [TestMethod]
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
            
            Assert.IsTrue(totalBlocks > 0, $"Example file {fileName} should contain at least one code block");
            
            // Basic validation - ensure examples have expected structure
            Assert.IsTrue(content.Contains("## What You'll Learn") || content.Contains("# "), 
                $"Example file {fileName} should have proper section headers");
            
            if (csharpBlocks > 0)
            {
                Assert.IsTrue(content.Contains("using"), 
                    $"C# examples in {fileName} should contain using statements");
            }
        }
    }

    [TestMethod]
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
            Assert.IsTrue(content.Contains("Version") || content.Contains("version"), 
                $"Example file {fileName} should contain version information");
            
            // Check for prerequisites section
            Assert.IsTrue(content.Contains("Prerequisites") || content.Contains("prerequisite"), 
                $"Example file {fileName} should contain prerequisites section");
            
            // Check for .NET 8 requirement
            Assert.IsTrue(content.Contains(".NET 8") || content.Contains("net8.0"), 
                $"Example file {fileName} should specify .NET 8 requirement");
        }
    }

    [TestMethod]
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
            
            Assert.IsTrue(matches.Count > 0, $"Example file {fileName} should contain properly formatted code blocks");
            
            foreach (Match match in matches)
            {
                var language = match.Groups[1].Value;
                var code = match.Groups[2].Value;
                
                Assert.IsFalse(string.IsNullOrWhiteSpace(code), 
                    $"Code block in {fileName} should not be empty");
                
                // Basic language-specific validation
                switch (language.ToLowerInvariant())
                {
                    case "csharp":
                        // Should not contain obvious placeholder text
                        Assert.IsFalse(code.Contains("TODO") || code.Contains("PLACEHOLDER"), 
                            $"C# code in {fileName} should not contain placeholder text");
                        break;
                        
                    case "xml":
                        // Should look like valid XML structure
                        Assert.IsTrue(code.TrimStart().StartsWith("<") && code.TrimEnd().EndsWith(">"), 
                            $"XML code in {fileName} should have proper XML structure");
                        break;
                        
                    case "powershell":
                        // Should contain valid PowerShell patterns
                        Assert.IsTrue(code.Contains("dotnet") || code.Contains("$") || code.Contains("-"), 
                            $"PowerShell code in {fileName} should contain valid PowerShell syntax");
                        break;
                }
            }
        }
    }

    [TestMethod]
    [TestCategory("FutureEnhancement")]
    public void ExampleCode_ShouldCompileSuccessfully()
    {
        // This test is marked as FutureEnhancement and will be skipped until the framework is implemented
        Assert.Inconclusive("Example compilation validation will be implemented once Terminal.Gui.Xaml framework has working code");
        
        // Future implementation will:
        // 1. Extract C# code blocks from examples
        // 2. Create temporary project files
        // 3. Add Terminal.Gui.Xaml package reference
        // 4. Attempt compilation
        // 5. Report any compilation errors
    }

    [TestMethod] 
    [TestCategory("FutureEnhancement")]
    public void ExampleCode_ShouldRunWithoutRuntimeErrors()
    {
        // This test is marked as FutureEnhancement and will be skipped until the framework is implemented
        Assert.Inconclusive("Example runtime validation will be implemented once Terminal.Gui.Xaml framework has working code");
        
        // Future implementation will:
        // 1. Compile examples (from previous test)
        // 2. Execute them in isolated process
        // 3. Check for runtime exceptions
        // 4. Validate expected output patterns
    }
}