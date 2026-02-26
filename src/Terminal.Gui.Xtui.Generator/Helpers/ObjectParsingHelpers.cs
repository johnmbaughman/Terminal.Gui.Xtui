using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

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
        { "HighlightStates", "MouseState" },

        // TextField-specific properties
        { "Secret", "bool" },

        // Button-specific properties
        { "IsDefault", "bool" },

        // MenuItem/MenuBarItem/Shortcut properties
        { "HelpText", "string" },
        { "Key", "Key" },
        { "Command", "Command" },
        { "BindKeyToApplication", "bool" },

        // StatusBar/MenuBar/Bar properties
        { "AlignmentModes", "AlignmentModes" },
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

        // Check for binding directive: {Binding PropertyName}
        if (value.StartsWith ("{Binding ", StringComparison.OrdinalIgnoreCase) && value.EndsWith ("}"))
        {
            // Extract the property path from {Binding PropertyName}
            string bindingPath = value.Substring (9, value.Length - 10).Trim ();
            // Generate code like: this.PropertyName or whatever the binding path is
            return ParseExpression (bindingPath);
        }

        // Check for other directive syntax: {expression}
        // For Pos and Dim properties, let the specialized parsers handle ALL {...} expressions
        // For other properties, check if it's a known Pos/Dim method (which would indicate the property type is wrong)
        if (value.StartsWith ("{") && value.EndsWith ("}"))
        {
            string trimmed = value.Substring (1, value.Length - 2).Trim ();

            // If the property type is Pos or Dim, always defer to the specialized parser
            // (It will validate the method name and throw appropriate exceptions)
            if (!string.IsNullOrWhiteSpace (propertyName) &&
                PropertyTypes.TryGetValue (propertyName, out string? propertyType) &&
                (propertyType == "Pos" || propertyType == "Dim"))
            {
                // Fall through to the property type handling below
            }
            // For non-Pos/Dim properties, check if this looks like a Pos/Dim expression
            // (This helps catch cases where the wrong property type is being used)
            else if (trimmed.StartsWith ("Dim ", StringComparison.OrdinalIgnoreCase) ||
                     trimmed.StartsWith ("Pos ", StringComparison.OrdinalIgnoreCase))
            {
                // Explicit Dim/Pos prefix - fall through to property type handling
            }
            else
            {
                // Not a Pos/Dim property and not an explicit Pos/Dim expression
                // Extract method name to check if it's accidentally using Pos/Dim syntax
                string methodName = trimmed.Split ([' ', '=', '{', '+', '-'], StringSplitOptions.RemoveEmptyEntries)[0].Trim ();
                if (IsValidPosMethod (methodName) || IsValidDimMethod (methodName))
                {
                    // Looks like a Pos/Dim method but property isn't Pos/Dim type
                    // Fall through to property type handling which will likely fail with a better error
                }
                else
                {
                    // Generic expression - not related to Pos/Dim
                    return ParseExpression (trimmed);
                }
            }
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
            case "AlignmentModes":
            case "MouseState":
                // Use EnumMapper to resolve enum values
                string enumExpression = Mappers.EnumMapper.GetEnumValue (expectedType, value);
                return ParseExpression (enumExpression);

            case "Command":
                // Command is an enum; map to fully-qualified member expression (e.g. Terminal.Gui.Input.Command.Quit)
                string cmdExpression = Mappers.EnumMapper.GetEnumValue ("Command", value);
                return ParseExpression (cmdExpression);

            case "Key":
                // Support multiple formats for Key:
                // 1. Special symbolic names: "QuitKey" -> Application.QuitKey
                // 2. Simple enum names: "F10", "A", "Enter" -> Key.F10, Key.A, Key.Enter
                // 3. Fully-qualified: "Key.F1", "Application.QuitKey" -> use as-is
                // 4. Method calls: "Key.A.WithCtrl" -> use as-is
                // 5. String representations: "Ctrl+Q" -> new Key("Ctrl+Q")
                string trimmedKey = value.Trim ();

                // Check for fully-qualified or method-style expressions
                bool looksLikeExpression = trimmedKey.Contains (".") || trimmedKey.Contains ("With");
                if (looksLikeExpression)
                {
                    return ParseExpression (trimmedKey);
                }

                // Check for special Application keys
                if (trimmedKey == "QuitKey")
                {
                    return ParseExpression ("Application.QuitKey");
                }

                // Check if it looks like a simple Key enum member name (alphanumeric, possibly with underscore)
                // Common examples: F1-F12, A-Z, Enter, Escape, Tab, Space, etc.
                bool looksLikeEnumMember = Regex.IsMatch (
                    trimmedKey,
                    "^[A-Z][a-zA-Z0-9_]*$");

                if (looksLikeEnumMember)
                {
                    // Generate: Key.{value}
                    return MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName ("Key"),
                        IdentifierName (trimmedKey));
                }

                // Fall back to string representation: new Key("value")
                return ObjectCreationExpression (
                    IdentifierName ("Key"))
                    .WithArgumentList (
                        ArgumentList (
                            SingletonSeparatedList (
                                Argument (
                                    LiteralExpression (
                                        SyntaxKind.StringLiteralExpression,
                                        Literal (trimmedKey))))));

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
            string methodName = viewOperatorMatch.Groups[1].Value;
            string viewRef = viewOperatorMatch.Groups[2].Value;
            string op = viewOperatorMatch.Groups[3].Value;
            int offset = int.Parse (viewOperatorMatch.Groups[4].Value);

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
            string methodName = operatorMatch.Groups[1].Value;
            string op = operatorMatch.Groups[2].Value;
            int offset = int.Parse (operatorMatch.Groups[3].Value);

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
            string methodName = expressionMatch.Groups[1].Value;
            string args = expressionMatch.Groups[2].Success ? expressionMatch.Groups[2].Value.Trim () : string.Empty;

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
            string methodName = operatorMatch.Groups[1].Value;
            string op = operatorMatch.Groups[2].Value;
            int offset = int.Parse (operatorMatch.Groups[3].Value);

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

        // Note: simple {MethodName ...} parsing handled after complex `{Dim ...}` branch

        // Support complex Dim expressions with key/value parameters using
        // the form: {Dim Auto={style=Auto;minimumContentDim={Dim Func={_ => ...}};maximumContentDim=...}}
        // This allows specifying named parameters for Dim.Auto in XTUI.
        Match complexDimMatch = Regex.Match (value, @"^\{\s*Dim\s+(\w+)\s*=\s*(\{.*\}|.+)\s*\}$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (complexDimMatch.Success)
        {
            string methodName = complexDimMatch.Groups[1].Value;
            string argsText = complexDimMatch.Groups[2].Value.Trim ();

            // Handle Dim.Func specially when it is embedded: {Dim Func={_ => ...}}
            if (string.Equals (methodName, "Func", StringComparison.OrdinalIgnoreCase))
            {
                // argsText may be a braced lambda: {_ => ...} or a plain lambda
                string lambdaText = argsText;
                if (lambdaText.StartsWith ("{") && lambdaText.EndsWith ("}"))
                {
                    lambdaText = lambdaText.Substring (1, lambdaText.Length - 2).Trim ();
                }

                // Emit: Dim.Func(_ => ...)
                ExpressionSyntax lambdaExpr = ParseExpression (lambdaText);
                return InvocationExpression (
                    MemberAccessExpression (
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName ("Dim"),
                        IdentifierName ("Func")))
                    .WithArgumentList (ArgumentList (SingletonSeparatedList (Argument (lambdaExpr))));
            }

            // For Dim.Auto and similar, argsText is often a braced set of key=value pairs
            if (argsText.StartsWith ("{") && argsText.EndsWith ("}"))
            {
                string inner = argsText.Substring (1, argsText.Length - 2);
                // Split into top-level ';' separated key=value pairs, respecting nested braces
                Dictionary<string, string> pairs = new (StringComparer.OrdinalIgnoreCase);
                int depth = 0;
                int start = 0;
                for (int i = 0; i < inner.Length; i++)
                {
                    char c = inner[i];

                    switch (c)
                    {
                        case '{':
                            depth++;

                            break;
                        case '}':
                            depth--;

                            break;
                        case ';' when depth == 0:
                        {
                            string part = inner.Substring (start, i - start);
                            int eq = part.IndexOf ('=');
                            if (eq >= 0)
                            {
                                string k = part.Substring (0, eq).Trim ();
                                string v = part.Substring (eq + 1).Trim ();
                                pairs[k] = v;
                            }
                            start = i + 1;

                            break;
                        }
                    }
                }
                // last part
                if (start < inner.Length)
                {
                    string part = inner.Substring (start).Trim ();
                    int eq = part.IndexOf ('=');
                    if (eq >= 0)
                    {
                        string k = part.Substring (0, eq).Trim ();
                        string v = part.Substring (eq + 1).Trim ();
                        pairs[k] = v;
                    }
                }

                // Build arguments for known named parameters (style, minimumContentDim, maximumContentDim)
                List<ArgumentSyntax> args = new ();

                if (pairs.TryGetValue ("style", out string? styleVal) && !string.IsNullOrEmpty (styleVal))
                {
                    // Map style token to the DimAutoStyle enum in Terminal.Gui.ViewBase
                    string enumExpr = $"Terminal.Gui.ViewBase.DimAutoStyle.{styleVal.Trim ()}";
                    args.Add (Argument (ParseExpression (enumExpr)).WithNameColon (NameColon (IdentifierName ("style"))));
                }

                if (pairs.TryGetValue ("minimumContentDim", out string? minVal) && !string.IsNullOrEmpty (minVal))
                {
                    ExpressionSyntax minExpr;
                    // If this is a nested Dim expression, parse recursively
                    if (Regex.IsMatch (minVal, "^\\{\\s*Dim", RegexOptions.IgnoreCase))
                    {
                        minExpr = ParseDimExpression (minVal, propertyName);
                    }
                    else if (minVal.StartsWith ("{") && minVal.EndsWith ("}"))
                    {
                        // Possibly a bare lambda: {_ => ...}
                        string lambdaInner = minVal.Substring (1, minVal.Length - 2).Trim ();
                        minExpr = InvocationExpression (
                            MemberAccessExpression (
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName ("Dim"),
                                IdentifierName ("Func")))
                            .WithArgumentList (ArgumentList (SingletonSeparatedList (Argument (ParseExpression (lambdaInner)))));
                    }
                    else
                    {
                        minExpr = ParseDimExpression (minVal, propertyName);
                    }

                    args.Add (Argument (minExpr).WithNameColon (NameColon (IdentifierName ("minimumContentDim"))));
                }

                if (pairs.TryGetValue ("maximumContentDim", out string? maxVal) && !string.IsNullOrEmpty (maxVal))
                {
                    ExpressionSyntax maxExpr;
                    if (Regex.IsMatch (maxVal, "^\\{\\s*Dim", RegexOptions.IgnoreCase))
                    {
                        maxExpr = ParseDimExpression (maxVal, propertyName);
                    }
                    else if (maxVal.StartsWith ("{") && maxVal.EndsWith ("}"))
                    {
                        string lambdaInner = maxVal.Substring (1, maxVal.Length - 2).Trim ();
                        maxExpr = InvocationExpression (
                            MemberAccessExpression (
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName ("Dim"),
                                IdentifierName ("Func")))
                            .WithArgumentList (ArgumentList (SingletonSeparatedList (Argument (ParseExpression (lambdaInner)))));
                    }
                    else
                    {
                        maxExpr = ParseDimExpression (maxVal, propertyName);
                    }

                    args.Add (Argument (maxExpr).WithNameColon (NameColon (IdentifierName ("maximumContentDim"))));
                }

                // Build invocation: Dim.{methodName}(...)
                InvocationExpressionSyntax invocation = InvocationExpression (
                    MemberAccessExpression (SyntaxKind.SimpleMemberAccessExpression, IdentifierName ("Dim"), IdentifierName (methodName)))
                    .WithArgumentList (ArgumentList (SeparatedList (args)));

                return invocation;
            }
        }

        // Try parsing as expression syntax: {MethodName} or {MethodName arg} or {MethodName "arg"}
        Match expressionMatch = Regex.Match (value, @"^\{\s*(\w+)(?:\s+(.+?))?\s*\}$");
        if (expressionMatch.Success)
        {
            string methodName = expressionMatch.Groups[1].Value;
            string args = expressionMatch.Groups[2].Success ? expressionMatch.Groups[2].Value.Trim () : string.Empty;

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
            string stringValue = quotedMatch.Groups[1].Value;
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
        if (Regex.IsMatch (args, "^[_a-zA-Z][_a-zA-Z0-9]*$"))
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
            string stringValue = quotedMatch.Groups[1].Value;
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