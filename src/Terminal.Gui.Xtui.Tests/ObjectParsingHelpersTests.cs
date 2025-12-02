using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests;

public class ObjectParsingHelpersTests
{
    [Theory]
    [InlineData("true", "Enabled", "true")]
    [InlineData("false", "Visible", "false")]
    [InlineData("True", "CanFocus", "true")]
    [InlineData("FALSE", "HasFocus", "false")]
    public void ParseValueWithType_ParsesBooleanValues(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var literal = Assert.IsType<LiteralExpressionSyntax>(result);
        Assert.Contains(expected, literal.ToString());
    }

    [Theory]
    [InlineData("Hello World", "Text", "\"Hello World\"")]
    [InlineData("Button Text", "Title", "\"Button Text\"")]
    public void ParseValueWithType_ParsesStringValues(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var literal = Assert.IsType<LiteralExpressionSyntax>(result);
        Assert.Equal(expected, literal.ToString());
    }

    [Theory]
    [InlineData("5", "X", "5")]
    [InlineData("10", "Y", "10")]
    [InlineData("0", "X", "0")]
    public void ParseValueWithType_ParsesIntegerPosValues(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var literal = Assert.IsType<LiteralExpressionSyntax>(result);
        Assert.Equal(expected, literal.ToString());
    }

    [Theory]
    [InlineData("50%", "X", "Pos.Percent(50)")]
    [InlineData("75%", "Y", "Pos.Percent(75)")]
    public void ParseValueWithType_ParsesPercentagePosValues(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var invocation = Assert.IsType<InvocationExpressionSyntax>(result);
        Assert.Equal(expected, invocation.ToString());
    }

    [Theory]
    [InlineData("{Center}", "X", "Pos.Center()")]
    [InlineData("{AnchorEnd}", "Y", "Pos.AnchorEnd()")]
    public void ParseValueWithType_ParsesSimplePosExpressions(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var invocation = Assert.IsType<InvocationExpressionSyntax>(result);
        Assert.Equal(expected, invocation.ToString());
    }

    [Theory]
    [InlineData("{Center + 5}", "X", "Pos.Center()+5")]
    [InlineData("{Center - 10}", "Y", "Pos.Center()-10")]
    [InlineData("{AnchorEnd - 5}", "X", "Pos.AnchorEnd()-5")]
    [InlineData("{Center +5}", "X", "Pos.Center()+5")]
    [InlineData("{Center- 5}", "X", "Pos.Center()-5")]
    public void ParseValueWithType_ParsesPosExpressionsWithOperators(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var binary = Assert.IsType<BinaryExpressionSyntax>(result);
        Assert.Equal(expected, binary.ToString());
    }

    [Theory]
    [InlineData("{Right _usernameLabel + 1}", "X", "Pos.Right(_usernameLabel)+1")]
    [InlineData("{Right _passwordLabel + 1}", "X", "Pos.Right(_passwordLabel)+1")]
    [InlineData("{Left myView - 5}", "X", "Pos.Left(myView)-5")]
    [InlineData("{Right _usernameLabel +1}", "X", "Pos.Right(_usernameLabel)+1")]
    [InlineData("{Right _usernameLabel- 1}", "X", "Pos.Right(_usernameLabel)-1")]
    public void ParseValueWithType_ParsesPosExpressionsWithViewReferences(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var binary = Assert.IsType<BinaryExpressionSyntax>(result);
        Assert.Equal(expected, binary.ToString());
    }

    [Theory]
    [InlineData("{Right _usernameLabel}", "X", "Pos.Right(_usernameLabel)")]
    [InlineData("{Left myView}", "X", "Pos.Left(myView)")]
    public void ParseValueWithType_ParsesPosExpressionsWithViewReferencesNoOperator(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var invocation = Assert.IsType<InvocationExpressionSyntax>(result);
        Assert.Equal(expected, invocation.ToString());
    }

    [Theory]
    [InlineData("{Fill}", "Width", "Dim.Fill()")]
    [InlineData("{Auto}", "Height", "Dim.Auto()")]
    public void ParseValueWithType_ParsesSimpleDimExpressions(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var invocation = Assert.IsType<InvocationExpressionSyntax>(result);
        Assert.Equal(expected, invocation.ToString());
    }

    [Theory]
    [InlineData("{Fill - 5}", "Width", "Dim.Fill()-5")]
    [InlineData("{Auto + 10}", "Height", "Dim.Auto()+10")]
    [InlineData("{Fill -5}", "Width", "Dim.Fill()-5")]
    [InlineData("{Auto +10}", "Height", "Dim.Auto()+10")]
    public void ParseValueWithType_ParsesDimExpressionsWithOperators(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var binary = Assert.IsType<BinaryExpressionSyntax>(result);
        Assert.Equal(expected, binary.ToString());
    }

    [Theory]
    [InlineData("50%", "Width", "Dim.Percent(50)")]
    [InlineData("80%", "Height", "Dim.Percent(80)")]
    public void ParseValueWithType_ParsesPercentageDimValues(string value, string propertyName, string expected)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var invocation = Assert.IsType<InvocationExpressionSyntax>(result);
        Assert.Equal(expected, invocation.ToString());
    }

    [Theory]
    [InlineData("Checked", "CheckedState")]
    [InlineData("UnChecked", "CheckedState")]
    [InlineData("None", "CheckedState")]
    public void ParseValueWithType_ParsesEnumValues(string value, string propertyName)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        Assert.NotNull(result);
        Assert.Contains("Terminal.Gui.Views.CheckState", result.ToString());
    }

    [Fact]
    public void ParseValueWithType_ThrowsForUnknownProperty()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ObjectParsingHelpers.ParseValueWithType("value", "UnknownProperty"));
        
        Assert.Contains("Unknown property 'UnknownProperty'", ex.Message);
    }

    [Fact]
    public void ParseValueWithType_ThrowsForInvalidBoolValue()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ObjectParsingHelpers.ParseValueWithType("notabool", "Enabled"));
        
        Assert.Contains("Cannot parse value 'notabool' as bool", ex.Message);
    }

    [Fact]
    public void ParseValueWithType_ThrowsForInvalidPosMethod()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ObjectParsingHelpers.ParseValueWithType("{InvalidMethod}", "X"));
        
        Assert.Contains("Invalid Pos method 'InvalidMethod'", ex.Message);
    }

    [Fact]
    public void ParseValueWithType_ThrowsForInvalidDimMethod()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ObjectParsingHelpers.ParseValueWithType("{InvalidMethod}", "Width"));
        
        Assert.Contains("Invalid Dim method 'InvalidMethod'", ex.Message);
    }

    [Theory]
    [InlineData("", "Text")]
    public void ParseValueWithType_HandlesEmptyStrings(string value, string propertyName)
    {
        var result = ObjectParsingHelpers.ParseValueWithType(value, propertyName);
        
        var literal = Assert.IsType<LiteralExpressionSyntax>(result);
        Assert.Equal("\"\"", literal.ToString());
    }

    [Fact]
    public void ParseValueWithType_HandlesNullString()
    {
        var result = ObjectParsingHelpers.ParseValueWithType(null!, "Text");
        
        var literal = Assert.IsType<LiteralExpressionSyntax>(result);
        Assert.Equal("\"\"", literal.ToString());
    }

    [Fact]
    public void ParseValueWithType_ThrowsForMissingPropertyName()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            ObjectParsingHelpers.ParseValueWithType("value", ""));
        
        Assert.Contains("Property name is required", ex.Message);
    }
}
