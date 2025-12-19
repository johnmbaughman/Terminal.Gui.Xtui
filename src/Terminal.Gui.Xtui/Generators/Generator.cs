using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terminal.Gui.Xtui.Generators;

/// <summary>
/// Base class for code generators that produce C# syntax to create and
/// configure UI elements parsed from .xtui files.
///
/// Concrete generators should override <see cref="GenerateStatements"/>
/// to return the statements required to construct a control and configure
/// its properties, and may override <see cref="GenerateClass"/> to return
/// a full generated class source when appropriate.
/// </summary>
internal abstract class Generator
{
    /// <summary>
    /// Generates the set of <see cref="StatementSyntax"/> nodes that create
    /// and configure the control represented by <paramref name="node"/>.
    ///
    /// Implementations should produce one or more statements (for example,
    /// a local variable declaration followed by property initializers) that
    /// can be inserted into the generated <c>InitializeComponent</c> method.
    /// </summary>
    /// <param name="node">The parsed element node describing the control.</param>
    /// <param name="variableName">The name of the local variable to use for the created instance.</param>
    /// <param name="generators">Factory to obtain other generators for child elements.</param>
    /// <returns>
    /// An array of <see cref="StatementSyntax"/> representing the statements
    /// required to create and initialize the control.
    /// </returns>
    internal virtual StatementSyntax[] GenerateStatements(ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        return Array.Empty<StatementSyntax>();
    }

    /// <summary>
    /// Public wrapper that returns the generated statements as formatted C# source.
    /// This preserves the internal use of Roslyn <see cref="StatementSyntax"/>
    /// while exposing a string-based API suitable for public consumers and tests.
    /// </summary>
    public string GenerateStatementsAsString(ElementNode node, string variableName, IGeneratorFactory generators)
    {
        var stmts = GenerateStatements(node, variableName, generators);
        // Create a temporary block to get formatted source for the statements
        var block = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(stmts);
        return block.ToFullString();
    }

    /// <summary>
    /// Optionally generates a complete class source for the given element.
    ///
    /// This method is used when a generator wants to emit an entire partial
    /// class (for example to provide an explicit <c>InitializeComponent</c>
    /// implementation) rather than only returning statements. The default
    /// implementation returns an empty string.
    /// </summary>
    /// <param name="node">The parsed element node representing the root element.</param>
    /// <param name="namespaceName">The namespace to place the generated class in.</param>
    /// <param name="className">The name of the generated class.</param>
    /// <param name="generators">Factory used to create generators for child elements.</param>
    /// <returns>
    /// The generated C# source for the class, or an empty string if the
    /// generator does not emit a full class.
    /// </returns>
    public virtual string GenerateClass(ElementNode node, string namespaceName, string className,
        IGeneratorFactory generators)
    {
        return string.Empty;
    }
}