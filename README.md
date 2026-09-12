[Leer en Español](README_ES.md)

# TowerFall Localization & Multi-Language Mod

A localization and translation mod for **TowerFall** built using **FortRise** and **Harmony**. It translates 100% of the game UI, menus, game modes, match variants, statistics, awards, archer titles, and hints.

## Features

- **Full In-Game Localization:** Complete translation of menus, audio/video options, controls, pause screens, and all game modes (Versus, Quest, Dark World, and Trials).
- **Multi-Language Packs (`langs/`):** Includes complete 536-string translation dictionaries for:
  * 🇪🇸 **Spanish** (`translations_ES.json`) — Default active translation
  * 🇫🇷 **French** (`translations_FR.json`)
  * 🇧🇷 **Brazilian Portuguese** (`translations_PT_BR.json`)
  * 🇩🇪 **German** (`translations_DE.json`)
  * 🇮🇹 **Italian** (`translations_IT.json`)
  * 🇷🇺 **Russian** (`translations_RU.json`)
- **Balanced Line Wrapping:** Automatically recalculates two-line splits for end-game awards and button guides to prevent text clipping.
- **Non-Invasive Runtime Hooking:** Operates entirely in memory via Harmony patches without modifying original XMLs or game binaries.

---

## Requirements

- TowerFall installed with [FortRise](https://github.com/FortRise/FortRise).
- .NET SDK `10.0.401` *(only required if compiling from source)*.
- The installation must contain `TowerFall.Patch.dll`, `0Harmony.dll`, and `Microsoft.Extensions.Logging.Abstractions.dll`.

---

## Build and Install

Double-click `compilar.bat`. The script compiles the C# source code and copies the mod files to your game installation:

```text
Mods/TowerFallEspanol/TowerFallEspanol.dll
Mods/TowerFallEspanol/translations.json
```

Default installation path:
```text
C:\Program Files (x86)\Steam\steamapps\common\TowerFall - FortRise
```

You can also pass a custom game directory as the first argument:
```bat
compilar.bat "D:\Games\TowerFall - FortRise"
```

---

## Project Structure

```text
TowerFallEspanol/
├─ src/
│  └─ TowerFallEspanolModule.cs    # FortRise module entry & Harmony patches
├─ langs/                          # Language packs
│  ├─ translations_ES.json
│  ├─ translations_FR.json
│  ├─ translations_PT_BR.json
│  ├─ translations_DE.json
│  ├─ translations_IT.json
│  └─ translations_RU.json
├─ translations.json               # Active translation file (Spanish by default)
├─ meta.json                       # FortRise mod metadata manifest
├─ compilar.bat                    # Build and install script
├─ README.md                       # English documentation
└─ README_ES.md                    # Spanish documentation
```

To use another language, copy the desired JSON file from `langs/` and replace `translations.json`.

---

## How It Works (Translation Mechanism)

The mod loads as a FortRise DLL module. Upon initialization, it instantiates `TranslationService`, loads `translations.json` via `IModContent`, and registers Harmony runtime patches:

- **Drawing text:** Intercepts `Monocle.Draw`, `Text`, and `OutlineText`.
- **Buttons and guides:** Translates text before measuring in `VariantButton` and `MenuButtonGuide` so bubbles and guides dynamically adapt their width.
- **Archer names:** Translates titles and names when `ArcherData` finishes initializing.
- **Awards:** Intercepts `AwardInfo` and calculates an optimal, balanced two-line split.
- **Dynamic text fragments:** Translates `|`-delimited strings and gameplay tips.

---

## Credits and Acknowledgments

- **[FortRise](https://github.com/FortRise/FortRise):** The official community mod loader and framework for TowerFall.
