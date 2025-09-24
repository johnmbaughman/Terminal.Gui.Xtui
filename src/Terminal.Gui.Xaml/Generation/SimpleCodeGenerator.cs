using Terminal.Gui.Xaml.Exceptions;

namespace Terminal.Gui.Xaml.Generation;

/// <summary>
/// Minimal implementation of <see cref="ICodeGenerator"/> that generates placeholder code.
/// </summary>
public sealed class SimpleCodeGenerator : ICodeGenerator
{
    private readonly Dictionary<string, string> _templates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Generates source code for a single XAML document.
    /// </summary>
    /// <param name="xaml">The XAML text.</param>
    /// <returns>Generated source code as a string.</returns>
    /// <exception cref="CodeGenerationException">Thrown when the XAML fails validation.</exception>
    public Task<string> GenerateAsync(string xaml)
    {
        if (!ValidateGeneration(xaml))
        {
            throw new CodeGenerationException("Invalid XAML for generation");
        }

        var code = "// Generated from XAML\nnamespace Generated { public class View { public string Title => \"Hello\"; } }";
        return Task.FromResult(code);
    }

    /// <summary>
    /// Generates multiple class descriptors for a set of XAML documents.
    /// </summary>
    /// <param name="xamlDocuments">A sequence of XAML strings.</param>
    /// <returns>A sequence of <see cref="GeneratedClass"/> items.</returns>
    /// <exception cref="CodeGenerationException">Thrown when any document fails validation.</exception>
    public Task<IEnumerable<GeneratedClass>> GenerateClassesAsync(IEnumerable<string> xamlDocuments)
    {
        var result = new List<GeneratedClass>();
        int i = 0;
        foreach (var xaml in xamlDocuments)
        {
            if (!ValidateGeneration(xaml))
            {
                throw new CodeGenerationException("Invalid XAML in collection");
            }

            result.Add(new GeneratedClass
            {
                ClassName = $"Generated{i++}",
                Namespace = "Generated",
                Code = "// generated"
            });
        }

        return Task.FromResult<IEnumerable<GeneratedClass>>(result);
    }

    /// <summary>
    /// Validates that the XAML is adequate for generation in this simple implementation.
    /// </summary>
    /// <param name="xaml">The XAML text.</param>
    /// <returns><c>true</c> if the XAML appears valid; otherwise, <c>false</c>.</returns>
    public bool ValidateGeneration(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml))
        {
            return false;
        }

        var hasWindowOpen = xaml.Contains("<Window");
        var hasWindowClose = xaml.Contains("</Window>");
        var hasWindowSelfClose = xaml.Contains("<Window/>") || (xaml.Contains("<Window ") && xaml.Contains("/>"));
        if (!hasWindowOpen || (!hasWindowClose && !hasWindowSelfClose))
        {
            return false;
        }

        // If there's an explicit <Label> (no attributes), ensure it's closed
        if (xaml.Contains("<Label>") && !xaml.Contains("</Label>"))
        {
            return false;
        }

        // If there's a non-self-closing <Label ...>, ensure there is a closing tag
        if (xaml.Contains("<Label ") && !xaml.Contains("/>") && !xaml.Contains("</Label>"))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Registers or replaces a named code generation template.
    /// </summary>
    /// <param name="templateName">The template identifier.</param>
    /// <param name="templateContent">The template body.</param>
    public void RegisterTemplate(string templateName, string templateContent)
    {
        _templates[templateName] = templateContent;
    }
}
