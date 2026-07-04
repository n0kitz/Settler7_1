---
description: "Cost saving rules, session protocol, golden rules, and learned mistakes. Read before writing any code."
---

# Settlers Clone — Cost Saving Rules

## Mandatory Session Order (every session, no exceptions)
1. Read CLAUDE.md          → architecture, conventions, game design spec
2. Read VISION.md          → the goal + Definition of Done (tie-breaker for decisions)
3. Read project_status.md  → current state, known issues, next roadmap phase
4. Read this skill         → cost rules before writing any code
5. State understanding     → one sentence: current state + what changes
6. Run /session-start      → enforces the above automatically

At session end:
- Run /session-end         → updates project_status.md before closing
- Never close without updating project_status.md

## Golden Rules

### Rule 1 — Read Before You Write
Check if a class/system already exists before creating a new one.
This prevents the #1 cost: implementing wrong, then re-doing.

### Rule 2 — Simulation Layer First
Layer 2 is pure C# — no Unity Editor needed, testable with NUnit.
Get logic right before building any visual layer.
80% of Claude Code value is delivered in Layer 2.

### Rule 3 — One System Per Session
Never implement more than one system per session.
Scope creep = token waste + 529 API overload errors.

### Rule 4 — Stubs Before References
If any file references a class, that class MUST exist first as a stub.
Missing stubs = CS0246 errors = project won't open.
Lesson learned: PrestigeChartUI, TechTreeUI, TradeMapUI, ArmyPanel,
TavernUI were all missing — blocked the entire project from opening.

### Rule 5 — No Over-Explaining
Reference CLAUDE.md section numbers instead of re-stating requirements.
Don't repeat requirements back — just implement them.

### Rule 6 — ScriptableObjects Are Data, Not Code
New recipe = new data asset, NOT new C# code.
Use Editor menu scripts to batch-create SO assets.

### Rule 7 — Batch By Type, Not By Feature
All recipes in one session. All building definitions in one session.
All tech definitions in one session.

## Avoid These Token Wastes
| Waste                                  | Instead                          |
|----------------------------------------|----------------------------------|
| Writing stubs to fill in later         | Implement fully or skip          |
| XML docs on private methods            | Only document public APIs        |
| Refactoring working code for style     | Only if blocking new features    |
| MonoBehaviour for pure logic           | Keep in Simulation (Layer 2)     |
| Multiple systems in one session        | One system per session           |
| Forgetting to update project_status.md | Run /session-end every time      |
| Creating classes without stub first    | Stubs before any reference       |

## What NOT To Do (Learned The Hard Way)
- Do NOT use DOTS/ECS — wrong scale (see CLAUDE.md)
- Do NOT reference UnityEngine in Layer 2 — breaks NUnit tests
- Do NOT implement multiple systems in one session — causes 529 errors
- Do NOT wire up class references before the stub file exists
- Do NOT skip project_status.md at session start — you will repeat solved problems
- Do NOT end a session without running /session-end

## Cost Estimates
| Task                                        | Est. Tokens |
|---------------------------------------------|-------------|
| New simulation system                       | 10–20K      |
| All ScriptableObject definitions (20+)      | 15–25K      |
| Editor script to batch-create SO assets     | 5–10K       |
| NUnit test suite for a system               | 5–10K       |
| UI panel (Canvas-based)                     | 8–15K       |
| Presentation MonoBehaviour                  | 8–15K       |
| Bug fix                                     | 3–8K        |
| Full production chain (all recipes)         | 15–25K      |
| Creating missing UI stubs (5 classes)       | 3–5K        |
