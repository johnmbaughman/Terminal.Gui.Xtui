using System;
using System.Text;

namespace Terminal.Gui.Xaml.Documentation.Logging;

/// <summary>
/// Simple console logger implementation. In library contexts this may be replaced by an adapter
/// over an application logging framework.
/// </summary>
public sealed class ConsoleDocumentationLogger : IDocumentationLogger
{
    private readonly object _lock = new();
    private readonly bool _includeTimestamps;

    /// <summary>
    /// Creates a new console logger.
    /// </summary>
    /// <param name="includeTimestamps">If true, UTC timestamps are prefixed to each line.</param>
    public ConsoleDocumentationLogger(bool includeTimestamps = true)
    {
        _includeTimestamps = includeTimestamps;
    }

    private void Write(string level, string message, ConsoleColor color, Exception? ex = null)
    {
        lock (_lock)
        {
            var previous = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                var prefix = _includeTimestamps ? DateTimeOffset.Now.ToString("u") + " " : string.Empty;
                var sb = new StringBuilder();
                sb.Append(prefix).Append('[').Append(level).Append("] ").Append(message);
                if (ex != null)
                {
                    sb.Append(" :: ").Append(ex.GetType().Name).Append(':').Append(' ').Append(ex.Message);
                }
                Console.WriteLine(sb.ToString());
            }
            finally
            {
                Console.ForegroundColor = previous;
            }
        }
    }

    /// <inheritdoc />
    public void Debug(string message) => Write("DBG", message, ConsoleColor.DarkGray);
    /// <inheritdoc />
    public void Info(string message) => Write("INF", message, ConsoleColor.Gray);
    /// <inheritdoc />
    public void Warn(string message) => Write("WRN", message, ConsoleColor.Yellow);
    /// <inheritdoc />
    public void Error(string message, Exception? ex = null) => Write("ERR", message, ConsoleColor.Red, ex);
}
