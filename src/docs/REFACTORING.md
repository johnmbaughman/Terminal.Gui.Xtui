# Generator Helper API Reference

**Project**: Terminal.Gui.Xtui  
**Created**: 2025-12-18  
**Status**: Phase 1 Complete

## Overview

This document describes the helper APIs available for generator refactoring. These helpers were extracted from duplicate generator code to improve maintainability, testability, and code quality.

The helper library is located at `src/Terminal.Gui.Xtui/Generators/Helpers/` and provides common functionality for:
- Type name manipulation and variable naming
- Roslyn syntax tree construction
- XML namespace handling
- Child element processing
- Class and method generation
- Field transformation (TopLevelGenerator patterns)

## Purpose

The refactoring effort (Feature 001) aims to:
- **Reduce code duplication** across generators by 40%+ (Success Criterion SC-004)
- **Improve maintainability** with well-tested, reusable helper methods
- **Maintain backward compatibility** (zero-tolerance for output changes)
- **Enable TDD** with comprehensive test coverage (>90% per helper)

## Helper Modules

### TypeNameHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/TypeNameHelpers.cs`  
**Purpose**: Type name manipulation and variable name generation

#### Key Methods

```csharp
// Extract simple type name from qualified name
string ExtractLocalTypeName(string fullTypeName)
// Example: "Terminal.Gui.Label" → "Label"

// Convert type name to camelCase variable name
string ToVariableName(string typeName)
// Example: "TextField" → "textField"

// Create indexed variable name
string CreateIndexedVariableName(string fullTypeName, int index)
// Example: "Terminal.Gui.Label", 0 → "label0"

// Get control variable name (with fallback to indexed name)
string GetControlVariableName(ElementNode element, string typeName, int index)
// Uses Id attribute if present, otherwise generates indexed name

// Extract namespace from qualified type name
string ExtractNamespace(string fullTypeName)
// Example: "Terminal.Gui.Label" → "Terminal.Gui"

// Check if type name is qualified (contains namespace)
bool IsQualifiedTypeName(string typeName)
// Example: "Terminal.Gui.Label" → true, "Label" → false
```

#### Usage Example

```csharp
var fullType = element.Name; // "Terminal.Gui.Button"
var simpleType = TypeNameHelpers.ExtractLocalTypeName(fullType); // "Button"
var varName = TypeNameHelpers.GetControlVariableName(element, simpleType, index);
// If element has Id="submitBtn" → "submitBtn"
// Otherwise → "button0"
```

### SyntaxHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/SyntaxHelpers.cs`  
**Purpose**: Roslyn syntax node construction for common patterns

#### Key Methods

```csharp
// Create object initialization expression
ObjectCreationExpressionSyntax CreateObjectWithInitializer(
    string typeName, 
    IEnumerable<AssignmentExpressionSyntax> initializers)

// Create local variable declaration
LocalDeclarationStatementSyntax CreateLocalDeclaration(
    string typeName,
    string variableName,
    ObjectCreationExpressionSyntax initializer)

// Create field declaration
FieldDeclarationSyntax CreateFieldDeclaration(
    string typeName,
    string fieldName,
    SyntaxKind accessModifier = SyntaxKind.PrivateKeyword)

// Create property assignment (obj.Property = value)
AssignmentExpressionSyntax CreatePropertyAssignment(
    string propertyName,
    ExpressionSyntax value)

// Create this.field assignment
ExpressionStatementSyntax CreateThisFieldAssignment(
    string fieldName,
    ExpressionSyntax value)

// Create Add invocation (container.Add(item))
ExpressionStatementSyntax CreateAddInvocation(
    string containerVar,
    string itemVar)
```

#### Usage Example

```csharp
// Create: var button0 = new Button { Text = "Click me" };
var assignments = new[] {
    SyntaxHelpers.CreatePropertyAssignment("Text", 
        SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression, 
            SyntaxFactory.Literal("Click me")))
};

var objectCreation = SyntaxHelpers.CreateObjectWithInitializer("Button", assignments);
var declaration = SyntaxHelpers.CreateLocalDeclaration("Button", "button0", objectCreation);
```

### NamespaceHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/NamespaceHelpers.cs`  
**Purpose**: XML namespace to C# namespace mapping

#### Key Methods

```csharp
// Collect required namespaces from element tree
HashSet<string> CollectRequiredNamespaces(ElementNode root)

// Map XML namespace URI to C# namespace
string MapXmlNamespaceUriToCSharp(string xmlNamespaceUri)
// Handles: clr-namespace:, terminal gui xtui default, custom URIs
```

#### Usage Example

```csharp
var requiredNamespaces = NamespaceHelpers.CollectRequiredNamespaces(rootElement);
// Returns: ["Terminal.Gui", "System", "MyApp.Controls"]

var csharpNs = NamespaceHelpers.MapXmlNamespaceUriToCSharp(
    "clr-namespace:MyApp.Controls;assembly=MyApp");
// Returns: "MyApp.Controls"
```

### ChildProcessingHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/ChildProcessingHelpers.cs`  
**Purpose**: Process child elements and generate container code

#### Key Methods

```csharp
// Get child elements (handles Content.Items pattern)
IEnumerable<ElementNode> GetContainerChildren(ElementNode element)

// Process children with Add statements
void ProcessChildrenWithAdd(
    ElementNode containerElement,
    string containerVarName,
    List<StatementSyntax> statements,
    ref int childIndex)
```

#### Usage Example

```csharp
var statements = new List<StatementSyntax>();
int childIndex = 0;

ChildProcessingHelpers.ProcessChildrenWithAdd(
    windowElement, 
    "window0", 
    statements, 
    ref childIndex);

// Generates: window0.Add(button0); window0.Add(label0); etc.
```

### ClassGenerationHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/ClassGenerationHelpers.cs`  
**Purpose**: Generate partial classes, methods, and compilation units

#### Key Methods

```csharp
// Build InitializeComponent method
MethodDeclarationSyntax BuildInitializeComponentMethod(
    IEnumerable<StatementSyntax> statements)

// Build partial class with fields and methods
ClassDeclarationSyntax BuildPartialClass(
    string className,
    IEnumerable<FieldDeclarationSyntax> fields,
    IEnumerable<MethodDeclarationSyntax> methods)

// Build complete compilation unit
CompilationUnitSyntax BuildCompilationUnit(
    string targetNamespace,
    IEnumerable<string> usingDirectives,
    ClassDeclarationSyntax classDeclaration)

// Add #nullable enable directive
CompilationUnitSyntax AddNullableDirective(CompilationUnitSyntax compilationUnit)
```

#### Usage Example

```csharp
var method = ClassGenerationHelpers.BuildInitializeComponentMethod(statements);
var partialClass = ClassGenerationHelpers.BuildPartialClass(
    "MainWindow", 
    fields, 
    new[] { method });
var compilationUnit = ClassGenerationHelpers.BuildCompilationUnit(
    "MyApp.Views",
    new[] { "Terminal.Gui", "System" },
    partialClass);
var withNullable = ClassGenerationHelpers.AddNullableDirective(compilationUnit);
```

### FieldTransformationHelpers

**Location**: `Terminal.Gui.Xtui/Generators/Helpers/FieldTransformationHelpers.cs`  
**Purpose**: Extract variable-to-field transformation patterns from TopLevelGenerator

#### Key Methods

```csharp
// Transform local variable declarations to field declarations
IEnumerable<FieldDeclarationSyntax> TransformVariablesToFields(
    IEnumerable<LocalDeclarationStatementSyntax> localDeclarations)

// Update variable references to field references (this.field)
StatementSyntax UpdateVariableReferencesToFields(
    StatementSyntax statement,
    HashSet<string> fieldNames)

// Apply complete variable-to-field transformation
TransformationResult ApplyFieldTransformation(
    IEnumerable<StatementSyntax> statements,
    IEnumerable<string> topLevelVariables)
```

#### Usage Example

```csharp
// Transform: var button0 = new Button(); 
// To: private Button button0;
//     this.button0 = new Button();

var result = FieldTransformationHelpers.ApplyFieldTransformation(
    statements,
    topLevelVariables);

// result.Fields contains field declarations
// result.Statements contains transformed statements with this. references
```

## Generator Refactoring Patterns

### Before Refactoring (Duplicate Code)

```csharp
public class ButtonGenerator
{
    public string GenerateClass(...)
    {
        // Duplicate: type name extraction
        var simpleType = element.Name.Contains('.') 
            ? element.Name.Split('.').Last() 
            : element.Name;
            
        // Duplicate: variable naming
        var varName = element.GetAttribute("Id") ?? 
            (char.ToLower(simpleType[0]) + simpleType.Substring(1) + index);
            
        // Duplicate: object creation syntax
        var objCreation = SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.IdentifierName(simpleType))
            .WithInitializer(...);
    }
}
```

### After Refactoring (Using Helpers)

```csharp
public class ButtonGenerator
{
    public string GenerateClass(...)
    {
        // Clean: use helper methods
        var simpleType = TypeNameHelpers.ExtractLocalTypeName(element.Name);
        var varName = TypeNameHelpers.GetControlVariableName(element, simpleType, index);
        var objCreation = SyntaxHelpers.CreateObjectWithInitializer(simpleType, initializers);
    }
}
```

## Testing Guidelines

All helper methods are thoroughly tested (>90% coverage, >95% for FieldTransformationHelpers).

### Test Structure

```
Terminal.Gui.Xtui.Tests/
  Generators.Helpers.Tests/
    TypeNameHelpersTests.cs
    SyntaxHelpersTests.cs
    NamespaceHelpersTests.cs
    ChildProcessingHelpersTests.cs
    ClassGenerationHelpersTests.cs
    FieldTransformationHelpersTests.cs
```

### Example Test Pattern

```csharp
[Fact]
public void ExtractLocalTypeName_QualifiedName_ReturnsSimpleName()
{
    // Arrange
    var fullType = "Terminal.Gui.Button";
    
    // Act
    var result = TypeNameHelpers.ExtractLocalTypeName(fullType);
    
    // Assert
    Assert.Equal("Button", result);
}
```

## Baseline Compatibility

**Critical**: The refactoring maintains byte-for-byte output compatibility with pre-refactor generators.

### Validation Process

1. **Baseline file**: `refactor/generated-baseline.cs` (authoritative reference)
2. **Integration test**: `BaselineGenerationTests.cs` validates exact output match
3. **CI enforcement**: Baseline diff must show zero differences

```powershell
# CI runs this check on every PR:
git --no-pager diff --no-index --ignore-cr-at-eol `
    refactor\generated-baseline.cs `
    artifacts\generated\generated-baseline.cs
```

**Zero-tolerance policy**: Any baseline diff is a BUG, not a rebaseline scenario.

## Performance Requirements

Refactoring must not regress generator performance:

- **Baseline**: Median generation time captured in CI artifacts
- **Threshold**: <10% regression vs baseline median
- **Measurement**: N=5 runs, median value, wall-clock milliseconds
- **CI enforcement**: Benchmark job fails if threshold exceeded

See `.github/workflows/benchmarks.yml` for implementation.

## Code Metrics & Progress

The refactoring tracks code quality improvements:

| Metric | Baseline | Target | Current |
|--------|----------|--------|---------|
| Duplicate Code | 100% | -40% | See CI artifacts |
| Test Coverage | ~60% | >90% | >95% (helpers) |
| Lines of Code (Generators/) | Baseline | -40% | See CI artifacts |
| Helper Test Count | 0 | >200 | 200+ |

Metrics are captured in benchmark artifacts (`artifacts/benchmarks/summary.json`).

## Migration Guide

### For New Generators

When creating a new generator:

1. Import helper namespace:
   ```csharp
   using Terminal.Gui.Xtui.Generators.Helpers;
   ```

2. Use helpers instead of duplicating logic:
   - Type names → `TypeNameHelpers`
   - Syntax trees → `SyntaxHelpers`
   - Namespaces → `NamespaceHelpers`
   - Child processing → `ChildProcessingHelpers`
   - Class generation → `ClassGenerationHelpers`

3. Follow TDD pattern:
   - Write failing tests first
   - Implement to make tests pass
   - Verify baseline diff shows zero changes

### For Existing Generators

When refactoring an existing generator:

1. **Create test branch** following stacked PR workflow
2. **Capture baseline** before changes
3. **Replace duplicate code** with helper calls
4. **Run tests** and verify baseline parity
5. **Run benchmarks** and verify <10% regression
6. **Open PR** with test evidence and artifact links

See [merge_guidelines.md](../checklists/merge_guidelines.md) for complete workflow.

## Common Pitfalls

### Pitfall 1: Changing Output Format

❌ **Wrong**: Modify helper to "improve" formatting
```csharp
// This changes output! Don't do this during refactor:
return $"{namespace}.{typeName}"; // Original was: namespace + "." + typeName
```

✅ **Right**: Extract exact existing logic
```csharp
// Match original behavior exactly:
return namespace + "." + typeName;
```

### Pitfall 2: Over-Engineering Helpers

❌ **Wrong**: Create complex, flexible helper APIs
```csharp
public static SyntaxNode CreateStatement(
    StatementType type, 
    StatementConfig config, 
    params Option[] options) { ... }
```

✅ **Right**: Keep helpers focused and simple
```csharp
public static LocalDeclarationStatementSyntax CreateLocalDeclaration(
    string typeName,
    string variableName,
    ObjectCreationExpressionSyntax initializer) { ... }
```

### Pitfall 3: Skipping Tests

❌ **Wrong**: Extract helper without tests
```csharp
// Helper exists but no tests → can't verify behavior
```

✅ **Right**: Write tests first (TDD)
```csharp
[Fact]
public void Helper_EdgeCase_ReturnsExpectedValue() { ... }
```

## References

- **Feature Specification**: `specs/001-refactor-generators/spec.md`
- **Implementation Plan**: `specs/001-refactor-generators/plan.md`
- **Task List**: `specs/001-refactor-generators/tasks.md`
- **Merge Guidelines**: `specs/001-refactor-generators/checklists/merge_guidelines.md`
- **Constitution Check**: `specs/001-refactor-generators/CONSTITUTION_CHECK.md`

## Support

For questions about helper usage or refactoring guidelines:

1. Review this document and plan.md
2. Check existing generator refactors (ButtonGenerator, etc.)
3. Run helper tests to understand behavior
4. Consult merge_guidelines.md for workflow questions

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18  
**Phase**: 1 Complete (Helpers Extraction)
