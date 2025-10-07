# Legacy Binding Syntax

> ⚠️ **DEPRECATED**: This binding syntax is deprecated as of Terminal.Gui.Xaml v2.0 and will be removed in v3.0. Use [Modern Binding Syntax](bind-data.md) instead.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Deprecated in**: Terminal.Gui.Xaml v2.0 (March 2026)  
> **Status**: Deprecated  
> **Removal planned**: Terminal.Gui.Xaml v3.0 (September 2026)

## What This Was

The legacy binding syntax used `{Data PropertyName}` format for data binding connections.

## Legacy Syntax (Deprecated)

```xml
<!-- OLD: Legacy syntax (don't use) -->
<Label Text="{Data UserName}" />
<TextField Text="{Data Email, Mode=TwoWay}" />
```

## Modern Replacement

```xml
<!-- NEW: Modern syntax (use instead) -->
<Label Text="{Binding UserName}" />
<TextField Text="{Binding Email, Mode=TwoWay}" />
```

## Migration Guide

### Automated Migration

Use the migration tool to update existing XAML files:

```powershell
# Install migration tool
dotnet tool install --global Terminal.Gui.Xaml.MigrationTool

# Migrate all XAML files
tgx-migrate --from=v1 --to=v2 --path="./src"
```

### Manual Migration

1. **Replace `{Data` with `{Binding`**:
   - Find: `{Data PropertyName}`  
   - Replace: `{Binding PropertyName}`

2. **Update mode syntax** (if used):
   - Find: `{Data PropertyName, Mode=TwoWay}`
   - Replace: `{Binding PropertyName, Mode=TwoWay}`

3. **Test thoroughly** after migration

### Breaking Changes

> ⚠️ **Breaking Change**: The `{Data}` syntax is not supported in Terminal.Gui.Xaml v2.0+. Applications using legacy syntax will fail at runtime with `XamlParseException`.

## Timeline

- **v1.0 - v1.9**: Legacy `{Data}` syntax supported
- **v2.0**: Legacy syntax deprecated, modern `{Binding}` syntax required
- **v2.x**: Warning messages for legacy syntax usage  
- **v3.0**: Legacy syntax removed entirely

## See Also

- **[Modern Data Binding](bind-data.md)** - Current binding implementation
- **[Migration Guide](../migration/v1-to-v2.md)** - Complete migration instructions
- **[Breaking Changes](../migration/breaking-changes.md)** - All v2.0 breaking changes