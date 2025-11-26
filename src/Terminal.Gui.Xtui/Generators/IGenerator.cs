using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Terminal.Gui.Xtui.Generators;

internal abstract class Generator
{
    /// <summary>
    /// Generates statements to create and configure the control.
    /// Returns a list of statements that declare and initialize the control.
    /// </summary>
    public virtual StatementSyntax[] GenerateStatements(ElementNode node, string variableName,
        IGeneratorFactory generators)
    {
        return [];
    }

    public virtual string GenerateClass(ElementNode node, string namespaceName, string className,
        IGeneratorFactory generators)
    {
        return string.Empty;
    }
}