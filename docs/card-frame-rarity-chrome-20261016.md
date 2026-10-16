# 长门品质横幅与蓝卡内框 · 2026-10-16

本次使用 imagegen 内置工具编辑透明 PNG，不使用品质调色滤镜。

## 品质映射

| 品质 | 横幅 | 内框 |
| --- | --- | --- |
| Basic / Common | `nagato_Banner_common.png`：苍白、轻微风化 | 原 common 三种内框不变 |
| Uncommon / 其他普通品质 | `nagato_Banner.png`：原朱红鸟居不变 | 三种蓝卡内框改为明确的琉璃蓝 |
| Rare | `nagato_Banner_rare.png`：漆金、莳绘与华贵金饰 | 原 rare 三种内框不变 |
| Ancient | 原先古横幅和边框不变 | 不受本次修改影响 |

资源目录：`D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/`。

保留既有布局、无滤镜材质和品质底牌，不额外添加渲染节点或着色器。旧图备份位于 `art_sources/card_frames/rarity_chrome_20261016/`（已加 `.gdignore`）。

## 最终文件与验证

均保存到 `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/`：

- `nagato_Banner_common.png`、`nagato_Banner_rare.png`：2170 × 725，真实透明通道。沿用 TitleBanner 的现有 KeepAspectCovered 与 1.12 显示缩放，未增加节点或着色器。
- `nagato_portrait_border_attack.png`、`nagato_portrait_border_skill.png`、`nagato_portrait_border_power.png`：1437 × 1095，真实透明内部。沿用现有类型布局，不修改 common / rare 内框。
- 原 `nagato_Banner.png` 未覆盖，仍供蓝卡和普通回退使用。先古卡资源和专用分支不变。

显示几何不变：外框 X 1.06，攻击 / 技能 / 能力内框 X 分别 1.034 / 0.994 / 1.009，卡图 1.10，横幅 1.12。生成图为更高清的源 PNG，画布像素不等同于控件显示尺寸。

代码绑定：`NagatoSpire2Code/Cards/NagatoCardModel.cs` 的 `ResolveBannerTexturePath()` 根据模型实际品质选择；继续使用已有 `UnfilteredChromeMaterial`，不通过品质材质调色。

验证产物位于 `D:/GitClone/.codex-build/nagato-rarity-chrome-20261016/`：

- Release 编译通过，0 警告 / 0 错误。
- `NagatoSpire2.dll` SHA256：`12F0973FB3AE2E000E3308E6E21E1AC618AA4F374224DE702CD74DBC901DBF5E`。
- `NagatoSpire2.pck`：最终索引 651 项，移除了宿主全局配置 / 注册表，未部署到游戏安装目录。
- `rarity-preview-packed.png`：直接加载最终 PCK 渲染三种品质 × 三种类型。
- 九种组合的卡图周边最低覆盖率为 0.988；内框压柱比例为 50.0%–52.3%，左右单侧也通过检查，保留约一半柱子 / 一半卡图的布局。
- 最终 PCK 内贴图尺寸、抽样颜色和 alpha 与源 PNG 一致，未加载旧缓存。

Godot 导出日志仍有已有角色选择场景备份的重复 UID 提示，并在 savepack 完成后的编辑器程序集重载阶段报告 `.NET: Failed to load project assembly`；独立 Release 编译和最终包原生贴图加载验证均通过。未进行真实游戏内战斗截图验证。

## 最终提示词

### banner_common

输入（第一张为编辑目标，其余仅为材质参考）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_Banner.png`
- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_portrait_border_common_skill.png`

```text
Use case: precise-object-edit.
Asset type: transparent Japanese torii card-title banner, one production PNG.
Input image 1 is the exact banner edit target. Input image 2 is ONLY a material/color style reference for the matching inner border.
Preserve the existing complete torii beam silhouette, upturned tips, double crossbeam, positions and sizes of the two small end post fittings, centered tomoe crest, blank central title plaque, shimenawa rope and three shide, restrained sakura accents. Keep original framing, registration and the same very wide 657x173 canvas/aspect; don't zoom out or add padding. Do not add any objects outside its original silhouette. Keep the central title plaque dark enough for light-colored game text. No drawn words or characters. Background and gaps must remain real transparent alpha; no white fill, scene or fake checkerboard. Polished clean-lined anime-game painting, sharp enough for a small UI asset. Change surface materials and decoration only; do not move any part or alter opacity of solid objects.
Primary request: COMMON/white-rarity variation, a slightly timeworn and pale torii, coordinated with the ivory and softly silvery common inner border. Main crossbeams in faded warm ivory/ash-white lacquered wood with delicate visible wood grain, a few tiny hairline cracks and sparse small worn paint flecks, just a LITTLE aged rather than broken or ruined. Upper dark cap softened to weathered charcoal, slim muted silver/champagne metal rims instead of rich shiny gold. The tomoe crest is softly tarnished silver with a restrained pale-gold hint. Quiet ivory rope, white paper, a few pale pink sakura. Let tiny traces of old muted vermilion remain at joinery so it still belongs with the existing red outer torii pillars, but make the broad beam clearly PALE, not the original bright red. A dark desaturated wood nameplate with thin pale silver edges. Modest, dignified, subtly worn. Absolutely no heavy damage, broken beams, missing parts, moss, vines, excessive scratches, or added ornaments.
```

### banner_rare

输入（第一张为编辑目标，其余仅为材质参考）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_Banner.png`
- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_portrait_border_rare_skill.png`

```text
Use case: precise-object-edit.
Asset type: transparent Japanese torii card-title banner, one production PNG.
Input image 1 is the exact banner edit target. Input image 2 is ONLY a material/color style reference for the matching inner border.
Preserve the existing complete torii beam silhouette, upturned tips, double crossbeam, positions and sizes of the two small end post fittings, centered tomoe crest, blank central title plaque, shimenawa rope and three shide, restrained sakura accents. Keep original framing, registration and the same very wide 657x173 canvas/aspect; don't zoom out or add padding. Do not add any objects outside its original silhouette. Keep the central title plaque dark enough for light-colored game text. No drawn words or characters. Background and gaps must remain real transparent alpha; no white fill, scene or fake checkerboard. Polished clean-lined anime-game painting, sharp enough for a small UI asset. Change surface materials and decoration only; do not move any part or alter opacity of solid objects.
Primary request: RARE/gold-rarity variation, visibly more luxurious Japanese urushi lacquer and maki-e goldwork, coordinated with the rare black/vermilion/gold inner border in image 2. Rich glossy black and deep vermilion lacquer double beams, more substantial polished gold rims and finely engraved gold sakura/floral arabesques fitted WITHIN the existing beams, a refined raised golden tomoe crest. Gold metal joinery with crisp highlights and a few tasteful deep-red lacquer insets. Keep the central nameplate dark black-burgundy with exquisite thin gold borders and restrained gold floral engraving near its edges, leaving its middle plain for game text. The diagonal-shine lacquer and precise gold inlay should read as a genuine rarer and more expensive crafted torii, not a yellow-tinted version. A rich but controlled premium anime-game UI look. Keep rope, shide and sparse sakura in the same places. No extra hanging trinkets, gem clusters, birds, dragons, weapons, new silhouettes, glow, bloom or sparkles.
```

### blue_attack

输入（第一张为编辑目标，其余仅为材质参考）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_portrait_border_attack.png`

```text
Use case: precise-object-edit.
Asset type: existing transparent 2D card portrait inner-border PNG.
Input image 1 is the exact edit target. Repaint only its surface colors and finish to make this UNCOMMON/BLUE-rarity frame clearly blue, unmistakably different from the off-white common frame.
Primary change: replace ALL the broad ivory/white enamel panels along both vertical side rails AND the lower crossbar's side panels with saturated medium cobalt-blue/lapis-blue enamel lacquer. Blue should dominate at least 70% of the decorative panel area, not just a tiny central badge. Main blue approximately #3678BA with deep blue shadows #224E88 and crisp azure highlights #74BDF0; rich clear BLUE, not white, mint, turquoise, gray, or pastel. Make the lower central asanoha-pattern inlay a coordinated richer azure blue. Retain the existing thin gold rims, gold joinery and small engraved sakura/leaf motifs with restrained warm gold so they contrast against the blue. Small white shide lozenges may remain white, but no large white or ivory panels. Keep the detail density appropriate to uncommon, less lavish than the rare black/gold lacquer frame.
CRITICAL invariants: preserve the EXACT frame skeleton and registration, both vertical-rail positions, widths and heights, their straightness, all end caps and lower joints, the outline of the lower rail, its center height, inner opening and all transparency. Same 551x420 canvas and framing, no padding or zoom. Keep the lower rail's existing shallow V-shaped profile exactly. This is a material repaint, not new geometry: do not thicken, thin, shift, stretch or move the rails. Fully transparent background and opening, all colors baked into this PNG, no translucent overlays.
Style: the same polished clean-lined anime game UI painting as the input, precise gold trim and subtle blue-lacquer specular highlights. No full outer torii, title banner, text, badge, character, scene, flowers beyond the current outline, glows, extra ornaments, white background or fake checkerboard.
```

### blue_skill

输入（第一张为编辑目标，其余仅为材质参考）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_portrait_border_skill.png`

```text
Use case: precise-object-edit.
Asset type: existing transparent 2D card portrait inner-border PNG.
Input image 1 is the exact edit target. Repaint only its surface colors and finish to make this UNCOMMON/BLUE-rarity frame clearly blue, unmistakably different from the off-white common frame.
Primary change: replace ALL the broad ivory/white enamel panels along both vertical side rails AND the lower crossbar's side panels with saturated medium cobalt-blue/lapis-blue enamel lacquer. Blue should dominate at least 70% of the decorative panel area, not just a tiny central badge. Main blue approximately #3678BA with deep blue shadows #224E88 and crisp azure highlights #74BDF0; rich clear BLUE, not white, mint, turquoise, gray, or pastel. Make the lower central asanoha-pattern inlay a coordinated richer azure blue. Retain the existing thin gold rims, gold joinery and small engraved sakura/leaf motifs with restrained warm gold so they contrast against the blue. Small white shide lozenges may remain white, but no large white or ivory panels. Keep the detail density appropriate to uncommon, less lavish than the rare black/gold lacquer frame.
CRITICAL invariants: preserve the EXACT frame skeleton and registration, both vertical-rail positions, widths and heights, their straightness, all end caps and lower joints, the outline of the lower rail, its center height, inner opening and all transparency. Same 551x420 canvas and framing, no padding or zoom. Keep the lower rail's existing straight horizontal profile exactly. This is a material repaint, not new geometry: do not thicken, thin, shift, stretch or move the rails. Fully transparent background and opening, all colors baked into this PNG, no translucent overlays.
Style: the same polished clean-lined anime game UI painting as the input, precise gold trim and subtle blue-lacquer specular highlights. No full outer torii, title banner, text, badge, character, scene, flowers beyond the current outline, glows, extra ornaments, white background or fake checkerboard.
```

### blue_power

输入（第一张为编辑目标，其余仅为材质参考）：

- `D:/GitClone/NagatoSpire2/NagatoSpire2/images/card_frames/nagato_portrait_border_power.png`

```text
Use case: precise-object-edit.
Asset type: existing transparent 2D card portrait inner-border PNG.
Input image 1 is the exact edit target. Repaint only its surface colors and finish to make this UNCOMMON/BLUE-rarity frame clearly blue, unmistakably different from the off-white common frame.
Primary change: replace ALL the broad ivory/white enamel panels along both vertical side rails AND the lower crossbar's side panels with saturated medium cobalt-blue/lapis-blue enamel lacquer. Blue should dominate at least 70% of the decorative panel area, not just a tiny central badge. Main blue approximately #3678BA with deep blue shadows #224E88 and crisp azure highlights #74BDF0; rich clear BLUE, not white, mint, turquoise, gray, or pastel. Make the lower central asanoha-pattern inlay a coordinated richer azure blue. Retain the existing thin gold rims, gold joinery and small engraved sakura/leaf motifs with restrained warm gold so they contrast against the blue. Small white shide lozenges may remain white, but no large white or ivory panels. Keep the detail density appropriate to uncommon, less lavish than the rare black/gold lacquer frame.
CRITICAL invariants: preserve the EXACT frame skeleton and registration, both vertical-rail positions, widths and heights, their straightness, all end caps and lower joints, the outline of the lower rail, its center height, inner opening and all transparency. Same 551x420 canvas and framing, no padding or zoom. Keep the lower rail's existing rounded concave profile exactly. This is a material repaint, not new geometry: do not thicken, thin, shift, stretch or move the rails. Fully transparent background and opening, all colors baked into this PNG, no translucent overlays.
Style: the same polished clean-lined anime game UI painting as the input, precise gold trim and subtle blue-lacquer specular highlights. No full outer torii, title banner, text, badge, character, scene, flowers beyond the current outline, glows, extra ornaments, white background or fake checkerboard.
```

