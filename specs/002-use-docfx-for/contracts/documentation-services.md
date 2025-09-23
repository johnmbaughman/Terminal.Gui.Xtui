# API Contract: Documentation Generation Service

## IDocumentationGeneratorService

### GenerateDocumentationAsync
**Purpose**: Generate complete documentation for the framework
**HTTP Equivalent**: `POST /api/documentation/generate`

**Request**:
```csharp
public class GenerateDocumentationRequest
{
    public DocFxConfiguration Configuration { get; set; }
    public string[] SourcePaths { get; set; }
    public DocumentationBuildMode BuildMode { get; set; }
    public bool ValidateOnly { get; set; } = false;
}
```

**Response**:
```csharp
public class GenerateDocumentationResponse
{
    public bool Success { get; set; }
    public string OutputPath { get; set; }
    public ApiDocumentation[] GeneratedDocuments { get; set; }
    public DocumentationValidationResult ValidationResult { get; set; }
    public TimeSpan GenerationTime { get; set; }
    public string[] Errors { get; set; }
}
```

**Contract Requirements**:
- Must complete within 5 minutes for full framework
- Must validate XML documentation coverage ≥80%
- Must generate searchable HTML output
- Must include API cross-references
- Must handle incremental builds
- Must report detailed error information

### ValidateDocumentationAsync
**Purpose**: Validate documentation quality without generation
**HTTP Equivalent**: `POST /api/documentation/validate`

**Request**:
```csharp
public class ValidateDocumentationRequest
{
    public string[] SourcePaths { get; set; }
    public bool CheckLinks { get; set; } = true;
    public bool CheckExamples { get; set; } = true;
    public double MinimumCoverage { get; set; } = 80.0;
}
```

**Response**:
```csharp
public class ValidateDocumentationResponse
{
    public DocumentationValidationResult ValidationResult { get; set; }
    public CoverageMetrics Coverage { get; set; }
    public ValidationIssue[] Issues { get; set; }
    public bool PassesRequirements { get; set; }
}
```

**Contract Requirements**:
- Must complete within 1 minute
- Must identify all missing documentation
- Must validate all documentation links
- Must compile-check all code examples
- Must provide actionable error messages

## IDocumentationConfigurationService

### LoadConfigurationAsync
**Purpose**: Load DocFX configuration from project settings
**HTTP Equivalent**: `GET /api/documentation/configuration`

**Response**:
```csharp
public class LoadConfigurationResponse
{
    public DocFxConfiguration Configuration { get; set; }
    public bool ConfigurationExists { get; set; }
    public string[] ValidationErrors { get; set; }
}
```

### SaveConfigurationAsync
**Purpose**: Save DocFX configuration to project settings
**HTTP Equivalent**: `PUT /api/documentation/configuration`

**Request**:
```csharp
public class SaveConfigurationRequest
{
    public DocFxConfiguration Configuration { get; set; }
    public bool ValidateBeforeSave { get; set; } = true;
}
```

**Response**:
```csharp
public class SaveConfigurationResponse
{
    public bool Success { get; set; }
    public string ConfigurationPath { get; set; }
    public string[] ValidationErrors { get; set; }
}
```

## IBuildIntegrationService

### RegisterMSBuildTargetAsync
**Purpose**: Register documentation generation with MSBuild pipeline
**HTTP Equivalent**: `POST /api/build/register-target`

**Request**:
```csharp
public class RegisterMSBuildTargetRequest
{
    public string ProjectPath { get; set; }
    public string[] TargetConfigurations { get; set; } // e.g., ["Release"]
    public bool RunOnBuild { get; set; } = true;
    public bool RunOnPublish { get; set; } = true;
}
```

**Response**:
```csharp
public class RegisterMSBuildTargetResponse
{
    public bool Success { get; set; }
    public string TargetFilePath { get; set; }
    public string[] IntegratedConfigurations { get; set; }
    public string[] Errors { get; set; }
}
```

### ExecuteBuildTargetAsync
**Purpose**: Execute documentation generation as part of build
**HTTP Equivalent**: `POST /api/build/execute-target`

**Request**:
```csharp
public class ExecuteBuildTargetRequest
{
    public string ProjectPath { get; set; }
    public string Configuration { get; set; } // e.g., "Release"
    public IDictionary<string, string> MSBuildProperties { get; set; }
}
```

**Response**:
```csharp
public class ExecuteBuildTargetResponse
{
    public bool Success { get; set; }
    public string BuildOutput { get; set; }
    public GenerateDocumentationResponse DocumentationResult { get; set; }
    public string[] BuildErrors { get; set; }
    public TimeSpan BuildTime { get; set; }
}
```

## Error Codes

### Documentation Generation Errors
- `DOC_001`: Configuration file not found or invalid
- `DOC_002`: Source paths contain no documentable code
- `DOC_003`: Documentation coverage below minimum threshold
- `DOC_004`: Template files missing or corrupted
- `DOC_005`: Output directory not accessible
- `DOC_006`: DocFX tool execution failed
- `DOC_007`: Generated documentation validation failed

### Build Integration Errors
- `BUILD_001`: MSBuild project file not found
- `BUILD_002`: Unable to modify project file
- `BUILD_003`: Target configuration not supported
- `BUILD_004`: Build execution failed
- `BUILD_005`: Documentation target already registered

### Validation Errors
- `VAL_001`: XML documentation malformed
- `VAL_002`: Documentation links broken
- `VAL_003`: Code examples compilation failed
- `VAL_004`: Required documentation missing
- `VAL_005`: Documentation coverage calculation failed

## Performance Requirements

### Service Level Objectives
- Documentation generation: <5 minutes (full framework)
- Documentation validation: <1 minute
- Configuration operations: <5 seconds
- Build integration: <10 seconds

### Resource Limits
- Memory usage: <200MB during generation
- Disk I/O: Respect existing build output locations
- CPU usage: Utilize available cores efficiently
- Network: Minimal (local operation only)
