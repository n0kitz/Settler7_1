# Audio — CC0 drop-in folder

`AudioManager` auto-loads clips from this folder at runtime (`Resources.Load`).
Drop CC0 audio files here with **exactly these names** (any of `.wav`, `.ogg`,
`.mp3` — Unity imports by base name, so the extension doesn't matter):

| Filename (no extension) | Plays when… | Suggested type |
|-------------------------|-------------|----------------|
| `music_main`           | game starts (loops) | ambient medieval music track |
| `building_placed`      | you place a building | short thud/hammer |
| `building_complete`    | a building finishes | positive chime |
| `production_complete`  | a good is produced  | soft click/pop |
| `sector_conquered`     | a sector is taken   | horn/fanfare |
| `tech_researched`      | a tech completes    | magical shimmer |
| `vp_gained`            | you gain a VP       | triumphant sting |
| `combat_start`         | combat resolves     | sword clash |
| `ui_click`             | UI buttons (optional)| soft UI click |

Anything missing simply stays silent — no errors. Inspector-assigned clips on
the `AudioManager` component take priority over files here.

## Good CC0 sources
- **Kenney** — https://kenney.nl/assets?q=audio (UI, impact, RPG packs; all CC0)
- **freesound.org** — filter by "Creative Commons 0" license
- **OpenGameArt.org** — filter by CC0

Recommended: grab Kenney's "RPG Audio" + "UI Audio" + "Impact Sounds" packs,
rename a handful of files to the names above, drop them in this folder. Done.
