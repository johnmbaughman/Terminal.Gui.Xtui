using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Terminal.Gui.Xaml.Events;

/// <summary>
/// Central registry for event handler binding and dispatch in XAML runtime.
/// </summary>
public class EventHandlerRegistry
{
    private readonly ConcurrentDictionary<string, Delegate> _handlers = new();

    /// <summary>
    /// Registers an event handler for a given event name.
    /// </summary>
    public void Register(string eventName, Delegate handler)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name cannot be null or whitespace.", nameof(eventName));
        }

        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        _handlers [eventName] = handler;
    }

    /// <summary>
    /// Gets the registered handler for an event name, or null if not found.
    /// </summary>
    public Delegate? GetHandler(string eventName)
    {
        _handlers.TryGetValue(eventName, out var handler);
        return handler;
    }

    /// <summary>
    /// Dispatches the event to the registered handler, supporting async if needed.
    /// </summary>
    public void Dispatch(string eventName, object sender, object[] args)
    {
        var handler = GetHandler(eventName);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for event '{eventName}'.");
        }

        handler.DynamicInvoke(args.Prepend(sender).ToArray());
    }
}
