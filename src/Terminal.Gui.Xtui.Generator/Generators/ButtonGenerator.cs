using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Terminal.Gui.Xtui.Generator.Helpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

internal sealed class ButtonGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        // Create Button with object initializer using shared helper
        List<StatementSyntax> statements = [];

        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("Button", node.Attributes);

        // Create local variable declaration: var {variableName} = new Button { ... };
        LocalDeclarationStatementSyntax declaration = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
            .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(Identifier(variableName))
                    .WithInitializer(EqualsValueClause(objectCreation)))));

        statements.Add(declaration);

        // Process children using shared helper
        List<StatementSyntax>? childStatements = ChildProcessingHelpers.ProcessChildElements(node, variableName, generators);
        if (childStatements is { Count: > 0 })
        {
            statements.AddRange(childStatements);
        }

        return [.. statements];
    }
}
