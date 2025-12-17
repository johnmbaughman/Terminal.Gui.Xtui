using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generators.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

internal sealed class WindowGenerator : Generator
{
    /// <inheritdoc />
    internal override StatementSyntax[] GenerateStatements (ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Window with object initializer: var {variableName} = new Window { ... };
        ObjectCreationExpressionSyntax objectCreation = SyntaxHelpers.CreateObjectWithInitializer ("Window", node.Attributes);

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
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower ()}{i}";
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
                ObjectCreationExpressionSyntax childObjectCreation = SyntaxHelpers.CreateObjectWithInitializer (child.ElementTypeName, attributesWithoutId);

                // Extract local type name for variable naming
                string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
                string childVarName = $"{localTypeName.ToLower ()}{i}";

                // Always generate local variable declaration
                initializeComponentStatements.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                                IdentifierName("var"))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                            Identifier(childVarName))
                                        .WithInitializer(
                                            EqualsValueClause(childObjectCreation))))));

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

                // this.Add(childVarName);
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
                                    Argument (IdentifierName (childVarName)))))));
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

        // Collect using directives
        var usings = new List<UsingDirectiveSyntax>
        {
            UsingDirective(QualifiedName(
                QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                IdentifierName("Views"))),
            UsingDirective(QualifiedName(
                QualifiedName(IdentifierName("Terminal"), IdentifierName("Gui")),
                IdentifierName("ViewBase")))
        };

        // Collect all namespaces from the element tree
        var allNamespaces = CollectAllNamespaces(node);

        // Add usings for all collected namespaces
        foreach (var ns in allNamespaces.Where(ns => !string.IsNullOrEmpty(ns) && ns != "Terminal.Gui.Views"))
        {
            // Parse the namespace into qualified name
            var parts = ns.Split('.');
            NameSyntax qualifiedName = IdentifierName(parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                qualifiedName = QualifiedName(qualifiedName, IdentifierName(parts[i]));
            }
            usings.Add(UsingDirective(qualifiedName));
        }

        // Build the compilation unit with using directives
        CompilationUnitSyntax compilationUnit = CompilationUnit ()
            .WithUsings (List (usings))
            .WithMembers (SingletonList<MemberDeclarationSyntax> (namespaceDeclaration))
            .NormalizeWhitespace ();

        // Add #nullable enable directive at the top
        return "#nullable enable\n" + compilationUnit.ToFullString ();
    }

    /// <summary>
    /// Extracts the local type name from a qualified type name.
    /// </summary>
    private static string GetLocalTypeName(string qualifiedTypeName)
    {
        int lastDot = qualifiedTypeName.LastIndexOf('.');
        return lastDot >= 0 ? qualifiedTypeName.Substring(lastDot + 1) : qualifiedTypeName;
    }

    /// <summary>
    /// Recursively collects all C# namespaces from the element tree.
    /// Maps XML namespace URIs to C# namespaces and filters out XML schema namespaces.
    /// </summary>
    private static HashSet<string> CollectAllNamespaces(ElementNode node)
    {
        var namespaces = new HashSet<string>();

        // Add C# namespaces from current node (filter out XML schema namespaces)
        foreach (var nsUri in node.Namespaces.Values)
        {
            if (!string.IsNullOrEmpty(nsUri) && 
                !nsUri.StartsWith("http://www.w3.org/") && 
                !nsUri.StartsWith("http://schemas.microsoft.com/"))
            {
                // Map XML namespace URI to C# namespace
                string csNamespace = MapXmlNamespaceUriToCSharp(nsUri);
                namespaces.Add(csNamespace);
            }
        }

        // Recursively collect from children
        foreach (var child in node.Children)
        {
            var childNamespaces = CollectAllNamespaces(child);
            foreach (var ns in childNamespaces)
            {
                namespaces.Add(ns);
            }
        }

        return namespaces;
    }

    /// <summary>
    /// Maps an XML namespace URI to a C# namespace.
    /// Supports XAML-style clr-namespace syntax: clr-namespace:Namespace.Name or clr-namespace:Namespace.Name;assembly=AssemblyName
    /// </summary>
    private static string MapXmlNamespaceUriToCSharp(string uri)
    {
        if (uri == "http://schemas.terminal.gui/xtui")
        {
            return "Terminal.Gui.Views";
        }
        
        // Parse XAML-style clr-namespace declarations
        // Format: clr-namespace:MyApp.ViewModels or clr-namespace:MyApp.ViewModels;assembly=MyAssembly
        if (uri.StartsWith("clr-namespace:"))
        {
            string nsDeclaration = uri.Substring("clr-namespace:".Length);
            int assemblyIndex = nsDeclaration.IndexOf(";");
            if (assemblyIndex > 0)
            {
                // Extract namespace before assembly reference
                return nsDeclaration.Substring(0, assemblyIndex);
            }
            return nsDeclaration;
        }
        
        // Legacy support: plain namespace strings are used as-is
        return uri;
    }
}