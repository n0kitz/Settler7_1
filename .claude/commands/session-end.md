Mandatory session end protocol. Run this before closing every session.

## Steps

### Step 1 — Update project_status.md
Read the current `project_status.md` (project root), then update:

- **Last updated** — set to today's date
- **Current Position** — the active Definition-of-Done tier and the next roadmap phase (keep aligned with VISION.md)
- **Health at a Glance / File Counts** — refresh test count and file counts if files were added/changed
- **Roadmap Progress** — advance the ✅/▶/○ markers to reflect what was just completed
- **Known Issues / Tech Debt** — add new issues (with file paths); remove ones that were fixed
- **Key Patterns** — add any bite-you-if-forgotten pattern discovered this session
- **Recent Sessions** — add a one-line dated entry (what was done, what worked, what broke)

### Step 2 — Update auto-memory
Run `/update-memory` to sync the auto-memory files.

### Step 3 — Log the session
Write a session log to the Wissensdatenbank (`AI_Sessions/Claude_Code/Siedler-Clone/`) per the global CLAUDE.md protocol.

### Step 4 — Confirm
State in one sentence what `project_status.md` now says the next session should start with.

A session without a status update is a session that will be repeated.
