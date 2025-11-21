using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Collections.Immutable;

namespace Terminal.Gui.Xaml;

[Generator]
public class FromXamlGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // find additional files that end with .xaml
        var xamlFiles = context.AdditionalTextsProvider
            .Where(at => at.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Select((additionalText, cancellation) => new
            {
                Path = additionalText.Path,
                Text = additionalText.GetText(cancellation)?.ToString()
            });

        // Collect all XAML files into an array to combine with compilation info
        var collectedXamlFiles = xamlFiles.Collect();

        // Combine compilation information with the collected XAML files so we can check whether
        // the consumer compilation actually references Terminal.Gui before generating code.
        var compilationAndXamls = context.CompilationProvider.Combine(collectedXamlFiles);

        // for each compilation + xaml files, parse and generate source only when Terminal.Gui is available
        context.RegisterSourceOutput(compilationAndXamls, (spc, item) =>
        {
            var compilation = item.Left;
            var xamls = item.Right;

            // Quick check: ensure Terminal.Gui.View type is available in the compilation
            var viewType = compilation.GetTypeByMetadataName("Terminal.Gui.View");
            if (viewType == null)
            {
                // Terminal.Gui not referenced by this project; skip generation
                return;
            }

            try
            {
                if (xamls.IsDefaultOrEmpty) return;

                var generator = new ViewGenerator();

                foreach (var file in xamls)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(file.Text)) continue;

                        var root = XamlLoader.LoadFromString(file.Text);
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file.Path) ?? "XamlGenerated";
                        var className = fileName + "G";
                        var code = generator.GenerateClass(root, "Generated", className);

                        spc.AddSource(className + ".g.cs", SourceText.From(code, Encoding.UTF8));
                    }
                    catch (Exception exFile)
                    {
                        var hint = "FromXamlGenerator_Error";
                        var safe = $"// Error generating from XAML: {EscapeForComment(exFile.Message)}";
                        spc.AddSource(hint + ".g.cs", SourceText.From(safe, Encoding.UTF8));
                    }
                }
            }
            catch (Exception ex)
            {
                var hint = "FromXamlGenerator_Error_Global";
                var safe = $"// Error generating from XAML: {EscapeForComment(ex.Message)}";
                spc.AddSource(hint + ".g.cs", SourceText.From(safe, Encoding.UTF8));
            }
        });
    }

    private static string EscapeForComment(string s) => s?.Replace("*/", "*\\/") ?? "";
}