using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Terminal.Gui.Xtui.Generators;

namespace Terminal.Gui.Xtui;

[Generator]
/// <summary>
/// Roslyn incremental source generator that converts `.xtui` files into C# partial
/// classes at compile time when the consuming project references Terminal.Gui.
/// </summary>
public class XtuiGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator pipeline. Discovers `.xtui` AdditionalFiles,
    /// verifies the presence of Terminal.Gui in the compilation, parses XTUI content,
    /// and registers a per-file source output that generates a `.g.cs` partial
    /// class for each XTUI file.
    /// </summary>
    /// <param name="context">The generator initialization context provided by Roslyn.</param>
    public void Initialize (IncrementalGeneratorInitializationContext context)
    {
        // find additional files that end with .xtui
        var xamlFiles = context.AdditionalTextsProvider
            .Where (at => at.Path.EndsWith (".xtui", StringComparison.OrdinalIgnoreCase))
            .Select ((additionalText, cancellationToken) =>
            {
                SourceText? text = additionalText.GetText (cancellationToken);
                return new
                {
                    additionalText.Path,
                    FileName = Path.GetFileNameWithoutExtension (additionalText.Path),
                    Content = text?.ToString () ?? string.Empty
                };
            })
            .Where (x => !string.IsNullOrWhiteSpace (x.Content));

        // Detect duplicate filenames among AdditionalTexts (e.g., dirA/MyWindow.xtui and dirB/MyWindow.xtui)
        // and emit a diagnostic early so tests and users get immediate feedback.
        var duplicateFilenameGroups = xamlFiles.Collect().Select((arr, _) =>
        {
            return arr.GroupBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.ToArray())
                .ToArray();
        });

        context.RegisterSourceOutput(duplicateFilenameGroups, (spc, groups) =>
        {
            foreach (var group in groups)
            {
                // Report a diagnostic for each pair in the group (first vs others)
                var first = group.First();
                foreach (var other in group.Skip(1))
                {
                    var descriptor = new DiagnosticDescriptor(
                        id: "XTUI003",
                        title: "XTUI Generated Class Name Collision",
                        messageFormat: "XTUI files '{0}' and '{1}' generate the same class '{2}.{3}'. Rename one input or change class-name resolution.",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Warning,
                        isEnabledByDefault: true);

                    var diagnostic = Diagnostic.Create(descriptor, Location.None, first.Path, other.Path, first.FileName, first.FileName);
                    spc.ReportDiagnostic(diagnostic);
                }
            }
        });

        // Check if Terminal.Gui is referenced in the compilation
        // We check for multiple core types to ensure Terminal.Gui is properly referenced
        string [] requiredTypes = new []
        {
            "Terminal.Gui.App.Application",
            "Terminal.Gui.Views.Window",
            "Terminal.Gui.Views.View"
        };

        IncrementalValueProvider<bool> hasTerminalGuiReference = context.CompilationProvider
            .Select ((compilation, _) =>
                requiredTypes.Any (typeName =>
                    compilation.GetTypeByMetadataName (typeName) != null));

        // Combine XTUI files with both the reference check and compilation
        var xamlWithCompilation = xamlFiles
            .Combine (hasTerminalGuiReference)
            .Combine (context.CompilationProvider);

        // Register source output for each XTUI file individually
        context.RegisterSourceOutput (xamlWithCompilation, (spc, item) =>
        {
            var file = item.Left.Left;
            bool hasReference = item.Left.Right;
            Compilation compilation = item.Right;

            if (!hasReference)
            {
                // Terminal.Gui not referenced by this project; skip generation
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace (file.Content))
                {
                    return;
                }

                // Parse with the legacy XML loader for code generation
                ElementNode xmlRoot = XtuiLoaderXml.LoadFromString (file.Content);

                // Also parse with the XamlX-backed loader for compatibility testing, but never use its output for generation
                try
                {
                    _ = XtuiLoader.LoadFromString (file.Content);
                }
                catch (Exception ex)
                {
                    // Informational only: XamlX parser failures should not block generation
                    var descriptor = new DiagnosticDescriptor (
                        id: "XTUI004",
                        title: "XamlX parser failed (non-blocking)",
                        messageFormat: "XamlX parser failed for '{0}': {1}. Code generation uses XtuiLoaderXml.",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Info,
                        isEnabledByDefault: true);

                    Diagnostic diagnostic = Diagnostic.Create (
                        descriptor,
                        Location.None,
                        file.Path,
                        ex.Message);

                    spc.ReportDiagnostic (diagnostic);
                }
                string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiGenerated";

                // Check for 'class' attribute on root element and use it as class name (similar to x:Class in XAML)
                string className = xmlRoot.Attributes.TryGetValue("class", out string? classAttr) && !string.IsNullOrEmpty(classAttr)
                    ? classAttr
                    : fileName;

                // Remove the 'class' attribute so it's not treated as a property
                xmlRoot.Attributes.Remove("class");

                // Try to find the partial class in the compilation to get the actual namespace and class name
                (string namespaceName, string actualClassName) = FindPartialClass (compilation, className);

                GeneratorFactory generatorFactory = new GeneratorFactory ();
                Generator generator = generatorFactory.GetGenerator (xmlRoot.ElementTypeName);
                string code = generator.GenerateClass (xmlRoot, namespaceName, className, generatorFactory);

                // Create a unique hint name that includes a hash of the full path to avoid collisions
                // when multiple .xtui files have the same filename in different directories
                string pathHash = Math.Abs(file.Path.GetHashCode()).ToString("X8");
                string generatedFileName = className + ".g.cs";
                string uniqueHintName = $"{className}_{pathHash}.g.cs";
                spc.AddSource (uniqueHintName, SourceText.From (code, Encoding.UTF8));

                // Record bookkeeping info for the generated file (input .xtui -> generated file metadata)
                try
                {
                    GeneratedFileBookkeeping.Record (file.Path, generatedFileName, namespaceName, className, spc);
                }
                catch
                {
                    // Non-fatal: bookkeeping best-effort, don't let it break generation
                }
            }
            catch (InvalidOperationException ex)
            {
                // InvalidOperationException typically indicates a parsing or validation error
                string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiError";
                Diagnostic diagnostic = Diagnostic.Create (
                    new DiagnosticDescriptor (
                        id: "XTUI001",
                        title: "XTUI Parsing Error",
                        messageFormat: "Error parsing XTUI file '{0}': {1}",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    file.Path,
                    ex.Message);

                spc.ReportDiagnostic (diagnostic);

                // Also generate a comment file so the error appears in generated files list
                string hint = fileName + "_Error";
                StringBuilder errorComment = new StringBuilder ();
                errorComment.AppendLine ("// XTUI Parsing Error");
                errorComment.AppendLine ($"// File: {file.Path}");
                errorComment.AppendLine ($"// Error: {EscapeForComment (ex.Message)}");
                errorComment.AppendLine ("//");
                errorComment.AppendLine ("// This file was not generated due to the error above.");
                errorComment.AppendLine ("// Please fix the XTUI syntax and rebuild.");

                spc.AddSource (hint + ".g.cs", SourceText.From (errorComment.ToString (), Encoding.UTF8));
            }
            catch (System.Xml.XmlException ex)
            {
                // Treat raw XML parse exceptions as XTUI parsing errors as well.
                string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiError";
                Diagnostic diagnostic = Diagnostic.Create (
                    new DiagnosticDescriptor (
                        id: "XTUI001",
                        title: "XTUI Parsing Error",
                        messageFormat: "Error parsing XTUI file '{0}': {1}",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    file.Path,
                    ex.Message);

                spc.ReportDiagnostic (diagnostic);

                // Also generate a comment file so the error appears in generated files list
                string hint = fileName + "_Error";
                StringBuilder errorComment = new StringBuilder ();
                errorComment.AppendLine ("// XTUI Parsing Error");
                errorComment.AppendLine ($"// File: {file.Path}");
                errorComment.AppendLine ($"// Error: {EscapeForComment (ex.Message)}");
                errorComment.AppendLine ("//");
                errorComment.AppendLine ("// This file was not generated due to the error above.");
                errorComment.AppendLine ("// Please fix the XTUI syntax and rebuild.");

                spc.AddSource (hint + ".g.cs", SourceText.From (errorComment.ToString (), Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // If the root cause is a parsing/validation error (XmlException or InvalidOperationException),
                // report it as XTUI001 so tests and users see a parsing diagnostic.
                Exception root = ex;
                while (root.InnerException != null)
                {
                    root = root.InnerException;
                }

                if (root is System.Xml.XmlException || root is InvalidOperationException ||
                    root.Message?.Contains("hexadecimal value 0x3C") == true ||
                    root.Message?.Contains("Name cannot begin with the '<'") == true)
                {
                    string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiError";
                    Diagnostic diagnostic = Diagnostic.Create (
                        new DiagnosticDescriptor (
                            id: "XTUI001",
                            title: "XTUI Parsing Error",
                            messageFormat: "Error parsing XTUI file '{0}': {1}",
                            category: "Terminal.Gui.Xtui",
                            defaultSeverity: DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None,
                        file.Path,
                        root.Message);

                    spc.ReportDiagnostic (diagnostic);

                    // Also generate a comment file so the error appears in generated files list
                    string hint = fileName + "_Error";
                    StringBuilder errorComment = new StringBuilder ();
                    errorComment.AppendLine ("// XTUI Parsing Error");
                    errorComment.AppendLine ($"// File: {file.Path}");
                    errorComment.AppendLine ($"// Error: {EscapeForComment (root.Message)}");
                    errorComment.AppendLine ("//");
                    errorComment.AppendLine ("// This file was not generated due to the error above.");
                    errorComment.AppendLine ("// Please fix the XTUI syntax and rebuild.");

                    spc.AddSource (hint + ".g.cs", SourceText.From (errorComment.ToString (), Encoding.UTF8));
                }
                else
                {
                    // Unexpected errors
                    string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiError";
                    Diagnostic diagnostic = Diagnostic.Create (
                        new DiagnosticDescriptor (
                            id: "XTUI002",
                            title: "XTUI Generation Error",
                            messageFormat: "Unexpected error generating code from XTUI file '{0}': {1}",
                            category: "Terminal.Gui.Xtui",
                            defaultSeverity: DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None,
                        file.Path,
                        ex.Message);

                    spc.ReportDiagnostic (diagnostic);

                    // Generate detailed error comment file
                    string hint = fileName + "_Error";
                    StringBuilder errorComment = new StringBuilder ();
                    errorComment.AppendLine ("// XTUI Generation Error");
                    errorComment.AppendLine ($"// File: {file.Path}");
                    errorComment.AppendLine ($"// Error: {EscapeForComment (ex.Message)}");
                    if (ex.StackTrace != null)
                    {
                        errorComment.AppendLine ("// Stack Trace:");
                        foreach (string? line in ex.StackTrace.Split ('\n'))
                        {
                            errorComment.AppendLine ($"//   {EscapeForComment (line.TrimEnd ())}");
                        }
                    }
                    errorComment.AppendLine ("//");
                    errorComment.AppendLine ("// This file was not generated due to the error above.");

                    spc.AddSource (hint + ".g.cs", SourceText.From (errorComment.ToString (), Encoding.UTF8));
                }
            }
        });
    }

    private static string EscapeForComment (string s) => s.Replace ("*/", "*\\/");

    /// <summary>
    /// Finds a partial class in the compilation that matches the expected class name.
    /// Returns the namespace and class name from the found class, or defaults if not found.
    /// </summary>
    private static (string namespaceName, string className) FindPartialClass (Compilation compilation, string expectedClassName)
    {
        // Search through all syntax trees in the compilation
        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            SyntaxNode root = syntaxTree.GetRoot ();
            SemanticModel semanticModel = compilation.GetSemanticModel (syntaxTree);

            // Find all class declarations
            System.Collections.Generic.IEnumerable<ClassDeclarationSyntax> classDeclarations = root.DescendantNodes ()
                .OfType<ClassDeclarationSyntax> ();

            foreach (ClassDeclarationSyntax classDecl in classDeclarations)
            {
                // Check if it's a partial class with matching name
                if (classDecl.Identifier.Text == expectedClassName &&
                    classDecl.Modifiers.Any (m => m.IsKind (Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)))
                {
                    // Get the symbol to extract the namespace
                    ISymbol? classSymbol = semanticModel.GetDeclaredSymbol (classDecl);
                    if (classSymbol != null)
                    {
                        string namespaceName = classSymbol.ContainingNamespace?.ToDisplayString () ?? "Global";
                        return (namespaceName, classSymbol.Name);
                    }
                }
            }
        }

        // If no partial class found, use the filename as class name and a default namespace
        return ("Generated", expectedClassName);
    }
}