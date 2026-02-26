using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Terminal.Gui.Xtui.Generator.Helpers.TypeNameHelpers;
using BaseGenerator = Terminal.Gui.Xtui.Generator.Helpers.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Generators;

/// <summary>
/// Terminal.Gui.Xtui.Generator.Helpers.Generator for StatusBar controls. StatusBars display shortcuts at the bottom of a Toplevel.
/// </summary>
internal sealed class StatusBarGenerator : BaseGenerator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create StatusBar with object initializer: var {variableName} = new StatusBar { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer("StatusBar", node.Attributes);

        List<StatementSyntax> statements =
        [
            LocalDeclarationStatement (
                VariableDeclaration (
                    IdentifierName ("var"))
                    .WithVariables (
                        SingletonSeparatedList (
                            VariableDeclarator (
                                Identifier (variableName))
                                .WithInitializer (
                                    EqualsValueClause (objectCreation)))))
                .NormalizeWhitespace ()
        ];

        // Process children - looking for Shortcuts container or direct Shortcut elements
        if (node.Children.Count <= 0)
        {
            return statements.ToArray ();
        }

        // Check if there's a Shortcuts container element
        ElementNode? shortcutsContainer = node.Children.FirstOrDefault(c => ExtractLocalTypeName(c.ElementTypeName) == "Shortcuts");
        List<ElementNode> itemsToProcess = shortcutsContainer != null
                                               ? shortcutsContainer.Children
                                               : node.Children.Where(c => ExtractLocalTypeName(c.ElementTypeName) == "Shortcut").ToList();

        List<string> childVariableNames = [];

        for (int i = 0; i < itemsToProcess.Count; i++)
        {
            ElementNode child = itemsToProcess[i];
            string localTypeName = ExtractLocalTypeName(child.ElementTypeName);
            string childVarName = $"{localTypeName.ToLower()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator(child.ElementTypeName);
            StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

            statements.AddRange(childStatements);
            childVariableNames.Add(childVarName);
        }

        // Generate single param array Add call: {variableName}.Add(shortcut0, shortcut1, shortcut2, ...);
        if (childVariableNames.Count > 0)
        {
            statements.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(variableName),
                            IdentifierName("Add")))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList(
                                    childVariableNames.Select(varName =>
                                        Argument(IdentifierName(varName))))))));
        }

        return statements.ToArray();
    }

    /// <inheritdoc />
    public override string GenerateClass(ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        // StatusBar can be a top-level element, so generate a partial class
        // The pattern is: partial class inherits from StatusBar with InitializeComponent method

        List<StatementSyntax> initializeComponentStatements = [];
        List<FieldDeclarationSyntax> fieldDeclarations = [];

        // Set properties on 'this' from node attributes
        if (node.Attributes.Count > 0)
        {
            initializeComponentStatements.AddRange (
                from attr in node.Attributes
                where attr.Key != "Id"
                select ExpressionStatement (
                    AssignmentExpression (
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression (
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression (),
                            IdentifierName (attr.Key)),
                        ObjectParsingHelpers.ParseValueWithType (attr.Value, attr.Key))));
        }

        // Process children - looking for Shortcuts container or direct Shortcut elements
        List<string> childVariableNames = [];

        if (node.Children.Count > 0)
        {
            // Check if there's a Shortcuts container element
            ElementNode? shortcutsContainer = node.Children.FirstOrDefault(c => ExtractLocalTypeName(c.ElementTypeName) == "Shortcuts");
            List<ElementNode> itemsToProcess = shortcutsContainer != null
                ? shortcutsContainer.Children
                : node.Children.Where(c => ExtractLocalTypeName(c.ElementTypeName) == "Shortcut").ToList();

            for (int i = 0; i < itemsToProcess.Count; i++)
            {
                ElementNode child = itemsToProcess[i];

                string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;
                string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
                string childVarName = !string.IsNullOrEmpty(controlId) ? controlId! : $"{localTypeName.ToLower()}{i}";

                // Get the appropriate Terminal.Gui.Xtui.Generator.Helpers.Generator for this child type and generate statements
                BaseGenerator childGenerator = generators.GetGenerator(child.ElementTypeName);
                StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

                // Add all the child's generation statements to InitializeComponent
                initializeComponentStatements.AddRange(childStatements);

                // Declare a private field for this child control
                // Use the Id value for the field name if provided, otherwise use the generated var name
                string fieldName = !string.IsNullOrEmpty(controlId) ? controlId! : childVarName;
                fieldDeclarations.Add(
                    FieldDeclaration(
                        VariableDeclaration(
                            NullableType(IdentifierName(localTypeName)))
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

                // Track child variable names for param array Add call
                childVariableNames.Add(childVarName);
            }

            // Add all shortcuts in a single param array call: this.Add(shortcut0, shortcut1, shortcut2, ...);
            if (childVariableNames.Count > 0)
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
                                SeparatedList(
                                    childVariableNames.Select(varName =>
                                        Argument(IdentifierName(varName))))))));
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

        // Build class members list
        List<MemberDeclarationSyntax> members = [];
        members.AddRange(fieldDeclarations);
        members.Add(initMethod);

        // Build the partial class
        ClassDeclarationSyntax classDeclaration = ClassDeclaration(className)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword)))
            .WithBaseList(
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(IdentifierName("StatusBar")))))
            .WithMembers(List(members));

        // Build the namespace
        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(
                IdentifierName(namespaceName))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));

        // Collect using directives
        List<UsingDirectiveSyntax> usings =
            [UsingDirective (QualifiedName (QualifiedName (IdentifierName ("Terminal"), IdentifierName ("Gui")), IdentifierName ("Views")))];

        // Collect all namespaces from the element tree
        HashSet<string> allNamespaces = CollectAllNamespaces(node);

        // Add usings for all collected namespaces
        foreach (string? ns in allNamespaces.Where(ns => !string.IsNullOrEmpty(ns) && ns != "Terminal.Gui.Views"))
        {
            // Parse the namespace into qualified name
            string[] parts = ns.Split('.');
            NameSyntax qualifiedName = IdentifierName(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                qualifiedName = QualifiedName(qualifiedName, IdentifierName(parts[i]));
            }
            usings.Add(UsingDirective(qualifiedName));
        }

        // Build the compilation unit with usings
        CompilationUnitSyntax compilationUnit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
            .NormalizeWhitespace();

        return compilationUnit.ToFullString();
    }
}
