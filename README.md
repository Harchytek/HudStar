# HudStar

**Valheim** mod that allows you to reposition and scale the enemy stars on the HUD.

![HudStar](https://imgur.com/CqUuyNP.png)

## Features
* **Custom Position**: Move stars (X, Y) to fit your UI.
* **Scaling**: Make stars larger or smaller.
* **Performance**: Lightweight script that only updates when necessary.

## Installation
1. Install [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/).
2. Place `HudStar.dll` in your `BepInEx/plugins` folder.
3. Launch the game once to generate the config file.

## Configuration
Edit the config file at `BepInEx/config/harchytek.hudstar.cfg` to adjust offsets and scale.

## Version comparison

Version 2.0.0 is much more stable and performs better. It uses Harmony to intervene only when necessary, whereas version 1.0.0 constantly scanned the game.

| Version | v1.0.0 (Scan) | v2.0.0 (Harmony) |
| :--- | :--- | :--- |
| **CPU Impact** | Medium (Regular hierarchy scans) | **Very Low** (Event-based) |
| **RAM Usage** | Temporary object creation | **Static & Optimized** |
| **Compatibility** | Potential conflicts | **Native** (Isolated group) |
| **Reliability** | May miss stars depending on timing | **Captures 100%** via m_gui |

## About myself

If you have any suggestions or need help, you can contact me on Github.

I make these mods for my friends and me, but they can be useful to everyone. It's my way of contributing to the Valheim community.
