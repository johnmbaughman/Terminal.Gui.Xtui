using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class LabelGenerator : Generator
{
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Label with object initializer: var {variableName} = new Label { ... };
        var objectCreation = CreateObjectWithInitializer("Label", node.Attributes);
        
        var statements = new List<StatementSyntax>
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

        // Labels typically don't have children, but handle them just in case
        if (node.Children.Count <= 0) return statements.ToArray();
        
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childVarName = $"{child.Name.ToLower()}{i}";
            var childGenerator = generators.GetGenerator(child.Name);
            var childStatements = childGenerator.GenerateStatements(child, childVarName, generators);
                
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

    /// <summary>
    /// Creates an object creation expression with an object initializer for the given attributes.
    /// Example: new Label { Text = "Hello", Visible = true }
    /// </summary>
    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string typeName, 
        Dictionary<string, string> attributes)
    {
        var objectCreation = ObjectCreationExpression(IdentifierName(typeName))
            .WithArgumentList(ArgumentList());
        
        if (attributes.Count > 0)
        {
            // Create property assignments for the object initializer
            var assignments = attributes.Select(attr =>
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(attr.Key),
                    ObjectParsingHelpers.ParseValueWithType(attr.Value, attr.Key)));
            
            // Create the initializer: { Property1 = "value1", Property2 = 123, Property3 = true }
            var initializer = InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax>(assignments));
            
            objectCreation = objectCreation.WithInitializer(initializer);
        }
        
        return objectCreation;
    }
}