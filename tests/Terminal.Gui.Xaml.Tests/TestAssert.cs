using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Terminal.Gui.Xaml.Tests;

/// <summary>
/// Lightweight assertion helpers to replace FluentAssertions usage with xUnit Assert calls.
/// Only implements the subset needed by the existing tests.
/// </summary>
internal static class TestAssert
{
    public static void NotNull(object? value, string? because = null)
        => Assert.NotNull(value);

    public static void Null(object? value)
        => Assert.Null(value);

    public static void True(bool condition, string? message = null)
        => Assert.True(condition, message);

    public static void False(bool condition, string? message = null)
        => Assert.False(condition, message);

    public static void Equal<T>(T expected, T actual)
        => Assert.Equal(expected, actual);

    public static void NotEqual<T>(T notExpected, T actual)
        => Assert.NotEqual(notExpected, actual);

    public static void InRange<T>(T actual, T low, T high) where T : IComparable
    {
        Assert.True(actual.CompareTo(low) >= 0 && actual.CompareTo(high) <= 0, $"Value {actual} not in range [{low},{high}]");
    }

    public static void GreaterThan<T>(T actual, T threshold) where T : IComparable
        => Assert.True(actual.CompareTo(threshold) > 0, $"Expected {actual} > {threshold}");

    public static void GreaterThanOrEqual<T>(T actual, T threshold) where T : IComparable
        => Assert.True(actual.CompareTo(threshold) >= 0, $"Expected {actual} >= {threshold}");

    public static void LessThan<T>(T actual, T threshold) where T : IComparable
        => Assert.True(actual.CompareTo(threshold) < 0, $"Expected {actual} < {threshold}");

    public static void LessThanOrEqual<T>(T actual, T threshold) where T : IComparable
        => Assert.True(actual.CompareTo(threshold) <= 0, $"Expected {actual} <= {threshold}");

    public static void NotEmpty<T>(IEnumerable<T>? enumerable, string? message = null)
    {
        Assert.NotNull(enumerable);
        Assert.True(enumerable!.Any(), message ?? "Expected collection to have at least one element");
    }

    public static void Empty<T>(IEnumerable<T>? enumerable)
    {
        Assert.NotNull(enumerable);
        Assert.False(enumerable!.Any(), "Expected collection to be empty");
    }

    public static void Contains<T>(IEnumerable<T> enumerable, Predicate<T> predicate, string? message = null)
    {
        Assert.True(enumerable.Any(e => predicate(e)), message ?? "Expected at least one matching element");
    }

    public static void Contains<T>(IEnumerable<T> enumerable, T expected)
    {
        Assert.Contains(expected, enumerable);
    }

    public static void NotContains<T>(IEnumerable<T> enumerable, Predicate<T> predicate, string? message = null)
    {
        Assert.False(enumerable.Any(e => predicate(e)), message ?? "Did not expect any matching element");
    }

    public static void StringContains(string? actual, string expectedSubstring)
    {
        Assert.NotNull(actual);
        Assert.Contains(expectedSubstring, actual, StringComparison.OrdinalIgnoreCase);
    }

    public static void NotStringContains(string? actual, string unexpectedSubstring)
    {
        if (actual == null)
        {
            return;
        }

        Assert.DoesNotContain(unexpectedSubstring, actual, StringComparison.OrdinalIgnoreCase);
    }

    public static void Count<T>(IEnumerable<T> enumerable, int expectedCount)
        => Assert.Equal(expectedCount, enumerable.Count());
}
