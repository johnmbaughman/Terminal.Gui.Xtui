using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terminal.Gui.Xtui.Helpers;

namespace Terminal.Gui.Xtui.Benchmarks;

/// <summary>
/// Benchmarks for Pos/Dim expression parsing performance.
/// Tests parsing of various expression types (literals, percentages, named methods, operators).
/// </summary>
[MemoryDiagnoser]
[SimpleJob (RuntimeMoniker.Net80)]
public class ExpressionParsingBenchmarks
{
    private const string PropertyNameX = "X";
    private const string PropertyNameWidth = "Width";

    [Benchmark (Baseline = true, Description = "Literal Integer")]
    public ExpressionSyntax ParseLiteralInteger ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("10", PropertyNameX);
    }

    [Benchmark (Description = "Percentage")]
    public ExpressionSyntax ParsePercentage ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("50%", PropertyNameX);
    }

    [Benchmark (Description = "Named Method - Center")]
    public ExpressionSyntax ParseNamedMethodCenter ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Center}", PropertyNameX);
    }

    [Benchmark (Description = "Named Method - AnchorEnd")]
    public ExpressionSyntax ParseNamedMethodAnchorEnd ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{AnchorEnd}", PropertyNameX);
    }

    [Benchmark (Description = "Method with Argument")]
    public ExpressionSyntax ParseMethodWithArgument ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{AnchorEnd 5}", PropertyNameX);
    }

    [Benchmark (Description = "Operator Expression - Addition")]
    public ExpressionSyntax ParseOperatorAddition ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Center + 10}", PropertyNameX);
    }

    [Benchmark (Description = "Operator Expression - Subtraction")]
    public ExpressionSyntax ParseOperatorSubtraction ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Center - 10}", PropertyNameX);
    }

    [Benchmark (Description = "Dim Fill")]
    public ExpressionSyntax ParseDimFill ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Fill}", PropertyNameWidth);
    }

    [Benchmark (Description = "Dim Auto")]
    public ExpressionSyntax ParseDimAuto ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Auto}", PropertyNameWidth);
    }

    [Benchmark (Description = "Dim Fill with Operator")]
    public ExpressionSyntax ParseDimFillOperator ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("{Fill - 5}", PropertyNameWidth);
    }

    [Benchmark (Description = "String Literal")]
    public ExpressionSyntax ParseStringLiteral ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("Hello World", "Text");
    }

    [Benchmark (Description = "Boolean True")]
    public ExpressionSyntax ParseBooleanTrue ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("true", "Visible");
    }

    [Benchmark (Description = "Boolean False")]
    public ExpressionSyntax ParseBooleanFalse ()
    {
        return ObjectParsingHelpers.ParseValueWithType ("false", "Visible");
    }
}

