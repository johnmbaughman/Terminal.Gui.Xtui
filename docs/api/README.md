# API Documentation Templates

This folder contains templates and examples for documenting public APIs, classes, and methods in the Terminal.Gui.Xaml Framework.

## Template: C# XML Documentation

```csharp
/// <summary>
/// [Describe the purpose and usage of the class or method.]
/// </summary>
/// <param name="paramName">[Describe the parameter]</param>
/// <returns>[Describe the return value]</returns>
```

## Example: Documenting a Public Method

```csharp
/// <summary>
/// Parses a XAML file and returns the corresponding Terminal.Gui view hierarchy.
/// </summary>
/// <param name="xamlPath">Path to the XAML file.</param>
/// <returns>Root view element.</returns>
public View ParseXaml(string xamlPath) { ... }
```

## Guidelines
- Use XML documentation for all public APIs
- Include summary, parameter, and return tags
- Document exceptions thrown
- Reference constitutional requirements where relevant
