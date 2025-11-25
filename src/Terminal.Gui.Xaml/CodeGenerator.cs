using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Terminal.Gui.Xaml.Generators;

namespace Terminal.Gui.Xaml;

[Generator]
public class CodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find additional files that end with .xaml
        var xamlFiles = context.AdditionalTextsProvider
            .Where(at => at.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
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
        var hasTerminalGuiReference = context.CompilationProvider
            .Select((compilation, _) => 
                compilation.GetTypeByMetadataName("Terminal.Gui.Views.Window") != null);

        // Combine each XAML file with the Terminal.Gui reference check
        var xamlWithReferenceCheck = xamlFiles.Combine(hasTerminalGuiReference);

        // Register source output for each XAML file individually
        context.RegisterSourceOutput(xamlWithReferenceCheck, (spc, item) =>
        {
            var file = item.Left;
            var hasReference = item.Right;

            if (!hasReference)
            {
                // Terminal.Gui not referenced by this project; skip generation
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(file.Content)) return;

                var root = XamlLoader.LoadFromString(file.Content);
                var fileName = Path.GetFileNameWithoutExtension(file.Path) ?? "XamlGenerated";
                var className = fileName;
                //var generator = new WindowGenerator_Old();
                var generator = new WindowGenerator();
                //var code = generator.GenerateClass(root, "Xaml", className);
                var code = generator.Generate(root, null);

                spc.AddSource(className + ".g.cs", SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception exFile)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.Path) ?? "XamlError";
                var hint = fileName + "_Error";
                var safe = $"// Error generating from XAML file '{file.Path}': {EscapeForComment(exFile.Message)}";
                spc.AddSource(hint + ".g.cs", SourceText.From(safe, Encoding.UTF8));
            }
        });
    }

    private static string EscapeForComment(string s) => s.Replace("*/", "*\\/");
}