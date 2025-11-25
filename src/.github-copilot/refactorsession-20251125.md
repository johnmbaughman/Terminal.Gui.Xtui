# Refactor Session - November 25, 2025

## Summary
This session focused on enhancing the Terminal.Gui XAML parser to support XML comments, operator expressions for layout positioning, and refactoring to the standard `InitializeComponent` pattern.

## Changes Implemented

### 1. XML Comments Support
**File:** `XamlLoader.cs`

- Added support for XML comments in XAML files
- Comments are automatically ignored during parsing
- `XDocument.Parse` creates `XComment` nodes which are filtered out
- Only `XElement` and `XText` nodes are processed
- Added XML documentation to clarify comment handling

**Supported comment types:**
- Single-line comments: `<!-- comment -->`
- Multi-line comments
- Commented-out XML elements: `<!-- <Label ... /> -->`

### 2. Operator Expressions for Pos and Dim
**File:** `ObjectParsingHelpers.cs`

Added support for arithmetic operator expressions in XAML layout attributes:

**Pos operators:**
- Addition: `{Center + 5}` → `Pos.Center() + 5`
- Subtraction: `{Center - 10}` → `Pos.Center() - 10`
- Works with any valid Pos method: `{AnchorEnd - 25}` → `Pos.AnchorEnd() - 25`

**Dim operators:**
- Addition: `{Auto + 2}` → `Dim.Auto() + 2`
- Subtraction: `{Fill - 5}` → `Dim.Fill() - 5`

**Implementation details:**
- Added regex pattern: `@"^\{\s*(\w+)\s*([+\-])\s*(\d+)\s*\}$"`
- Operator expression parsing happens before general expression parsing
- Generates C# binary expressions using Roslyn SyntaxFactory
- Validates method names before generating code

### 3. InitializeComponent Pattern
**File:** `WindowGenerator.cs`

Refactored code generation to align with Windows Forms and WPF XAML standards:

**Before:**
```csharp
public MyWindow()
{
    this.Add(new Label() { ... });
    this.Add(new Button() { ... });
}
```

**After:**
```csharp
public MyWindow()
{
    InitializeComponent();
}

private void InitializeComponent()
{
    this.Add(new Label() { ... });
    this.Add(new Button() { ... });
}
```

**Key changes:**
- Removed constructor generation from `WindowGenerator.GenerateClass()`
- Constructor must be defined in the user's partial class
- Generated code only provides `InitializeComponent()` method
- Allows users to add custom initialization logic before/after `InitializeComponent()` call

### 4. Test XAML Updates
**File:** `Examples/Xaml/MyWindow.xaml`

Updated test file to demonstrate all new features:
```xml
<Window>
    <!-- This is a simple label -->
    <Label Text="Hello" X="10" Y="5" Width="20" Height="1" />
    
    <!-- Multi-line comment
         This button is centered -->
    <Button Text="Click Me" X="{Center}" Y="50%" Width="80%" Height="3" />
    
    <!-- Commented-out element -->
    <!-- <Label Text="This is commented out" X="0" Y="0" Width="10" Height="1" /> -->
    
    <!-- Testing Pos operator expressions -->
    <Label Text="Offset Left" X="{Center - 10}" Y="10" Width="20" Height="1" />
    <Label Text="Offset Right" X="{Center + 5}" Y="12" Width="20" Height="1" />
    <Label Text="From End" X="{AnchorEnd - 25}" Y="14" Width="20" Height="1" />
    
    <!-- Testing Dim operator expressions -->
    <Label Text="Fill Minus" X="5" Y="16" Width="{Fill - 5}" Height="1" />
    <Label Text="Auto Plus" X="5" Y="18" Width="{Auto + 2}" Height="1" />
    
    <Label Text="Anchored" X="{AnchorEnd}" Y="{AnchorEnd 5}" Width="{Auto}" Height="{Fill}" />
</Window>
```

## Generated Code Example

```csharp
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;

namespace Xaml
{
    public partial class MyWindow : Window
    {
        private void InitializeComponent()
        {
            this.Add(new Label() { Text = "Hello", X = 10, Y = 5, Width = 20, Height = 1 });
            this.Add(new Button() { Text = "Click Me", X = Pos.Center(), Y = Pos.Percent(50), Width = Dim.Percent(80), Height = 3 });
            this.Add(new Label() { Text = "Offset Left", X = Pos.Center() - 10, Y = 10, Width = 20, Height = 1 });
            this.Add(new Label() { Text = "Offset Right", X = Pos.Center() + 5, Y = 12, Width = 20, Height = 1 });
            this.Add(new Label() { Text = "From End", X = Pos.AnchorEnd() - 25, Y = 14, Width = 20, Height = 1 });
            this.Add(new Label() { Text = "Fill Minus", X = 5, Y = 16, Width = Dim.Fill() - 5, Height = 1 });
            this.Add(new Label() { Text = "Auto Plus", X = 5, Y = 18, Width = Dim.Auto() + 2, Height = 1 });
            this.Add(new Label() { Text = "Anchored", X = Pos.AnchorEnd(), Y = Pos.AnchorEnd(5), Width = Dim.Auto(), Height = Dim.Fill() });
        }
    }
}
```

## Build Results

Both example projects build successfully:
- **Xaml** project: Build succeeded with 14 warnings (Terminal.Gui related, not XAML)
- **Xaml.Mvvm** project: Build succeeded with 14 warnings (Terminal.Gui related, not XAML)

All warnings are from the Terminal.Gui project itself (XML comments, analyzers) and were explicitly excluded from fixes per user request.

## Technical Notes

### Parsing Order
The order of regex matching in `ParsePosExpression` and `ParseDimExpression` is critical:
1. Integer literals (e.g., `10`)
2. Percentage notation (e.g., `50%`)
3. **Operator expressions** (e.g., `{Center - 10}`) - Must be checked FIRST
4. General expression syntax (e.g., `{Center}`, `{AnchorEnd 5}`)

The operator expression regex must be evaluated before the general expression regex because the general pattern `@"^\{\s*(\w+)(?:\s+(.+?))?\s*\}$"` would match operator expressions and incorrectly parse them as method calls with arguments.

### Roslyn Code Generation
Operator expressions generate clean binary expression syntax:
```csharp
BinaryExpression(
    SyntaxKind.AddExpression,  // or SubtractExpression
    invocation,                 // Pos.Center()
    literalOffset)              // 10
```

This produces idiomatic C# code: `Pos.Center() - 10`

### Standards Alignment
The `InitializeComponent` pattern follows these conventions:
- **Windows Forms Designer:** `InitializeComponent()` method contains all designer-generated code
- **WPF XAML:** `InitializeComponent()` calls `LoadComponent()` to load XAML
- **Terminal.Gui XAML:** `InitializeComponent()` contains object initialization code

The pattern allows developers to:
- Add initialization logic before `InitializeComponent()` (e.g., service setup)
- Add initialization logic after `InitializeComponent()` (e.g., event handlers, data binding)
- Override behavior in derived classes
- Keep generated code separate from user code

## Future Enhancements

From the TODO comments in the code:
1. Handle complex Pos/Dim expressions with nested operators
2. Support view references in expressions: `Left(myView)`, `Right(myView)`
3. Handle Dim expressions with operators in arguments: `{Fill() - 10}`
4. Support more complex argument types beyond integers and quoted strings

## Files Modified

1. `Terminal.Gui.Xaml/XamlLoader.cs` - Added XML comment documentation
2. `Terminal.Gui.Xaml/Generators/ObjectParsingHelpers.cs` - Added operator expression parsing for Pos and Dim
3. `Terminal.Gui.Xaml/Generators/WindowGenerator.cs` - Refactored to InitializeComponent pattern
4. `Examples/Xaml/MyWindow.xaml` - Added test cases for comments and operator expressions
5. `Examples/Xaml.Mvvm/MyWindow.xaml` - Updated with new syntax (implicit from earlier session)

## Verification

All changes have been tested and verified:
- ✅ XML comments are properly ignored (no elements generated for commented-out markup)
- ✅ Operator expressions generate correct C# binary expressions
- ✅ InitializeComponent pattern compiles successfully
- ✅ Both example projects build without XAML-related errors
- ✅ Generated code is clean and idiomatic

## Session Context

This session built upon previous work that included:
- Object initializer refactoring
- PropertyTypes dictionary implementation
- Pos/Dim expression syntax (`{Center}`, `{AnchorEnd 5}`)
- Percentage notation (`50%`, `80%`)
- Error diagnostics (XAML001, XAML002)
- Removal of legacy method call syntax

The changes in this session complete the XAML parser's core feature set for Terminal.Gui layout positioning and align it with industry-standard code generation patterns.
