using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using Terminal.Gui.Xtui.Generator.Helpers;using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

/// <summary>
/// Terminal.Gui.Xtui.Generator.Helpers.Generator for MenuItem controls. MenuItems are individual menu items that appear in menus.
/// </summary>
internal sealed class MenuItemGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create MenuItem with object initializer: var {variableName} = new MenuItem { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("MenuItem", node.Attributes);

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

        return statements.ToArray();
    }
}
