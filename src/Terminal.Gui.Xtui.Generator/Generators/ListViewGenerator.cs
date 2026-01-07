using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generator.Helpers;using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Generators;

internal sealed class ListViewGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        // Create ListView with object initializer: var {variableName} = new ListView { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer ("ListView", node.Attributes);

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
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower ()}{i}";
            Terminal.Gui.Xtui.Generator.Helpers.Generator childGenerator = generators.GetGenerator (child.ElementTypeName);
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
}
