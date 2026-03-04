using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Helper methods for generating Roslyn syntax nodes for common code patterns.
/// Provides utilities for creating variable declarations, assignments, method calls,
/// and other frequently-used syntax constructs in code generation.
/// </summary>
internal static class SyntaxHelpers
{
    /// <summary>
    /// Creates a local variable declaration statement with object creation initializer.
    /// Generates: var {variableName} = new {typeName}();
    /// </summary>
    /// <param name="variableName">The name of the variable to declare</param>
    /// <param name="typeName">The type name for the object creation</param>
    /// <returns>A StatementSyntax representing the variable declaration</returns>
    /// <exception cref="ArgumentNullException">Thrown when variableName or typeName is null</exception>
    /// <example>
    /// CreateVariableDeclaration("button0", "Button") generates:
    /// var button0 = new Button();
    /// </example>
    public static StatementSyntax CreateVariableDeclaration(string variableName, string typeName)
    {
        if (variableName == null)
        {
            throw new ArgumentNullException(nameof(variableName));
        }

        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        ObjectCreationExpressionSyntax objectCreation = CreateObjectCreation(typeName);

        return LocalDeclarationStatement(
            VariableDeclaration(
                    IdentifierName("var"))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                                Identifier(variableName))
                            .WithInitializer(
                                EqualsValueClause(objectCreation)))))
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Creates an assignment statement for a field.
    /// Generates: this.{fieldName} = {variableName};
    /// </summary>
    /// <param name="fieldName">The name of the field to assign to</param>
    /// <param name="variableName">The name of the variable to assign from</param>
    /// <returns>A StatementSyntax representing the assignment</returns>
    /// <exception cref="ArgumentNullException">Thrown when fieldName or variableName is null</exception>
    /// <example>
    /// CreateFieldAssignment("menuBar", "menubar0") generates:
    /// this.menuBar = menubar0;
    /// </example>
    public static StatementSyntax CreateFieldAssignment(string fieldName, string variableName)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (variableName == null)
        {
            throw new ArgumentNullException(nameof(variableName));
        }

        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(fieldName)),
                IdentifierName(variableName)))
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Creates a method call statement with a single argument.
    /// Generates: {targetVariable}.{methodName}({argumentVariable});
    /// </summary>
    /// <param name="targetVariable">The target variable on which to call the method</param>
    /// <param name="methodName">The name of the method to call</param>
    /// <param name="argumentVariable">The argument variable to pass</param>
    /// <returns>A StatementSyntax representing the method call</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <example>
    /// CreateMethodCall("window", "Add", "button0") generates:
    /// window.Add(button0);
    /// </example>
    public static StatementSyntax CreateMethodCall(
        string targetVariable,
        string methodName,
        string argumentVariable)
    {
        return CreateMethodCall(targetVariable, methodName, new[] { argumentVariable });
    }

    /// <summary>
    /// Creates a method call statement with multiple arguments.
    /// Generates: {targetVariable}.{methodName}({arg1}, {arg2}, ...);
    /// </summary>
    /// <param name="targetVariable">The target variable on which to call the method</param>
    /// <param name="methodName">The name of the method to call</param>
    /// <param name="argumentVariables">The argument variables to pass</param>
    /// <returns>A StatementSyntax representing the method call</returns>
    /// <exception cref="ArgumentNullException">Thrown when targetVariable or methodName is null</exception>
    /// <example>
    /// CreateMethodCall("statusBar", "Add", new[] {"s0", "s1", "s2"}) generates:
    /// statusBar.Add(s0, s1, s2);
    /// </example>
    public static StatementSyntax CreateMethodCall(
        string targetVariable,
        string methodName,
        params string[]? argumentVariables)
    {
        if (targetVariable == null)
        {
            throw new ArgumentNullException(nameof(targetVariable));
        }

        if (methodName == null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        argumentVariables ??= Array.Empty<string>();

        SeparatedSyntaxList<ArgumentSyntax> arguments = argumentVariables.Length > 0
            ? SeparatedList(argumentVariables.Select(arg => Argument(IdentifierName(arg))))
            : SeparatedList<ArgumentSyntax>();

        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(targetVariable),
                        IdentifierName(methodName)))
                .WithArgumentList(
                    ArgumentList(arguments)))
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Creates an object creation expression.
    /// Generates: new {typeName}()
    /// </summary>
    /// <param name="typeName">The type name for the object creation</param>
    /// <returns>An ObjectCreationExpressionSyntax representing the creation</returns>
    /// <exception cref="ArgumentNullException">Thrown when typeName is null</exception>
    /// <example>
    /// CreateObjectCreation("Window") generates:
    /// new Window()
    /// </example>
    public static ObjectCreationExpressionSyntax CreateObjectCreation(string typeName)
    {
        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        return ObjectCreationExpression(IdentifierName(typeName))
            .WithArgumentList(ArgumentList());
    }

    /// <summary>
    /// Creates a private nullable field declaration.
    /// Generates: private {typeName}? {fieldName};
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="typeName">The type name of the field</param>
    /// <returns>A FieldDeclarationSyntax representing the field</returns>
    /// <exception cref="ArgumentNullException">Thrown when fieldName or typeName is null</exception>
    /// <example>
    /// CreateThisFieldDeclaration("statusBar", "StatusBar") generates:
    /// private StatusBar? statusBar;
    /// </example>
    public static FieldDeclarationSyntax CreateThisFieldDeclaration(string fieldName, string typeName)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        return FieldDeclaration(
                VariableDeclaration(
                        NullableType(IdentifierName(typeName)))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier(fieldName)))))
            .WithModifiers(
                TokenList(Token(SyntaxKind.PrivateKeyword)))
            .NormalizeWhitespace();
    }

    /// <summary>
    /// Creates an object creation expression with an object initializer for the given attributes.
    /// Example: new Button { Text = "Click Me", Enabled = true }
    /// </summary>
    /// <param name="fullTypeName">The full type name (may contain namespace)</param>
    /// <param name="attributes">Dictionary of attribute name-value pairs</param>
    /// <returns>An ObjectCreationExpressionSyntax with property initializers</returns>
    /// <exception cref="ArgumentNullException">Thrown when fullTypeName or attributes is null</exception>
    public static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string fullTypeName,
        Dictionary<string, string> attributes)
    {
        if (fullTypeName == null)
        {
            throw new ArgumentNullException(nameof(fullTypeName));
        }

        if (attributes == null)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        // Extract local type name for object creation
        string typeName = fullTypeName.Contains('.') ? fullTypeName.Split('.').Last() : fullTypeName;
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression(IdentifierName(typeName))
            .WithArgumentList(ArgumentList());

        if (attributes.Count > 0)
        {
            // Create property assignments for the object initializer. If the RHS is a
            // lambda (e.g. an event handler), use '+=' so event handlers are attached
            // rather than attempting invalid assignment to events.
            IEnumerable<ExpressionSyntax> expressions = attributes.Select(attr =>
            {
                string propertyName = attr.Key;
                ExpressionSyntax rhs = ObjectParsingHelpers.ParseValueWithType(attr.Value, attr.Key);

                // If RHS is a lambda or anonymous method, generate an add-assignment (+=)
                if (rhs is Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax || rhs is Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousMethodExpressionSyntax)
                {
                    return (ExpressionSyntax)AssignmentExpression(
                        SyntaxKind.AddAssignmentExpression,
                        IdentifierName(propertyName),
                        rhs);
                }

                return (ExpressionSyntax)AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(propertyName),
                    rhs);
            });

            // Create the initializer: { Property1 = "value1", Property2 = 123, Property3 = true }
            InitializerExpressionSyntax initializer = InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax>(expressions));

            objectCreation = objectCreation.WithInitializer(initializer);
        }

        return objectCreation;
    }
}
