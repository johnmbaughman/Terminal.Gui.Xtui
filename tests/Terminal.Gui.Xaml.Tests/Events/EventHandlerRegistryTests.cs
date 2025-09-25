using System;
using Terminal.Gui.Xaml.Events;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Events;

public class EventHandlerRegistryTests
{
    [Fact]
    public void RegisterAndGetHandler_Works()
    {
        var registry = new EventHandlerRegistry();
        EventHandler handler = (s, e) => { };
        registry.Register("TestEvent", handler);
        var retrieved = registry.GetHandler("TestEvent");
        Assert.Equal(handler, retrieved);
    }

    [Fact]
    public void Dispatch_InvokesHandler()
    {
        var registry = new EventHandlerRegistry();
        bool called = false;
        EventHandler handler = (s, e) => called = true;
        registry.Register("TestEvent", handler);
        registry.Dispatch("TestEvent", this, new object[] { EventArgs.Empty });
        Assert.True(called);
    }

    [Fact]
    public void Dispatch_NoHandler_Throws()
    {
        var registry = new EventHandlerRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.Dispatch("MissingEvent", this, Array.Empty<object>()));
    }
}
