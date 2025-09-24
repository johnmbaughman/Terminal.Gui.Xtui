// <copyright file="RuntimeBindingContractTests.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using Xunit;
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
        var engine = GetEngine ();
        engine.Bind("SourceProp", "TargetProp", BindingMode.OneWay);
        // No exception means success
    }

    [Fact]
    public void PropertyBinding_TwoWay_Succeeds()
    {
        var engine = GetEngine ();
        engine.Bind("SourceProp", "TargetProp", BindingMode.TwoWay);
        // No exception means success
    }

    [Fact]
    public void EventHandlerBinding_Succeeds()
    {
        var engine = GetEngine ();
        engine.BindEvent("Button.Clicked", "OnClicked");
        // No exception means success
    }

    [Fact]
    public void BindingError_ThrowsBindingException()
    {
        var engine = GetEngine ();
        Assert.Throws<Terminal.Gui.Xaml.Exceptions.BindingException>(() => engine.Bind("BadSource", "BadTarget", BindingMode.OneWay));
    }

    [Fact]
    public void MVVMSupport_INotifyPropertyChanged_Succeeds()
    {
        var engine = GetEngine ();
        engine.Bind("ViewModelProp", "ViewProp", BindingMode.TwoWay);
        // No exception means success
    }

    private static IDataBindingEngine GetEngine()
    {
        return new SimpleDataBindingEngine();
    }
}
