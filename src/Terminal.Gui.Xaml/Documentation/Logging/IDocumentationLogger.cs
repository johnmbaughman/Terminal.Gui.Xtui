// Copyright (c) 2025
// Lightweight logging abstraction for documentation generation/validation
using System;

namespace Terminal.Gui.Xaml.Documentation.Logging;

/// <summary>
/// Minimal logger abstraction to avoid depending on external logging frameworks while still
/// enabling structured diagnostic messages for generation and validation workflows.
/// </summary>
public interface IDocumentationLogger
{
    /// <summary>Logs a verbose or diagnostic message not generally shown in normal summaries.</summary>
    void Debug(string message);

    /// <summary>Logs an informational message about high-level progress.</summary>
    void Info(string message);

    /// <summary>Logs a warning that indicates a non-fatal issue.</summary>
    void Warn(string message);

    /// <summary>Logs an error describing a failure (may still allow continuation).</summary>
    void Error(string message, Exception? ex = null);
}
