Update project_status.md and auto-memory files to match the actual codebase state.

## Steps

1. **Read current state:**
   - Read `project_status.md` in project root (the single working-status source of truth)
   - Read `project_folder_structure.md` in project root (structure map)

2. **Scan the codebase:**
   - Count all `.cs` files under `Assets/Scripts/` and `Assets/Tests/`
   - List all files grouped by layer (Simulation, Presentation, UI, Data, Editor, Tests)
   - Note any new directories or structural changes
   - Flag any file over 300 lines (architecture rule)

3. **Update `project_status.md` (project root):**
   - Refresh Health at a Glance (test count) and File Counts with real numbers
   - Update Roadmap Progress markers (✅ / ▶ / ○) to match reality
   - Update Known Issues / Tech Debt (remove fixed, add new — including any 300-line violations)
   - Add any bite-you-if-forgotten pattern to Key Patterns
   - Keep Current Position aligned with the Definition-of-Done tiers in VISION.md

4. **Update structure map:**
   - Update `project_folder_structure.md` with current actual files listed by layer

5. **Report what changed** — show a brief diff of what was updated.
