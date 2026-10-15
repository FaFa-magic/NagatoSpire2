# 长门卡框柱子外移修订（2026-10-14）

## 已完成

按用户的补充要求，使用 imagegen 技能的内置 image_gen 再次编辑三张现有外框：柱子向画布边缘移动，同时恢复更厚实的红漆柱身。保留各类型的分隔线、朱漆金饰、樱花和麻叶纹正文底板。

最终素材（均为 1056×1489 PNG，保留生成 Alpha）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_attack.png`
- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_skill.png`
- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_power.png`

旧版与生成原图保存在 `art_sources/card_frames/edge_columns_20261014/`，有 .gdignore，未加入游戏资源包。内置生成工具的原文件继续保留在 Codex generated_images 中。

## 实际几何与显示

以正文主体高度（图像高度的 70%）处的可见红漆/金边测量：

| 类型 | 修订前左柱外侧空隙 | 修订后左柱外侧空隙 | 修订前柱身厚度（左/右） | 修订后柱身厚度（左/右） |
| --- | ---: | ---: | ---: | ---: |
| 攻击 | 31 px | 11 px | 70 / 69 px | 86 / 87 px |
| 技能 | 31 px | 20 px | 70 / 68 px | 84 / 84 px |
| 能力 | 38 px | 16 px | 59 / 59 px | 82 / 79 px |

这些是生成结果的实测值；Prompt 中的 4–6 px 是目标描述，不当作已达到的精确结果。窗口中心 Alpha 仍为 0。

沿用上一轮仅针对长门普通卡的外框 1.06 倍横向显示宽度。正文主体的内侧金边约在 -128..128 到 -130..130，官方正文范围仍为 -122..121；主要正文两侧留有约 6–9 个逻辑像素的间距。没有新增蓝光缩放代码。

## 观感检查

独立 Godot 预览用相同的原版高亮 Shader 和参数、相同的文字/横幅/内框位置对比两版；修订后的柱子覆盖更多侧边蓝光。预览通过正常加载和原生渲染。游戏继续使用自己的动态文本布局，尚未实战验证。

对比图：`D:/GitClone/.codex-build/nagato-frame-edge-columns-20261014/comparison.png`。

本轮仅改三个 PNG 与记录文档，代码绑定继续沿用现有资源路径。上一轮 DLL 可与本轮更新后的 PCK 配套使用。

最终导出、封包索引校验（577 个资源）、正式 PCK 挂载和三张外框新增柱身覆盖位置的 Alpha 对照均通过，包内尺寸与源图一致。正式封包渲染图为检查目录中的 `comparison-packed.png`。配套 `NagatoSpire2.pck` 和 `NagatoSpire2.dll` 也保存在该目录，未自动部署到游戏。

导出在完成 savepack 后记录了独立编辑器的项目程序集重载失败，进程正常退出；最终 PCK 在独立 Godot 中成功加载渲染。另有既存角色选择背景重复 UID 警告。生产 C# 在本轮没有新增修改，配套 DLL 与上一轮编译通过的版本一致。

## 最终编辑 Prompt 集

### attack

```text
Use case: precise-object-edit.
Asset type: transparent game card outer frame, one existing Nagato torii PNG.
Input image 1 is the edit target. Make a precise geometry adjustment to the two vermilion vertical side pillars.
CRITICAL primary change: move both pillars OUTWARD to the very edges of this same canvas, and restore a sturdy medium thickness. The current pillars leave about 30 px of empty transparent margin outside their red shafts. REMOVE that margin by relocating the pillar assembly outward and increasing its shaft width slightly. Do NOT solve this by making the pillars thin.
For a roughly 1057-pixel-wide canvas: the LEFT red shaft plus gold inner trim spans approximately x=4..94, the RIGHT red shaft plus gold inner trim spans approximately x=963..1052. Each pillar is about 90 pixels wide, about 8.5% of total width, visibly thicker than this input, not a thin strip. The outside edge of each shaft runs almost flush with the corresponding canvas edge (only 4–6 pixels of transparent clearance). Their center lines move outward. Both pillars are straight, parallel, constant width, and vertical for the full shaft length y=150..1300; do not incline or taper.
Move their existing sakura ornaments, gold bindings, black-and-gold feet and gold edge trims with the pillar assembly. Fit ornaments within the canvas without clipping. Keep the fine inner gold rim at about x94 on the left and x963 on the right. Keep the dark text panel continuous all the way to these rims; preserve and extend its existing subtle charcoal asanoha pattern naturally. Preserve a wide unobstructed text space.
Keep this exact canvas aspect and approximate original resolution 1057x1488; do not introduce new padding, scale the whole card down, zoom out, or shift anything vertically.
Preserve the existing vermilion lacquer, polished gold details, black foot collars, small white/pink sakura, top centered gold tomoe crest, top rail height, bottom rail height, and the exact shape and center height of the illustration/text divider in this input. Adjust short rail endpoints only as needed to connect to the shifted posts. Same polished anime-game painting and crisp details. Retain real alpha transparency in the illustration opening and outside the frame. No letters, banner, type badge, character, illustration, glow, background scene or watermark.
The finished frame must visibly touch much closer to both image edges through almost the entire height, with healthy pillar thickness.
Type-specific invariant: ATTACK — keep its shallow V-shaped gold illustration/text dividing edge.
```

### skill

```text
Use case: precise-object-edit.
Asset type: transparent game card outer frame, one existing Nagato torii PNG.
Input image 1 is the edit target. Make a precise geometry adjustment to the two vermilion vertical side pillars.
CRITICAL primary change: move both pillars OUTWARD to the very edges of this same canvas, and restore a sturdy medium thickness. The current pillars leave about 30 px of empty transparent margin outside their red shafts. REMOVE that margin by relocating the pillar assembly outward and increasing its shaft width slightly. Do NOT solve this by making the pillars thin.
For a roughly 1057-pixel-wide canvas: the LEFT red shaft plus gold inner trim spans approximately x=4..94, the RIGHT red shaft plus gold inner trim spans approximately x=963..1052. Each pillar is about 90 pixels wide, about 8.5% of total width, visibly thicker than this input, not a thin strip. The outside edge of each shaft runs almost flush with the corresponding canvas edge (only 4–6 pixels of transparent clearance). Their center lines move outward. Both pillars are straight, parallel, constant width, and vertical for the full shaft length y=150..1300; do not incline or taper.
Move their existing sakura ornaments, gold bindings, black-and-gold feet and gold edge trims with the pillar assembly. Fit ornaments within the canvas without clipping. Keep the fine inner gold rim at about x94 on the left and x963 on the right. Keep the dark text panel continuous all the way to these rims; preserve and extend its existing subtle charcoal asanoha pattern naturally. Preserve a wide unobstructed text space.
Keep this exact canvas aspect and approximate original resolution 1057x1488; do not introduce new padding, scale the whole card down, zoom out, or shift anything vertically.
Preserve the existing vermilion lacquer, polished gold details, black foot collars, small white/pink sakura, top centered gold tomoe crest, top rail height, bottom rail height, and the exact shape and center height of the illustration/text divider in this input. Adjust short rail endpoints only as needed to connect to the shifted posts. Same polished anime-game painting and crisp details. Retain real alpha transparency in the illustration opening and outside the frame. No letters, banner, type badge, character, illustration, glow, background scene or watermark.
The finished frame must visibly touch much closer to both image edges through almost the entire height, with healthy pillar thickness.
Type-specific invariant: SKILL — keep its straight horizontal gold illustration/text dividing edge.
```

### power

```text
Use case: precise-object-edit.
Asset type: transparent game card outer frame, one existing Nagato torii PNG.
Input image 1 is the edit target. Make a precise geometry adjustment to the two vermilion vertical side pillars.
CRITICAL primary change: move both pillars OUTWARD to the very edges of this same canvas, and restore a sturdy medium thickness. The current pillars leave about 30 px of empty transparent margin outside their red shafts. REMOVE that margin by relocating the pillar assembly outward and increasing its shaft width slightly. Do NOT solve this by making the pillars thin.
For a roughly 1057-pixel-wide canvas: the LEFT red shaft plus gold inner trim spans approximately x=4..94, the RIGHT red shaft plus gold inner trim spans approximately x=963..1052. Each pillar is about 90 pixels wide, about 8.5% of total width, visibly thicker than this input, not a thin strip. The outside edge of each shaft runs almost flush with the corresponding canvas edge (only 4–6 pixels of transparent clearance). Their center lines move outward. Both pillars are straight, parallel, constant width, and vertical for the full shaft length y=150..1300; do not incline or taper.
Move their existing sakura ornaments, gold bindings, black-and-gold feet and gold edge trims with the pillar assembly. Fit ornaments within the canvas without clipping. Keep the fine inner gold rim at about x94 on the left and x963 on the right. Keep the dark text panel continuous all the way to these rims; preserve and extend its existing subtle charcoal asanoha pattern naturally. Preserve a wide unobstructed text space.
Keep this exact canvas aspect and approximate original resolution 1057x1488; do not introduce new padding, scale the whole card down, zoom out, or shift anything vertically.
Preserve the existing vermilion lacquer, polished gold details, black foot collars, small white/pink sakura, top centered gold tomoe crest, top rail height, bottom rail height, and the exact shape and center height of the illustration/text divider in this input. Adjust short rail endpoints only as needed to connect to the shifted posts. Same polished anime-game painting and crisp details. Retain real alpha transparency in the illustration opening and outside the frame. No letters, banner, type badge, character, illustration, glow, background scene or watermark.
The finished frame must visibly touch much closer to both image edges through almost the entire height, with healthy pillar thickness.
Type-specific invariant: POWER — keep its rounded concave gold illustration/text dividing edge.
```

