// <copyright file="CodeGenerationContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
using Terminal.Gui.Xaml.Generation;
using System.Threading.Tasks;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for ICodeGenerator API, validating constitutional requirements.
/// </summary>
public class CodeGenerationContractTests
{
    [Fact]
    public async Task GenerateAsync_ValidXaml_GeneratesCode()
    {
        var generator = GetGenerator();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
        var code = await generator.GenerateAsync(xaml);
        code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateAsync_InvalidXaml_ThrowsCodeGenerationException()
    {
        var generator = GetGenerator();
        var xaml = "<Window><Label></Window>";
        await FluentActions.Invoking(() => generator.GenerateAsync(xaml))
            .Should().ThrowAsync<Terminal.Gui.Xaml.Exceptions.CodeGenerationException>();
    }

    [Fact]
    public async Task GenerateClassesAsync_MultiClass_GeneratesAllClasses()
    {
        var generator = GetGenerator();
        var xamlDocs = new[] { "<Window x:Class=\"A\" />", "<Window x:Class=\"B\" />" };
        var classes = await generator.GenerateClassesAsync(xamlDocs);
        classes.Should().HaveCount(2);
    }

    [Fact]
    public void ValidateGeneration_ValidXaml_ReturnsTrue()
    {
        var generator = GetGenerator();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
        generator.ValidateGeneration(xaml).Should().BeTrue();
    }

    [Fact]
    public void RegisterTemplate_CustomTemplate_Succeeds()
    {
        var generator = GetGenerator();
        generator.RegisterTemplate("CustomTemplate", "template content");
        // No exception means success
    }

    [Fact]
    public async Task GenerateAsync_PerformanceRequirement()
    {
        var generator = GetGenerator();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await generator.GenerateAsync(xaml);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

#pragma warning disable CA1822 // Mark members as static
    private ICodeGenerator GetGenerator()
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Provide a test implementation or mock
        throw new NotImplementedException();
    }
}
