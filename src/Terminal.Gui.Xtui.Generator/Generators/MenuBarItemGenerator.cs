using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Terminal.Gui.Xtui.Generator.Helpers.TypeNameHelpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

/// <summary>
/// Terminal.Gui.Xtui.Generator.Helpers.Generator for MenuBarItem controls. MenuBarItems are menu items that appear in a MenuBar.
/// </summary>
internal sealed class MenuBarItemGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create MenuBarItem with object initializer: var {variableName} = new MenuBarItem { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("MenuBarItem", node.Attributes);

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

        // Process children - looking for MenuItems container or direct MenuItem elements
        if (node.Children.Count <= 0)
        {
            return statements.ToArray ();
        }

        // Check if there's a MenuItems container element
        ElementNode? menuItemsContainer = node.Children.FirstOrDefault(c =>
            string.Equals(ExtractLocalTypeName(c.ElementTypeName), "MenuItems", StringComparison.Ordinal));
        List<ElementNode> itemsToProcess = menuItemsContainer != null
                                               ? menuItemsContainer.Children
                                               : node.Children.Where(c =>
                                                   string.Equals(ExtractLocalTypeName(c.ElementTypeName),
                                                                 "MenuItem",
                                                                 StringComparison.Ordinal)).ToList();
        if (itemsToProcess.Count <= 0)
        {
            return statements.ToArray ();
        }

        // Create PopoverMenu with a new Menu as the root
        // {variableName}.PopoverMenu = new PopoverMenu(new Menu());
        statements.Add(
            ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(variableName),
                        IdentifierName("PopoverMenu")),
                        ObjectCreationExpression(
                            IdentifierName("PopoverMenu"))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            ObjectCreationExpression(
                                                IdentifierName("Menu"))
                                                .WithArgumentList(ArgumentList()))))))));

        for (int i = 0; i < itemsToProcess.Count; i++)
        {
            ElementNode child = itemsToProcess[i];
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator(child.ElementTypeName);
            StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

            statements.AddRange(childStatements);

            // {variableName}.PopoverMenu.Root.Add({childVarName});
            statements.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(variableName),
                                        IdentifierName("PopoverMenu")),
                                    IdentifierName("Root")),
                                IdentifierName("Add")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(IdentifierName(childVarName)))))));
        }

        return statements.ToArray();
    }
}

