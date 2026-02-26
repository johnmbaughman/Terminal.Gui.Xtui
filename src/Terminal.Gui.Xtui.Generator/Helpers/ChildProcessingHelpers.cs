using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Helper methods for processing child elements in generators.
/// Provides utilities for child iteration, variable naming, and generating
/// Add() method invocations for parent-child relationships.
/// </summary>
internal static class ChildProcessingHelpers
{
    /// <summary>
    /// Creates a camelCase variable name for a child element based on its type name and index.
    /// Extracts the simple type name from qualified names, converts to camelCase, and appends the index.
    /// </summary>
    /// <param name="childTypeName">The child element's type name (simple or qualified)</param>
    /// <param name="index">The zero-based index of the child in the parent's children collection</param>
    /// <returns>A camelCase variable name with index (e.g., "label0", "button1")</returns>
    /// <exception cref="ArgumentNullException">Thrown when childTypeName is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when index is negative</exception>
    /// <example>
    /// CreateChildVariableName("Label", 0) returns "label0"
    /// CreateChildVariableName("Terminal.Gui.Button", 5) returns "button5"
    /// </example>
    public static string CreateChildVariableName(string childTypeName, int index)
    {
        if (childTypeName == null)
        {
            throw new ArgumentNullException(nameof(childTypeName));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative");
        }

        // Extract local type name (e.g., "Terminal.Gui.Label" -> "Label")
        string localTypeName = childTypeName.Contains('.') 
            ? childTypeName.Split('.').Last() 
            : childTypeName;

        // Convert to camelCase and append index
        string camelCaseName = char.ToLowerInvariant(localTypeName[0]) + localTypeName.Substring(1);
        return $"{camelCaseName}{index}";
    }

    /// <summary>
    /// Creates an expression statement for invoking the Add() method on a parent variable
    /// to add a child variable. Generates: parentVarName.Add(childVarName);
    /// </summary>
    /// <param name="parentVarName">The parent variable name</param>
    /// <param name="childVarName">The child variable name to add</param>
    /// <returns>An ExpressionStatementSyntax representing the Add() method call</returns>
    /// <exception cref="ArgumentNullException">Thrown when parentVarName or childVarName is null</exception>
    /// <example>
    /// CreateAddMethodCall("window", "label0") generates: window.Add(label0);
    /// </example>
    public static ExpressionStatementSyntax CreateAddMethodCall(string parentVarName, string childVarName)
    {
        if (parentVarName == null)
        {
            throw new ArgumentNullException(nameof(parentVarName));
        }

        if (childVarName == null)
        {
            throw new ArgumentNullException(nameof(childVarName));
        }

        return ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(parentVarName),
                        IdentifierName("Add")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(IdentifierName(childVarName))))));
    }

    /// <summary>
    /// Processes all child elements of a node, generating statements for each child
    /// and Add() method calls to add them to the parent. This encapsulates the common
    /// child processing loop pattern used across generators.
    /// </summary>
    /// <param name="node">The parent ElementNode whose children should be processed</param>
    /// <param name="parentVarName">The parent variable name</param>
    /// <param name="generatorFactory">The generator factory to obtain child generators</param>
    /// <returns>A list of statements including child variable declarations and Add() calls</returns>
    /// <exception cref="ArgumentNullException">Thrown when node, parentVarName, or generatorFactory is null</exception>
    /// <example>
    /// For a Window with two Label children, generates:
    /// var label0 = new Label();
    /// window.Add(label0);
    /// var label1 = new Label();
    /// window.Add(label1);
    /// </example>
    public static List<StatementSyntax> ProcessChildElements(
        ElementNode node,
        string parentVarName,
        IGeneratorFactory generatorFactory)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (parentVarName == null)
        {
            throw new ArgumentNullException(nameof(parentVarName));
        }

        if (generatorFactory == null)
        {
            throw new ArgumentNullException(nameof(generatorFactory));
        }

        List<StatementSyntax> statements = new List<StatementSyntax>();

        // Early return if no children
        if (node.Children.Count == 0)
        {
            return statements;
        }

        // Process each child element
        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            
            // Create variable name for child
            string childVarName = CreateChildVariableName(child.ElementTypeName, i);
            
            // Get generator for child and generate its statements
            Generator childGenerator = generatorFactory.GetGenerator(child.ElementTypeName);
            StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generatorFactory);
            
            statements.AddRange(childStatements);
            
            // Add statement to add child to parent: parentVarName.Add(childVarName);
            statements.Add(CreateAddMethodCall(parentVarName, childVarName));
        }

        return statements;
    }

    /// <summary>
    /// Checks whether a node has any child elements.
    /// </summary>
    /// <param name="node">The ElementNode to check</param>
    /// <returns>True if the node has at least one child, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when node is null</exception>
    /// <example>
    /// HasChildren(nodeWithChildren) returns true
    /// HasChildren(nodeWithoutChildren) returns false
    /// </example>
    public static bool HasChildren(ElementNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return node.Children.Count > 0;
    }
}
