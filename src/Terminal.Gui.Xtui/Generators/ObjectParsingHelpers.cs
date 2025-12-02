using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generators;

/// <summary>
/// Helper methods for parsing XTUI attribute values and generating appropriate C# expressions.
/// </summary>
internal static class ObjectParsingHelpers
{
    /// <summary>
    /// Dictionary mapping property names to their C# types.
    /// Used for type-safe code generation from XTUI attributes.
    /// </summary>
    private static readonly Dictionary<string, string> PropertyTypes = new (StringComparer.OrdinalIgnoreCase)
    {
        // String properties
        { "Text", "string" },      // View, Window, Label, Button
        { "Title", "string" },     // View, Window
        { "Id", "string" },        // View, Window, Label, Button
        
        // Boolean properties
        { "Visible", "bool" },                      // View, Window, Label, Button
        { "Enabled", "bool" },                      // View, Window, Label, Button
        { "CanFocus", "bool" },                     // View, Window, Label, Button
        { "HasFocus", "bool" },                     // View, Window, Label, Button
        { "IsInitialized", "bool" },                // View, Window, Label, Button
        { "PreserveTrailingSpaces", "bool" },       // View, Window, Label, Button
        { "SuperViewRendersLineCanvas", "bool" },   // View, Window, Label, Button
        { "ContentSizeTracksViewport", "bool" },    // View, Window, Label, Button
        { "WantContinuousButtonPressed", "bool" },  // View, Window, Label, Button
        { "WantMousePositionReports", "bool" },     // View, Window, Label, Button
        { "ValidatePosDim", "bool" },               // View, Window, Label, Button
        
        // Position properties (Pos type)
        { "X", "Pos" },            // View, Window, Label, Button
        { "Y", "Pos" },            // View, Window, Label, Button
        
        // Dimension properties (Dim type)
        { "Width", "Dim" },        // View, Window, Label, Button
        { "Height", "Dim" },       // View, Window, Label, Button
        
        // CheckBox-specific properties
        { "CheckedState", "CheckState" },
        { "AllowCheckStateNone", "bool" },
        { "RadioStyle", "bool" },
        
        // TextField-specific properties
        { "Secret", "bool" },
        
        // Button-specific properties
        { "IsDefault", "bool" },
    };

    /// <summary>
    /// Parses a value based on its known expected type.
    /// Property name must be provided and must exist in the PropertyTypes dictionary.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be parsed to the expected type, property name is missing, or property is not in the dictionary.</exception>
    public static ExpressionSyntax ParseValueWithType (string value, string propertyName)
    {
        if (string.IsNullOrEmpty (value))
        {
            return LiteralExpression (SyntaxKind.StringLiteralExpression, Literal (string.Empty));
        }

        // Property name must be provided and must exist in the dictionary
        if (string.IsNullOrWhiteSpace (propertyName))
        {
            throw new InvalidOperationException (
                $"Property name is required for parsing attribute value '{value}'. " +
                $"Cannot generate code without knowing the property type.");
        }

        if (!PropertyTypes.TryGetValue (propertyName, out string? expectedType))
        {
            throw new InvalidOperationException (
                $"Unknown property '{propertyName}' with value '{value}'. " +
                $"This property is not defined in the type dictionary and cannot be used for code generation. " +
                $"Add the property to the PropertyTypes dictionary with its correct type.");
        }

        switch (expectedType)
        {
            case "bool":
                if (bool.TryParse (value, out bool boolValue))
                {
                    return LiteralExpression (
                        boolValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                }
                throw new InvalidOperationException (
                    $"Cannot parse value '{value}' as bool for property '{propertyName}'. " +
                    $"Expected 'true' or 'false' (case-insensitive).");

            case "int":
                if (int.TryParse (value, out int intValue))
                {
                    return LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (intValue));
                }
                throw new InvalidOperationException (
                    $"Cannot parse value '{value}' as int for property '{propertyName}'. " +
                    $"Expected a valid integer value.");

            case "double":
                if (double.TryParse (value, out double doubleValue))
                {
                    return LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (doubleValue));
                }
                throw new InvalidOperationException (
                    $"Cannot parse value '{value}' as double for property '{propertyName}'. " +
                    $"Expected a valid numeric value.");

            case "string":
                return LiteralExpression (SyntaxKind.StringLiteralExpression, Literal (value));

            case "Pos":
                return ParsePosExpression (value, propertyName);

            case "Dim":
                return ParseDimExpression (value, propertyName);

            case "CheckState":
            case "TextAlignment":
            case "BorderStyle":
                // Use EnumMapper to resolve enum values
                string enumExpression = Terminal.Gui.Xtui.Mappers.EnumMapper.GetEnumValue (expectedType, value);
                return ParseExpression (enumExpression);

            default:
                throw new InvalidOperationException (
                    $"Unknown type '{expectedType}' for property '{propertyName}'. " +
                    $"This type is not supported for code generation.");
        }
    }

    /// <summary>
    /// Parses a Pos expression from XTUI attribute value.
    /// Supports: integers (implicit conversion), percentage notation (50%), expression syntax ({Center}, {AnchorEnd 10})
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the expression cannot be parsed.</exception>
    private static ExpressionSyntax ParsePosExpression (string value, string propertyName)
    {
        value = value.Trim ();

        // Try parsing as integer (implicit conversion to Pos.Absolute)
        if (int.TryParse (value, out int intValue))
        {
            return LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (intValue));
        }

        // Try parsing as percentage notation: 50%
        if (value.EndsWith ("%") && int.TryParse (value.Substring (0, value.Length - 1), out int percentValue))
        {
            return InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Pos"),
                    IdentifierName ("Percent")))
                .WithArgumentList (
                    ArgumentList (
                        SingletonSeparatedList (
                            Argument (
                                LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (percentValue))))));
        }

        // Try parsing as expression syntax with view reference and operators: {MethodName viewRef +|- offset}
        // Supports: {Right _usernameLabel + 1}, {Right _usernameLabel +1}, {Right _usernameLabel- 1}
        Match viewOperatorMatch = Regex.Match (value, @"^\{\s*(\w+)\s+([_a-zA-Z][_a-zA-Z0-9]*)\s*([+\-])\s*(\d+)\s*\}$");
        if (viewOperatorMatch.Success)
        {
            string methodName = viewOperatorMatch.Groups [1].Value;
            string viewRef = viewOperatorMatch.Groups [2].Value;
            string op = viewOperatorMatch.Groups [3].Value;
            int offset = int.Parse (viewOperatorMatch.Groups [4].Value);

            // Validate method name is a valid Pos factory method
            if (!IsValidPosMethod (methodName))
            {
                throw new InvalidOperationException (
                    $"Invalid Pos method '{methodName}' for property '{propertyName}'. " +
                    $"Valid methods: Absolute, Percent, Center, AnchorEnd, Left, Right, Top, Bottom, X, Y, Func, Align.");
            }

            // Build: Pos.{methodName}(viewRef) +/- offset
            InvocationExpressionSyntax invocation = InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Pos"),
                    IdentifierName (methodName)))
                .WithArgumentList (
                    ArgumentList (
                        SingletonSeparatedList (
                            Argument (IdentifierName (viewRef)))));

            BinaryExpressionSyntax binaryExpression = BinaryExpression (
                op == "+" ? SyntaxKind.AddExpression : SyntaxKind.SubtractExpression,
                invocation,
                LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (offset)));

            return binaryExpression;
        }

        // Try parsing as expression syntax with operators: {MethodName +|- offset}
        // Supports: {Center + 1}, {Center +1}, {Center- 1}, {AnchorEnd - 5}
        Match operatorMatch = Regex.Match (value, @"^\{\s*(\w+)\s*([+\-])\s*(\d+)\s*\}$");
        if (operatorMatch.Success)
        {
            string methodName = operatorMatch.Groups [1].Value;
            string op = operatorMatch.Groups [2].Value;
            int offset = int.Parse (operatorMatch.Groups [3].Value);

            // Validate method name is a valid Pos factory method
            if (!IsValidPosMethod (methodName))
            {
                throw new InvalidOperationException (
                    $"Invalid Pos method '{methodName}' for property '{propertyName}'. " +
                    $"Valid methods: Absolute, Percent, Center, AnchorEnd, Left, Right, Top, Bottom, X, Y, Func, Align.");
            }

            // Build: Pos.{methodName}() +/- offset
            InvocationExpressionSyntax invocation = InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Pos"),
                    IdentifierName (methodName)))
                .WithArgumentList (ArgumentList ());

            BinaryExpressionSyntax binaryExpression = BinaryExpression (
                op == "+" ? SyntaxKind.AddExpression : SyntaxKind.SubtractExpression,
                invocation,
                LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (offset)));

            return binaryExpression;
        }

        // Try parsing as expression syntax: {MethodName} or {MethodName arg} or {MethodName "arg"}
        Match expressionMatch = Regex.Match (value, @"^\{\s*(\w+)(?:\s+(.+?))?\s*\}$");
        if (expressionMatch.Success)
        {
            string methodName = expressionMatch.Groups [1].Value;
            string args = expressionMatch.Groups [2].Success ? expressionMatch.Groups [2].Value.Trim () : string.Empty;

            // Validate method name is a valid Pos factory method
            if (!IsValidPosMethod (methodName))
            {
                throw new InvalidOperationException (
                    $"Invalid Pos method '{methodName}' for property '{propertyName}'. " +
                    $"Valid methods: Absolute, Percent, Center, AnchorEnd, Left, Right, Top, Bottom, X, Y, Func, Align.");
            }

            // Build invocation: Pos.{methodName}({args})
            InvocationExpressionSyntax invocation = InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Pos"),
                    IdentifierName (methodName)));

            // Parse arguments if any
            if (!string.IsNullOrEmpty (args))
            {
                ArgumentListSyntax argumentList = ParsePosMethodArguments (args, methodName);
                invocation = invocation.WithArgumentList (argumentList);
            }
            else
            {
                invocation = invocation.WithArgumentList (ArgumentList ());
            }

            return invocation;
        }

        throw new InvalidOperationException (
            $"Cannot parse Pos value '{value}' for property '{propertyName}'. " +
            $"Expected an integer, percentage (e.g., '50%'), or expression syntax (e.g., '{{Center}}', '{{Percent 50}}', '{{AnchorEnd 10}}').");
    }

    /// <summary>
    /// Parses a Dim expression from XTUI attribute value.
    /// Supports: integers (implicit conversion), percentage notation (50%), expression syntax ({Auto}, {Fill 10})
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the expression cannot be parsed.</exception>
    private static ExpressionSyntax ParseDimExpression (string value, string propertyName)
    {
        value = value.Trim ();

        // Try parsing as integer (implicit conversion to Dim.Absolute)
        if (int.TryParse (value, out int intValue))
        {
            return LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (intValue));
        }

        // Try parsing as percentage notation: 50%
        if (value.EndsWith ("%") && int.TryParse (value.Substring (0, value.Length - 1), out int percentValue))
        {
            return InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Dim"),
                    IdentifierName ("Percent")))
                .WithArgumentList (
                    ArgumentList (
                        SingletonSeparatedList (
                            Argument (
                                LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (percentValue))))));
        }

        // Try parsing as expression syntax with operators: {MethodName +/- offset}
        Match operatorMatch = Regex.Match (value, @"^\{\s*(\w+)\s*([+\-])\s*(\d+)\s*\}$");
        if (operatorMatch.Success)
        {
            string methodName = operatorMatch.Groups [1].Value;
            string op = operatorMatch.Groups [2].Value;
            int offset = int.Parse (operatorMatch.Groups [3].Value);

            // Validate method name is a valid Dim factory method
            if (!IsValidDimMethod (methodName))
            {
                throw new InvalidOperationException (
                    $"Invalid Dim method '{methodName}' for property '{propertyName}'. " +
                    $"Valid methods: Absolute, Percent, Fill, Auto, Width, Height, Func.");
            }

            // Build: Dim.{methodName}() +/- offset
            InvocationExpressionSyntax invocation = InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Dim"),
                    IdentifierName (methodName)))
                .WithArgumentList (ArgumentList ());

            BinaryExpressionSyntax binaryExpression = BinaryExpression (
                op == "+" ? SyntaxKind.AddExpression : SyntaxKind.SubtractExpression,
                invocation,
                LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (offset)));

            return binaryExpression;
        }

        // Try parsing as expression syntax: {MethodName} or {MethodName arg} or {MethodName "arg"}
        Match expressionMatch = Regex.Match (value, @"^\{\s*(\w+)(?:\s+(.+?))?\s*\}$");
        if (expressionMatch.Success)
        {
            string methodName = expressionMatch.Groups [1].Value;
            string args = expressionMatch.Groups [2].Success ? expressionMatch.Groups [2].Value.Trim () : string.Empty;

            // Validate method name is a valid Dim factory method
            if (!IsValidDimMethod (methodName))
            {
                throw new InvalidOperationException (
                    $"Invalid Dim method '{methodName}' for property '{propertyName}'. " +
                    $"Valid methods: Absolute, Percent, Fill, Auto, Width, Height, Func.");
            }

            // Build invocation: Dim.{methodName}({args})
            InvocationExpressionSyntax invocation = InvocationExpression (
                MemberAccessExpression (
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName ("Dim"),
                    IdentifierName (methodName)));

            // Parse arguments if any
            if (!string.IsNullOrEmpty (args))
            {
                ArgumentListSyntax argumentList = ParseDimMethodArguments (args, methodName);
                invocation = invocation.WithArgumentList (argumentList);
            }
            else
            {
                invocation = invocation.WithArgumentList (ArgumentList ());
            }

            return invocation;
        }

        throw new InvalidOperationException (
            $"Cannot parse Dim value '{value}' for property '{propertyName}'. " +
            $"Expected an integer, percentage (e.g., '50%'), or expression syntax (e.g., '{{Auto}}', '{{Fill 10}}').");
    }

    /// <summary>
    /// Validates if a method name is a valid Pos factory method.
    /// </summary>
    private static bool IsValidPosMethod (string methodName)
    {
        return methodName switch
        {
            "Absolute" or "Percent" or "Center" or "AnchorEnd" or
            "Left" or "Right" or "Top" or "Bottom" or
            "X" or "Y" or "Func" or "Align" => true,
            _ => false
        };
    }

    /// <summary>
    /// Validates if a method name is a valid Dim factory method.
    /// </summary>
    private static bool IsValidDimMethod (string methodName)
    {
        return methodName switch
        {
            "Absolute" or "Percent" or "Fill" or "Auto" or
            "Width" or "Height" or "Func" => true,
            _ => false
        };
    }

    /// <summary>
    /// Parses arguments for Pos method calls.
    /// Handles integer arguments, quoted string arguments, and view identifier references.
    /// </summary>
    private static ArgumentListSyntax ParsePosMethodArguments (string args, string methodName)
    {
        // Try parsing as quoted string: "value"
        Match quotedMatch = Regex.Match (args, @"^""(.*)""$");
        if (quotedMatch.Success)
        {
            string stringValue = quotedMatch.Groups [1].Value;
            return ArgumentList (
                SingletonSeparatedList (
                    Argument (
                        LiteralExpression (SyntaxKind.StringLiteralExpression, Literal (stringValue)))));
        }

        // For simple cases, try parsing as integer
        if (int.TryParse (args, out int intArg))
        {
            return ArgumentList (
                SingletonSeparatedList (
                    Argument (
                        LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (intArg)))));
        }

        // Try parsing as identifier (view reference): myView, _myView
        if (Regex.IsMatch (args, @"^[_a-zA-Z][_a-zA-Z0-9]*$"))
        {
            return ArgumentList (
                SingletonSeparatedList (
                    Argument (IdentifierName (args))));
        }

        throw new InvalidOperationException (
            $"Cannot parse argument '{args}' for Pos.{methodName}(). " +
            $"Supported formats: integer, quoted string, or identifier (view reference).");
    }

    /// <summary>
    /// Parses arguments for Dim method calls.
    /// Handles integer arguments and quoted string arguments.
    /// </summary>
    private static ArgumentListSyntax ParseDimMethodArguments (string args, string methodName)
    {
        // Try parsing as quoted string: "value"
        Match quotedMatch = Regex.Match (args, @"^""(.*)""$");
        if (quotedMatch.Success)
        {
            string stringValue = quotedMatch.Groups [1].Value;
            return ArgumentList (
                SingletonSeparatedList (
                    Argument (
                        LiteralExpression (SyntaxKind.StringLiteralExpression, Literal (stringValue)))));
        }

        // For simple cases, try parsing as integer
        if (int.TryParse (args, out int intArg))
        {
            return ArgumentList (
                SingletonSeparatedList (
                    Argument (
                        LiteralExpression (SyntaxKind.NumericLiteralExpression, Literal (intArg)))));
        }

        // TODO: Handle more complex cases like view references: Width(myView), Height(myView)

        throw new InvalidOperationException (
            $"Cannot parse argument '{args}' for Dim.{methodName}(). " +
            $"Currently only integer and quoted string arguments are supported.");
    }
}
