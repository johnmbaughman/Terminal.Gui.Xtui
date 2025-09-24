// <copyright file="GlobalSuppressions.cs" company="Terminal.Gui.Xaml">
// Copyright © Terminal.Gui.Xaml 2025. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

// Global suppressions for project-wide analysis rules
[assembly: SuppressMessage ("Design", "CA1032:Implement standard exception constructors", Justification = "Custom exceptions use specific constructors for error context")]
[assembly: SuppressMessage ("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Some classes are instantiated via reflection or dependency injection")]
[assembly: SuppressMessage ("Design", "CA1031:Do not catch general exception types", Justification = "XAML parsing requires broad exception handling for error reporting")]
[assembly: SuppressMessage ("StyleCop.CSharp.LayoutRules", "SA1518:File is required to end with a single newline character", Justification = "Temporary suppression during initial setup")]
