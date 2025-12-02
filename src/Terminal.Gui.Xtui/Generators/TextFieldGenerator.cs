using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class TextFieldGenerator : Generator
{
    /// <inheritdoc />
    public override StatementSyntax [] GenerateStatements (ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        // Create TextField with object initializer: var {variableName} = new TextField { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer ("TextField", node.Attributes);

        List<StatementSyntax> statements = new List<StatementSyntax>
        {
            LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier(variableName))
                                .WithInitializer(
                                    EqualsValueClause(objectCreation)))))
        };

        // Process children if any
        if (node.Children.Count <= 0)
        {
            return [.. statements];
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children [i];
            string childVarName = $"{child.ElementTypeName.ToLower ()}{i}";
            Generator childGenerator = generators.GetGenerator (child.ElementTypeName);
            StatementSyntax [] childStatements = childGenerator.GenerateStatements (child, childVarName, generators);

            statements.AddRange (childStatements);

            // {variableName}.Add({childVarName});
            statements.Add (
                ExpressionStatement (
                    InvocationExpression (
                            MemberAccessExpression (
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName (variableName),
                                IdentifierName ("Add")))
                        .WithArgumentList (
                            ArgumentList (
                                SingletonSeparatedList (
                                    Argument (
                                        IdentifierName (childVarName)))))));
        }

        return [.. statements];
    }

    /// <summary>
    /// Creates an object creation expression with an optional initializer block.
    /// Example: new TextField { Text = "Enter text", Secret = true }
    /// </summary>
    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer (
        string typeName,
        Dictionary<string, string> attributes)
    {
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression (IdentifierName (typeName))
            .WithArgumentList (ArgumentList ());

        if (attributes.Count > 0)
        {
            // Create property assignments for the object initializer
            IEnumerable<AssignmentExpressionSyntax> assignments = attributes.Select (attr =>
                AssignmentExpression (
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName (attr.Key),
                    ObjectParsingHelpers.ParseValueWithType (attr.Value, attr.Key)));

            // Create the initializer: { Property1 = "value1", Property2 = true }
            InitializerExpressionSyntax initializer = InitializerExpression (
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax> (assignments));

            objectCreation = objectCreation.WithInitializer (initializer);
        }

        return objectCreation;
    }
}
