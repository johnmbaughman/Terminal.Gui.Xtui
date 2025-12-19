using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Terminal.Gui.Xtui.Generators.Helpers;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class ButtonGenerator : Generator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        // Create Button with object initializer using shared helper
        var statements = new List<StatementSyntax>();

        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("Button", node.Attributes);

        // Create local variable declaration: var {variableName} = new Button { ... };
        var declaration = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(variableName))
                                .WithInitializer(EqualsValueClause(objectCreation)))));

        statements.Add(declaration);

        // Process children using shared helper
        var childStatements = ChildProcessingHelpers.ProcessChildElements(node, variableName, generators);
        if (childStatements != null && childStatements.Count > 0)
        {
            statements.AddRange(childStatements);
        }

        return [.. statements];
    }
}