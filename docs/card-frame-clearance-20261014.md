# 长门普通卡框文字净空调整（2026-10-14）

> 本文保留第一轮调整记录。当前三个外框素材已进一步外移、加厚柱子；最新结果见 `card-frame-edge-columns-20261014.md`。

## 已保存的素材与使用方式

使用 imagegen 技能的内置 image_gen 编辑原有三张 PNG，保留朱漆、金饰、樱花、麻叶纹底板及各类型的分隔线，调直并收窄柱子。透明 Alpha 原样保留，没有额外滤镜或后期栅格变形。

- 攻击：`NagatoSpire2/images/card_frames/nagato_attack.png`（1057×1488）
- 技能：`NagatoSpire2/images/card_frames/nagato_skill.png`（1056×1489）
- 能力：`NagatoSpire2/images/card_frames/nagato_power.png`（1056×1489）

生成工具对技能、能力输出有 1 像素的画布取整，保留原始输出，由现有 TextureRect 等比适配。旧图与生成原图保存在 `art_sources/card_frames/straight_columns_20261014/`，该目录有 .gdignore，不进入游戏资源包。

## 布局与蓝光

官方 `scenes/cards/card.tscn` 中外框为 300×422，正文横向范围为 -122..121。旧图的柱子和内侧金边会进入这一区域。

`NagatoCardChromePatch` 仅对长门的非先古攻击、技能、能力外框增加 1.06 倍横向显示宽度，保持中心和高度，正文区域、字号、横幅、PortraitBorder 与品质牌都沿用原布局。结合新图，正文主体处的柱子内侧约为 -129..129，给官方正文两侧留出约 7–8 个逻辑像素的间距。顶部与底部原有金饰形状仍保留。

布局从缓存的原始值计算，刷新不累乘；节点切换为其他角色或先古卡时恢复原值。先古卡美术未修改。

原来的 `HighlightWidthScale=0.95`、`HighlightHeightScale=0.97`、`OriginalHighlights`、`UpdateHighlightBounds` 和 `OriginalHighlightLayout` 已完整移除。此补丁不读取或改写 Highlight 节点；所有高亮尺寸、颜色与动画均使用官方行为。

## 验证

- Release 编译成功，0 警告、0 错误，禁用了自动游戏部署。
- 独立 Godot 原生预览复用了官方各组件矩形和原版 card_ripple 高亮 Shader，对比了三种卡框。正文示例用于观感检查，正式游戏继续使用自己的动态文本布局。
- 预览：`D:/GitClone/.codex-build/nagato-frame-clearance-20261014/comparison.png`。
- 已检查透明窗口、文件加载及主要正文高度的柱子边界。尚未进行游戏内实战验证。
- 最终 PCK 通过 577 项资源索引校验，并在独立 Godot 中成功挂载、加载全部三种新外框及内框/横幅/品质牌完成渲染。配套 DLL、PCK 位于上述预览目录，未部署至游戏目录。
- 导出已完成 savepack；随后独立编辑器报告项目程序集重载失败并滞留，已核对并关闭本次启动的导出进程。生产代码 Release 编译和最终封包实际加载均成功。日志还包含已有角色选择背景的重复 UID 警告。

## 最终编辑 Prompt 集

### skill

编辑输入：`NagatoSpire2/images/card_frames/nagato_skill.png` 的修改前版本。

```text
Use case: precise-object-edit.
Asset type: production transparent 2D card outer-frame PNG for NagatoSpire2.
Input image 1 is the exact edit target, not a loose style reference.
Primary request: make a SMALL geometric refinement to this existing Japanese torii card frame so long card-description text no longer overlaps the pillars. Keep the same design, rendering, textures, sakura, gold fittings, crest, upper and lower rails and overall silhouette. NOT a redesign.
Output canvas and framing: exactly 1057 x 1488 pixels, unchanged portrait aspect and unchanged transparent canvas registration. Flat front view, no perspective. One frame only.
Required geometry: both red lacquer side pillars must be perfectly straight, vertical and parallel, NOT leaning inward toward the top. Move the pillars and their thin inner gold rims slightly outward and make them a little slimmer. Throughout the lower text panel from y=850 to y=1340, keep a generous clean interior: the left pillar and inner gold rim must end by x=72, the right pillar and rim must begin at x=985. Thus the inner opening runs x=73..984 (at least 86% canvas width), with consistent vertical edges. Keep the base collars modest and out of the text zone; do not put flower clusters or dangling ornaments into that clear lower interior. Join pillars naturally to the existing beams, corner fittings and bottom rail.
Preserve: vermilion lacquer colors and fine highlights, small white/pink sakura branch accents, black/gold foot collars, gold tomoe crest centered at the same top position, charcoal Japanese asanoha-pattern description panel, exact dark-panel top boundary shape and HEIGHT appropriate to this input's card type, and the bottom rail at the original height. Fill the newly widened description panel with seamlessly matching existing charcoal asanoha texture. Keep the upper illustration window fully transparent; retain real alpha transparency outside the frame. Render the polished anime-game details at least as clean and sharp as the input.
Avoid: adding a banner/title ribbon or type plaque, text, characters, illustration, white fill, background, glow, a new decorative style, extra petals, warping the image, tilting pillars, a collage. No text at all.
Card type: SKILL. Preserve this input's existing straight horizontal illustration/text division exactly; only its side endpoints extend naturally to the new outward pillar positions.
```

### attack

编辑输入：`NagatoSpire2/images/card_frames/nagato_attack.png` 的修改前版本。

```text
Use case: precise-object-edit.
Asset type: production transparent 2D card outer-frame PNG for NagatoSpire2.
Input image 1 is the exact edit target, not a loose style reference.
Primary request: make a SMALL geometric refinement to this existing Japanese torii card frame so long card-description text no longer overlaps the pillars. Keep the same design, rendering, textures, sakura, gold fittings, crest, upper and lower rails and overall silhouette. NOT a redesign.
Output canvas and framing: exactly 1057 x 1488 pixels, unchanged portrait aspect and unchanged transparent canvas registration. Flat front view, no perspective. One frame only.
Required geometry: both red lacquer side pillars must be perfectly straight, vertical and parallel, NOT leaning inward toward the top. Move the pillars and their thin inner gold rims slightly outward and make them a little slimmer. Throughout the lower text panel from y=850 to y=1340, keep a generous clean interior: the left pillar and inner gold rim must end by x=72, the right pillar and rim must begin at x=985. Thus the inner opening runs x=73..984 (at least 86% canvas width), with consistent vertical edges. Keep the base collars modest and out of the text zone; do not put flower clusters or dangling ornaments into that clear lower interior. Join pillars naturally to the existing beams, corner fittings and bottom rail.
Preserve: vermilion lacquer colors and fine highlights, small white/pink sakura branch accents, black/gold foot collars, gold tomoe crest centered at the same top position, charcoal Japanese asanoha-pattern description panel, exact dark-panel top boundary shape and HEIGHT appropriate to this input's card type, and the bottom rail at the original height. Fill the newly widened description panel with seamlessly matching existing charcoal asanoha texture. Keep the upper illustration window fully transparent; retain real alpha transparency outside the frame. Render the polished anime-game details at least as clean and sharp as the input.
Avoid: adding a banner/title ribbon or type plaque, text, characters, illustration, white fill, background, glow, a new decorative style, extra petals, warping the image, tilting pillars, a collage. No text at all.
Card type: ATTACK. Preserve this input's existing shallow V-shaped illustration/text division exactly; only its side endpoints extend naturally to the new outward pillar positions.
```

### power

编辑输入：`NagatoSpire2/images/card_frames/nagato_power.png` 的修改前版本。

```text
Use case: precise-object-edit.
Asset type: production transparent 2D card outer-frame PNG for NagatoSpire2.
Input image 1 is the exact edit target, not a loose style reference.
Primary request: make a SMALL geometric refinement to this existing Japanese torii card frame so long card-description text no longer overlaps the pillars. Keep the same design, rendering, textures, sakura, gold fittings, crest, upper and lower rails and overall silhouette. NOT a redesign.
Output canvas and framing: exactly 1057 x 1488 pixels, unchanged portrait aspect and unchanged transparent canvas registration. Flat front view, no perspective. One frame only.
Required geometry: both red lacquer side pillars must be perfectly straight, vertical and parallel, NOT leaning inward toward the top. Move the pillars and their thin inner gold rims slightly outward and make them a little slimmer. Throughout the lower text panel from y=850 to y=1340, keep a generous clean interior: the left pillar and inner gold rim must end by x=72, the right pillar and rim must begin at x=985. Thus the inner opening runs x=73..984 (at least 86% canvas width), with consistent vertical edges. Keep the base collars modest and out of the text zone; do not put flower clusters or dangling ornaments into that clear lower interior. Join pillars naturally to the existing beams, corner fittings and bottom rail.
Preserve: vermilion lacquer colors and fine highlights, small white/pink sakura branch accents, black/gold foot collars, gold tomoe crest centered at the same top position, charcoal Japanese asanoha-pattern description panel, exact dark-panel top boundary shape and HEIGHT appropriate to this input's card type, and the bottom rail at the original height. Fill the newly widened description panel with seamlessly matching existing charcoal asanoha texture. Keep the upper illustration window fully transparent; retain real alpha transparency outside the frame. Render the polished anime-game details at least as clean and sharp as the input.
Avoid: adding a banner/title ribbon or type plaque, text, characters, illustration, white fill, background, glow, a new decorative style, extra petals, warping the image, tilting pillars, a collage. No text at all.
Card type: POWER. Preserve this input's existing rounded concave illustration/text division exactly; only its side endpoints extend naturally to the new outward pillar positions.
```

