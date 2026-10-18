# 长门能力与遗物图标（2026-10-18）

> 后续四项重绘：交叉火控仪、旗舰号令、齐射令、诱爆的当前版本请见 `art_sources/icons/naval_refit_20261018/README.md`，使用新版母图及预览；本文保留第一批制作记录。

本批使用内置 `image_gen`，每项独立生成透明 PNG，未使用 CLI/API 绘图。母图、完整 prompts、参考素材和旧图备份统一保存在 `art_sources/icons/naval_batch_20261018/`，该目录有 `.gdignore`，不会作为游戏纹理导入。

## 能力图标

| 中文名 | 文件名 / 类名 | 视觉意向 |
| --- | --- | --- |
| 诱爆 | SympatheticDetonationPower | 炮弹引发连续三段爆炸 |
| 鸟居 | ToriiPower | 参考一代遗物轮廓的朱红鸟居、注连绳与纸垂 |
| 神意 | DivineWillPower | 御币、纸垂与樱纹结饰 |
| 旗舰号令 | FlagshipCommandPower | 红白金色军配团扇 |
| 炮术教范 | GunneryManualPower | 蓝金炮术书册与炮弹封面 |
| 齐射令 | SalvoFirePower | 三联装舰炮同时开火 |

每项的游戏资源均位于：

- `NagatoSpire2/images/powers/big/<类名>.png`：256×256。
- `NagatoSpire2/images/powers/packed/<类名>.png`：64×64。
- 六个能力类各增加 `UsesPackedIcon => true`，使状态栏使用 packed，小提示大图仍使用 big；未更改能力机制、数值和触发时序。

使用现有 `tools/Build-PowerIconSizes.gd` 从同一母图导出大小图，仅做缩放与透明边缘 RGB 修正，不使用稀有度滤镜。

## 遗物图标与 outline

| 遗物 | 文件名 / 类名 | 视觉意向 |
| --- | --- | --- |
| 交叉火控仪 | CrossfireDirector | 双目测距仪、交叉指针、黄铜与深色舰装金属 |
| Z旗 | ZFlag | 保留碧蓝航线素材的四色旗面和白色旗杆，去掉装备 UI |

游戏资源均位于：

- `NagatoSpire2/images/relics/big/<类名>.png`：256×256。
- `NagatoSpire2/images/relics/packed/<类名>.png`：88×88。
- `NagatoSpire2/images/relics/outline/<类名>.png`：88×88。

现有 `NagatoRelicModel.AssetProfile` 已按类名绑定以上三条路径，不需要改变遗物效果代码。原 ZFlag 三张文件都是同一张 256×256 的紫色扇子占位图，本次替换为正确的旗帜与对应尺寸。

`tools/Build-NavalRelicIcons.gd` 负责可重现的机械导出：

1. 只裁去母图透明留白，保留完整物体；big 内容最长边 244px，packed 内容最长边 80px。
2. 从 `outline/BigShipsBigGuns.png` 中不透明樱粉色环的高频颜色直接取色，结果为 **#F96AB7**。
3. 在 packed 的 alpha 外派生圆形抗锯齿描边，再合成原彩色图标。参数半径为 **2.25px**，原生 88px 图中可见粉边约 **2px**，不是 5px 的粗边。
4. 对照 `BigShipsBigGuns` 和 `BigShipsBigGunsDivineMight` 的粉边像素测量：90% 分位的粉边至物体距离均为 2px，最远约 2.24px；新两张亦分别为 2px 和约 2.24px。

注意：旧 packed 与 outline 的彩色物体缩放略有差异，单纯比较两张透明外包围盒会把这部分差异误算为描边。最终校准依据是 outline 内粉色边缘本身。描边已烘焙进 PNG，不增加运行时滤镜或额外绘制节点。

## 参考资料与生成输入

- [Slay the Spire 1 Torii 素材与介绍](https://slaythespire.gg/relics/torii)：使用其中原遗物图作为鸟居造型参考，制作新的长门风格能力图标，不直接复制成最终图片。
- [碧蓝航线 BWIKI：Z旗](https://wiki.biligame.com/blhx/Z%E6%97%97)：下载页面展示的游戏装备图作为编辑输入。源图只有 115×115，且包含彩色装备底框、星级和页面标记；最终为基于原素材的背景提取及高清重绘，并非未经修改的原文件拷贝。
- 参考源文件：`references/sts1_torii.png`、`references/azur_lane_zflag_t0.jpg`。
- 八项完整的最终生成 prompt、对应参考路径及内置模式记录：`art_sources/icons/naval_batch_20261018/prompts.json`。
- 最终透明母图：同目录下的 `<类名>_master.png`，共八张。
- 被替换的旧游戏 PNG：同目录的 `previous/`。交叉火控仪、诱爆与鸟居此前缺少对应图片；这些新增图片没有旧图备份。

## 验证

- Release 全量重新编译通过：0 警告、0 错误。使用 `IsInnerGodotExport=true`，未自动部署 DLL 或生成游戏目录中的 PCK。
- 18 张新游戏 PNG 的尺寸、RGBA 透明与角落透明检查通过。
- Godot 重新导入结束，退出码 0；独立原生渲染验证逐像素核对了导入 `.ctex` 与 PNG 的 alpha 和不透明颜色，八项全部通过。
- 检查了六张能力图标在 64×64 深浅底色上的表现，以及两件遗物的 88×88 packed/outline 与两件现有鸟居遗物的对照。
- 最终原生渲染总览：`art_sources/icons/naval_batch_20261018/preview.png`。
- 未运行完整游戏战斗测试。项目编辑器加载另外报告现有 `NagatoMerchantCharacter` 类型加载、能量盘 `_particles` 属性及角色选择场景 UID 问题，未扩大本次范围修改；这些提示未阻止本批纹理导入与独立渲染验证。

验证脚本与完整日志位于 `D:/GitClone/.codex-build/nagato-icon-batch-20261018/`。
