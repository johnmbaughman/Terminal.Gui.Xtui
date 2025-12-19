using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generators.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Unit tests for ClassGenerationHelpers class.
/// Tests helper methods for generating class declarations, method declarations,
/// and field declarations with proper modifiers and structure.
/// </summary>
public class ClassGenerationHelpersTests
{
    #region CreateClass Tests

    [Fact]
    public void CreateClass_WithSimpleName_CreatesBasicClass()
    {
        // Arrange
        string className = "MyWindow";

        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(className);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("class MyWindow", code);
    }

    [Fact]
    public void CreateClass_WithPublicModifier_CreatesPublicClass()
    {
        // Arrange
        string className = "MyWindow";

        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(
            className, 
            isPublic: true);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("public", code);
        Assert.Contains("class MyWindow", code);
    }

    [Fact]
    public void CreateClass_WithPartialModifier_CreatesPartialClass()
    {
        // Arrange
        string className = "MyWindow";

        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(
            className, 
            isPartial: true);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("partial", code);
        Assert.Contains("class MyWindow", code);
    }

    [Fact]
    public void CreateClass_WithPublicAndPartial_CreatesBoth()
    {
        // Arrange
        string className = "MyWindow";

        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(
            className, 
            isPublic: true, 
            isPartial: true);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("public", code);
        Assert.Contains("partial", code);
        Assert.Contains("class MyWindow", code);
    }

    [Fact]
    public void CreateClass_WithBaseType_CreatesClassWithInheritance()
    {
        // Arrange
        string className = "MyWindow";
        string baseType = "Toplevel";

        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(
            className, 
            baseTypeName: baseType);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("class MyWindow", code);
        Assert.Contains(": Toplevel", code);
    }

    [Fact]
    public void CreateClass_WithMembers_AddsMembers()
    {
        // Arrange
        string className = "MyWindow";
        var field = FieldDeclaration(
            VariableDeclaration(IdentifierName("string"))
                .WithVariables(SingletonSeparatedList(VariableDeclarator("myField"))));
        
        // Act
        ClassDeclarationSyntax result = ClassGenerationHelpers.CreateClass(
            className, 
            members: new[] { field });

        // Assert
        Assert.Single(result.Members);
    }

    [Fact]
    public void CreateClass_WithNullClassName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ClassGenerationHelpers.CreateClass(null!));
    }

    [Fact]
    public void CreateClass_WithEmptyClassName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            ClassGenerationHelpers.CreateClass(string.Empty));
    }

    #endregion

    #region CreateMethod Tests

    [Fact]
    public void CreateMethod_WithVoidReturnType_CreatesVoidMethod()
    {
        // Arrange
        string methodName = "InitializeComponent";

        // Act
        MethodDeclarationSyntax result = ClassGenerationHelpers.CreateMethod(
            methodName, 
            "void");

        // Assert
        string code = result.ToFullString();
        Assert.Contains("void InitializeComponent", code);
    }

    [Fact]
    public void CreateMethod_WithPrivateModifier_CreatesPrivateMethod()
    {
        // Arrange
        string methodName = "InitializeComponent";

        // Act
        MethodDeclarationSyntax result = ClassGenerationHelpers.CreateMethod(
            methodName, 
            "void", 
            isPrivate: true);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("private", code);
        Assert.Contains("void InitializeComponent", code);
    }

    [Fact]
    public void CreateMethod_WithStatements_AddsBody()
    {
        // Arrange
        string methodName = "InitializeComponent";
        var statement = ExpressionStatement(
            InvocationExpression(IdentifierName("DoSomething")));

        // Act
        MethodDeclarationSyntax result = ClassGenerationHelpers.CreateMethod(
            methodName, 
            "void", 
            statements: new[] { statement });

        // Assert
        Assert.NotNull(result.Body);
        Assert.Single(result.Body!.Statements);
    }

    [Fact]
    public void CreateMethod_WithMultipleStatements_AddsAllStatements()
    {
        // Arrange
        string methodName = "InitializeComponent";
        var statement1 = ExpressionStatement(InvocationExpression(IdentifierName("First")));
        var statement2 = ExpressionStatement(InvocationExpression(IdentifierName("Second")));

        // Act
        MethodDeclarationSyntax result = ClassGenerationHelpers.CreateMethod(
            methodName, 
            "void", 
            statements: new[] { statement1, statement2 });

        // Assert
        Assert.Equal(2, result.Body!.Statements.Count);
    }

    [Fact]
    public void CreateMethod_WithNullMethodName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ClassGenerationHelpers.CreateMethod(null!, "void"));
    }

    [Fact]
    public void CreateMethod_WithNullReturnType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ClassGenerationHelpers.CreateMethod("Method", null!));
    }

    #endregion

    #region CreatePrivateField Tests

    [Fact]
    public void CreatePrivateField_WithSimpleType_CreatesField()
    {
        // Arrange
        string fieldName = "myLabel";
        string typeName = "Label";

        // Act
        FieldDeclarationSyntax result = ClassGenerationHelpers.CreatePrivateField(
            fieldName, 
            typeName);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("private", code);
        Assert.Contains("Label", code);
        Assert.Contains("myLabel", code);
    }

    [Fact]
    public void CreatePrivateField_WithNullableType_CreatesNullableField()
    {
        // Arrange
        string fieldName = "myButton";
        string typeName = "Button";

        // Act
        FieldDeclarationSyntax result = ClassGenerationHelpers.CreatePrivateField(
            fieldName, 
            typeName, 
            isNullable: true);

        // Assert
        string code = result.ToFullString();
        Assert.Contains("private", code);
        Assert.Contains("Button?", code);
        Assert.Contains("myButton", code);
    }

    [Fact]
    public void CreatePrivateField_WithNullFieldName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ClassGenerationHelpers.CreatePrivateField(null!, "string"));
    }

    [Fact]
    public void CreatePrivateField_WithNullTypeName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            ClassGenerationHelpers.CreatePrivateField("field", null!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_CreateClass_MatchesWindowGeneratorPattern()
    {
        // This test verifies that ClassGenerationHelpers can reproduce the pattern from WindowGenerator
        // Arrange
        string className = "MyWindow";
        string fieldName = "label0";
        string typeName = "Label";

        // Act - Build class following WindowGenerator pattern
        var field = ClassGenerationHelpers.CreatePrivateField(fieldName, typeName, isNullable: true);
        
        var statement = ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(fieldName)),
                IdentifierName(fieldName)));
        
        var method = ClassGenerationHelpers.CreateMethod(
            "InitializeComponent", 
            "void", 
            isPrivate: true, 
            statements: new[] { statement });

        var classDecl = ClassGenerationHelpers.CreateClass(
            className, 
            isPublic: true, 
            isPartial: true, 
            baseTypeName: "Window",
            members: new MemberDeclarationSyntax[] { field, method });

        // Assert
        string code = classDecl.ToFullString();
        Assert.Contains("public partial class MyWindow : Window", code);
        Assert.Contains("private Label? label0", code);
        Assert.Contains("private void InitializeComponent", code);
    }

    [Fact]
    public void Integration_CreateClass_MatchesTopLevelGeneratorPattern()
    {
        // This test verifies that ClassGenerationHelpers can reproduce the pattern from TopLevelGenerator
        // Arrange
        string className = "MyApp";

        // Act - Build class following TopLevelGenerator pattern
        var method = ClassGenerationHelpers.CreateMethod(
            "InitializeComponent", 
            "void", 
            isPrivate: true,
            statements: Array.Empty<StatementSyntax>());

        var classDecl = ClassGenerationHelpers.CreateClass(
            className, 
            isPublic: true, 
            isPartial: true, 
            baseTypeName: "Toplevel",
            members: new MemberDeclarationSyntax[] { method });

        // Assert
        string code = classDecl.ToFullString();
        Assert.Contains("public partial class MyApp : Toplevel", code);
        Assert.Contains("private void InitializeComponent", code);
    }

    [Fact]
    public void Integration_CreateMultipleFields_CreatesAllFields()
    {
        // Arrange
        var field1 = ClassGenerationHelpers.CreatePrivateField("label0", "Label", isNullable: true);
        var field2 = ClassGenerationHelpers.CreatePrivateField("button1", "Button", isNullable: true);
        var field3 = ClassGenerationHelpers.CreatePrivateField("textField2", "TextField", isNullable: true);

        // Act
        var classDecl = ClassGenerationHelpers.CreateClass(
            "MyWindow",
            members: new[] { field1, field2, field3 });

        // Assert
        Assert.Equal(3, classDecl.Members.Count);
        string code = classDecl.ToFullString();
        Assert.Contains("Label? label0", code);
        Assert.Contains("Button? button1", code);
        Assert.Contains("TextField? textField2", code);
    }

    #endregion
}
