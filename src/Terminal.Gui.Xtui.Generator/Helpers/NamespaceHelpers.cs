using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Helper methods for creating namespace declarations, using directives,
/// and building qualified names from namespace strings.
/// Provides utilities to simplify Roslyn syntax generation for namespace-related code.
/// </summary>
internal static class NamespaceHelpers
{
    /// <summary>
    /// Creates a namespace declaration from a namespace string.
    /// Supports both simple names (e.g., "MyApp") and qualified names (e.g., "Terminal.Gui.Views").
    /// </summary>
    /// <param name="namespaceName">The namespace name (simple or qualified with dots)</param>
    /// <param name="members">Optional members to include in the namespace</param>
    /// <returns>A NamespaceDeclarationSyntax representing the namespace</returns>
    /// <exception cref="ArgumentNullException">Thrown when namespaceName is null</exception>
    /// <exception cref="ArgumentException">Thrown when namespaceName is empty</exception>
    /// <example>
    /// CreateNamespaceDeclaration("MyApp") creates: namespace MyApp { }
    /// CreateNamespaceDeclaration("Terminal.Gui.Views") creates: namespace Terminal.Gui.Views { }
    /// </example>
    public static NamespaceDeclarationSyntax CreateNamespaceDeclaration(
        string namespaceName,
        params MemberDeclarationSyntax[] members)
    {
        if (namespaceName == null)
        {
            throw new ArgumentNullException(nameof(namespaceName));
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            throw new ArgumentException("Namespace name cannot be empty", nameof(namespaceName));
        }

        NameSyntax nameSyntax = BuildQualifiedName(namespaceName);
        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(nameSyntax);

        if (members != null && members.Length > 0)
        {
            namespaceDeclaration = namespaceDeclaration.WithMembers(List(members));
        }

        return namespaceDeclaration;
    }

    /// <summary>
    /// Creates a using directive from a namespace string.
    /// The result is normalized and includes a semicolon.
    /// </summary>
    /// <param name="namespaceName">The namespace name (simple or qualified with dots)</param>
    /// <returns>A UsingDirectiveSyntax representing the using statement</returns>
    /// <exception cref="ArgumentNullException">Thrown when namespaceName is null</exception>
    /// <exception cref="ArgumentException">Thrown when namespaceName is empty</exception>
    /// <example>
    /// CreateUsingDirective("System") creates: using System;
    /// CreateUsingDirective("Terminal.Gui.Views") creates: using Terminal.Gui.Views;
    /// </example>
    public static UsingDirectiveSyntax CreateUsingDirective(string namespaceName)
    {
        if (namespaceName == null)
        {
            throw new ArgumentNullException(nameof(namespaceName));
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            throw new ArgumentException("Namespace name cannot be empty", nameof(namespaceName));
        }

        NameSyntax nameSyntax = BuildQualifiedName(namespaceName);
        return UsingDirective(nameSyntax).NormalizeWhitespace();
    }

    /// <summary>
    /// Builds a qualified name from a dot-separated namespace string.
    /// Single-part names return IdentifierNameSyntax.
    /// Multi-part names return nested QualifiedNameSyntax.
    /// </summary>
    /// <param name="namespaceName">The namespace name (e.g., "System.Collections.Generic")</param>
    /// <returns>A NameSyntax representing the qualified name</returns>
    /// <exception cref="ArgumentNullException">Thrown when namespaceName is null</exception>
    /// <exception cref="ArgumentException">Thrown when namespaceName is empty</exception>
    /// <example>
    /// BuildQualifiedName("System") returns IdentifierName("System")
    /// BuildQualifiedName("System.Text") returns QualifiedName(IdentifierName("System"), IdentifierName("Text"))
    /// BuildQualifiedName("Terminal.Gui.Views") returns nested QualifiedNameSyntax
    /// </example>
    public static NameSyntax BuildQualifiedName(string namespaceName)
    {
        if (namespaceName == null)
        {
            throw new ArgumentNullException(nameof(namespaceName));
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            throw new ArgumentException("Namespace name cannot be empty", nameof(namespaceName));
        }

        string[] parts = namespaceName.Split('.');

        // Single part - just return identifier
        if (parts.Length == 1)
        {
            return IdentifierName(parts[0]);
        }

        // Build qualified name from parts
        NameSyntax qualifiedName = IdentifierName(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            qualifiedName = QualifiedName(qualifiedName, IdentifierName(parts[i]));
        }

        return qualifiedName;
    }

    /// <summary>
    /// Creates a compilation unit with optional using directives and members.
    /// The compilation unit is normalized for consistent formatting.
    /// </summary>
    /// <param name="usings">Optional using directives to include</param>
    /// <param name="members">Optional members (e.g., namespaces, classes) to include</param>
    /// <returns>A CompilationUnitSyntax representing the top-level file structure</returns>
    /// <example>
    /// CreateCompilationUnit() creates an empty compilation unit
    /// CreateCompilationUnit(usings) creates a unit with using directives
    /// CreateCompilationUnit(usings, members) creates a complete file structure
    /// </example>
    public static CompilationUnitSyntax CreateCompilationUnit(
        UsingDirectiveSyntax[]? usings = null,
        MemberDeclarationSyntax[]? members = null)
    {
        CompilationUnitSyntax compilationUnit = CompilationUnit();

        if (usings != null && usings.Length > 0)
        {
            compilationUnit = compilationUnit.WithUsings(List(usings));
        }

        if (members != null && members.Length > 0)
        {
            compilationUnit = compilationUnit.WithMembers(List(members));
        }

        return compilationUnit.NormalizeWhitespace();
    }

    /// <summary>
    /// Adds the #nullable enable directive to the beginning of a compilation unit.
    /// Returns the complete code as a string with the directive prepended.
    /// </summary>
    /// <param name="compilationUnit">The compilation unit to prepend the directive to</param>
    /// <returns>A string with "#nullable enable\n" followed by the normalized compilation unit code</returns>
    /// <exception cref="ArgumentNullException">Thrown when compilationUnit is null</exception>
    /// <example>
    /// AddNullableDirective(unit) returns: "#nullable enable\nusing System;\n..."
    /// </example>
    public static string AddNullableDirective(CompilationUnitSyntax compilationUnit)
    {
        if (compilationUnit == null)
        {
            throw new ArgumentNullException(nameof(compilationUnit));
        }

        return "#nullable enable\n" + compilationUnit.ToFullString();
    }
}
