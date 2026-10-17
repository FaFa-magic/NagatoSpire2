# 居合：闭眼拔刀与樱花书法收尾

只为 `Iai.AfterCardDrawn` 的回合结束抽到触发添加演出，不修改官方特效、其他卡牌或 `IaiDrawPower` 的回合结束钩子。伤害仍由官方 `DamageCmd.Attack` 结算，数值保持 60 / 升级后 75，目标保持全体敌方。

## 时间与画面

- 0–1.50 秒：216 片樱花在全屏缓落（原版演出为 48 片、局限于战斗中部），背景轻微压暗，保持角色可见。
- 0.06–1.50 秒：正面闭眼长门特写位于屏幕右半边，以最高 80% 不透明度淡入、轻微推近。0.99–1.45 秒依次播放准备、初拔、半出鞘、原 release 四姿态，出鞘过程为 0.46 秒，比之前约缩短 25%。每次相邻过渡约 61 毫秒，辅以少量刃口残光和樱花碎粒；保留原蓄势图与原 release 图不改。它仍是四关键姿态的 2.5D 演出，不是逐帧手绘影片或新 Spine 骨骼。左右耳尖、袖口、发梢与刀具轮廓完整，整图等比放入右侧安全区域，不裁一半人物。
- 1.18 秒起：88 片定向樱花沿右下至左上方的曲线路径扫过，为左侧出刀和文字留白。
- 1.50–1.62 秒：鞘口象牙白、金色微光。
- 1.62–1.82 秒：复用官方 `slash_flipbook.png` 的成形月牙帧，突然出现再侵蚀。月牙移到左侧樱花斩轨，不遮挡右侧正面特写；出刀音效、Strong / Short 震屏保留。
- 1.82–2.32 秒：一张完整的樱花粉书法 PNG 自上而下刷出。图内是两列错落竖排，顶部带樱花，窄长随性笔触；共用一个刷出前沿，不按两列分别播放。两段原创文字各 50%：`樱落无声，神意如刃。` / `此身为誓，护佑重樱。`。
- 2.32 秒后：完整文字亮起粉白光晕。2.44 秒释放伤害等待，官方攻击结算前提交受击碎瓣、金色细光、扩散光环及 Strong / Normal 震屏。
- 2.59–3.49 秒：同一字图以平滑噪声侵蚀为细碎光墨，同时缓慢淡出；3.54 秒清理余辉。若官方攻击钩子延迟，则伤害前保持演出时钟，消散随实际结算顺延。

正常模式基础时长 3.54 秒，快速模式按 1.6 倍压缩，约 2.21 秒；瞬时模式、测试模式和非交互模拟不等待特效。震屏沿用游戏的强度偏好。樱花与光粒使用局部数学噪声；文案用独立 `System.Random.Shared.Next(2)`，均不消耗战斗随机数。一次演出只选择一段，玩家间允许文案不同，伤害与同步不受影响。

## 绑定

- 场景：`NagatoSpire2/scenes/vfx/nagato_iai_sakura.tscn`
- 角色姿态：`NagatoSpire2/images/vfx/iai/nagato_iai_ready.png`、`nagato_iai_draw_early.png`、`nagato_iai_draw.png`、`nagato_iai_release.png`，各 1254×1254，真实 alpha。ready 与 release 保留原图。
- 相邻姿态透明度稳定混合：`NagatoSpire2/shaders/vfx/iai_pose_blend.gdshader`；只在本特写实例使用。
- 收尾字图：同目录 `nagato_iai_verse_a.png`、`nagato_iai_verse_b.png`；每个文件已包含两列字和顶部樱花。
- 角色、定向斩轨与字图绘制：`NagatoSpire2Code/Nodes/Vfx/NIaiCinematicLayer.cs`。
- 整图刷出、发光及消散材质：`NagatoSpire2/shaders/vfx/iai_caption_brush.gdshader`。仅作用于这张收尾字图。
- 动画、全屏樱花与官方月牙帧播放：`NagatoSpire2Code/Nodes/Vfx/NIaiSakuraVfx.cs`
- 月牙颜色：`NagatoSpire2/shaders/vfx/iai_official_crescent.gdshader`；只对本实例做象牙白、樱粉映射，不改官方纹理或材质。
- 战斗位置、播放速度、音效及震屏适配：`NagatoSpire2Code/Nodes/Vfx/NagatoIaiVfx.cs`
- 触发、等待和伤害：`NagatoSpire2Code/Cards/Rare/Iai.cs`

动画自己的时钟释放伤害等待，避免独立计时器导致刀光与扣血错位。低帧率下保证成形月牙至少显示一帧再释放伤害；攻击钩子尚未完成时保持樱花与蓄势氛围，不重复出刀；结束战斗、角色死亡或移除场景时结束等待；攻击未结算时不留下受击特效。自动触发不再追加角色普通的炮击动画。

改动涉及 C# 和新场景资源，因此游戏内测试需同时更新 DLL 与 PCK。独立 Godot 渲染与时序检查不能代替真实战斗验收。

生图使用内置 imagegen，未修改原卡图。完整实际提示词及参考资料见 `docs/iai-cinematic-20261016-prompts.md`。旧 `nagato_iai_cutin.png` 与旧生图记录保留作历史参考，运行时不再使用。

2026-10-17 补帧与原 release 恢复记录、提示词及验证详见 `docs/iai-draw-refinement-20261017.md`。下列三姿态检查记录为补帧前版本，四姿态结果以该记录为准。

## 立绘最终位置微调（2026-10-17）

只调整 `NIaiCinematicLayer.PortraitRegionAt` / `PortraitRectAt` 的等比缩放和锚点，不改人物 PNG、原 release、拔刀时长或收尾文字。

- 右侧可用区域宽度由屏幕 47.5% 改为 49%，高度上限由 89% 改为 94%；连同缩放余量调整，16:9 立绘约放大 8%，21:9 约放大 11%。仍保留完整耳尖与左右轮廓，不侵占左侧文字区域。
- 取消垂直居中，统一锚定到屏幕底部。原图下方约 1.8% 的虚化边缘超出屏幕，叠加原有轻微呼吸漂移后，裁切始终小于图高 2.5%，不留下悬空横边。
- 四姿态与刃口粒子共用同一矩形计算，切帧不改变锚点。源图未重绘，原蓄势与 release 文件的字节一致性验证仍通过。
- Release Rebuild 与原生完整演出通过；16:9、21:9、5:4 检查涵盖放大量、头部/左右边界和贴底裁切范围。新预览：`D:/GitClone/.codex-build/nagato-iai-cinematic-20261016/nagato-iai-bottom-anchor-preview.gif`。未部署游戏或实机验收。

## 2026-10-17 验证

- Release 全量 Rebuild：0 错误、0 警告（`IsInnerGodotExport=true`，不自动部署游戏）。
- Godot 4.5.1 原生运行实际两份 VFX 源码：正常模式文案 A、快速模式文案 B 均通过；检查了出刀先于伤害、战斗取消无伤害释放等待、播放结束清理。
- 16:9、21:9、5:4 的右侧完整轮廓放置与姿态权重检查通过。三姿态源图左右实体余量分别为 38/36、26/19、19/17 像素。
- 正式 PackedScene 加载检查通过：三姿态、两段字图与刷出/发光/消散 shader 均有真实资源绑定。五张新 PNG 已导入。
- 独立预览与日志位于 `D:/GitClone/.codex-build/nagato-iai-cinematic-20261016/`。该预览使用示意战场，不是实际游戏内战斗验收；尚未部署 PCK/DLL 或运行游戏战斗。
- 项目整体编辑器导入仍记录既有的能量 VFX `_particles` 属性与角色选择背景 UID 警告，本次未扩大到这些无关问题；新增居合资源加载通过。
