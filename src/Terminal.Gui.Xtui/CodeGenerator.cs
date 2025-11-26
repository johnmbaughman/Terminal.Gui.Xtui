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
public class CodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find additional files that end with .xtui
        var xamlFiles = context.AdditionalTextsProvider
            .Where(at => at.Path.EndsWith(".xtui", StringComparison.OrdinalIgnoreCase))
            .Select((additionalText, cancellationToken) => 
            {
                var text = additionalText.GetText(cancellationToken);
                return new
                {
                    additionalText.Path,
                    FileName = Path.GetFileNameWithoutExtension(additionalText.Path),
                    Content = text?.ToString() ?? string.Empty
                };
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Content));

        // Check if Terminal.Gui is referenced in the compilation
        // We check for multiple core types to ensure Terminal.Gui is properly referenced
        var requiredTypes = new[]
        {
            "Terminal.Gui.App.Application",
            "Terminal.Gui.Views.Window",
            "Terminal.Gui.Views.View"
        };
        
        var hasTerminalGuiReference = context.CompilationProvider
            .Select((compilation, _) => 
                requiredTypes.Any(typeName => 
                    compilation.GetTypeByMetadataName(typeName) != null));

        // Combine XTUI files with both the reference check and compilation
        var xamlWithCompilation = xamlFiles
            .Combine(hasTerminalGuiReference)
            .Combine(context.CompilationProvider);

        // Register source output for each XTUI file individually
        context.RegisterSourceOutput(xamlWithCompilation, (spc, item) =>
        {
            var file = item.Left.Left;
            var hasReference = item.Left.Right;
            var compilation = item.Right;

            if (!hasReference)
            {
                // Terminal.Gui not referenced by this project; skip generation
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(file.Content)) return;

                var root = XtuiLoader.LoadFromString(file.Content);
                var fileName = Path.GetFileNameWithoutExtension(file.Path) ?? "XtuiGenerated";
                
                // Try to find the partial class in the compilation to get the actual namespace and class name
                var (namespaceName, className) = FindPartialClass(compilation, fileName);
                
                var generatorFactory = new GeneratorFactory();
                var generator = generatorFactory.GetGenerator(root.ElementTypeName);
                var code = generator.GenerateClass(root, namespaceName, className, generatorFactory);

                spc.AddSource(className + ".g.cs", SourceText.From(code, Encoding.UTF8));
            }
            catch (InvalidOperationException ex)
            {
                // InvalidOperationException typically indicates a parsing or validation error
                var fileName = Path.GetFileNameWithoutExtension(file.Path) ?? "XtuiError";
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "XTUI001",
                        title: "XTUI Parsing Error",
                        messageFormat: "Error parsing XTUI file '{0}': {1}",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    file.Path,
                    ex.Message);
                
                spc.ReportDiagnostic(diagnostic);
                
                // Also generate a comment file so the error appears in generated files list
                var hint = fileName + "_Error";
                var errorComment = new StringBuilder();
                errorComment.AppendLine("// XTUI Parsing Error");
                errorComment.AppendLine($"// File: {file.Path}");
                errorComment.AppendLine($"// Error: {EscapeForComment(ex.Message)}");
                errorComment.AppendLine("//");
                errorComment.AppendLine("// This file was not generated due to the error above.");
                errorComment.AppendLine("// Please fix the XTUI syntax and rebuild.");
                
                spc.AddSource(hint + ".g.cs", SourceText.From(errorComment.ToString(), Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // Unexpected errors
                var fileName = Path.GetFileNameWithoutExtension(file.Path) ?? "XtuiError";
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "XTUI002",
                        title: "XTUI Generation Error",
                        messageFormat: "Unexpected error generating code from XTUI file '{0}': {1}",
                        category: "Terminal.Gui.Xtui",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    Location.None,
                    file.Path,
                    ex.Message);
                
                spc.ReportDiagnostic(diagnostic);
                
                // Generate detailed error comment file
                var hint = fileName + "_Error";
                var errorComment = new StringBuilder();
                errorComment.AppendLine("// XTUI Generation Error");
                errorComment.AppendLine($"// File: {file.Path}");
                errorComment.AppendLine($"// Error: {EscapeForComment(ex.Message)}");
                if (ex.StackTrace != null)
                {
                    errorComment.AppendLine("// Stack Trace:");
                    foreach (var line in ex.StackTrace.Split('\n'))
                    {
                        errorComment.AppendLine($"//   {EscapeForComment(line.TrimEnd())}");
                    }
                }
                errorComment.AppendLine("//");
                errorComment.AppendLine("// This file was not generated due to the error above.");
                
                spc.AddSource(hint + ".g.cs", SourceText.From(errorComment.ToString(), Encoding.UTF8));
            }
        });
    }

    private static string EscapeForComment(string s) => s.Replace("*/", "*\\/");

    /// <summary>
    /// Finds a partial class in the compilation that matches the expected class name.
    /// Returns the namespace and class name from the found class, or defaults if not found.
    /// </summary>
    private static (string namespaceName, string className) FindPartialClass(Compilation compilation, string expectedClassName)
    {
        // Search through all syntax trees in the compilation
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var root = syntaxTree.GetRoot();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            // Find all class declarations
            var classDeclarations = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classDeclarations)
            {
                // Check if it's a partial class with matching name
                if (classDecl.Identifier.Text == expectedClassName &&
                    classDecl.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)))
                {
                    // Get the symbol to extract the namespace
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                    if (classSymbol != null)
                    {
                        var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString() ?? "Global";
                        return (namespaceName, classSymbol.Name);
                    }
                }
            }
        }

        // If no partial class found, use the filename as class name and a default namespace
        return ("Generated", expectedClassName);
    }
}