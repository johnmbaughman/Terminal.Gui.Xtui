using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Terminal.Gui.Xtui.Generator.Helpers.TypeNameHelpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Generators;

internal sealed class LabelGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Label with object initializer: var {variableName} = new Label { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer ("Label", node.Attributes);

        List<StatementSyntax> statements =
        [
            LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier(variableName))
                                .WithInitializer(
                                    EqualsValueClause(objectCreation)))))
        ];

        // Labels typically don't have children, but handle them just in case
        if (node.Children.Count <= 0)
        {
            return [.. statements];
        }

        for (var i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children [i];
            // Extract local type name for variable naming
            string localTypeName = ExtractLocalTypeName (child.ElementTypeName);
            var childVarName = $"{localTypeName.ToLower ()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator (child.ElementTypeName);
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
                                    Argument (IdentifierName (childVarName)))))));
        }

        return [.. statements];
    }
}
