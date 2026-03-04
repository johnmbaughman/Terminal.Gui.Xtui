using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;

namespace Terminal.Gui.Xtui.Generator.Generators;

internal sealed class RunnableGenerator : BaseGenerator
{
    internal override StatementSyntax[] GenerateStatements (ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Runnable with object initializer: var {variableName} = new Runnable { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("Runnable", node.Attributes);

        List<StatementSyntax> statements = new ()
        {
            LocalDeclarationStatement (
                VariableDeclaration (
                    IdentifierName ("var"))
                    .WithVariables (
                        SingletonSeparatedList (
                            VariableDeclarator (
                                     Identifier (variableName))
                                 .WithInitializer (
                                     EqualsValueClause (objectCreation)))))
        };

        // Process children
        for (var i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            string localTypeName = child.ElementTypeName.Contains ('.') ? child.ElementTypeName.Split ('.').Last () : child.ElementTypeName;
            var childVarName = $"{localTypeName.ToLower ()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator (child.ElementTypeName);
            var childStatements = childGenerator.GenerateStatements (child, childVarName, generators);
            statements.AddRange (childStatements);

            statements.Add (
                ExpressionStatement (
                    InvocationExpression (
                            MemberAccessExpression (
                                Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName (variableName),
                                IdentifierName ("Add")))
                        .WithArgumentList (
                            ArgumentList (
                                SingletonSeparatedList (
                                    Argument (IdentifierName (childVarName)))))));
        }

        return statements.ToArray ();
    }

    public override string GenerateClass (ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        // Generate class similar to WindowGenerator but assign fields directly and use Runnable as base
        List<StatementSyntax> initializeComponentStatements = new();
        List<FieldDeclarationSyntax> fieldDeclarations = new();

        if (node.Children.Count > 0)
        {
            for (var i = 0; i < node.Children.Count; i++)
            {
                ElementNode child = node.Children[i];

                string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;

                Dictionary<string, string> attributesWithoutId = child.Attributes
                    .Where(kvp => kvp.Key != "Id")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                ObjectCreationExpressionSyntax childObjectCreation = SyntaxHelpers.CreateObjectWithInitializer(child.ElementTypeName, attributesWithoutId);

                string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
                var childVarName = $"{localTypeName.ToLower()}{i}";

                string fieldName = !string.IsNullOrEmpty(controlId) ? controlId! : childVarName;
                fieldDeclarations.Add(
                    FieldDeclaration(
                        VariableDeclaration(
                            NullableType(IdentifierName(localTypeName)))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier(fieldName)))))
                    .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));

                // Directly assign new object to the field: this.fieldName = new Type { ... };
                initializeComponentStatements.Add(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(fieldName)),
                            childObjectCreation)));

                // Special-case: MenuBar and StatusBar should be added to the Runnable (top-level)
                // Emit: this.Add(this.<fieldName>);
                string localTypeLower = localTypeName.ToLower();
                if (localTypeLower == "menubar" || localTypeLower == "statusbar")
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
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                ThisExpression(),
                                                IdentifierName(fieldName))))))));
                }
            }
        }

        MethodDeclarationSyntax initializeComponent = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("InitializeComponent"))
            .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
            .WithBody(Block(initializeComponentStatements));

        List<MemberDeclarationSyntax> members = new();
        members.AddRange(fieldDeclarations);
        members.Add(initializeComponent);

        ClassDeclarationSyntax classDeclaration = ClassDeclaration(className)
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(IdentifierName("Runnable")))))
            .WithMembers(List(members));

        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(IdentifierName(namespaceName))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));

        List<UsingDirectiveSyntax> usings = new()
        {
            UsingDirective(QualifiedName(QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")), IdentifierName("Views"))),
            UsingDirective(QualifiedName(QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")), IdentifierName("ViewBase")))
        };

        HashSet<string> allNamespaces = CollectAllNamespaces(node);
        foreach (string? ns in allNamespaces.Where(ns => !string.IsNullOrEmpty(ns) && ns != "Terminal.Gui.Views"))
        {
            string[] parts = ns.Split('.');
            NameSyntax qualifiedName = IdentifierName(parts[0]);
            for (var j = 1; j < parts.Length; j++)
            {
                qualifiedName = QualifiedName(qualifiedName, IdentifierName(parts[j]));
            }
            usings.Add(UsingDirective(qualifiedName));
        }

        CompilationUnitSyntax compilationUnit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
            .NormalizeWhitespace();

        return "#nullable enable\n" + compilationUnit.ToFullString();
    }
}
