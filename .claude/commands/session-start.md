Mandatory session start protocol. Run this at the beginning of every session.

## Steps (execute in this exact order)

### Step 1 — Read CLAUDE.md
Read `CLAUDE.md` (project root). This is the technical source of truth: game design spec, architecture, folder structure, naming conventions, critical rules. Never assume anything about the project.

### Step 2 — Read VISION.md
Read `VISION.md` (project root). This is the north star and the Definition of Done — what "finished and great" looks like, and the tiered checklist for getting there. When a decision needs a tie-breaker, the option that serves the vision wins.

### Step 3 — Read project_status.md
Read `project_status.md` (project root). This is where the project is *right now*: current position, health at a glance, file counts, roadmap progress, known issues, key patterns, and recent sessions. This is what happened since CLAUDE.md was last updated.

### Step 4 — Read cost-saving rules
Read the `cost-saving` skill. Contains session protocol, golden rules, token budgets, and learned mistakes. Never write code without reading this first.

### Step 5 — State understanding
In 1-2 sentences: current state + blocking issues + what the next priority task is. If anything in `project_status.md` contradicts CLAUDE.md or VISION.md, flag it before proceeding.

### Step 6 — Wait for instructions
Do NOT start working until the user gives a goal. If the user says "continue", pick the next phase from `project_status.md` → Roadmap Progress (the row marked ▶ next), propose it in one sentence, and wait for confirmation.
