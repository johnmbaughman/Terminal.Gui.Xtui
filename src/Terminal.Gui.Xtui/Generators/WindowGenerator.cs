using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class WindowGenerator : Generator
{
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Window with object initializer: var {variableName} = new Window { ... };
        var objectCreation = CreateObjectWithInitializer("Window", node.Attributes);
        
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

        // Process children
        if (node.Children.Count <= 0) return statements.ToArray();
        
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childVarName = $"{child.ElementTypeName.ToLower()}{i}";
            var childGenerator = generators.GetGenerator(child.ElementTypeName);
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

    public override string GenerateClass(ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        // Generate statements for children in InitializeComponent using object initializers
        var initializeComponentStatements = new List<StatementSyntax>();
        
        if (node.Children.Count > 0)
        {
            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                
                // Create child with object initializer: new Label { Text = "Hello" }
                var childObjectCreation = CreateObjectWithInitializer(child.ElementTypeName, child.Attributes);
                
                // this.Add(new Label { Text = "Hello" });
                initializeComponentStatements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName("Add")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(childObjectCreation))))));
            }
        }

        // Build InitializeComponent method: private void InitializeComponent() { ... }
        var initializeComponent = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("InitializeComponent"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(initializeComponentStatements));

        // Build the class declaration: public partial class {className} : Window
        // Note: The constructor is not generated here - it must be defined in the other partial class
        var classDeclaration = ClassDeclaration(className)
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(IdentifierName("Window")))))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(initializeComponent));

        // Build the namespace declaration
        var namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));

        // Build the compilation unit with using directives
        var compilationUnit = CompilationUnit()
            .WithUsings(List(new[]
            {
                UsingDirective(QualifiedName(
                    QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                    IdentifierName("Views"))),
                UsingDirective(QualifiedName(
                    QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                    IdentifierName("ViewBase")))
            }))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    /// <summary>
    /// Creates an object creation expression with an object initializer for the given attributes.
    /// Example: new Window { Title = "Main", Width = 80, Visible = true }
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