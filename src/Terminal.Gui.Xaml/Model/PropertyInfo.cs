// <copyright file="PropertyInfo.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System;
using System.Reflection;

namespace Terminal.Gui.Xaml.Model;

/// <summary>
/// Provides metadata and binding support for a property in a XAML model.
/// </summary>
public class PropertyInfo
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyInfo"/> class.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <param name="propertyType">The property type.</param>
	/// <param name="reflectionInfo">The reflection PropertyInfo.</param>
	public PropertyInfo(string name, Type propertyType, PropertyInfo? reflectionInfo = null)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
		ReflectionInfo = reflectionInfo;
	}

	/// <summary>
	/// Gets the property name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the property type.
	/// </summary>
	public Type PropertyType { get; }

	/// <summary>
	/// Gets the underlying PropertyInfo from reflection.
	/// </summary>
	public PropertyInfo? ReflectionInfo { get; }

	/// <summary>
	/// Gets or sets the binding expression for this property.
	/// </summary>
	public string? BindingExpression { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether two-way binding is enabled.
	/// </summary>
	public bool IsTwoWay { get; set; }

	/// <summary>
	/// Parses and validates the binding expression.
	/// </summary>
	/// <returns>True if the expression is valid; otherwise, false.</returns>
	public bool ValidateBindingExpression()
	{
		// Simple validation: must not be null or whitespace
		return !string.IsNullOrWhiteSpace(BindingExpression);
	}

	/// <summary>
	/// Gets the value of the property from the specified target object.
	/// </summary>
	/// <param name="target">The object to get the property value from.</param>
	/// <returns>The value of the property.</returns>
	public object? GetValue(object target)
	{
		return ReflectionInfo?.GetValue(target);
	}

	/// <summary>
	/// Sets the value of the property on the specified target object.
	/// </summary>
	/// <param name="target">The object to set the property value on.</param>
	/// <param name="value">The value to set.</param>
	public void SetValue(object target, object? value)
	{
		ReflectionInfo?.SetValue(target, value);
	}
}

