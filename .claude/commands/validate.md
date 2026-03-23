Run architecture validation checks on the codebase. Report results grouped by category.

## Checks to perform

### 1. Layer Violation: No UnityEngine in Simulation/
Search all files in `Assets/Scripts/Simulation/` for `using UnityEngine`.
- `Vector3` and `Mathf` are allowed (they're value types).
- Any other UnityEngine import is a violation.
Report: list of violating files, or "PASS".

### 2. File Size: No file over 300 lines
Count lines in every `.cs` file under `Assets/Scripts/` and `Assets/Tests/`.
Report any file exceeding 300 lines with its line count, or "PASS".

### 3. File Count
Count all `.cs` files under `Assets/Scripts/` and `Assets/Tests/`.
Compare against the count in memory file `project_status.md`.
Report: actual count, expected count, PASS/MISMATCH.

### 4. Public API Documentation
In all `.cs` files under `Assets/Scripts/`, find `public` methods/properties that lack XML doc comments (`///`).
Report: list of undocumented public members (file:line), or "PASS".

## Output format
```
=== Architecture Validation ===
Layer Separation: PASS | FAIL (N violations)
File Sizes:       PASS | FAIL (N files over 300 lines)
File Count:       PASS | MISMATCH (expected N, found M)
Public Docs:      PASS | FAIL (N undocumented)
```
Then list details for any failures.
