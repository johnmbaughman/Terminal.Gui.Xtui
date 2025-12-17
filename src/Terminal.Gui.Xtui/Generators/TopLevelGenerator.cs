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
    internal override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
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
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower()}{i}";
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
        Dictionary<string, string> variableToFieldMap = new Dictionary<string, string>(); // Maps local var names to field names
        HashSet<string> processedFields = new HashSet<string>(); // Track which fields have been declared

        if (node.Children.Count > 0)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                ElementNode child = node.Children[i];

                string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;
                // Extract local type name for variable naming
                string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
                string childVarName = !string.IsNullOrEmpty(controlId) ? controlId! : $"{localTypeName.ToLower()}{i}";

                // Get the appropriate generator for this child type and generate statements
                Generator childGenerator = generators.GetGenerator(child.ElementTypeName);
                StatementSyntax[] childStatements = childGenerator.GenerateStatements(child, childVarName, generators);

                // Process statements: convert local variable declarations to field assignments
                // and collect field declarations
                foreach (var statement in childStatements)
                {
                    var processedStatement = ProcessStatement(statement, fieldDeclarations, variableToFieldMap, processedFields);
                    if (processedStatement != null)
                    {
                        initializeComponentStatements.Add(processedStatement);
                    }
                }

                // If the child is a MenuBar or StatusBar, emit an Add call so it's added to
                // the Toplevel even when the caller constructor does not explicitly add it.
                if (string.Equals(localTypeName, "MenuBar", StringComparison.Ordinal) ||
                    string.Equals(localTypeName, "StatusBar", StringComparison.Ordinal))
                {
                    // Use field reference since the variable is now a field
                    string fieldName = !string.IsNullOrEmpty(controlId) ? controlId! : childVarName;
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

        CompilationUnitSyntax compilationUnit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration))
            .NormalizeWhitespace();

        return "#nullable enable\n" + compilationUnit.ToFullString();
    }

    /// <summary>
    /// Process a statement from a child generator, transforming local variable declarations
    /// to field assignments and collecting field declarations.
    /// </summary>
    private static StatementSyntax? ProcessStatement(
        StatementSyntax statement,
        List<FieldDeclarationSyntax> fieldDeclarations,
        Dictionary<string, string> variableToFieldMap,
        HashSet<string> processedFields)
    {
        // If this is a local variable declaration, transform it to a field assignment
        if (statement is LocalDeclarationStatementSyntax localDecl)
        {
            var variable = localDecl.Declaration.Variables.FirstOrDefault();
            if (variable != null && variable.Initializer != null)
            {
                string varName = variable.Identifier.Text;
                string fieldName = varName; // Use same name for field
                
                // Only create field if not already processed
                if (!processedFields.Contains(fieldName))
                {
                    variableToFieldMap[varName] = fieldName;

                    // Extract type from the initializer (ObjectCreationExpression)
                    string typeName;
                    if (variable.Initializer.Value is ObjectCreationExpressionSyntax objCreation)
                    {
                        // Get the type from the object creation expression
                        typeName = objCreation.Type.ToString();
                    }
                    else
                    {
                        // Fallback: try to get it from the declaration type if it's not 'var'
                        var declType = localDecl.Declaration.Type.ToString();
                        if (declType != "var" && declType != "var?")
                        {
                            typeName = declType.TrimEnd('?');
                        }
                        else
                        {
                            // Can't determine type, skip
                            return statement;
                        }
                    }

                    // Create field declaration
                    fieldDeclarations.Add(
                        FieldDeclaration(
                            VariableDeclaration(
                                NullableType(IdentifierName(typeName)))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(fieldName)))))
                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword))));

                    processedFields.Add(fieldName);
                }

                // Transform to field assignment: this.fieldName = initializer;
                return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(fieldName)),
                        variable.Initializer.Value));
            }
        }

        // For non-declaration statements, replace any references to mapped variables
        // with field references (this.fieldName)
        return ReplaceVariableReferences(statement, variableToFieldMap);
    }

    /// <summary>
    /// Replace variable references with field references in a statement.
    /// </summary>
    private static StatementSyntax ReplaceVariableReferences(
        StatementSyntax statement,
        Dictionary<string, string> variableToFieldMap)
    {
        if (variableToFieldMap.Count == 0)
        {
            return statement;
        }

        // Use a syntax rewriter to replace identifier names
        var rewriter = new VariableToFieldRewriter(variableToFieldMap);
        return (StatementSyntax)rewriter.Visit(statement);
    }

    /// <summary>
    /// Syntax rewriter that replaces variable identifiers with field access expressions.
    /// </summary>
    private class VariableToFieldRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _variableToFieldMap;

        public VariableToFieldRewriter(Dictionary<string, string> variableToFieldMap)
        {
            _variableToFieldMap = variableToFieldMap;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            string identifier = node.Identifier.Text;
            
            // If this identifier is a mapped variable, replace with this.fieldName
            if (_variableToFieldMap.ContainsKey(identifier))
            {
                // Check if this is already part of a member access (to avoid this.this.field)
                if (node.Parent is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression == node)
                {
                    // Don't replace if it's the left side of a member access
                    return base.VisitIdentifierName(node);
                }

                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(_variableToFieldMap[identifier]));
            }

            return base.VisitIdentifierName(node);
        }
    }

    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer(
        string fullTypeName,
        Dictionary<string, string> attributes)
    {
        // Extract local type name for object creation
        string typeName = fullTypeName.Contains('.') ? fullTypeName.Split('.').Last() : fullTypeName;
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

    /// <summary>
    /// Extracts the local type name from a qualified type name.
    /// </summary>
    private static string GetLocalTypeName(string qualifiedTypeName)
    {
        int lastDot = qualifiedTypeName.LastIndexOf('.');
        return lastDot >= 0 ? qualifiedTypeName.Substring(lastDot + 1) : qualifiedTypeName;
    }

    /// <summary>
    /// Recursively collects all descendant ElementNodes from the tree, including nested children.
    /// Used to create private fields for ALL controls in the hierarchy, not just direct children.
    /// </summary>
    private static List<(ElementNode node, string varName, string fieldName, string typeName)> CollectAllDescendants(ElementNode parent, IGeneratorFactory generators)
    {
        var descendants = new List<(ElementNode, string, string, string)>();
        CollectDescendantsRecursive(parent, generators, descendants, new Dictionary<ElementNode, string>());
        return descendants;
    }

    /// <summary>
    /// Recursive helper that walks the element tree depth-first and collects all nodes with their variable names.
    /// Skips container elements like MenuBarItems, MenuItems, and Shortcuts which are organizational wrappers.
    /// </summary>
    private static void CollectDescendantsRecursive(ElementNode node, IGeneratorFactory generators, List<(ElementNode, string, string, string)> descendants, Dictionary<ElementNode, string> nodeToVarName)
    {
        // List of container element types that don't generate actual controls
        var containerTypes = new HashSet<string> { "MenuBarItems", "MenuItems", "Shortcuts" };
        
        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            string localTypeName = child.ElementTypeName.Contains('.') ? child.ElementTypeName.Split('.').Last() : child.ElementTypeName;
            
            // Skip container elements - they don't generate actual control instances
            if (containerTypes.Contains(localTypeName))
            {
                // But still recursively process their children
                CollectDescendantsRecursive(child, generators, descendants, nodeToVarName);
                continue;
            }
            
            string? controlId = child.Attributes.TryGetValue("Id", out string? id) ? id : null;
            string childVarName = !string.IsNullOrEmpty(controlId) ? controlId! : $"{localTypeName.ToLower()}{i}";
            string fieldName = !string.IsNullOrEmpty(controlId) ? controlId! : childVarName;
            
            descendants.Add((child, childVarName, fieldName, localTypeName));
            nodeToVarName[child] = childVarName;
            
            // Recursively collect grandchildren
            CollectDescendantsRecursive(child, generators, descendants, nodeToVarName);
        }
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
