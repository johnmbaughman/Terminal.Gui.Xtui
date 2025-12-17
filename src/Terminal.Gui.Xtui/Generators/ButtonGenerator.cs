using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class ButtonGenerator : Generator
{
    /// <inheritdoc />
    public override StatementSyntax [] GenerateStatements (ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        // Create Button with object initializer: var {variableName} = new Button { ... };
        var statements = new List<StatementSyntax>();

        // Build object creation with initializer using existing helper in this file
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer("Button", node.Attributes);

        // Create local variable declaration: var {variableName} = new Button { ... };
        var declaration = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(variableName))
                                .WithInitializer(EqualsValueClause(objectCreation)))))
            .NormalizeWhitespace();

        statements.Add(declaration);

        // Use ChildProcessingHelpers to process children and append their statements
        var childStatements = Helpers.ChildProcessingHelpers.ProcessChildElements(node, variableName, generators);
        if (childStatements != null && childStatements.Count > 0)
        {
            statements.AddRange(childStatements);
        }

        return statements.ToArray();
    }

    /// <summary>
    /// Creates an object creation expression with an object initializer for the given attributes.
    /// Example: new Button { Text = "Click Me", Enabled = true }
    /// </summary>
    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer (
        string fullTypeName,
        Dictionary<string, string> attributes)
    {
        // Extract local type name for object creation
        string typeName = fullTypeName.Contains('.') ? fullTypeName.Split('.').Last() : fullTypeName;
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression (IdentifierName (typeName))
            .WithArgumentList (ArgumentList ());

        if (attributes.Count > 0)
        {
            // Create property assignments for the object initializer
            IEnumerable<AssignmentExpressionSyntax> assignments = attributes.Select (attr =>
                AssignmentExpression (
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName (attr.Key),
                    ObjectParsingHelpers.ParseValueWithType (attr.Value, attr.Key)));

            // Create the initializer: { Property1 = "value1", Property2 = 123, Property3 = true }
            InitializerExpressionSyntax initializer = InitializerExpression (
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax> (assignments));

            objectCreation = objectCreation.WithInitializer (initializer);
        }

        return objectCreation;
    }
}