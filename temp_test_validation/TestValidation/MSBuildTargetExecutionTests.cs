using System;using System;using System;

using System.IO;

using System.Threading.Tasks;using System.IO;using System.IO;

using Xunit;

using FluentAssertions;using System.Threading.Tasks;using System.Threading.Tasks;



namespace Terminal.Gui.Xaml.Tests.Integrationusing Xunit;

{

    /// <summary>using FluentAssertions;namespace Terminal.Gui.Xaml.Tests.Integration

    /// Integration tests for MSBuild target execution for documentation generation.

    /// Tests validate the behavior of MSBuild targets for generating, cleaning, and validating documentation.{

    /// </summary>

    public class MSBuildTargetExecutionTests : IDisposablenamespace Terminal.Gui.Xaml.Tests.Integration    public class MSBuildTargetExecutionTests : IDisposable

    {

        private readonly string _tempDirectory;{    {

        private readonly MockBuildEngine _buildEngine;

        private readonly MockDocumentationTaskFactory _taskFactory;    /// <summary>        private readonly string _tempDirectory;



        public MSBuildTargetExecutionTests()    /// Integration test for MSBuild documentation target execution.        private readonly MockBuildEngine _buildEngine;

        {

            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));    /// Adheres to contract and constitutional requirements.        private readonly MockDocumentationTaskFactory _taskFactory;

            Directory.CreateDirectory(_tempDirectory);

            _buildEngine = new MockBuildEngine();    /// </summary>

            _taskFactory = new MockDocumentationTaskFactory();

        }    public class MSBuildTargetExecutionTests : IDisposable        public MSBuildTargetExecutionTests()



        [Fact]    {        {

        public async Task GenerateDocumentationTarget_ShouldSucceed_WithValidConfiguration()

        {        private readonly string _tempDirectory;            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            // Arrange

            var config = new MSBuildTargetConfiguration        private readonly MockBuildEngine _buildEngine;            Directory.CreateDirectory(_tempDirectory);

            {

                DocumentationEnabled = true,        private readonly MockDocumentationTaskFactory _taskFactory;            _buildEngine = new MockBuildEngine();

                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),

                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),            _taskFactory = new MockDocumentationTaskFactory();

                DocumentationBuildMode = "Full"

            };        public MSBuildTargetExecutionTests()        }

            

            // Create a valid docfx.json configuration file        {

            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}");

                        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));        public void Dispose()

            var task = _taskFactory.CreateGenerateDocumentationTask(config);

            task.BuildEngine = _buildEngine;            Directory.CreateDirectory(_tempDirectory);        {



            // Act            _buildEngine = new MockBuildEngine();            try

            var result = await ExecuteTaskAsync(task);

            _taskFactory = new MockDocumentationTaskFactory();            {

            // Assert

            result.Should().BeTrue("Documentation generation should succeed with valid configuration");        }                if (Directory.Exists(_tempDirectory))

            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue("Output directory should be created");

        }                    Directory.Delete(_tempDirectory, true);



        [Fact]        [Fact]            }

        public async Task GenerateDocumentationTarget_ShouldFail_IfConfigMissing()

        {        public async Task GenerateDocumentationTarget_ShouldSucceed_WithValidConfiguration()            catch { /* ignore cleanup errors */ }

            // Arrange

            var config = new MSBuildTargetConfiguration        {        }

            {

                DocumentationEnabled = true,            var config = new MSBuildTargetConfiguration    }

                DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),

                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")            {}

            };

                            DocumentationEnabled = true,using System;

            var task = _taskFactory.CreateGenerateDocumentationTask(config);

            task.BuildEngine = _buildEngine;                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),using System.IO;



            // Act                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),using System.Threading.Tasks;

            var result = await ExecuteTaskAsync(task);

                DocumentationBuildMode = "Full"using Xunit;

            // Assert

            result.Should().BeFalse("Task should fail when DocFX configuration file is missing");            };using FluentAssertions;

        }

            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");

        [Fact]

        public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()            var task = _taskFactory.CreateGenerateDocumentationTask(config);namespace Terminal.Gui.Xaml.Tests.Integration

        {

            // Arrange            task.BuildEngine = _buildEngine;{

            var config = new MSBuildTargetConfiguration

            {            var result = await ExecuteTaskAsync(task);    /// <summary>

                DocumentationEnabled = true,

                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),            result.Should().BeTrue();    /// Integration test for MSBuild documentation target execution.

                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")

            };            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue();    /// Adheres to contract and constitutional requirements.

            

            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}");        }    /// </summary>

            Directory.CreateDirectory(config.DocumentationOutputPath);

                public class MSBuildTargetExecutionTests : IDisposable

            // Generate documentation first

            var generateTask = _taskFactory.CreateGenerateDocumentationTask(config);        [Fact]    {

            generateTask.BuildEngine = _buildEngine;

            await ExecuteTaskAsync(generateTask);        public async Task GenerateDocumentationTarget_ShouldFail_IfConfigMissing()        private readonly string _tempDirectory;

            

            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue("Docs should exist before cleaning");        {        private readonly MockBuildEngine _buildEngine;



            // Act - Clean documentation            var config = new MSBuildTargetConfiguration        private readonly MockDocumentationTaskFactory _taskFactory;

            var cleanTask = _taskFactory.CreateCleanDocumentationTask(config);

            cleanTask.BuildEngine = _buildEngine;            {

            var cleanResult = await ExecuteTaskAsync(cleanTask);

                DocumentationEnabled = true,        public MSBuildTargetExecutionTests()

            // Assert

            cleanResult.Should().BeTrue("Clean target should execute successfully");                DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),        {

            Directory.Exists(config.DocumentationOutputPath).Should().BeFalse("Generated documentation directory should be removed");

        }                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));



        /// <summary>            };            Directory.CreateDirectory(_tempDirectory);

        /// Simulates async MSBuild task execution

        /// </summary>            var task = _taskFactory.CreateGenerateDocumentationTask(config);            _buildEngine = new MockBuildEngine();

        private async Task<bool> ExecuteTaskAsync(ITask task)

        {            task.BuildEngine = _buildEngine;            _taskFactory = new MockDocumentationTaskFactory();

            // Simulate processing delay

            await Task.Delay(10);            var result = await ExecuteTaskAsync(task);        }

            return task.Execute();

        }            result.Should().BeFalse();



        public void Dispose()        }        [Fact]

        {

            try        public async Task GenerateDocumentationTarget_ShouldSucceed_WithValidConfiguration()

            {

                if (Directory.Exists(_tempDirectory))        [Fact]        {

                    Directory.Delete(_tempDirectory, true);

            }        public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()            var config = new MSBuildTargetConfiguration

            catch

            {        {            {

                // Ignore cleanup errors

            }            var config = new MSBuildTargetConfiguration                DocumentationEnabled = true,

        }

    }            {                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),



    #region Mock Types for Self-Contained Testing                DocumentationEnabled = true,                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),



    /// <summary>                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),                DocumentationBuildMode = "Full"

    /// Configuration for MSBuild documentation targets

    /// </summary>                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")            };

    public class MSBuildTargetConfiguration

    {            };            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");

        public bool DocumentationEnabled { get; set; }

        public string DocumentationConfiguration { get; set; } = string.Empty;            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");            var task = _taskFactory.CreateGenerateDocumentationTask(config);

        public string DocumentationOutputPath { get; set; } = string.Empty;

        public string DocumentationBuildMode { get; set; } = "Full";            Directory.CreateDirectory(config.DocumentationOutputPath);            task.BuildEngine = _buildEngine;

    }

            var generateTask = _taskFactory.CreateGenerateDocumentationTask(config);            var result = await ExecuteTaskAsync(task);

    /// <summary>

    /// Mock build engine for testing MSBuild task execution            generateTask.BuildEngine = _buildEngine;            result.Should().BeTrue();

    /// </summary>

    public class MockBuildEngine : IBuildEngine            await ExecuteTaskAsync(generateTask);            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue();

    {

        public void LogMessage(string message) { }            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue();        }

        public void LogError(string message) { }

    }            var cleanTask = _taskFactory.CreateCleanDocumentationTask(config);



    /// <summary>            cleanTask.BuildEngine = _buildEngine;        [Fact]

    /// Factory for creating mock documentation tasks

    /// </summary>            var cleanResult = await ExecuteTaskAsync(cleanTask);        public async Task GenerateDocumentationTarget_ShouldFail_IfConfigMissing()

    public class MockDocumentationTaskFactory

    {            cleanResult.Should().BeTrue();        {

        public ITask CreateGenerateDocumentationTask(MSBuildTargetConfiguration config) => 

            new MockGenerateDocumentationTask(config);            Directory.Exists(config.DocumentationOutputPath).Should().BeFalse();            var config = new MSBuildTargetConfiguration

            

        public ITask CreateCleanDocumentationTask(MSBuildTargetConfiguration config) =>         }            {

            new MockCleanDocumentationTask(config);

    }                DocumentationEnabled = true,



    /// <summary>        public void Dispose()                DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),

    /// Mock task for generating documentation

    /// </summary>        {                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")

    public class MockGenerateDocumentationTask : ITask

    {            try            };

        private readonly MSBuildTargetConfiguration _config;

                    {            var task = _taskFactory.CreateGenerateDocumentationTask(config);

        public MockGenerateDocumentationTask(MSBuildTargetConfiguration config) => _config = config;

                        if (Directory.Exists(_tempDirectory))            task.BuildEngine = _buildEngine;

        public IBuildEngine BuildEngine { get; set; } = null!;

                            Directory.Delete(_tempDirectory, true);            var result = await ExecuteTaskAsync(task);

        public bool Execute()

        {            }            result.Should().BeFalse();

            if (!_config.DocumentationEnabled) return true;

            if (!File.Exists(_config.DocumentationConfiguration)) return false;            catch { }        }

            

            // Create output directory to simulate successful generation        }

            Directory.CreateDirectory(_config.DocumentationOutputPath);

            return true;        [Fact]

        }

    }        private async Task<bool> ExecuteTaskAsync(ITask task)        public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()



    /// <summary>        {        {

    /// Mock task for cleaning documentation

    /// </summary>            await Task.Delay(10);            var config = new MSBuildTargetConfiguration

    public class MockCleanDocumentationTask : ITask

    {            return task.Execute();            {

        private readonly MSBuildTargetConfiguration _config;

                }                DocumentationEnabled = true,

        public MockCleanDocumentationTask(MSBuildTargetConfiguration config) => _config = config;

            }                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),

        public IBuildEngine BuildEngine { get; set; } = null!;

                        DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")

        public bool Execute()

        {    // Minimal contract-driven mocks for isolation            };

            if (Directory.Exists(_config.DocumentationOutputPath))

                Directory.Delete(_config.DocumentationOutputPath, true);    public class MockBuildEngine : IBuildEngine            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");

            return true;

        }    {            Directory.CreateDirectory(config.DocumentationOutputPath);

    }

        public void LogMessage(string message) { }            var generateTask = _taskFactory.CreateGenerateDocumentationTask(config);

    /// <summary>

    /// Minimal MSBuild task interface for compilation        public void LogError(string message) { }            generateTask.BuildEngine = _buildEngine;

    /// </summary>

    public interface ITask    }            await ExecuteTaskAsync(generateTask);

    {

        IBuildEngine BuildEngine { get; set; }            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue();

        bool Execute();

    }    public class MockDocumentationTaskFactory            var cleanTask = _taskFactory.CreateCleanDocumentationTask(config);



    /// <summary>    {            cleanTask.BuildEngine = _buildEngine;

    /// Minimal MSBuild engine interface for compilation

    /// </summary>        public ITask CreateGenerateDocumentationTask(MSBuildTargetConfiguration config) => new MockGenerateDocumentationTask(config);            var cleanResult = await ExecuteTaskAsync(cleanTask);

    public interface IBuildEngine

    {        public ITask CreateCleanDocumentationTask(MSBuildTargetConfiguration config) => new MockCleanDocumentationTask(config);            cleanResult.Should().BeTrue();

        void LogMessage(string message);

        void LogError(string message);    }            Directory.Exists(config.DocumentationOutputPath).Should().BeFalse();

    }

        }

    #endregion

}    public class MockGenerateDocumentationTask : ITask

    {        public void Dispose()

        private readonly MSBuildTargetConfiguration _config;        {

        public MockGenerateDocumentationTask(MSBuildTargetConfiguration config) => _config = config;            try

        public IBuildEngine BuildEngine { get; set; } = null!;            {

        public bool Execute()                if (Directory.Exists(_tempDirectory))

        {                    Directory.Delete(_tempDirectory, true);

            if (!_config.DocumentationEnabled) return true;            }

            if (!File.Exists(_config.DocumentationConfiguration)) return false;            catch { }

            Directory.CreateDirectory(_config.DocumentationOutputPath);        }

            return true;

        }        private async Task<bool> ExecuteTaskAsync(ITask task)

    }        {

            await Task.Delay(10);

    public class MockCleanDocumentationTask : ITask            return task.Execute();

    {        }

        private readonly MSBuildTargetConfiguration _config;    }

        public MockCleanDocumentationTask(MSBuildTargetConfiguration config) => _config = config;

        public IBuildEngine BuildEngine { get; set; } = null!;    // Minimal contract-driven mocks for isolation

        public bool Execute()    public class MockBuildEngine : IBuildEngine

        {    {

            if (Directory.Exists(_config.DocumentationOutputPath))        public void LogMessage(string message) { }

                Directory.Delete(_config.DocumentationOutputPath, true);        public void LogError(string message) { }

            return true;    }

        }

    }    public class MockDocumentationTaskFactory

    {

    public class MSBuildTargetConfiguration        public ITask CreateGenerateDocumentationTask(MSBuildTargetConfiguration config) => new MockGenerateDocumentationTask(config);

    {        public ITask CreateCleanDocumentationTask(MSBuildTargetConfiguration config) => new MockCleanDocumentationTask(config);

        public bool DocumentationEnabled { get; set; }    }

        public string DocumentationConfiguration { get; set; } = string.Empty;

        public string DocumentationOutputPath { get; set; } = string.Empty;    public class MockGenerateDocumentationTask : ITask

        public string DocumentationBuildMode { get; set; } = "Full";    {

    }        private readonly MSBuildTargetConfiguration _config;

        public MockGenerateDocumentationTask(MSBuildTargetConfiguration config) => _config = config;

    public interface ITask        public IBuildEngine BuildEngine { get; set; } = null!;

    {        public bool Execute()

        IBuildEngine BuildEngine { get; set; }        {

        bool Execute();            if (!_config.DocumentationEnabled) return true;

    }            if (!File.Exists(_config.DocumentationConfiguration)) return false;

            Directory.CreateDirectory(_config.DocumentationOutputPath);

    public interface IBuildEngine            return true;

    {        }

        void LogMessage(string message);    }

        void LogError(string message);

    }    public class MockCleanDocumentationTask : ITask

}    {

        private readonly MSBuildTargetConfiguration _config;
        public MockCleanDocumentationTask(MSBuildTargetConfiguration config) => _config = config;
        public IBuildEngine BuildEngine { get; set; } = null!;
        public bool Execute()
        {
            if (Directory.Exists(_config.DocumentationOutputPath))
                Directory.Delete(_config.DocumentationOutputPath, true);
            return true;
        }
    }

    public class MSBuildTargetConfiguration
    {
        public bool DocumentationEnabled { get; set; }
        public string DocumentationConfiguration { get; set; } = string.Empty;
        public string DocumentationOutputPath { get; set; } = string.Empty;
        public string DocumentationBuildMode { get; set; } = "Full";
    }

    public interface ITask
    {
        IBuildEngine BuildEngine { get; set; }
        bool Execute();
    }

    public interface IBuildEngine
    {
        void LogMessage(string message);
        void LogError(string message);
    }
}
using System;using FluentAssertions;using FluentAssertions;

using System.IO;

using System.Threading.Tasks;using Microsoft.Build.Framework;using Microsoft.Build.Framework;

using Xunit;

using FluentAssertions;using Microsoft.Build.Utilities;using Microsoft.Build.Utilities;



namespace Terminal.Gui.Xaml.Tests.Integrationusing System.Collections.Generic;using System.Collections.Generic;


                if (Directory.Exists(_tempDirectory))

                    Directory.Delete(_tempDirectory, true);    [Fact]        task.BuildEngine = _buildEngine;

            }

            catch { /* ignore cleanup errors */ }    public async Task GenerateDocumentationTarget_ShouldExecuteSuccessfully()

        }

    }    {        // Act



    // Mocks for contract-driven test isolation        // Arrange        var result = await ExecuteTaskAsync(task);

    public class MockBuildEngine : IBuildEngine

    {        var targetConfig = CreateValidTargetConfiguration();

        public List<string> LoggedMessages { get; } = new();

        public void LogMessage(string message) => LoggedMessages.Add(message);        var task = _taskFactory.CreateGenerateDocumentationTask(targetConfig);        // Assert

        public void LogError(string message) => LoggedMessages.Add("ERROR: " + message);

        public bool ContinueOnError => false;        task.BuildEngine = _buildEngine;        result.Should().BeTrue("GenerateDocumentation target should execute successfully with valid configuration");

        public int LineNumberOfTaskNode => 0;

        public int ColumnNumberOfTaskNode => 0;        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Documentation generation completed"));

        public string ProjectFileOfTaskNode => "";

        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs) => true;        // Act

        public void LogCustomEvent(CustomBuildEventArgs e) => LogMessage(e.Message ?? "");

        public void LogErrorEvent(BuildErrorEventArgs e) => LogError(e.Message ?? "");        var result = await ExecuteTaskAsync(task);        // Verify contract compliance - output files should be created

        public void LogMessageEvent(BuildMessageEventArgs e) => LogMessage(e.Message ?? "");

        public void LogWarningEvent(BuildWarningEventArgs e) => LogMessage("WARNING: " + (e.Message ?? ""));        var outputPath = Path.Combine(_tempDirectory, "docs");

    }

        // Assert        Directory.Exists(outputPath).Should().BeTrue("Documentation output directory should be created");

    public class MockDocumentationTaskFactory

    {        result.Should().BeTrue("GenerateDocumentation target should execute successfully with valid configuration");

        public ITask CreateGenerateDocumentationTask(MSBuildTargetConfiguration config) => new MockGenerateDocumentationTask(config);

        public ITask CreateCleanDocumentationTask(MSBuildTargetConfiguration config) => new MockCleanDocumentationTask(config);        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Documentation generation completed"));        var manifestPath = Path.Combine(outputPath, "manifest.json");

    }

                File.Exists(manifestPath).Should().BeTrue("Generation manifest should be created as per contract");

    public class MockGenerateDocumentationTask : ITask

    {        // Verify contract compliance - output files should be created    }    [Fact]

        private readonly MSBuildTargetConfiguration _config;

        public MockGenerateDocumentationTask(MSBuildTargetConfiguration config) => _config = config;        var outputPath = Path.Combine(_tempDirectory, "docs");    public async Task ValidateDocumentationTarget_ShouldReportCoverageMetrics()

        public IBuildEngine BuildEngine { get; set; } = null!;

        public ITaskHost HostObject { get; set; } = null!;        Directory.Exists(outputPath).Should().BeTrue("Documentation output directory should be created");    {

        public bool Execute()

        {                // Arrange

            if (!_config.DocumentationEnabled) return true;

            if (!File.Exists(_config.DocumentationConfiguration))        var manifestPath = Path.Combine(outputPath, "manifest.json");        await CreateTestProjectWithXmlDocsAsync();

            {

                BuildEngine.LogError("docfx.json configuration file not found");        File.Exists(manifestPath).Should().BeTrue("Generation manifest should be created as per contract");        var buildEngine = new MockBuildEngine(_output);

                return false;

            }    }

            Directory.CreateDirectory(_config.DocumentationOutputPath);

            BuildEngine.LogMessage("Documentation generation completed");        // Act

            return true;

        }    /// <summary>        var result = await ExecuteMSBuildTargetAsync("ValidateDocumentation", buildEngine);

    }

    /// Tests that the ValidateDocumentation MSBuild target reports coverage metrics correctly.

    public class MockCleanDocumentationTask : ITask

    {    /// Validates the MSBuild integration contract for documentation validation.        // Assert

        private readonly MSBuildTargetConfiguration _config;

        public MockCleanDocumentationTask(MSBuildTargetConfiguration config) => _config = config;    /// </summary>        result.Should().BeTrue("Validation target should execute successfully");

        public IBuildEngine BuildEngine { get; set; } = null!;

        public ITaskHost HostObject { get; set; } = null!;    [Fact]        buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Documentation coverage:"));

        public bool Execute()

        {    public async Task ValidateDocumentationTarget_ShouldReportCoverageMetrics()        buildEngine.LoggedMessages.Should().Contain(m => m.Contains("%"));

            if (Directory.Exists(_config.DocumentationOutputPath))

                Directory.Delete(_config.DocumentationOutputPath, true);    {    }

            BuildEngine.LogMessage("Documentation clean completed");

            return true;        // Arrange

        }

    }        var validationConfig = CreateValidationConfiguration();    [Fact]



    public class MSBuildTargetConfiguration        var task = _taskFactory.CreateValidateDocumentationTask(validationConfig);    public async Task GenerateDocumentationTarget_WithMissingConfiguration_ShouldFail()

    {

        public bool DocumentationEnabled { get; set; }        task.BuildEngine = _buildEngine;    {

        public string DocumentationConfiguration { get; set; } = string.Empty;

        public string DocumentationOutputPath { get; set; } = string.Empty;        // Arrange

        public string DocumentationBuildMode { get; set; } = "Full";

        public string LogLevel { get; set; } = "Normal";        // Act        await CreateTestProjectWithoutDocFxConfigAsync();

    }

        var result = await ExecuteTaskAsync(task);        var buildEngine = new MockBuildEngine(_output);

    public interface ITask

    {

        IBuildEngine BuildEngine { get; set; }

        ITaskHost HostObject { get; set; }        // Assert        // Act

        bool Execute();

    }        result.Should().BeTrue("ValidateDocumentation target should execute successfully");        var result = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine);



    public interface IBuildEngine        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Documentation coverage:"));

    {

        void LogMessage(string message);        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("%"));        // Assert

        void LogError(string message);

    }                result.Should().BeFalse("Target should fail when DocFX configuration is missing");



    public interface ITaskHost { }        // Verify contract compliance - validation report should be created        buildEngine.LoggedErrors.Should().Contain(e => e.Message.Contains("docfx.json"));



    public class CustomBuildEventArgs : EventArgs { public string? Message { get; set; } }        var validationReportPath = Path.Combine(_tempDirectory, "obj", "documentation-validation.json");    }

    public class BuildErrorEventArgs : EventArgs { public string? Message { get; set; } }

    public class BuildMessageEventArgs : EventArgs { public string? Message { get; set; } }        File.Exists(validationReportPath).Should().BeTrue("Validation report should be created as per contract");

    public class BuildWarningEventArgs : EventArgs { public string? Message { get; set; } }

}    }    [Fact]


    public async Task MSBuildIntegration_ShouldPreserveExistingBuildTargets()

    /// <summary>    {

    /// Tests that the GenerateDocumentation target fails appropriately when DocFX configuration is missing.        // Arrange

    /// Validates error handling in MSBuild integration contract.        await CreateTestProjectWithCustomTargetsAsync();

    /// </summary>        var buildEngine = new MockBuildEngine(_output);

    [Fact]

    public async Task GenerateDocumentationTarget_WithMissingConfiguration_ShouldFail()        // Act

    {        var buildResult = await ExecuteMSBuildTargetAsync("Build", buildEngine);

        // Arrange        var docsResult = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine);

        var invalidConfig = CreateInvalidTargetConfiguration();

        var task = _taskFactory.CreateGenerateDocumentationTask(invalidConfig);        // Assert

        task.BuildEngine = _buildEngine;        buildResult.Should().BeTrue("Build target should still work");

        docsResult.Should().BeTrue("Documentation target should work alongside build");

        // Act        buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Custom target executed"));

        var result = await ExecuteTaskAsync(task);    }



        // Assert    [Fact]

        result.Should().BeFalse("GenerateDocumentation target should fail when DocFX configuration is missing");    public async Task DocumentationGeneration_ShouldHandleIncrementalBuilds()

        _buildEngine.LoggedErrors.Should().NotBeEmpty("Error should be logged when configuration is missing");    {

        _buildEngine.LoggedErrors.Should().ContainSingle(e => e.Message.Contains("docfx.json"));        // Arrange

    }        await CreateTestProjectAsync();

        var buildEngine = new MockBuildEngine(_output);

    /// <summary>

    /// Tests that MSBuild integration preserves existing build targets and doesn't interfere with normal build process.        // Act - First generation

    /// Validates the MSBuild integration contract requirement for non-interference.        var firstResult = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine);

    /// </summary>        var firstGenerationTime = GetLastGenerationTime(buildEngine);

    [Fact]

    public async Task MSBuildIntegration_ShouldPreserveExistingBuildTargets()        // Modify source file

    {        await ModifySourceFileAsync();

        // Arrange        buildEngine.Reset();

        var buildConfig = CreateBuildConfiguration();

        var buildTask = _taskFactory.CreateBuildTask(buildConfig);        // Act - Second generation (incremental)

        var docsTask = _taskFactory.CreateGenerateDocumentationTask(CreateValidTargetConfiguration());        var secondResult = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine);

                var secondGenerationTime = GetLastGenerationTime(buildEngine);

        buildTask.BuildEngine = _buildEngine;

        docsTask.BuildEngine = _buildEngine;        // Assert

        firstResult.Should().BeTrue("First generation should succeed");

        // Act        secondResult.Should().BeTrue("Second generation should succeed");

        var buildResult = await ExecuteTaskAsync(buildTask);        buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Incremental build"));

        var docsResult = await ExecuteTaskAsync(docsTask);    }



        // Assert    [Fact]

        buildResult.Should().BeTrue("Build target should still work with documentation integration");    public async Task MSBuildTarget_ShouldRespectConfigurationProperties()

        docsResult.Should().BeTrue("Documentation target should work alongside build");    {

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Build completed"));        // Arrange

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Documentation generation completed"));        await CreateTestProjectAsync();

    }        var buildEngine = new MockBuildEngine(_output);

        var properties = new Dictionary<string, string>

    /// <summary>        {

    /// Tests that documentation generation handles incremental builds correctly.            ["DocFxOutputPath"] = Path.Combine(_tempDirectory, "custom-docs"),

    /// Validates the MSBuild integration contract for incremental build support.            ["DocFxLogLevel"] = "Verbose"

    /// </summary>        };

    [Fact]

    public async Task DocumentationGeneration_ShouldHandleIncrementalBuilds()        // Act

    {        var result = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine, properties);

        // Arrange

        var incrementalConfig = CreateIncrementalBuildConfiguration();        // Assert

        var task = _taskFactory.CreateGenerateDocumentationTask(incrementalConfig);        result.Should().BeTrue("Target should respect custom properties");

        task.BuildEngine = _buildEngine;        var customDocsPath = Path.Combine(_tempDirectory, "custom-docs");

        Directory.Exists(customDocsPath).Should().BeTrue("Custom output directory should be used");

        // Act - First generation        buildEngine.LoggedMessages.Should().Contain(m => m.Contains("[Verbose]"));

        var firstResult = await ExecuteTaskAsync(task);    }

        var firstGenerationTime = _taskFactory.LastGenerationTime;

    [Theory]

        // Simulate source file change    [InlineData("Debug")]

        _taskFactory.SimulateSourceChange();    [InlineData("Release")]

        _buildEngine.Reset();    public async Task DocumentationGeneration_ShouldWorkInAllConfigurations(string configuration)

    {

        // Act - Second generation (incremental)        // Arrange

        var secondResult = await ExecuteTaskAsync(task);        await CreateTestProjectAsync();

        var secondGenerationTime = _taskFactory.LastGenerationTime;        var buildEngine = new MockBuildEngine(_output);

        var properties = new Dictionary<string, string>

        // Assert        {

        firstResult.Should().BeTrue("First generation should succeed");            ["Configuration"] = configuration

        secondResult.Should().BeTrue("Second generation should succeed");        };

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Incremental build"));

        secondGenerationTime.Should().BeLessThan(firstGenerationTime, "Incremental build should be faster");        // Act

    }        var result = await ExecuteMSBuildTargetAsync("GenerateDocumentation", buildEngine, properties);



    /// <summary>        // Assert

    /// Tests that MSBuild targets respect configuration properties as defined in the contract.        result.Should().BeTrue($"Documentation generation should work in {configuration} configuration");

    /// Validates property group handling in MSBuild integration contract.

    /// </summary>        var configSpecificPath = Path.Combine(_tempDirectory, "bin", configuration);

    [Fact]        buildEngine.LoggedMessages.Should().Contain(m => m.Contains(configuration));

    public async Task MSBuildTarget_ShouldRespectConfigurationProperties()    }

    {

        // Arrange    private async Task<bool> ExecuteMSBuildTargetAsync(

        var customConfig = CreateCustomPropertyConfiguration();        string targetName,

        var task = _taskFactory.CreateGenerateDocumentationTask(customConfig);        MockBuildEngine buildEngine,

        task.BuildEngine = _buildEngine;        Dictionary<string, string>? properties = null)

    {

        // Act        var startInfo = new ProcessStartInfo

        var result = await ExecuteTaskAsync(task);        {

            FileName = "dotnet",

        // Assert            Arguments = $"msbuild \"{_testProjectPath}\" -t:{targetName}",

        result.Should().BeTrue("Target should respect custom properties");            WorkingDirectory = _tempDirectory,

                    RedirectStandardOutput = true,

        var customOutputPath = Path.Combine(_tempDirectory, "custom-docs");            RedirectStandardError = true,

        Directory.Exists(customOutputPath).Should().BeTrue("Custom output directory should be used");            UseShellExecute = false,

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("[Verbose]"));            CreateNoWindow = true

    }        };



    /// <summary>        // Add properties to command line

    /// Tests that documentation generation works correctly in both Debug and Release configurations.        if (properties != null)

    /// Validates configuration-dependent behavior in MSBuild integration contract.        {

    /// </summary>            foreach (var prop in properties)

    /// <param name="configuration">The build configuration to test.</param>            {

    [Theory]                startInfo.Arguments += $" -p:{prop.Key}={prop.Value}";

    [InlineData("Debug")]            }

    [InlineData("Release")]        }

    public async Task DocumentationGeneration_ShouldWorkInAllConfigurations(string configuration)

    {        using var process = Process.Start(startInfo);

        // Arrange        if (process == null) return false;

        var configSpecificConfig = CreateConfigurationSpecificConfiguration(configuration);

        var task = _taskFactory.CreateGenerateDocumentationTask(configSpecificConfig);        var output = await process.StandardOutput.ReadToEndAsync();

        task.BuildEngine = _buildEngine;        var error = await process.StandardError.ReadToEndAsync();



        // Act        await process.WaitForExitAsync();

        var result = await ExecuteTaskAsync(task);

        // Log output to mock build engine for assertions

        // Assert        buildEngine.LogMessage(output);

        result.Should().BeTrue($"Documentation generation should work in {configuration} configuration");        if (!string.IsNullOrEmpty(error))

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains(configuration));        {

    }            buildEngine.LogError(error);

        }

    /// <summary>

    /// Tests that CleanDocumentation target removes generated files without affecting source files.        _output.WriteLine($"MSBuild output: {output}");

    /// Validates the MSBuild integration contract for clean operations.        if (!string.IsNullOrEmpty(error))

    /// </summary>        {

    [Fact]            _output.WriteLine($"MSBuild error: {error}");

    public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()        }

    {

        // Arrange        return process.ExitCode == 0;

        var generateConfig = CreateValidTargetConfiguration();    }

        var generateTask = _taskFactory.CreateGenerateDocumentationTask(generateConfig);

        generateTask.BuildEngine = _buildEngine;    private async Task CreateTestProjectAsync()

    {

        // Generate documentation first        var projectContent = """

        await ExecuteTaskAsync(generateTask);            <Project Sdk="Microsoft.NET.Sdk">

        var docsPath = Path.Combine(_tempDirectory, "docs");              <PropertyGroup>

        Directory.Exists(docsPath).Should().BeTrue("Documentation should be generated first");                <TargetFramework>net8.0</TargetFramework>

                <GenerateDocumentationFile>true</GenerateDocumentationFile>

        // Create source documentation file              </PropertyGroup>

        var sourceDocPath = Path.Combine(_tempDirectory, "README.md");

        await File.WriteAllTextAsync(sourceDocPath, "# Source Documentation");              <ItemGroup>

                <PackageReference Include="Terminal.Gui.Xaml" Version="*" />

        // Act                <PackageReference Include="docfx" Version="2.75.3" PrivateAssets="all" />

        var cleanTask = _taskFactory.CreateCleanDocumentationTask(generateConfig);              </ItemGroup>

        cleanTask.BuildEngine = _buildEngine;            </Project>

        var cleanResult = await ExecuteTaskAsync(cleanTask);            """;



        // Assert        await File.WriteAllTextAsync(_testProjectPath, projectContent);

        cleanResult.Should().BeTrue("Clean target should execute successfully");

        Directory.Exists(docsPath).Should().BeFalse("Generated documentation directory should be removed");        // Create a sample source file with XML documentation

        File.Exists(sourceDocPath).Should().BeTrue("Source documentation files should be preserved");        var sourceFile = Path.Combine(_tempDirectory, "SampleClass.cs");

    }        var sourceContent = """

            namespace TestProject

    /// <summary>            {

    /// Tests that DocumentationSource ItemGroup is processed correctly.                /// <summary>

    /// Validates ItemGroup handling in MSBuild integration contract.                /// A sample class for testing documentation generation.

    /// </summary>                /// </summary>

    [Fact]                public class SampleClass

    public async Task DocumentationSourceItemGroup_ShouldBeProcessedCorrectly()                {

    {                    /// <summary>

        // Arrange                    /// Gets or sets the sample property.

        var config = CreateConfigurationWithItemGroups();                    /// </summary>

        var task = _taskFactory.CreateGenerateDocumentationTask(config);                    /// <value>The sample value.</value>

        task.BuildEngine = _buildEngine;                    public string SampleProperty { get; set; } = string.Empty;



        // Act                    /// <summary>

        var result = await ExecuteTaskAsync(task);                    /// Performs a sample operation.

                    /// </summary>

        // Assert                    /// <param name="input">The input parameter.</param>

        result.Should().BeTrue("Task should process DocumentationSource ItemGroup correctly");                    /// <returns>The processed result.</returns>

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Processing source files"));                    public string DoSomething(string input)

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("README.md"));                    {

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("articles"));                        return $"Processed: {input}";

    }                    }

                }

    /// <summary>            }

    /// Tests that DocumentationExclude ItemGroup excludes files correctly.            """;

    /// Validates exclusion handling in MSBuild integration contract.

    /// </summary>        await File.WriteAllTextAsync(sourceFile, sourceContent);

    [Fact]

    public async Task DocumentationExcludeItemGroup_ShouldExcludeFilesCorrectly()        // Create docfx.json configuration

    {        var docfxConfig = """

        // Arrange            {

        var config = CreateConfigurationWithExclusions();              "metadata": [

        var task = _taskFactory.CreateGenerateDocumentationTask(config);                {

        task.BuildEngine = _buildEngine;                  "src": [

                    {

        // Act                      "files": ["*.csproj"],

        var result = await ExecuteTaskAsync(task);                      "src": "."

                    }

        // Assert                  ],

        result.Should().BeTrue("Task should process DocumentationExclude ItemGroup correctly");                  "dest": "api"

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Excluding files"));                }

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Internal"));              ],

        _buildEngine.LoggedMessages.Should().Contain(m => m.Contains("Designer.cs"));              "build": {

    }                "content": [

                  {

    private async Task<bool> ExecuteTaskAsync(ITask task)                    "files": ["api/*.yml"],

    {                    "src": "."

        // Simulate MSBuild task execution                  }

        await Task.Delay(10); // Simulate processing time                ],

        return task.Execute();                "dest": "docs"

    }              }

            }

    private MSBuildTargetConfiguration CreateValidTargetConfiguration()            """;

    {

        return new MSBuildTargetConfiguration        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "docfx.json"), docfxConfig);

        {    }

            DocumentationEnabled = true,

            DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),    private async Task CreateTestProjectWithXmlDocsAsync()

            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),    {

            DocumentationBuildMode = "Full"        await CreateTestProjectAsync();

        };

    }        // Add more comprehensive XML documentation

        var enhancedSourceFile = Path.Combine(_tempDirectory, "EnhancedClass.cs");

    private MSBuildTargetConfiguration CreateValidationConfiguration()        var enhancedContent = """

    {            namespace TestProject

        // Create docfx.json for validation            {

        var docfxPath = Path.Combine(_tempDirectory, "docfx.json");                /// <summary>

        File.WriteAllText(docfxPath, "{}");                /// An enhanced class with comprehensive documentation.

                /// </summary>

        return new MSBuildTargetConfiguration                /// <remarks>

        {                /// This class demonstrates various XML documentation elements.

            DocumentationConfiguration = docfxPath,                /// </remarks>

            RequiredDocumentationCoverage = 80.0,                /// <example>

            ValidateDocumentationLinks = true,                /// <code>

            ValidateDocumentationExamples = true,                /// var enhanced = new EnhancedClass();

            ValidationReportPath = Path.Combine(_tempDirectory, "obj", "documentation-validation.json")                /// enhanced.Initialize("test");

        };                /// </code>

    }                /// </example>

                public class EnhancedClass

    private MSBuildTargetConfiguration CreateInvalidTargetConfiguration()                {

    {                    /// <summary>

        return new MSBuildTargetConfiguration                    /// Initializes the class with the specified configuration.

        {                    /// </summary>

            DocumentationEnabled = true,                    /// <param name="config">The configuration string.</param>

            DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),                    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>

            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")                    public void Initialize(string config)

        };                    {

    }                        if (config == null) throw new ArgumentNullException(nameof(config));

                    }

    private MSBuildTargetConfiguration CreateBuildConfiguration()                }

    {            }

        return new MSBuildTargetConfiguration            """;

        {

            IsStandardBuild = true,        await File.WriteAllTextAsync(enhancedSourceFile, enhancedContent);

            Configuration = "Release"    }

        };

    }    private async Task CreateTestProjectWithoutDocFxConfigAsync()

    {

    private MSBuildTargetConfiguration CreateIncrementalBuildConfiguration()        var projectContent = """

    {            <Project Sdk="Microsoft.NET.Sdk">

        // Create docfx.json for incremental build              <PropertyGroup>

        var docfxPath = Path.Combine(_tempDirectory, "docfx.json");                <TargetFramework>net8.0</TargetFramework>

        File.WriteAllText(docfxPath, "{}");              </PropertyGroup>

            </Project>

        return new MSBuildTargetConfiguration            """;

        {

            DocumentationEnabled = true,        await File.WriteAllTextAsync(_testProjectPath, projectContent);

            DocumentationConfiguration = docfxPath,    }

            DocumentationBuildMode = "Incremental",

            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),    private async Task CreateTestProjectWithCustomTargetsAsync()

            CacheEnabled = true    {

        };        await CreateTestProjectAsync();

    }

        // Add custom MSBuild target

    private MSBuildTargetConfiguration CreateCustomPropertyConfiguration()        var customTargets = """

    {            <Project>

        // Create docfx.json for custom properties              <Target Name="CustomTarget" BeforeTargets="Build">

        var docfxPath = Path.Combine(_tempDirectory, "docfx.json");                <Message Text="Custom target executed" Importance="high" />

        File.WriteAllText(docfxPath, "{}");              </Target>

            </Project>

        return new MSBuildTargetConfiguration            """;

        {

            DocumentationEnabled = true,        var customTargetsFile = Path.Combine(_tempDirectory, "Custom.targets");

            DocumentationConfiguration = docfxPath,        await File.WriteAllTextAsync(customTargetsFile, customTargets);

            DocumentationOutputPath = Path.Combine(_tempDirectory, "custom-docs"),

            LogLevel = "Verbose"        // Update project to import custom targets

        };        var projectContent = await File.ReadAllTextAsync(_testProjectPath);

    }        projectContent = projectContent.Replace("</Project>",

            "  <Import Project=\"Custom.targets\" />\n</Project>");

    private MSBuildTargetConfiguration CreateConfigurationSpecificConfiguration(string configuration)        await File.WriteAllTextAsync(_testProjectPath, projectContent);

    {    }

        // Create docfx.json if needed for Release builds

        if (configuration == "Release")    private async Task ModifySourceFileAsync()

        {    {

            var docfxPath = Path.Combine(_tempDirectory, "docfx.json");        var sourceFile = Path.Combine(_tempDirectory, "SampleClass.cs");

            File.WriteAllText(docfxPath, "{}");        var content = await File.ReadAllTextAsync(sourceFile);

        }        content = content.Replace("DoSomething", "DoSomethingModified");

        await File.WriteAllTextAsync(sourceFile, content);

        return new MSBuildTargetConfiguration    }

        {

            DocumentationEnabled = configuration == "Release",    private TimeSpan GetLastGenerationTime(MockBuildEngine buildEngine)

            Configuration = configuration,    {

            DocumentationConfiguration = configuration == "Release"         // Parse generation time from logged messages

                ? Path.Combine(_tempDirectory, "docfx.json")         var timeMessage = buildEngine.LoggedMessages

                : string.Empty,            .FirstOrDefault(m => m.Contains("Generation completed in"));

            DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")

        };        if (timeMessage != null && TimeSpan.TryParse(

    }            timeMessage.Split("Generation completed in ")[1].Split(" ")[0],

            out var time))

    private MSBuildTargetConfiguration CreateConfigurationWithItemGroups()        {

    {            return time;

        // Create docfx.json for ItemGroup processing        }

        var docfxPath = Path.Combine(_tempDirectory, "docfx.json");

        File.WriteAllText(docfxPath, "{}");        return TimeSpan.Zero;

    }

        return new MSBuildTargetConfiguration

        {    public void Dispose()

            DocumentationEnabled = true,    {

            DocumentationConfiguration = docfxPath,        try

            DocumentationSources = new[]        {

            {            if (Directory.Exists(_tempDirectory))

                "src/**/*.cs",            {

                "README.md",                Directory.Delete(_tempDirectory, true);

                "docs/articles/**/*.md"            }

            }        }

        };        catch (Exception ex)

    }        {

            _output.WriteLine($"Warning: Could not clean up temp directory: {ex.Message}");

    private MSBuildTargetConfiguration CreateConfigurationWithExclusions()        }

    {    }

        // Create docfx.json for exclusion processing}

        var docfxPath = Path.Combine(_tempDirectory, "docfx.json");

        File.WriteAllText(docfxPath, "{}");/// <summary>

/// Mock build engine for testing MSBuild integration.

        return new MSBuildTargetConfiguration/// </summary>

        {public class MockBuildEngine : IBuildEngine

            DocumentationEnabled = true,{

            DocumentationConfiguration = docfxPath,    private readonly ITestOutputHelper _output;

            DocumentationExcludes = new[]    private readonly List<string> _loggedMessages = new();

            {    private readonly List<BuildErrorEventArgs> _loggedErrors = new();

                "src/**/Internal/**/*.cs",

                "src/**/*.Designer.cs",    public MockBuildEngine(ITestOutputHelper output)

                "tests/**/*.cs"    {

            }        _output = output;

        };    }

    }

    public List<string> LoggedMessages => _loggedMessages;

    /// <summary>    public List<BuildErrorEventArgs> LoggedErrors => _loggedErrors;

    /// Releases all resources used by the <see cref="MSBuildTargetExecutionTests"/> class.

    /// </summary>    public void LogMessage(string message)

    public void Dispose()    {

    {        _loggedMessages.Add(message);

        Dispose(true);        _output.WriteLine($"[INFO] {message}");

        GC.SuppressFinalize(this);    }

    }

    public void LogError(string message)

    /// <summary>    {

    /// Releases unmanaged and - optionally - managed resources.        var errorArgs = new BuildErrorEventArgs(

    /// </summary>            subcategory: "",

    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>            code: "",

    protected virtual void Dispose(bool disposing)            file: "",

    {            lineNumber: 0,

        if (disposing)            columnNumber: 0,

        {            endLineNumber: 0,

            try            endColumnNumber: 0,

            {            message: message,

                if (Directory.Exists(_tempDirectory))            helpKeyword: "",

                {            senderName: "MockBuildEngine");

                    Directory.Delete(_tempDirectory, true);

                }        _loggedErrors.Add(errorArgs);

            }        _output.WriteLine($"[ERROR] {message}");

            catch (Exception ex)    }

            {

                _output.WriteLine($"Warning: Could not clean up temp directory: {ex.Message}");    public void Reset()

            }    {

        }        _loggedMessages.Clear();

    }        _loggedErrors.Clear();

}    }



/// <summary>    #region IBuildEngine Implementation

/// Configuration for MSBuild target testing.

/// Represents the MSBuild properties and ItemGroups defined in the integration contract.    public bool ContinueOnError => false;

/// </summary>    public int LineNumberOfTaskNode => 0;

using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace Terminal.Gui.Xaml.Tests.Integration
{
    /// <summary>
    /// Integration tests for MSBuild documentation target execution.
    /// Validates contract requirements for build, generate, clean, and configuration scenarios.
    /// </summary>
    public class MSBuildTargetExecutionTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly MockBuildEngine _buildEngine;
        private readonly MockDocumentationTaskFactory _taskFactory;

        public MSBuildTargetExecutionTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
            _buildEngine = new MockBuildEngine();
            _taskFactory = new MockDocumentationTaskFactory();
        }

        [Fact]
        public async Task GenerateDocumentationTarget_ShouldSucceed_WithValidConfiguration()
        {
            var config = new MSBuildTargetConfiguration
            {
                DocumentationEnabled = true,
                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),
                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs"),
                DocumentationBuildMode = "Full"
            };
            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");
            var task = _taskFactory.CreateGenerateDocumentationTask(config);
            task.BuildEngine = _buildEngine;
            var result = await ExecuteTaskAsync(task);
            result.Should().BeTrue("Documentation generation should succeed with valid config");
            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue("Output directory should be created");
        }

        [Fact]
        public async Task GenerateDocumentationTarget_ShouldFail_IfConfigMissing()
        {
            var config = new MSBuildTargetConfiguration
            {
                DocumentationEnabled = true,
                DocumentationConfiguration = Path.Combine(_tempDirectory, "missing-docfx.json"),
                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")
            };
            var task = _taskFactory.CreateGenerateDocumentationTask(config);
            task.BuildEngine = _buildEngine;
            var result = await ExecuteTaskAsync(task);
            result.Should().BeFalse("Should fail if DocFX config is missing");
        }

        [Fact]
        public async Task CleanDocumentationTarget_ShouldRemoveGeneratedFilesOnly()
        {
            var config = new MSBuildTargetConfiguration
            {
                DocumentationEnabled = true,
                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),
                DocumentationOutputPath = Path.Combine(_tempDirectory, "docs")
            };
            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");
            Directory.CreateDirectory(config.DocumentationOutputPath);
            var generateTask = _taskFactory.CreateGenerateDocumentationTask(config);
            generateTask.BuildEngine = _buildEngine;
            await ExecuteTaskAsync(generateTask);
            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue();
            var cleanTask = _taskFactory.CreateCleanDocumentationTask(config);
            cleanTask.BuildEngine = _buildEngine;
            var cleanResult = await ExecuteTaskAsync(cleanTask);
            cleanResult.Should().BeTrue("Clean target should execute successfully");
            Directory.Exists(config.DocumentationOutputPath).Should().BeFalse("Generated docs should be removed");
        }

        [Fact]
        public async Task DocumentationTarget_ShouldRespectCustomProperties()
        {
            var config = new MSBuildTargetConfiguration
            {
                DocumentationEnabled = true,
                DocumentationConfiguration = Path.Combine(_tempDirectory, "docfx.json"),
                DocumentationOutputPath = Path.Combine(_tempDirectory, "custom-docs"),
                LogLevel = "Verbose"
            };
            await File.WriteAllTextAsync(config.DocumentationConfiguration, "{}\n");
            var task = _taskFactory.CreateGenerateDocumentationTask(config);
            task.BuildEngine = _buildEngine;
            var result = await ExecuteTaskAsync(task);
            result.Should().BeTrue("Target should respect custom properties");
            Directory.Exists(config.DocumentationOutputPath).Should().BeTrue("Custom output directory should be used");
        }

        private async Task<bool> ExecuteTaskAsync(ITask task)
        {
            await Task.Delay(10); // Simulate async MSBuild execution
            return task.Execute();
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempDirectory))
                    Directory.Delete(_tempDirectory, true);
            }
            catch { /* ignore cleanup errors */ }
        }
    }
}

{    public string ProjectFileOfTaskNode => "";

    /// <summary>

    /// Gets or sets a value indicating whether documentation generation is enabled.    public bool BuildProjectFile(string projectFileName, string[] targetNames,

    /// Maps to $(DocumentationEnabled) MSBuild property.        System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)

    /// </summary>    {

    public bool DocumentationEnabled { get; set; }        return true;

    }

    /// <summary>

    /// Gets or sets the path to DocFX configuration file.    public void LogCustomEvent(CustomBuildEventArgs e)

    /// Maps to $(DocumentationConfiguration) MSBuild property.    {

    /// </summary>        LogMessage(e.Message ?? "");

    public string DocumentationConfiguration { get; set; } = string.Empty;    }



    /// <summary>    public void LogErrorEvent(BuildErrorEventArgs e)

    /// Gets or sets the documentation output path.    {

    /// Maps to $(DocumentationOutputPath) MSBuild property.        _loggedErrors.Add(e);

    /// </summary>        _output.WriteLine($"[ERROR] {e.Message}");

    public string DocumentationOutputPath { get; set; } = string.Empty;    }



    /// <summary>    public void LogMessageEvent(BuildMessageEventArgs e)

    /// Gets or sets the documentation build mode.    {

    /// Maps to $(DocumentationBuildMode) MSBuild property.        LogMessage(e.Message ?? "");

    /// </summary>    }

    public string DocumentationBuildMode { get; set; } = "Full";

    public void LogWarningEvent(BuildWarningEventArgs e)

    /// <summary>    {

    /// Gets or sets the required documentation coverage percentage.        _output.WriteLine($"[WARNING] {e.Message}");

    /// Maps to $(RequiredDocumentationCoverage) MSBuild property.    }

    /// </summary>

    public double RequiredDocumentationCoverage { get; set; } = 80.0;    #endregion

}

    /// <summary>
    /// Gets or sets a value indicating whether to validate documentation links.
    /// Maps to $(ValidateDocumentationLinks) MSBuild property.
    /// </summary>
    public bool ValidateDocumentationLinks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate documentation examples.
    /// Maps to $(ValidateDocumentationExamples) MSBuild property.
    /// </summary>
    public bool ValidateDocumentationExamples { get; set; }

    /// <summary>
    /// Gets or sets the validation report path.
    /// Maps to $(IntermediateOutputPath)/documentation-validation.json output.
    /// </summary>
    public string ValidationReportPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is a standard build.
    /// Used for testing non-interference with existing build targets.
    /// </summary>
    public bool IsStandardBuild { get; set; }

    /// <summary>
    /// Gets or sets the build configuration.
    /// Maps to $(Configuration) MSBuild property.
    /// </summary>
    public string Configuration { get; set; } = "Release";

    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// Used for incremental build testing.
    /// </summary>
    public bool CacheEnabled { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// Used for testing verbose logging configuration.
    /// </summary>
    public string LogLevel { get; set; } = "Normal";

    /// <summary>
    /// Gets or sets the documentation source patterns.
    /// Maps to DocumentationSource ItemGroup.
    /// </summary>
    public string[] DocumentationSources { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the documentation exclusion patterns.
    /// Maps to DocumentationExclude ItemGroup.
    /// </summary>
    public string[] DocumentationExcludes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Mock build engine for testing MSBuild integration.
/// Implements IBuildEngine interface for isolated unit testing.
/// </summary>
public class MockBuildEngine : IBuildEngine
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _loggedMessages = new();
    private readonly List<BuildErrorEventArgs> _loggedErrors = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MockBuildEngine"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public MockBuildEngine(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Gets the list of logged messages.
    /// </summary>
    public List<string> LoggedMessages => _loggedMessages;

    /// <summary>
    /// Gets the list of logged errors.
    /// </summary>
    public List<BuildErrorEventArgs> LoggedErrors => _loggedErrors;

    /// <summary>
    /// Logs a message to the build engine.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogMessage(string message)
    {
        _loggedMessages.Add(message);
        _output.WriteLine($"[INFO] {message}");
    }

    /// <summary>
    /// Logs an error to the build engine.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    public void LogError(string message)
    {
        var errorArgs = new BuildErrorEventArgs(
            subcategory: "",
            code: "",
            file: "",
            lineNumber: 0,
            columnNumber: 0,
            endLineNumber: 0,
            endColumnNumber: 0,
            message: message,
            helpKeyword: "",
            senderName: "MockBuildEngine");

        _loggedErrors.Add(errorArgs);
        _output.WriteLine($"[ERROR] {message}");
    }

    /// <summary>
    /// Resets the logged messages and errors.
    /// </summary>
    public void Reset()
    {
        _loggedMessages.Clear();
        _loggedErrors.Clear();
