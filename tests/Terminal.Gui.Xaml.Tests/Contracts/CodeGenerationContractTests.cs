// <copyright file="CodeGenerationContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
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
        var generator = GetGenerator ();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
        var code = await generator.GenerateAsync(xaml);
    Assert.False(string.IsNullOrEmpty(code));
    }

    [Fact]
    public async Task GenerateAsync_InvalidXaml_ThrowsCodeGenerationException()
    {
        var generator = GetGenerator ();
        var xaml = "<Window><Label></Window>";
        await Assert.ThrowsAsync<Terminal.Gui.Xaml.Exceptions.CodeGenerationException>(() => generator.GenerateAsync(xaml));
    }

    [Fact]
    public async Task GenerateClassesAsync_MultiClass_GeneratesAllClasses()
    {
        var generator = GetGenerator ();
        var xamlDocs = new[] { "<Window x:Class=\"A\" />", "<Window x:Class=\"B\" />" };
    var classes = await generator.GenerateClassesAsync(xamlDocs);
    Assert.Equal(2, classes.Count());
    }

    [Fact]
    public void ValidateGeneration_ValidXaml_ReturnsTrue()
    {
        var generator = GetGenerator ();
        var xaml = "<Window><Label Text=\"Hello\" /></Window>";
    Assert.True(generator.ValidateGeneration(xaml));
    }

    [Fact]
    public void RegisterTemplate_CustomTemplate_Succeeds()
    {
        var generator = GetGenerator ();
        generator.RegisterTemplate("CustomTemplate", "template content");
        // No exception means success
    }

    [Fact]
    public async Task GenerateAsync_PerformanceRequirement()
    {
        var generator = GetGenerator ();
        var xaml = "<Window>" + string.Concat(Enumerable.Repeat("<Label Text=\"X\" />", 1000)) + "</Window>";
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await generator.GenerateAsync(xaml);
        sw.Stop();
    Assert.True(sw.ElapsedMilliseconds < 200, $"Expected <200ms, actual {sw.ElapsedMilliseconds}ms");
    }

    private static ICodeGenerator GetGenerator()
    {
        return new SimpleCodeGenerator();
    }
}
