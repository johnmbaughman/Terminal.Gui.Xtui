# Quickstart Guide: DocFX Documentation Generation

## Overview
This quickstart guide demonstrates how to set up and use DocFX documentation generation with the Terminal.Gui.Xaml framework. Follow these steps to generate comprehensive API documentation from your code.

## Prerequisites
- .NET 8+ SDK installed
- Terminal.Gui.Xaml framework project
- XML documentation comments in source code

## Step 1: Install Documentation Package

Add the documentation generation package to your project:

```xml
<PackageReference Include="Terminal.Gui.Xaml.Documentation" Version="1.0.0" />
```

Or using the .NET CLI:
```bash
dotnet add package Terminal.Gui.Xaml.Documentation
```

## Step 2: Configure Documentation Generation

The package automatically creates a `docfx.json` configuration file. Customize it for your project:

```json
{
  "metadata": [
    {
      "src": [
        {
          "files": ["src/**/*.cs"],
          "exclude": ["src/**/Internal/**", "tests/**"]
        }
      ],
      "dest": "api",
      "includePrivateMembers": false,
      "disableGitFeatures": false,
      "disableDefaultFilter": false
    }
  ],
  "build": {
    "content": [
      {
        "files": ["api/**.yml", "api/index.md"]
      },
      {
        "files": ["articles/**.md", "articles/**/toc.yml", "toc.yml", "*.md"]
      }
    ],
    "resource": [
      {
        "files": ["images/**"]
      }
    ],
    "output": "docs",
    "template": ["default"],
    "globalMetadata": {
      "_appTitle": "Terminal.Gui.Xaml Documentation",
      "_appFooter": "Terminal.Gui.Xaml Framework",
      "_enableSearch": true
    }
  }
}
```

## Step 3: Add XML Documentation Comments

Ensure your public APIs have XML documentation comments:

```csharp
/// <summary>
/// Represents a XAML document with namespace support and validation.
/// </summary>
/// <example>
/// <code>
/// var document = new XamlDocument(xamlContent);
/// var isValid = document.Validate(out var errors);
/// </code>
/// </example>
public class XamlDocument
{
    /// <summary>
    /// Gets the raw XAML content of the document.
    /// </summary>
    /// <value>The original XAML string content.</value>
    public string RawContent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlDocument"/> class.
    /// </summary>
    /// <param name="rawContent">The raw XAML content to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rawContent"/> is null.</exception>
    public XamlDocument(string rawContent)
    {
        // Implementation
    }

    /// <summary>
    /// Validates the XAML document against Microsoft XAML standards.
    /// </summary>
    /// <param name="errors">Output parameter containing validation errors, if any.</param>
    /// <returns><c>true</c> if the document is valid; otherwise, <c>false</c>.</returns>
    public bool Validate(out List<string> errors)
    {
        // Implementation
    }
}
```

## Step 4: Build Documentation

### Automatic Build Integration
Documentation is automatically generated during Release builds:

```bash
dotnet build --configuration Release
```

The generated documentation will be available in the `docs` folder.

### Manual Generation
Generate documentation manually using the MSBuild target:

```bash
dotnet build --target:GenerateDocumentation
```

### Validation Only
Validate documentation quality without generating files:

```bash
dotnet build --target:ValidateDocumentation
```

## Step 5: View Generated Documentation

1. Open `docs/index.html` in your browser
2. Navigate through the API reference
3. Use the search functionality to find specific APIs
4. Browse code examples and tutorials

## Step 6: Customize Documentation

### Add Articles
Create markdown files in the `articles` folder:

```markdown
# Getting Started with XAML Parsing

This article explains how to parse XAML documents using Terminal.Gui.Xaml.

## Basic Usage

```csharp
var parser = new XamlParser();
var document = await parser.ParseAsync(xamlContent);
```
```

### Customize Templates
Override the default DocFX template by placing custom templates in the `templates` folder.

### Add Images and Resources
Place images in the `images` folder and reference them in articles:

```markdown
![XAML Structure](images/xaml-structure.png)
```

## Integration Testing Scenarios

### Test 1: Basic Documentation Generation
```csharp
[Fact]
public async Task Should_Generate_Documentation_For_Sample_Project()
{
    // Arrange
    var generator = new DocumentationGenerator();
    var config = LoadTestConfiguration();
    
    // Act
    var result = await generator.GenerateAsync(config);
    
    // Assert
    Assert.True(result.Success);
    Assert.True(File.Exists(Path.Combine(result.OutputPath, "index.html")));
    Assert.Contains("XamlDocument", result.GeneratedContent);
}
```

### Test 2: XML Documentation Validation
```csharp
[Fact]
public async Task Should_Validate_XML_Documentation_Coverage()
{
    // Arrange
    var validator = new DocumentationValidator();
    var sourcePaths = new[] { "src/" };
    
    // Act
    var result = await validator.ValidateAsync(sourcePaths, minimumCoverage: 80.0);
    
    // Assert
    Assert.True(result.PassesRequirements);
    Assert.True(result.Coverage.CoveragePercentage >= 80.0);
}
```

### Test 3: MSBuild Integration
```csharp
[Fact]
public async Task Should_Integrate_With_MSBuild_Pipeline()
{
    // Arrange
    var buildService = new BuildIntegrationService();
    var request = new RegisterMSBuildTargetRequest
    {
        ProjectPath = "TestProject.csproj",
        TargetConfigurations = new[] { "Release" }
    };
    
    // Act
    var result = await buildService.RegisterMSBuildTargetAsync(request);
    
    // Assert
    Assert.True(result.Success);
    Assert.Contains("GenerateDocumentation", result.IntegratedConfigurations);
}
```

## Troubleshooting

### Common Issues

**Issue**: Documentation generation fails with "No documentable APIs found"
**Solution**: Ensure public classes have XML documentation comments and are included in `docfx.json` source patterns.

**Issue**: Build time is too slow
**Solution**: Enable incremental builds by setting `DocumentationBuildMode` to `Incremental` in your project file.

**Issue**: Generated documentation is missing code examples
**Solution**: Add `<example>` tags with `<code>` blocks in XML documentation comments.

**Issue**: Documentation coverage validation fails
**Solution**: Add XML documentation to all public members or adjust the minimum coverage threshold.

### Performance Optimization

1. **Use incremental builds**: Set `DocumentationBuildMode` to `Incremental`
2. **Exclude unnecessary files**: Update exclude patterns in `docfx.json`
3. **Generate docs only in Release**: Set `DocumentationEnabled` condition
4. **Enable parallel processing**: Set `ParallelBuild` to `true` in configuration

## Next Steps

1. Set up automated documentation deployment to GitHub Pages
2. Integrate documentation generation with CI/CD pipeline
3. Add more comprehensive code examples and tutorials
4. Customize the documentation theme to match your project branding
5. Set up documentation site analytics and feedback collection
