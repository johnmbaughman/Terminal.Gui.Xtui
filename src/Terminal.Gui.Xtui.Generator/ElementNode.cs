using System.Collections.Generic;

namespace Terminal.Gui.Xtui.Generator;

/// <summary>
/// Represents a node in the parsed XTUI (XML) tree.
/// Each node contains the element type name, attribute dictionary,
/// a list of child nodes and optional inner text.
/// </summary>
public class ElementNode
{
    /// <summary>
    /// The XTUI element name (for example, "Window", "Label", "Button").
    /// </summary>
    public string ElementTypeName { get; set; } = string.Empty;

    /// <summary>
    /// The attributes present on the element as a dictionary of name → value.
    /// </summary>
    public Dictionary<string, string> Attributes { get; } = new ();

    /// <summary>
    /// Child elements of this node.
    /// </summary>
    public List<ElementNode> Children { get; } = [];

    /// <summary>
    /// Optional inner text of the element (trimmed). Null when not present.
    /// </summary>
    public string? InnerText { get; set; }

    /// <summary>
    /// XML namespaces defined in this element or ancestors.
    /// Key is prefix (empty string for default namespace), value is the namespace URI (used as C# namespace).
    /// </summary>
    public Dictionary<string, string> Namespaces { get; set; } = new ();
}
