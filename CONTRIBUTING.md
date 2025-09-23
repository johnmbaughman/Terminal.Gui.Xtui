# Contributing to Terminal.Gui XAML Framework

Thank you for your interest in contributing to the Terminal.Gui XAML Framework! This document provides guidelines and information for contributors.

## 🏛️ Constitutional Principles

All contributions must adhere to our four constitutional principles:

### 1. Code Quality Standards
- Follow Microsoft C# coding conventions
- Use nullable reference types
- Include comprehensive XML documentation
- Apply SOLID principles in design
- Maintain clean architecture

### 2. Test-Driven Development
- Write tests first (TDD approach)
- Maintain minimum 80% code coverage
- Include unit, integration, and performance tests
- All tests must pass before merging

### 3. User Experience Consistency  
- Use intuitive and consistent APIs
- Follow Terminal.Gui patterns and conventions
- Provide clear error messages and documentation
- Support accessibility features

### 4. Performance Requirements
- XAML parsing must complete <100ms
- UI rendering must maintain >30 FPS
- Memory usage must stay <50MB
- Component initialization must complete <50ms

## 🚀 Getting Started

### Development Setup

1. **Prerequisites**:
   - .NET 8 SDK or later
   - Git
   - PowerShell (for development scripts)

2. **Fork and Clone**:
   ```bash
   git clone https://github.com/your-username/Terminal.Gui.Xaml.git
   cd Terminal.Gui.Xaml
   ```

3. **Build and Test**:
   ```bash
   ./scripts/build.ps1
   ./scripts/test.ps1 -Coverage
   ./scripts/validate-constitution.ps1
   ```

### Development Workflow

1. **Create Feature Branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Follow TDD Approach**:
   - Write failing tests first
   - Implement minimal code to pass tests
   - Refactor while keeping tests green

3. **Validate Constitutional Compliance**:
   ```bash
   ./scripts/validate-constitution.ps1 -Detailed
   ```

4. **Submit Pull Request**:
   - Ensure all tests pass
   - Include performance benchmarks if applicable
   - Provide clear description of changes

## 📝 Coding Standards

### C# Style Guidelines

- Use PascalCase for public members
- Use camelCase for private fields
- Prefix private fields with underscore: `_fieldName`
- Use nullable reference types consistently
- Include XML documentation for all public APIs

### Example Code Style:

```csharp
/// <summary>
/// Represents a XAML document with parsing and validation capabilities.
/// </summary>
public class XamlDocument
{
    private readonly string _filePath;
    private readonly XamlNamespace[] _namespaces;

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlDocument"/> class.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="namespaces">The declared XML namespaces.</param>
    public XamlDocument(string filePath, XamlNamespace[] namespaces)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
    }

    /// <summary>
    /// Gets the source file path for this XAML document.
    /// </summary>
    public string FilePath => _filePath;
}
```

## 🧪 Testing Guidelines

### Test Categories

1. **Unit Tests** (`tests/Terminal.Gui.Xaml.Tests/`):
   - Test individual components in isolation
   - Use mocking for dependencies
   - Fast execution (<1ms per test)

2. **Integration Tests** (`tests/Terminal.Gui.Xaml.Tests/Integration/`):
   - Test component interactions
   - Validate end-to-end workflows
   - Include XAML parsing to UI rendering

3. **Performance Tests** (`tests/Terminal.Gui.Xaml.Benchmarks/`):
   - Validate constitutional performance requirements
   - Use BenchmarkDotNet for accurate measurements
   - Include memory allocation benchmarks

### Test Example:

```csharp
[Fact]
public async Task ParseAsync_ValidXamlDocument_ReturnsXamlDocument()
{
    // Arrange
    var parser = new XamlParser();
    var xamlContent = @"
        <Window xmlns=""http://schemas.terminal-gui.org/xaml"">
            <Label Text=""Hello World"" />
        </Window>";

    // Act
    var document = await parser.ParseAsync(xamlContent);

    // Assert
    document.Should().NotBeNull();
    document.RootElement.Should().NotBeNull();
    document.RootElement.Name.Should().Be("Window");
}
```

## 🏗️ Architecture Guidelines

### Project Structure

```
src/
├── Terminal.Gui.Xaml/          # Main library
│   ├── Core/                   # Core XAML parsing
│   ├── CodeGeneration/         # Roslyn source generators
│   ├── Runtime/                # Runtime binding and execution
│   ├── MSBuild/               # MSBuild integration
│   └── Extensions/            # Extensibility features
tests/
├── Terminal.Gui.Xaml.Tests/      # Unit and integration tests
├── Terminal.Gui.Xaml.Benchmarks/ # Performance benchmarks
└── TestAssets/                    # Test data and fixtures
```

### Design Patterns

- **Repository Pattern**: For data access abstractions
- **Strategy Pattern**: For parsing and generation algorithms
- **Observer Pattern**: For property change notifications
- **Factory Pattern**: For object creation and dependency injection

## 📊 Performance Guidelines

### Constitutional Performance Requirements

All code must meet these performance requirements:

```csharp
// XAML Parsing Performance
[Fact]
public async Task ParseAsync_LargeDocument_CompletesWithinConstitutionalLimit()
{
    var parser = new XamlParser();
    var largeXaml = GenerateLargeXamlDocument(1000); // 1000+ elements
    
    var stopwatch = Stopwatch.StartNew();
    var document = await parser.ParseAsync(largeXaml);
    stopwatch.Stop();
    
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // <100ms requirement
}
```

### Performance Best Practices

1. **Minimize Allocations**: Use object pooling for frequently created objects
2. **Cache Expensive Operations**: Cache parsed XAML and generated code
3. **Lazy Loading**: Load resources only when needed
4. **Async Operations**: Use async/await for I/O operations

## 🔍 Code Review Process

### Pull Request Requirements

1. **Constitutional Compliance**: All four principles must be met
2. **Test Coverage**: Minimum 80% code coverage
3. **Performance Validation**: No regression in constitutional metrics
4. **Documentation**: Updated documentation for public API changes
5. **Changelog**: Entry in CHANGELOG.md for notable changes

### Review Checklist

- [ ] Code follows style guidelines
- [ ] Tests are comprehensive and pass
- [ ] Performance requirements are met
- [ ] Documentation is updated
- [ ] Constitutional compliance validated
- [ ] No breaking changes (unless versioned appropriately)

## 🐛 Reporting Issues

### Bug Reports

Include the following information:

1. **Environment**: OS, .NET version, Terminal.Gui version
2. **Reproduction Steps**: Clear steps to reproduce the issue
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **XAML Sample**: Minimal XAML that reproduces the issue

### Feature Requests

Include the following information:

1. **Use Case**: Why is this feature needed?
2. **Proposed API**: How should the feature work?
3. **Constitutional Impact**: How does it align with our principles?
4. **Performance Considerations**: Any performance implications

## 🏆 Recognition

Contributors who make significant improvements while maintaining constitutional compliance will be recognized in:

- Repository contributors list
- Release notes
- Project documentation
- Community discussions

## 📞 Getting Help

- **GitHub Discussions**: Ask questions and discuss ideas
- **Issues**: Report bugs or request features  
- **Discord/Slack**: Real-time chat with maintainers (if available)

Thank you for contributing to the Terminal.Gui XAML Framework! Together we can build an amazing declarative UI framework for the terminal. 🚀
