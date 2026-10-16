# 长门 Power 图标 · 2026-10-16

使用 imagegen 内置工具分别生成四张真实透明母图；用 Godot Lanczos 等比导出用户要求的 256×256 与 64×64 PNG，不另画大小版本、不裁主体、不添加滤镜。

## 文件与设计

资源根目录：`D:/GitClone/NagatoSpire2/NagatoSpire2/images/powers/`。

| Power | 图意 | big / packed 文件名 |
| --- | --- | --- |
| 白光 | 核爆初闪、扩张的双重光环；只用光表现 | `LastWhiteLightEchoPower.png` |
| 白光将尽 | 核爆终末白炽、猛烈不规则光浪；只用光表现 | `LastWhiteLightPower.png` |
| 海军假日 | 轻微倾斜的樱花舰锚与红白绳结，不带背后甲片 | `NavyHolidayPower.png` |
| 魂迎 | 魂火归入甲壳；对应消耗牌获得覆甲 | `SoulWelcomingPower.png` |

每个文件在 `big/` 中为 256×256，在 `packed/` 中为 64×64。四个 Power 通过 `UsesPackedIcon` 显式启用 packed 小图；其余 Power 保留原有路径策略，不改变卡牌效果。大图仍通过 `BigIconPath` 绑定。

母图与旧海军假日图备份位于 `art_sources/powers/power_icons_20261016/`，该目录已用 `.gdignore` 排除打包。复现导出脚本为 `tools/Build-PowerIconSizes.gd`。

## 最终验证与限制

- 四款均已导出 `big/` 256×256 和 `packed/` 64×64，共八张真正带 alpha 的 RGBA PNG；由同一最终母图等比缩小，大小图一致。
- 原生 Godot 预览验证通过：`D:/GitClone/.codex-build/nagato-power-icons-20261016/power-icons-preview.png`，包含深浅底与 64 像素原生大小对照。
- 最终导入的 `.ctex` 也已直接加载、逐像素检查 alpha / 不透明区域颜色及尺寸，与最终 PNG 一致，排除旧图缓存；预览为 `power-icons-imported-preview.png`，未依赖失败的整包编译。
- 海军假日最终母图为去甲片、轻微倾斜版本。白光双图最终为纯核爆强光意象，无星徽、花纹、弹体或蘑菇云。旧设计仅留作被忽略的源图历史备份，不被 Power 引用。
- 本次只新增四项 packed 路径选择，不改 Power 数值、触发条件和 VFX。
- 初次增量构建报告成功，但随后的全量 Release Rebuild 发现当前游戏 API 兼容错误，不能据此宣称 DLL 完整验证通过：`Sealed.cs` 的 `CardLocation`、`NNagatoOrbVfx.cs` 的 `NOrbVfx`、`ArmorSuperiorityPower` / `LockOnPower` 的 `ModifyDamageMultiplicative` 重写签名，以及 `LastWhiteLightEchoPower` 的 `ITemporaryPower.IgnoreNextInstance()` 接口成员。图标路径语句不在报错位置；没有顺带修改这些逻辑。
- 不交付旧中间 PCK/DLL 作为最终可用整包；未覆盖游戏安装目录，未进行游戏内验证。素材尺寸、透明度和原生显示均已检查。

## 提示词（内置 imagegen，最终版本）

### LastWhiteLightEchoPower

```text
Use case: stylized-concept.
Asset type: single transparent Power status icon for NagatoSpire2, square, readable at 64x64 and polished at 256x256.
Primary intent: the blinding light of a NUCLEAR DETONATION expressed ONLY WITH LIGHT, not an ornamental magical emblem. Clean premium anime painted game-effect sprite with bold light masses, simple strong silhouette, deliberately controlled glow. Genuine transparent background with safe margins; light remains luminous without becoming an opaque white square. White-hot highlights with thin cool shadow contours for legibility on light and dark UI.
Strict exclusions: NO bomb, weapon, nuclear device, mushroom cloud, smoke cloud, buildings, landscape, radiation trefoil, skull, flower, sakura, engraved pattern, gold jewelry, metal, crystal, heraldic star, compass, compass points, magical sigil, gemstone, dark black-hole vortex, text, numbers, tile or frame. The entire image consists of light and radiating light waves only. No many tiny particles. A single icon, not a contact sheet.
Subject: "White Light" — the first actinic flash and its expanding light echoes. A small intensely saturated white circular core emits one very broad horizontal lens-flash with soft tapering ends. Around the core, two clear expanding elliptical shock-light rings overlap in perspective, one pearly white and one barely ice-blue, with their upper and lower arcs separated by readable transparent gaps. A few short soft white diagonal light streaks emerge behind the core, but NO pointed starburst shape. Mostly ivory-white/blue-white, a restrained pale warm-yellow heart. Atmosphere: utterly dazzling and overwhelming nuclear light, elegant and simple, recognizable as a flashwave rather than a sun badge. Compose centrally within 84 percent of the square so all light fades before the edge.
```

### LastWhiteLightPower

```text
Use case: stylized-concept.
Asset type: single transparent Power status icon for NagatoSpire2, square, readable at 64x64 and polished at 256x256.
Primary intent: the blinding light of a NUCLEAR DETONATION expressed ONLY WITH LIGHT, not an ornamental magical emblem. Clean premium anime painted game-effect sprite with bold light masses, simple strong silhouette, deliberately controlled glow. Genuine transparent background with safe margins; light remains luminous without becoming an opaque white square. White-hot highlights with thin cool shadow contours for legibility on light and dark UI.
Strict exclusions: NO bomb, weapon, nuclear device, mushroom cloud, smoke cloud, buildings, landscape, radiation trefoil, skull, flower, sakura, engraved pattern, gold jewelry, metal, crystal, heraldic star, compass, compass points, magical sigil, gemstone, dark black-hole vortex, text, numbers, tile or frame. The entire image consists of light and radiating light waves only. No many tiny particles. A single icon, not a contact sheet.
Subject: "Fading White Light" — the terminal, catastrophic whiteout flash. An intensely white compact nucleus radiates a dense violent spherical light-front, with one thick incomplete expanding pressure-light ring and a handful of broad irregular tongues of white light shooting outward, like a detonation frame frozen at maximum brightness. The ring has two large empty gaps and uneven thickness; it is luminous energy, never a broken physical ring. Use a tiny white-yellow searing center, ivory-white rays, a very thin hot amber/pale-orange rim, and minimal blue-grey edge contrast. Avoid purple jewels or a black center. Distinguish it clearly from the other icon: more circular, denser and unstable with irregular broad rays, instead of the orderly pair of horizontal elliptical echo rings. Still coherent and sharply readable at 64x64. No regular geometric four/eight-point star. Entire outer silhouette and glow fit safely within the square, transparent corners.
```

### NavyHolidayPower

```text
Use case: precise-object-edit.
Asset type: transparent game Power icon, NavyHolidayPower.
Input image 1 is the exact edit target. Make only these requested design changes: completely REMOVE all three dark navy Japanese armor plates behind the anchor, including every gold rim and gold wave pattern belonging to those plates. All areas formerly occupied by the armor must become genuinely transparent, not filled or replaced. Keep the gold-and-ivory naval anchor, the single pink five-petal sakura blossom, the red-and-white ceremonial rope knot and short red tassels, their excellent painted anime game-icon material quality and clear outlines.
Then tilt the complete remaining anchor-and-sakura emblem gently, about 12 degrees clockwise, as one coherent object. Keep it centered on a square canvas and fit the complete tilted silhouette inside safe transparent margins; do not crop the ring, the flukes, the rope or tassels. No armor, no shield, no backing badge, no frame, no backdrop, no added props or particles. No text, no watermark. Genuine alpha background. Bold clear silhouette remains legible at 64x64, with polished detail at 256x256. Preserve the existing palette and rendering style of the surviving elements.
```

### SoulWelcomingPower

```text
Use case: stylized-concept.
Asset type: one transparent game status-effect / Power icon for NagatoSpire2.
Style: premium hand-painted Japanese anime fantasy game UI icon, compact polished emblem, clean deliberate dark outline, broad readable color planes, restrained cel shading and a few crisp material highlights. Not photorealistic, not a flat generic clip-art symbol. Coordinate with a solemn sakura-and-shrine naval commander theme.
Composition: one centered coherent icon on a square canvas, isolated on genuine transparent alpha. Main silhouette occupies about 80-85 percent of canvas with safe transparent margins; no clipping. Design must stay instantly legible at 64x64 and remain attractive at 256x256. Bold primary silhouette and very few large details. Keep peripheral glow tight so the icon has a clear edge against both dark battle backgrounds and pale tooltips.
Constraints: no letters, no numbers, no text, no watermark, no rectangular tile, no enclosing UI frame, no background scenery, no extra separate icons, no character portrait, no tiny decorative noise. Output a single finished square icon, not a contact sheet.
Subject: "Soul Welcoming", a buff that turns exhausted cards into protective plating. A single large elegant Japanese hitodama spirit flame, ivory at its center and lavender-violet at its broad curling tip, resting inside a small open cupped armor shell made of two overlapping Japanese lamellar plates. The armor shell is muted shrine vermilion with warm gold edging and one simple five-petal sakura seal at the center. Two smooth broad flame curls sweep inward, suggesting a soul returning and being sheltered, not a hostile fireball. Strong silhouette, serene and sacred, violet-white flame clearly distinct from the white star icons and the navy anchor. No face on the spirit, no ghost cartoon, no skull, no extra souls, no shrine scenery, no cards with text, no many sparks.
```

