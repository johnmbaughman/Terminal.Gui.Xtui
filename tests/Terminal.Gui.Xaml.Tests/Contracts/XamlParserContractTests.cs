// <copyright file="XamlParserContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using Terminal.Gui.Xaml.Parsing;
using System.Threading.Tasks;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for IXamlParser API, validating constitutional requirements.
/// </summary>
public class XamlParserContractTests
{
    [Fact]
    public async Task ParseAsync_ValidXaml_ReturnsViewHierarchy()
    {
        // Arrange
        var parser = GetParser ();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";

        // Act
        var view = await parser.ParseAsync(xaml);

        // Assert
    Assert.NotNull(view);
    Assert.Equal("Window", view.GetType().Name);
    }

    [Fact]
    public async Task ParseAsync_InvalidXaml_ThrowsXamlParseException()
    {
        var parser = GetParser ();
        var xaml = "<Window><Label></Window>"; // Malformed
        await Assert.ThrowsAsync<Terminal.Gui.Xaml.Exceptions.XamlParseException>(() => parser.ParseAsync(xaml));
    }

    [Fact]
    public async Task ParseFileAsync_ValidFile_ReturnsViewHierarchy()
    {
        var parser = GetParser ();
        var filePath = "TestAssets/SimpleApp.xaml";
        var view = await parser.ParseFileAsync(filePath);
    Assert.NotNull(view);
    }

    [Fact]
    public void Validate_ValidXaml_ReturnsTrue()
    {
        var parser = GetParser ();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
    Assert.True(parser.Validate(xaml));
    }

    [Fact]
    public void RegisterControl_CustomControl_Succeeds()
    {
        var parser = GetParser ();
        parser.RegisterControl("CustomControl", typeof(object));
        // No exception means success
    }

    [Fact]
    public async Task ParseAsync_PerformanceRequirement()
    {
        var parser = GetParser ();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await parser.ParseAsync(xaml);
        sw.Stop();
    Assert.True(sw.ElapsedMilliseconds < 100, $"Expected <100ms, actual {sw.ElapsedMilliseconds}ms");
    }

    private static IXamlParser GetParser()
    {
        return new SimpleXamlParser();
    }
}
