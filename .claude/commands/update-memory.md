Update memory files to match the actual codebase state.

## Steps

1. **Scan the codebase:**
   - Count all `.cs` files under `Assets/Scripts/` and `Assets/Tests/`
   - List all files grouped by layer (Simulation, Presentation, UI, Data, Editor, Tests)
   - Note any new directories or structural changes

2. **Update `project_folder_structure.md`:**
   - Replace the file listing with the current actual files
   - Keep the same format as the existing memory file

3. **Update `project_status.md`:**
   - Update file counts to match reality
   - Keep phase and other status info unchanged unless the user specifies otherwise

4. **Report what changed** — show a brief diff of what was updated in each memory file.
