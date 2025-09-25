# [Terminal.Gui.Xaml Documentation](../index.md)

## API Reference

This section contains the complete API reference for Terminal.Gui.Xaml framework components:

> **💡 New to Terminal.Gui.Xaml?** Start with our [Getting Started Guide](../articles/getting-started.md) and [Examples](../articles/examples/index.md) to see these APIs in action.

### Core Namespaces

- [Terminal.Gui.Xaml.Generation](Terminal.Gui.Xaml.Generation.yml) - Code generation
- [Terminal.Gui.Xaml.Parsing](Terminal.Gui.Xaml.Parsing.yml) - XAML parsing
- [Terminal.Gui.Xaml.Model](Terminal.Gui.Xaml.Model.yml) - Data models and entities
- [Terminal.Gui.Xaml.Validation](Terminal.Gui.Xaml.Validation.yml) - Validation engine
- [Terminal.Gui.Xaml.Runtime](Terminal.Gui.Xaml.Runtime.yml) - Runtime loader and services
- [Terminal.Gui.Xaml.Events](Terminal.Gui.Xaml.Events.yml) - Event system
- [Terminal.Gui.Xaml.Binding](Terminal.Gui.Xaml.Binding.yml) - Data binding primitives
- [Terminal.Gui.Xaml.Build](Terminal.Gui.Xaml.Build.yml) - MSBuild integration
- [Terminal.Gui.Xaml.Logging](Terminal.Gui.Xaml.Logging.yml) - Logging infrastructure
- [Terminal.Gui.Xaml.Exceptions](Terminal.Gui.Xaml.Exceptions.yml) - Exception hierarchy
- [Terminal.Gui.Xaml.Performance](Terminal.Gui.Xaml.Performance.yml) - Performance monitoring
- [Terminal.Gui.Xaml.Documentation.Models](Terminal.Gui.Xaml.Documentation.Models.yml) - Documentation data models
- [Terminal.Gui.Xaml.Documentation.Services](Terminal.Gui.Xaml.Documentation.Services.yml) - Documentation services
- [Terminal.Gui.Xaml.Documentation.Logging](Terminal.Gui.Xaml.Documentation.Logging.yml) - Documentation logging

### Key Interfaces

- [IXamlParser](Terminal.Gui.Xaml.Parsing.IXamlParser.yml) - XAML parsing interface
- [ICodeGenerator](Terminal.Gui.Xaml.Generation.ICodeGenerator.yml) - Code generation interface  
- [IDocumentationGeneratorService](Terminal.Gui.Xaml.Documentation.Services.IDocumentationGeneratorService.yml) - Documentation generation
- [IBuildIntegrationService](Terminal.Gui.Xaml.Build.IBuildIntegrationService.yml) - MSBuild integration

### Core Types

- [XamlDocument](Terminal.Gui.Xaml.Model.XamlDocument.yml) - XAML document representation
- [ParsedTemplate](Terminal.Gui.Xaml.Model.ParsedTemplate.yml) - Parsed XAML template representation
- [DocFxConfiguration](Terminal.Gui.Xaml.Documentation.Models.DocFxConfiguration.yml) - Documentation configuration

## Quick Navigation by Use Case

### 🏗️ Building User Interfaces

**Concepts:** [Layout System](../articles/concepts/layout.md) | [Data Binding](../articles/concepts/binding.md) | [Event Handling](../articles/concepts/events.md)

**Step-by-step Guides:**
- [Creating Your First Window](../articles/guides/create-window.md)
- [Data Binding Guide](../articles/guides/bind-data.md)
- [Handling User Input](../articles/guides/handle-input.md)
- [Navigation Between Views](../articles/guides/navigation.md)

**Complete Examples:**
- [Hello World](../articles/examples/hello-world.md) - Basic window and XAML structure
- [Basic Layout](../articles/examples/basic-layout.md) - Layout panels and controls
- [Button Click Handling](../articles/examples/button-click.md) - Event handling and user interaction

**Related APIs:**
- `Terminal.Gui.Xaml` namespace - Core UI framework types
- `IXamlParser` - XAML parsing and compilation
- `XamlDocument`, `XamlElement` - Document object model

### 🎨 Styling and Theming

**Concepts:** [Layout System](../articles/concepts/layout.md) | [Visual Theming](../articles/concepts/layout.md)

**Best Practices:**
- [Accessibility Guidelines](../articles/accessibility.md) - Color contrast and screen reader support
- [High Contrast Support](../articles/accessibility.md)

**Related APIs:**
- `Terminal.Gui.Xaml` namespace - UI controls and styling
- Color and theme-related types (when implemented)

### 🔧 Framework Integration

**Build Integration:**
- [MSBuild Integration Guide](../articles/guides/create-window.md) 
- XAML compilation and code generation

**Related APIs:**
- `Terminal.Gui.Xaml.Build` namespace - MSBuild tasks and integration
- `ICodeGenerator` - Code generation from XAML
- `IBuildIntegrationService` - Build system integration

### 📊 Performance and Monitoring

**Performance Concepts:** [Constitutional Performance Requirements](../articles/concepts/layout.md)

**Related APIs:**
- `Terminal.Gui.Xaml.Performance` namespace - Performance monitoring and metrics
- `Terminal.Gui.Xaml.Logging` namespace - Logging and diagnostics

### 🐛 Error Handling and Diagnostics

**Troubleshooting:** [Common Issues FAQ](../articles/faq.md)

**Related APIs:**
- `Terminal.Gui.Xaml.Exceptions` namespace - Framework exception types
- `Terminal.Gui.Xaml.Logging` namespace - Diagnostic logging

## See Also

### Learning Path by Experience Level

**🟢 Beginner (New to Terminal.Gui.Xaml)**
1. [Getting Started Guide](../articles/getting-started.md) - Setup and first application
2. [Hello World Example](../articles/examples/hello-world.md) - Basic window creation
3. [Layout Concepts](../articles/concepts/layout.md) - Understanding UI structure  
4. [Basic Layout Example](../articles/examples/basic-layout.md) - Working with panels and controls

**🟡 Intermediate (Building Applications)**  
1. [Data Binding Guide](../articles/guides/bind-data.md) - Connect UI to data
2. [Button Click Example](../articles/examples/button-click.md) - User interaction patterns
3. [Input Handling Guide](../articles/guides/handle-input.md) - Keyboard and mouse events
4. [Navigation Guide](../articles/guides/navigation.md) - Multi-window applications

**🔴 Advanced (Framework Integration)**
1. [MSBuild Integration](../articles/guides/create-window.md) - Custom build processes  
2. [Performance Guidelines](../articles/concepts/layout.md) - Optimization techniques
3. [Accessibility Implementation](../articles/accessibility.md) - A11y compliance
4. [Contributing Guide](../articles/contributing/index.md) - Framework development

### API Reference Quick Links

**Core Framework Types:**
- [`Terminal.Gui.Xaml.Model`](Terminal.Gui.Xaml.Model.yml) → See [Getting Started Guide](../articles/getting-started.md)
- [`IXamlParser`](Terminal.Gui.Xaml.Parsing.IXamlParser.yml) → See [Layout Concepts](../articles/concepts/layout.md)
- [`XamlDocument`](Terminal.Gui.Xaml.Model.XamlDocument.yml) → See [Hello World Example](../articles/examples/hello-world.md)
- [`ParsedTemplate`](Terminal.Gui.Xaml.Model.ParsedTemplate.yml) → See [Basic Layout Example](../articles/examples/basic-layout.md)

**Build and Integration:**
- [`Terminal.Gui.Xaml.Build`](Terminal.Gui.Xaml.Build.yml) → See [Window Creation Guide](../articles/guides/create-window.md)
- [`ICodeGenerator`](Terminal.Gui.Xaml.Generation.ICodeGenerator.yml) → See [Build Process Documentation](../articles/guides/create-window.md)
- [`IBuildIntegrationService`](Terminal.Gui.Xaml.Build.IBuildIntegrationService.yml) → See [MSBuild Integration](../articles/guides/create-window.md)

**Error Handling and Diagnostics:**
- [`Terminal.Gui.Xaml.Exceptions`](Terminal.Gui.Xaml.Exceptions.yml) → See [Troubleshooting FAQ](../articles/faq.md)
- [`Terminal.Gui.Xaml.Logging`](Terminal.Gui.Xaml.Logging.yml) → See [Debugging Guide](../articles/faq.md)

### Getting Started Resources
- [Framework Overview](../articles/concepts/index.md) - High-level architecture and concepts
- [Getting Started Guide](../articles/getting-started.md) - Your first Terminal.Gui.Xaml application
- [Examples Collection](../articles/examples/index.md) - Working code examples

### Advanced Topics  
- [Accessibility Guidelines](../articles/accessibility.md) - Building accessible terminal applications
- [Contributing Guide](../articles/contributing/index.md) - Contributing to the framework
- [Frequently Asked Questions](../articles/faq.md) - Common questions and solutions

### Reference Materials
- [Glossary](../articles/glossary.md) - Key terms and definitions
- [What's New](../articles/changelog.md) - Latest changes and version history
