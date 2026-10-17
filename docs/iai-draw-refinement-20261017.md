# 居合拔刀衔接调整（2026-10-17）

## 最终采用方案

按用户最后确认，恢复原 `nagato_iai_release.png`，不再采用重画刀身的 release。蓄势 `nagato_iai_ready.png` 保持原样。两张图均通过逐字节/哈希一致性验证。

- ready SHA256：`266F21358E48D14D428DC48DCCC11B4F61722FBD1DB98514E716ED3CB3AC92AC`
- release SHA256：`87C36C8DF0CB4C61B9F8490D9766D997FFFD95A74FD8BEFBFDDD49C022FFD2FD`

保留一张新增初拔帧 `NagatoSpire2/images/vfx/iai/nagato_iai_draw_early.png`，1254×1254 真透明 PNG。顺序为 ready → draw_early → 现有 draw → 原 release。额外生成的晚拔刀帧有侧边裁切，不纳入项目；两版重画 release 均已弃用，不作为运行时资源。

## 动作与特效

- 出鞘段由旧版 0.90–1.51 秒（约 0.61 秒）调整为 0.99–1.45 秒（0.46 秒），约缩短 25%。
- 取消中间姿态长时间停留。四张注册姿态依次连接，每次相邻过渡约 61 毫秒；其余时间保持清晰的关键姿态，减少双手、双刀长时间重影。
- 新增 `iai_pose_blend.gdshader`，在单次绘制中以带 alpha 的混合衔接相邻姿态，避免普通多层叠化导致固定脸部忽暗忽明；不逐帧创建图片、离屏渲染或修改原 PNG。
- 刃口附加短残光、细小闪点和少量樱花碎粒（总计 30 个短命数学粒子）；只在拔刀阶段出现，不遮脸、不延长演出、不消耗战斗随机数。轮廓路径按保留的原 release 更新。
- 1.50 秒全屏樱花蓄势窗口、官方月牙、伤害时序、收尾整图文字与两段各 50% 概率均未改变。

场景 `nagato_iai_sakura.tscn` 和 `Iai.ExtraRunAssetPaths` 已绑定补帧与新材质，移除了未采用 late 图的引用。

## 生成方式与实际提示词

使用内置 imagegen 编辑，未使用 CLI/API 回退，未用程序修改生产人物画。输入 1 为不可改的 ready 注册锚点，输入 2 为现有半出鞘 draw 作为动作方向参考。最终只交付以下新初拔帧：

```text
Use case: precise-object-edit.
Asset type: a registered transparent animation key pose for NagatoSpire2's frontal eyes-closed iaido cut-in.
Input image 1 is the immutable READY pose and exact registration anchor. Input image 2 is the current half-drawn pose, for arm motion continuity and costume details only.
Strict invariants: preserve the exact square 1254x1254 framing, head/face position and size, CLOSED eyes, ears, bangs, expression, hairpins, torso/obi position, palette, lighting, polished crisp anime drawing and transparent background. No head rotation or zoom. Keep moderate hair volume. Only arms/hands, sleeves, sword and slight outer ribbon/hair-tip response may move.
The READY pose itself must not be changed. The new pose must align to it for animation. Both lateral outlines (hair, sleeves, sword hilt/tip) must terminate naturally INSIDE the image with transparent margins at least 18 px, ears complete, lower waist softly faded as source. No crop, extra limb, fingers, double sword, detached blade, text, effects, background, collage, watermark. Exactly ONE key pose with true alpha.

Primary request: create ONE EARLY DRAW inbetween, between the READY and half-drawn poses.
Keep left hand/scabbard mouth near (790,850) stable. Right hand travels diagonally up-left from its ready position to roughly (500,735), and sword guard to around (605,770). Show only the first 150-190 pixels of silver blade, aligned with scabbard mouth. Keep a SINGLE guard at the hilt and a simple open metal rim on the scabbard; not two sword guards. The right sleeve has begun to extend without a large swing. All face/head/ear/torso pixels match the anchor's registration. One precise subtle inbetween, no afterimages or VFX.
```

## 验证

后续仅微调位置：四姿态等比放大约 8%–11%，下方约 1.8% 虚化区超出屏幕，贴底显示；不再垂直居中。详见 `iai-vfx.md` 的“立绘最终位置微调”。人物图片与本记录的拔刀节奏不变。

- Release 全量 Rebuild：0 警告、0 错误；禁止自动部署游戏目录。
- 实际生产 VFX 源码在 Godot 4.5.1 原生预览，检查四帧权重归一、16:9/21:9/5:4 安全区域、原 ready/release 字节不变、出刀先于伤害、战斗取消释放等待及结束清理。
- 源 PNG 左右实体余量（像素）：ready 38/36；early 17/18；draw 26/19；原 release 19/17。
- 预览与检查脚本位于 `D:/GitClone/.codex-build/nagato-iai-cinematic-20261016/`；新预览文件为 `nagato-iai-continuous-draw-preview.gif`，旧预览保留。预览使用示意战场，不是游戏内战斗截图，未部署 DLL/PCK 或实机战斗验收。

