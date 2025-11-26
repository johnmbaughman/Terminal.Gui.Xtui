# XTUI IntelliSense Support

This document explains how to enable IntelliSense/code completion for `.xtui` files in Visual Studio, Visual Studio Code, and JetBrains Rider.

## Overview

XTUI files are XML-based markup files for defining Terminal.Gui user interfaces. The package includes an XML Schema Definition (XSD) file that enables IntelliSense support in IDEs.

## Visual Studio

### Automatic Setup (When Using NuGet Package)

When you reference the `Terminal.Gui.Xtui` NuGet package, the schema is automatically included and your `.xtui` files should get IntelliSense support.

### Manual Setup (For Development)

1. Ensure your `.xtui` files include the XML namespace declaration at the top:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://schemas.terminal.gui/xtui ../../Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd">
    <!-- Your UI elements here -->
</Window>
```

2. The `xsi:schemaLocation` attribute should point to the relative path of the `Terminal.Gui.Xtui.xsd` file.

3. Visual Studio will automatically provide IntelliSense for:
   - Element names (Window, Label, Button, etc.)
   - Attribute names (Text, X, Y, Width, Height, etc.)
   - Documentation tooltips for elements and attributes

### Configuring XML Editor Association

If Visual Studio doesn't automatically recognize `.xtui` files as XML:

1. Right-click a `.xtui` file in Solution Explorer
2. Select "Open With..."
3. Choose "XML (Text) Editor"
4. Click "Set as Default" (optional)
5. Click OK

## Visual Studio Code

### Prerequisites

Install the **XML Language Support by Red Hat** extension:
1. Open VS Code
2. Press `Ctrl+Shift+X` (or `Cmd+Shift+X` on Mac) to open Extensions
3. Search for "XML"
4. Install **XML** by Red Hat (extension ID: `redhat.vscode-xml`)

### Workspace Setup

For the best experience, add these files to your workspace root (this repo already includes them in `.vscode/`):

**`.vscode/settings.json`:**
```json
{
  "xml.fileAssociations": [
    {
      "pattern": "**/*.xtui",
      "systemId": "Terminal.Gui.Xtui.xsd"
    }
  ],
  "files.associations": {
    "*.xtui": "xml"
  }
}
```

**`.vscode/extensions.json`:**
```json
{
  "recommendations": [
    "redhat.vscode-xml"
  ]
}
```

### Schema Configuration

#### Option 1: Using schemaLocation (Recommended)

Ensure your `.xtui` files include the XML declaration with schema location:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://schemas.terminal.gui/xtui ../../Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd">
    <!-- Your UI elements here -->
</Window>
```

The relative path in `xsi:schemaLocation` should point to where the `Terminal.Gui.Xtui.xsd` file is located.

#### Option 2: Using Workspace Settings

The workspace `.vscode/settings.json` can map `.xtui` files to the schema:

```json
{
  "xml.fileAssociations": [
    {
      "pattern": "**/*.xtui",
      "systemId": "path/to/Terminal.Gui.Xtui.xsd"
    }
  ]
}
```

Replace `path/to/` with the actual path to the schema file (e.g., `Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd`).

### Verifying It Works

1. Open a `.xtui` file in VS Code
2. Check the bottom-right corner - it should show "XML" as the language mode
3. Start typing `<` inside the Window element - you should see completion suggestions for `Label`, `Button`, etc.
4. Inside an element, start typing an attribute name - you should see suggestions like `Text`, `X`, `Y`, etc.
5. Hover over element or attribute names to see documentation tooltips

### Troubleshooting VS Code

**No IntelliSense appearing:**
- Verify the XML extension is installed and enabled
- Check that the file is recognized as XML (bottom-right corner should say "XML")
- Ensure the schema path in `xsi:schemaLocation` or settings is correct
- Try reloading the window: `Ctrl+Shift+P` → "Developer: Reload Window"

**Schema not found errors:**
- Check the relative path to the XSD file
- Verify the XSD file exists at that location
- Try using an absolute path temporarily to verify it works

**Extension not working:**
- Check Output panel: View → Output → Select "XML Support" from dropdown
- Look for any error messages about schema loading

## JetBrains Rider

### Automatic Setup (When Using NuGet Package)

Rider will automatically recognize the schema when you reference the `Terminal.Gui.Xtui` NuGet package.

### Manual Setup (For Development)

1. Ensure your `.xtui` files include the XML declaration as shown above.

2. If Rider doesn't automatically detect the schema:
   - Go to **Settings** → **Languages & Frameworks** → **Schemas and DTDs** → **XML Schemas**
   - Click **+** to add a new schema
   - Browse to `Terminal.Gui.Xtui.xsd`
   - Set the namespace to `http://schemas.terminal.gui/xtui`
   - Click OK

3. Associate `.xtui` extension with XML files:
   - Go to **Settings** → **Editor** → **File Types**
   - Find "XML files" in the list
   - Add `*.xtui` to the file name patterns
   - Click OK

## IntelliSense Features

Once configured, you'll get:

### 1. **Element Completion**
Start typing `<` and you'll see a list of available elements:
- `Window`
- `Label`
- `Button`
- More controls as they're added

### 2. **Attribute Completion**
Inside an element, start typing and you'll see available attributes:
- `Text` - The text content (string)
- `X`, `Y` - Position (number, percentage, or expression like `{Center}`)
- `Width`, `Height` - Dimensions (number, percentage, or expression like `{Fill}`)
- `Visible`, `Enabled`, `CanFocus` - Boolean properties
- And many more...

### 3. **Documentation**
Hover over any element or attribute to see documentation describing its purpose.

### 4. **Validation**
The IDE will highlight errors if you:
- Use invalid element names
- Use invalid attribute names
- Have malformed XML

## Supported Attributes

Common attributes available on most controls:

### String Properties
- `Text` - Display text
- `Title` - Title text (Window, Dialog)
- `Id` - Identifier

### Position (Pos type)
- `X` - Horizontal position
  - Number: `X="10"`
  - Percentage: `X="50%"`
  - Expression: `X="{Center}"`, `X="{Center + 10}"`, `X="{AnchorEnd - 5}"`
- `Y` - Vertical position (same formats as X)

### Dimensions (Dim type)
- `Width` - Width
  - Number: `Width="20"`
  - Percentage: `Width="80%"`
  - Expression: `Width="{Fill}"`, `Width="{Fill - 5}"`, `Width="{Auto}"`
- `Height` - Height (same formats as Width)

### Boolean Properties
- `Visible` - Visibility state
- `Enabled` - Enabled state
- `CanFocus` - Can receive focus
- `HasFocus` - Currently has focus
- And more...

## Example

```xml
<?xml version="1.0" encoding="utf-8"?>
<Window xmlns="http://schemas.terminal.gui/xtui"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        xsi:schemaLocation="http://schemas.terminal.gui/xtui ../../Terminal.Gui.Xtui/Terminal.Gui.Xtui.xsd">
    
    <!-- Label with fixed position -->
    <Label Text="Hello World" X="10" Y="5" Width="20" Height="1" />
    
    <!-- Button centered horizontally -->
    <Button Text="Click Me" X="{Center}" Y="10" Width="20" Height="3" />
    
    <!-- Label that fills available space -->
    <Label Text="Status" X="0" Y="{AnchorEnd}" Width="{Fill}" Height="1" />
    
</Window>
```

## Troubleshooting

### IntelliSense Not Working

1. **Check XML Declaration**: Ensure your file starts with `<?xml version="1.0" encoding="utf-8"?>`

2. **Check Namespace**: Verify the `xmlns` attribute is set to `http://schemas.terminal.gui/xtui`

3. **Check Schema Location**: Ensure `xsi:schemaLocation` points to the correct relative path to `Terminal.Gui.Xtui.xsd`

4. **Restart IDE**: Sometimes a restart helps the IDE pick up schema changes

5. **Clear Caches**: 
   - **Visual Studio**: Delete `.vs` folder in solution directory
   - **Rider**: File → Invalidate Caches / Restart

### Schema Not Found Errors

If you see errors about the schema not being found:

1. Verify the relative path in `xsi:schemaLocation` is correct
2. Check that `Terminal.Gui.Xtui.xsd` exists at that location
3. For NuGet package users, ensure the package is properly restored

## Adding New Elements

As new Terminal.Gui controls are supported in XTUI, the `Terminal.Gui.Xtui.xsd` file is updated to include them. After updating the package, IntelliSense will automatically reflect the new elements and attributes.
