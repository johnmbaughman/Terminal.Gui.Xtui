// <copyright file="RuntimeBindingContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
using Terminal.Gui.Xaml.Binding;

namespace Terminal.Gui.Xaml.Tests.Contracts;

/// <summary>
/// Contract tests for runtime data binding and MVVM support.
/// </summary>
public class RuntimeBindingContractTests
{
    [Fact]
    public void PropertyBinding_OneWay_Succeeds()
    {
        var engine = GetEngine();
        engine.Bind("SourceProp", "TargetProp", BindingMode.OneWay);
        // No exception means success
    }

    [Fact]
    public void PropertyBinding_TwoWay_Succeeds()
    {
        var engine = GetEngine();
        engine.Bind("SourceProp", "TargetProp", BindingMode.TwoWay);
        // No exception means success
    }

    [Fact]
    public void EventHandlerBinding_Succeeds()
    {
        var engine = GetEngine();
        engine.BindEvent("Button.Clicked", "OnClicked");
        // No exception means success
    }

    [Fact]
    public void BindingError_ThrowsBindingException()
    {
        var engine = GetEngine();
        FluentActions.Invoking(() => engine.Bind("BadSource", "BadTarget", BindingMode.OneWay))
            .Should().Throw<Terminal.Gui.Xaml.Exceptions.BindingException>();
    }

    [Fact]
    public void MVVMSupport_INotifyPropertyChanged_Succeeds()
    {
        var engine = GetEngine();
        engine.Bind("ViewModelProp", "ViewProp", BindingMode.TwoWay);
        // No exception means success
    }

#pragma warning disable CA1822 // Mark members as static
    private IDataBindingEngine GetEngine()
#pragma warning restore CA1822 // Mark members as static
    {
        // TODO: Provide a test implementation or mock
        throw new NotImplementedException();
    }
}
