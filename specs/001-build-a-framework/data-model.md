# Data Model: Terminal.Gui XAML Framework

## Core XAML Processing Entities

### XamlDocument
**Purpose**: Represents a complete XAML file with metadata and parsed content
**Key Attributes**:
- `string FilePath` - Source file location
- `XamlNamespace[] Namespaces` - Declared XML namespaces
- `XamlElement RootElement` - Top-level XAML element
- `XamlResource[] Resources` - Declared resources and styles
- `Dictionary<string, string> Attributes` - Document-level attributes

**Relationships**: 
- Contains one RootElement
- Contains zero or more Resources
- References multiple Namespaces

**Validation Rules**:
- FilePath must be valid file system path
- RootElement cannot be null
- Namespace prefixes must be unique within document

### XamlElement
**Purpose**: Represents a single XAML element (control, layout, etc.)
**Key Attributes**:
- `string Name` - Element name (e.g., "Button", "StackView")
- `string Namespace` - XML namespace prefix
- `Dictionary<string, XamlAttribute> Attributes` - Element attributes
- `List<XamlElement> Children` - Child elements
- `string Content` - Text content (for content properties)
- `SourceLocation Location` - Position in source file for errors

**Relationships**:
- Parent-child hierarchy with other XamlElements
- References XamlAttributes
- Maps to Terminal.Gui View types

**State Transitions**:
- Parsed → Validated → CodeGenerated → RuntimeBound

### XamlAttribute
**Purpose**: Represents an attribute on a XAML element
**Key Attributes**:
- `string Name` - Attribute name
- `string Value` - Raw attribute value
- `XamlAttributeType Type` - Attribute type (Property, Event, AttachedProperty)
- `BindingExpression Binding` - Data binding information (if applicable)
- `bool IsMarkupExtension` - Whether value uses markup extension syntax

**Validation Rules**:
- Name must be valid identifier
- Event attributes must reference valid method signatures
- Binding expressions must have valid syntax

## Code Generation Entities

### CodeGenerator
**Purpose**: Transforms XAML documents into C# source code
**Key Attributes**:
- `GenerationContext Context` - Current generation session data
- `List<GeneratedClass> GeneratedClasses` - Output classes
- `DiagnosticReporter Diagnostics` - Error and warning reporting
- `TemplateEngine Templates` - Code generation templates

**Relationships**:
- Processes XamlDocuments
- Produces GeneratedClasses
- Reports to MSBuild via diagnostics

### GeneratedClass
**Purpose**: Represents a C# class generated from XAML
**Key Attributes**:
- `string ClassName` - Generated class name
- `string Namespace` - Target namespace
- `string SourceCode` - Complete C# source code
- `List<GeneratedMethod> Methods` - Generated methods
- `List<GeneratedProperty> Properties` - Generated properties
- `string BaseClass` - Base class (typically Terminal.Gui.View derivative)

**Relationships**:
- Generated from XamlDocument
- Contains GeneratedMethods and GeneratedProperties

### BindingExpression
**Purpose**: Represents data binding configuration and runtime behavior
**Key Attributes**:
- `string Path` - Property path (e.g., "User.Name")
- `BindingMode Mode` - OneWay, TwoWay, OneTime
- `IValueConverter Converter` - Optional value conversion
- `string ConverterParameter` - Converter parameter
- `object Source` - Binding source override

**State Transitions**:
- Parsed → Resolved → Compiled → Active

## Runtime Execution Entities

### ViewFactory
**Purpose**: Creates Terminal.Gui views from generated code at runtime
**Key Attributes**:
- `Dictionary<string, Type> RegisteredViews` - Available view types
- `IServiceProvider ServiceProvider` - Dependency injection container
- `ViewFactoryOptions Options` - Configuration options

**Relationships**:
- Creates Terminal.Gui View instances
- Integrates with dependency injection
- Coordinates with DataBindingEngine

### DataBindingEngine
**Purpose**: Manages runtime data binding between views and data sources
**Key Attributes**:
- `List<ActiveBinding> ActiveBindings` - Currently active bindings
- `ChangeNotificationManager ChangeNotifications` - Property change handling
- `IValueConverterRegistry Converters` - Available value converters

**Relationships**:
- Monitors INotifyPropertyChanged sources
- Coordinates with Terminal.Gui view properties
- Manages binding lifecycle

### EventHandler
**Purpose**: Manages event routing from XAML to code-behind methods
**Key Attributes**:
- `Dictionary<string, MethodInfo> EventHandlers` - Mapped event methods
- `object Target` - Code-behind instance
- `EventHandlerCache Cache` - Performance optimization cache

## Build Integration Entities

### XamlBuildTask
**Purpose**: MSBuild task for XAML file processing and validation
**Key Attributes**:
- `ITaskItem[] XamlFiles` - Input XAML files
- `string OutputPath` - Generated file output location
- `bool EnableOptimizations` - Performance optimization flags
- `MSBuildLogger Logger` - Build process logging

**Relationships**:
- Integrates with MSBuild execution engine
- Coordinates with SourceGeneratorContext
- Produces build artifacts

### SourceGeneratorContext
**Purpose**: Context object for Roslyn source generator execution
**Key Attributes**:
- `Compilation Compilation` - Current compilation context
- `ImmutableArray<AdditionalText> XamlFiles` - XAML files to process
- `AnalyzerConfigOptions GlobalOptions` - MSBuild configuration
- `CancellationToken CancellationToken` - Cancellation support

## Performance and Caching Entities

### XamlParseCache
**Purpose**: Caches parsed XAML AST between builds for performance
**Key Attributes**:
- `Dictionary<string, CacheEntry> ParsedDocuments` - Cached parse results
- `TimeSpan CacheExpiry` - Cache invalidation timeout
- `long MaxCacheSize` - Memory usage limits

**Constitutional Compliance**:
- Ensures XAML parsing <100ms for cached documents
- Memory usage stays <50MB as per performance requirements

### PerformanceCounters
**Purpose**: Tracks constitutional compliance metrics during execution
**Key Attributes**:
- `TimeSpan ParseTime` - XAML parsing duration
- `long MemoryUsage` - Current memory consumption
- `double UIFrameRate` - Terminal.Gui rendering performance
- `TimeSpan InitializationTime` - Component startup time

**Validation Rules**:
- ParseTime must be <100ms for typical documents
- MemoryUsage must be <50MB for standard applications
- UIFrameRate must be >30 FPS
- InitializationTime must be <50ms

## Extension and Customization Entities

### MarkupExtension
**Purpose**: Base class for custom XAML markup extensions
**Key Attributes**:
- `string ExtensionName` - Markup extension identifier
- `Dictionary<string, object> Parameters` - Extension parameters
- `Type TargetType` - Expected return type

**Relationships**:
- Plugs into XamlParser extension points
- Integrates with code generation pipeline

### CustomControlDefinition
**Purpose**: Metadata for user-defined Terminal.Gui controls
**Key Attributes**:
- `Type ControlType` - .NET type of the control
- `string XamlName` - Element name in XAML
- `PropertyDescriptor[] Properties` - Available properties
- `EventDescriptor[] Events` - Available events
- `ContentPropertyDescriptor ContentProperty` - Default content property

**Relationships**:
- Extends ViewFactory registration
- Integrates with design-time services
- Enables XAML IntelliSense support