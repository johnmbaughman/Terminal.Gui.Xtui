# Research: Terminal.Gui XAML Framework

## Roslyn Source Generator Integration

### Decision: Use Incremental Source Generators with Syntax Receivers
**Rationale**: Incremental source generators (introduced in .NET 6+, enhanced in .NET 8) provide optimal build performance by only processing changed files and leveraging caching mechanisms. They integrate seamlessly with MSBuild and provide excellent debugging support.

**Alternatives considered**:
- ISourceGenerator (legacy): Less performant, no incremental processing
- T4 Text Templates: Outdated, poor Visual Studio integration
- Custom MSBuild tasks: More complex, less IDE integration

### Implementation Approach
- Use `ISyntaxReceiver` to identify XAML files during compilation
- Leverage `IncrementalGeneratorInitializationContext` for build optimization
- Generate partial classes for XAML code-behind integration
- Provide comprehensive diagnostic reporting for XAML errors

## .NET 8+ Cross-Platform Features

### Decision: Leverage Native AOT and Trimming Support
**Rationale**: .NET 8 offers mature Native AOT compilation and trimming features that significantly improve startup time and reduce memory footprint for console applications. This is crucial for Terminal.Gui applications.

**Key .NET 8 Features to Utilize**:
- Source generators with improved performance and debugging
- Native AOT compatibility for faster startup
- Trimming-friendly reflection patterns  
- Enhanced System.Text.Json source generation
- Improved cross-platform file system APIs

**Alternatives considered**:
- Target .NET 6/7: Missing performance optimizations and latest language features
- Multi-targeting: Increased complexity without significant benefit for new framework

## Terminal.Gui v2+ Architecture

### Decision: Build on Terminal.Gui v2 View-based Architecture
**Rationale**: Terminal.Gui v2 introduces a more robust view hierarchy, improved layout system, and better event handling patterns that align well with XAML's declarative nature.

**Key Terminal.Gui v2 Features**:
- Enhanced View base class with improved lifecycle management
- Advanced layout containers (StackView, AbsoluteLayout, etc.)
- Improved data binding foundation through INotifyPropertyChanged
- Better keyboard and mouse handling patterns
- Enhanced theming and styling capabilities

**Integration Strategy**:
- Map XAML elements directly to Terminal.Gui View derivatives
- Leverage existing layout containers for XAML layout support
- Extend Terminal.Gui's property system for XAML attribute mapping
- Integrate with Terminal.Gui's theming system for XAML styles

## Microsoft XAML Standards Compliance

### Decision: Implement Core XAML 2009 Specification Subset
**Rationale**: Focus on essential XAML features that provide maximum developer productivity while maintaining compatibility with existing Microsoft XAML knowledge and tooling.

**Core Features to Implement**:
- XAML namespaces and qualified names
- Property element syntax and attribute syntax
- Content properties and collection initialization
- Markup extensions ({Binding}, {StaticResource}, etc.)
- Event handler binding via x:Name attributes
- Basic type conversion and value serialization

**Features to Defer**:
- Complex XAML features (view inheritance, templates, triggers) - Phase 2
- BAML compilation - Use runtime parsing initially
- Designer integration - Focus on basic IntelliSense first

### XAML Syntax Compatibility
- Follow Microsoft naming conventions (PascalCase properties, camelCase attributes)
- Support standard XML namespaces pattern
- Implement x: namespace for XAML infrastructure
- Provide clear error messages matching Visual Studio patterns

## MSBuild Integration Architecture

### Decision: Custom MSBuild Task with Source Generator Coordination
**Rationale**: Combine MSBuild tasks for file discovery and validation with Roslyn source generators for code generation to provide the best developer experience.

**Implementation Approach**:
- MSBuild task to validate XAML files and collect metadata
- Source generator to produce C# code from validated XAML
- NuGet package with automatic MSBuild integration
- Clear separation between build-time validation and compile-time generation

**Build Integration Points**:
- Automatic XAML file discovery via MSBuild item groups
- Integration with Visual Studio project system
- Support for hot reload during development
- Comprehensive error reporting in Error List window

## Performance Optimization Strategy

### Decision: Multi-layered Caching and Lazy Loading
**Rationale**: Meet constitutional performance requirements through strategic caching at parse-time, build-time, and runtime.

**Optimization Techniques**:
- Parse-time caching: Cache parsed XAML AST between builds
- Build-time optimization: Generate optimal C# code with minimal allocations
- Runtime caching: Cache frequently accessed binding expressions
- Lazy loading: Defer resource loading until actually needed

**Performance Monitoring**:
- Benchmark tests for all constitutional performance metrics
- Build-time performance tracking in MSBuild
- Runtime performance counters for production monitoring

## Technology Stack Summary

**Core Dependencies** (minimal external references):
- Microsoft.CodeAnalysis.Analyzers (Roslyn source generators)
- System.Xml.Linq (.NET 8 built-in XML processing)
- Terminal.Gui v2+ (target framework, GitHub repository branch 'v2_release', [API Reference](https://gui-cs.github.io/Terminal.Gui/api/Terminal.Gui.App.html))

**Development Dependencies**:
- xUnit + FluentAssertions (testing)
- Microsoft.CodeAnalysis.Testing (source generator testing)
- BenchmarkDotNet (performance testing)

**Target Framework**: net8.0 exclusively for optimal performance and feature access
**Deployment**: NuGet package with MSBuild integration, cross-platform compatible
