using System;
using System.Text;

namespace Terminal.Gui.Xaml.Generators;

internal class WindowGenerator_Old
{
    public string GenerateClass(ElementNode root, string @namespace, string className)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Terminal.Gui.Views;");
        sb.AppendLine($"namespace {@namespace}");
        sb.AppendLine("{");
        sb.AppendLine($"    public partial class {className} : Terminal.Gui.Views.Window");
        sb.AppendLine("    {");
        sb.AppendLine($"        public {className}()");
        sb.AppendLine("        {");
        
        int id = 0;

        if (root != null)
        {
            // if top-level is a View, emit its children into 'root'
            if (root.Name.Equals("Window", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var child in root.Children)
                    GenerateNode(child, "this", sb, ref id, 12);
            }
            else
            {
                // single top-level element -> add it to root
                GenerateNode(root, "this", sb, ref id, 12);
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private void GenerateNode(ElementNode node, string parentVar, StringBuilder sb, ref int id, int indent)
    {
        if (node == null) return;
        var ind = new string(' ', indent);

        if (node.Name.Equals("Label", StringComparison.OrdinalIgnoreCase))
        {
            var text = node.Attributes.ContainsKey("Text") ? node.Attributes["Text"] : node.InnerText ?? "";
            var varName = $"label{id++}";
            sb.AppendLine($"{ind}var {varName} = new Label();");
            sb.AppendLine($"{ind}{varName}.Text = \"{Escape(text)}\";");
            sb.AppendLine($"{ind}{parentVar}.Add({varName});");
        }
        else if (node.Name.Equals("Button", StringComparison.OrdinalIgnoreCase))
        {
            var text = node.Attributes.ContainsKey("Text") ? node.Attributes["Text"] : node.InnerText ?? "";
            var varName = $"button{id++}";
            sb.AppendLine($"{ind}var {varName} = new Button();");
            sb.AppendLine($"{ind}{varName}.Text = \"{Escape(text)}\";");
            sb.AppendLine($"{ind}{parentVar}.Add({varName});");
        }
        else if (node.Name.Equals("View", StringComparison.OrdinalIgnoreCase))
        {
            var varName = $"view{id++}";
            sb.AppendLine($"{ind}var {varName} = new Window();");
            sb.AppendLine($"{ind}{parentVar}.Add({varName});");
            foreach (var child in node.Children)
                GenerateNode(child, varName, sb, ref id, indent + 4);
        }
        else
        {
            // unknown element -> treat as container View
            var varName = $"view{id++}";
            sb.AppendLine($"{ind}var {varName} = new Window();");
            sb.AppendLine($"{ind}{parentVar}.Add({varName});");
            foreach (var child in node.Children)
                GenerateNode(child, varName, sb, ref id, indent + 4);
        }
    }

    private string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";
}