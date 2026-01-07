using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generator.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Comprehensive test suite for FieldTransformationHelpers (>95% coverage target).
/// Tests the variable→field transformation logic extracted from TopLevelGenerator.
/// </summary>
public class FieldTransformationHelpersTests
{
    #region ProcessStatement Tests

    [Fact]
    public void ProcessStatement_WithLocalDeclaration_CreatesFieldAndFieldAssignment()
    {
        // Arrange: var button0 = new Button { ... };
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("button0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Button"))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ExpressionStatementSyntax>(result);
        
        // Should create field assignment: this.button0 = new Button { ... };
        var exprStmt = (ExpressionStatementSyntax)result;
        Assert.IsType<AssignmentExpressionSyntax>(exprStmt.Expression);
        
        var assignment = (AssignmentExpressionSyntax)exprStmt.Expression;
        Assert.IsType<MemberAccessExpressionSyntax>(assignment.Left);
        
        var memberAccess = (MemberAccessExpressionSyntax)assignment.Left;
        Assert.IsType<ThisExpressionSyntax>(memberAccess.Expression);
        Assert.Equal("button0", memberAccess.Name.Identifier.Text);

        // Should add field declaration
        Assert.Single(fieldDeclarations);
        var fieldDecl = fieldDeclarations[0];
        Assert.Contains("private", fieldDecl.ToFullString());
        Assert.Contains("Button?", fieldDecl.ToFullString());
        Assert.Contains("button0", fieldDecl.ToFullString());

        // Should add to maps
        Assert.Single(variableToFieldMap);
        Assert.Equal("button0", variableToFieldMap["button0"]);
        Assert.Contains("button0", processedFields);
    }

    [Fact]
    public void ProcessStatement_WithDuplicateVariableName_OnlyCreatesFieldOnce()
    {
        // Arrange: Two statements with same variable name
        var localDecl1 = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("label0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Label"))
                                .WithArgumentList(ArgumentList()))))));

        var localDecl2 = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("label0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Label"))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        FieldTransformationHelpers.ProcessStatement(
            localDecl1, fieldDeclarations, variableToFieldMap, processedFields);
        FieldTransformationHelpers.ProcessStatement(
            localDecl2, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert - should only create one field
        Assert.Single(fieldDeclarations);
        Assert.Single(variableToFieldMap);
        Assert.Single(processedFields);
    }

    [Fact]
    public void ProcessStatement_WithExplicitType_ExtractsTypeName()
    {
        // Arrange: TextField textField0 = new TextField { ... };
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("TextField"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("textField0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("TextField"))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert
        Assert.NotNull(result);
        Assert.Single(fieldDeclarations);
        
        var fieldDecl = fieldDeclarations[0];
        Assert.Contains("TextField?", fieldDecl.ToFullString());
        Assert.Contains("textField0", fieldDecl.ToFullString());
    }

    [Fact]
    public void ProcessStatement_WithNullableType_HandlesCorrectly()
    {
        // Arrange: var? label0 = new Label { ... };
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(NullableType(IdentifierName("var")))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("label0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Label"))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert
        Assert.NotNull(result);
        Assert.Single(fieldDeclarations);
    }

    [Fact]
    public void ProcessStatement_WithoutInitializer_ReturnsOriginalStatement()
    {
        // Arrange: var button0; (no initializer)
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("button0")))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert - should return original statement unchanged
        Assert.NotNull(result);
        Assert.Empty(fieldDeclarations);
        Assert.Empty(variableToFieldMap);
        Assert.Empty(processedFields);
    }

    [Fact]
    public void ProcessStatement_WithNonDeclarationStatement_CallsReplaceVariableReferences()
    {
        // Arrange: button0.Add(label0); (expression statement)
        var exprStmt = ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("button0"),
                    IdentifierName("Add")))
                .WithArgumentList(ArgumentList(
                    SingletonSeparatedList(Argument(IdentifierName("label0"))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>
        {
            { "button0", "button0" },
            { "label0", "label0" }
        };
        var processedFields = new HashSet<string> { "button0", "label0" };

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            exprStmt, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert - should replace identifiers with field references
        Assert.NotNull(result);
        var resultStr = result.ToFullString();
        Assert.Contains("this.button0", resultStr);
        Assert.Contains("this.label0", resultStr);
    }

    #endregion

    #region ReplaceVariableReferences Tests

    [Fact]
    public void ReplaceVariableReferences_WithEmptyMap_ReturnsOriginalStatement()
    {
        // Arrange
        var stmt = ExpressionStatement(
            InvocationExpression(IdentifierName("DoSomething")));
        var emptyMap = new Dictionary<string, string>();

        // Act
        var result = FieldTransformationHelpers.ReplaceVariableReferences(stmt, emptyMap);

        // Assert
        Assert.Equal(stmt.ToFullString(), result.ToFullString());
    }

    [Fact]
    public void ReplaceVariableReferences_WithSingleVariable_ReplacesWithFieldAccess()
    {
        // Arrange: button0.Width = 10;
        var stmt = ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("button0"),
                    IdentifierName("Width")),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(10))));

        var map = new Dictionary<string, string> { { "button0", "button0" } };

        // Act
        var result = FieldTransformationHelpers.ReplaceVariableReferences(stmt, map);

        // Assert
        var resultStr = result.ToFullString();
        Assert.Contains("this.button0", resultStr);
        Assert.Contains("Width = 10", resultStr);
    }

    [Fact]
    public void ReplaceVariableReferences_WithMultipleVariables_ReplacesAll()
    {
        // Arrange: button0.Add(label0);
        var stmt = ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("button0"),
                    IdentifierName("Add")))
                .WithArgumentList(ArgumentList(
                    SingletonSeparatedList(Argument(IdentifierName("label0"))))));

        var map = new Dictionary<string, string>
        {
            { "button0", "button0" },
            { "label0", "label0" }
        };

        // Act
        var result = FieldTransformationHelpers.ReplaceVariableReferences(stmt, map);

        // Assert
        var resultStr = result.ToFullString();
        Assert.Contains("this.button0", resultStr);
        Assert.Contains("this.label0", resultStr);
    }

    [Fact]
    public void ReplaceVariableReferences_WithNestedExpression_ReplacesAllOccurrences()
    {
        // Arrange: this.Add(menuBar);
        var stmt = ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName("Add")))
                .WithArgumentList(ArgumentList(
                    SingletonSeparatedList(Argument(IdentifierName("menuBar"))))));

        var map = new Dictionary<string, string> { { "menuBar", "menuBar" } };

        // Act
        var result = FieldTransformationHelpers.ReplaceVariableReferences(stmt, map);

        // Assert
        var resultStr = result.ToFullString();
        Assert.Contains("this.Add(this.menuBar)", resultStr);
    }

    [Fact]
    public void ReplaceVariableReferences_WithUnmappedVariable_LeavesUnchanged()
    {
        // Arrange: otherVariable.DoSomething();
        var stmt = ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("otherVariable"),
                    IdentifierName("DoSomething"))));

        var map = new Dictionary<string, string> { { "button0", "button0" } };

        // Act
        var result = FieldTransformationHelpers.ReplaceVariableReferences(stmt, map);

        // Assert
        var resultStr = result.ToFullString();
        Assert.Contains("otherVariable.DoSomething", resultStr);
        Assert.DoesNotContain("this.otherVariable", resultStr);
    }

    #endregion

    #region VariableToFieldRewriter Tests

    [Fact]
    public void VariableToFieldRewriter_WithIdentifierInArgumentList_ReplacesWithFieldAccess()
    {
        // Arrange: Add(button0)
        var invocation = InvocationExpression(IdentifierName("Add"))
            .WithArgumentList(ArgumentList(
                SingletonSeparatedList(Argument(IdentifierName("button0")))));

        var map = new Dictionary<string, string> { { "button0", "button0" } };
        var rewriter = FieldTransformationHelpers.CreateVariableToFieldRewriter(map);

        // Act
        var result = rewriter.Visit(invocation);

        // Assert
        var resultStr = result?.ToFullString();
        Assert.NotNull(resultStr);
        Assert.Contains("this.button0", resultStr);
    }

    [Fact]
    public void VariableToFieldRewriter_WithIdentifierAsMethodReceiver_DoesNotDoubleReplace()
    {
        // Arrange: button0.Add(label0) should become this.button0.Add(this.label0)
        // NOT this.this.button0.Add(...)
        var invocation = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("button0"),
                IdentifierName("Add")))
            .WithArgumentList(ArgumentList(
                SingletonSeparatedList(Argument(IdentifierName("label0")))));

        var map = new Dictionary<string, string>
        {
            { "button0", "button0" },
            { "label0", "label0" }
        };
        var rewriter = FieldTransformationHelpers.CreateVariableToFieldRewriter(map);

        // Act
        var result = rewriter.Visit(invocation);

        // Assert
        var resultStr = result?.ToFullString();
        Assert.NotNull(resultStr);
        Assert.Contains("this.button0.Add", resultStr);
        Assert.DoesNotContain("this.this.", resultStr);
    }

    [Fact]
    public void VariableToFieldRewriter_WithMultipleMappedIdentifiers_ReplacesAllCorrectly()
    {
        // Arrange: complex expression with multiple identifiers
        var expr = BinaryExpression(
            SyntaxKind.AddExpression,
            IdentifierName("var1"),
            IdentifierName("var2"));

        var map = new Dictionary<string, string>
        {
            { "var1", "var1" },
            { "var2", "var2" }
        };
        var rewriter = FieldTransformationHelpers.CreateVariableToFieldRewriter(map);

        // Act
        var result = rewriter.Visit(expr);

        // Assert
        var resultStr = result?.ToFullString();
        Assert.NotNull(resultStr);
        Assert.Contains("this.var1", resultStr);
        Assert.Contains("this.var2", resultStr);
    }

    [Fact]
    public void VariableToFieldRewriter_PreservesUnmappedIdentifiers()
    {
        // Arrange
        var expr = BinaryExpression(
            SyntaxKind.AddExpression,
            IdentifierName("mappedVar"),
            IdentifierName("unmappedVar"));

        var map = new Dictionary<string, string> { { "mappedVar", "mappedVar" } };
        var rewriter = FieldTransformationHelpers.CreateVariableToFieldRewriter(map);

        // Act
        var result = rewriter.Visit(expr);

        // Assert
        var resultStr = result?.ToFullString();
        Assert.NotNull(resultStr);
        Assert.Contains("this.mappedVar", resultStr);
        Assert.Contains("unmappedVar", resultStr);
        Assert.DoesNotContain("this.unmappedVar", resultStr);
    }

    #endregion

    #region Integration Tests (Matching TopLevelGenerator Pattern)

    [Fact]
    public void Integration_ProcessMultipleStatements_MatchesTopLevelGeneratorBehavior()
    {
        // Arrange: Simulate TopLevelGenerator child processing
        // var button0 = new Button { ... };
        // var label0 = new Label { ... };
        // button0.Add(label0);
        
        var statements = new List<StatementSyntax>
        {
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("button0"))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(IdentifierName("Button"))
                                    .WithArgumentList(ArgumentList())))))),
            
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("label0"))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(IdentifierName("Label"))
                                    .WithArgumentList(ArgumentList())))))),
            
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("button0"),
                        IdentifierName("Add")))
                    .WithArgumentList(ArgumentList(
                        SingletonSeparatedList(Argument(IdentifierName("label0"))))))
        };

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();
        var transformedStatements = new List<StatementSyntax>();

        // Act - Process each statement like TopLevelGenerator does
        foreach (var stmt in statements)
        {
            var processed = FieldTransformationHelpers.ProcessStatement(
                stmt, fieldDeclarations, variableToFieldMap, processedFields);
            if (processed != null)
            {
                transformedStatements.Add(processed);
            }
        }

        // Assert - Should have:
        // 1. Two field declarations (Button? button0; Label? label0;)
        Assert.Equal(2, fieldDeclarations.Count);
        Assert.All(fieldDeclarations, fd => Assert.Contains("private", fd.ToFullString()));
        
        // 2. Three transformed statements (two field assignments + one Add call)
        Assert.Equal(3, transformedStatements.Count);
        
        // First statement: this.button0 = new Button { };
        Assert.Contains("this.button0", transformedStatements[0].ToFullString());
        Assert.Contains("Button", transformedStatements[0].ToFullString());
        
        // Second statement: this.label0 = new Label { };
        Assert.Contains("this.label0", transformedStatements[1].ToFullString());
        Assert.Contains("Label", transformedStatements[1].ToFullString());
        
        // Third statement: this.button0.Add(this.label0);
        var addStmt = transformedStatements[2].ToFullString();
        Assert.Contains("this.button0.Add", addStmt);
        Assert.Contains("this.label0", addStmt);
    }

    [Fact]
    public void Integration_WithIdAttribute_PreservesVariableName()
    {
        // Arrange: When TopLevelGenerator uses controlId from Id attribute
        // var myButton = new Button { ... };
        
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("myButton"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Button"))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert - Field should use same name as variable
        Assert.Single(fieldDeclarations);
        var fieldDecl = fieldDeclarations[0];
        Assert.Contains("myButton", fieldDecl.ToFullString());
        
        Assert.NotNull(result);
        Assert.Contains("this.myButton", result.ToFullString());
    }

    [Fact]
    public void Integration_ComplexScenario_MenuBarAndStatusBar()
    {
        // Arrange: Simulate TopLevelGenerator handling MenuBar and StatusBar
        // var menuBar = new MenuBar { ... };
        // var statusBar = new StatusBar { ... };
        // this.Add(this.menuBar);
        // this.Add(this.statusBar);
        
        var statements = new List<StatementSyntax>
        {
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("menuBar"))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(IdentifierName("MenuBar"))
                                    .WithArgumentList(ArgumentList())))))),
            
            LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("statusBar"))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(IdentifierName("StatusBar"))
                                    .WithArgumentList(ArgumentList())))))),
            
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName("Add")))
                    .WithArgumentList(ArgumentList(
                        SingletonSeparatedList(Argument(IdentifierName("menuBar")))))),
            
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName("Add")))
                    .WithArgumentList(ArgumentList(
                        SingletonSeparatedList(Argument(IdentifierName("statusBar"))))))
        };

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();
        var transformedStatements = new List<StatementSyntax>();

        // Act
        foreach (var stmt in statements)
        {
            var processed = FieldTransformationHelpers.ProcessStatement(
                stmt, fieldDeclarations, variableToFieldMap, processedFields);
            if (processed != null)
            {
                transformedStatements.Add(processed);
            }
        }

        // Assert
        Assert.Equal(2, fieldDeclarations.Count);
        Assert.Equal(4, transformedStatements.Count);
        
        // Verify MenuBar and StatusBar field declarations
        Assert.Contains(fieldDeclarations, fd => fd.ToFullString().Contains("MenuBar?") && fd.ToFullString().Contains("menuBar"));
        Assert.Contains(fieldDeclarations, fd => fd.ToFullString().Contains("StatusBar?") && fd.ToFullString().Contains("statusBar"));
        
        // Verify Add statements use field references
        var addStmt1 = transformedStatements[2].ToFullString();
        var addStmt2 = transformedStatements[3].ToFullString();
        Assert.Contains("this.Add(this.menuBar)", addStmt1);
        Assert.Contains("this.Add(this.statusBar)", addStmt2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ProcessStatement_WithComplexInitializer_HandlesCorrectly()
    {
        // Arrange: var button0 = new Button { Text = "Click Me", Width = 100 };
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("button0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(IdentifierName("Button"))
                                .WithArgumentList(ArgumentList())
                                .WithInitializer(
                                    InitializerExpression(SyntaxKind.ObjectInitializerExpression,
                                        SeparatedList<ExpressionSyntax>(new[]
                                        {
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName("Text"),
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal("Click Me"))),
                                            AssignmentExpression(
                                                SyntaxKind.SimpleAssignmentExpression,
                                                IdentifierName("Width"),
                                                LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    Literal(100)))
                                        }))))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert
        Assert.NotNull(result);
        Assert.Single(fieldDeclarations);
        
        var resultStr = result.ToFullString();
        Assert.Contains("this.button0", resultStr);
        Assert.Contains("Text", resultStr);
        Assert.Contains("Click Me", resultStr);
        Assert.Contains("Width", resultStr);
    }

    [Fact]
    public void ProcessStatement_WithQualifiedTypeName_ExtractsLocalType()
    {
        // Arrange: var button0 = new Terminal.Gui.Views.Button { ... };
        var localDecl = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("button0"))
                        .WithInitializer(EqualsValueClause(
                            ObjectCreationExpression(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("Terminal"),
                                            IdentifierName("Gui")),
                                        IdentifierName("Views")),
                                    IdentifierName("Button")))
                                .WithArgumentList(ArgumentList()))))));

        var fieldDeclarations = new List<FieldDeclarationSyntax>();
        var variableToFieldMap = new Dictionary<string, string>();
        var processedFields = new HashSet<string>();

        // Act
        var result = FieldTransformationHelpers.ProcessStatement(
            localDecl, fieldDeclarations, variableToFieldMap, processedFields);

        // Assert
        Assert.NotNull(result);
        Assert.Single(fieldDeclarations);
        
        // Field should use full qualified type name
        var fieldDecl = fieldDeclarations[0];
        var fieldStr = fieldDecl.ToFullString();
        Assert.Contains("button0", fieldStr);
    }

    #endregion
}
