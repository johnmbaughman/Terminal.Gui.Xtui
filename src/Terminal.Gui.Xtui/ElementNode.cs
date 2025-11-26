using System.Collections.Generic;

namespace Terminal.Gui.Xtui;

public class ElementNode
{
    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    public List<ElementNode> Children { get; } = new List<ElementNode>();
    public string InnerText { get; set; }
}