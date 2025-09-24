# Concepts

Core concepts and architectural principles of Terminal.Gui.Xaml.

This section explains the fundamental concepts you need to understand to effectively use Terminal.Gui.Xaml for building terminal applications.

## Framework Concepts

- **[Layout](layout.md)** - Character-based positioning and responsive design patterns
- **[Data Binding](binding.md)** - Two-way binding, INotifyPropertyChanged, and MVVM patterns  
- **[Events](events.md)** - Event handling, commands, and user interaction patterns

## Understanding Terminal.Gui.Xaml

Terminal.Gui.Xaml brings declarative UI development to terminal applications by combining:

- **XAML Markup** - Familiar XML-based UI definition language
- **Terminal.Gui Foundation** - Cross-platform terminal UI framework
- **Modern .NET** - Latest C# features and .NET 8+ capabilities

## Architecture Overview

```
┌─────────────────────────────────────────┐
│            Your Application             │
├─────────────────────────────────────────┤
│         Terminal.Gui.Xaml               │
│  ┌─────────────┬──────────────────────┐ │
│  │    XAML     │    Data Binding      │ │
│  │   Engine    │     Engine           │ │
│  └─────────────┴──────────────────────┘ │
├─────────────────────────────────────────┤
│            Terminal.Gui v2+             │
├─────────────────────────────────────────┤
│     Platform (Windows/Linux/macOS)     │
└─────────────────────────────────────────┘
```

## Key Benefits

- **Declarative UI** - Define interfaces in XAML instead of imperative code
- **Familiar Patterns** - Leverage existing WPF/UWP knowledge
- **Terminal Native** - Optimized for character-based rendering
- **Cross-Platform** - Runs on Windows, Linux, and macOS terminals

## Next Steps

1. **Start with [Layout](layout.md)** to understand positioning and sizing
2. **Learn [Data Binding](binding.md)** for dynamic UI updates  
3. **Master [Events](events.md)** for user interaction handling
4. **Apply concepts** in our [Guides](../guides/toc.yml) and [Examples](../examples/toc.yml)

---

> **Related Documentation**: [Getting Started](../getting-started.md) • [Guides](../guides/toc.yml) • [Examples](../examples/toc.yml) • [API Reference](~/api/index.md)