using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Terminal.Gui.Xtui.Generator.Helpers.TypeNameHelpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

internal sealed class CheckBoxGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create CheckBox with object initializer using shared helper
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("CheckBox", node.Attributes);

        List<StatementSyntax> statements =
        [
            LocalDeclarationStatement (
                VariableDeclaration (
                    IdentifierName ("var"))
                    .WithVariables (
                        SingletonSeparatedList (
                            VariableDeclarator (
                                Identifier (variableName))
                                .WithInitializer (
                                EqualsValueClause (objectCreation)))))
        ];

        // CheckBox does not typically have child elements, but handle them if present
        if (node.Children.Count <= 0)
        {
            return statements.ToArray();
        }

        for (var i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            // Extract local type name for variable naming
            string localTypeName = ExtractLocalTypeName (child.ElementTypeName);
            var childVarName = $"{localTypeName.ToLower()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator(child.ElementTypeName);
            StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

            statements.AddRange(childStatements);

            // {variableName}.Add({childVarName});
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

        return statements.ToArray();
    }
}
