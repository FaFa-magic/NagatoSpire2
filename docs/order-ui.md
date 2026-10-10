# 长门「布局」部署桌面

## 交付范围

仅修改「布局 / BattleDisposition」抽牌堆排序界面，不修改普通抉择界面、官方全局 UI、卡框、卡牌效果或联机同步协议。

- 背景：`NagatoSpire2/images/ui/order/nagato_order_table.png`，1672 × 941，按 1920 × 1080 设计区显示。
- 透明操作牌匾：`NagatoSpire2/images/ui/order/nagato_order_command_plate.png`，2172 × 724，复用为撤销、恢复原序和颁令按钮。保留生成文件原始 alpha；运行时忽略低于 16/255 的透明噪点计算 AtlasTexture 显示边界，不覆盖原图。
- 展示场景：`NagatoSpire2/scenes/ui/nagato_order_table.tscn`。
- 入口仍为 `NagatoSpire2/scenes/screens/card_order_screen.tscn`。
- 顶部无标题；全部说明、部署状态、操作按钮位于底部。
- 游戏原生确认按钮仅在此实例换肤，仍保留官方热键、点击和进出场动画。

## 流程

1. 按抽牌顺序展现卡牌。左上 01 是下一张抽取的牌，逐行从左到右。
2. 拖动到目标位置：插入并顺移中间的牌。拖动靠近滚动区域边缘可自动滚动。
3. 点击两张牌：仅交换这对卡牌。再次点击已选的牌可取消选择。
4. 点击「撤销一步」回退插入、交换或恢复原序；「恢复原序」也可被撤销。
5. 右键查看详情。查看战场时隐藏整张部署桌面和其所有操作区。
6. 「颁令」确认，沿用既有结果校验、联机同步和抽牌堆写入逻辑。关闭或意外移除仍回退原序。

新文案在 zhs/eng/jpn 的 `gameplay_ui.json` 中，使用 `NAGATO_ORDER.*`，不覆盖卡牌文案。

## 检查

- Release 编译：无错误、无警告。
- 独立 UI 回归：75 项通过。
- 工作区独立 Godot 检查：真实项目的场景、展示脚本、网格、覆盖界面和操作按钮代码；游戏模型、卡牌持有器、覆盖栈边界使用测试替身。
- 覆盖初始化、顺序标记、透明资产、鼠标拖动插入、交换、自选取消、撤销、恢复原序的撤销、拖动中禁用操作、查看战场、遮盖/恢复界面、确认、意外移除，以及四种窗口比例。
- 排版截图中的卡牌是布局占位，不是游戏内截图。正式界面使用原生 NCard / NGridCardHolder。
- 独立程序集检查通过真实已安装游戏的 NButton 和 IOverlayScreen 类型兼容性。
- 资源包在工作区导出并清理宿主全局配置，验证可挂载和场景/新图档可加载。
- 单独启动项目编辑器时，原有 NagatoMerchantCharacter 出现 TypeLoadException；加载实际游戏依赖的独立程序可解析该类型。未改动商人代码。另有原有角色选择背景的重复 UID 警告。
- 未写入游戏安装目录，未执行实际游戏或联机实测。

## 生图方式与最终 prompt

使用内置 imagegen，未使用 CLI/API fallback。背景生成后依据实际纸面位置布置卡牌区，运行时文字不烘焙进素材。

### 背景

Use case: stylized-concept. Asset type: production background texture for Azur Lane Nagato's fleet deployment / draw-pile reordering game UI, landscape 1920x1080 16:9. Top-down orthographic view of a quiet Japanese naval command table, polished dark vermilion lacquer wood, a vast unfolded ivory washi tactical naval chart covers center from x=230 to1780 y=165 to885 in a1920x1080 design. This entire center is exceptionally empty and low contrast to host five columns of actual game cards, faint graphite grid, extremely faint sea coastline only at perimeter, no cards baked in. Elegant flat detailed anime game illustration, crisp linework and restrained soft shadows, ceremonial leadership of Nagato, gold brass straightedge at far right, navy blue folded dispatch sleeve and golden cherry crest at far left, a few tiny cherry blossoms at corners, wax seals and restrained red braided cord near bottom edges. Upper 160 pixels dark quiet desk, absolutely no title, no banner or textual lettering there. Lower 160 pixels dark uncluttered lacquer desk reserved for runtime instruction and buttons. Broad practical spacious layout, expensive but simple, restrained Japanese shrine accents, not fantasy magical. Ivory, navy, vermilion, aged brass gold, delicate sakura. No people, no text, no labels, no numbers, no UI controls, no weapons, no heavy central decoration. Highest quality coherent hand-painted anime 2D game environment, usable flat UI backing texture.

### 操作牌匾

Use case: stylized-concept. Asset type: isolated blank horizontal command-plaque game UI component on genuine transparent alpha background. A very simple long Japanese naval deployment-table nameplate / order button, width to height exactly 4:1, centered and tightly filling frame with small transparent margin. Dark navy lacquer inset, straight vermilion wooden rails, narrow muted brass-gold edge, subtly chamfered corners, tiny restrained sakura crest engraved at the extreme left and right ends only. Flat front orthographic view, all outlines sharp, elegant premium anime hand-painted 2D RPG UI, restrained Nagato leadership and Japanese shrine ceremony, compatible with ivory washi naval command chart and vermilion wood table. Center 80% completely plain quiet dark navy for runtime cream text. No text whatsoever, no letters, no symbols in center, no huge torii roof, no cords, no tassels, no ribbons, no background scene, no shadows outside the object. The entire exterior must be transparent. This is one reusable UI plate, not a mockup or collection.

