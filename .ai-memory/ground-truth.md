# Ground Truth

**Goal:** Roslyn source generator that compiles `.xtui` (XAML-style XML) files into C# at build time for Terminal.Gui v2. Zero runtime overhead; no runtime dependency on `Terminal.Gui.Xtui`.

**Architecture:** Generator emits public interfaces (`IXtuiAppHelpers`, `IXtuiViewHelpers`, `IXtuiBindingHelpers`) + internal implementations into the *user's* assembly. Public `Xtui` static accessor wires them. `InitializeComponent` partial-class pattern. `App.xtui` handles app-level resources and optional `Program.Main` generation.

**Key Decisions:**
- All helpers code-generated into user assembly; no Xtui runtime package reference
- Interfaces public + minimal; implementations internal
- `ConditionalWeakTable<View,Holder>` for per-view attached state
- `IValueConverter` follows WinRT-style contract; converters emitted as cached static fields
- Resources registered via `EnsureInitialized()` (module initializer pattern)

**Constraints:**
- `View`/`UserControl` live in Terminal.Gui assembly (separate)
- Generated code must not reference `Terminal.Gui.Xtui` at runtime
- Keep public interface surface stable and minimal

**Open Items:**
- `App.xtui` parser + `AppXtui.g.cs` emitter (spec `001-refactor-generators`)
- Binding/converter codegen wiring
- `Xtui.Interfaces.g.cs` emission into user assembly

**Next Action:** Implement `App.xtui` parsing and `AppXtui.g.cs` code emission per spec `001-refactor-generators`.
