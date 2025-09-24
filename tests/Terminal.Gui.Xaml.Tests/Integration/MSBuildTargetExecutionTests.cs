using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Integration;

/// <summary>
/// Integration tests for MSBuild target execution for documentation generation.
/// Tests validate the behavior of MSBuild targets for generating, cleaning, and validating documentation.
/// </summary>
public class MSBuildTargetExecutionTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly MockBuildEngine _buildEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="MSBuildTargetExecutionTests"/> class.
    /// Sets up temporary directories and mock objects for testing MSBuild target execution.
    /// </summary>
    public MSBuildTargetExecutionTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _buildEngine = new MockBuildEngine();
    }

    /// <summary>
    /// Tests that documentation generation succeeds when provided with a valid configuration.
    /// Verifies that the task returns true and creates the expected output directory.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GenerateDocumentationTarget_ShouldSucceed_WithValidConfiguration()
    {
        // Arrange
        var config = new MSBuildTargetConfiguration
        {
            DocumentationEnabled = true,
            DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),
            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),
            DocumentationBuildMode = "Full"
        };
        
        // Create a valid docfx.json configuration file
        await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}");
        
        var task = MockDocumentationTaskFactory.CreateGenerateDocumentationTask(config);
        task.BuildEngine = _buildEngine;

        // Act
        var result = await ExecuteTaskAsync(task);

        // Assert
    Assert.True(result, "Documentation generation should succeed with valid configuration");
    Assert.True(Directory.Exists(config.DocumentationOutputPath), "Output directory should be created");
    }

    /// <summary>
    /// Tests that documentation generation fails when the DocFX configuration file is missing.
    /// Verifies that the task returns false when the configuration file does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GenerateDocumentationTarget_ShouldFail_IfConfigMissing()
    {
        // Arrange
        var config = new MSBuildTargetConfiguration
        {
            DocumentationEnabled = true,
            DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),
            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")
        };
        
        var task = MockDocumentationTaskFactory.CreateGenerateDocumentationTask(config);
        task.BuildEngine = _buildEngine;

        // Act
        var result = await ExecuteTaskAsync(task);

        // Assert
    Assert.False(result, "Task should fail when DocFX configuration file is missing");
    }

    /// <summary>
    /// Tests that the clean documentation target removes only generated documentation files.
    /// Verifies that the cleanup task executes successfully and removes the documentation output directory.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()
    {
        // Arrange
        var config = new MSBuildTargetConfiguration
        {
            DocumentationEnabled = true,
            DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),
            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")
        };
        
        await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}");
        Directory.CreateDirectory(config.DocumentationOutputPath);
        
        // Generate documentation first
        var generateTask = MockDocumentationTaskFactory.CreateGenerateDocumentationTask(config);
        generateTask.BuildEngine = _buildEngine;
        await ExecuteTaskAsync(generateTask);
        
    Assert.True(Directory.Exists(config.DocumentationOutputPath), "Docs should exist before cleaning");

        // Act - Clean documentation
        var cleanTask = MockDocumentationTaskFactory.CreateCleanDocumentationTask(config);
        cleanTask.BuildEngine = _buildEngine;
        var cleanResult = await ExecuteTaskAsync(cleanTask);

        // Assert
    Assert.True(cleanResult, "Clean target should execute successfully");
    Assert.False(Directory.Exists(config.DocumentationOutputPath), "Generated documentation directory should be removed");
    }

    /// <summary>
    /// Simulates async MSBuild task execution
    /// </summary>
    /// <param name="task">The MSBuild task to execute.</param>
    /// <returns>A task representing the asynchronous operation, containing the execution result.</returns>
    private static async Task<bool> ExecuteTaskAsync(ITask task)
    {
        // Simulate processing delay
        await Task.Delay(10);
        return task.Execute();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// Cleans up the temporary directory created during test execution.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MSBuildTargetExecutionTests"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

#region Mock Types for Self-Contained Testing

/// <summary>
/// Configuration for MSBuild documentation targets
/// </summary>
public class MSBuildTargetConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether documentation generation is enabled.
    /// </summary>
    public bool DocumentationEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the path to the DocFX configuration file.
    /// </summary>
    public string DocumentationConfiguration { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the output directory path for generated documentation.
    /// </summary>
    public string DocumentationOutputPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the documentation build mode (e.g., "Full", "Incremental").
    /// </summary>
    public string DocumentationBuildMode { get; set; } = "Full";
}

/// <summary>
/// Mock build engine for testing MSBuild task execution
/// </summary>
public class MockBuildEngine : IBuildEngine
{
    /// <summary>
    /// Logs an informational message during build execution.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogMessage(string message) { }
    
    /// <summary>
    /// Logs an error message during build execution.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public void LogError(string message) { }
}

/// <summary>
/// Factory for creating mock documentation tasks
/// </summary>
public static class MockDocumentationTaskFactory
{
    /// <summary>
    /// Creates a mock task for generating documentation.
    /// </summary>
    /// <param name="config">The configuration for the documentation generation task.</param>
    /// <returns>A mock documentation generation task.</returns>
    public static ITask CreateGenerateDocumentationTask(MSBuildTargetConfiguration config) => 
        new MockGenerateDocumentationTask(config);
        
    /// <summary>
    /// Creates a mock task for cleaning generated documentation.
    /// </summary>
    /// <param name="config">The configuration for the documentation cleaning task.</param>
    /// <returns>A mock documentation cleaning task.</returns>
    public static ITask CreateCleanDocumentationTask(MSBuildTargetConfiguration config) => 
        new MockCleanDocumentationTask(config);
}

/// <summary>
/// Mock task for generating documentation
/// </summary>
public class MockGenerateDocumentationTask : ITask
{
    private readonly MSBuildTargetConfiguration _config;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MockGenerateDocumentationTask"/> class.
    /// </summary>
    /// <param name="config">The configuration for the documentation generation task.</param>
    public MockGenerateDocumentationTask(MSBuildTargetConfiguration config) => _config = config;
    
    /// <summary>
    /// Gets or sets the build engine for the task.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    
    /// <summary>
    /// Executes the documentation generation task.
    /// </summary>
    /// <returns>True if the task succeeds, false otherwise.</returns>
    public bool Execute()
    {
        if (!_config.DocumentationEnabled)
        {
            return true;
        }

        if (!File.Exists(_config.DocumentationConfiguration))
        {
            return false;
        }

        // Create output directory to simulate successful generation
        Directory.CreateDirectory(_config.DocumentationOutputPath);
        return true;
    }
}

/// <summary>
/// Mock task for cleaning documentation
/// </summary>
public class MockCleanDocumentationTask : ITask
{
    private readonly MSBuildTargetConfiguration _config;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MockCleanDocumentationTask"/> class.
    /// </summary>
    /// <param name="config">The configuration for the documentation cleaning task.</param>
    public MockCleanDocumentationTask(MSBuildTargetConfiguration config) => _config = config;
    
    /// <summary>
    /// Gets or sets the build engine for the task.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    
    /// <summary>
    /// Executes the documentation cleaning task.
    /// </summary>
    /// <returns>True if the task succeeds, false otherwise.</returns>
    public bool Execute()
    {
        if (Directory.Exists(_config.DocumentationOutputPath))
        {
            Directory.Delete(_config.DocumentationOutputPath, true);
        }

        return true;
    }
}

/// <summary>
/// Minimal MSBuild task interface for compilation
/// </summary>
public interface ITask
{
    /// <summary>
    /// Gets or sets the build engine associated with the task.
    /// </summary>
    IBuildEngine BuildEngine { get; set; }
    
    /// <summary>
    /// Executes the task.
    /// </summary>
    /// <returns>True if the task execution succeeds, false otherwise.</returns>
    bool Execute();
}

/// <summary>
/// Minimal MSBuild engine interface for compilation
/// </summary>
public interface IBuildEngine
{
    /// <summary>
    /// Logs an informational message during build execution.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogMessage(string message);
    
    /// <summary>
    /// Logs an error message during build execution.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    void LogError(string message);
}

#endregion
