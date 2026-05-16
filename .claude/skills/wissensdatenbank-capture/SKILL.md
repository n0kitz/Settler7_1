---
name: wissensdatenbank-capture
description: >
  Save, archive, and organize any session output into Normen's Obsidian-based Wissensdatenbank.
  ALWAYS use this skill when the user says "speicher das", "leg das ab", "in die Wissensdatenbank",
  "ins Obsidian", "als Notiz", "archiviere", "notiere", or wants to save research results,
  decisions, exam materials, project notes, infrastructure plans, or any structured knowledge.
  Also trigger when another skill (deep-research, ihk-exam-creator, presentation-de, etc.)
  produces output that should be persisted. This is the universal sink for all knowledge capture.
---

# Wissensdatenbank Capture

Normen's Wissensdatenbank is an Obsidian vault mounted at:
`/Users/normenkitzmann/Wissensdatenbank/`

## Critical: Write Timeout Workaround

**`write_file` times out when Obsidian has the target file open.**
Always use this pattern instead:

```bash
cat > "/Users/normenkitzmann/Wissensdatenbank/PATH/filename.md" << 'EOF'
[content]
EOF
```

For large files (>200 lines), split into sections and append:
```bash
cat >> "/Users/normenkitzmann/Wissensdatenbank/PATH/filename.md" << 'EOF'
[additional content]
EOF
```

Always verify with:
```bash
ls -la "/Users/normenkitzmann/Wissensdatenbank/PATH/filename.md"
```

---

## Folder Structure

```
Wissensdatenbank/
├── 01_Projekte/           # Active projects (SettlersX, NFLpredictiveAI, NAS, etc.)
├── 02_Bereiche/           # Ongoing responsibilities (Azure, IHK, Cloud-Team)
├── 03_Ressourcen/         # Reference material, links, tools
├── 04_Archiv/             # Completed or paused projects
├── 05_Inbox/              # Unsorted / quick captures → default if unsure
├── 06_Templates/          # Obsidian templates
├── 07_Pruefungsaufgaben/  # IHK exam materials
│   ├── AP1/
│   └── AP2/
├── 08_KI_Prompting/       # AI/Claude related
│   ├── Claude/
│   ├── MCP_Server/
│   └── Externe_Tools/
└── 09_Monatsberichte/     # Monthly reports for Cloud-Team
```

### Folder Selection Guide

| Content type | Target folder |
|---|---|
| Active project notes (SettlersX, NFL, NAS) | `01_Projekte/[Projektname]/` |
| Azure / Cloud-Team work | `02_Bereiche/Azure/` |
| IHK teaching materials, AP1/AP2 prep | `07_Pruefungsaufgaben/AP1/` or `AP2/` |
| Claude setups, MCP configs, prompts | `08_KI_Prompting/Claude/` |
| MCP server docs | `08_KI_Prompting/MCP_Server/` |
| Monthly reports (Monatsberichte) | `09_Monatsberichte/` |
| Research results, tool references | `03_Ressourcen/` |
| Unsure / quick save | `05_Inbox/` |

---

## File Naming Convention

```
YYYY-MM-DD_Titel-mit-Bindestrichen.md
```

Examples:
- `2026-05-11_SettlersX-ECS-Architektur.md`
- `2026-05-11_NAS-Infrastruktur-Planung.md`
- `2026-05-11_AP2-Pruefungsfragen-Netzwerk.md`

For evergreen reference notes (kein Datum sinnvoll):
```
Thema-Untertitel.md
```

---

## YAML Frontmatter Template

```yaml
---
title: "Titel der Notiz"
date: YYYY-MM-DD
tags: [tag1, tag2, tag3]
type: note|project|exam|report|reference|infrastructure
status: active|draft|archived
related:
  - "[[Verwandte Notiz]]"
---
```

### Tag Conventions

| Kontext | Tags |
|---|---|
| SettlersX | `godot`, `csharp`, `gamedev`, `settlersX` |
| NFLpredictiveAI | `nfl`, `python`, `ml`, `fastapi` |
| Azure / Cloud | `azure`, `cloud`, `powershell` |
| IHK | `ihk`, `ausbildung`, `ap1` oder `ap2` |
| Lokale KI | `localai`, `ollama`, `llm`, `infrastructure` |
| NAS | `nas`, `selfhosted`, `infrastructure` |
| Claude / MCP | `claude`, `mcp`, `prompting` |
| Monatsbericht | `monatsbericht`, `cloud-team` |

---

## Wikilink Style

Always use Obsidian wikilinks for cross-references:
```markdown
Siehe auch [[Verwandte Notiz]] und [[Anderes Thema]].
```

For section links:
```markdown
[[Notiz#Abschnitt]]
```

---

## Standard Note Structure

### For project/research notes:
```markdown
---
[frontmatter]
---

# Titel

## Zusammenfassung
_Kernaussage in 2-3 Sätzen._

## Kontext
Warum ist das relevant? Welches Problem löst es?

## Inhalt / Ergebnisse
[Hauptinhalt]

## Nächste Schritte
- [ ] Aufgabe 1
- [ ] Aufgabe 2

## Quellen / Referenzen
- [Link oder Quelle]

## Verwandte Notizen
- [[Andere Notiz]]
```

### For IHK exam materials:
```markdown
---
[frontmatter with type: exam]
---

# Thema: [Themenname]

## Lernziele
- ...

## Theorie
[Erklärung]

## Prüfungsaufgaben
### Aufgabe 1
**Aufgabenstellung:** ...
**Musterlösung:** ...
**Bewertung:** X Punkte

## Merkhilfen
> 💡 Merksatz: ...

## Verwandte Themen
- [[AP2-Thema]]
```

### For Monatsberichte:
```markdown
---
[frontmatter with type: report]
---

# Monatsbericht [Monat] [Jahr]

## Thema
## Zusammenfassung
## Technische Details
## Ausblick
```

---

## Execution Checklist

Before writing a file:
1. **Determine target folder** using the Folder Selection Guide above
2. **Choose filename** following naming convention
3. **Fill frontmatter** with appropriate tags and type
4. **Use bash cat heredoc** — never `write_file` directly
5. **Verify** file was written with `ls -la`
6. **Report** the full path to the user so they can open it in Obsidian

After writing:
```
✅ Gespeichert: /Users/normenkitzmann/Wissensdatenbank/[Pfad]/[Dateiname].md
```
