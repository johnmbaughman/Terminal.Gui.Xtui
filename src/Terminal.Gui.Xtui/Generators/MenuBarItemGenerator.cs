using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

/// <summary>
/// Generator for MenuBarItem controls. MenuBarItems are menu items that appear in a MenuBar.
/// </summary>
internal sealed class MenuBarItemGenerator : Generator
{
    /// <inheritdoc />
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create MenuBarItem with object initializer: var {variableName} = new MenuBarItem { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer("MenuBarItem", node.Attributes);

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

        // Process children - looking for MenuItems container or direct MenuItem elements
        if (node.Children.Count > 0)
        {
            // Extract local type names (handle both simple names and fully-qualified names)
            static string GetLocalTypeName(string typeName) => typeName.Contains('.') ? typeName.Split('.').Last() : typeName;
            
            // Check if there's a MenuItems container element
            ElementNode? menuItemsContainer = node.Children.FirstOrDefault(c => 
                string.Equals(GetLocalTypeName(c.ElementTypeName), "MenuItems", StringComparison.Ordinal));
            List<ElementNode> itemsToProcess = menuItemsContainer != null 
                ? menuItemsContainer.Children 
                : node.Children.Where(c => 
                    string.Equals(GetLocalTypeName(c.ElementTypeName), "MenuItem", StringComparison.Ordinal)).ToList();

            if (itemsToProcess.Count > 0)
            {
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
                    Generator childGenerator = generators.GetGenerator(child.ElementTypeName);
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
            }
        }

        return statements.ToArray();
    }

    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string fullTypeName,
        Dictionary<string, string> attributes)
    {
        // Extract local type name for object creation
        string typeName = fullTypeName.Contains('.') ? fullTypeName.Split('.').Last() : fullTypeName;
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression(IdentifierName(typeName))
            .WithArgumentList(ArgumentList());

        if (attributes.Count > 0)
        {
            // Create property assignments for the object initializer
            IEnumerable<AssignmentExpressionSyntax> assignments = attributes.Select(attr =>
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(attr.Key),
                    ObjectParsingHelpers.ParseValueWithType(attr.Value, attr.Key)));

            // Create the initializer: { Property1 = "value1", Property2 = 123, Property3 = true }
            InitializerExpressionSyntax initializer = InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax>(assignments));

            objectCreation = objectCreation.WithInitializer(initializer);
        }

        return objectCreation;
    }
}
