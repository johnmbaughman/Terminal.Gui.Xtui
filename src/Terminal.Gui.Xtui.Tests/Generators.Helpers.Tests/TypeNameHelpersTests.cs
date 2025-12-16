using Xunit;

namespace Terminal.Gui.Xtui.Tests.Generators.Helpers.Tests;

/// <summary>
/// Tests for TypeNameHelpers utility methods.
/// These tests verify the extraction and manipulation of type names for code generation.
/// 
/// Pattern Analysis from existing generators:
/// - Fully qualified type names (e.g., "Terminal.Gui.Label") need to be extracted to simple names (e.g., "Label")
/// - Simple type names (e.g., "Button") should remain unchanged
/// - Used for: variable naming, object creation syntax, namespace-aware code generation
/// </summary>
public class TypeNameHelpersTests
{
    [Theory]
    [InlineData("Label", "Label")]
    [InlineData("Button", "Button")]
    [InlineData("TextField", "TextField")]
    [InlineData("Window", "Window")]
    public void ExtractLocalTypeName_WithSimpleTypeName_ReturnsUnchanged(string fullTypeName, string expected)
    {
        // Arrange & Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ExtractLocalTypeName(fullTypeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Terminal.Gui.Label", "Label")]
    [InlineData("Terminal.Gui.Button", "Button")]
    [InlineData("Terminal.Gui.TextField", "TextField")]
    [InlineData("System.Windows.Forms.Label", "Label")]
    [InlineData("MyApp.Controls.CustomButton", "CustomButton")]
    public void ExtractLocalTypeName_WithFullyQualifiedTypeName_ReturnsLastSegment(string fullTypeName, string expected)
    {
        // Arrange & Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ExtractLocalTypeName(fullTypeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Terminal.Gui.Views.Label", "Label")]
    [InlineData("A.B.C.D.E.FinalType", "FinalType")]
    public void ExtractLocalTypeName_WithMultipleDots_ReturnsLastSegment(string fullTypeName, string expected)
    {
        // Arrange & Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ExtractLocalTypeName(fullTypeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractLocalTypeName_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        string fullTypeName = string.Empty;

        // Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ExtractLocalTypeName(fullTypeName);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractLocalTypeName_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? fullTypeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ExtractLocalTypeName(fullTypeName!));
    }

    [Theory]
    [InlineData("Label", "label")]
    [InlineData("Button", "button")]
    [InlineData("TextField", "textField")]
    [InlineData("CustomControl", "customControl")]
    [InlineData("MyView", "myView")]
    [InlineData("A", "a")]
    public void ToVariableName_WithTypeName_ReturnsCamelCase(string typeName, string expected)
    {
        // Arrange & Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ToVariableName(typeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToVariableName_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        string typeName = string.Empty;

        // Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ToVariableName(typeName);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToVariableName_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? typeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.ToVariableName(typeName!));
    }

    [Theory]
    [InlineData("Terminal.Gui.Label", 0, "label0")]
    [InlineData("Terminal.Gui.Button", 1, "button1")]
    [InlineData("TextField", 2, "textField2")]
    [InlineData("Window", 10, "window10")]
    public void CreateIndexedVariableName_WithTypeNameAndIndex_ReturnsCamelCaseWithIndex(string fullTypeName, int index, string expected)
    {
        // Arrange & Act
        string result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.CreateIndexedVariableName(fullTypeName, index);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CreateIndexedVariableName_WithNegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        string fullTypeName = "Label";
        int index = -1;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.CreateIndexedVariableName(fullTypeName, index));
    }

    [Theory]
    [InlineData("Label", true)]
    [InlineData("Button", true)]
    [InlineData("TextField", true)]
    [InlineData("", false)]
    public void IsSimpleTypeName_WithVariousInputs_ReturnsExpectedResult(string typeName, bool expected)
    {
        // Arrange & Act
        bool result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.IsSimpleTypeName(typeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Terminal.Gui.Label", false)]
    [InlineData("System.String", false)]
    public void IsSimpleTypeName_WithQualifiedNames_ReturnsFalse(string typeName, bool expected)
    {
        // Arrange & Act
        bool result = Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.IsSimpleTypeName(typeName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsSimpleTypeName_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? typeName = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            Terminal.Gui.Xtui.Generators.Helpers.TypeNameHelpers.IsSimpleTypeName(typeName!));
    }
}
