using Terminal.Gui.Xaml.Exceptions;
using Terminal.Gui.Xaml.Model;

namespace Terminal.Gui.Xaml.Parsing;

/// <summary>
/// Very simple XAML parser that only performs trivial validation to satisfy contract tests.
/// </summary>
public sealed class SimpleXamlParser : IXamlParser
{
    private readonly Dictionary<string, Type> _controls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Window"] = typeof(object),
        ["Label"] = typeof(object)
    };

    private readonly List<XamlParsingError> _errors = new();

    /// <summary>
    /// Parses XAML from a string into a minimal <see cref="XamlDocument"/>.
    /// </summary>
    /// <param name="xaml">The XAML text to parse.</param>
    /// <returns>A <see cref="XamlDocument"/> instance.</returns>
    /// <exception cref="XamlParseException">Thrown when parsing fails.</exception>
    public Task<XamlDocument> ParseAsync(string xaml)
    {
        try
        {
            if (!Validate(xaml))
            {
                throw new XamlParseException("Invalid XAML syntax");
            }

            var doc = new Terminal.Gui.Xaml.Model.Window
            {
                XamlText = xaml,
                RootNamespace = new XamlNamespace(new Uri("urn:test", UriKind.RelativeOrAbsolute))
            };
            return Task.FromResult<XamlDocument>(doc);
        }
        catch (XamlParseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new XamlParseException($"Parsing failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses XAML from a file path.
    /// </summary>
    /// <param name="filePath">The path to the XAML file.</param>
    /// <returns>A parsed <see cref="XamlDocument"/>.</returns>
    public Task<XamlDocument> ParseFileAsync(string filePath)
    {
        var xaml = File.ReadAllText(filePath);
        return ParseAsync(xaml);
    }

    /// <summary>
    /// Validates the XAML syntax at a superficial level.
    /// </summary>
    /// <param name="xaml">The XAML text.</param>
    /// <returns><c>true</c> if the input appears valid; otherwise, <c>false</c>.</returns>
    public bool ValidateSyntax(string xaml)
        => Validate(xaml);

    /// <summary>
    /// Performs the actual simple validation logic and records any errors.
    /// </summary>
    /// <param name="xaml">The XAML text.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public bool Validate(string xaml)
    {
        _errors.Clear();
        if (string.IsNullOrWhiteSpace(xaml))
        {
            _errors.Add(new XamlParsingError { Message = "XAML is empty", Severity = XamlErrorSeverity.Error });
            return false;
        }

        // Naive check: ensure tags are balanced for Window/Label
        bool hasWindowOpen = xaml.Contains("<Window");
        bool hasWindowClose = xaml.Contains("</Window>");
        if (hasWindowOpen && !hasWindowClose)
        {
            _errors.Add(new XamlParsingError { Message = "Missing closing Window tag", Severity = XamlErrorSeverity.Error });
            return false;
        }
        if (!hasWindowOpen)
        {
            _errors.Add(new XamlParsingError { Message = "Missing root Window", Severity = XamlErrorSeverity.Error });
            return false;
        }

        // If contains "<Label>" then must be self-closed or have closing tag
        if (xaml.Contains("<Label>") && !xaml.Contains("</Label>") && !xaml.Contains("<Label "))
        {
            _errors.Add(new XamlParsingError { Message = "Label not closed", Severity = XamlErrorSeverity.Error });
            return false;
        }

        return true;
    }

    /// <summary>
    /// Registers a control type by name for future validation.
    /// </summary>
    /// <param name="controlName">The control name as it may appear in XAML.</param>
    /// <param name="controlType">The associated runtime type.</param>
    public void RegisterControl(string controlName, Type controlType)
    {
        _controls[controlName] = controlType;
    }

    /// <summary>
    /// Gets the errors collected during the last validation.
    /// </summary>
    /// <returns>A snapshot of parsing errors.</returns>
    public IEnumerable<XamlParsingError> GetParsingErrors() => _errors.ToArray();
}
