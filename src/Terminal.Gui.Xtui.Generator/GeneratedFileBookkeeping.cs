using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;


namespace Terminal.Gui.Xtui.Generator;

// Small, immutable key describing an input .xtui file
// Use a readonly struct with value equality to avoid requiring C# record support on older TFMs
internal readonly struct GeneratedFileKey (string inputPath) : System.IEquatable<GeneratedFileKey>
{
    public readonly string InputPath = inputPath;

    public bool Equals(GeneratedFileKey other) => string.Equals(InputPath, other.InputPath, System.StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is GeneratedFileKey k && Equals(k);

    public override int GetHashCode() => InputPath is null ? 0 : System.StringComparer.OrdinalIgnoreCase.GetHashCode(InputPath);
}

// Metadata for a generated file
internal sealed class GeneratedFileInfo (string generatedFileName, string ns, string className)
{
    public string GeneratedFileName { get; } = generatedFileName;
    public string Namespace { get; } = ns;
    public string ClassName { get; } = className;
}

// Small identity describing the generated symbol (namespace + class)
internal readonly struct GeneratedIdentity (string? ns, string? className) : System.IEquatable<GeneratedIdentity>
{
    public readonly string Namespace = ns ?? string.Empty;
    public readonly string ClassName = className ?? string.Empty;

    public bool Equals(GeneratedIdentity other) =>
        string.Equals(Namespace, other.Namespace, System.StringComparison.Ordinal) &&
        string.Equals(ClassName, other.ClassName, System.StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is GeneratedIdentity g && Equals(g);
    public override int GetHashCode() => (System.StringComparer.Ordinal.GetHashCode(Namespace) * 397) ^ System.StringComparer.Ordinal.GetHashCode(ClassName);
}

// Central bookkeeping for generated files. Uses a record-based key to avoid
// accidental reference-equality issues and provide value semantics.
internal static class GeneratedFileBookkeeping
{
    private static readonly ConcurrentDictionary<GeneratedFileKey, GeneratedFileInfo> SGeneratedFiles = new();
    // Reverse map from generated identity -> input path that produced it
    private static readonly ConcurrentDictionary<GeneratedIdentity, string> SIdentityToInput = new();

    /// <summary>
    /// Record a generated file mapping without reporting diagnostics.
    /// </summary>
    public static void Record(string inputPath, string generatedFileName, string @namespace, string className)
    {
        GeneratedFileKey key = new (inputPath);
        GeneratedFileInfo info = new (generatedFileName, @namespace, className);
        SGeneratedFiles[key] = info;

        GeneratedIdentity identity = new (@namespace, className);
        // Best-effort reverse mapping; do not attempt to report diagnostics from this path.
        SIdentityToInput.TryAdd(identity, inputPath);
    }

    /// <summary>
    /// Record a generated file mapping and report diagnostics via the provided <paramref name="spc"/>.
    /// </summary>
    public static void Record(string inputPath, string generatedFileName, string @namespace, string className, SourceProductionContext spc)
    {
        GeneratedFileKey key = new (inputPath);
        GeneratedFileInfo info = new (generatedFileName, @namespace, className);
        SGeneratedFiles[key] = info;

        GeneratedIdentity identity = new (@namespace, className);
        if (!SIdentityToInput.TryAdd(identity, inputPath))
        {
            // Already exists - check if it's a different input
            if (SIdentityToInput.TryGetValue(identity, out string? existingInput) && !string.Equals(existingInput, inputPath, System.StringComparison.OrdinalIgnoreCase))
            {
                DiagnosticDescriptor descriptor = new (
                                                       id: "XTUI003",
                                                       title: "XTUI Generated Class Name Collision",
                                                       messageFormat: "XTUI files '{0}' and '{1}' generate the same class '{2}.{3}'. Rename one input or change class-name resolution.",
                                                       category: "Terminal.Gui.Xtui",
                                                       defaultSeverity: DiagnosticSeverity.Warning,
                                                       isEnabledByDefault: true);

                Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, existingInput, inputPath, @namespace, className);
                spc.ReportDiagnostic(diagnostic);
            }
        }
        else
        {
            // Fallback: if the identity map succeeded but there's another recorded generated file
            // with the same namespace+class name from a different input (possible due to race or key mismatch),
            // enumerate the known generated files to double-check and report a diagnostic if found.
            foreach (KeyValuePair<GeneratedFileKey, GeneratedFileInfo> kvp in SGeneratedFiles)
            {
                GeneratedFileKey otherKey = kvp.Key;
                GeneratedFileInfo? otherInfo = kvp.Value;
                if (!string.Equals(otherKey.InputPath, inputPath, System.StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(otherInfo.Namespace, @namespace, System.StringComparison.Ordinal) &&
                    string.Equals(otherInfo.ClassName, className, System.StringComparison.Ordinal))
                {
                    DiagnosticDescriptor descriptor = new (
                                                           id: "XTUI003",
                                                           title: "XTUI Generated Class Name Collision",
                                                           messageFormat: "XTUI files '{0}' and '{1}' generate the same class '{2}.{3}'. Rename one input or change class-name resolution.",
                                                           category: "Terminal.Gui.Xtui",
                                                           defaultSeverity: DiagnosticSeverity.Warning,
                                                           isEnabledByDefault: true);

                    Diagnostic diagnostic = Diagnostic.Create(descriptor, Location.None, otherKey.InputPath, inputPath, @namespace, className);
                    spc.ReportDiagnostic(diagnostic);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Backwards-compatible nullable overload; delegates to the non-nullable overload when available.
    /// </summary>
    public static void Record(string inputPath, string generatedFileName, string @namespace, string className, SourceProductionContext? spc = null)
    {
        if (spc.HasValue)
        {
            Record(inputPath, generatedFileName, @namespace, className, spc.Value);
        }
        else
        {
            Record(inputPath, generatedFileName, @namespace, className);
        }
    }

    public static bool TryGet(string inputPath, out GeneratedFileInfo? info)
    {
        return SGeneratedFiles.TryGetValue(new GeneratedFileKey(inputPath), out info);
    }

    public static GeneratedFileInfo? Get(string inputPath)
    {
        return SGeneratedFiles.TryGetValue(new GeneratedFileKey(inputPath), out GeneratedFileInfo? info) ? info : null;
    }

    public static int Count => SGeneratedFiles.Count;

    public static void Clear() => SGeneratedFiles.Clear();
}
