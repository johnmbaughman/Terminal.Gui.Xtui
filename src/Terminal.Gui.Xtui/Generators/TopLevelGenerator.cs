using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class TopLevelGenerator : Generator
{
    public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Toplevel with object initializer: var {variableName} = new Toplevel { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer("Toplevel", node.Attributes);

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

        // Process children
        if (node.Children.Count <= 0)
        {
            return statements.ToArray();
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
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

        return statements.ToArray();
    }

    public override string GenerateClass(ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        List<StatementSyntax> initializeComponentStatements = new List<StatementSyntax>();
        List<FieldDeclarationSyntax> fieldDeclarations = new List<FieldDeclarationSyntax>();

        if (node.Children.Count > 0)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                ElementNode child = node.Children[i];

                string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;
                string childVarName = !string.IsNullOrEmpty(controlId) ? controlId! : $"{child.ElementTypeName.ToLower()}{i}";

                // Get the appropriate generator for this child type and generate statements
                Generator childGenerator = generators.GetGenerator(child.ElementTypeName);
                StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

                // Add all the child's generation statements to InitializeComponent
                initializeComponentStatements.AddRange(childStatements);

                if (!string.IsNullOrEmpty(controlId))
                {
                    // Declare a private field for this child control
                    string fieldName = controlId!;
                    fieldDeclarations.Add(
                        FieldDeclaration(
                            VariableDeclaration(
                                NullableType(IdentifierName(child.ElementTypeName)))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(fieldName)))))
                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));
                    
                    // Assign the local variable to the field: this.fieldName = childVarName;
                    initializeComponentStatements.Add(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(fieldName)),
                                IdentifierName(childVarName))));
                }

                // If the child is a MenuBar, emit an Add call so the menu bar is added to
                // the Toplevel even when the caller constructor does not explicitly add it.
                // This mirrors the expected behavior when `CreateMenuBar()` is used (the
                // generator should ensure the MenuBar is present in the view hierarchy).
                if (string.Equals(child.ElementTypeName, "MenuBar", StringComparison.Ordinal))
                {
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
                                            Argument(IdentifierName(childVarName)))))));
                }
            }
        }

        MethodDeclarationSyntax initializeComponent = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("InitializeComponent"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(initializeComponentStatements));

        List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();
        members.AddRange(fieldDeclarations);
        members.Add(initializeComponent);

        ClassDeclarationSyntax classDeclaration = ClassDeclaration(className)
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(IdentifierName("Toplevel")))))
            .WithMembers(List(members));

        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));

        CompilationUnitSyntax compilationUnit = CompilationUnit()
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

        return "#nullable enable\n" + compilationUnit.ToFullString();
    }

    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string typeName,
        Dictionary<string, string> attributes)
    {
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression(IdentifierName(typeName))
            .WithArgumentList(ArgumentList());

        if (attributes.Count > 0)
        {
            IEnumerable<AssignmentExpressionSyntax> assignments = attributes.Select(attr =>
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(attr.Key),
                    ObjectParsingHelpers.ParseValueWithType(attr.Value, attr.Key)));

            InitializerExpressionSyntax initializer = InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SeparatedList<ExpressionSyntax>(assignments));

            objectCreation = objectCreation.WithInitializer(initializer);
        }

        return objectCreation;
    }
}
