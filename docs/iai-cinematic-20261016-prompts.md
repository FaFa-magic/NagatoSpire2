# 居合演出素材提示词（2026-10-16 / 17）

后续补帧见 `iai-draw-refinement-20261017.md`：新增初拔姿态，蓄势与原 release 图不改；本文件保留最初三姿态素材的生成记录。

生成方式：内置 imagegen，真实 alpha 透明 PNG；未使用 CLI/API 回退，未对生产 PNG 做程序绘图或字体替代。

## 参考与素材用途

角色识别参考：`art_sources/cards/iaido_20261007/nagato_identity_reference.png`。服装与刀具参考：旧 `NagatoSpire2/images/vfx/iai/nagato_iai_cutin.png`。收尾字图参考：用户提供的 GIF 与 `codex-clipboard-a3dcec32-0640-4e78-9588-6942822e4e2c.png`；仅借鉴排布和笔触，不复制参考文字或游戏背景。

长门身份资料：[动画官方角色页](https://azurlane-anime.jp/character/30.html)、[Crosswave 官方角色页](https://www.compileheart.com/alcw/chara/?page=ch16)。基于重樱旗舰、庄严与守护的角色特征原创两段文案，并非官方台词：`樱落无声，神意如刃。` / `此身为誓，护佑重樱。`，由用户确认各 50% 出现。

## 最终生产文件

均位于 `NagatoSpire2/images/vfx/iai/`：

- `nagato_iai_ready.png`、`nagato_iai_draw.png`、`nagato_iai_release.png`：三个注册对齐的闭眼拔刀姿态，各 1254×1254；画面左右有透明余量。
- `nagato_iai_verse_a.png`：樱落无声 / 神意如刃。
- `nagato_iai_verse_b.png`：此身为誓 / 护佑重樱。

每段字是一张完整 PNG：双列错落，左列先起，右列低约两字，左上樱花，窄长随性笔触。不是独立字块，不按两列分别计时。材质统一自上向下刷出，再亮起、碎散淡出。

## 实际提示词

### 正面闭眼起始姿态（初稿）

```text
Use case: stylized-concept.
Asset type: single production transparent anime upper-body cinematic animation key pose for NagatoSpire2's iaido attack.
Input image 1: Nagato identity reference ONLY (face, bangs, long upright dark fox ears with white inner fur, black-brown hair and gold shrine hair ornaments). Do NOT copy the water background or pose.
Input image 2: Nagato iaido costume/material/katana style reference ONLY; do NOT copy its three-quarter pose, enormous flowing hair or full-body composition.
Primary request: NEW STRAIGHT-FRONT VIEW, centered waist-up portrait of Azur Lane Nagato. Both eyes gently CLOSED, chin slightly lowered, serene concentrated commander/shrine-maiden dignity. Her right hand crosses the lower chest to grip the still-sheathed katana at her left waist (viewer right); her left hand firmly holds the black-lacquer gold-accented scabbard mouth. Sword remains completely sheathed in this ready-to-draw starting pose. Both hands visibly anatomically correct. Frontal head and shoulders, not profile, not turned away.
Costume: opaque modest red-white-navy formal Japanese kimono with restrained gold sakura edging, broad white outer sleeves, dark obi. Match image 2's palette and precision. Correct Nagato upright slender dark fox ears, white inner fur, blunt straight bangs, gold fan/chrysanthemum hairpins. Hair controlled and moderate-volume, falling behind shoulders with a few separated locks and short red ribbons; no enormous hair cloud.
Composition: vertical 3:4 canvas, ears to below waist, complete ears, head, both sleeves and hands with 6 percent transparent margins, no feet or full-body skirt. Face centered around x=.50,y=.27; waist/scabbard around y=.78. Pose must be suitable for later same-camera animation keyframes. She will be placed on screen RIGHT; do not pre-crop her body to one half.
Style: exceptionally clean polished high-end 2D anime game illustration, confident crisp outlines especially face, bangs and clothes, controlled cel shading with subtle painted gradients, beautiful detailed hands, precise folds, clear shapes. No messy painterly blocks, photorealism or chibi proportions.
Background: genuinely transparent alpha. No environment, no petals, no external light effects, no motion blur, no text, no frame, no watermark, no extra weapons or people. Exactly ONE static key pose, not a sprite sheet.
```

### 补全左右轮廓后的最终起始姿态

```text
Use case: precise-object-edit.
Asset type: transparent animation key pose, Nagato frontal eyes-closed iaido READY pose.
Input 1 is the edit target. Keep her exact face, CLOSED eyes, straight-front camera, upright Nagato ears, costume, hand grip, sheathed katana, line quality and color palette.
CRITICAL: the current image is cropped at BOTH left and right and at the ears. REFRAME OUTWARD and OUTPAINT all missing sides. Use a SQUARE transparent canvas, show her from complete ear tips to below the obi. Make the entire person-and-sword grouping SMALLER, only about 65-70% of canvas width. Reconstruct full flowing hair ends, full left and right kimono sleeve silhouettes/cuffs and the COMPLETE far-right scabbard tip. BOTH lateral silhouettes MUST curve naturally back inward and terminate INSIDE the image, with a wide continuously transparent margin on the far left and far right, at least 8% canvas width. NOTHING touches either side border. Also leave visible transparent space above both ear tips. Do not merely add padding to the already-cropped edges: actually draw the missing parts to completion. The lower torso can softly fade into transparency below the waist; never hard-crop the sleeves.
Retain poised eyes-closed frontal stance, right hand holding the katana hilt and left hand holding the scabbard. No drawn blade yet. Keep hair moderate-volume and controlled. High-end clean anime linework, modest opaque costume. Genuine transparent background, no scenery, no petals, no text or watermark. One complete key pose, not a collage.
```

### draw

```text
Use case: precise-object-edit.
Asset type: transparent registered animation keyframe for a Nagato iaido cut-in.
Input image 1 is the exact FIRST FRAME / character and registration anchor, not a loose style suggestion. Keep the identical SQUARE canvas, camera, frontal head, CLOSED eyes, mouth, bangs, ear tips, hairpins, shoulder positions, face size/position, costume and lighting. Eyes must remain CLOSED. Change ONLY the arms/hands/sleeves and katana pose as requested below, plus a very slight natural response of the outer hair tips. Do not rotate the head or body or zoom/reframe. Maintain this exact character identity and excellent clean anime outlines.
CRITICAL full silhouette: both lateral hair ends, both complete sleeve contours/cuffs and all parts of sword/scabbard MUST terminate naturally INSIDE the image, with transparent margin left and right. Also keep complete ear tips and the lower faded waist. No part can be cropped at ANY lateral boundary. No extra fingers, no extra arms, no duplicate weapons.
Genuine transparent background, no scenery, no petals or effects, no motion blur, no text, no watermark. ONE pose only, not a sheet.
Pose 2: the middle of drawing the sword. Keep the left hand holding the scabbard mouth in the SAME position. The right hand and hilt have moved diagonally toward VIEWER LEFT and slightly upward by about one palm/forearm length. Show the first 25-35 cm of bright clean steel blade between the sword guard and the scabbard mouth, aligned exactly with the scabbard opening so the draw is physically plausible. Right elbow opens a little, white sleeve folds stretch with the forearm. No large swing yet. Preserve the face pixel-aligned to input and leave the entire blade safely in the central canvas.
```

### release

```text
Use case: precise-object-edit.
Asset type: transparent registered animation keyframe for a Nagato iaido cut-in.
Input image 1 is the exact FIRST FRAME / character and registration anchor, not a loose style suggestion. Keep the identical SQUARE canvas, camera, frontal head, CLOSED eyes, mouth, bangs, ear tips, hairpins, shoulder positions, face size/position, costume and lighting. Eyes must remain CLOSED. Change ONLY the arms/hands/sleeves and katana pose as requested below, plus a very slight natural response of the outer hair tips. Do not rotate the head or body or zoom/reframe. Maintain this exact character identity and excellent clean anime outlines.
CRITICAL full silhouette: both lateral hair ends, both complete sleeve contours/cuffs and all parts of sword/scabbard MUST terminate naturally INSIDE the image, with transparent margin left and right. Also keep complete ear tips and the lower faded waist. No part can be cropped at ANY lateral boundary. No extra fingers, no extra arms, no duplicate weapons.
Genuine transparent background, no scenery, no petals or effects, no motion blur, no text, no watermark. ONE pose only, not a sheet.
Pose 3: the decisive completed draw, the right forearm has opened toward VIEWER LEFT and the gleaming katana blade is fully OUT of the scabbard in a controlled sweeping iaido finish. The left hand still holds the now-empty scabbard at her left waist (viewer right). The unsheathed blade sweeps toward viewer LEFT across the lower chest, angled gently upward, with hilt, guard, steel and full blade tip all clearly visible and anatomically connected to the correct right hand. Keep the blade inside the available transparent space, at least 4% canvas width away from the left border; do not crop it or grow a giant sword. Sleeves and short ribbons respond subtly to the quick draw. Expression stays serene with CLOSED eyes and frontal head perfectly registered to the input. A clean held end pose, no slash VFX or blur painted into the image.
```

### verse_a

```text
Use case: text-localization.
Asset type: a SINGLE transparent cinematic iaido calligraphy overlay for NagatoSpire2.
Input image 1 is the user's EXACT ART-DIRECTION reference: copy the composition rhythm, SAKURA emblem at the top, narrow spontaneous brush characters, staggered vertical two-line arrangement, luminous sakura-pink ink and Japanese finishing-move atmosphere. Do not copy its wording or dark game background.
Composition: tall portrait about 1:2.3 aspect. TWO STAGGERED VERTICAL COLUMNS within ONE unified transparent artwork. First four-character clause at viewer LEFT, starting near the top; second four-character clause at viewer RIGHT, starting about TWO CHARACTER HEIGHTS LOWER. Read each column top to bottom, then from the earlier left clause to the lower right clause. Above the first/left column, place ONE delicate outlined five-petal sakura blossom in the same luminous pink brush style, like the reference. Keep columns visually airy and irregular; not aligned side by side at the same top edge.
Lettering style: informal expressive Japanese brush-calligraphy, thin-to-thick but predominantly slender and elongated strokes, naturally varied handwritten character shapes with dry-brush gaps, gracefully tapered endings. Each glyph should be NARROWER horizontally than its height, not blocky, not a regular print/Mincho font, not a square seal font. Slightly dancing vertical rhythm like the screenshot. Sakura-pink light through the strokes, near-white luminous centers, a restrained soft pink edge aura and a very thin dark-plum edge so the letter shapes stay readable on any background.
Text accuracy is essential. Use EXACTLY the eight simplified Chinese glyphs below, once each, clearly recognizable. Ignore punctuation; the comma is expressed by the staggered columns. No other letters, no signature, no title, no stamp, no frame, no game UI, no background scenery, no opaque black panel. Genuine alpha transparency. Complete blossom and every brush end contained inside transparent margins. ONE composite artwork, never separate tiles or a sheet. The game will reveal this WHOLE image from top to bottom, then make it glow and dissolve.
Exact phrase: "樱落无声，神意如刃。".
LEFT earlier column top-to-bottom: 樱 / 落 / 无 / 声.
RIGHT later/lower column top-to-bottom: 神 / 意 / 如 / 刃.
No extra character. Use simplified 樱、无、声 rather than traditional variants.
```

### verse_b

```text
Use case: text-localization.
Asset type: a SINGLE transparent cinematic iaido calligraphy overlay for NagatoSpire2.
Input image 1 is the user's EXACT ART-DIRECTION reference: copy the composition rhythm, SAKURA emblem at the top, narrow spontaneous brush characters, staggered vertical two-line arrangement, luminous sakura-pink ink and Japanese finishing-move atmosphere. Do not copy its wording or dark game background.
Composition: tall portrait about 1:2.3 aspect. TWO STAGGERED VERTICAL COLUMNS within ONE unified transparent artwork. First four-character clause at viewer LEFT, starting near the top; second four-character clause at viewer RIGHT, starting about TWO CHARACTER HEIGHTS LOWER. Read each column top to bottom, then from the earlier left clause to the lower right clause. Above the first/left column, place ONE delicate outlined five-petal sakura blossom in the same luminous pink brush style, like the reference. Keep columns visually airy and irregular; not aligned side by side at the same top edge.
Lettering style: informal expressive Japanese brush-calligraphy, thin-to-thick but predominantly slender and elongated strokes, naturally varied handwritten character shapes with dry-brush gaps, gracefully tapered endings. Each glyph should be NARROWER horizontally than its height, not blocky, not a regular print/Mincho font, not a square seal font. Slightly dancing vertical rhythm like the screenshot. Sakura-pink light through the strokes, near-white luminous centers, a restrained soft pink edge aura and a very thin dark-plum edge so the letter shapes stay readable on any background.
Text accuracy is essential. Use EXACTLY the eight simplified Chinese glyphs below, once each, clearly recognizable. Ignore punctuation; the comma is expressed by the staggered columns. No other letters, no signature, no title, no stamp, no frame, no game UI, no background scenery, no opaque black panel. Genuine alpha transparency. Complete blossom and every brush end contained inside transparent margins. ONE composite artwork, never separate tiles or a sheet. The game will reveal this WHOLE image from top to bottom, then make it glow and dissolve.
Exact phrase: "此身为誓，护佑重樱。".
LEFT earlier column top-to-bottom: 此 / 身 / 为 / 誓.
RIGHT later/lower column top-to-bottom: 护 / 佑 / 重 / 樱.
No extra character. The second glyph of the right column is 佑. Use simplified 为、护、樱 rather than traditional variants.
```

