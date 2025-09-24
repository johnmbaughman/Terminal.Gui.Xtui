using FluentAssertions;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using HtmlAgilityPack;
using System.Diagnostics;

namespace Terminal.Gui.Xaml.Tests.Integration;

/// <summary>
/// Integration tests for documentation site navigation and search functionality.
/// Validates that generated documentation sites are properly structured and searchable.
/// </summary>
public class DocumentationSiteTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _tempDirectory;
    private readonly string _siteDirectory;
    private HttpClient? _httpClient;
    private Process? _serverProcess;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentationSiteTests"/> class.
    /// </summary>
    /// <param name="output">The test output helper.</param>
    public DocumentationSiteTests(ITestOutputHelper output)
    {
        _output = output;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "TerminalGuiXaml_SiteTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _siteDirectory = Path.Combine(_tempDirectory, "docs");
    }

    /// <summary>
    /// Tests that the generated documentation site has a valid navigation structure.
    /// </summary>
    [Fact]
    public async Task GeneratedSite_ShouldHaveValidNavigationStructure()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Act
        var indexPath = Path.Combine(_siteDirectory, "index.html");
        var indexContent = await File.ReadAllTextAsync(indexPath);
        var doc = new HtmlDocument();
        doc.LoadHtml(indexContent);

        // Assert
        doc.Should().NotBeNull("Index page should be generated");

        // Check for navigation menu
        var navMenu = doc.DocumentNode.SelectSingleNode("//nav") ??
                     doc.DocumentNode.SelectSingleNode("//*[@class='navbar']") ??
                     doc.DocumentNode.SelectSingleNode("//*[@role='navigation']");
        navMenu.Should().NotBeNull("Navigation menu should be present");

        // Check for API documentation links
        var apiLinks = doc.DocumentNode.SelectNodes("//a[contains(@href, 'api/')]");
        apiLinks.Should().NotBeNullOrEmpty("API documentation links should be present");

        // Check for search functionality
        var searchElement = doc.DocumentNode.SelectSingleNode("//input[@type='search']") ??
                          doc.DocumentNode.SelectSingleNode("//*[@id='search']") ??
                          doc.DocumentNode.SelectSingleNode("//*[@class*='search']");
        searchElement.Should().NotBeNull("Search functionality should be available");
    }

    /// <summary>
    /// Tests that API documentation pages are accessible and properly structured.
    /// </summary>
    [Fact]
    public async Task ApiDocumentationPages_ShouldBeAccessible()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Act & Assert
        var apiDirectory = Path.Combine(_siteDirectory, "api");
        Directory.Exists(apiDirectory).Should().BeTrue("API documentation directory should exist");

        var apiFiles = Directory.GetFiles(apiDirectory, "*.html", SearchOption.AllDirectories);
        apiFiles.Should().NotBeEmpty("API documentation HTML files should be generated");

        foreach (var apiFile in apiFiles.Take(5)) // Test first 5 files to avoid excessive testing
        {
            var content = await File.ReadAllTextAsync(apiFile);
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            // Check for proper HTML structure
            doc.DocumentNode.SelectSingleNode("//title").Should().NotBeNull($"API page {Path.GetFileName(apiFile)} should have a title");

            // Check for breadcrumb navigation
            var breadcrumb = doc.DocumentNode.SelectSingleNode("//*[@class*='breadcrumb']") ??
                           doc.DocumentNode.SelectSingleNode("//nav[@aria-label='breadcrumb']");
            breadcrumb.Should().NotBeNull($"API page {Path.GetFileName(apiFile)} should have breadcrumb navigation");
        }
    }

    /// <summary>
    /// Tests that the search functionality returns relevant results for queries.
    /// </summary>
    [Fact]
    public async Task SearchFunctionality_ShouldReturnRelevantResults()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();
        await StartLocalServerAsync();

        // Act
        var searchResults = await PerformSearchAsync("SampleClass");

        // Assert
        searchResults.Should().NotBeEmpty("Search should return results for 'SampleClass'");
        searchResults.Should().Contain(r => r.Title.Contains("SampleClass") || r.Content.Contains("SampleClass"));

        // Test search for methods
        var methodResults = await PerformSearchAsync("DoSomething");
        methodResults.Should().NotBeEmpty("Search should return results for method names");
    }

    /// <summary>
    /// Tests that cross-references in documentation link correctly to their targets.
    /// </summary>
    [Fact]
    public async Task CrossReferences_ShouldLinkCorrectly()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Act
        var apiFiles = Directory.GetFiles(Path.Combine(_siteDirectory, "api"), "*.html", SearchOption.AllDirectories);
        var testFile = apiFiles.FirstOrDefault(f => Path.GetFileName(f).Contains("SampleClass"));
        testFile.Should().NotBeNull("SampleClass documentation should exist");

        var content = await File.ReadAllTextAsync(testFile!);
        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Assert
        var links = doc.DocumentNode.SelectNodes("//a[@href]");
        links.Should().NotBeNullOrEmpty("Documentation should contain cross-reference links");

        // Check that internal links point to valid files
        var internalLinks = links.Where(l =>
            l.GetAttributeValue("href", "").StartsWith("../") ||
            !l.GetAttributeValue("href", "").StartsWith("http"))
            .Take(10); // Test first 10 to avoid excessive validation

        foreach (var link in internalLinks)
        {
            var href = link.GetAttributeValue("href", "");
            if (!string.IsNullOrEmpty(href) && !href.StartsWith("#"))
            {
                var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testFile)!, href));
                if (fullPath.StartsWith(_siteDirectory))
                {
                    File.Exists(fullPath).Should().BeTrue($"Cross-reference link should point to existing file: {href}");
                }
            }
        }
    }

    /// <summary>
    /// Tests that the documentation site includes responsive design elements.
    /// </summary>
    [Fact]
    public async Task DocumentationSite_ShouldBeResponsive()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Act
        var indexPath = Path.Combine(_siteDirectory, "index.html");
        var indexContent = await File.ReadAllTextAsync(indexPath);
        var doc = new HtmlDocument();
        doc.LoadHtml(indexContent);

        // Assert
        // Check for responsive viewport meta tag
        var viewportMeta = doc.DocumentNode.SelectSingleNode("//meta[@name='viewport']");
        viewportMeta.Should().NotBeNull("Site should have responsive viewport meta tag");

        // Check for responsive CSS classes or media queries
        var cssLinks = doc.DocumentNode.SelectNodes("//link[@rel='stylesheet']");
        bool hasResponsiveDesign = false;

        if (cssLinks != null)
        {
            foreach (var cssLink in cssLinks)
            {
                var href = cssLink.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                {
                    var cssPath = Path.Combine(_siteDirectory, href.TrimStart('/'));
                    if (File.Exists(cssPath))
                    {
                        var cssContent = await File.ReadAllTextAsync(cssPath);
                        if (cssContent.Contains("@media") || cssContent.Contains("responsive"))
                        {
                            hasResponsiveDesign = true;
                            break;
                        }
                    }
                }
            }
        }

        hasResponsiveDesign.Should().BeTrue("Site should include responsive design elements");
    }

    /// <summary>
    /// Tests that the site structure follows DocFX conventions and includes expected files and directories.
    /// </summary>
    [Fact]
    public async Task SiteStructure_ShouldFollowDocFxConventions()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Assert
        var expectedFiles = new[]
        {
            "index.html",
            "toc.html",
            "manifest.json"
        };

        foreach (var expectedFile in expectedFiles)
        {
            var filePath = Path.Combine(_siteDirectory, expectedFile);
            File.Exists(filePath).Should().BeTrue($"Expected DocFX file should exist: {expectedFile}");
        }

        var expectedDirectories = new[]
        {
            "api",
            "_site",
            "styles"
        };

        foreach (var expectedDir in expectedDirectories.Where(d => d != "_site")) // _site might not exist in all configurations
        {
            var dirPath = Path.Combine(_siteDirectory, expectedDir);
            Directory.Exists(dirPath).Should().BeTrue($"Expected DocFX directory should exist: {expectedDir}");
        }
    }

    /// <summary>
    /// Tests that a search index is generated and contains valid JSON data.
    /// </summary>
    [Fact]
    public async Task SearchIndex_ShouldBeGeneratedAndValid()
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();

        // Act
        var searchIndexPath = Path.Combine(_siteDirectory, "index.json");
        if (!File.Exists(searchIndexPath))
        {
            // Try alternative locations
            searchIndexPath = Path.Combine(_siteDirectory, "search", "index.json");
            if (!File.Exists(searchIndexPath))
            {
                searchIndexPath = Directory.GetFiles(_siteDirectory, "*index*.json", SearchOption.AllDirectories).FirstOrDefault();
            }
        }

        // Assert
        searchIndexPath.Should().NotBeNull("Search index file should exist");
        File.Exists(searchIndexPath!).Should().BeTrue("Search index file should exist");

        var indexContent = await File.ReadAllTextAsync(searchIndexPath!);
        indexContent.Should().NotBeNullOrEmpty("Search index should have content");

        // Validate JSON structure
        var searchData = JsonSerializer.Deserialize<JsonElement>(indexContent);
        searchData.ValueKind.Should().NotBe(JsonValueKind.Null, "Search index should be valid JSON");
    }

    /// <summary>
    /// Tests that search functionality can find specific content by search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    [Theory]
    [InlineData("SampleClass")]
    [InlineData("DoSomething")]
    [InlineData("Initialize")]
    public async Task SearchFunctionality_ShouldFindSpecificContent(string searchTerm)
    {
        // Arrange
        await GenerateTestDocumentationSiteAsync();
        await StartLocalServerAsync();

        // Act
        var results = await PerformSearchAsync(searchTerm);

        // Assert
        results.Should().NotBeEmpty($"Search should find results for '{searchTerm}'");
        results.Should().Contain(r =>
            r.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            r.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
            $"Search results should contain '{searchTerm}'");
    }

    private async Task GenerateTestDocumentationSiteAsync()
    {
        // Create test project structure
        var projectPath = Path.Combine(_tempDirectory, "TestProject.csproj");
        var projectContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <GenerateDocumentationFile>true</GenerateDocumentationFile>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="docfx" Version="2.75.3" PrivateAssets="all" />
              </ItemGroup>
            </Project>
            """;
        await File.WriteAllTextAsync(projectPath, projectContent);

        // Create sample source files with comprehensive documentation
        await CreateSampleSourceFilesAsync();

        // Create DocFX configuration
        var docfxConfigPath = Path.Combine(_tempDirectory, "docfx.json");
        var docfxConfig = $$"""
            {
              "metadata": [
                {
                  "src": [
                    {
                      "files": ["*.csproj"],
                      "src": "."
                    }
                  ],
                  "dest": "api-metadata"
                }
              ],
              "build": {
                "content": [
                  {
                    "files": ["api-metadata/*.yml"],
                    "src": "."
                  },
                  {
                    "files": ["*.md"],
                    "src": "."
                  }
                ],
                "resource": [
                  {
                    "files": ["images/**"],
                    "src": "."
                  }
                ],
                "dest": "{{_siteDirectory.Replace(_tempDirectory, ".").Replace("\\", "/")}}",
                "globalMetadata": {
                  "_appTitle": "Terminal.Gui.Xaml Test Documentation",
                  "_enableSearch": true
                }
              }
            }
            """;
        await File.WriteAllTextAsync(docfxConfigPath, docfxConfig);

        // Create additional documentation files
        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "index.md"), """
            # Terminal.Gui.Xaml Test Documentation

            Welcome to the test documentation site.

            ## API Reference

            Browse the [API documentation](api/) for detailed information about classes and methods.
            """);

        await File.WriteAllTextAsync(Path.Combine(_tempDirectory, "toc.yml"), """
            - name: Home
              href: index.md
            - name: API Documentation
              href: api/
            """);

        // Generate documentation using DocFX
        await RunDocFxAsync(_tempDirectory);
    }

    private async Task CreateSampleSourceFilesAsync()
    {
        var sampleClassContent = """
            using System;

            namespace TestProject
            {
                /// <summary>
                /// A sample class for testing documentation generation and site navigation.
                /// </summary>
                /// <remarks>
                /// This class provides examples of various documentation elements.
                /// </remarks>
                public class SampleClass
                {
                    /// <summary>
                    /// Gets or sets the sample property.
                    /// </summary>
                    /// <value>The sample value used for demonstration.</value>
                    public string SampleProperty { get; set; } = string.Empty;

                    /// <summary>
                    /// Performs a sample operation on the provided input.
                    /// </summary>
                    /// <param name="input">The input parameter to process.</param>
                    /// <returns>The processed result string.</returns>
                    /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
                    /// <example>
                    /// <code>
                    /// var sample = new SampleClass();
                    /// var result = sample.DoSomething("test");
                    /// </code>
                    /// </example>
                    public string DoSomething(string input)
                    {
                        if (input == null) throw new ArgumentNullException(nameof(input));
                        return $"Processed: {input}";
                    }

                    /// <summary>
                    /// Initializes the class with the specified configuration.
                    /// </summary>
                    /// <param name="config">The configuration to apply.</param>
                    /// <seealso cref="DoSomething(string)"/>
                    public void Initialize(string config)
                    {
                        // Implementation details
                    }
                }

                /// <summary>
                /// Interface for demonstration of interface documentation.
                /// </summary>
                public interface ISampleInterface
                {
                    /// <summary>
                    /// Performs an interface operation.
                    /// </summary>
                    /// <returns>The operation result.</returns>
                    Task<string> PerformOperationAsync();
                }
            }
            """;

        await File.WriteAllTextAsync(
            Path.Combine(_tempDirectory, "SampleClass.cs"),
            sampleClassContent);
    }

    private async Task RunDocFxAsync(string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "docfx docfx.json",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start DocFX process");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        _output.WriteLine($"DocFX output: {output}");
        if (!string.IsNullOrEmpty(error))
        {
            _output.WriteLine($"DocFX error: {error}");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"DocFX failed with exit code {process.ExitCode}: {error}");
        }
    }

    private async Task StartLocalServerAsync()
    {
        if (_httpClient != null) return; // Already started

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"docfx serve \"{_siteDirectory}\" --port 8080",
            WorkingDirectory = _tempDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _serverProcess = Process.Start(startInfo);
        if (_serverProcess == null)
        {
            throw new InvalidOperationException("Failed to start DocFX server");
        }

        // Wait for server to start
        await Task.Delay(3000);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };

        // Verify server is responding
        var retries = 5;
        while (retries > 0)
        {
            try
            {
                var response = await _httpClient.GetAsync("/");
                if (response.IsSuccessStatusCode) break;
            }
            catch
            {
                // Ignore and retry
            }

            retries--;
            if (retries > 0) await Task.Delay(1000);
        }

        if (retries == 0)
        {
            throw new InvalidOperationException("DocFX server failed to start or respond");
        }
    }

    private async Task<List<SearchResult>> PerformSearchAsync(string query)
    {
        if (_httpClient == null)
        {
            await StartLocalServerAsync();
        }

        try
        {
            // Try to find search endpoint
            var searchUrl = $"/search?q={Uri.EscapeDataString(query)}";
            var response = await _httpClient!.GetAsync(searchUrl);

            if (!response.IsSuccessStatusCode)
            {
                // Try alternative search approach - look for search index
                var indexResponse = await _httpClient.GetAsync("/index.json");
                if (indexResponse.IsSuccessStatusCode)
                {
                    var indexContent = await indexResponse.Content.ReadAsStringAsync();
                    return ParseSearchResults(indexContent, query);
                }
            }
            else
            {
                var searchContent = await response.Content.ReadAsStringAsync();
                return ParseSearchResults(searchContent, query);
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Search failed: {ex.Message}");
        }

        // Fallback: search through static files
        return await SearchStaticFilesAsync(query);
    }

    private static List<SearchResult> ParseSearchResults(string content, string query)
    {
        var results = new List<SearchResult>();

        try
        {
            var searchData = JsonSerializer.Deserialize<JsonElement>(content);

            if (searchData.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in searchData.EnumerateArray())
                {
                    if (item.TryGetProperty("title", out var title) &&
                        item.TryGetProperty("content", out var itemContent))
                    {
                        var titleStr = title.GetString() ?? "";
                        var contentStr = itemContent.GetString() ?? "";

                        if (titleStr.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            contentStr.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(new SearchResult
                            {
                                Title = titleStr,
                                Content = contentStr,
                                Url = item.TryGetProperty("url", out var url) ? url.GetString() ?? "" : ""
                            });
                        }
                    }
                }
            }
        }
        catch
        {
            // If JSON parsing fails, try HTML parsing
            var doc = new HtmlDocument();
            doc.LoadHtml(content);

            var searchResults = doc.DocumentNode.SelectNodes("//*[contains(text(), '" + query + "')]");
            if (searchResults != null)
            {
                results.AddRange(searchResults.Take(10).Select(node => new SearchResult
                {
                    Title = query,
                    Content = node.InnerText,
                    Url = ""
                }));
            }
        }

        return results;
    }

    private async Task<List<SearchResult>> SearchStaticFilesAsync(string query)
    {
        var results = new List<SearchResult>();
        var htmlFiles = Directory.GetFiles(_siteDirectory, "*.html", SearchOption.AllDirectories);

        foreach (var file in htmlFiles.Take(20)) // Limit to avoid excessive processing
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                if (content.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? Path.GetFileName(file);

                    results.Add(new SearchResult
                    {
                        Title = title,
                        Content = content.Substring(0, Math.Min(200, content.Length)),
                        Url = Path.GetRelativePath(_siteDirectory, file)
                    });
                }
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        return results;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="DocumentationSiteTests"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();

            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                try
                {
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"Warning: Could not stop server process: {ex.Message}");
                }
                finally
                {
                    _serverProcess.Dispose();
                }
            }

            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Warning: Could not clean up temp directory: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Represents a search result from documentation site search.
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Gets or sets the title of the search result.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the search result.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the search result.
    /// </summary>
#pragma warning disable CA1056 // URI-like properties should not be strings
    public string Url { get; set; } = string.Empty;
#pragma warning restore CA1056 // URI-like properties should not be strings
}
