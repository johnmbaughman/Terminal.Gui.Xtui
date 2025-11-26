# WindowGenerator.cs - Detailed Analysis

## Overview

The `WindowGenerator` class is a Roslyn-based source generator that transforms XTUI markup into C# code for Terminal.Gui applications. It uses the **Microsoft.CodeAnalysis.CSharp.SyntaxFactory** API to programmatically construct C# syntax trees, which are then converted into source code.

This document provides a comprehensive, verbose explanation of how the Roslyn API is used within this generator, perfect for learning about code generation with Roslyn.

---

## Purpose and Context

### What Does This Generator Do?

Given a XTUI file like:
```xml
<Window>
    <Label Text="Hello" />
    <Button Text="Click Me" />
</Window>
```

And a partial class definition:
```csharp
public partial class MyWindow { }
```

The `WindowGenerator` generates the other half of the partial class:
```csharp
using Terminal.Gui.Views;

namespace Xtui
{
    public partial class MyWindow : Window
    {
        public MyWindow()
        {
            var label0 = new Label();
            label0.Text = "Hello";
            this.Add(label0);
            var button1 = new Button();
            button1.Text = "Click Me";
            this.Add(button1);
        }
    }
}
```

This enables developers to design UI in XTUI while Terminal.Gui remains code-first, with the XTUI being transformed into the imperative C# code that Terminal.Gui expects.

---

## Imports and Static Using Directive

```csharp
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
```

### Key Import: `using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;`

This is a **static using directive** that imports all static methods from the `SyntaxFactory` class. This is crucial because:

1. **SyntaxFactory** is the factory class that creates all C# syntax nodes
2. The `static` import means we can call methods like `LocalDeclarationStatement()` instead of `SyntaxFactory.LocalDeclarationStatement()`
3. This dramatically improves readability when building complex syntax trees

**Without static using:**
```csharp
var statement = SyntaxFactory.LocalDeclarationStatement(
    SyntaxFactory.VariableDeclaration(
        SyntaxFactory.IdentifierName("var")));
```

**With static using:**
```csharp
var statement = LocalDeclarationStatement(
    VariableDeclaration(
        IdentifierName("var")));
```

---

## Class Structure

```csharp
internal sealed class WindowGenerator : Generator
```

- **internal**: Only visible within the `Terminal.Gui.Xtui` assembly
- **sealed**: Cannot be inherited (optimization and design decision)
- **Inherits from Generator**: Abstract base class that defines the code generation contract

The class has two main methods:
1. `GenerateStatements()` - Creates statements to instantiate a Window as part of a larger code block
2. `GenerateClass()` - Generates a complete C# class that inherits from Window

---

## Method 1: GenerateStatements()

This method generates C# statements to create and configure a Window instance within existing code.

### Method Signature

```csharp
public override StatementSyntax[] GenerateStatements(
    ElementNode node,           // The parsed XTUI element
    string variableName,        // Name for the variable to create
    IGeneratorFactory generators // Factory to get generators for child elements
)
```

### Understanding the ElementNode Parameter

The `ElementNode` represents the parsed XTUI structure:
```csharp
public class ElementNode
{
    public string Name { get; set; }                            // "Window", "Label", etc.
    public Dictionary<string, string> Attributes { get; }       // XML attributes (Text="Hello")
    public List<ElementNode> Children { get; }                  // Nested elements
    public string InnerText { get; set; }                       // Text content
}
```

---

## Step 1: Variable Declaration Statement

### The Code Being Generated

```csharp
var window = new Window();
```

### The Roslyn Code to Generate It

```csharp
LocalDeclarationStatement(
    VariableDeclaration(
            IdentifierName("var"))
        .WithVariables(
            SingletonSeparatedList(
                VariableDeclarator(
                        Identifier(variableName))
                    .WithInitializer(
                        EqualsValueClause(
                            ObjectCreationExpression(
                                    IdentifierName("Window"))
                                .WithArgumentList(ArgumentList()))))))
```

### Breaking Down Each SyntaxFactory Call

#### 1. `LocalDeclarationStatement(...)`
Creates a local variable declaration statement (any statement that declares a variable inside a method).

**Type:** Returns `LocalDeclarationStatementSyntax`

#### 2. `VariableDeclaration(IdentifierName("var"))`
Creates the declaration part, specifying the type (in this case, the `var` keyword).

**Parameters:**
- `IdentifierName("var")` - Creates a type syntax node representing the `var` keyword

**Type:** Returns `VariableDeclarationSyntax`

**Think of it as:** The "var" part in `var x = ...`

#### 3. `.WithVariables(...)`
Adds the variable declarator(s) to the declaration. You can declare multiple variables in one statement (e.g., `var x = 1, y = 2`), so this accepts a list.

**Parameters:**
- `SingletonSeparatedList(...)` - Creates a list with exactly one element (no commas needed)

**Type:** Modifies and returns `VariableDeclarationSyntax`

#### 4. `VariableDeclarator(Identifier(variableName))`
Creates the actual variable being declared.

**Parameters:**
- `Identifier(variableName)` - Creates an identifier token with the given name (e.g., "window")

**Type:** Returns `VariableDeclaratorSyntax`

**Think of it as:** The "x" part in `var x = ...`

#### 5. `.WithInitializer(...)`
Adds the initialization expression to the variable declarator.

**Parameters:**
- `EqualsValueClause(...)` - Represents the `= value` part

**Type:** Modifies and returns `VariableDeclaratorSyntax`

#### 6. `EqualsValueClause(...)`
Creates the equals sign and the value being assigned.

**Type:** Returns `EqualsValueClauseSyntax`

**Think of it as:** The `= new Window()` part

#### 7. `ObjectCreationExpression(IdentifierName("Window"))`
Creates a `new TypeName()` expression.

**Parameters:**
- `IdentifierName("Window")` - The type being instantiated

**Type:** Returns `ObjectCreationExpressionSyntax`

**Think of it as:** The `new Window` part (without parentheses yet)

#### 8. `.WithArgumentList(ArgumentList())`
Adds the constructor argument list (the parentheses).

**Parameters:**
- `ArgumentList()` - Creates an empty argument list `()`

**Type:** Modifies and returns `ObjectCreationExpressionSyntax`

**Complete:** Now we have `new Window()`

### The Fluent API Pattern

Notice the **"With" pattern** used throughout:
- `.WithVariables(...)`
- `.WithInitializer(...)`
- `.WithArgumentList(...)`

This is because syntax nodes in Roslyn are **immutable**. Each "With" method returns a new instance with the specified property set. This pattern is similar to C# record types.

---

## Step 2: Setting Properties from XTUI Attributes

### The XTUI

```xml
<Window Title="Main Window" Width="80">
```

### The Generated Code

```csharp
window.Title = "Main Window";
window.Width = "80";
```

### The Roslyn Generation Code

```csharp
if (node.Attributes.Count > 0)
{
    statements.AddRange(node.Attributes
        .Select(attr => ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, 
                    IdentifierName(variableName), 
                    IdentifierName(attr.Key)), 
                LiteralExpression(
                    SyntaxKind.StringLiteralExpression, 
                    Literal(attr.Value))))));
}
```

### Breaking This Down

#### 1. LINQ Transformation
```csharp
node.Attributes.Select(attr => ...)
```

For each attribute in the XTUI element, we generate one assignment statement. This uses LINQ to transform the `Dictionary<string, string>` of attributes into syntax nodes.

#### 2. `ExpressionStatement(...)`
Wraps an expression in a statement context. 

**Why?** In C#, `window.Title = "Hello";` is an **expression statement** (an expression followed by a semicolon that forms a complete statement).

**Type:** Returns `ExpressionStatementSyntax`

#### 3. `AssignmentExpression(...)`
Creates an assignment expression.

**Parameters:**
1. `SyntaxKind.SimpleAssignmentExpression` - The kind of assignment (`=`, `+=`, `-=`, etc.). We use simple `=`.
2. **Left side** - The target being assigned to
3. **Right side** - The value being assigned

**Type:** Returns `AssignmentExpressionSyntax`

**Structure:** `left = right`

#### 4. `MemberAccessExpression(...)`
Creates a member access expression (accessing a property or field).

**Parameters:**
1. `SyntaxKind.SimpleMemberAccessExpression` - The kind of access (could be `?.` for null-conditional)
2. **Expression** - The object being accessed (left of the dot)
3. **Name** - The member being accessed (right of the dot)

**Type:** Returns `MemberAccessExpressionSyntax`

**Example:** For `window.Title`, this creates the access to the `Title` property on the `window` variable.

#### 5. `IdentifierName(variableName)` and `IdentifierName(attr.Key)`
- First one: Creates the identifier for the variable (e.g., "window")
- Second one: Creates the identifier for the property name (e.g., "Title")

**Type:** Returns `IdentifierNameSyntax`

#### 6. `LiteralExpression(...)`
Creates a literal value expression.

**Parameters:**
1. `SyntaxKind.StringLiteralExpression` - The kind of literal (string, number, boolean, etc.)
2. `Literal(attr.Value)` - The actual literal token with the value

**Type:** Returns `LiteralExpressionSyntax`

**Example:** Creates `"Main Window"` (including the quotes)

### Complete Transformation for One Attribute

For attribute `Title="Main Window"`:

1. `Literal("Main Window")` → Token representing `"Main Window"`
2. `LiteralExpression(StringLiteral, ...)` → Expression: `"Main Window"`
3. `IdentifierName("window")` → Expression: `window`
4. `IdentifierName("Title")` → Identifier: `Title`
5. `MemberAccessExpression(Simple, window, Title)` → Expression: `window.Title`
6. `AssignmentExpression(Simple, window.Title, "Main Window")` → Expression: `window.Title = "Main Window"`
7. `ExpressionStatement(...)` → Statement: `window.Title = "Main Window";`

---

## Step 3: Processing Child Elements

### The XTUI

```xml
<Window>
    <Label Text="Hello" />
    <Button Text="Click Me" />
</Window>
```

### The Generated Code

```csharp
var label0 = new Label();
label0.Text = "Hello";
window.Add(label0);
var button1 = new Button();
button1.Text = "Click Me";
window.Add(button1);
```

### The Roslyn Code

```csharp
if (node.Children.Count <= 0) return statements.ToArray();

for (var i = 0; i < node.Children.Count; i++)
{
    var child = node.Children[i];
    var childVarName = $"{child.Name.ToLower()}{i}";
    var childGenerator = generators.GetGenerator(child.Name);
    var childStatements = childGenerator.GenerateStatements(child, childVarName, generators);
    
    statements.AddRange(childStatements);

    statements.Add(
        ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(variableName),
                        IdentifierName("Add")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(IdentifierName(childVarName)))))));
}
```

### Breaking Down the Child Processing

#### 1. Variable Naming
```csharp
var childVarName = $"{child.Name.ToLower()}{i}";
```

Creates unique variable names: `label0`, `button1`, etc. The index ensures uniqueness even with multiple elements of the same type.

#### 2. Recursive Generator Pattern
```csharp
var childGenerator = generators.GetGenerator(child.Name);
var childStatements = childGenerator.GenerateStatements(child, childVarName, generators);
statements.AddRange(childStatements);
```

This is the **Strategy Pattern** in action:
- Each element type (Window, Label, Button) has its own generator
- `GeneratorFactory` provides the appropriate generator based on the element name
- Generators recursively call each other for nested elements
- Each child generator returns an array of statements

For a `Label`, the `LabelGenerator` would return statements like:
```csharp
[
    var label0 = new Label();,
    label0.Text = "Hello";
]
```

These statements are added to the parent's statement list.

#### 3. The Add() Method Invocation

Now we need to generate: `window.Add(label0);`

Let's build this from the inside out:

##### a) `IdentifierName(childVarName)` 
Creates: `label0`

**Type:** `IdentifierNameSyntax`

##### b) `Argument(...)`
Wraps the identifier as an argument to a method call.

**Type:** `ArgumentSyntax`

**Think of it as:** Preparing `label0` to be passed as an argument

##### c) `SingletonSeparatedList(...)`
Creates a list containing just this one argument. If there were multiple arguments, we'd use `SeparatedList` with comma separators.

**Type:** `SeparatedSyntaxList<ArgumentSyntax>`

##### d) `ArgumentList(...)`
Wraps the argument list in parentheses.

**Type:** `ArgumentListSyntax`

**Represents:** `(label0)`

##### e) `MemberAccessExpression(...)`
Creates the member access to the `Add` method on the window variable.

**Parameters:**
- `SyntaxKind.SimpleMemberAccessExpression` - Regular dot access
- `IdentifierName(variableName)` - The `window` identifier
- `IdentifierName("Add")` - The `Add` method name

**Type:** `MemberAccessExpressionSyntax`

**Represents:** `window.Add`

##### f) `InvocationExpression(...)`
Converts the member access into a method call by adding the argument list.

**Type:** `InvocationExpressionSyntax`

**Represents:** `window.Add(label0)`

##### g) `.WithArgumentList(...)`
Attaches the argument list to the invocation.

**Complete:** `window.Add(label0)`

##### h) `ExpressionStatement(...)`
Wraps it as a statement.

**Final Result:** `window.Add(label0);`

---

## Method 2: GenerateClass()

This method generates a complete C# class file, not just statements. It's used by the main `CodeGenerator` to produce the `.g.cs` files.

### Method Signature

```csharp
public override string GenerateClass(
    ElementNode node, 
    string namespaceName, 
    string className, 
    IGeneratorFactory generators
)
```

This method builds the entire class structure from the bottom up:
1. Constructor body statements
2. Constructor declaration
3. Class declaration with base type
4. Namespace declaration
5. Compilation unit with usings
6. Convert to string

---

## Building the Constructor Body

```csharp
var constructorStatements = new List<StatementSyntax>();

if (node.Children.Count > 0)
{
    for (var i = 0; i < node.Children.Count; i++)
    {
        var child = node.Children[i];
        var childVarName = $"{child.Name.ToLower()}{i}";
        var childGenerator = generators.GetGenerator(child.Name);
        var childStatements = childGenerator.GenerateStatements(child, childVarName, generators);
        
        constructorStatements.AddRange(childStatements);

        constructorStatements.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName("Add")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(IdentifierName(childVarName)))))));
    }
}
```

### Key Difference from GenerateStatements()

Notice `ThisExpression()` instead of `IdentifierName(variableName)`:
- In `GenerateStatements()`, we add to a variable: `window.Add(label0)`
- In `GenerateClass()`, we're inside the Window class itself, so we add to `this`: `this.Add(label0)`

#### `ThisExpression()`
Creates the `this` keyword as an expression.

**Type:** Returns `ThisExpressionSyntax`

**Example:** Used in `this.Add(label0)` to reference the current instance

---

## Building the Constructor Declaration

### The Goal

```csharp
public MyWindow()
{
    // constructor body statements
}
```

### The Roslyn Code

```csharp
var constructor = ConstructorDeclaration(className)
    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
    .WithParameterList(ParameterList())
    .WithBody(Block(constructorStatements));
```

### Breaking It Down

#### 1. `ConstructorDeclaration(className)`
Creates a constructor declaration.

**Parameters:**
- `className` - The name of the constructor (must match the class name)

**Type:** Returns `ConstructorDeclarationSyntax`

**Note:** In C#, constructors are special methods with the same name as the class

#### 2. `.WithModifiers(...)`
Adds access modifiers to the constructor.

**Parameters:**
- `TokenList(...)` - A list of modifier tokens

**Type:** Modifies and returns `ConstructorDeclarationSyntax`

#### 3. `TokenList(Token(SyntaxKind.PublicKeyword))`
Creates a list containing the `public` keyword.

**Breaking it down further:**
- `Token(SyntaxKind.PublicKeyword)` - Creates a token representing `public`
- `TokenList(...)` - Wraps it in a syntax token list

**Type:** Returns `SyntaxTokenList`

**Note:** You could have multiple modifiers: `TokenList(Token(Public), Token(Static))`

#### 4. `.WithParameterList(ParameterList())`
Adds the parameter list (the parentheses).

**Parameters:**
- `ParameterList()` - Creates an empty parameter list `()`

**Type:** Modifies and returns `ConstructorDeclarationSyntax`

**Note:** For constructors with parameters, you'd populate the `ParameterList` with `Parameter` nodes

#### 5. `.WithBody(Block(constructorStatements))`
Adds the constructor body.

**Parameters:**
- `Block(...)` - Creates a block statement (code between `{` and `}`)

**Type:** Modifies and returns `ConstructorDeclarationSyntax`

#### 6. `Block(constructorStatements)`
Creates a block containing the list of statements.

**Type:** Returns `BlockSyntax`

**Represents:** The `{ ... }` with all the statements inside

---

## Building the Class Declaration

### The Goal

```csharp
public partial class MyWindow : Window
{
    // constructor here
}
```

### The Roslyn Code

```csharp
var classDeclaration = ClassDeclaration(className)
    .WithModifiers(TokenList(
        Token(SyntaxKind.PublicKeyword),
        Token(SyntaxKind.PartialKeyword)))
    .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
        SimpleBaseType(IdentifierName("Window")))))
    .WithMembers(SingletonList<MemberDeclarationSyntax>(constructor));
```

### Breaking It Down

#### 1. `ClassDeclaration(className)`
Creates a class declaration.

**Parameters:**
- `className` - The name of the class (e.g., "MyWindow")

**Type:** Returns `ClassDeclarationSyntax`

#### 2. `.WithModifiers(TokenList(...))`
Adds the `public` and `partial` modifiers.

**Parameters:**
- `TokenList(Token(PublicKeyword), Token(PartialKeyword))` - Multiple tokens in a list

**Type:** Modifies and returns `ClassDeclarationSyntax`

**Note:** The `partial` keyword is essential because the user writes one part of the class, and this generator produces the other part. The C# compiler merges all partial declarations of the same class.

#### 3. `.WithBaseList(...)`
Specifies the base class and/or interfaces.

**Type:** Modifies and returns `ClassDeclarationSyntax`

#### 4. `BaseList(...)`
Creates the list of base types (the `: Window` part).

**Type:** Returns `BaseListSyntax`

#### 5. `SingletonSeparatedList<BaseTypeSyntax>(...)`
Creates a list with one base type. Classes can have multiple items here (one base class and multiple interfaces).

**Generic Parameter:** `BaseTypeSyntax` - The base type for all inheritance list items

**Type:** Returns `SeparatedSyntaxList<BaseTypeSyntax>`

#### 6. `SimpleBaseType(IdentifierName("Window"))`
Creates a simple base type reference.

**Parameters:**
- `IdentifierName("Window")` - The name of the base class

**Type:** Returns `SimpleBaseTypeSyntax`

**Note:** "Simple" means it's just a type name, not a complex generic type like `List<T>`

#### 7. `.WithMembers(...)`
Adds the members of the class (constructors, methods, properties, fields, etc.).

**Parameters:**
- `SingletonList<MemberDeclarationSyntax>(constructor)` - A list containing just the constructor

**Type:** Modifies and returns `ClassDeclarationSyntax`

**Note:** More complex generators might add multiple members (properties for XTUI elements, methods for event handlers, etc.)

---

## Building the Namespace Declaration

### The Goal

```csharp
namespace Xtui
{
    // class here
}
```

### The Roslyn Code

```csharp
var namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
    .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
```

### Breaking It Down

#### 1. `NamespaceDeclaration(...)`
Creates a namespace declaration.

**Parameters:**
- `IdentifierName(namespaceName)` - The namespace name (could be complex like `Company.Product.Feature`)

**Type:** Returns `NamespaceDeclarationSyntax`

**Note:** For nested namespaces like `A.B.C`, you'd use `QualifiedName` to build the hierarchy

#### 2. `.WithMembers(...)`
Adds the members of the namespace (typically classes, interfaces, structs, enums).

**Parameters:**
- `SingletonList<MemberDeclarationSyntax>(classDeclaration)` - The class we created

**Type:** Modifies and returns `NamespaceDeclarationSyntax`

---

## Building the Compilation Unit (The Complete File)

### The Goal

```csharp
using Terminal.Gui.Views;

namespace Xtui
{
    // class and constructor here
}
```

### The Roslyn Code

```csharp
var compilationUnit = CompilationUnit()
    .WithUsings(List(new[]
    {
        UsingDirective(QualifiedName(
            QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
            IdentifierName("Views")))
    }))
    .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
    .NormalizeWhitespace();

return compilationUnit.ToFullString();
```

### Breaking It Down

#### 1. `CompilationUnit()`
Creates the root node of a C# syntax tree - represents an entire source file.

**Type:** Returns `CompilationUnitSyntax`

**Note:** This is always the top-level node in a C# syntax tree

#### 2. `.WithUsings(...)`
Adds using directives to the top of the file.

**Type:** Modifies and returns `CompilationUnitSyntax`

#### 3. `List(new[] { ... })`
Creates a syntax list from an array. Different from `SeparatedList` - this doesn't need separators because using directives don't have commas between them.

**Type:** Returns `SyntaxList<UsingDirectiveSyntax>`

#### 4. `UsingDirective(...)`
Creates a using directive.

**Type:** Returns `UsingDirectiveSyntax`

#### 5. `QualifiedName(...)`
Creates a qualified (dotted) name like `Terminal.Gui.Views`.

**Parameters:**
- **Left side** - The left part of the dotted name
- **Right side** - The right part (an identifier)

**Type:** Returns `QualifiedNameSyntax`

**Structure:** For `Terminal.Gui.Views`:
```csharp
QualifiedName(
    QualifiedName(
        IdentifierName("Terminal"),  // Left of first dot
        IdentifierName("Gui")),      // Right of first dot: Terminal.Gui
    IdentifierName("Views"))         // Right of second dot: Terminal.Gui.Views
```

This is a recursive structure building from left to right.

#### 6. `.WithMembers(...)`
Adds top-level members (typically namespace declarations, or in C# 10+, could be top-level statements or classes).

**Parameters:**
- `SingletonList<MemberDeclarationSyntax>(namespaceDeclaration)` - The namespace we created

**Type:** Modifies and returns `CompilationUnitSyntax`

#### 7. `.NormalizeWhitespace()`
This is a crucial formatting step!

**What it does:** Adds proper indentation, newlines, and spacing to make the syntax tree readable as C# code.

**Type:** Modifies and returns `CompilationUnitSyntax`

**Without NormalizeWhitespace:**
```csharp
namespace Xtui{public partial class MyWindow:Window{public MyWindow(){var label0=new Label();}}}
```

**With NormalizeWhitespace:**
```csharp
namespace Xtui
{
    public partial class MyWindow : Window
    {
        public MyWindow()
        {
            var label0 = new Label();
        }
    }
}
```

**Note:** You can customize the whitespace formatting by passing an `ElasticWhitespace` or custom trivia

#### 8. `compilationUnit.ToFullString()`
Converts the syntax tree to a string representation.

**Type:** Returns `string`

**Note:** There's also `.ToString()` which only includes the non-trivia tokens, but `ToFullString()` includes all whitespace, comments, etc.

---

## Advanced Roslyn Concepts Used

### 1. Immutability
All syntax nodes are immutable. The `With*` methods return new instances:

```csharp
var node1 = ClassDeclaration("MyClass");
var node2 = node1.WithModifiers(...);
// node1 is unchanged; node2 is a new instance with modifiers
```

**Benefits:**
- Thread-safe
- Can reuse nodes
- Clear data flow
- Enables undo/redo scenarios

### 2. Syntax Kinds (SyntaxKind Enum)
The `SyntaxKind` enum specifies the exact type of syntax element:

- `SyntaxKind.PublicKeyword` - The `public` keyword
- `SyntaxKind.SimpleAssignmentExpression` - The `=` operator
- `SyntaxKind.SimpleMemberAccessExpression` - The `.` operator
- `SyntaxKind.StringLiteralExpression` - A string literal

**Why?** Many syntax constructs have variations:
- Assignment: `=`, `+=`, `-=`, `*=`, etc.
- Member access: `.`, `?.` (null-conditional)
- This specificity gives you complete control over the generated code

### 3. Separated Lists vs Regular Lists
**SeparatedSyntaxList:** Used when items are separated by tokens (like commas):
- Method arguments: `(arg1, arg2, arg3)`
- Array elements: `{ 1, 2, 3 }`

**SyntaxList:** Used when items are not separated:
- Using directives (each on its own line)
- Class members

### 4. Trivia
"Trivia" in Roslyn refers to whitespace, comments, and preprocessor directives - things that don't affect the meaning of code but affect its formatting.

`NormalizeWhitespace()` adds appropriate trivia to make code readable.

### 5. Tokens vs Nodes
- **Tokens** - Atomic lexical elements (keywords, identifiers, operators)
  - Examples: `public`, `class`, `=`, `myVariable`
- **Nodes** - Structural elements composed of tokens and other nodes
  - Examples: `ClassDeclaration`, `MethodDeclaration`, `IfStatement`

---

## The Complete Generation Pipeline

Let's trace the complete flow for our example XTUI:

```xml
<Window>
    <Label Text="Hello" />
</Window>
```

### Step-by-Step Execution

1. **CodeGenerator** (main generator) reads the XTUI file
2. **XtuiLoader** parses XTUI into an `ElementNode` tree:
   ```
   ElementNode { Name: "Window", Children: [
       ElementNode { Name: "Label", Attributes: { "Text": "Hello" } }
   ]}
   ```

3. **CodeGenerator** calls `GenerateClass()` on `WindowGenerator`

4. **WindowGenerator.GenerateClass()** processes the root Window:
   - Iterates through children (the Label)
   - Gets `LabelGenerator` from factory
   - Calls `LabelGenerator.GenerateStatements()` for the Label child

5. **LabelGenerator.GenerateStatements()** returns:
   ```csharp
   [
       var label0 = new Label();,
       label0.Text = "Hello";
   ]
   ```

6. **WindowGenerator** adds these statements to constructor body

7. **WindowGenerator** generates the `this.Add(label0);` statement

8. **WindowGenerator** builds the complete syntax tree:
   - Constructor with the statements
   - Class with the constructor
   - Namespace with the class
   - CompilationUnit with using directives and namespace

9. **WindowGenerator** calls `.NormalizeWhitespace().ToFullString()`

10. **CodeGenerator** receives the string and adds it as source to the compilation

---

## Key Design Patterns

### 1. Factory Pattern
`GeneratorFactory` creates the appropriate generator for each element type:
```csharp
var generator = generatorFactory.GetGenerator("Label"); // Returns LabelGenerator
```

### 2. Composite Pattern
The `ElementNode` tree represents a hierarchy:
```csharp
Window
├── Label
└── Button
    └── Icon (hypothetical)
```

### 3. Builder Pattern
SyntaxFactory uses a fluent builder pattern:
```csharp
ClassDeclaration("MyClass")
    .WithModifiers(...)
    .WithBaseList(...)
    .WithMembers(...)
```

### 4. Strategy Pattern
Each generator implements a different strategy for generating code, but all follow the same interface.

### 5. Visitor Pattern (implicit)
While not explicitly implemented as a visitor, the recursive traversal of the `ElementNode` tree follows visitor-like patterns.

---

## Common Pitfalls and Best Practices

### Pitfall 1: Forgetting NormalizeWhitespace()
Without it, your generated code is unreadable:
```csharp
namespace Xtui{public class MyWindow{}}
```

**Solution:** Always call `.NormalizeWhitespace()` before `.ToFullString()`

### Pitfall 2: Wrong List Type
Using `SeparatedList` when you need `List` or vice versa causes compilation errors.

**Solution:** 
- Use `SeparatedSyntaxList` for comma-separated items (arguments, parameters)
- Use `SyntaxList` for non-separated items (using directives, members)

### Pitfall 3: Incorrect SyntaxKind
Using `SyntaxKind.AddAssignmentExpression` when you meant `SyntaxKind.SimpleAssignmentExpression`:
```csharp
x += 1  // AddAssignmentExpression
x = 1   // SimpleAssignmentExpression
```

**Solution:** Use the Roslyn Syntax Visualizer tool to see the exact `SyntaxKind` values

### Pitfall 4: Building Qualified Names Wrong
For `A.B.C`, you might try:
```csharp
QualifiedName(
    IdentifierName("A"), 
    IdentifierName("B"), 
    IdentifierName("C"))  // WRONG! Too many parameters
```

**Solution:** Nest them:
```csharp
QualifiedName(
    QualifiedName(IdentifierName("A"), IdentifierName("B")),
    IdentifierName("C"))
```

### Best Practice 1: Use Static Using for SyntaxFactory
Makes code much more readable:
```csharp
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
```

### Best Practice 2: Build Bottom-Up
Start with the innermost expressions and work your way out:
1. Identifiers and literals
2. Expressions
3. Statements
4. Members
5. Declarations

### Best Practice 3: Test with Roslyn Syntax Visualizer
Visual Studio has a Syntax Visualizer (View → Other Windows → Syntax Visualizer) that shows the exact structure of any C# code. Use it to understand how to build complex syntax trees.

### Best Practice 4: Use Roslyn Quoter
The [Roslyn Quoter](https://roslynquoter.azurewebsites.net/) website lets you paste C# code and see the SyntaxFactory API calls needed to generate it. Invaluable for learning!

---

## Performance Considerations

### 1. Object Creation
Creating syntax nodes is relatively cheap, but creating thousands of them can add up. The generator is designed to:
- Run only when XTUI files change (incremental generation)
- Process files in parallel (handled by the source generator framework)

### 2. String Building
`ToFullString()` builds a string representation of the entire syntax tree. For large files, this can be memory-intensive. The framework handles this efficiently by using `SourceText` instead of plain strings.

### 3. Immutability Overhead
While immutability is beneficial, it means creating new objects for each modification. The Roslyn team has optimized this with internal object pooling and structural sharing.

---

## Debugging Generated Code

### Viewing Generated Files
Generated files appear in:
```
obj/Generated/Terminal.Gui.Xtui/Terminal.Gui.Xtui.CodeGenerator/MyWindow.g.cs
```

### Debugging Techniques

1. **Add diagnostic comments:**
```csharp
var comment = Comment($"// Processing {node.Name}");
// Add comment as trivia to statements
```

2. **Emit error markers:**
```csharp
catch (Exception ex)
{
    spc.AddSource(
        "Error.g.cs", 
        $"// Error: {ex.Message}");
}
```

3. **Use the debugger:**
Attach to the `VBCSCompiler.exe` or `dotnet.exe` process that's running the source generator

4. **Log to file:**
```csharp
System.IO.File.AppendAllText(
    @"C:\temp\generator-log.txt", 
    $"Generated: {className}\n");
```

---

## Extending the Generator

### Adding Support for More Properties

To handle different property types (not just strings):

```csharp
// For bool properties
LiteralExpression(
    SyntaxKind.TrueLiteralExpression)  // or FalseLiteralExpression

// For numbers
LiteralExpression(
    SyntaxKind.NumericLiteralExpression,
    Literal(42))

// For complex expressions
ParseExpression("new Pos(10)")  // Parse a string as an expression
```

### Adding Event Handlers

To generate event handler subscriptions:

```csharp
// button.Clicked += OnButtonClicked;
ExpressionStatement(
    AssignmentExpression(
        SyntaxKind.AddAssignmentExpression,  // Note: ADD not SIMPLE
        MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName("button"),
            IdentifierName("Clicked")),
        IdentifierName("OnButtonClicked")))
```

### Adding Fields for Named Elements

For XTUI with `x:Name` attributes:

```csharp
// private Label _myLabel;
FieldDeclaration(
    VariableDeclaration(
        IdentifierName("Label"))
    .WithVariables(
        SingletonSeparatedList(
            VariableDeclarator("_myLabel"))))
.WithModifiers(
    TokenList(Token(SyntaxKind.PrivateKeyword)))
```

---

## Comparison with T4 Templates

### T4 (Text Template Transformation Toolkit)
```csharp
<#@ template language="C#" #>
namespace <#= namespaceName #>
{
    public class <#= className #>
    {
<# foreach (var child in children) { #>
        var <#= child.Name #> = new <#= child.Type #>();
<# } #>
    }
}
```

**Pros:**
- Easier to read for simple templates
- Direct string manipulation

**Cons:**
- No compile-time checking
- Difficult to build complex structures
- Hard to debug
- String-based (injection risks)

### Roslyn SyntaxFactory

**Pros:**
- Type-safe
- Compile-time checking
- Proper syntax validation
- Reusable node structures
- Better IntelliSense support

**Cons:**
- Steeper learning curve
- More verbose for simple cases
- Requires understanding of Roslyn APIs

---

## Real-World Applications

### 1. XTUI to Code Generators
Like this WindowGenerator - converting markup to code

### 2. Code Modernization Tools
Automatically updating old code patterns to new ones:
```csharp
// Old: var x = new List<int>(new[] { 1, 2, 3 });
// New: var x = new List<int> { 1, 2, 3 };
```

### 3. Boilerplate Reduction
Generating repetitive code from attributes or configuration:
```csharp
[GenerateEqualsAndHashCode]
public class Person { }
```

### 4. DSL (Domain-Specific Language) Compilers
Compiling custom languages to C#:
```
component Button {
    text: "Click Me"
    onClick: handleClick
}
```

### 5. API Clients from Specifications
Generating client code from OpenAPI/Swagger specs

---

## Resources for Learning More

### Official Documentation
- [Roslyn GitHub Repository](https://github.com/dotnet/roslyn)
- [Roslyn API Documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis)
- [Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)

### Tools
- **Roslyn Quoter**: https://roslynquoter.azurewebsites.net/
- **Syntax Visualizer**: Built into Visual Studio (View → Other Windows → Syntax Visualizer)
- **SharpLab**: https://sharplab.io/ - See IL, JIT asm, and syntax trees

### Key Concepts to Study Further
1. **Syntax Trees** - The structure of C# code as a tree
2. **Semantic Model** - Type information and symbol resolution
3. **Symbols** - Representations of types, methods, properties, etc.
4. **Incremental Generators** - Performance optimization for source generators
5. **Analyzers** - Code analysis using Roslyn

---

## Summary

The `WindowGenerator` demonstrates core Roslyn code generation concepts:

1. **SyntaxFactory** provides factory methods for all C# syntax constructs
2. **Fluent API** with `With*` methods builds syntax trees immutably
3. **Bottom-up construction** builds from expressions → statements → members → declarations
4. **Type safety** ensures generated code is syntactically correct
5. **NormalizeWhitespace()** makes generated code readable
6. **Recursive generation** handles nested structures elegantly

The generator transforms declarative XTUI into imperative C# code, enabling a XTUI-based UI design experience for Terminal.Gui while maintaining the framework's code-first nature.

Understanding this pattern opens the door to creating your own source generators, code modernization tools, and domain-specific language compilers using Roslyn's powerful APIs.

---

## Example: Complete Trace of Syntax Tree Building

Let's trace building `var x = 5;` to solidify understanding:

```csharp
// 1. Create the literal value: 5
var literalToken = Literal(5);
var literalExpr = LiteralExpression(
    SyntaxKind.NumericLiteralExpression, 
    literalToken);

// 2. Create the equals value clause: = 5
var equalsValue = EqualsValueClause(literalExpr);

// 3. Create the variable declarator: x = 5
var varDeclarator = VariableDeclarator(Identifier("x"))
    .WithInitializer(equalsValue);

// 4. Create the variable declaration: var x = 5
var varDeclaration = VariableDeclaration(IdentifierName("var"))
    .WithVariables(SingletonSeparatedList(varDeclarator));

// 5. Create the statement: var x = 5;
var statement = LocalDeclarationStatement(varDeclaration);

// 6. Use in a method
var method = MethodDeclaration(
        PredefinedType(Token(SyntaxKind.VoidKeyword)),
        "MyMethod")
    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
    .WithBody(Block(statement));

// Result:
// public void MyMethod()
// {
//     var x = 5;
// }
```

This systematic, bottom-up approach is the key to mastering Roslyn code generation!
