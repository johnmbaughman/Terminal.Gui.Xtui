using System.Linq;
using Terminal.Gui.Xtui;
using Terminal.Gui.Xtui.Generators;
using Xunit;

namespace Terminal.Gui.Xtui.Tests.Generators
{
    public class ButtonGeneratorTests
    {
        [Fact]
        public void GenerateButton_CreatesVariableAndInitializer()
        {
            var node = new ElementNode { ElementTypeName = "Button" };
            node.Attributes["Text"] = "Click Me";

            var generator = new ButtonGenerator();
            var factory = new GeneratorFactory();

            var statements = generator.GenerateStatements(node, "button0", factory);
            var generated = string.Concat(statements.Select(s => s.ToString()));

            Assert.Contains("new Button", generated);
            Assert.Contains("Text=\"Click Me\"", generated);
        }
    }
}
