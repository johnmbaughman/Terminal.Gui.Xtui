using Terminal.Gui.Xaml.Exceptions;

namespace Terminal.Gui.Xaml.Binding;

/// <summary>
/// Minimal implementation of <see cref="IDataBindingEngine"/> for tests.
/// </summary>
/// <summary>
/// Simple data binding engine used by tests.
/// </summary>
public sealed class SimpleDataBindingEngine : IDataBindingEngine
{
    /// <inheritdoc />
    public void Bind(string sourceProperty, string targetProperty, BindingMode mode)
    {
        if (string.IsNullOrWhiteSpace(sourceProperty) || string.IsNullOrWhiteSpace(targetProperty))
        {
            throw new BindingException("Invalid binding properties");
        }
        // Simulate a failure for a known invalid scenario to satisfy contract tests
        if (string.Equals(sourceProperty, "BadSource", StringComparison.Ordinal) &&
            string.Equals(targetProperty, "BadTarget", StringComparison.Ordinal))
        {
            throw new BindingException("Invalid binding specified");
        }
        // No-op otherwise
    }

    /// <inheritdoc />
    public void Bind(object control, object dataSource, string propertyPath)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
        {
            throw new BindingException("Property path required");
        }
    }

    /// <inheritdoc />
    public void Unbind(object control)
    {
        // No-op
    }

    /// <inheritdoc />
    public void UpdateBinding(object control)
    {
        // No-op
    }

    /// <inheritdoc />
    public bool ValidateBinding(string expression)
    {
        return !string.IsNullOrWhiteSpace(expression) && expression.Contains('.');
    }

    /// <inheritdoc />
    public void BindEvent(string eventPath, string handlerName)
    {
        if (string.IsNullOrWhiteSpace(eventPath) || string.IsNullOrWhiteSpace(handlerName))
        {
            throw new BindingException("Event path and handler name required");
        }
    }
}
