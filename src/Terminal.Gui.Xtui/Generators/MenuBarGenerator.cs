using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

/// <summary>
/// Generator for MenuBar controls. MenuBars are horizontal menus that can contain MenuBarItems.
/// </summary>
internal sealed class MenuBarGenerator : Generator
{
    /// <inheritdoc />
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create MenuBar with object initializer: var {variableName} = new MenuBar { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer("MenuBar", node.Attributes);

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

        // Process children - looking for MenuBarItems container or direct MenuBarItem elements
        if (node.Children.Count > 0)
        {
            // Check if there's a MenuBarItems container element
            ElementNode? menuBarItemsContainer = node.Children.FirstOrDefault(c => c.ElementTypeName == "MenuBarItems");
            List<ElementNode> itemsToProcess = menuBarItemsContainer != null 
                ? menuBarItemsContainer.Children 
                : node.Children.Where(c => c.ElementTypeName == "MenuBarItem").ToList();

            for (int i = 0; i < itemsToProcess.Count; i++)
            {
                ElementNode child = itemsToProcess[i];
                string childVarName = $"{child.ElementTypeName.ToLower()}{i}";
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
        }

        return statements.ToArray();
    }

    /// <inheritdoc />
    public override string GenerateClass(ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        // MenuBar can be a top-level element, so generate a partial class
        // The pattern is: partial class inherits from MenuBar with InitializeComponent method
        // Since the class inherits from MenuBar, we set properties on 'this' and add MenuBarItems to 'this'
        
        List<StatementSyntax> initializeComponentStatements = new List<StatementSyntax>();

        // Set properties on 'this' from node attributes
        if (node.Attributes.Count > 0)
        {
            foreach (var attr in node.Attributes)
            {
                // Skip Id attribute - it's not a settable property
                if (attr.Key == "Id") continue;

                // this.PropertyName = value;
                initializeComponentStatements.Add(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(attr.Key)),
                            ObjectParsingHelpers.ParseValueWithType(attr.Value, attr.Key))));
            }
        }

        // Process children - looking for MenuBarItems container or direct MenuBarItem elements
        if (node.Children.Count > 0)
        {
            // Check if there's a MenuBarItems container element
            ElementNode? menuBarItemsContainer = node.Children.FirstOrDefault(c => c.ElementTypeName == "MenuBarItems");
            List<ElementNode> itemsToProcess = menuBarItemsContainer != null 
                ? menuBarItemsContainer.Children 
                : node.Children.Where(c => c.ElementTypeName == "MenuBarItem").ToList();

            for (int i = 0; i < itemsToProcess.Count; i++)
            {
                ElementNode child = itemsToProcess[i];
                
                // Create the child object inline: this.Add(new MenuBarItem { ... });
                Dictionary<string, string> attributesWithoutId = child.Attributes
                    .Where(kvp => kvp.Key != "Id")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                ObjectCreationExpressionSyntax childObjectCreation = CreateObjectWithInitializer(
                    child.ElementTypeName,
                    attributesWithoutId);

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

        // Build the InitializeComponent method
        MethodDeclarationSyntax initMethod = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("InitializeComponent"))
            .WithModifiers(
                TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(
                Block(initializeComponentStatements));

        // Build the partial class
        ClassDeclarationSyntax classDeclaration = ClassDeclaration(className)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(IdentifierName("MenuBar")))))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(initMethod));

        // Build the namespace
        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(
                IdentifierName(namespaceName))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));

        // Build the compilation unit with usings
        CompilationUnitSyntax compilationUnit = CompilationUnit()
            .WithUsings(
                List(
                    new[]
                    {
                        UsingDirective(QualifiedName(QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")), IdentifierName("Views"))),
                        UsingDirective(QualifiedName(QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")), IdentifierName("ViewBase")))
                    }))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }

    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string typeName,
        Dictionary<string, string> attributes)
    {
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
