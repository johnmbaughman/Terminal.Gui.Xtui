using System.Collections.Generic;
using Terminal.Gui.Xaml.Build;
using Terminal.Gui.Xaml.Documentation.Utilities;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class BuildEnginePrefixedAdapterTests
{
    private class RecorderEngine : IBuildEngine
    {
        public List<string> Messages { get; } = new();
        public List<string> Errors { get; } = new();
        public void LogMessage(string message) => Messages.Add(message);
        public void LogError(string message) => Errors.Add(message);
    }

    [Fact]
    public void Adapter_Prefixes_Messages_And_Errors()
    {
        var recorder = new RecorderEngine();
        var adapter = new BuildEnginePrefixedAdapter(recorder);

        adapter.LogMessage("Starting docs");
        adapter.LogError("Failure happened");

        Assert.Contains(recorder.Messages, m => m.StartsWith(DocumentationUtilities.DocsPrefix));
        Assert.Contains(recorder.Errors, e => e.StartsWith(DocumentationUtilities.DocsPrefix));
    }
}
