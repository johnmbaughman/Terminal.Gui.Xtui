# Accessibility in Terminal.Gui.Xaml

Making your terminal applications accessible to all users, including those using screen readers, keyboard-only navigation, and users with visual or motor impairments.

> **Version**: Terminal.Gui.Xaml 1.0+  
> **Last Updated**: September 2025  
> **Applies To**: All Terminal.Gui.Xaml applications

## Why Accessibility Matters

Terminal applications have unique accessibility advantages and challenges:

**✅ Inherent Benefits:**
- Text-based output works well with screen readers
- No mouse dependency by design
- High contrast terminal themes available
- Consistent keyboard navigation patterns

**⚠️ Common Challenges:**
- Complex layouts can confuse screen readers
- Color-only information excludes colorblind users
- Custom controls may lack proper accessibility metadata
- Focus management in modal dialogs

## Core Accessibility Principles

### 1. Keyboard Navigation

**All functionality must be accessible via keyboard:**

```xml
<!-- Good: Proper tab order and keyboard shortcuts -->
<Dialog Title="User Settings" TabIndex="0">
    <StackView Orientation="Vertical">
        <Label Text="Name:" AccessKey="N" Target="{Binding ElementName=nameField}" />
        <TextField Name="nameField" TabIndex="1" />
        
        <Label Text="Email:" AccessKey="E" Target="{Binding ElementName=emailField}" />
        <TextField Name="emailField" TabIndex="2" />
        
        <StackView Orientation="Horizontal" TabIndex="3">
            <Button Text="OK" IsDefault="true" TabIndex="3" />
            <Button Text="Cancel" IsCancel="true" TabIndex="4" />
        </StackView>
    </StackView>
</Dialog>
```

**Key Navigation Principles:**
- **Tab Order**: Set logical `TabIndex` values
- **Access Keys**: Use `AccessKey` for quick navigation (Alt+Key)
- **Default Actions**: Mark primary buttons with `IsDefault="true"`
- **Cancel Actions**: Mark cancel buttons with `IsCancel="true"`

### 2. Screen Reader Support

**Provide meaningful text descriptions:**

```xml
<!-- Good: Descriptive labels and accessible names -->
<FrameView Title="File Operations" AccessibleName="File management controls">
    <StackView Orientation="Vertical">
        <!-- Progress indicator with accessible description -->
        <ProgressBar Name="uploadProgress" 
                     Value="45" 
                     AccessibleName="File upload progress"
                     AccessibleDescription="Upload progress: 45% complete, 3 of 10 files uploaded" />
        
        <!-- Button with clear accessible name -->
        <Button Text="📁 Browse Files" 
                AccessKey="B"
                AccessibleName="Browse and select files for upload"
                ToolTip="Click to open file browser" />
        
        <!-- Status with live updates -->
        <Label Name="statusLabel" 
               Text="Ready to upload"
               AccessibleRole="Status"
               AccessibleLive="Polite" />
    </StackView>
</FrameView>
```

**Screen Reader Properties:**
- `AccessibleName`: Brief description of the control's purpose
- `AccessibleDescription`: Detailed explanation if needed
- `AccessibleRole`: Control type (Button, Status, Dialog, etc.)
- `AccessibleLive`: How screen readers handle content changes

### 3. Visual Accessibility

**Support users with visual impairments:**

```csharp
// Good: Multiple ways to convey information
public void UpdateStatus(string message, StatusType type)
{
    statusLabel.Text = type switch
    {
        StatusType.Success => $"✓ Success: {message}",
        StatusType.Warning => $"⚠ Warning: {message}",
        StatusType.Error => $"✗ Error: {message}",
        _ => message
    };
    
    // Don't rely on color alone - use symbols too
    statusLabel.ForegroundColor = type switch
    {
        StatusType.Success => Color.Green,
        StatusType.Warning => Color.Yellow,
        StatusType.Error => Color.Red,
        _ => Color.White
    };
    
    // Update accessible description for screen readers
    statusLabel.AccessibleDescription = $"Status update: {type} - {message}";
}
```

**Visual Best Practices:**
- **Contrast**: Ensure sufficient contrast between text and background
- **Color Independence**: Never use color as the only way to convey information
- **Symbols**: Use text symbols (✓, ⚠, ✗) alongside color coding
- **Font Size**: Respect terminal font size settings

### 4. Focus Management

**Ensure users always know where they are:**

```csharp
public partial class AccessibleDialog : Dialog
{
    public AccessibleDialog()
    {
        InitializeComponent();
        SetupAccessibleFocus();
    }
    
    private void SetupAccessibleFocus()
    {
        // Set initial focus to first interactive control
        Loaded += (s, e) => nameField.SetFocus();
        
        // Announce dialog opening
        AccessibleName = "User Settings Dialog";
        AccessibleDescription = "Configure user account settings";
        
        // Trap focus within dialog
        KeyDown += OnKeyDown;
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Handle Escape key
        if (e.KeyEvent.Key == Key.Esc)
        {
            AnnounceClose();
            Close();
            e.Handled = true;
        }
        
        // Cycle focus with Tab/Shift+Tab at boundaries
        if (e.KeyEvent.Key == Key.Tab)
        {
            // Focus management logic here
        }
    }
    
    private void AnnounceClose()
    {
        // Announce dialog closure to screen readers
        Application.Driver.Beep(); // Audio cue
    }
}
```

## XAML Accessibility Patterns

### Labels and Form Controls

```xml
<!-- Pattern 1: Explicit labeling -->
<Label Text="Username:" AccessKey="U" Target="{Binding ElementName=usernameField}" />
<TextField Name="usernameField" 
           AccessibleName="Username"
           AccessibleDescription="Enter your login username" />

<!-- Pattern 2: Embedded labeling -->
<TextField PlaceholderText="Enter username" 
           AccessibleName="Username field"
           AccessibleDescription="Your login username" />

<!-- Pattern 3: Group labeling -->
<FrameView Title="Contact Information" 
           AccessibleName="Contact information form"
           AccessibleRole="Group">
    <TextField AccessibleName="Email address" />
    <TextField AccessibleName="Phone number" />
</FrameView>
```

### Lists and Tables

```xml
<!-- Accessible list with proper roles -->
<ListView Name="fileList"
          AccessibleName="File listing"
          AccessibleRole="List"
          AccessibleDescription="List of files in current directory">
    <ListView.ItemTemplate>
        <DataTemplate>
            <!-- Each item should have meaningful accessible name -->
            <Label Text="{Binding DisplayName}" 
                   AccessibleName="{Binding AccessibleSummary}" />
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>

<!-- Table with headers and cell identification -->
<TableView Name="dataTable"
           AccessibleName="Customer data table"
           AccessibleRole="Table">
    <TableView.Columns>
        <TableColumn Header="Name" AccessibleName="Customer name column" />
        <TableColumn Header="Status" AccessibleName="Account status column" />
    </TableView.Columns>
</TableView>
```

### Progressive Disclosure

```xml
<!-- Expandable sections with clear state -->
<TreeView Name="categoryTree"
          AccessibleName="Product categories"
          AccessibleRole="Tree">
    <TreeNode Text="Electronics" 
              AccessibleName="Electronics category"
              AccessibleDescription="Expand to view electronic products"
              AccessibleState="Collapsed" />
</TreeView>

<!-- Details/summary pattern -->
<FrameView Name="advancedOptions"
           Title="⊞ Advanced Options"
           AccessibleName="Advanced options panel"
           AccessibleState="Collapsed">
    <!-- Content here -->
</FrameView>
```

## Code-Behind Accessibility

### Dynamic Content Updates

```csharp
// Good: Announce dynamic changes
public void AddNotification(string message, NotificationType type)
{
    var notification = new Label 
    { 
        Text = $"{GetTypeIcon(type)} {message}",
        AccessibleName = $"{type} notification",
        AccessibleDescription = message,
        AccessibleLive = "Polite" // Announce to screen readers
    };
    
    notificationPanel.Add(notification);
    
    // Move focus to new notification if important
    if (type == NotificationType.Error)
    {
        notification.SetFocus();
        Application.Driver.Beep(); // Audio cue
    }
}

// Good: Progress updates
public void UpdateProgress(int percentage, string status)
{
    progressBar.Value = percentage;
    
    // Update accessible description for milestones
    if (percentage % 25 == 0)
    {
        progressBar.AccessibleDescription = $"Progress: {percentage}% complete. {status}";
    }
    
    statusLabel.Text = status;
    statusLabel.AccessibleLive = "Polite";
}
```

### Error Handling and Validation

```csharp
// Good: Accessible error reporting
public void ValidateForm()
{
    var errors = new List<string>();
    
    if (string.IsNullOrEmpty(nameField.Text))
    {
        errors.Add("Name is required");
        nameField.AccessibleDescription = "Error: Name is required";
        nameField.ForegroundColor = Color.Red;
    }
    
    if (errors.Any())
    {
        // Summary for screen readers
        var errorSummary = $"Form has {errors.Count} error(s): {string.Join(", ", errors)}";
        errorLabel.Text = errorSummary;
        errorLabel.AccessibleLive = "Assertive"; // Immediate announcement
        
        // Focus first error field
        nameField.SetFocus();
        
        // Audio cue
        Application.Driver.Beep();
    }
}
```

## Testing Accessibility

### Manual Testing Checklist

**Keyboard Navigation:**
- [ ] All controls accessible via Tab key
- [ ] Tab order is logical (left-to-right, top-to-bottom)
- [ ] Access keys (Alt+Key) work for all labeled controls
- [ ] Enter key activates default buttons
- [ ] Escape key closes dialogs and cancels operations
- [ ] Arrow keys navigate within complex controls (lists, trees)

**Screen Reader Testing:**
- [ ] Use NVDA (Windows) or Orca (Linux) to test
- [ ] All controls have meaningful names
- [ ] Status changes are announced appropriately
- [ ] Error messages are read clearly
- [ ] Progress updates are conveyed

**Visual Testing:**
- [ ] Information is conveyed without relying on color alone
- [ ] Text has sufficient contrast in all supported terminal themes
- [ ] Focus indicators are clearly visible
- [ ] Text symbols used alongside color coding

### Automated Testing

```csharp
[Test]
public void TestAccessibilityMetadata()
{
    var dialog = new UserSettingsDialog();
    
    // Test that all interactive controls have accessible names
    var interactiveControls = dialog.GetAllControls()
        .Where(c => c is Button || c is TextField || c is CheckBox);
    
    foreach (var control in interactiveControls)
    {
        Assert.IsNotNullOrEmpty(control.AccessibleName, 
            $"Control {control.GetType().Name} missing AccessibleName");
    }
    
    // Test tab order is sequential
    var tabbableControls = dialog.GetAllControls()
        .Where(c => c.TabIndex >= 0)
        .OrderBy(c => c.TabIndex);
    
    var expectedIndex = 0;
    foreach (var control in tabbableControls)
    {
        Assert.AreEqual(expectedIndex, control.TabIndex,
            $"Tab order gap detected at {control.AccessibleName}");
        expectedIndex++;
    }
}

[Test]
public void TestKeyboardShortcuts()
{
    var dialog = new UserSettingsDialog();
    
    // Test that access keys are unique
    var accessKeys = dialog.GetAllControls()
        .Where(c => !string.IsNullOrEmpty(c.AccessKey))
        .Select(c => c.AccessKey.ToLower())
        .ToList();
    
    var duplicates = accessKeys.GroupBy(k => k)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);
    
    Assert.IsFalse(duplicates.Any(), 
        $"Duplicate access keys found: {string.Join(", ", duplicates)}");
}
```

## Platform-Specific Considerations

### Windows

```csharp
// Windows-specific accessibility features
public void ConfigureWindowsAccessibility()
{
    // Enable High Contrast mode detection
    if (SystemInformation.HighContrast)
    {
        // Adjust color scheme
        ApplyHighContrastTheme();
    }
    
    // Support Narrator announcements
    if (SystemInformation.ScreenReaderRunning)
    {
        // Enable live region updates
        EnableScreenReaderSupport();
    }
}
```

### Linux

```csharp
// Linux accessibility with Orca support
public void ConfigureLinuxAccessibility()
{
    // Check for AT-SPI support
    if (Environment.GetEnvironmentVariable("AT_SPI_BUS") != null)
    {
        // Enable assistive technology integration
        EnableATSPISupport();
    }
    
    // Support high contrast themes
    var gtkTheme = Environment.GetEnvironmentVariable("GTK_THEME");
    if (gtkTheme?.Contains("HighContrast") == true)
    {
        ApplyHighContrastTheme();
    }
}
```

## Common Accessibility Anti-Patterns

### ❌ Avoid These Mistakes

```xml
<!-- Bad: No accessible name -->
<Button Text="🔍" Click="OnSearch" />

<!-- Good: Clear accessible name -->
<Button Text="🔍 Search" 
        AccessKey="S"
        AccessibleName="Search products"
        Click="OnSearch" />
```

```csharp
// Bad: Color-only status indication
statusLabel.ForegroundColor = isError ? Color.Red : Color.Green;

// Good: Color + text + symbol
statusLabel.Text = isError ? "❌ Error occurred" : "✅ Success";
statusLabel.ForegroundColor = isError ? Color.Red : Color.Green;
statusLabel.AccessibleDescription = isError ? "Error status" : "Success status";
```

```csharp
// Bad: No focus management in dialogs
public void ShowDialog()
{
    var dialog = new MyDialog();
    Application.Run(dialog);
}

// Good: Proper focus management
public void ShowDialog()
{
    var dialog = new MyDialog();
    
    // Set initial focus
    dialog.Loaded += (s, e) => dialog.firstField.SetFocus();
    
    // Announce dialog
    dialog.AccessibleName = "Settings Dialog";
    
    Application.Run(dialog);
}
```

## Resources and Tools

### Testing Tools
- **NVDA** (Windows): Free screen reader for testing
- **Orca** (Linux): Built-in screen reader
- **Terminal Accessibility Checker**: Custom validation tools

### Guidelines and Standards
- **WCAG 2.1**: Web Content Accessibility Guidelines (adapted for terminal UI)
- **Section 508**: US Federal accessibility requirements
- **Platform Guidelines**: Windows, macOS, and Linux accessibility standards

### Development Resources
- [Accessibility Testing Guide](contributing/accessibility-testing.md)
- [Screen Reader Commands Reference](contributing/screen-reader-commands.md)
- [High Contrast Theme Guide](guides/theming.md#high-contrast)

## Next Steps

1. **Audit Existing App**: Use the manual testing checklist
2. **Add Accessible Names**: Ensure all interactive controls have meaningful names
3. **Test with Screen Reader**: Install NVDA or Orca and test your app
4. **Validate Keyboard Navigation**: Ensure all functionality works without mouse
5. **Create Accessibility Tests**: Add automated tests for accessibility metadata

## Related Documentation

- [Keyboard Navigation Guide](guides/keyboard-navigation.md)
- [Theming and Colors](guides/theming.md)
- [Testing Strategies](../tests/TestStrategy.md)
- [Contributing Guidelines](contributing/index.md)

---

> **Remember**: Accessibility is not an afterthought—it's a core requirement that makes your applications usable by everyone. Start with accessible patterns from day one rather than retrofitting later.
