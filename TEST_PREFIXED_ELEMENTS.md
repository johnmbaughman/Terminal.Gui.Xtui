# Testing Prefixed Element Support

## How It Works

The XtuiLoader already supports prefixed elements like `<mah:MetroWindow>` because the .NET XML parser automatically resolves namespace prefixes.

### Example 1: MahApps Metro Controls

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:mah="clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro">
    <mah:MetroWindow Title="My App" Width="800" Height="600">
        <Label Text="Hello from MahApps!" />
    </mah:MetroWindow>
</Window>
```

**What happens:**
1. XML parser sees `xmlns:mah="clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro"`
2. When it encounters `<mah:MetroWindow>`, it resolves:
   - `el.Name.Prefix` = "mah"
   - `el.Name.LocalName` = "MetroWindow"
   - `el.Name.NamespaceName` = "clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro"
3. XtuiLoader calls `MapNamespaceUri("clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro")`
4. Returns: `"MahApps.Metro.Controls"`
5. Final ElementTypeName: `"MahApps.Metro.Controls.MetroWindow"`

### Example 2: Custom ViewModels

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:vm="clr-namespace:MyApp.ViewModels">
    <Label Id="_statusLabel" Text="Status" />
    <!-- In the future, could support: -->
    <!-- <vm:LoginViewModel x:Key="loginVM" /> -->
</Window>
```

**What happens:**
1. XML parser resolves `xmlns:vm="clr-namespace:MyApp.ViewModels"`
2. Any element with `vm:` prefix gets namespace URI = "clr-namespace:MyApp.ViewModels"
3. XtuiLoader maps it to C# namespace: `"MyApp.ViewModels"`
4. Using directive generated: `using MyApp.ViewModels;`

### Example 3: Multiple Custom Libraries

```xml
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:controls="clr-namespace:MyCompany.Controls"
        xmlns:charts="clr-namespace:MyCompany.Charts;assembly=Charts"
        xmlns:vm="clr-namespace:MyApp.ViewModels">
    
    <controls:CustomButton Id="_saveBtn" Text="Save" />
    <charts:LineChart Id="_chart" Width="50" Height="20" />
    <Label Text="{Binding vm:MainViewModel.Status}" />
</Window>
```

**Generated using directives:**
```csharp
using Terminal.Gui.Views;
using Terminal.Gui.ViewBase;
using MyCompany.Controls;
using MyCompany.Charts;
using MyApp.ViewModels;
```

## Technical Details

### XML Namespace Resolution (Automatic)

The .NET `System.Xml.Linq.XElement` class automatically handles namespace prefix resolution:

```csharp
// Given: <mah:MetroWindow xmlns:mah="clr-namespace:MahApps.Metro.Controls">
XElement el = ...;

// These properties are automatically populated by XML parser:
el.Name.Prefix           // "mah"
el.Name.LocalName        // "MetroWindow"
el.Name.NamespaceName    // "clr-namespace:MahApps.Metro.Controls"
```

### XtuiLoader Processing

```csharp
// Current implementation (lines 148-160 in XtuiLoader.cs)
string nsUri = el.Name.NamespaceName;        // Get resolved namespace URI
string localName = el.Name.LocalName;         // Get element name without prefix
string csNamespace = MapNamespaceUri(nsUri);  // Map to C# namespace
node.ElementTypeName = csNamespace + "." + localName;  // Combine
```

### MapNamespaceUri Logic

```csharp
private static string MapNamespaceUri(string uri)
{
    if (string.IsNullOrEmpty(uri))
        return "Terminal.Gui.Views";
    
    if (uri == "http://schemas.terminal.gui/xtui")
        return "Terminal.Gui.Views";
    
    // Parse clr-namespace: declarations
    if (uri.StartsWith("clr-namespace:"))
    {
        string nsDeclaration = uri.Substring("clr-namespace:".Length);
        int assemblyIndex = nsDeclaration.IndexOf(";");
        if (assemblyIndex > 0)
            return nsDeclaration.Substring(0, assemblyIndex);  // Extract namespace only
        return nsDeclaration;
    }
    
    // Fallback: use URI as-is
    return uri;
}
```

## Current Support Status

✅ **Fully Supported:**
- Prefixed elements (e.g., `<mah:MetroWindow>`)
- clr-namespace syntax with and without assembly
- Multiple namespace prefixes in same file
- Namespace inheritance to child elements
- Automatic using directive generation

⚠️ **Limitations:**
- The `ResolveElementTypeName()` helper method (lines 45-88) is currently unused
- It was created for potential future enhancements but isn't needed
- XML parser already handles all prefix resolution automatically

🎯 **Best Practice:**
Always use XAML-style clr-namespace syntax for custom types:
```xml
xmlns:prefix="clr-namespace:Namespace.Name"
xmlns:prefix="clr-namespace:Namespace.Name;assembly=AssemblyName"
```

## Testing

To verify this works, you can:

1. Create a test XTUI file with prefixed elements
2. Run the generator
3. Check that the generated code has correct:
   - Using directives for all custom namespaces
   - Fully qualified type names (namespace + class name)
   - Proper instantiation of prefixed types

Example test case for unit tests:

```csharp
[Fact]
public void XtuiLoader_WithPrefixedElement_ResolvesCorrectly()
{
    string xtui = @"
        <Window xmlns='http://schemas.terminal.gui/xtui'
                xmlns:mah='clr-namespace:MahApps.Metro.Controls'>
            <mah:MetroWindow />
        </Window>";
    
    var root = XtuiLoader.LoadFromString(xtui);
    var metroWindow = root.Children[0];
    
    Assert.Equal("MahApps.Metro.Controls.MetroWindow", metroWindow.ElementTypeName);
}
```
