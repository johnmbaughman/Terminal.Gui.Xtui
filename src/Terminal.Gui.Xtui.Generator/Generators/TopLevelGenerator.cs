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

internal sealed class TopLevelGenerator : BaseGenerator
{
    internal override StatementSyntax[] GenerateStatements (ElementNode node, string variableName, IGeneratorFactory generators)
    {
        // Create Toplevel with object initializer: var {variableName} = new Toplevel { ... };
        ObjectCreationExpressionSyntax objectCreation = CreateObjectWithInitializer ("Toplevel", node.Attributes);

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
        ];

        // Process children
        if (node.Children.Count <= 0)
        {
            return statements.ToArray ();
        }

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            // Extract local type name for variable naming
            string localTypeName = child.ElementTypeName.Contains ('.') ? child.ElementTypeName.Split ('.').Last () : child.ElementTypeName;
            string childVarName = $"{localTypeName.ToLower ()}{i}";
            BaseGenerator childGenerator = generators.GetGenerator (child.ElementTypeName);
            StatementSyntax[] childStatements = childGenerator.GenerateStatements (child, childVarName, generators);

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

        return statements.ToArray ();
    }

    public override string GenerateClass (ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
    {
        List<StatementSyntax> initializeComponentStatements = [];
        List<FieldDeclarationSyntax> fieldDeclarations = [];
        Dictionary<string, string> variableToFieldMap = new (); // Maps local var names to field names
        HashSet<string> processedFields = []; // Track which fields have been declared

        // Identify resource children: They have x:Key attribute and are typically converters or other non-visual resources
        // Resources are children added from property element syntax like <Runnable.Resources>
        List<ElementNode> resourceChildren = node.Children.Where (c => c.Attributes.ContainsKey ("x:Key")).ToList ();
        List<ElementNode> visualChildren = node.Children.Except (resourceChildren).ToList ();

        if (visualChildren.Count > 0)
        {
            for (int i = 0; i < visualChildren.Count; i++)
            {
                ElementNode child = visualChildren[i];

                string? controlId = child.Attributes.TryGetValue ("Id", out string? id) ? id : null;
                // Extract local type name for variable naming
                string localTypeName = child.ElementTypeName.Contains ('.') ? child.ElementTypeName.Split ('.').Last () : child.ElementTypeName;
                string childVarName = !string.IsNullOrEmpty (controlId) ? controlId! : $"{localTypeName.ToLower ()}{i}";

                // Get the appropriate Terminal.Gui.Xtui.Generator.Helpers.Generator for this child type and generate statements
                BaseGenerator childGenerator = generators.GetGenerator (child.ElementTypeName);
                StatementSyntax[] childStatements = childGenerator.GenerateStatements (child, childVarName, generators);

                // Process statements: convert local variable declarations to field assignments
                // and collect field declarations
                initializeComponentStatements.AddRange (childStatements.Select (statement => ProcessStatement (statement, fieldDeclarations, variableToFieldMap, processedFields)).OfType<StatementSyntax> ());

                // If the child is a MenuBar or StatusBar, emit an Add call so it's added to
                // the Toplevel even when the caller constructor does not explicitly add it.
                if (!string.Equals (localTypeName, "MenuBar", StringComparison.Ordinal)
                    && !string.Equals (localTypeName, "StatusBar", StringComparison.Ordinal))
                {
                    continue;
                }

                // Use field reference since the variable is now a field
                string fieldName = !string.IsNullOrEmpty (controlId) ? controlId! : childVarName;
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
                                       Argument (
                                                 MemberAccessExpression (
                                                     SyntaxKind.SimpleMemberAccessExpression,
                                                     ThisExpression (),
                                                     IdentifierName (fieldName))))))));
            }
        }

        // Process Resources if present (children with x:Key attributes)
        List<(string key, string typeName)> resources = [];
        foreach (ElementNode? resourceChild in resourceChildren)
        {
            // Each child is a resource with x:Key attribute
            if (!resourceChild.Attributes.TryGetValue ("x:Key", out string? key))
            {
                continue;
            }

            resources.Add ((key, resourceChild.ElementTypeName));

            // Emit field: private static readonly TypeName _res_Key = new TypeName();
            string fieldName = "_res_" + key;
            fieldDeclarations.Add (
                FieldDeclaration (
                    VariableDeclaration (
                        IdentifierName (resourceChild.ElementTypeName))
                    .WithVariables (SingletonSeparatedList (
                        VariableDeclarator (fieldName)
                        .WithInitializer (EqualsValueClause (
                            ObjectCreationExpression (
                            IdentifierName (resourceChild.ElementTypeName))
                            .WithArgumentList (ArgumentList ()))))))
                    .WithModifiers (TokenList (
                        Token (SyntaxKind.PrivateKeyword),
                        Token (SyntaxKind.StaticKeyword),
                        Token (SyntaxKind.ReadOnlyKeyword))));

            // Emit call in InitializeComponent: __XtuiGeneratedHelpers.SetResource(this, "Key", _res_Key);
            initializeComponentStatements.Insert (0,
                ExpressionStatement (
                    InvocationExpression (
                        MemberAccessExpression (
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName ("__XtuiGeneratedHelpers"),
                            IdentifierName ("SetResource")))
                    .WithArgumentList (ArgumentList (SeparatedList (new[]
                    {
                        Argument(ThisExpression()),
                        Argument(LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(key))),
                        Argument(IdentifierName(fieldName))
                    })))));
        }

        MethodDeclarationSyntax initializeComponent = MethodDeclaration (
                PredefinedType (Token (SyntaxKind.VoidKeyword)),
                Identifier ("InitializeComponent"))
            .WithModifiers (TokenList (Token (SyntaxKind.PrivateKeyword)))
            .WithBody (Block (initializeComponentStatements));

        List<MemberDeclarationSyntax> members = [];

        // Add __XtuiGeneratedHelpers class if there are resources
        if (resources.Count > 0)
        {
            members.Add (GenerateHelpersClass ());
        }

        members.AddRange (fieldDeclarations);
        members.Add (initializeComponent);

        // Use the actual root element type name as the base class
        string baseTypeName = node.ElementTypeName.Contains ('.')
            ? node.ElementTypeName.Split ('.').Last ()
            : node.ElementTypeName;

        ClassDeclarationSyntax classDeclaration = ClassDeclaration (className)
            .WithModifiers (TokenList (
                Token (SyntaxKind.PublicKeyword),
                Token (SyntaxKind.PartialKeyword)))
            .WithBaseList (BaseList (SingletonSeparatedList<BaseTypeSyntax> (
                SimpleBaseType (IdentifierName (baseTypeName)))))
            .WithMembers (List (members));

        NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration (IdentifierName (namespaceName))
            .WithMembers (SingletonList<MemberDeclarationSyntax> (classDeclaration));

        // Collect using directives
        List<UsingDirectiveSyntax> usings =
        [
            UsingDirective (IdentifierName ("System")),
            UsingDirective (
                QualifiedName (
                    IdentifierName ("System"),
                    IdentifierName ("Collections"))),

            UsingDirective (
                QualifiedName (
                    QualifiedName (IdentifierName ("System"), IdentifierName ("Collections")),
                    IdentifierName ("Generic"))),

            UsingDirective (
                QualifiedName (
                QualifiedName (IdentifierName ("System"), IdentifierName ("Runtime")),
                IdentifierName ("CompilerServices"))),

            UsingDirective (
                QualifiedName (
                QualifiedName (IdentifierName ("Terminal"), IdentifierName ("Gui")),
                IdentifierName ("Views"))),

            UsingDirective (
                QualifiedName (
                QualifiedName (IdentifierName ("Terminal"), IdentifierName ("Gui")),
                IdentifierName ("ViewBase")))
        ];

        // Collect all namespaces from the element tree
        HashSet<string> allNamespaces = CollectAllNamespaces (node);

        // Add usings for all collected namespaces
        foreach (string? ns in allNamespaces.Where (ns => !string.IsNullOrEmpty (ns) && ns != "Terminal.Gui.Views"))
        {
            // Parse the namespace into qualified name
            string[] parts = ns.Split ('.');
            NameSyntax qualifiedName = IdentifierName (parts[0]);
            for (int i = 1; i < parts.Length; i++)
            {
                qualifiedName = QualifiedName (qualifiedName, IdentifierName (parts[i]));
            }
            usings.Add (UsingDirective (qualifiedName));
        }

        CompilationUnitSyntax compilationUnit = CompilationUnit ()
            .WithUsings (List (usings))
            .WithMembers (SingletonList<MemberDeclarationSyntax> (namespaceDeclaration))
            .NormalizeWhitespace ();

        return "#nullable enable\n" + compilationUnit.ToFullString ();
    }

    /// <summary>
    /// Process a statement from a child Terminal.Gui.Xtui.Generator.Helpers.Generator, transforming local variable declarations
    /// to field assignments and collecting field declarations.
    /// </summary>
    private static StatementSyntax ProcessStatement (
        StatementSyntax statement,
        List<FieldDeclarationSyntax> fieldDeclarations,
        Dictionary<string, string> variableToFieldMap,
        HashSet<string> processedFields)
    {
        // If this is a local variable declaration, transform it to a field assignment
        if (statement is not LocalDeclarationStatementSyntax localDecl)
        {
            return ReplaceVariableReferences (statement, variableToFieldMap);
        }

        VariableDeclaratorSyntax? variable = localDecl.Declaration.Variables.FirstOrDefault ();

        if (variable?.Initializer == null)
        {
            return ReplaceVariableReferences (statement, variableToFieldMap);
        }

        string varName = variable.Identifier.Text;

        // Only create field if not already processed
        if (processedFields.Contains (varName))
        {
            return ExpressionStatement (
                AssignmentExpression (
                    SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression (),
                        IdentifierName (varName)),
                        variable.Initializer.Value));
        }

        variableToFieldMap[varName] = varName;

        // Extract type from the initializer (ObjectCreationExpression)
        string typeName;
        if (variable.Initializer.Value is ObjectCreationExpressionSyntax objCreation)
        {
            // Get the type from the object creation expression
            typeName = objCreation.Type.ToString ();
        }
        else
        {
            // Fallback: try to get it from the declaration type if it's not 'var'
            string declType = localDecl.Declaration.Type.ToString ();
            if (declType != "var" && declType != "var?")
            {
                typeName = declType.TrimEnd ('?');
            }
            else
            {
                // Can't determine type, skip
                return statement;
            }
        }

        // Create field declaration
        fieldDeclarations.Add (
            FieldDeclaration (
                VariableDeclaration (
                    NullableType (IdentifierName (typeName)))
                        .WithVariables (
                            SingletonSeparatedList (
                                VariableDeclarator (Identifier (varName)))))
                                   .WithModifiers (TokenList (Token (SyntaxKind.PrivateKeyword))));

        processedFields.Add (varName);

        // Transform to field assignment: this.fieldName = initializer;
        return ExpressionStatement (
            AssignmentExpression (
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression (),
                    IdentifierName (varName)),
                variable.Initializer.Value));

        // For non-declaration statements, replace any references to mapped variables
        // with field references (this.fieldName)
    }

    /// <summary>
    /// Replace variable references with field references in a statement.
    /// </summary>
    private static StatementSyntax ReplaceVariableReferences (
        StatementSyntax statement,
        Dictionary<string, string> variableToFieldMap)
    {
        if (variableToFieldMap.Count == 0)
        {
            return statement;
        }

        // Use a syntax rewriter to replace identifier names
        VariableToFieldRewriter rewriter = new (variableToFieldMap);
        return (StatementSyntax)rewriter.Visit (statement);
    }

    /// <summary>
    /// Syntax rewriter that replaces variable identifiers with field access expressions.
    /// </summary>
    private class VariableToFieldRewriter (Dictionary<string, string> variableToFieldMap) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitIdentifierName (IdentifierNameSyntax node)
        {
            string identifier = node.Identifier.Text;

            // If this identifier is a mapped variable, replace with this.fieldName
            if (!variableToFieldMap.TryGetValue (identifier, out string? value))
            {
                return base.VisitIdentifierName (node);
            }

            // Check if this is already part of a member access (to avoid this.this.field)
            if (node.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Expression == node)
            {
                // Don't replace if it's the left side of a member access
                return base.VisitIdentifierName (node);
            }

            return MemberAccessExpression (
                SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression (),
                IdentifierName (value));
        }
    }

    private static ObjectCreationExpressionSyntax CreateObjectWithInitializer (
        string fullTypeName,
        Dictionary<string, string> attributes)
    {
        // Extract local type name for object creation
        string typeName = fullTypeName.Contains ('.') ? fullTypeName.Split ('.').Last () : fullTypeName;
        ObjectCreationExpressionSyntax objectCreation = ObjectCreationExpression (IdentifierName (typeName))
            .WithArgumentList (ArgumentList ());

        if (attributes.Count <= 0)
        {
            return objectCreation;
        }

        IEnumerable<AssignmentExpressionSyntax> assignments = attributes.Select (attr =>
            AssignmentExpression (
                SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName (attr.Key),
                    ObjectParsingHelpers.ParseValueWithType (attr.Value, attr.Key)));

        InitializerExpressionSyntax initializer = InitializerExpression (
            SyntaxKind.ObjectInitializerExpression,
            SeparatedList<ExpressionSyntax> (assignments));

        objectCreation = objectCreation.WithInitializer (initializer);

        return objectCreation;
    }

    /// <summary>
    /// Recursively collects all descendant ElementNodes from the tree, including nested children.
    /// Used to create private fields for ALL controls in the hierarchy, not just direct children.
    /// </summary>
    private static List<(ElementNode node, string varName, string fieldName, string typeName)> CollectAllDescendants (ElementNode parent, IGeneratorFactory generators)
    {
        List<(ElementNode, string, string, string)> descendants = [];
        CollectDescendantsRecursive (parent, descendants, new Dictionary<ElementNode, string> ());
        return descendants;
    }

    /// <summary>
    /// Recursive helper that walks the element tree depth-first and collects all nodes with their variable names.
    /// Skips container elements like MenuBarItems, MenuItems, and Shortcuts which are organizational wrappers.
    /// </summary>
    private static void CollectDescendantsRecursive (ElementNode node, List<(ElementNode, string, string, string)> descendants, Dictionary<ElementNode, string> nodeToVarName)
    {
        // List of container element types that don't generate actual controls
        HashSet<string> containerTypes = ["MenuBarItems", "MenuItems", "Shortcuts"];

        for (int i = 0; i < node.Children.Count; i++)
        {
            ElementNode child = node.Children[i];
            string localTypeName = child.ElementTypeName.Contains ('.') ? child.ElementTypeName.Split ('.').Last () : child.ElementTypeName;

            // Skip container elements - they don't generate actual control instances
            if (containerTypes.Contains (localTypeName))
            {
                // But still recursively process their children
                CollectDescendantsRecursive (child, descendants, nodeToVarName);
                continue;
            }

            string? controlId = child.Attributes.TryGetValue ("Id", out string? id) ? id : null;
            string childVarName = !string.IsNullOrEmpty (controlId) ? controlId! : $"{localTypeName.ToLower ()}{i}";
            string fieldName = !string.IsNullOrEmpty (controlId) ? controlId! : childVarName;

            descendants.Add ((child, childVarName, fieldName, localTypeName));
            nodeToVarName[child] = childVarName;

            // Recursively collect grandchildren
            CollectDescendantsRecursive (child, descendants, nodeToVarName);
        }
    }

    /// <summary>
    /// Generates the __XtuiGeneratedHelpers nested class for Resources and binding support.
    /// This class provides per-instance Resources storage using ConditionalWeakTable.
    /// </summary>
    private static ClassDeclarationSyntax GenerateHelpersClass ()
    {
        ClassDeclarationSyntax holderClass = ClassDeclaration ("Holder")
            .WithModifiers (TokenList (Token (SyntaxKind.PrivateKeyword)))
            .WithMembers (List (new MemberDeclarationSyntax[]
            {
                PropertyDeclaration(
                    GenericName("IDictionary")
                        .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new[]
                        {
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            PredefinedType(Token(SyntaxKind.ObjectKeyword))
                        }))),
                    "Resources")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(AccessorList(List(new[]
                    {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(
                            GenericName("Dictionary")
                                .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new[]
                                {
                                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                                    PredefinedType(Token(SyntaxKind.ObjectKeyword))
                                }))))
                                .WithArgumentList(ArgumentList())))
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            }));

        return ClassDeclaration ("__XtuiGeneratedHelpers")
            .WithModifiers (TokenList (
                Token (SyntaxKind.PrivateKeyword),
                Token (SyntaxKind.StaticKeyword)))
            .WithMembers (List (new MemberDeclarationSyntax[]
            {
                holderClass,

                // private static readonly ConditionalWeakTable<View, Holder> s_table = new();
                FieldDeclaration(
                    VariableDeclaration(
                        GenericName("ConditionalWeakTable")
                            .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new[]
                            {
                                IdentifierName("View"),
                                IdentifierName("Holder")
                            }))))
                        .WithVariables(SingletonSeparatedList(
                            VariableDeclarator("s_table")
                                .WithInitializer(EqualsValueClause(
                                    ObjectCreationExpression(
                                        GenericName("ConditionalWeakTable")
                                            .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new[]
                                            {
                                                IdentifierName("View"),
                                                IdentifierName("Holder")
                                            }))))
                                    .WithArgumentList(ArgumentList()))))))
                    .WithModifiers(TokenList(
                        Token(SyntaxKind.PrivateKeyword),
                        Token(SyntaxKind.StaticKeyword),
                        Token(SyntaxKind.ReadOnlyKeyword))),

                // public static IDictionary<string, object> GetResources(View v) => s_table.GetOrCreateValue(v).Resources;
                MethodDeclaration(
                    GenericName("IDictionary")
                        .WithTypeArgumentList(TypeArgumentList(SeparatedList<TypeSyntax>(new[]
                        {
                            PredefinedType(Token(SyntaxKind.StringKeyword)),
                            PredefinedType(Token(SyntaxKind.ObjectKeyword))
                        }))),
                    "GetResources")
                    .WithModifiers(TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(ParameterList(SingletonSeparatedList(
                        Parameter(Identifier("v")).WithType(IdentifierName("View")))))
                    .WithExpressionBody(ArrowExpressionClause(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("s_table"),
                                    IdentifierName("GetOrCreateValue")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                    Argument(IdentifierName("v"))))),
                            IdentifierName("Resources"))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),

                // public static void SetResource(View v, string key, object value)
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    "SetResource")
                    .WithModifiers(TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(ParameterList(SeparatedList(new[]
                    {
                        Parameter(Identifier("v")).WithType(IdentifierName("View")),
                        Parameter(Identifier("key")).WithType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                        Parameter(Identifier("value")).WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    })))
                    .WithExpressionBody(ArrowExpressionClause(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            ElementAccessExpression(
                                InvocationExpression(
                                    IdentifierName("GetResources"))
                                    .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                        Argument(IdentifierName("v"))))))
                                .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(
                                    Argument(IdentifierName("key"))))),
                            IdentifierName("value"))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            }));
    }
}
