using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Terminal.Gui.Xaml.Binding;

/// <summary>
/// Provides runtime data binding and property change notification for Terminal.Gui.Xaml.
/// </summary>
public class DataBindingEngine : IDataBindingEngine
{
    private readonly Dictionary<string, PropertyChangeTracker> _trackers = new();

    /// <summary>
    /// Binds a source property to a target property with a given mode.
    /// </summary>
    /// <param name="sourceProperty">The source property path.</param>
    /// <param name="targetProperty">The target property path.</param>
    /// <param name="mode">The binding mode.</param>
    public void Bind(string sourceProperty, string targetProperty, BindingMode mode)
    {
        // TODO: Implement property path binding with mode support
        throw new NotImplementedException();
    }

    /// <summary>
    /// Binds data to a control.
    /// </summary>
    /// <param name="control">The control to bind data to.</param>
    /// <param name="dataSource">The data source.</param>
    /// <param name="propertyPath">The property path for binding.</param>
    public void Bind(object control, object dataSource, string propertyPath)
    {
        // TODO: Implement control-data binding
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unbinds data from a control.
    /// </summary>
    /// <param name="control">The control to unbind data from.</param>
    public void Unbind(object control)
    {
        // TODO: Implement unbinding for control
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the binding for a control.
    /// </summary>
    /// <param name="control">The control to update binding for.</param>
    public void UpdateBinding(object control)
    {
        // TODO: Implement binding update logic
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates a binding expression.
    /// </summary>
    /// <param name="expression">The binding expression.</param>
    /// <returns>True if the binding expression is valid; otherwise, false.</returns>
    public bool ValidateBinding(string expression)
    {
        // TODO: Implement binding expression validation
        throw new NotImplementedException();
    }

    /// <summary>
    /// Binds an event on a control to a handler name.
    /// </summary>
    /// <param name="eventPath">The event path (e.g., Button.Clicked).</param>
    /// <param name="handlerName">The handler method name.</param>
    public void BindEvent(string eventPath, string handlerName)
    {
        // TODO: Implement event binding
        throw new NotImplementedException();
    }

    /// <summary>
    /// Binds a source property to a target property for property change notification.
    /// </summary>
    /// <param name="source">The source implementing INotifyPropertyChanged.</param>
    /// <param name="target">The target object.</param>
    /// <param name="sourceProperty">The source property name.</param>
    /// <param name="targetProperty">The target property name.</param>
    public void Bind(INotifyPropertyChanged source, object target, string sourceProperty, string targetProperty)
    {
        var sourceProp = source.GetType().GetProperty(sourceProperty);
        var targetProp = target.GetType().GetProperty(targetProperty);
        if (sourceProp == null || targetProp == null)
        {
            throw new ArgumentException("Invalid property names for binding.");
        }
        var tracker = new PropertyChangeTracker(source, sourceProperty, target, targetProperty);
        _trackers[$"{source.GetHashCode()}:{sourceProperty}"] = tracker;
    }

    /// <summary>
    /// Unbinds a source property from property change notification.
    /// </summary>
    /// <param name="source">The source implementing INotifyPropertyChanged.</param>
    /// <param name="sourceProperty">The source property name.</param>
    public void Unbind(INotifyPropertyChanged source, string sourceProperty)
    {
        var key = $"{source.GetHashCode()}:{sourceProperty}";
        if (_trackers.TryGetValue(key, out var tracker))
        {
            tracker.Detach();
            _trackers.Remove(key);
        }
    }

    
}

/// <summary>
    /// Tracks property changes and synchronizes values between source and target.
    /// </summary>
    public class PropertyChangeTracker
    {
    private readonly INotifyPropertyChanged _source;
    private readonly string _sourceProperty;
    private readonly object _target;
    private readonly string _targetProperty;
    private bool _isDetached;

        /// <summary>
        /// Initializes a new instance of PropertyChangeTracker.
        /// </summary>
        /// <param name="source">The source implementing INotifyPropertyChanged.</param>
        /// <param name="sourceProperty">The source property name.</param>
        /// <param name="target">The target object.</param>
        /// <param name="targetProperty">The target property name.</param>
        public PropertyChangeTracker(INotifyPropertyChanged source, string sourceProperty, object target, string targetProperty)
        {
            _source = source;
            _sourceProperty = sourceProperty;
            _target = target;
            _targetProperty = targetProperty;
            _source.PropertyChanged += Source_PropertyChanged;
            SyncValue();
        }

        /// <summary>
        /// Detaches the PropertyChanged event handler from the source.
        /// </summary>
        public void Detach()
        {
            if (!_isDetached)
            {
                _source.PropertyChanged -= Source_PropertyChanged;
                _isDetached = true;
            }
        }

        private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _sourceProperty)
            {
                SyncValue();
            }
        }

        private void SyncValue()
        {
            var sourceProp = _source.GetType().GetProperty(_sourceProperty);
            var targetProp = _target.GetType().GetProperty(_targetProperty);
            if (sourceProp != null && targetProp != null)
            {
                var value = sourceProp.GetValue(_source);
                targetProp.SetValue(_target, value);
            }
        }
    }