Update MEMORY.md and auto-memory files to match the actual codebase state.

## Steps

1. **Read current state:**
   - Read `MEMORY.md` in project root (Claude Code's working memory)
   - Read `project_status.md` in project root (auto-generated file counts + phase)

2. **Scan the codebase:**
   - Count all `.cs` files under `Assets/Scripts/` and `Assets/Tests/`
   - List all files grouped by layer (Simulation, Presentation, UI, Data, Editor, Tests)
   - Note any new directories or structural changes

3. **Update `MEMORY.md` (project root):**
   - Update ## Current State with actual file counts and system inventory
   - Update ## Known Bugs (remove fixed, add new if found)
   - Update ## Next Up based on what has been completed
   - Add any new architecture decisions to ## Decisions Made
   - Add any failed approaches to ## What NOT To Do

4. **Update auto-memory files (project root):**
   - Update `project_folder_structure.md` with current actual files listed by layer
   - Update `project_status.md` with current file counts and phase info

5. **Report what changed** — show a brief diff of what was updated.
