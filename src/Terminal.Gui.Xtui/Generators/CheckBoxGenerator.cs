using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Terminal.Gui.Xtui.Generators.Helpers;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class CheckBoxGenerator : Generator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create CheckBox with object initializer using shared helper
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("CheckBox", node.Attributes);

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

        // CheckBox does not typically have child elements, but handle them if present
        if (node.Children.Count <= 0)
        {
            return statements.ToArray();
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower()}{i}";
            Generator childGenerator = generators.GetGenerator(child.ElementTypeName);
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
