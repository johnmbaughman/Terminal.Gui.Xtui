using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Tests for SyntaxHelpers utility methods.
/// These tests verify the generation of Roslyn syntax nodes for common code patterns.
/// 
/// Pattern Analysis from existing generators:
/// - Variable declarations: var myVar = new Type();
/// - Assignment statements: this.field = value;
/// - Method calls: parent.Add(child);
/// - Object creation: new Type() or new Type { Prop = val }
/// </summary>
public class SyntaxHelpersTests
{
    [Fact]
    public void CreateVariableDeclaration_WithSimpleInitializer_GeneratesCorrectSyntax()
    {
        // Arrange
        string variableName = "button0";
        string typeName = "Button";

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateVariableDeclaration(variableName, typeName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("var button0", code);
        Assert.Contains("new Button()", code);
    }

    [Fact]
    public void CreateVariableDeclaration_WithNullVariableName_ThrowsArgumentNullException()
    {
        // Arrange
        string? variableName = null;
        string typeName = "Label";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateVariableDeclaration(variableName!, typeName));
    }

    [Fact]
    public void CreateVariableDeclaration_WithNullTypeName_ThrowsArgumentNullException()
    {
        // Arrange
        string variableName = "label0";
        string? typeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateVariableDeclaration(variableName, typeName!));
    }

    [Fact]
    public void CreateFieldAssignment_GeneratesThisFieldAssignment()
    {
        // Arrange
        string fieldName = "menuBar";
        string variableName = "menubar0";

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateFieldAssignment(fieldName, variableName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("this.menuBar = menubar0", code);
    }

    [Fact]
    public void CreateFieldAssignment_WithNullFieldName_ThrowsArgumentNullException()
    {
        // Arrange
        string? fieldName = null;
        string variableName = "value";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateFieldAssignment(fieldName!, variableName));
    }

    [Fact]
    public void CreateFieldAssignment_WithNullVariableName_ThrowsArgumentNullException()
    {
        // Arrange
        string fieldName = "field";
        string? variableName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateFieldAssignment(fieldName, variableName!));
    }

    [Fact]
    public void CreateMethodCall_WithSingleArgument_GeneratesCorrectSyntax()
    {
        // Arrange
        string targetVariable = "window";
        string methodName = "Add";
        string argumentVariable = "button0";

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateMethodCall(targetVariable, methodName, argumentVariable);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("window.Add(button0)", code);
    }

    [Fact]
    public void CreateMethodCall_WithMultipleArguments_GeneratesCorrectSyntax()
    {
        // Arrange
        string targetVariable = "statusBar";
        string methodName = "Add";
        string[] arguments = new[] { "shortcut0", "shortcut1", "shortcut2" };

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateMethodCall(targetVariable, methodName, arguments);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("statusBar.Add(shortcut0, shortcut1, shortcut2)", code);
    }

    [Fact]
    public void CreateMethodCall_WithNoArguments_GeneratesCorrectSyntax()
    {
        // Arrange
        string targetVariable = "view";
        string methodName = "Refresh";
        string[] arguments = Array.Empty<string>();

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateMethodCall(targetVariable, methodName, arguments);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("view.Refresh()", code);
    }

    [Fact]
    public void CreateMethodCall_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        string? targetVariable = null;
        string methodName = "Add";
        string argumentVariable = "child";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateMethodCall(targetVariable!, methodName, argumentVariable));
    }

    [Fact]
    public void CreateMethodCall_WithNullMethodName_ThrowsArgumentNullException()
    {
        // Arrange
        string targetVariable = "parent";
        string? methodName = null;
        string argumentVariable = "child";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateMethodCall(targetVariable, methodName!, argumentVariable));
    }

    [Fact]
    public void CreateObjectCreation_WithNoInitializer_GeneratesSimpleNew()
    {
        // Arrange
        string typeName = "Window";

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateObjectCreation(typeName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("Window()", code);
        Assert.Contains("new", code);
    }

    [Fact]
    public void CreateObjectCreation_WithNullTypeName_ThrowsArgumentNullException()
    {
        // Arrange
        string? typeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateObjectCreation(typeName!));
    }

    [Fact]
    public void CreateThisFieldDeclaration_GeneratesPrivateNullableField()
    {
        // Arrange
        string fieldName = "statusBar";
        string typeName = "StatusBar";

        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateThisFieldDeclaration(fieldName, typeName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains("private StatusBar? statusBar", code);
    }

    [Fact]
    public void CreateThisFieldDeclaration_WithNullFieldName_ThrowsArgumentNullException()
    {
        // Arrange
        string? fieldName = null;
        string typeName = "Label";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateThisFieldDeclaration(fieldName!, typeName));
    }

    [Fact]
    public void CreateThisFieldDeclaration_WithNullTypeName_ThrowsArgumentNullException()
    {
        // Arrange
        string fieldName = "field";
        string? typeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
                .CreateThisFieldDeclaration(fieldName, typeName!));
    }

    [Theory]
    [InlineData("label0", "Label")]
    [InlineData("myButton", "Button")]
    [InlineData("_privateField", "TextField")]
    public void CreateVariableDeclaration_WithVariousNames_GeneratesCorrectVariables(
        string variableName, string typeName)
    {
        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateVariableDeclaration(variableName, typeName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains($"var {variableName}", code);
        Assert.Contains($"new {typeName}()", code);
    }

    [Theory]
    [InlineData("field1", "var1")]
    [InlineData("_myField", "myVariable")]
    [InlineData("statusBar", "statusbar0")]
    public void CreateFieldAssignment_WithVariousNames_GeneratesCorrectAssignments(
        string fieldName, string variableName)
    {
        // Act
        var result = Terminal.Gui.Xtui.Generators.Helpers.SyntaxHelpers
            .CreateFieldAssignment(fieldName, variableName);

        // Assert
        Assert.NotNull(result);
        string code = result.ToFullString();
        Assert.Contains($"this.{fieldName} = {variableName}", code);
    }
}
