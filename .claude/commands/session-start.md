Mandatory session start protocol. Run this at the beginning of every session.

## Steps (execute in this exact order)

### Step 1 — Read CLAUDE.md
Read `CLAUDE.md` (project root). This is the single source of truth for game design, architecture, folder structure, naming conventions, and implementation phases. Never assume anything about the project.

### Step 2 — Read MEMORY.md
Read `MEMORY.md` (project root). This contains the last known project state, open bugs with file+line references, queued tasks, and settled decisions. This is what happened since CLAUDE.md was last updated.

### Step 3 — Read cost-saving rules
Read the `cost-saving` skill. Contains session protocol, golden rules, token budgets, and learned mistakes. Never write code without reading this first.

### Step 4 — State understanding
In 1-2 sentences: current state + blocking issues + what the next priority task is. If anything in MEMORY.md contradicts CLAUDE.md, flag it before proceeding.

### Step 5 — Wait for instructions
Do NOT start working until the user gives a goal. If the user says "continue", pick the top item from MEMORY.md ## Next Up, propose it in one sentence, and wait for confirmation.
