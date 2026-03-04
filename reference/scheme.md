# Scheme Deep Dive

A `Scheme` is a mapping from `VisualRole`s (e.g., `VisualRole.Focus`) to `Attribute`s, defining how a `View` should look based on its purpose (for example, Menu or Dialog). `SchemeManager.Schemes` is a dictionary of schemes indexed by name and `SchemeManager` manages available schemes and defaults.

Only `Normal` is required; other properties inherit values via well-defined rules if not explicitly set.

---

## Overview

### Scheme Inheritance

- By default, a `View` inherits its `Scheme` from its `SuperView`.
- If no `SuperView` has a scheme, the view falls back to the `Base` scheme from `SchemeManager.GetCurrentSchemes()`.
- `GetScheme()` resolves scheme via (in order): explicit `_scheme` field, `SchemeName` lookup, `SuperView.GetScheme()`, then `SchemeManager.GetCurrentSchemes()["Base"]`.

### Explicit Assignment and Names

- Assign a `Scheme` directly via `View.Scheme` (calls `SetScheme(value)`), which overrides inheritance.
- Alternatively set `View.SchemeName` to resolve a named scheme from `SchemeManager` (useful for declarative configs).
- `SetScheme(Scheme? scheme)` updates the internal `_scheme`, triggers `SetNeedsDraw()` on changes, and handles special cases like `Border`.

### Events for Customization

- `GettingScheme` (raised in `GetScheme()`): handlers can set `args.NewScheme` or cancel default resolution via `args.Cancel = true`.
- `SettingScheme` (raised in `SetScheme()`): handlers can cancel scheme changes.
- `OnGettingScheme` and `OnSettingScheme` virtual methods allow derived classes to customize behavior.

### Attributes and Visual Roles

- `GetAttributeForRole(VisualRole role)` retrieves the `Attribute` for a role from the active `Scheme` and raises `GettingAttributeForRole` (allowing modification or cancellation).
- If a view is disabled (`Enabled == false`) and a non-disabled role is requested, the method falls back to `VisualRole.Disabled` to ensure disabled appearance.
- `SetAttributeForRole(VisualRole role)` tells the driver which `Attribute` to use; `SetAttribute(Attribute attribute)` sets the driver's attribute directly (but using roles is preferred).

---

## Flexible Scheme Management in `Terminal.Gui.View`

1. Scheme Inheritance (default behavior) — described above.
2. Explicit Scheme Assignment — set `View.Scheme` or `View.SchemeName`.
3. Event-Driven Customization — use the `GettingScheme`/`SettingScheme` and `GettingAttributeForRole` events to intercept and modify resolution.
4. Retrieving/Applying Attributes — use `GetAttributeForRole` and `SetAttributeForRole` for rendering.

### Impact of SuperViews and SubViews via Events

- `SuperView` can subscribe to its `SubView`'s `GettingScheme` or `GettingAttributeForRole` to dynamically adjust children appearance.
- `SubView` influence over `SuperView` is less common, but events enable advanced scenarios where appearance decisions need external logic.

In summary, Terminal.Gui offers layered scheme management: inheritance and explicit setting for common cases, and an event system for advanced customization and dynamic control.

---

## Common Schemes

Typical named schemes managed by `SchemeManager` include:

- `Base` — default for most views
- `Dialog` — used for `Dialog`, `MessageBox`, and other dialog-like views
- `Error` — used for error displays like `ErrorQuery`
- `Menu` — used by `Menu`, `MenuBar`, and `StatusBar`
- `TopLevel` — used for the application top-level view

Use `SchemeManager.GetScheme(Schemes.Dialog)` to retrieve a scheme, and `ConfigurationManager` can override defaults or add schemes.

---

## See Also

- Drawing Deep Dive — overview of the drawing system
- Configuration — overview of configuration and `ConfigurationManager`

---

*(Extracted from https://gui-cs.github.io/Terminal.Gui/docs/scheme.html)*
