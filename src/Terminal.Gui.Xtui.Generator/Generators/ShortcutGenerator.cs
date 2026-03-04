using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using Terminal.Gui.Xtui.Generator.Helpers;
using static Terminal.Gui.Xtui.Generator.Helpers.TypeNameHelpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

/// <summary>
/// Terminal.Gui.Xtui.Generator.Helpers.Generator for Shortcut controls. Shortcuts are displayed in StatusBars to show command bindings.
/// </summary>
internal sealed class ShortcutGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Shortcut with object initializer: var {variableName} = new Shortcut { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("Shortcut", node.Attributes);

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
                .NormalizeWhitespace ()
        ];

        // Process children if any - assign to CommandView property
        if (node.Children.Count <= 0)
        {
            return statements.ToArray ();
        }

        // Shortcut supports one child element assigned to CommandView
        ElementNode child = node.Children[0];
        string localTypeName = ExtractLocalTypeName (child.ElementTypeName);
        var childVarName = $"{localTypeName.ToLower()}0";
        BaseGenerator childGenerator = generators.GetGenerator(child.ElementTypeName);
        StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

        statements.AddRange(childStatements);

        // {variableName}.CommandView = {childVarName};
        statements.Add(
            ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(variableName),
                            IdentifierName("CommandView")),
                        IdentifierName(childVarName))));

        return statements.ToArray();
    }
}
