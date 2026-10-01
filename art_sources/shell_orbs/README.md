# Nagato shell orb sources

The current game assets are the three `*_final.png` files, copied to
`NagatoSpire2/images/orbs/`. Their scenes continue to use those stable paths.

`tools/build_shell_orbs.py` builds three separate lathed 3D geometry guides:

| Shell | Length in caliber units | Geometry | Finish |
| --- | ---: | --- | --- |
| High explosive | 2.60 | Short body, round nose | Scarlet/orange lacquer, gold, explosive flower |
| Armor piercing | 2.98 | Long body, rounded ogive | Navy/cobalt enamel, gold, spear crest |
| Type 3 | 2.78 | Medium body, round nose | Ivory/vermilion lacquer, gold, sakura |

All models use a **1.00-unit diameter** (0.50 radius) and the same camera
projection scale. The final visual finish was painted with built-in image
generation: each `*_geometry.png` was the shape/pose guide, and the matching
user-supplied earlier projectile image was a **surface-style-only** reference.
The icons are standalone projectiles on transparent backgrounds, with no
cartridge cases or external effects. Type 3 remains closed, not flower-shaped.

The `*_uv.png` sheets are reproducible color-blocking guides, not the final
painterly finish. Older `shell_41cm.obj`, `neutral.png`, `comparison_256.png`,
and `previous_*.png` remain only for comparison; they are not current assets.

To regenerate geometry guides: `python tools/build_shell_orbs.py --out-dir
art_sources/shell_orbs` (requires NumPy and Pillow). Repainting is a separate
image-generation step, not reproduced by that script.

Art prompt summary: retain geometry-guide camera, caliber and rounded
silhouette; transfer only the corresponding reference's lacquer, metal gloss,
gold bands, palette and emblem; make a polished Q-version anime game icon on
genuine transparency, without letters or surrounding effects.
