using System.IO;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Build;
using Terminal.Gui.Xaml.Documentation.Utilities;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class GenerateDocumentationTaskTests
{
    [Fact]
    public void Execute_ShouldLogDocsPrefix_WhenConfigMissing()
    {
        // Arrange
        var task = new GenerateDocumentationTask();
        var recorder = new TestBuildEngine();
        task.BuildEngine = recorder;
        task.DocumentationEnabled = true;
        task.DocumentationConfiguration = Path.Combine(Path.GetTempPath(), "non-existent-docfx.json");

        // Act
        var result = task.Execute();

        // Assert - should fail and log the standardized docs prefix
        Assert.False(result);
        Assert.Contains(DocumentationUtilities.DocsPrefix, recorder.LastError ?? string.Empty);
    }

    internal class TestBuildEngine : IBuildEngine
    {
        public string? LastMessage { get; private set; }
        public string? LastError { get; private set; }
        public void LogMessage(string message) => LastMessage = message;
        public void LogError(string message) => LastError = message;
    }
}
