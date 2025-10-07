using System;
using Terminal.Gui.Xaml.Documentation.Utilities;

namespace Terminal.Gui.Xaml.Documentation.Logging;

/// <summary>
/// Logger decorator that prefixes messages with the standardized DOCS prefix where appropriate.
/// This centralizes where the DOCS: prefix is applied so callers don't need to embed it in strings.
/// </summary>
public sealed class PrefixedDocumentationLogger : IDocumentationLogger
{
    private readonly IDocumentationLogger _inner;
    private readonly bool _prefixAll;

    /// <summary>
    /// Creates a new wrapper logger.
    /// </summary>
    /// <param name="inner">The inner logger to forward messages to.</param>
    /// <param name="prefixAll">If true, prefix all messages (debug/info/warn/error). If false, prefix only warnings and errors.</param>
    public PrefixedDocumentationLogger(IDocumentationLogger inner, bool prefixAll = false)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _prefixAll = prefixAll;
    }

    private string MaybePrefix(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        if (_prefixAll)
        {
            return DocumentationUtilities.DocsPrefix + " " + message;
        }

        // If message already starts with the DocsPrefix, leave it alone.
        return message.StartsWith(DocumentationUtilities.DocsPrefix, StringComparison.Ordinal) ? message : message;
    }

    /// <inheritdoc />
    public void Debug(string message)
    {
    _inner.Debug(MaybePrefix(message));
    }

    /// <inheritdoc />
    public void Info(string message)
    {
    _inner.Info(MaybePrefix(message));
    }

    /// <inheritdoc />
    public void Warn(string message)
    {
    _inner.Warn(DocumentationUtilities.DocsPrefix + " " + message);
    }

    /// <inheritdoc />
    public void Error(string message, Exception? ex = null)
    {
    _inner.Error(DocumentationUtilities.DocsPrefix + " " + message, ex);
    }
}
