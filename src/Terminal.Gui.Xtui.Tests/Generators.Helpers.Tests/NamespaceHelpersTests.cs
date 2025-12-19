using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generators.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Unit tests for NamespaceHelpers class.
/// Tests helper methods for creating namespace declarations, using directives,
/// and building qualified names from namespace strings.
/// </summary>
public class NamespaceHelpersTests
{
    #region CreateNamespaceDeclaration Tests

    [Fact]
    public void CreateNamespaceDeclaration_WithSimpleName_CreatesCorrectSyntax()
    {
        // Arrange
        string namespaceName = "MyApp";

        // Act
        NamespaceDeclarationSyntax result = NamespaceHelpers.CreateNamespaceDeclaration(namespaceName);

        // Assert
        string code = result.NormalizeWhitespace().ToFullString();
        Assert.Contains("namespace MyApp", code);
    }

    [Fact]
    public void CreateNamespaceDeclaration_WithQualifiedName_CreatesCorrectSyntax()
    {
        // Arrange
        string namespaceName = "Terminal.Gui.Views";

        // Act
        NamespaceDeclarationSyntax result = NamespaceHelpers.CreateNamespaceDeclaration(namespaceName);

        // Assert
        string code = result.NormalizeWhitespace().ToFullString();
        Assert.Contains("namespace Terminal.Gui.Views", code);
    }

    [Fact]
    public void CreateNamespaceDeclaration_WithMembers_AddsMembers()
    {
        // Arrange
        string namespaceName = "MyApp";
        ClassDeclarationSyntax classDecl = ClassDeclaration("MyClass");

        // Act
        NamespaceDeclarationSyntax result = NamespaceHelpers.CreateNamespaceDeclaration(namespaceName, classDecl);

        // Assert
        string code = result.NormalizeWhitespace().ToFullString();
        Assert.Contains("namespace MyApp", code);
        Assert.Contains("class MyClass", code);
    }

    [Fact]
    public void CreateNamespaceDeclaration_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NamespaceHelpers.CreateNamespaceDeclaration(null!));
    }

    [Fact]
    public void CreateNamespaceDeclaration_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => NamespaceHelpers.CreateNamespaceDeclaration(string.Empty));
    }

    #endregion

    #region CreateUsingDirective Tests

    [Fact]
    public void CreateUsingDirective_WithSimpleName_CreatesCorrectSyntax()
    {
        // Arrange
        string namespaceName = "System";

        // Act
        UsingDirectiveSyntax result = NamespaceHelpers.CreateUsingDirective(namespaceName);

        // Assert
        string code = result.ToFullString();
        Assert.Equal("using System;", code);
    }

    [Fact]
    public void CreateUsingDirective_WithQualifiedName_CreatesCorrectSyntax()
    {
        // Arrange
        string namespaceName = "System.Collections.Generic";

        // Act
        UsingDirectiveSyntax result = NamespaceHelpers.CreateUsingDirective(namespaceName);

        // Assert
        string code = result.ToFullString();
        Assert.Equal("using System.Collections.Generic;", code);
    }

    [Fact]
    public void CreateUsingDirective_WithTerminalGuiViews_CreatesCorrectSyntax()
    {
        // Arrange
        string namespaceName = "Terminal.Gui.Views";

        // Act
        UsingDirectiveSyntax result = NamespaceHelpers.CreateUsingDirective(namespaceName);

        // Assert
        string code = result.ToFullString();
        Assert.Equal("using Terminal.Gui.Views;", code);
    }

    [Fact]
    public void CreateUsingDirective_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NamespaceHelpers.CreateUsingDirective(null!));
    }

    [Fact]
    public void CreateUsingDirective_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => NamespaceHelpers.CreateUsingDirective(string.Empty));
    }

    #endregion

    #region BuildQualifiedName Tests

    [Fact]
    public void BuildQualifiedName_WithSinglePart_ReturnsIdentifierName()
    {
        // Arrange
        string namespaceName = "System";

        // Act
        NameSyntax result = NamespaceHelpers.BuildQualifiedName(namespaceName);

        // Assert
        Assert.IsType<IdentifierNameSyntax>(result);
        Assert.Equal("System", result.ToFullString());
    }

    [Fact]
    public void BuildQualifiedName_WithTwoParts_ReturnsQualifiedName()
    {
        // Arrange
        string namespaceName = "System.Text";

        // Act
        NameSyntax result = NamespaceHelpers.BuildQualifiedName(namespaceName);

        // Assert
        Assert.IsType<QualifiedNameSyntax>(result);
        string code = result.ToFullString();
        Assert.Equal("System.Text", code);
    }

    [Fact]
    public void BuildQualifiedName_WithThreeParts_ReturnsQualifiedName()
    {
        // Arrange
        string namespaceName = "Terminal.Gui.Views";

        // Act
        NameSyntax result = NamespaceHelpers.BuildQualifiedName(namespaceName);

        // Assert
        Assert.IsType<QualifiedNameSyntax>(result);
        string code = result.ToFullString();
        Assert.Equal("Terminal.Gui.Views", code);
    }

    [Fact]
    public void BuildQualifiedName_WithFourParts_ReturnsNestedQualifiedName()
    {
        // Arrange
        string namespaceName = "System.Collections.Generic.List";

        // Act
        NameSyntax result = NamespaceHelpers.BuildQualifiedName(namespaceName);

        // Assert
        Assert.IsType<QualifiedNameSyntax>(result);
        string code = result.ToFullString();
        Assert.Equal("System.Collections.Generic.List", code);
    }

    [Fact]
    public void BuildQualifiedName_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NamespaceHelpers.BuildQualifiedName(null!));
    }

    [Fact]
    public void BuildQualifiedName_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => NamespaceHelpers.BuildQualifiedName(string.Empty));
    }

    #endregion

    #region CreateCompilationUnit Tests

    [Fact]
    public void CreateCompilationUnit_WithNoUsingsOrMembers_CreatesEmptyUnit()
    {
        // Act
        CompilationUnitSyntax result = NamespaceHelpers.CreateCompilationUnit();

        // Assert
        string code = result.ToFullString();
        Assert.NotNull(code);
        Assert.Empty(result.Usings);
        Assert.Empty(result.Members);
    }

    [Fact]
    public void CreateCompilationUnit_WithUsings_AddsUsingDirectives()
    {
        // Arrange
        UsingDirectiveSyntax[] usings = new[]
        {
            NamespaceHelpers.CreateUsingDirective("System"),
            NamespaceHelpers.CreateUsingDirective("System.Linq")
        };

        // Act
        CompilationUnitSyntax result = NamespaceHelpers.CreateCompilationUnit(usings);

        // Assert
        Assert.Equal(2, result.Usings.Count);
        string code = result.ToFullString();
        Assert.Contains("using System;", code);
        Assert.Contains("using System.Linq;", code);
    }

    [Fact]
    public void CreateCompilationUnit_WithNamespace_AddsMember()
    {
        // Arrange
        NamespaceDeclarationSyntax ns = NamespaceHelpers.CreateNamespaceDeclaration("MyApp");

        // Act
        CompilationUnitSyntax result = NamespaceHelpers.CreateCompilationUnit(members: new[] { ns });

        // Assert
        Assert.Single(result.Members);
        string code = result.ToFullString();
        Assert.Contains("namespace MyApp", code);
    }

    [Fact]
    public void CreateCompilationUnit_WithUsingsAndMembers_CreatesFull()
    {
        // Arrange
        UsingDirectiveSyntax[] usings = new[]
        {
            NamespaceHelpers.CreateUsingDirective("System")
        };
        NamespaceDeclarationSyntax ns = NamespaceHelpers.CreateNamespaceDeclaration("MyApp");

        // Act
        CompilationUnitSyntax result = NamespaceHelpers.CreateCompilationUnit(usings, new[] { ns });

        // Assert
        string code = result.ToFullString();
        Assert.Contains("using System;", code);
        Assert.Contains("namespace MyApp", code);
    }

    #endregion

    #region AddNullableDirective Tests

    [Fact]
    public void AddNullableDirective_WithCompilationUnit_AddsDirective()
    {
        // Arrange
        CompilationUnitSyntax compilationUnit = NamespaceHelpers.CreateCompilationUnit();

        // Act
        string result = NamespaceHelpers.AddNullableDirective(compilationUnit);

        // Assert
        Assert.StartsWith("#nullable enable", result);
    }

    [Fact]
    public void AddNullableDirective_PreservesContent_AfterDirective()
    {
        // Arrange
        UsingDirectiveSyntax usingDirective = NamespaceHelpers.CreateUsingDirective("System");
        CompilationUnitSyntax compilationUnit = NamespaceHelpers.CreateCompilationUnit(new[] { usingDirective });

        // Act
        string result = NamespaceHelpers.AddNullableDirective(compilationUnit);

        // Assert
        Assert.StartsWith("#nullable enable\n", result);
        Assert.Contains("using System;", result);
    }

    [Fact]
    public void AddNullableDirective_WithNullCompilationUnit_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NamespaceHelpers.AddNullableDirective(null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_CreateFullCompilationUnit_WithAllComponents()
    {
        // Arrange
        string namespaceName = "Terminal.Gui.Generated";
        string className = "MyWindow";

        // Act
        UsingDirectiveSyntax[] usings = new[]
        {
            NamespaceHelpers.CreateUsingDirective("Terminal.Gui.Views"),
            NamespaceHelpers.CreateUsingDirective("Terminal.Gui.ViewBase")
        };

        ClassDeclarationSyntax classDecl = ClassDeclaration(className);
        NamespaceDeclarationSyntax ns = NamespaceHelpers.CreateNamespaceDeclaration(namespaceName, classDecl);
        CompilationUnitSyntax compilationUnit = NamespaceHelpers.CreateCompilationUnit(usings, new[] { ns });
        string code = NamespaceHelpers.AddNullableDirective(compilationUnit);

        // Assert
        Assert.StartsWith("#nullable enable", code);
        Assert.Contains("using Terminal.Gui.Views;", code);
        Assert.Contains("using Terminal.Gui.ViewBase;", code);
        Assert.Contains("namespace Terminal.Gui.Generated", code);
        Assert.Contains("class MyWindow", code);
    }

    [Fact]
    public void Integration_MatchesTopLevelGeneratorPattern()
    {
        // This test verifies that NamespaceHelpers can reproduce the pattern from TopLevelGenerator
        // Arrange
        string namespaceName = "MyApp";

        // Act - Build using pattern from TopLevelGenerator.cs lines 142-174
        NamespaceDeclarationSyntax ns = NamespaceHelpers.CreateNamespaceDeclaration(namespaceName);

        UsingDirectiveSyntax[] usings = new[]
        {
            NamespaceHelpers.CreateUsingDirective("Terminal.Gui.Views"),
            NamespaceHelpers.CreateUsingDirective("Terminal.Gui.ViewBase")
        };

        CompilationUnitSyntax compilationUnit = NamespaceHelpers.CreateCompilationUnit(usings, new[] { ns });
        string code = NamespaceHelpers.AddNullableDirective(compilationUnit);

        // Assert
        Assert.Contains("#nullable enable", code);
        Assert.Contains("using Terminal.Gui.Views;", code);
        Assert.Contains("using Terminal.Gui.ViewBase;", code);
        Assert.Contains("namespace MyApp", code);
    }

    #endregion
}
