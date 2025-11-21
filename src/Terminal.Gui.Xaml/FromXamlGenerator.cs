using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Terminal.Gui.Xaml
{
    [Generator]
    public class FromXamlGenerator : IIncrementalGenerator
    {
        private const string OutputClassString =
            """
            Hello from FromXamlGenerator!
            """;
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx
                .AddSource("FromXamlGenerator.gs.cs", SourceText.From(OutputClassString, Encoding.UTF8)));
        }
    }
}