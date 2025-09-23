# Code Generation API Contract

## ICodeGenerator Interface

```csharp
/// <summary>
/// Roslyn-based code generator for transforming XAML into C# source code
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Generate C# source code from XAML document
    /// Performance requirement: Must complete within 200ms for typical documents
    /// </summary>
    /// <param name="xamlDocument">Parsed XAML document</param>
    /// <param name="options">Code generation options</param>
    /// <returns>Generated C# source code</returns>
    Task<GeneratedCode> GenerateAsync(XamlDocument xamlDocument, CodeGenerationOptions? options = null);
    
    /// <summary>
    /// Generate multiple classes from XAML document (if needed)
    /// </summary>
    /// <param name="xamlDocument">Parsed XAML document</param>
    /// <param name="options">Code generation options</param>
    /// <returns>Collection of generated classes</returns>
    Task<IEnumerable<GeneratedClass>> GenerateClassesAsync(XamlDocument xamlDocument, CodeGenerationOptions? options = null);
    
    /// <summary>
    /// Validate that XAML can be successfully generated into code
    /// </summary>
    /// <param name="xamlDocument">XAML document to validate</param>
    /// <returns>Generation feasibility results</returns>
    GenerationValidationResult ValidateGeneration(XamlDocument xamlDocument);
    
    /// <summary>
    /// Register custom code generation templates
    /// </summary>
    /// <param name="controlType">Target control type</param>
    /// <param name="template">Code generation template</param>
    void RegisterTemplate(Type controlType, ICodeTemplate template);
}
```

## CodeGenerationOptions Configuration

```csharp
public class CodeGenerationOptions
{
    /// <summary>
    /// Target namespace for generated classes
    /// </summary>
    public string TargetNamespace { get; set; } = "Generated";
    
    /// <summary>
    /// Class name for generated code-behind
    /// </summary>
    public string ClassName { get; set; } = "GeneratedView";
    
    /// <summary>
    /// Generate debugging symbols and comments
    /// </summary>
    public bool IncludeDebugInfo { get; set; } = true;
    
    /// <summary>
    /// Optimize generated code for performance
    /// </summary>
    public bool OptimizePerformance { get; set; } = true;
    
    /// <summary>
    /// Include XML documentation in generated code
    /// </summary>
    public bool IncludeDocumentation { get; set; } = true;
}
```

## Generated Code Structure

```csharp
public class GeneratedCode
{
    /// <summary>
    /// Complete C# source code text
    /// </summary>
    public string SourceCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Generated class name
    /// </summary>
    public string ClassName { get; set; } = string.Empty;
    
    /// <summary>
    /// Target namespace
    /// </summary>
    public string Namespace { get; set; } = string.Empty;
    
    /// <summary>
    /// Dependencies required by generated code
    /// </summary>
    public List<string> Dependencies { get; set; } = new();
    
    /// <summary>
    /// Source map for debugging support
    /// </summary>
    public SourceMap? SourceMap { get; set; }
}
```

## Constitutional Compliance Requirements

- **Code Quality**: Generated code must pass StyleCop and FxCop analysis
- **Performance**: Code generation must complete within 200ms
- **Readability**: Generated code must be human-readable and debuggable
- **Documentation**: All public APIs must have XML documentation
- **Error Handling**: Must provide detailed error messages with source locations

## Template System Contract

```csharp
public interface ICodeTemplate
{
    /// <summary>
    /// Template name for registration
    /// </summary>
    string TemplateName { get; }
    
    /// <summary>
    /// Generate code for specific XAML element
    /// </summary>
    /// <param name="element">XAML element to generate code for</param>
    /// <param name="context">Generation context</param>
    /// <returns>Generated code fragment</returns>
    string GenerateCode(XamlElement element, GenerationContext context);
    
    /// <summary>
    /// Validate that element can be processed by this template
    /// </summary>
    /// <param name="element">XAML element to check</param>
    /// <returns>True if template can handle this element</returns>
    bool CanProcess(XamlElement element);
}
```