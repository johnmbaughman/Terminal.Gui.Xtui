// <copyright file="TestDataFactory.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

namespace Terminal.Gui.Xaml.Tests.Fixtures;

/// <summary>
/// Factory for generating test data and mock objects for XAML tests.
/// </summary>
public static class TestDataFactory
{
    public static string ValidWindowXaml => "<Window><Label Text=\"Hello\" /></Window>";
    public static string InvalidWindowXaml => "<Window><Label></Window>";
    // Add more sample generators as needed
}
