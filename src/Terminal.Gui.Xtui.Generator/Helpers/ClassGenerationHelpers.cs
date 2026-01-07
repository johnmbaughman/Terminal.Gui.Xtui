using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Helper methods for generating class declarations, method declarations,
/// and field declarations with proper modifiers and structure.
/// Simplifies the creation of Roslyn syntax for common class-level constructs.
/// </summary>
internal static class ClassGenerationHelpers
{
    /// <summary>
    /// Creates a class declaration with optional modifiers, base type, and members.
    /// </summary>
    /// <param name="className">The name of the class</param>
    /// <param name="isPublic">Whether the class should be public (default: false)</param>
    /// <param name="isPartial">Whether the class should be partial (default: false)</param>
    /// <param name="baseTypeName">Optional base type name for inheritance</param>
    /// <param name="members">Optional members to add to the class</param>
    /// <returns>A ClassDeclarationSyntax representing the class</returns>
    /// <exception cref="ArgumentNullException">Thrown when className is null</exception>
    /// <exception cref="ArgumentException">Thrown when className is empty</exception>
    /// <example>
    /// CreateClass("MyWindow", isPublic: true, isPartial: true, baseTypeName: "Window")
    /// generates: public partial class MyWindow : Window { }
    /// </example>
    public static ClassDeclarationSyntax CreateClass(
        string className,
        bool isPublic = false,
        bool isPartial = false,
        string? baseTypeName = null,
        MemberDeclarationSyntax[]? members = null)
    {
        if (className == null)
        {
            throw new ArgumentNullException(nameof(className));
        }

        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentException("Class name cannot be empty", nameof(className));
        }

        // Build modifiers list
        var modifiers = new List<SyntaxToken>();
        if (isPublic)
        {
            modifiers.Add(Token(SyntaxKind.PublicKeyword));
        }
        if (isPartial)
        {
            modifiers.Add(Token(SyntaxKind.PartialKeyword));
        }

        // Create base class declaration
        var classDecl = ClassDeclaration(className);

        // Add modifiers if any
        if (modifiers.Count > 0)
        {
            classDecl = classDecl.WithModifiers(TokenList(modifiers));
        }

        // Add base type if specified
        if (!string.IsNullOrEmpty(baseTypeName))
        {
            classDecl = classDecl.WithBaseList(
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(IdentifierName(baseTypeName!)))));
        }

        // Add members if specified
        if (members != null && members.Length > 0)
        {
            classDecl = classDecl.WithMembers(List(members));
        }

        return classDecl.NormalizeWhitespace();
    }

    /// <summary>
    /// Creates a method declaration with optional modifiers and body statements.
    /// </summary>
    /// <param name="methodName">The name of the method</param>
    /// <param name="returnTypeName">The return type name (e.g., "void", "string", "int")</param>
    /// <param name="isPrivate">Whether the method should be private (default: false)</param>
    /// <param name="statements">Optional statements to include in the method body</param>
    /// <returns>A MethodDeclarationSyntax representing the method</returns>
    /// <exception cref="ArgumentNullException">Thrown when methodName or returnTypeName is null</exception>
    /// <example>
    /// CreateMethod("InitializeComponent", "void", isPrivate: true)
    /// generates: private void InitializeComponent() { }
    /// </example>
    public static MethodDeclarationSyntax CreateMethod(
        string methodName,
        string returnTypeName,
        bool isPrivate = false,
        StatementSyntax[]? statements = null)
    {
        if (methodName == null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        if (returnTypeName == null)
        {
            throw new ArgumentNullException(nameof(returnTypeName));
        }

        // Determine return type syntax
        TypeSyntax returnType = returnTypeName.ToLowerInvariant() == "void"
            ? PredefinedType(Token(SyntaxKind.VoidKeyword))
            : IdentifierName(returnTypeName);

        // Create method declaration
        var methodDecl = MethodDeclaration(returnType, Identifier(methodName));

        // Add private modifier if specified
        if (isPrivate)
        {
            methodDecl = methodDecl.WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }

        // Add body with statements if provided
        if (statements != null && statements.Length > 0)
        {
            methodDecl = methodDecl.WithBody(Block(statements));
        }
        else
        {
            methodDecl = methodDecl.WithBody(Block());
        }

        return methodDecl.NormalizeWhitespace();
    }

    /// <summary>
    /// Creates a private field declaration with optional nullable modifier.
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="typeName">The type name of the field</param>
    /// <param name="isNullable">Whether the field type should be nullable (default: false)</param>
    /// <returns>A FieldDeclarationSyntax representing the private field</returns>
    /// <exception cref="ArgumentNullException">Thrown when fieldName or typeName is null</exception>
    /// <example>
    /// CreatePrivateField("myLabel", "Label", isNullable: true)
    /// generates: private Label? myLabel;
    /// </example>
    public static FieldDeclarationSyntax CreatePrivateField(
        string fieldName,
        string typeName,
        bool isNullable = false)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        // Create type syntax (nullable or not)
        TypeSyntax fieldType = isNullable
            ? NullableType(IdentifierName(typeName))
            : IdentifierName(typeName);

        // Create field declaration
        return FieldDeclaration(
                VariableDeclaration(fieldType)
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(fieldName)))))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .NormalizeWhitespace();
    }
}
