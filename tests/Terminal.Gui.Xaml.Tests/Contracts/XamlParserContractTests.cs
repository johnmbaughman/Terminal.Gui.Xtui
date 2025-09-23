// <copyright file="XamlParserContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
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
        var parser = GetParser();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";

        // Act
        var view = await parser.ParseAsync(xaml);

        // Assert
        view.Should().NotBeNull();
        view.GetType().Name.Should().Be("Window");
    }

    [Fact]
    public async Task ParseAsync_InvalidXaml_ThrowsXamlParseException()
    {
        var parser = GetParser();
        var xaml = "<Window><Label></Window>"; // Malformed
        await FluentActions.Invoking(() => parser.ParseAsync(xaml))
            .Should().ThrowAsync<Terminal.Gui.Xaml.Exceptions.XamlParseException>();
    }

    [Fact]
    public async Task ParseFileAsync_ValidFile_ReturnsViewHierarchy()
    {
        var parser = GetParser();
        var filePath = "TestAssets/SimpleApp.xaml";
        var view = await parser.ParseFileAsync(filePath);
        view.Should().NotBeNull();
    }

    [Fact]
    public void Validate_ValidXaml_ReturnsTrue()
    {
        var parser = GetParser();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
        parser.Validate(xaml).Should().BeTrue();
    }

    [Fact]
    public void RegisterControl_CustomControl_Succeeds()
    {
        var parser = GetParser();
        parser.RegisterControl("CustomControl", typeof(object));
        // No exception means success
    }

    [Fact]
    public async Task ParseAsync_PerformanceRequirement()
    {
        var parser = GetParser();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await parser.ParseAsync(xaml);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

#pragma warning disable CA1822 // Mark members as static
    private IXamlParser GetParser()
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Provide a test implementation or mock
        throw new NotImplementedException();
    }
}
