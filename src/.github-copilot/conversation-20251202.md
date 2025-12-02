# Conversation Summary - December 2, 2025

## Topic: Fixing Generated File Bookkeeping and Collision Detection

### Context
Following previous work that added bookkeeping functionality to track generated files, this session focused on resolving compilation errors and implementing collision detection for duplicate generated class names.

---

## Issues Resolved

### 1. Compilation Errors in `GeneratedFileBookkeeping.cs`

**Problem:**
Two compilation errors prevented tests from running:
- `CS0103`: `HashCode.Combine()` not available on target framework
- `CS1061`: Nullable `SourceProductionContext?` doesn't support `ReportDiagnostic()` method

**Solution:**
- Replaced `HashCode.Combine()` with portable hash implementation:
  ```csharp
  (StringComparer.Ordinal.GetHashCode(Namespace) * 397) ^ StringComparer.Ordinal.GetHashCode(ClassName)
  ```
- Created three overloads for `Record()`:
  1. No diagnostics: `Record(path, fileName, ns, className)`
  2. With diagnostics: `Record(path, fileName, ns, className, SourceProductionContext spc)`
  3. Nullable wrapper: `Record(..., SourceProductionContext? spc = null)`

### 2. Roslyn Hint Name Collision

**Problem:**
Test `Generator_EmitsCollisionDiagnostic_ForDuplicateGeneratedIdentity` failed because multiple `.xtui` files with the same filename (e.g., `dirA/MyWindow.xtui` and `dirB/MyWindow.xtui`) tried to generate sources with identical hint names (`MyWindow.g.cs`), causing Roslyn to throw `ArgumentException`.

**Root Cause:**
```csharp
// Both files tried to use the same hint name
spc.AddSource("MyWindow.g.cs", code);  // Second call fails!
```

**Solution:**
Made hint names unique by incorporating a hash of the full input path:
```csharp
string pathHash = Math.Abs(file.Path.GetHashCode()).ToString("X8");
string uniqueHintName = $"{className}_{pathHash}.g.cs";
spc.AddSource(uniqueHintName, SourceText.From(code, Encoding.UTF8));
```

Result: `MyWindow_A1B2C3D4.g.cs` and `MyWindow_B5E6F7A8.g.cs` (different hint names, no Roslyn crash)

### 3. Collision Detection Implementation

**Dual Detection Strategy:**

**Early Detection (Pipeline Level):**
```csharp
// In XtuiGenerator.Initialize()
var duplicateFilenameGroups = xamlFiles.Collect()
    .Select((arr, _) => arr.GroupBy(x => x.FileName)
    .Where(g => g.Count() > 1));

context.RegisterSourceOutput(duplicateFilenameGroups, (spc, groups) => {
    // Report XTUI003 for duplicate filenames
});
```

**Late Detection (Bookkeeping Level):**
```csharp
// After successful generation
var identity = new GeneratedIdentity(namespace, className);
if (!s_identityToInput.TryAdd(identity, inputPath)) {
    // Collision: another file already generated this namespace+class
    spc.ReportDiagnostic(XTUI003);
}
else {
    // Fallback: enumerate s_generatedFiles for edge cases
    foreach (var entry in s_generatedFiles) {
        if (same namespace+class, different path)
            spc.ReportDiagnostic(XTUI003);
    }
}
```

**Diagnostic Descriptor:**
- **ID:** `XTUI003`
- **Title:** XTUI Generated Class Name Collision
- **Severity:** Warning
- **Message:** `"XTUI files '{0}' and '{1}' generate the same class '{2}.{3}'. Rename one input or change class-name resolution."`

---

## Architecture: Dual-Map Bookkeeping

### Data Structures

```
GeneratedFileKey (readonly struct)
├── InputPath: string (case-insensitive)
└── Implements IEquatable<T> for efficient lookups

GeneratedFileInfo (sealed class)
├── GeneratedFileName: string
├── Namespace: string
└── ClassName: string

GeneratedIdentity (readonly struct)
├── Namespace: string (case-sensitive)
├── ClassName: string (case-sensitive)
└── Implements IEquatable<T>
```

### Storage Maps

```
Forward Map:  s_generatedFiles
              ConcurrentDictionary<GeneratedFileKey, GeneratedFileInfo>
              InputPath → (GeneratedFileName, Namespace, ClassName)

Reverse Map:  s_identityToInput
              ConcurrentDictionary<GeneratedIdentity, string>
              (Namespace, ClassName) → InputPath
```

### Design Decisions

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| **Key Types** | `readonly struct` instead of `record` | Avoid `IsExternalInit` dependency for older TFMs |
| **Thread Safety** | `ConcurrentDictionary` | Generator runs in parallel |
| **Path Comparison** | Case-insensitive | Windows file system compatibility |
| **Type Comparison** | Case-sensitive | C# language semantics |
| **Hash Function** | `StringComparer.GetHashCode()` | Portable across .NET versions |
| **Detection Timing** | After `AddSource()` | Non-blocking, allows inspection of colliding code |
| **Error Handling** | Try-catch wrapper | Bookkeeping failures don't break generation |

---

## Test Results

**Final Status:** ✅ All 14 tests passing

```
Test Run Successful.
Total tests: 14
     Passed: 14
     Failed: 0
   Skipped: 0
 Total time: 1.6s
```

**Key Test Coverage:**
- ✅ `Generator_WithSimpleWindow_GeneratesInitializeComponent`
- ✅ `Generator_WithSimpleToplevel_GeneratesInitializeComponent`
- ✅ `Generator_RecordsGeneratedFileBookkeeping`
- ✅ `Generator_EmitsCollisionDiagnostic_ForDuplicateGeneratedIdentity` (new)
- ✅ `Generator_WithoutTerminalGuiReference_DoesNotGenerate`
- ✅ `Generator_WithInvalidXtui_ReportsDiagnostic`
- ✅ `Generator_WithMultipleChildren_GeneratesMultipleAddCalls`

---

## Why Detect After AddSource (Not Before)

### Advantages of Post-Generation Detection:

1. **Roslyn Constraint:** Requires unique hint names, not unique class names
   - Multiple files can intentionally generate the same class (different projects, configurations)
   - Hint name uniqueness prevents crashes; semantic collision is a warning-level concern

2. **Parallel Execution Safety:**
   - Pre-check race condition: both threads see "no collision" before either records
   - Post-record: actual outputs are tracked, race-safe with `ConcurrentDictionary`

3. **Non-Blocking Diagnostics:**
   - Build succeeds with warnings
   - Generated code is inspectable (helps debugging)
   - CI/CD collects all warnings in one pass

4. **Incremental Generation Support:**
   - Bookkeeping reflects actual outputs, not speculative checks
   - Works with Roslyn's caching and conditional generation

5. **Developer Experience:**
   - Users see *what* is colliding (generated code available)
   - All issues reported in one build (not first-failure-only)
   - Warning-as-error policies can enforce if desired

---

## Files Modified

### Core Implementation
- `src/Terminal.Gui.Xtui/GeneratedFileBookkeeping.cs`
  - Added three `Record()` overloads
  - Fixed hash code implementation
  - Added collision detection logic with dual-phase checking

- `src/Terminal.Gui.Xtui/XtuiGenerator.cs`
  - Added unique hint name generation using path hash
  - Added early duplicate filename detection pipeline
  - Calls bookkeeping with `SourceProductionContext`

### Tests
- `src/Terminal.Gui.Xtui.RoslynTests/IncrementalGeneratorTests.cs`
  - Added `Generator_EmitsCollisionDiagnostic_ForDuplicateGeneratedIdentity` test
  - Verified XTUI003 diagnostic emission

---

## Command History

```powershell
# Initial test run (compilation errors)
dotnet test "src/Terminal.Gui.Xtui.RoslynTests/..." -c Debug
# Error: HashCode not found, nullable SourceProductionContext issue

# After overload fixes (hint name collision)
dotnet test "src/Terminal.Gui.Xtui.RoslynTests/..." -c Debug
# Error: Duplicate hint name 'MyWindow.g.cs'

# Test with debugging output
dotnet test "src/Terminal.Gui.Xtui.RoslynTests/..." -c Debug --filter "Generator_EmitsCollisionDiagnostic_*"
# Revealed: ArgumentException for duplicate hint name

# After unique hint name fix
dotnet test "src/Terminal.Gui.Xtui.RoslynTests/..." -c Debug --filter "Generator_EmitsCollisionDiagnostic_*"
# Success: XTUI003 diagnostic emitted

# Final full test run
dotnet test "src/Terminal.Gui.Xtui.RoslynTests/..." -c Debug
# Success: 14/14 tests passing
```

---

## Key Takeaways

1. **Portable Hash Codes:** Use `StringComparer.GetHashCode()` instead of `HashCode.Combine()` for older TFMs
2. **Nullable Structs:** `SourceProductionContext?` requires `.HasValue` check and `.Value` access for methods
3. **Roslyn Hint Names:** Must be unique per generator; use path-based disambiguation
4. **Collision Strategy:** Detect semantic issues after generation, not before
5. **Dual Detection:** Early filename check + late semantic check provides comprehensive coverage
6. **Thread Safety:** `ConcurrentDictionary` + fallback enumeration handles parallel execution
7. **Non-Blocking:** Warnings preserve developer productivity better than hard failures

---

## Related Work

- Previous session: Added TopLevel generator, tests, benchmarks, and initial bookkeeping implementation
- Benchmark artifacts: `BenchmarkDotNet.Artifacts/Benchmarks-20251202.md`
- Diagnostic catalog: XTUI001 (parsing error), XTUI002 (generation error), XTUI003 (collision warning)
