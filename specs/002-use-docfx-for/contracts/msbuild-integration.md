# MSBuild Integration Contract

## Documentation Generation Targets

### Target: GenerateDocumentation
**Purpose**: Generate documentation as part of build process
**When**: After compilation, before packaging

**Inputs**:
- `$(DocumentationEnabled)`: bool - Enable/disable documentation generation
- `$(DocumentationConfiguration)`: string - Path to docfx.json configuration
- `$(DocumentationOutputPath)`: string - Output directory for generated docs
- `$(DocumentationBuildMode)`: string - Full, Incremental, or MetadataOnly

**Outputs**:
- `$(DocumentationOutputPath)/**/*`: Generated documentation files
- `$(DocumentationOutputPath)/manifest.json`: Generation manifest

**Properties**:
```xml
<PropertyGroup>
  <DocumentationEnabled Condition="'$(Configuration)' == 'Release'">true</DocumentationEnabled>
  <DocumentationEnabled Condition="'$(Configuration)' != 'Release'">false</DocumentationEnabled>
  <DocumentationOutputPath>$(OutputPath)/docs</DocumentationOutputPath>
  <DocumentationBuildMode>Incremental</DocumentationBuildMode>
</PropertyGroup>
```

### Target: ValidateDocumentation
**Purpose**: Validate documentation coverage and quality
**When**: Before documentation generation

**Inputs**:
- `$(RequiredDocumentationCoverage)`: double - Minimum coverage percentage (default: 80)
- `$(ValidateDocumentationLinks)`: bool - Check for broken links
- `$(ValidateDocumentationExamples)`: bool - Compile code examples

**Outputs**:
- `$(IntermediateOutputPath)/documentation-validation.json`: Validation report

### Target: CleanDocumentation
**Purpose**: Remove generated documentation files
**When**: During clean operation

**Contract Requirements**:
- Must integrate with existing Clean target
- Must remove all generated documentation files
- Must preserve source documentation files
- Must not affect build performance when documentation disabled

## ItemGroups

### DocumentationSource
**Purpose**: Specify source files for documentation generation

```xml
<ItemGroup>
  <DocumentationSource Include="src/**/*.cs" />
  <DocumentationSource Include="README.md" />
  <DocumentationSource Include="docs/articles/**/*.md" />
</ItemGroup>
```

### DocumentationExclude
**Purpose**: Exclude files from documentation generation

```xml
<ItemGroup>
  <DocumentationExclude Include="src/**/Internal/**/*.cs" />
  <DocumentationExclude Include="src/**/*.Designer.cs" />
  <DocumentationExclude Include="tests/**/*.cs" />
</ItemGroup>
```

## Task Contract: GenerateDocumentationTask

### Task Definition
```xml
<UsingTask TaskName="Terminal.Gui.Xaml.Build.GenerateDocumentationTask"
           AssemblyFile="$(MSBuildThisFileDirectory)Terminal.Gui.Xaml.Build.dll" />
```

### Task Parameters
- `ConfigurationFile` (string, required): Path to docfx.json
- `SourceFiles` (ITaskItem[], required): Files to include in documentation
- `OutputPath` (string, required): Output directory
- `BuildMode` (string, optional): Build mode (default: Incremental)
- `LogLevel` (string, optional): Logging verbosity (default: Normal)

### Task Outputs
- `GeneratedFiles` (ITaskItem[]): List of generated documentation files
- `ValidationResult` (ITaskItem): Validation result summary
- `Success` (bool): Whether generation succeeded

### Error Handling
- Must log all errors to MSBuild logger
- Must return false on failure
- Must provide actionable error messages
- Must not throw exceptions

## Integration Points

### Visual Studio Integration
- Must appear in Build menu when project is selected
- Must integrate with Solution Explorer context menu
- Must show progress in Output window
- Must support cancellation

### Command Line Integration
- Must work with `dotnet build`
- Must work with `msbuild`
- Must respect standard MSBuild properties
- Must support CI/CD environments

### NuGet Package Integration
- Must install MSBuild targets via NuGet package
- Must not require manual project file modification
- Must support multiple project types
- Must handle package updates gracefully

## Performance Contract

### Build Impact
- Must not slow down Debug builds
- Must complete within 5 minutes for full documentation
- Must support incremental builds
- Must leverage parallel processing

### Resource Usage
- Must not exceed 200MB memory during generation
- Must clean up temporary files
- Must not lock files unnecessarily
- Must respect build output redirection
