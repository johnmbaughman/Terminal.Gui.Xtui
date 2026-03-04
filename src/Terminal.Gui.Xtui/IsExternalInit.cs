// Polyfill for IsExternalInit to support records in netstandard2.0
#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130

internal static class IsExternalInit { }