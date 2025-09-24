# Contributing to Documentation

Help us improve Terminal.Gui.Xaml documentation by contributing guides, examples, fixes, and enhancements.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Quick Links**: [Getting Started](#getting-started) • [Writing Guidelines](#writing-guidelines) • [Review Process](#review-process)

## Why Contribute to Documentation?

Documentation is crucial for developer experience and adoption. By contributing, you:

- **Help other developers** learn and use Terminal.Gui.Xaml effectively
- **Share your expertise** and real-world usage patterns
- **Improve the framework** by identifying gaps and unclear explanations
- **Build community** around Terminal.Gui.Xaml development

## Types of Contributions

### 📝 Content Contributions
- **New guides and tutorials** for common scenarios
- **Code examples** showing best practices
- **API documentation improvements** with better descriptions
- **Troubleshooting sections** based on real issues

### 🐛 Documentation Fixes
- **Typos and grammar** corrections
- **Broken links** and reference fixes
- **Outdated information** updates
- **Code examples** that don't work

### 🎨 Structure and Experience
- **Navigation improvements** for better discoverability
- **Search optimization** with better keywords
- **Cross-linking** between related topics
- **Accessibility enhancements** for documentation

## Getting Started

### Prerequisites

Before contributing documentation:

1. **Install Required Tools**:
   ```powershell
   # Install .NET 8+ SDK
   dotnet --version  # Should be 8.0+
   
   # Install DocFX globally
   dotnet tool install -g docfx
   
   # Verify installation
   docfx --version
   ```

2. **Set Up Development Environment**:
   - **Git**: For version control and pull requests
   - **Editor**: VS Code, Visual Studio, or any text editor
   - **Terminal**: PowerShell, Command Prompt, or bash

3. **Understand the Structure**:
   ```
   docs/
   ├── articles/          # Conceptual documentation
   │   ├── concepts/      # Framework concepts
   │   ├── guides/        # Step-by-step instructions  
   │   ├── examples/      # Code samples
   │   └── contributing/  # Contribution guidelines
   ├── api/              # Generated API reference
   └── docfx.json        # DocFX configuration
   ```

### Fork and Clone Repository

1. **Fork the Repository**:
   - Go to [Terminal.Gui.Xaml GitHub](https://github.com/johnmbaughman/Terminal.Gui.Xaml)
   - Click "Fork" to create your copy

2. **Clone Your Fork**:
   ```powershell
   git clone https://github.com/YOUR-USERNAME/Terminal.Gui.Xaml.git
   cd Terminal.Gui.Xaml
   ```

3. **Set Up Upstream Remote**:
   ```powershell
   git remote add upstream https://github.com/johnmbaughman/Terminal.Gui.Xaml.git
   ```

### Local Development Setup

1. **Install Dependencies**:
   ```powershell
   # Restore .NET dependencies
   dotnet restore
   
   # Build the project to ensure everything works
   dotnet build
   ```

2. **Build Documentation Locally**:
   ```powershell
   # Build docs
   cd docs
   docfx docfx.json
   
   # Serve locally for preview
   docfx docfx.json --serve --port 8080
   ```

3. **Preview Your Changes**:
   - Open browser to `http://localhost:8080`
   - Changes to Markdown files auto-refresh
   - DocFX configuration changes require rebuild

## Writing Guidelines

### Documentation Standards

Follow our [Documentation Style Guide](docs-style-guide.md) for consistency:

#### Structure Pattern
Use the **What/How/Why** structure for all conceptual content:

```markdown
## What is [Topic]?
Brief explanation of the concept or feature.

## How to Use [Topic]
Step-by-step instructions with code examples.

## Why Use [Topic]?
Benefits, use cases, and when to apply it.
```

#### Code Examples
All code examples must be:

- **Runnable**: Include complete, working code
- **Tested**: Verify examples actually work
- **Explained**: Add comments explaining key concepts
- **Platform-Aware**: Note Windows/Linux differences if applicable

```csharp
// ✅ Good example - complete and runnable
using Terminal.Gui;
using Terminal.Gui.Xaml;

namespace MyApp;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();
        try
        {
            var window = new HelloWorldWindow();
            Application.Run(window);
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
```

#### Markdown Standards

- **Headers**: Use sentence case (not title case)
- **Code Blocks**: Always specify language for syntax highlighting
- **Links**: Use relative links for internal documentation
- **Images**: Include alt text for accessibility
- **Lists**: Use consistent bullet points or numbering

#### Version Metadata
Include version information in documentation headers:

```markdown
> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Applies To**: All Terminal.Gui.Xaml applications
```

### Content Guidelines

#### API Documentation
When documenting APIs:

1. **Summary**: Brief one-line description
2. **Parameters**: Describe each parameter's purpose
3. **Returns**: Explain what the method returns
4. **Examples**: Show common usage patterns
5. **Exceptions**: Document when exceptions are thrown
6. **See Also**: Link to related APIs and concepts

#### Guides and Tutorials
For step-by-step content:

1. **Prerequisites**: What users need before starting
2. **Learning Objectives**: What they'll accomplish
3. **Step-by-Step Instructions**: Clear, numbered steps
4. **Complete Examples**: Working code they can run
5. **Next Steps**: Where to go after completing the guide
6. **Troubleshooting**: Common issues and solutions

#### Examples
For code examples:

1. **Purpose**: What the example demonstrates
2. **Prerequisites**: Required knowledge or setup
3. **Complete Code**: Full, runnable example
4. **Explanation**: Walk through key concepts
5. **Variations**: Alternative approaches or extensions
6. **Related Topics**: Links to relevant documentation

## Contribution Workflow

### Making Changes

1. **Create Feature Branch**:
   ```powershell
   # Update your fork
   git fetch upstream
   git checkout main
   git merge upstream/main
   
   # Create feature branch
   git checkout -b docs/improve-binding-guide
   ```

2. **Make Your Changes**:
   - Edit Markdown files in `docs/articles/`
   - Test examples to ensure they work
   - Build docs locally to verify formatting
   - Check links and cross-references

3. **Test Your Changes**:
   ```powershell
   # Build and serve locally
   cd docs
   docfx docfx.json --serve --port 8080
   
   # Check for warnings or errors
   docfx docfx.json --warningsAsErrors
   ```

### Quality Checklist

Before submitting your contribution:

#### Content Quality
- [ ] **Accurate**: Information is correct and up-to-date
- [ ] **Complete**: Covers the topic comprehensively
- [ ] **Clear**: Easy to understand for the target audience
- [ ] **Consistent**: Follows our style guidelines
- [ ] **Accessible**: Uses plain language and includes alt text

#### Code Quality
- [ ] **Working**: All code examples compile and run
- [ ] **Complete**: Include necessary using statements and setup
- [ ] **Formatted**: Properly indented and styled
- [ ] **Commented**: Key concepts explained in comments
- [ ] **Cross-Platform**: Note any platform-specific considerations

#### Technical Quality
- [ ] **Links Work**: All internal and external links function
- [ ] **Images Display**: Screenshots and diagrams show correctly
- [ ] **Metadata**: Version information and headers included
- [ ] **Cross-References**: Related content properly linked
- [ ] **Search Friendly**: Good keywords and descriptions

### Submitting Pull Request

1. **Commit Your Changes**:
   ```powershell
   git add .
   git commit -m "docs: improve data binding guide with validation examples"
   ```

2. **Push to Your Fork**:
   ```powershell
   git push origin docs/improve-binding-guide
   ```

3. **Create Pull Request**:
   - Go to your fork on GitHub
   - Click "Compare & pull request"
   - Fill out the PR template completely
   - Reference any related issues

### Pull Request Template

Use this template for documentation PRs:

```markdown
## Description
Brief description of the changes and motivation.

## Type of Change
- [ ] New documentation (guides, examples, concepts)
- [ ] Bug fix (typos, broken links, incorrect info)
- [ ] Enhancement (improved explanations, additional examples)
- [ ] Structure (reorganization, navigation improvements)

## Documentation Checklist
- [ ] Content follows What/How/Why structure where applicable
- [ ] Code examples are complete and tested
- [ ] Links are working and appropriate
- [ ] Metadata (version info) is included
- [ ] Built locally without warnings or errors

## Testing
- [ ] Built documentation locally: `docfx docfx.json`
- [ ] Served locally to verify formatting: `docfx serve`
- [ ] Tested all code examples
- [ ] Verified all links work

## Screenshots (if applicable)
Add screenshots showing the improvements or new content.

## Related Issues
Fixes #issue_number
```

## Review Process

### Review Stages

Documentation contributions go through these review stages:

1. **Automated Checks**:
   - DocFX build verification
   - Link checking
   - Markdown linting
   - Spell checking

2. **Content Review**:
   - Technical accuracy verification
   - Code example testing
   - Style guide compliance
   - Completeness assessment

3. **Community Review**:
   - Feedback from maintainers
   - Input from community members
   - Suggestions for improvements
   - Discussion of alternatives

### Review Criteria

Your contribution will be evaluated on:

#### Content Quality
- **Accuracy**: Information is correct and current
- **Clarity**: Easy to understand for target audience
- **Completeness**: Covers topic thoroughly
- **Usefulness**: Provides value to developers

#### Technical Quality
- **Working Examples**: Code compiles and runs correctly
- **Best Practices**: Demonstrates recommended patterns
- **Error Handling**: Shows appropriate error management
- **Performance**: Considers efficiency implications

#### Documentation Standards
- **Style Consistency**: Follows established guidelines
- **Proper Structure**: Uses What/How/Why pattern appropriately
- **Good Navigation**: Easy to find and cross-referenced well
- **Accessibility**: Inclusive language and alt text

### Addressing Feedback

When reviewers provide feedback:

1. **Understand the Concern**: Ask for clarification if needed
2. **Make Requested Changes**: Update your branch accordingly  
3. **Test Changes**: Verify fixes work as expected
4. **Respond to Comments**: Acknowledge and explain your updates
5. **Be Patient**: Quality documentation takes time to get right

## Advanced Contributing

### Documentation Automation

Help improve our documentation tooling:

#### Link Validation
Add automated link checking:

```yaml
# .github/workflows/docs.yml
- name: Check Links
  run: |
    # Install link checker
    npm install -g markdown-link-check
    
    # Check all markdown files
    find docs -name "*.md" -exec markdown-link-check {} \;
```

#### Example Testing
Create tests that verify code examples:

```csharp
[Test]
public void ExampleCodeCompiles()
{
    var code = File.ReadAllText("docs/articles/examples/hello-world.md");
    var csharpBlocks = ExtractCSharpBlocks(code);
    
    foreach (var block in csharpBlocks)
    {
        var compilation = CSharpCompilation.Create("test")
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(block))
            .AddReferences(GetRequiredReferences());
            
        var result = compilation.Emit(new MemoryStream());
        Assert.IsTrue(result.Success, "Example code should compile");
    }
}
```

### Specialized Contributions

#### API Documentation
Improve generated API docs by enhancing XML comments:

```csharp
/// <summary>
/// Binds a property to a data source with automatic change notifications.
/// </summary>
/// <param name="path">The property path to bind to, using dot notation for nested properties.</param>
/// <param name="mode">The binding mode determining data flow direction.</param>
/// <returns>A binding expression that can be applied to XAML properties.</returns>
/// <example>
/// <code>
/// // Bind TextField to ViewModel property
/// &lt;TextField Text="{Binding Name, Mode=TwoWay}" /&gt;
/// </code>
/// </example>
/// <exception cref="BindingException">Thrown when the binding path is invalid or source property doesn't exist.</exception>
/// <seealso cref="INotifyPropertyChanged"/>
/// <seealso cref="BindingMode"/>
public BindingExpression Bind(string path, BindingMode mode = BindingMode.OneWay)
{
    // Implementation
}
```

#### Internationalization
Help make documentation accessible globally:

- **Translation Coordination**: Organize translation efforts
- **Cultural Adaptation**: Ensure examples work in different locales
- **RTL Support**: Consider right-to-left reading languages
- **Locale-Specific Examples**: Show region-appropriate scenarios

#### Accessibility Improvements
Enhance documentation accessibility:

- **Screen Reader Compatibility**: Test with assistive technologies
- **Alternative Formats**: Provide different content formats
- **Plain Language**: Use clear, simple explanations
- **Visual Descriptions**: Describe images and diagrams in text

## Recognition and Community

### Contributor Recognition
Documentation contributors are recognized through:

- **Contributors Section**: Listed in README and documentation
- **GitHub Contributions**: Contributions appear in GitHub history
- **Community Showcase**: Featured examples and guides highlighted
- **Maintainer Path**: Outstanding contributors may become maintainers

### Building Community
Help build a welcoming documentation community:

- **Mentor New Contributors**: Help others get started
- **Share Knowledge**: Present at conferences or write blog posts
- **Provide Feedback**: Review others' contributions constructively
- **Advocate for Quality**: Champion high documentation standards

## Resources and Tools

### Development Tools
- **VS Code Extensions**:
  - markdownlint (markdown linting)
  - Code Spell Checker (spell checking)
  - GitLens (git integration)
  - Live Server (local preview)

- **Useful Commands**:
  ```powershell
  # Find broken links
  grep -r "](.*\.md" docs/articles/ | grep -v "http"
  
  # Count documentation files
  find docs/articles -name "*.md" | wc -l
  
  # Search for outdated version references
  grep -r "v0\." docs/articles/
  ```

### Documentation Resources
- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [Markdown Guide](https://www.markdownguide.org/)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)
- [Microsoft Style Guide](https://docs.microsoft.com/en-us/style-guide/)

### Community Resources
- [GitHub Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xaml/discussions)
- [Issues and Bug Reports](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues)
- [Documentation Style Guide](docs-style-guide.md)

## Getting Help

### Support Channels
If you need help while contributing:

1. **GitHub Discussions**: Ask questions about contribution process
2. **Issues**: Report problems with documentation tooling
3. **Discord/Slack**: Real-time help from community (if available)
4. **Email**: Contact maintainers directly for complex issues

### Common Questions

**Q**: How do I test code examples?
**A**: Create a test project and copy the code to verify it compiles and runs.

**Q**: What if I'm not sure about technical accuracy?
**A**: Submit a draft PR marked as WIP (Work in Progress) for early feedback.

**Q**: How detailed should explanations be?
**A**: Assume readers are familiar with C# but new to Terminal.Gui.Xaml.

**Q**: Can I contribute if I'm not a native English speaker?
**A**: Absolutely! We welcome contributions from everyone and will help with language refinement.

## Next Steps

Ready to contribute? Here's how to get started:

1. **Read the Style Guide**: Familiarize yourself with our [Documentation Style Guide](docs-style-guide.md)
2. **Find an Issue**: Look for [documentation issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/labels/documentation) labeled "good first issue"
3. **Join the Community**: Introduce yourself in GitHub Discussions
4. **Start Small**: Begin with typo fixes or small improvements to get familiar with the process

Thank you for helping make Terminal.Gui.Xaml documentation better for everyone! 🎉

---

> **Quick Links**: [Style Guide](docs-style-guide.md) • [GitHub Repository](https://github.com/johnmbaughman/Terminal.Gui.Xaml) • [Issues](https://github.com/johnmbaughman/Terminal.Gui.Xaml/issues) • [Discussions](https://github.com/johnmbaughman/Terminal.Gui.Xaml/discussions)