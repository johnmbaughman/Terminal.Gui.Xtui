using System;

namespace Terminal.Gui.Xaml.Documentation.Logging;

/// <summary>
/// A no-op logger implementation used as a safe default when no logger is supplied.
/// </summary>
public sealed class NullDocumentationLogger : IDocumentationLogger
{
    /// <summary>Singleton instance to avoid repeated allocations.</summary>
    public static readonly IDocumentationLogger Instance = new NullDocumentationLogger();

    private NullDocumentationLogger() { }

    /// <inheritdoc />
    public void Debug(string message) { }
    /// <inheritdoc />
    public void Info(string message) { }
    /// <inheritdoc />
    public void Warn(string message) { }
    /// <inheritdoc />
    public void Error(string message, Exception? ex = null) { }
}
