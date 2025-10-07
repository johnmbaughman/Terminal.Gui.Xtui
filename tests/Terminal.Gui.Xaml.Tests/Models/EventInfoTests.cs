using System;
using System.Reflection;
using System.Threading.Tasks;
using Terminal.Gui.Xaml.Model;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Models;

public class EventInfoTests
{
    private class TestControl
    {
        public event EventHandler? SimpleEvent;
        public event Func<object, object[], Task>? AsyncEvent;
        public void RaiseSimpleEvent() => SimpleEvent?.Invoke(this, EventArgs.Empty);
        public async Task RaiseAsyncEvent() => await AsyncEvent?.Invoke(this, new object[] { "data" })!;
    }

    [Fact]
    public void RegisterHandler_BindsEventHandler()
    {
        var control = new TestControl();
        var eventInfo = new Terminal.Gui.Xaml.Model.EventInfo(
            "SimpleEvent",
            typeof(EventHandler),
            typeof(TestControl).GetEvent("SimpleEvent")!);
        bool called = false;
        EventHandler handler = (s, e) => called = true;
        eventInfo.RegisterHandler(control, handler);
        control.RaiseSimpleEvent();
        Assert.True(called);
    }

    [Fact]
    public void ValidateHandler_CorrectSignature_ReturnsTrue()
    {
        var method = typeof(EventInfoTests).GetMethod(nameof(SimpleHandler), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var eventInfo = new Terminal.Gui.Xaml.Model.EventInfo(
            "SimpleEvent",
            typeof(EventHandler),
            typeof(TestControl).GetEvent("SimpleEvent")!);
        Assert.True(eventInfo.ValidateHandler(method));
    }

    private void SimpleHandler(object sender, EventArgs args) { }

    [Fact]
    public async Task DispatchAsync_AsyncHandler_Works()
    {
        var control = new TestControl();
        var eventInfo = new Terminal.Gui.Xaml.Model.EventInfo(
            "AsyncEvent",
            typeof(Func<object, object[], Task>),
            typeof(TestControl).GetEvent("AsyncEvent")!,
            isAsyncHandler: true);
        bool called = false;
        Func<object, object[], Task> handler = async (s, args) => { called = true; await Task.CompletedTask; };
        eventInfo.RegisterHandler(control, handler);
        await eventInfo.DispatchAsync(control, new object[] { "data" }, handler);
        Assert.True(called);
    }
}
