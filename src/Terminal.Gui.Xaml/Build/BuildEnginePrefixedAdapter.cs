using Terminal.Gui.Xaml.Documentation.Utilities;

namespace Terminal.Gui.Xaml.Build;

/// <summary>
/// Adapter that ensures all messages sent via <see cref="IBuildEngine"/> are
/// prefixed with the canonical documentation prefix (e.g. "DOCS:"). This keeps
/// message prefixing centralized and deterministic for CI parsing.
/// </summary>
public sealed class BuildEnginePrefixedAdapter : IBuildEngine
{
    private readonly IBuildEngine _inner;

    /// <summary>
    /// Creates a new <see cref="BuildEnginePrefixedAdapter"/> that wraps the
    /// provided <see cref="IBuildEngine"/> and ensures messages are prefixed
    /// with the documentation prefix.
    /// </summary>
    /// <param name="inner">The inner build engine to wrap.</param>
    public BuildEnginePrefixedAdapter(IBuildEngine inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    private static string MaybePrefix(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }
        return message.StartsWith(DocumentationUtilities.DocsPrefix, StringComparison.Ordinal) ? message : DocumentationUtilities.DocsPrefix + " " + message;
    }

    /// <summary>
    /// Logs an informational message via the inner build engine after applying
    /// the documentation prefix when not already present.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogMessage(string message)
    {
        _inner.LogMessage(MaybePrefix(message));
    }

    /// <summary>
    /// Logs an error message via the inner build engine after applying the
    /// documentation prefix when not already present.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public void LogError(string message)
    {
        _inner.LogError(MaybePrefix(message));
    }
}
