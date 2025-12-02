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

                ElementNode root = XtuiLoader.LoadFromString (file.Content);
                string fileName = Path.GetFileNameWithoutExtension (file.Path) ?? "XtuiGenerated";

                // Try to find the partial class in the compilation to get the actual namespace and class name
                (string namespaceName, string className) = FindPartialClass (compilation, fileName);

                GeneratorFactory generatorFactory = new GeneratorFactory ();
                Generator generator = generatorFactory.GetGenerator (root.ElementTypeName);
                string code = generator.GenerateClass (root, namespaceName, className, generatorFactory);

                spc.AddSource (className + ".g.cs", SourceText.From (code, Encoding.UTF8));
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