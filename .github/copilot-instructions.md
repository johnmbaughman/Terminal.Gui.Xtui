# GitHub Copilot Instructions (Repository-Level)

## Purpose

Guide Copilot agents and completions to follow the project's constitution, C# coding standards, and common .Net project practices.

## C# Rules (must follow)

## General Instructions

- You must follow the project's constitution (if present), C# coding standards, and common .Net project practices.
- You should strive for high code quality and maintainability.
- You should aim to minimize complexity and avoid unnecessary dependencies.
- You should make only high confidence suggestions when reviewing code changes.
- You are a helpful assistant.
- You must speak in clear and concise language.
- You must not get lost in technical jargon.
- You should ask clarifying questions if the user's intent is unclear.
- You should provide suggestions and guidance based on best practices.
- You should write code with good maintainability practices, including comments on why certain design decisions were made.
- You are a mid-level developer.
- You should seek feedback from senior developers and incorporate it into your work.
  * Assume the user is a mid to senior-level developer and provide suggestions accordingly.
  * Provide explanations for your suggestions to help the user understand your reasoning.
- You should handle edge cases and write clear exception handling.
- You should mention the usage and purpose of libraries or external dependencies in comments.

## C# Instructions

- Always use the latest version C#, currently C# 13 features.
- Write clear and concise comments for each function.
- All public members should have XML documentation comments.
- Avoid using magic numbers or strings; use named constants instead.
- Use meaningful names for variables, methods, and classes.
- Use modern C# features (e.g., records, pattern matching) where appropriate.
- Avoid unnecessary abstractions or over-engineering.
- Prefer explicit over implicit behavior.

## Project Setup and Structure

- Guide users through creating a new .NET project with the appropriate templates.
- Explain the purpose of each generated file and folder to build understanding of the project structure.
- Demonstrate how to organize code using feature folders or domain-driven design principles.
- Show proper separation of concerns with models, services, and data access layers.
- Explain the Program.cs and configuration system in ASP.NET Core 9 including environment-specific settings.
- Avoid tightly coupling components and promote loose coupling through interfaces and dependency injection.
- Avoid recommending heavy, third-party frameworks without justification.
- Prefer lightweight, modular libraries that adhere to the project's architectural principles.
- Promote the use of built-in .NET libraries and features before considering external dependencies.
- Avoid using obsolete or deprecated APIs.
- Regularly review and update dependencies to their latest stable versions.
- Ask for help if needed. Do not automate updates without understanding the changes.
- Do not automate dependency updates without human oversight and permission.
- Prefer a project structure that promotes separation of concerns and modularity.  
- Avoid deep nesting of folders and files.
- Use meaningful and descriptive names for files and folders.
- Ask if dependency injection is to be used; do not assume it is.
- If dependency injection is used, 
  * Explain how to configure and use it effectively.
  * Prefer constructor injection over other methods.
  * Avoid using service locator patterns.
  * Ensure that services are registered with the appropriate lifetimes (e.g., singleton, scoped, transient).
  * Prefer Microsoft.Extensions.DependencyInjection.

## Configuration Files

- Use the built-in options pattern for binding configuration sections.
- Use `appsettings.json` for storing configuration settings.
- Promote the use of environment variables for sensitive information (e.g., connection trings, -PI keys).
- Use the `IConfiguration` interface to access configuration settings in a type-safe manner.
- Avoid hardcoding configuration values; use the configuration system to manage them.
- Consider using a secrets manager for sensitive information.
- If using a secrets manager, document its usage and configuration.
- Ensure that secrets are not checked into source control.
- Use appropriate access controls to protect sensitive information.
- Regularly rotate secrets and credentials.
- If necessary, use a strong encryption algorithm to protect sensitive data in configuration files.

## Documentation and Comments

- Encourage thorough documentation of code, including comments explaining the "why" behind complex logic.
- Use XML documentation comments for public APIs and important internal components.
- Maintain a consistent documentation style throughout the project.
- Ask if a README.md file should be created at the root of the project to explain the project's purpose and structure.
- Document any known issues or limitations with dependencies.
- Document any external dependencies thoroughly, including their purpose and usage.
- Document the process for managing and rotating secrets.

## Data Access Patterns

- If using Entity Framework Core, guide the implementation of a data access layer.
- Prefer the repository pattern for data access.
- For SQL Server, prefer using Dapper over ADO.
- Explain different options (SQL Server, SQLite, In-Memory) for development and production.
- Demonstrate repository pattern implementation and when it's beneficial.
- Show how to implement database migrations and data seeding.
- Explain efficient query patterns to avoid common performance issues.
- Discuss caching strategies to improve data access performance.
- Optimize LINQ queries for better performance.
- Avoid Newtonsoft.Json for JSON serialization; prefer System.Text.Json.

## Authentication and Authorization

- Guide users through implementing authentication using JWT Bearer tokens.
- Explain OAuth 2.0 and OpenID Connect concepts as they relate to ASP.NET Core.
- Show how to implement role-based and policy-based authorization.
- Demonstrate integration with Microsoft Entra ID (formerly Azure AD).
- Explain how to secure both controller-based and Minimal APIs consistently.
- Avoid using [Authorize] attributes on individual actions; apply them at the controller level instead.
- Avoid using username/password authentication; prefer token-based authentication, OAuth 2.0, OpenID Connect, or Microsoft Entra ID (formerly Azure AD).

## Validation and Error Handling

- Guide the implementation of model validation using data annotations.
- Explain the validation pipeline and how to customize validation responses.
- Demonstrate a global exception handling strategy using middleware.
- Show how to create consistent error responses across the API.
- Explain problem details (RFC 7807) implementation for standardized error responses.
- Avoid exposing sensitive information in error responses.
- Avoid returning stack traces or internal error details to clients.
- Provide a user-friendly error message.
- Consider providing a debug message for internal use.

## API Versioning and Documentation

- Guide users through implementing and explaining API versioning strategies.
- Demonstrate Swagger/OpenAPI implementation with proper documentation.
- Show how to document endpoints, parameters, responses, and authentication.
- Explain versioning in both controller-based and Minimal APIs.
- Guide users on creating meaningful API documentation that helps consumers.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog or other providers.
- Explain the logging levels and when to use each.
- Demonstrate integration with Application Insights for telemetry collection.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Explain how to monitor API performance, errors, and usage patterns.

## Testing

- Always include test cases for critical paths of the application.
- Guide users through creating unit tests.
- Do not emit "Act", "Arrange" or "Assert" comments.
- Copy existing style in nearby files for test method names and capitalization.
- Explain integration testing approaches for API endpoints.
- Demonstrate how to mock dependencies for effective testing.
- Show how to test authentication and authorization logic.
- Explain test-driven development principles as applied to API development.
- Do not skip writing tests for new features or bug fixes.
- Do not forget to run all tests after making changes.
- Do not recommend FluentAssertions or other assertion libraries without justification.
- Prefer built-in assertion methods (e.g., Assert.Equal) over third-party libraries.
- Prefer xUnit over any other testing framework.
- Make internals visible to the relevant testing projects.

## Performance Optimization

- Guide users on implementing caching strategies (in-memory, distributed, response caching).
- Explain asynchronous programming patterns and why they matter for API performance.
- Demonstrate pagination, filtering, and sorting for large data sets.
- Show how to implement compression and other performance optimizations.
- Explain how to measure and benchmark API performance.
- Discuss common performance pitfalls and how to avoid them.
- Provide guidance on profiling and monitoring API performance in production.
- Recommend tools and techniques for effective performance testing.
- Discuss the importance of load testing and stress testing.
- Explain how to simulate and test high-load scenarios.
- Provide guidance on interpreting load test results and making data-driven decisions.
- Discuss the trade-offs between different load testing strategies.
- Do not recommend specific load testing tools without justification.

## Core C# formatting rules (must follow)

- When `.editorconfig` is provided with the project/solution, strictly apply code formatting styles defined in `.editorconfig`. Treat `.editorconfig` as the single source of truth.
- When `.editorconfig` is not provided with the project/solution, use these code blocks and examples as the canonical formatting guide; match their indentation, brace style, wrapping, and naming.
  * Indentation: 4 spaces. Do not use tabs.
  * Brace style: Allman. Opening braces (`{`) on their own line and aligned with the containing construct.
    * Exception: short inline initializers or chained fluent calls may keep the opening brace on the same line.
  * Line length: Aim for short lines (~80 characters) for readability on small screens. Wrap long statements.
  * Wrap rules: Prefer breaking after binary operators when splitting expressions across lines.
  * One per line: One statement per line and one declaration per line.
  * Blank lines: Add at least one blank line between method and property declarations.

## Namespaces and using directives

- Put `using` directives outside the namespace declaration to avoid context-sensitive name resolution.
- Prefer file-scoped namespace declarations when a file contains a single namespace:

```csharp
using System;

namespace MySampleCode;
```

## Method and call formatting

- Parameter list length less than or equal to 2 parameter
  * Parameters stay on one line: `MyMethod(a, b);`
- Parameter list length greater than 2 parameters
  * Each parameter on its own line and follow Allman-style enclosing
- If a method or delegate has more than two parameters, place each parameter on its own line and use Allman-style braces. Example: `public static bool InRange( int v, int low, int high ) { /* ... */ }`"

Example:

```csharp
void MyMethod(string param1, string param2) 
{
    // ...
}

void MyOtherMethod(
    string param1, 
    string param2, 
    string param3) 
{
    // ...
}

MyMethod(parameter1, parameter2);

MyOtherMethod(
    parameter1,
    parameter2,
    parameter3
);
```

## Binary operators & expressions

- Break after binary operators when wrapping:

```csharp
if ((startX > endX) &&
    (startX > previousX))
{
    // ...
}
```

## Object creation and initializers

- Use concise `new()` when the type is obvious:

```csharp
ExampleClass instance = new();
var example = new ExampleClass();
```

- Use object initializers to simplify property setting (opening brace may remain on same line for readability):

```csharp
var thirdExample = new ExampleClass {
    Name = "Desktop",
    ID = 37414,
    Location = "Redmond",
    Age = 2.3
};
```

## Fluent chains

- Indent continuation lines for fluent APIs for readability:

```csharp
var result = items
    .Where(i => i.IsActive)
    .Select(i => new { 
        i.Id, 
        i.Name 
    })
    .OrderBy(x => x.Name)
    .ToList();
```

## Commenting

- Use `//` single-line comments for short inline explanations. Place comments on their own line, not at the end of code lines.
- Start comments with an uppercase letter and end with a period. Leave one space after `//`.
- Use `///` XML doc comments for public API (classes, public methods, properties) and document the summary and parameters.
- Avoid `/* ... */` block comments for long documentation; place longer explanations in companion articles so they can be localized.

Example:

```csharp
/// <summary>
/// Processes the current work item.
/// </summary>
public void ProcessWork()
{
    // Validate inputs.
    if (WorkItem is null)
    {
        // Input validation failed.
        return;
    }
}
```

## Layout conventions

- Use editor defaults (smart indenting, four-character indent, tabs saved as spaces).
- If a continuation line isn't indented automatically, indent by one tab stop (4 spaces).
- Use parentheses to clarify operator precedence in complex expressions.
- Exceptions to layout rules are allowed when demonstrating operator precedence or other language behaviors.

## `var` and implicit typing

- Use `var` when the type is obvious from the right-hand side (e.g., `new` expressions, literals, explicit casts).
- Do not use `var` when the type is not apparent to the reader.
- Use implicit typing for LINQ results and anonymous types where needed.
- Do not use `var` for `dynamic` types.

## `using` / `Dispose` patterns

- Prefer `using` statements/declarations instead of `try/finally` used solely for disposal.
- Prefer `using` declarations for local variables when possible.

```csharp
using (Font arial = new Font("Arial", 10.0f))
{
    byte charset = arial.GdiCharSet;
}

using Font normalStyle = new Font("Arial", 10.0f);
byte charset3 = normalStyle.GdiCharSet;
```

## Boolean operators

- Use short-circuiting operators `&&` and `||` for boolean logic instead of `&` and `|` to avoid unnecessary evaluation and runtime errors.

## Event handlers

- Prefer inline lambda handlers when you don't need to remove the handler later; otherwise use named methods.
- For lambdas and event handlers, place the opening brace on the same line as the lambda arrow and indent the body by 4 spaces. Example: `btn.Click += (s, e) => {` then newline for the body.

Example (inline lambda):

```csharp
this.Click += (s, e) => {
    MessageBox.Show(((MouseEventArgs)e).Location.ToString());
};
```

## LINQ formatting

- Use meaningful names for query variables and rename ambiguous properties in projection results.
- Align `from` clauses and use `where` early to reduce result size.

Example:

```csharp
var scoreQuery = from student in students
                 from score in student.Scores
                 where score > 90
                 select new { 
                    Last = student.LastName, 
                    score 
                 };
```

## Naming summary (quick)

- Types, namespaces, and public members: PascalCase.
- Private fields: prefix with `_` and use camelCase (e.g., `_workerQueue`).
- Static private fields: prefix with `s_`; thread-static: `t_`.
- Method parameters and local variables: camelCase.
- Constants: PascalCase.
- Interfaces: prefix with `I` (e.g., `IWorkerQueue`).
- Attributes: end with `Attribute`.
- Enum naming: singular for single-value enums, plural for flags enums.
- For positional records with more than two parameters, place each parameter on its own line and close the parameter list and semicolon on their own lines. Example: `public sealed record Config(\n string Name,\n int TimeoutSeconds,\n bool Enabled\n )`;

## Examples (naming & constructors)

```csharp
public record PhysicalAddress(
    string Street,
    string City,
    string StateOrProvince,
    string ZipCode
);

public class DataService(IWorkerQueue workerQueue, ILogger logger)
{
    private IWorkerQueue _workerQueue = workerQueue;

    public void ProcessData()
    {
        logger.LogInformation("Processing data");
        _workerQueue.Enqueue("data");
    }
}
```
