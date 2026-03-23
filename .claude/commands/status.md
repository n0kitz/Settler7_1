Show a quick project snapshot. Gather the following and display in a compact format.

## Data to collect

1. **File counts per layer** — count `.cs` files in each:
   - `Assets/Scripts/Simulation/`
   - `Assets/Scripts/Presentation/`
   - `Assets/Scripts/UI/`
   - `Assets/Scripts/Data/`
   - `Assets/Scripts/Editor/`
   - `Assets/Tests/Editor/`

2. **Total .cs file count**

3. **Largest files** — top 5 `.cs` files by line count

4. **Test count** — number of `[Test]` attributes across all test files

5. **Current phase** — read from memory file `project_status.md`

6. **Git status** — uncommitted changes summary

## Output format
```
=== Project Status ===
Phase: [from memory]
Files: N total (Simulation: X, Presentation: Y, UI: Z, Data: D, Editor: E, Tests: T)
Tests: N test methods
Largest: file.cs (N lines), ...
Git: clean | N uncommitted changes
```
