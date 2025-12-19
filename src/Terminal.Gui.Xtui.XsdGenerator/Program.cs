using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Terminal.Gui;
using Terminal.Gui.ViewBase;

namespace Terminal.Gui.Xtui.XsdGenerator;

class Program
{
    static void Main(string[] args)
    {
        string outputPath = args.Length > 0 
            ? args[0] 
            : Path.Combine("..", "Terminal.Gui.Xtui", "Terminal.Gui.Xtui.xsd");

        Console.WriteLine("Terminal.Gui XTUI XSD Generator");
        Console.WriteLine($"Output: {outputPath}");
        Console.WriteLine();

        var generator = new XsdGenerator();
        string xsd = generator.Generate();

        File.WriteAllText(outputPath, xsd);
        Console.WriteLine($"Generated XSD with {generator.ViewTypesCount} view types");
        Console.WriteLine("Done!");
    }
}

class XsdGenerator
{
    private readonly Assembly _terminalGuiAssembly;
    private readonly Type _viewBaseType;
    private readonly List<Type> _viewTypes = new();
    private readonly Dictionary<Type, XElement> _generatedTypes = new();
    private readonly HashSet<string> _definedSimpleTypes = new();
    private XDocument? _xmlDocumentation;

    public int ViewTypesCount => _viewTypes.Count;

    public XsdGenerator()
    {
        _terminalGuiAssembly = typeof(View).Assembly;
        _viewBaseType = typeof(View);
        LoadXmlDocumentation();
        DiscoverViewTypes();
    }

    private void LoadXmlDocumentation()
    {
        try
        {
            // Try to find the XML documentation file next to the Terminal.Gui assembly
            var assemblyLocation = _terminalGuiAssembly.Location;
            var xmlPath = Path.ChangeExtension(assemblyLocation, ".xml");
            
            if (File.Exists(xmlPath))
            {
                _xmlDocumentation = XDocument.Load(xmlPath);
                Console.WriteLine($"Loaded XML documentation from: {xmlPath}");
            }
            else
            {
                Console.WriteLine($"Warning: XML documentation not found at {xmlPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load XML documentation: {ex.Message}");
        }
    }

    private void DiscoverViewTypes()
    {
        var types = _terminalGuiAssembly.GetExportedTypes()
            .Where(t => t.IsClass 
                && !t.IsAbstract 
                && !t.IsGenericType // Exclude generic types like NumericUpDown<T>
                && _viewBaseType.IsAssignableFrom(t))
            .OrderBy(t => t.Name);

        _viewTypes.AddRange(types);
    }

    public string Generate()
    {
        var xsd = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            CreateSchema()
        );

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(false), // UTF-8 without BOM
            OmitXmlDeclaration = false
        };

        using var memoryStream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
        {
            xsd.Save(xmlWriter);
        }
        
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    private XElement CreateSchema()
    {
        XNamespace xs = "http://www.w3.org/2001/XMLSchema";
        XNamespace tns = "http://schemas.terminal.gui/xtui";

        var schema = new XElement(xs + "schema",
            new XAttribute("targetNamespace", tns),
            new XAttribute("elementFormDefault", "qualified"),
            new XAttribute(XNamespace.Xmlns + "xs", xs),
            new XAttribute("xmlns", tns)
        );

        // Add simple types for common patterns
        schema.Add(CreateSimpleTypes(xs));

        // Add attribute groups
        schema.Add(CreateAttributeGroups(xs));

        // Add view elements
        foreach (var viewType in _viewTypes)
        {
            schema.Add(CreateViewElement(viewType, xs));
        }

        return schema;
    }

    private IEnumerable<XElement> CreateSimpleTypes(XNamespace xs)
    {
        yield return new XElement(xs + "simpleType",
            new XAttribute("name", "PosExpression"),
            new XElement(xs + "annotation",
                new XElement(xs + "documentation",
                    "Position expression: integer, percentage (50%), or method call like {Center}, {AnchorEnd}, {Right _viewName + 1}"
                )
            ),
            new XElement(xs + "restriction",
                new XAttribute("base", "xs:string")
            )
        );

        yield return new XElement(xs + "simpleType",
            new XAttribute("name", "DimExpression"),
            new XElement(xs + "annotation",
                new XElement(xs + "documentation",
                    "Dimension expression: integer, percentage (50%), or method call like {Fill}, {Auto}, {Percent 75}"
                )
            ),
            new XElement(xs + "restriction",
                new XAttribute("base", "xs:string")
            )
        );

        // CheckState enum
        _definedSimpleTypes.Add("CheckState");
        yield return new XElement(xs + "simpleType",
            new XAttribute("name", "CheckState"),
            new XElement(xs + "restriction",
                new XAttribute("base", "xs:string"),
                new XElement(xs + "enumeration", new XAttribute("value", "UnChecked")),
                new XElement(xs + "enumeration", new XAttribute("value", "Checked")),
                new XElement(xs + "enumeration", new XAttribute("value", "None"))
            )
        );

        // TextAlignment enum
        _definedSimpleTypes.Add("TextAlignment");
        yield return new XElement(xs + "simpleType",
            new XAttribute("name", "TextAlignment"),
            new XElement(xs + "restriction",
                new XAttribute("base", "xs:string"),
                new XElement(xs + "enumeration", new XAttribute("value", "Left")),
                new XElement(xs + "enumeration", new XAttribute("value", "Right")),
                new XElement(xs + "enumeration", new XAttribute("value", "Centered")),
                new XElement(xs + "enumeration", new XAttribute("value", "Justified"))
            )
        );
    }

    private IEnumerable<XElement> CreateAttributeGroups(XNamespace xs)
    {
        yield return new XElement(xs + "attributeGroup",
            new XAttribute("name", "ViewAttributes"),
            new XElement(xs + "attribute", new XAttribute("name", "Id"), new XAttribute("type", "xs:string")),
            new XElement(xs + "attribute", new XAttribute("name", "X"), new XAttribute("type", "PosExpression")),
            new XElement(xs + "attribute", new XAttribute("name", "Y"), new XAttribute("type", "PosExpression")),
            new XElement(xs + "attribute", new XAttribute("name", "Width"), new XAttribute("type", "DimExpression")),
            new XElement(xs + "attribute", new XAttribute("name", "Height"), new XAttribute("type", "DimExpression")),
            new XElement(xs + "attribute", new XAttribute("name", "Visible"), new XAttribute("type", "xs:boolean")),
            new XElement(xs + "attribute", new XAttribute("name", "Enabled"), new XAttribute("type", "xs:boolean")),
            new XElement(xs + "attribute", new XAttribute("name", "CanFocus"), new XAttribute("type", "xs:boolean")),
            new XElement(xs + "attribute", new XAttribute("name", "TabIndex"), new XAttribute("type", "xs:int")),
            new XElement(xs + "attribute", new XAttribute("name", "TabStop"), new XAttribute("type", "xs:boolean")),
            new XElement(xs + "attribute", new XAttribute("name", "Text"), new XAttribute("type", "xs:string"))
        );
    }

    private XElement CreateViewElement(Type viewType, XNamespace xs)
    {
        var element = new XElement(xs + "element",
            new XAttribute("name", viewType.Name)
        );

        var complexType = new XElement(xs + "complexType");
        
        // Allow child elements for container types
        if (IsContainerType(viewType))
        {
            complexType.Add(new XElement(xs + "choice",
                new XAttribute("minOccurs", "0"),
                new XAttribute("maxOccurs", "unbounded"),
                new XElement(xs + "any",
                    new XAttribute("processContents", "lax")
                )
            ));
        }

        // Add attributes
        complexType.Add(new XElement(xs + "attributeGroup",
            new XAttribute("ref", "ViewAttributes")
        ));

        // Add type-specific attributes
        var specificAttributes = GetTypeSpecificAttributes(viewType, xs);
        foreach (var attr in specificAttributes)
        {
            complexType.Add(attr);
        }

        // Allow any additional attributes
        complexType.Add(new XElement(xs + "anyAttribute",
            new XAttribute("processContents", "lax")
        ));

        element.Add(complexType);
        return element;
    }

    private bool IsContainerType(Type type)
    {
        return type.Name == "Window" ||
               type.Name == "FrameView" ||
               type.Name == "Dialog" ||
               type.Name == "View" ||
               type.Name == "Panel" ||
               type.Name == "ScrollView" ||
               type.Name == "TabView";
    }

    private IEnumerable<XElement> GetTypeSpecificAttributes(Type viewType, XNamespace xs)
    {
        var properties = viewType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var prop in properties)
        {
            if (!prop.CanWrite) continue;
            if (IsCommonAttribute(prop.Name)) continue;

            var attrType = GetXsdType(prop.PropertyType);
            if (attrType != null)
            {
                var attr = new XElement(xs + "attribute",
                    new XAttribute("name", prop.Name),
                    new XAttribute("type", attrType)
                );

                // Add documentation if available
                var doc = GetPropertyDocumentation(prop);
                if (!string.IsNullOrEmpty(doc))
                {
                    attr.AddFirst(new XElement(xs + "annotation",
                        new XElement(xs + "documentation", doc)
                    ));
                }

                yield return attr;
            }
        }
    }

    private bool IsCommonAttribute(string propertyName)
    {
        return propertyName is "X" or "Y" or "Width" or "Height" or 
               "Visible" or "Enabled" or "CanFocus" or "TabIndex" or 
               "TabStop" or "Text" or "Id";
    }

    private string? GetXsdType(Type propertyType)
    {
        if (propertyType == typeof(bool)) return "xs:boolean";
        if (propertyType == typeof(int)) return "xs:int";
        if (propertyType == typeof(string)) return "xs:string";
        if (propertyType == typeof(double) || propertyType == typeof(float)) return "xs:decimal";
        
        // Handle enums
        if (propertyType.IsEnum)
        {
            var enumName = propertyType.Name;
            if (_definedSimpleTypes.Contains(enumName))
            {
                return enumName;
            }
        }

        // Handle Pos/Dim types
        if (propertyType.Name == "Pos") return "PosExpression";
        if (propertyType.Name == "Dim") return "DimExpression";

        return "xs:string"; // Default to string
    }

    private string? GetPropertyDocumentation(PropertyInfo property)
    {
        if (_xmlDocumentation == null)
            return null;

        try
        {
            var declaringType = property.DeclaringType;
            if (declaringType == null)
                return null;

            // Try multiple namespace variations since Terminal.Gui types may be in different namespaces
            var namespaceVariations = new[]
            {
                declaringType.FullName, // Try actual full name first
                $"Terminal.Gui.Views.{declaringType.Name}", // Try Views namespace
                $"Terminal.Gui.{declaringType.Name}" // Try base namespace
            };

            foreach (var typeFullName in namespaceVariations)
            {
                var memberName = $"P:{typeFullName}.{property.Name}";
                var memberElement = _xmlDocumentation.XPathSelectElement($"//member[@name='{memberName}']");
                
                if (memberElement != null)
                {
                    var summaryElement = memberElement.Element("summary");
                    
                    if (summaryElement != null)
                    {
                        // Get the raw XML content (not just .Value which strips tags)
                        var summary = summaryElement.ToString();
                        summary = CleanDocumentation(summary);
                        return summary;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error extracting documentation for {property.Name}: {ex.Message}");
        }

        return null;
    }

    private string CleanDocumentation(string documentation)
    {
        if (string.IsNullOrWhiteSpace(documentation))
            return string.Empty;

        var cleaned = documentation;

        // First, normalize whitespace within the XML to handle multi-line tags
        cleaned = Regex.Replace(cleaned, @"\s+", " ");

        // Convert <see cref="T:Type"/> to just the type name
        cleaned = Regex.Replace(cleaned, @"<see cref=""[TFMP]:([\w.]+)""\s*/?>", m =>
        {
            var fullName = m.Groups[1].Value;
            var parts = fullName.Split('.');
            return parts[^1]; // Return just the last part (type/member name)
        });

        // Convert <see langword="value"/> to just the value
        cleaned = Regex.Replace(cleaned, @"<see langword=""([^""]+)""\s*/?>", "$1");

        // Convert <paramref name="param"/> to param
        cleaned = Regex.Replace(cleaned, @"<paramref name=""([^""]+)""\s*/?>", "$1");

        // Remove any remaining XML tags
        cleaned = Regex.Replace(cleaned, @"<[^>]+>", string.Empty);
        
        // Final cleanup - normalize whitespace again
        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        
        return cleaned.Trim();
    }
}
