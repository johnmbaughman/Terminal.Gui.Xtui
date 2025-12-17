using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generators.Helpers;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Unit tests for ChildProcessingHelpers class.
/// Tests helper methods for processing child elements in generators,
/// including variable naming, child iteration, and Add() method invocation.
/// </summary>
public class ChildProcessingHelpersTests
{
    #region CreateChildVariableName Tests

    [Fact]
    public void CreateChildVariableName_WithSimpleTypeName_CreatesIndexedName()
    {
        // Arrange
        string typeName = "Label";
        int index = 0;

        // Act
        string result = ChildProcessingHelpers.CreateChildVariableName(typeName, index);

        // Assert
        Assert.Equal("label0", result);
    }

    [Fact]
    public void CreateChildVariableName_WithQualifiedTypeName_ExtractsLocalName()
    {
        // Arrange
        string typeName = "Terminal.Gui.Label";
        int index = 5;

        // Act
        string result = ChildProcessingHelpers.CreateChildVariableName(typeName, index);

        // Assert
        Assert.Equal("label5", result);
    }

    [Fact]
    public void CreateChildVariableName_WithMultipleChildren_CreatesUniqueNames()
    {
        // Arrange
        string typeName = "Button";

        // Act
        string name0 = ChildProcessingHelpers.CreateChildVariableName(typeName, 0);
        string name1 = ChildProcessingHelpers.CreateChildVariableName(typeName, 1);
        string name2 = ChildProcessingHelpers.CreateChildVariableName(typeName, 2);

        // Assert
        Assert.Equal("button0", name0);
        Assert.Equal("button1", name1);
        Assert.Equal("button2", name2);
    }

    [Fact]
    public void CreateChildVariableName_WithNullTypeName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.CreateChildVariableName(null!, 0));
    }

    [Fact]
    public void CreateChildVariableName_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            ChildProcessingHelpers.CreateChildVariableName("Label", -1));
    }

    #endregion

    #region CreateAddMethodCall Tests

    [Fact]
    public void CreateAddMethodCall_WithParentAndChild_CreatesCorrectSyntax()
    {
        // Arrange
        string parentVarName = "window";
        string childVarName = "label0";

        // Act
        ExpressionStatementSyntax result = ChildProcessingHelpers.CreateAddMethodCall(parentVarName, childVarName);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("window.Add(label0)", code);
    }

    [Fact]
    public void CreateAddMethodCall_WithDifferentNames_CreatesCorrectSyntax()
    {
        // Arrange
        string parentVarName = "statusBar";
        string childVarName = "shortcut1";

        // Act
        ExpressionStatementSyntax result = ChildProcessingHelpers.CreateAddMethodCall(parentVarName, childVarName);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("statusBar.Add(shortcut1)", code);
    }

    [Fact]
    public void CreateAddMethodCall_WithNullParentName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.CreateAddMethodCall(null!, "child"));
    }

    [Fact]
    public void CreateAddMethodCall_WithNullChildName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.CreateAddMethodCall("parent", null!));
    }

    #endregion

    #region ProcessChildElements Tests

    [Fact]
    public void ProcessChildElements_WithNoChildren_ReturnsEmptyList()
    {
        // Arrange
        var node = new ElementNode("Window", new Dictionary<string, string>());
        string parentVarName = "window";
        var mockFactory = new MockGeneratorFactory();

        // Act
        var result = ChildProcessingHelpers.ProcessChildElements(node, parentVarName, mockFactory);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ProcessChildElements_WithSingleChild_GeneratesStatementsAndAddCall()
    {
        // Arrange
        var child = new ElementNode("Label", new Dictionary<string, string> { { "Text", "Hello" } });
        var node = new ElementNode("Window", new Dictionary<string, string>());
        node.Children.Add(child);
        
        string parentVarName = "window";
        var mockFactory = new MockGeneratorFactory();

        // Act
        var result = ChildProcessingHelpers.ProcessChildElements(node, parentVarName, mockFactory);

        // Assert
        Assert.NotEmpty(result);
        // Should have child statements + Add() call
        string lastStatement = result[result.Count - 1].ToFullString();
        Assert.Contains("window.Add(label0)", lastStatement);
    }

    [Fact]
    public void ProcessChildElements_WithMultipleChildren_GeneratesCorrectSequence()
    {
        // Arrange
        var child1 = new ElementNode("Label", new Dictionary<string, string>());
        var child2 = new ElementNode("Button", new Dictionary<string, string>());
        var node = new ElementNode("Window", new Dictionary<string, string>());
        node.Children.Add(child1);
        node.Children.Add(child2);
        
        string parentVarName = "window";
        var mockFactory = new MockGeneratorFactory();

        // Act
        var result = ChildProcessingHelpers.ProcessChildElements(node, parentVarName, mockFactory);

        // Assert
        Assert.NotEmpty(result);
        string allCode = string.Join("\n", result.Select(s => s.ToFullString()));
        Assert.Contains("label0", allCode);
        Assert.Contains("button1", allCode);
        Assert.Contains("window.Add(label0)", allCode);
        Assert.Contains("window.Add(button1)", allCode);
    }

    [Fact]
    public void ProcessChildElements_WithQualifiedTypeNames_ExtractsLocalNames()
    {
        // Arrange
        var child = new ElementNode("Terminal.Gui.Views.Label", new Dictionary<string, string>());
        var node = new ElementNode("Terminal.Gui.Views.Window", new Dictionary<string, string>());
        node.Children.Add(child);
        
        string parentVarName = "window";
        var mockFactory = new MockGeneratorFactory();

        // Act
        var result = ChildProcessingHelpers.ProcessChildElements(node, parentVarName, mockFactory);

        // Assert
        string code = string.Join("\n", result.Select(s => s.ToFullString()));
        Assert.Contains("label0", code);
    }

    [Fact]
    public void ProcessChildElements_WithNullNode_ThrowsArgumentNullException()
    {
        // Arrange
        var mockFactory = new MockGeneratorFactory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.ProcessChildElements(null!, "parent", mockFactory));
    }

    [Fact]
    public void ProcessChildElements_WithNullParentVarName_ThrowsArgumentNullException()
    {
        // Arrange
        var node = new ElementNode("Window", new Dictionary<string, string>());
        var mockFactory = new MockGeneratorFactory();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.ProcessChildElements(node, null!, mockFactory));
    }

    [Fact]
    public void ProcessChildElements_WithNullGeneratorFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var node = new ElementNode("Window", new Dictionary<string, string>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.ProcessChildElements(node, "window", null!));
    }

    #endregion

    #region HasChildren Tests

    [Fact]
    public void HasChildren_WithNoChildren_ReturnsFalse()
    {
        // Arrange
        var node = new ElementNode("Window", new Dictionary<string, string>());

        // Act
        bool result = ChildProcessingHelpers.HasChildren(node);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasChildren_WithChildren_ReturnsTrue()
    {
        // Arrange
        var child = new ElementNode("Label", new Dictionary<string, string>());
        var node = new ElementNode("Window", new Dictionary<string, string>());
        node.Children.Add(child);

        // Act
        bool result = ChildProcessingHelpers.HasChildren(node);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasChildren_WithNullNode_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ChildProcessingHelpers.HasChildren(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_ProcessChildElements_MatchesGenericGeneratorPattern()
    {
        // This test verifies that ChildProcessingHelpers can reproduce the pattern from GenericGenerator
        // Arrange
        var child1 = new ElementNode("Terminal.Gui.Label", new Dictionary<string, string>());
        var child2 = new ElementNode("Terminal.Gui.Button", new Dictionary<string, string>());
        var parent = new ElementNode("Terminal.Gui.Window", new Dictionary<string, string>());
        parent.Children.Add(child1);
        parent.Children.Add(child2);
        
        var mockFactory = new MockGeneratorFactory();

        // Act
        var statements = ChildProcessingHelpers.ProcessChildElements(parent, "window", mockFactory);

        // Assert - Should have child statements for each child + Add() calls
        Assert.NotEmpty(statements);
        string allCode = string.Join("\n", statements.Select(s => s.ToFullString()));
        
        // Verify child variable names follow pattern
        Assert.Contains("label0", allCode);
        Assert.Contains("button1", allCode);
        
        // Verify Add() method calls
        Assert.Contains("window.Add(label0)", allCode);
        Assert.Contains("window.Add(button1)", allCode);
    }

    #endregion

    #region Mock Helper Classes

    /// <summary>
    /// Mock generator factory for testing child processing
    /// </summary>
    private class MockGeneratorFactory : IGeneratorFactory
    {
        public Generator GetGenerator(string elementTypeName)
        {
            return new MockGenerator();
        }
    }

    /// <summary>
    /// Mock generator that returns simple variable declaration statements
    /// </summary>
    private class MockGenerator : Generator
    {
        public override StatementSyntax[] GenerateStatements(ElementNode node, string variableName, IGeneratorFactory generators)
        {
            // Return a simple variable declaration for testing
            // var {variableName} = new Type();
            string typeName = node.ElementTypeName.Contains('.') 
                ? node.ElementTypeName.Split('.').Last() 
                : node.ElementTypeName;
            
            var statement = LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier(variableName))
                            .WithInitializer(EqualsValueClause(
                                ObjectCreationExpression(IdentifierName(typeName))
                                    .WithArgumentList(ArgumentList()))))));
            
            return new[] { statement };
        }

        public override string GenerateClass(ElementNode node, string namespaceName, string className, IGeneratorFactory generators)
        {
            throw new NotImplementedException("Not needed for child processing tests");
        }
    }

    #endregion
}
