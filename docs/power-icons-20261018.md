# 长门 Power 图标 · 2026-10-18

使用内置 imagegen 生成/编辑四枚真正透明的母图；沿用 `tools/Build-PowerIconSizes.gd` 做 Godot Lanczos 等比尺寸导出。未使用 CLI/API 回退，未用程序重绘图案、改色或去背景。

后续能力清理按用户要求保留 `SakuraFormPower` 大小图及神子形态卡牌，作为未来功能使用的资源；没有恢复此前清空的能力逻辑。下文是生成与检查时的历史记录，不代表这些资源目前全部启用。

## 文件与构图

生产资源根目录：`D:/GitClone/NagatoSpire2/NagatoSpire2/images/powers/`。

| 文件名（big 与 packed 同名） | 设计 |
| --- | --- |
| `SakuraFormPower.png` | 正面日式御狐面具；象牙白、朱红、少量金色，额间樱纹 |
| `OverflowDrawPower.png` | 官方式青色卡牌扇形，一张牌沿箭头向右下溢出，代表满手牌时抽到的牌进入弃牌堆 |
| `TemporaryPlatingPower.png` | 参考官方覆甲头盔，左半完整，右半向透明空间碎散；没有改为其他甲片或盾牌 |
| `StillWatersPower.png` | 单一平整椭圆池面加下方圆润半球底部，银白涟漪与粉色樱花瓣；按用户红线说明去掉向上凸起的上部，不做双凸水镜 |

每款均有 `big/` 256×256 与 `packed/` 64×64，共八个 RGBA PNG。母图和原有被替换的文件备份保存在 `art_sources/powers/power_icons_20261018/`，通过 `.gdignore` 排除生产包。旧双凸水镜图只留作历史参考，不是最终绑定。

## 官方本地参考

- `D:/GitClone/Slay the Spire 2/images/powers/plating_power.png`
- `D:/GitClone/Slay the Spire 2/images/powers/draw_cards_next_turn_power.png`
- `D:/GitClone/Slay the Spire 2/images/powers/retain_hand_power.png`

直接查看了本地官方纹理，而非凭记忆重画；未修改官方原文件。

## 路径绑定与范围

- `OverflowDrawPower`、`StillWatersPower` 仅添加 `UsesPackedIcon => true`，tooltip 大图继续使用同名 big 图片。
- `TemporaryPlatingPower` 只在本类覆盖 `AssetProfile` 选取 packed/big，未改变通用临时 Power 基类或其他 Power 的路径策略。
- 当前工作树已删除 `SakuraFormPower.cs`，本次没有恢复它或重建能力逻辑；仅替换并交付其同名大小图，供恢复/复用时使用。不能把它描述为当前已激活的第四项能力。
- 不修改能力数值、抽牌/手牌上限、覆甲扣除、触发时机、文案或其他正在编辑的卡牌逻辑，不改用户已有的删除、暂存或未暂存改动。

## 导出与检查

导出参数：

```text
--source=D:/GitClone/NagatoSpire2/art_sources/powers/power_icons_20261018/
--names=SakuraFormPower,OverflowDrawPower,TemporaryPlatingPower,StillWatersPower
```

- Release 全量 Rebuild（禁自动部署）：0 警告、0 错误。
- 导出脚本检查八张 PNG 尺寸、真实 alpha 与透明角点；大小图取自同一母图。
- 原生 Godot 预览以 256 像素大图和 64 像素原生小图显示，包含深浅底；之后直接读取最终导入的 `.ctex` 与 PNG 比较 alpha 和实色像素，避免旧缓存。
- 本地预览/日志：`D:/GitClone/.codex-build/nagato-power-icons-20261018/`。未导出/部署游戏安装目录，未游戏内实测。
- 实际 `.ctex` 检查与深浅底预览已通过八张图；最终预览为 `power-icons-imported-preview.png`。编辑器整体导入仍有既有能量盘 `_particles` 属性和角色选择背景 UID 警告，未扩大范围修改；本批图标没有导入错误。

## 最终提示词（内置 imagegen）

### SakuraFormPower

参考输入：D:/GitClone/NagatoSpire2/NagatoSpire2/images/powers/big/NavyHolidayPower.png。

```text
Use case: stylized-concept.
Asset type: ONE production Power/status icon for NagatoSpire2, square, designed for a 64x64 battle status row and a 256x256 tooltip.
Composition: a single bold compact silhouette occupying 82-88 percent of the square with full safe margins, centered on GENUINE TRANSPARENT ALPHA. Designed to remain instantly legible at 64x64. Clear primary shapes, very few large details, restrained two/three-tone cel shading and crisp thin dark edges. Polished Japanese anime card-game UI icon, not photorealistic or cluttered.
Avoid: any letters, words, numbers, watermark, UI tile, square backing, enclosing badge/frame, scenery, character body/portrait, excessive tiny ornamental noise, bloom covering the silhouette, or a contact sheet. No painted checkerboard or black background. True transparent background. Exactly one coherent icon.

Subject: a JAPANESE SACRED KITSUNE FOX MASK seen EXACTLY STRAIGHT FROM THE FRONT, symmetrical and solemn. This is an actual ceremonial porcelain/painted wooden fox mask, NOT a living fox head and NOT an anime girl's face.
Two upright pointed fox ears, almond narrow dark eye openings, subtly elongated fox muzzle, small vermilion nose. Main mask ivory-white with deep navy shadow edge, elegant vermilion-red painted fox markings sweeping above the eyes and onto the cheeks. One simple small pale-gold sakura mark on the forehead, plus restrained gold at the ear tips. Slight dimensional ceramic highlights without glossy plastic sheen. The red-white-gold palette evokes Nagato's shrine commander identity, dignified and mysterious rather than scary or silly. No hair, human skin, whiskers/fur, horns, crown, shoulder armor, dangling huge ribbons or extra props. Front view only, full ears and mask chin entirely within the canvas.
```

### OverflowDrawPower

参考输入：D:/GitClone/Slay the Spire 2/images/powers/draw_cards_next_turn_power.png；D:/GitClone/Slay the Spire 2/images/powers/retain_hand_power.png。

```text
Use case: stylized-concept.
Asset type: ONE production Power/status icon for NagatoSpire2, square, designed for a 64x64 battle status row and a 256x256 tooltip.
Composition: a single bold compact silhouette occupying 82-88 percent of the square with full safe margins, centered on GENUINE TRANSPARENT ALPHA. Designed to remain instantly legible at 64x64. Clear primary shapes, very few large details, restrained two/three-tone cel shading and crisp thin dark edges. Polished Japanese anime card-game UI icon, not photorealistic or cluttered.
Avoid: any letters, words, numbers, watermark, UI tile, square backing, enclosing badge/frame, scenery, character body/portrait, excessive tiny ornamental noise, bloom covering the silhouette, or a contact sheet. No painted checkerboard or black background. True transparent background. Exactly one coherent icon.

Input image 1 (official draw-cards-next-turn Power) is the reference for the CARD SHAPE, palette and simplified painted UI finish. Input image 2 (official retain-hand Power) is only the reference for a clean readable curved arrow. Do not copy their exact plus/circular-arrow symbols.
Subject: OVERFLOW — cards drawn while the hand is already full go to the discard pile. Show a small full hand of THREE overlapping upright cards fanned compactly toward the left/upper center, and ONE matching card slipping out downward to the lower right, separated by a clear transparent gap. One short thick curved teal arrow follows from the fan toward the departing card, making the overflow/discard direction unmistakable.
Cards have rounded rectangular mint/aqua rims, pale ivory-blue broad centers and a SINGLE simple small dark-teal diamond/sakura-like mark, never detailed card artwork or text. Flat colored planes and one restrained edge shadow, matching the official icon's visual grammar. Use saturated teal and cool ivory, with the departing card accented just slightly warm pale gold so the fourth card reads clearly. No plus sign, no trash can, no human hands, no piles of dozens of cards, no magic storm. Keep this a tiny game status pictogram, not a scene.
```

### TemporaryPlatingPower

参考输入：D:/GitClone/Slay the Spire 2/images/powers/plating_power.png。

```text
Use case: precise-object-edit.
Asset type: ONE transparent Temporary Plating Power icon for NagatoSpire2, to be exported at 256x256 and 64x64.
Input image 1 is the official PLATING Power icon and exact shape/color/style reference to edit: the pale blue riveted helmet with two large dark/transparent eye openings, a central nose guard, and flared lower cheek plates.
Preserve the recognizable official helmet's overall silhouette, front view, pale cyan/blue-gray flat painted planes, rivet seam, simple shading and proportions. Keep the LEFT HALF almost exactly intact as the original, solid and fully colored. Do NOT redesign as a shield, chest plate, samurai armor, futuristic helmet, or ornate gold icon.
Change ONLY the RIGHT HALF to show TEMPORARY plating vanishing: near the center seam it starts solid, then the right cheek and right temple progressively become translucent and break up into a FEW clear pale-blue flake fragments drifting a short distance toward viewer RIGHT. Roughly half of the full helmet is solid/original and half is actively disappearing. A narrow irregular dissolving boundary around the middle/right, but preserve enough ghost of the right eye opening/cheek shape to read as the SAME helmet. Fragments are the same material as the helmet, not smoke, fire, sparkles, wires, or square pixels. Dissolution should visibly remove part of the outer right contour into real transparent space, never be an opaque gray fog.
Composition: single centered compact icon, full helmet and fragments inside generous transparent margins, bold readable at 64px. Match the OFFICIAL simple flat icon style, do not increase ornament/detail or 3D gloss. True transparent alpha including vanished areas and eye holes. No letters, text, frame, background panel, black backdrop, checkerboard, watermark or extra symbols.
```

### StillWatersPower

参考输入：art_sources/powers/power_icons_20261018/StillWatersPower_double_dome_previous_master.png；用户红线构图说明 codex-clipboard-611845ec-a532-4844-a6f5-d56ced92ddbe.png。

```text
Use case: precise-object-edit.
Asset type: single transparent StillWatersPower status icon, to read at 256x256 and 64x64.
Input image 1 is the edit target. Input image 2 is the user's RED-LINE GEOMETRY GUIDE only; never include the red markings or its white background in the output.
Primary correction: REMOVE THE UPPER BULGING CAP / UPPER DOME COMPLETELY. The current shape looks like a two-sided convex lens, which is wrong.
Rebuild it as TWO CLEAR CONNECTED PARTS in one coherent icon:
(1) ONE FLAT, LEVEL ELLIPTICAL POOL SURFACE viewed obliquely, roughly where the user's upper red ellipse marks the water plane. Its BACK EDGE is the topmost silhouette of the entire object. There must be absolutely NO raised blue water mound, glass arch, second curved roof, thick rear dome, or anything standing above this flat surface. Water reflections sit ON this single level plane, not on a curved upper cap.
(2) UNDER that plane, retain a smooth rounded downward-facing blue HALF-DOME / hemispherical bowl-like water base, dark blue along the lower edge, visually supporting the pool. Think an open shallow hemispherical bowl filled exactly to its rim, not a closed biconvex marble or flying saucer. No upper shell, no separate pedestal, no metal bowl decorations.
Keep the same calm blue/teal water, one clean silver-white ripple ellipse ON the flat top pool, the single small pink sakura petal resting on it and its faint reflection. Slight moonlight streak okay only if it lies flat on the pool; do not raise any bright hump behind it. Preserve polished anime game-icon rendering, crisp dark outer contour, simple broad planes and transparency.
Compose the new flatter-top/rounded-bottom icon centrally and large enough to read at 64px, square canvas with safe transparent margins. Complete all edges inside canvas. Genuine alpha outside the object. No background, no red lines, white panel, text, frame, person, scenery, glass bubble, splash, extra objects or second icon. Output ONE assembled icon, NOT an exploded diagram or multiple tiles.
```

