using System;
using System.Collections.Generic;
using Terminal.Gui.Xaml.Documentation.Logging;
using Terminal.Gui.Xaml.Documentation.Utilities;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

class TestLogger : IDocumentationLogger
{
    public List<string> Messages { get; } = new();
    public void Debug(string message) => Messages.Add("DBG:" + message);
    public void Info(string message) => Messages.Add("INF:" + message);
    public void Warn(string message) => Messages.Add("WRN:" + message);
    public void Error(string message, Exception? ex = null) => Messages.Add("ERR:" + message + (ex != null ? "::" + ex.Message : string.Empty));
}

public class PrefixedDocumentationLoggerTests
{
    [Fact]
    public void WarnAndErrorArePrefixedByDefault()
    {
        var inner = new TestLogger();
        var wrapper = new PrefixedDocumentationLogger(inner);

        wrapper.Debug("d");
        wrapper.Info("i");
        wrapper.Warn("w");
        wrapper.Error("e");

        Assert.Contains(inner.Messages, m => m.StartsWith("WRN:" + DocumentationUtilities.DocsPrefix, StringComparison.Ordinal));
        Assert.Contains(inner.Messages, m => m.StartsWith("ERR:" + DocumentationUtilities.DocsPrefix, StringComparison.Ordinal));
        Assert.Contains(inner.Messages, m => m.StartsWith("DBG:d"));
        Assert.Contains(inner.Messages, m => m.StartsWith("INF:i"));
    }

    [Fact]
    public void PrefixAllOptionPrefixesAllLevels()
    {
        var inner = new TestLogger();
        var wrapper = new PrefixedDocumentationLogger(inner, prefixAll: true);

        wrapper.Debug("d");
        wrapper.Info("i");
        wrapper.Warn("w");
        wrapper.Error("e");

        Assert.Contains(inner.Messages, m => m.Contains(DocumentationUtilities.DocsPrefix));
    }
}
