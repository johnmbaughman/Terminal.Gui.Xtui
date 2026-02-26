using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Terminal.Gui.Xtui.Generator.Helpers;

/// <summary>
/// Helper methods for transforming local variable declarations to private field assignments.
/// Extracted from TopLevelGenerator to support variable→field transformation pattern.
/// This helper enables class-level field declarations with proper initialization in methods.
/// </summary>
internal static class FieldTransformationHelpers
{
    /// <summary>
    /// Process a statement from a child generator, transforming local variable declarations
    /// to field assignments and collecting field declarations.
    /// </summary>
    /// <param name="statement">The statement to process</param>
    /// <param name="fieldDeclarations">List to collect field declarations</param>
    /// <param name="variableToFieldMap">Map of local variable names to field names</param>
    /// <param name="processedFields">Set of field names that have been declared</param>
    /// <returns>The transformed statement, or null if the statement should be skipped</returns>
    /// <example>
    /// ProcessStatement converts: var button0 = new Button { };
    /// To field assignment: this.button0 = new Button { };
    /// And adds field declaration: private Button? button0;
    /// </example>
    public static StatementSyntax? ProcessStatement(
        StatementSyntax statement,
        List<FieldDeclarationSyntax> fieldDeclarations,
        Dictionary<string, string> variableToFieldMap,
        HashSet<string> processedFields)
    {
        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (fieldDeclarations == null)
        {
            throw new ArgumentNullException(nameof(fieldDeclarations));
        }

        if (variableToFieldMap == null)
        {
            throw new ArgumentNullException(nameof(variableToFieldMap));
        }

        if (processedFields == null)
        {
            throw new ArgumentNullException(nameof(processedFields));
        }

        // If this is a local variable declaration, transform it to a field assignment
        if (statement is LocalDeclarationStatementSyntax localDecl)
        {
            VariableDeclaratorSyntax? variable = localDecl.Declaration.Variables.FirstOrDefault();
            if (variable != null && variable.Initializer != null)
            {
                string varName = variable.Identifier.Text;
                string fieldName = varName; // Use same name for field
                
                // Only create field if not already processed
                if (!processedFields.Contains(fieldName))
                {
                    variableToFieldMap[varName] = fieldName;

                    // Extract type from the initializer (ObjectCreationExpression)
                    string typeName;
                    if (variable.Initializer.Value is ObjectCreationExpressionSyntax objCreation)
                    {
                        // Get the type from the object creation expression
                        typeName = objCreation.Type.ToString();
                    }
                    else
                    {
                        // Fallback: try to get it from the declaration type if it's not 'var'
                        string declType = localDecl.Declaration.Type.ToString();
                        if (declType != "var" && declType != "var?")
                        {
                            typeName = declType.TrimEnd('?');
                        }
                        else
                        {
                            // Can't determine type, skip
                            return statement;
                        }
                    }

                    // Create field declaration
                    fieldDeclarations.Add(
                        FieldDeclaration(
                            VariableDeclaration(
                                NullableType(IdentifierName(typeName)))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(fieldName)))))
                        .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)))
                        .NormalizeWhitespace());

                    processedFields.Add(fieldName);
                }

                // Transform to field assignment: this.fieldName = initializer;
                return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(fieldName)),
                        variable.Initializer.Value))
                    .NormalizeWhitespace();
            }
        }

        // For non-declaration statements, replace any references to mapped variables
        // with field references (this.fieldName)
        return ReplaceVariableReferences(statement, variableToFieldMap);
    }

    /// <summary>
    /// Replace variable references with field references in a statement.
    /// </summary>
    /// <param name="statement">The statement to transform</param>
    /// <param name="variableToFieldMap">Map of variable names to field names</param>
    /// <returns>The transformed statement with field references</returns>
    /// <example>
    /// ReplaceVariableReferences converts: button0.Add(label0);
    /// To: this.button0.Add(this.label0);
    /// </example>
    public static StatementSyntax ReplaceVariableReferences(
        StatementSyntax statement,
        Dictionary<string, string> variableToFieldMap)
    {
        if (statement == null)
        {
            throw new ArgumentNullException(nameof(statement));
        }

        if (variableToFieldMap == null)
        {
            throw new ArgumentNullException(nameof(variableToFieldMap));
        }

        if (variableToFieldMap.Count == 0)
        {
            return statement;
        }

        // Use a syntax rewriter to replace identifier names
        VariableToFieldRewriter rewriter = new (variableToFieldMap);
        StatementSyntax transformed = (StatementSyntax)rewriter.Visit(statement);
        return transformed.NormalizeWhitespace();
    }

    /// <summary>
    /// Create a VariableToFieldRewriter instance for replacing variable references with field access.
    /// </summary>
    /// <param name="variableToFieldMap">Map of variable names to field names</param>
    /// <returns>A CSharpSyntaxRewriter configured for variable→field transformation</returns>
    /// <remarks>
    /// This method is primarily used for testing to verify the rewriter behavior.
    /// Production code should use ProcessStatement or ReplaceVariableReferences directly.
    /// </remarks>
    public static CSharpSyntaxRewriter CreateVariableToFieldRewriter(
        Dictionary<string, string> variableToFieldMap)
    {
        if (variableToFieldMap == null)
        {
            throw new ArgumentNullException(nameof(variableToFieldMap));
        }

        return new VariableToFieldRewriter(variableToFieldMap);
    }

    /// <summary>
    /// Syntax rewriter that replaces variable identifiers with field access expressions.
    /// </summary>
    private sealed class VariableToFieldRewriter (Dictionary<string, string> variableToFieldMap) : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, string> _variableToFieldMap = variableToFieldMap ?? throw new ArgumentNullException(nameof(variableToFieldMap));

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // If the expression is a simple identifier that maps to a field, replace the whole member access
            if (node.Expression is IdentifierNameSyntax identifier &&
                _variableToFieldMap.ContainsKey(identifier.Identifier.Text))
            {
                // Replace identifier.Member with this.fieldName.Member
                return node.WithExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName(_variableToFieldMap[identifier.Identifier.Text])));
            }

            return base.VisitMemberAccessExpression(node);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            string identifier = node.Identifier.Text;
            
            // If this identifier is a mapped variable, replace with this.fieldName
            if (_variableToFieldMap.ContainsKey(identifier))
            {
                // Check if this is already part of a member access (handled by VisitMemberAccessExpression)
                if (node.Parent is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression == node)
                {
                    // Don't replace here, let VisitMemberAccessExpression handle it
                    return base.VisitIdentifierName(node);
                }

                return MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName(_variableToFieldMap[identifier]));
            }

            return base.VisitIdentifierName(node);
        }
    }
}
