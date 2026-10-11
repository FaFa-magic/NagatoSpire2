# 镇魂鸟居攻击特效

## 演出与绑定

- `SoulbindingTorii.OnPlay` 保持原来的封印手牌、计算伤害与升级规则。只把通用斩击替换为攻击的 `BeforeDamage` 专属鸟居演出。
- 0–0.22 秒：目标脚下的金白封印阵亮起。
- 0.22–0.80 秒：完整朱漆黑金鸟居从屏幕上方坠下，使用加速下落曲线而非等速滑动；建筑始终等比缩放，中心对准当前目标，柱脚与目标地面接触。
- 约 0.80 秒：鸟居重重落地，释放金白双层地面冲击环、碎屑与纸片、官方重击音、尊重玩家设置的强震屏，并释放原有伤害结算。
- 落地余辉保留 0.85 秒，鸟居短暂站立后淡去，目标死亡不会立即截断余辉。
- 快速模式 ×1.6；瞬间、测试、非交互模式不创建特效或增加等待。低帧率保证可见空中阶段；退出场景或战斗取消会释放等待并清理节点。只改变视觉，不使用战斗随机数。

## 素材

- 生产素材：`NagatoSpire2/images/vfx/soulbinding_torii/nagato_soulbinding_torii.png`，1254×1254 RGBA，真正透明背景，门洞也透明。
- 场景：`NagatoSpire2/scenes/vfx/nagato_soulbinding_torii.tscn`。
- 节点：`NSoulbindingToriiVfx`；伤害前接入：`NagatoSoulbindingToriiVfx`。素材、场景与重击音均加入卡牌运行资源预加载。
- 采用实际柱脚的非微透明边缘区域 `Rect2(17, 44, 1221, 1166)` 作为 Sprite 的显示/接触范围，避免透明画布留白造成悬空。PNG 未裁切或滤镜处理。
- 使用 imagegen 技能与内置 `image_gen` 生成，已将素材复制到项目中；未替换卡图、卡框、遗物或其他角色资产。
- 生成源：`C:/Users/15832/.codex/generated_images/01a0962c-fd42-7471-bad4-c4ed7591b726/exec-adb730bd-6bf5-4333-a505-e42794dd4b3d.png`。

## 最终生图 Prompt

```text
Use case: stylized-concept. Asset type: high-quality transparent 2D game VFX sprite, a single Japanese torii gate for a heavy descending attack in NagatoSpire2. Primary request: an exquisite, majestic Japanese shrine torii gate, full structure visible, rigid upright front view, almost orthographic with only a very slight view of the depth. Premium detailed anime game asset with confident clean linework and painted cel-shaded polished surfaces, readable when displayed 450 pixels wide. Vermilion red lacquer cylindrical pillars, a broad elegant black-lacquer slightly upturned double crossbeam with finely rimmed aged gold, gold metal joinery and restrained engraved sakura crests, black and gold heavy foot collars. A small unlettered central plaque with subtle gold floral crest, neatly braided ivory shimenawa rope with three small crisp white shide paper streamers under the beam. A few engraved/pale pink sakura details on the joints, restrained and dignified; do not engulf the gate with flowers. Robust heavy pillars, long enough for an enemy sprite to fit in the open gateway. Width-to-height of the actual gate approximately 1.15:1. Perfectly balanced straight architectural geometry: two equal vertical columns, coherent solid crossbeams, level feet. Lighting: refined warm-white directional highlights, carefully rendered red lacquer, quiet golden metallic sheen; keep design colors solid rather than translucent. Composition: one centered complete gate, square canvas with about 7% clear padding, nothing clipped. ALL background including the empty opening between the pillars must be genuinely TRANSPARENT alpha. No floor, no scenery, no characters, no rubble, no impact light, no dust, no glowing aura, no drop shadow outside the object, no motion blur, no text, no logos, no watermark, no white background or painted checkerboard. Create the gate only: descent and impact effects are animated separately in Godot.
```

## 验证

独立 Godot 检查工程位于 `D:/GitClone/.codex-build/nagato-soulbinding-torii-vfx-qa`，覆盖素材 Alpha、等比缩放、脚部落点、不同大小目标与镜头缩放、低帧率可见坠落、快速模式、目标移动、取消/退出、死亡后余辉与节点清理。预览中的敌人为占位形状，不能替代游戏实战验证。

2026-10-11 验证结果：

- 项目 Release 编译：0 警告、0 错误（禁用自动部署）。
- 保存最终透明接触区域后，原生 Godot 图形运行通过 111 项检查；挂载正式导出的 PCK 后通过 112 项检查，均正常退出。
- 对实际生产 DLL 与已安装游戏程序集进行独立只读探测：10 项模拟模式跳过及场景、素材、音效绑定检查通过。
- PCK 导出成功，最终封包校验通过 587 个资源，移除了独立项目的宿主全局配置项。导出记录仅存在原有角色选择背景的重复 UID 警告；本次新素材和新场景没有加载错误。
- 独立渲染预览：检查目录中的 `summon.png`、`descending.png`、`impact.png`、`afterglow.png`。未进行游戏内实战验证。
- 可用于手动安装测试的配套文件：检查目录中的 `NagatoSpire2.dll` 与 `NagatoSpire2.pck`。

检查产物保存在工作区，不自动覆盖游戏 DLL/PCK，也不修改存档。
