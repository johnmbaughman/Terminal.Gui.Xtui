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
    /// <inheritdoc />
    public override StatementSyntax [] GenerateStatements (ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Window with object initializer: var {variableName} = new Window { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer ("Window", node.Attributes);

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
            return [.. statements];
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children [i];
            string childVarName = $"{child.ElementTypeName.ToLower ()}{i}";
            Generator childGenerator = generators.GetGenerator (child.ElementTypeName);
            StatementSyntax [] childStatements = childGenerator.GenerateStatements (child, childVarName, generators);

            statements.AddRange (childStatements);

            // {variableName}.Add({childVarName});
            statements.Add (
                ExpressionStatement (
                    InvocationExpression (
                            MemberAccessExpression (
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName (variableName),
                                IdentifierName ("Add")))
                        .WithArgumentList (
                            ArgumentList (
                                SingletonSeparatedList (
                                    Argument (IdentifierName (childVarName)))))));
        }

        return [.. statements];
    }

    /// <inheritdoc />
    public override string GenerateClass (ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        // Generate statements for children in InitializeComponent using object initializers
        List<StatementSyntax> initializeComponentStatements = new List<StatementSyntax> ();
        List<FieldDeclarationSyntax> fieldDeclarations = new List<FieldDeclarationSyntax>();

        if (node.Children.Count > 0)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                ElementNode child = node.Children [i];

                // Check if the child has an Id attribute
                string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;
                
                // Remove Id from attributes used in object initializer (Id is not a settable property in all cases)
                Dictionary<string, string> attributesWithoutId = child.Attributes
                    .Where(kvp => kvp.Key != "Id")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Create child with object initializer: new Label { Text = "Hello" }
                ObjectCreationExpressionSyntax childObjectCreation = CreateObjectWithInitializer (child.ElementTypeName, attributesWithoutId);

                if (!string.IsNullOrEmpty(controlId))
                {
                    // Use the Id as-is for the field name (user controls the naming convention)
                    // controlId is guaranteed non-null here due to !string.IsNullOrEmpty check
                    string fieldName = controlId!;
                    fieldDeclarations.Add(
                        FieldDeclaration(
                            VariableDeclaration(
                                NullableType(IdentifierName(child.ElementTypeName)))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(fieldName)))))
                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));

                    // Generate: _fieldName = new Label { ... }; this.Add(_fieldName);
                    initializeComponentStatements.Add(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(fieldName),
                                childObjectCreation)));

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
                                        Argument(IdentifierName(fieldName)))))));
                }
                else
                {
                    // this.Add(new Label { Text = "Hello" });
                    initializeComponentStatements.Add (
                        ExpressionStatement (
                            InvocationExpression (
                                MemberAccessExpression (
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression (),
                                    IdentifierName ("Add")))
                            .WithArgumentList (
                                ArgumentList (
                                    SingletonSeparatedList (
                                        Argument (childObjectCreation))))));
                }
            }
        }

        // Build InitializeComponent method: private void InitializeComponent() { ... }
        MethodDeclarationSyntax initializeComponent = MethodDeclaration (
                PredefinedType (Token (SyntaxKind.VoidKeyword)),
                Identifier ("InitializeComponent"))
            .WithModifiers (TokenList (Token (SyntaxKind.PrivateKeyword)))
            .WithBody (Block (initializeComponentStatements));

        // Build the class declaration: public partial class {className} : Window
        // Note: The constructor is not generated here - it must be defined in the other partial class
        List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();
        members.AddRange(fieldDeclarations);
        members.Add(initializeComponent);

        ClassDeclarationSyntax classDeclaration = ClassDeclaration (className)
            .WithModifiers (TokenList (
                Token (SyntaxKind.PublicKeyword),
                Token (SyntaxKind.PartialKeyword)))
            .WithBaseList (BaseList (SingletonSeparatedList<BaseTypeSyntax> (
                SimpleBaseType (IdentifierName ("Window")))))
            .WithMembers (List(members));

        // Build the namespace declaration
        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration (IdentifierName (namespaceName))
            .WithMembers (SingletonList<MemberDeclarationSyntax> (classDeclaration));

        // Build the compilation unit with using directives
        CompilationUnitSyntax compilationUnit = CompilationUnit ()
            .WithUsings (List (new []
            {
                UsingDirective(QualifiedName(
                    QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                    IdentifierName("Views"))),
                UsingDirective(QualifiedName(
                    QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                    IdentifierName("ViewBase")))
            }))
            .WithMembers (SingletonList<MemberDeclarationSyntax> (namespaceDeclaration))
            .NormalizeWhitespace ();

        // Add #nullable enable directive at the top
        return "#nullable enable\n" + compilationUnit.ToFullString ();
    }

    /// <summary>
    /// Creates an object creation expression with an object initializer for the given attributes.
    /// Example: new Window { Title = "Main", Width = 80, Visible = true }
    /// </summary>
    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer (
        string typeName,
        Dictionary<string, string> attributes)
    {
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