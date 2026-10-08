# 居合透明角色特写：生图记录

工具：内置 imagegen（不是 CLI / API fallback）。

编辑目标：用户提供的 `codex-clipboard-2e67c48e-12f5-4e50-a4ee-7459c7591d3d.png`。GIF 仅用于代码中的特写演出参考，不作为生图目标。

最终图片：`NagatoSpire2/images/vfx/iai/nagato_iai_cutin.png`。原图和现有卡图均保留。生成结果为 1448 × 1086 RGBA PNG，透明采样与边缘检查通过，边界无不透明像素。

## 第一版：补全与去背景提示词

```text
Use case: background-extraction + identity-preserving outpainting.
Asset type: transparent PNG character cut-in for the Nagato Iai card anticipation animation.
Input image 1 is the EDIT TARGET, not merely a style reference. Preserve its face, golden eyes, expression, long dark hair, straight bangs, black animal ears with white inner fur, ornate golden hair ornament and tassels, red/white/dark-violet Japanese kimono, floral patterns, obi, hand positions and the katana/sheath iaido pose as faithfully as possible. Preserve the same crisp high-quality anime line art and cel shading, palette and existing perspective; do not redesign the character or costume.
Primary request: extend the canvas around the existing illustration and complete everything cut off by its four edges: both complete ear tips, all flowing hair ends and red ribbons, full kimono sleeve edges, both hands, complete sword hilt and scabbard tip, and a natural continuation of the lower kimono so that the single character has a complete, self-contained silhouette. Include the full standing figure and hem/feet where the lower part must be completed, maintaining original proportions and modest kimono coverage. The character remains facing the viewer in the original three-quarter iaido pose, right hand on katana hilt at her waist; no new pose, no drawn giant blade, no additional swords.
Scene/backdrop: REAL alpha transparency, no black background, no scenery, no ground, no shadows outside the figure, no petals or glow baked into the image. Retain full color inside the character rather than turning her into a solid black silhouette; 'silhouette' here means a clean isolated cutout.
Composition: one completely visible character centered with generous transparent safety margin on ALL four sides, including hair and trailing ribbons. Use a landscape or square expanded canvas suited to the flowing hair. Do not crop the ears, any locks of hair, sleeves, clothing hem or weapon. Fill the available resolution with detailed clean artwork, not a collage. Fine hair strands have clean alpha edges without a dark matte/halo. No text, labels, logo, border or watermark.
```

## 第二版：耳形与发量修正（当前使用）

编辑目标：第一版 `nagato_iai_cutin.png`。第二张输入为项目内的 `images/characters/character_icon_nagato.png`，只用于长门耳形和头部辨识参考，不使用其 Q 版身体比例。改成细长直立耳、粉色耳内和少量白色绒毛；减少整体发量，尤其收减画面右侧头发。衣服、姿势、手和刀鞘保持原设计。透明度没有烘焙进图片，由代码设置为最高 0.80。

使用内置 imagegen、透明背景模式。当前输出替换项目同名资源；修改前图片备份位于 `D:/GitClone/.codex-build/nagato-iai/art/nagato_iai_cutin_before_head_v2.png`，不参与项目导出。

```text
Use case: precise-object-edit.
Asset type: transparent anime character cut-in for an iaido game skill.
Input images: Image 1 is the edit target (full figure in floral kimono); Image 2 is Nagato's existing game head icon, reference ONLY for ear anatomy and head identity, not chibi body proportions.
Primary request: edit ONLY the head, ears, and hair of Image 1 to look more accurately like Azur Lane Nagato. Preserve her refined small face, golden eyes, straight neat dark bangs and gold floral hair ornament. Ears must match Image 2: two tall slender upright tapered ears, narrow bases, dark outer edges and pale pink inner upper section, with only a small white tuft near the lower inner edge. NOT broad triangular wolf/fox ears, NOT huge white fluffy ear interiors; no extra human ears showing.
Reduce the overall hair volume substantially, approximately 40%; on the viewer's RIGHT reduce the hair mass even more, approximately 60%, leaving transparent negative space rather than a giant swirling wall of hair. Keep long dark hair, but fewer tidy flowing locks, less width, fewer curls, and a clear silhouette. Keep viewer-left hair flowing but also reduced. Keep the exact pose, body proportions, hand placement, sheathed katana, obi, floral patterns, colors, ribbons, kimono sleeves and hems, feet, lighting, crisp high-quality anime linework and cel shading of Image 1. Everything below the neck except hair overlapping it should remain unchanged. Do not redesign the costume or weapon.
Composition: same full figure and orientation, full ears, feet, garment tips and sword visible with clean margins; match original landscape 4:3 canvas. Actual transparent RGBA background, not black, no ground, no scenery, no effects, no lettering or watermark. Subject fully opaque; display opacity will be controlled in code, do not bake fading into the image.
```
