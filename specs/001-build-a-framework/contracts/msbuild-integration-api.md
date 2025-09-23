# MSBuild Integration API Contract

## IXamlBuildTask Interface

```csharp
/// <summary>
/// MSBuild task for XAML file processing and integration
/// </summary>
public interface IXamlBuildTask
{
    /// <summary>
    /// Input XAML files to process
    /// </summary>
    ITaskItem[] XamlFiles { get; set; }
    
    /// <summary>
    /// Output directory for generated files
    /// </summary>
    string OutputPath { get; set; }
    
    /// <summary>
    /// Target namespace for generated classes
    /// </summary>
    string RootNamespace { get; set; }
    
    /// <summary>
    /// Enable build optimizations
    /// </summary>
    bool EnableOptimizations { get; set; }
    
    /// <summary>
    /// Execute the build task
    /// Performance requirement: Must complete efficiently for incremental builds
    /// </summary>
    /// <returns>True if successful, false if errors occurred</returns>
    bool Execute();
    
    /// <summary>
    /// Generated output files
    /// </summary>
    ITaskItem[] GeneratedFiles { get; }
    
    /// <summary>
    /// Build errors and warnings
    /// </summary>
    ITaskItem[] BuildErrors { get; }
}
```

## Source Generator Integration

```csharp
/// <summary>
/// Roslyn source generator for XAML processing
/// </summary>
[Generator]
public class XamlSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initialize the generator with build pipeline
    /// </summary>
    /// <param name="context">Generator initialization context</param>
    public void Initialize(IncrementalGeneratorInitializationContext context);
    
    /// <summary>
    /// Process XAML files during compilation
    /// Performance requirement: Must use incremental generation for build speed
    /// </summary>
    /// <param name="context">Source generation context</param>
    /// <param name="xamlFiles">XAML files to process</param>
    /// <returns>Generated source code</returns>
    Task<GeneratedSourceResult> ProcessXamlFiles(SourceProductionContext context, ImmutableArray<AdditionalText> xamlFiles);
}
```

## Build Integration Configuration

```csharp
public class XamlBuildConfiguration
{
    /// <summary>
    /// Enable XAML processing in build
    /// </summary>
    public bool EnableXamlProcessing { get; set; } = true;
    
    /// <summary>
    /// Generate debugging information
    /// </summary>
    public bool GenerateDebugInfo { get; set; } = true;
    
    /// <summary>
    /// Optimize generated code
    /// </summary>
    public bool OptimizeGeneratedCode { get; set; } = true;
    
    /// <summary>
    /// Validate XAML during build
    /// </summary>
    public bool ValidateXaml { get; set; } = true;
    
    /// <summary>
    /// Custom namespace mappings
    /// </summary>
    public Dictionary<string, string> NamespaceMappings { get; set; } = new();
    
    /// <summary>
    /// Additional reference assemblies
    /// </summary>
    public List<string> ReferenceAssemblies { get; set; } = new();
}
```

## NuGet Package Structure

```xml
<!-- Terminal.Gui.Xaml.targets -->
<Project>
  <PropertyGroup>
    <EnableXamlProcessing Condition="'$(EnableXamlProcessing)' == ''">true</EnableXamlProcessing>
    <XamlOutputPath Condition="'$(XamlOutputPath)' == ''">$(IntermediateOutputPath)Generated</XamlOutputPath>
  </PropertyGroup>

  <ItemGroup Condition="'$(EnableXamlProcessing)' == 'true'">
    <XamlFile Include="**/*.xaml" Exclude="bin/**;obj/**" />
    <Analyzer Include="$(MSBuildThisFileDirectory)../analyzers/Terminal.Gui.Xaml.Analyzers.dll" />
  </ItemGroup>

  <Target Name="ProcessXamlFiles" BeforeTargets="BeforeCompile" Condition="'$(EnableXamlProcessing)' == 'true' and '@(XamlFile)' != ''">
    <XamlBuildTask 
      XamlFiles="@(XamlFile)"
      OutputPath="$(XamlOutputPath)"
      RootNamespace="$(RootNamespace)"
      EnableOptimizations="$(Optimize)">
      <Output TaskParameter="GeneratedFiles" ItemName="Compile" />
      <Output TaskParameter="BuildErrors" ItemName="XamlError" />
    </XamlBuildTask>
  </Target>
</Project>
```

## Performance Requirements

- **Incremental Builds**: Must only process changed XAML files
- **Build Speed**: Must not significantly impact overall build time
- **Memory Usage**: Must stay within reasonable limits during build
- **Error Reporting**: Must integrate with Visual Studio Error List
- **Caching**: Must cache parsed results between builds

## Visual Studio Integration

```csharp
public interface IDesignTimeServices
{
    /// <summary>
    /// Provide IntelliSense for XAML editing
    /// </summary>
    /// <param name="xamlContent">Current XAML content</param>
    /// <param name="position">Cursor position</param>
    /// <returns>Available completions</returns>
    Task<CompletionList> GetCompletionsAsync(string xamlContent, int position);
    
    /// <summary>
    /// Validate XAML during editing
    /// </summary>
    /// <param name="xamlContent">Current XAML content</param>
    /// <returns>Validation diagnostics</returns>
    Task<IEnumerable<Diagnostic>> ValidateAsync(string xamlContent);
    
    /// <summary>
    /// Provide hover information
    /// </summary>
    /// <param name="xamlContent">Current XAML content</param>
    /// <param name="position">Hover position</param>
    /// <returns>Hover information</returns>
    Task<HoverInfo?> GetHoverInfoAsync(string xamlContent, int position);
}
```

## Constitutional Compliance

- **Code Quality**: All build outputs must pass static analysis
- **Performance**: Build integration must not slow development workflow
- **User Experience**: Must provide clear error messages and diagnostics
- **Cross-Platform**: Must work on Windows, Linux, and macOS build systems
- **Reliability**: Must handle build errors gracefully without corrupting project state