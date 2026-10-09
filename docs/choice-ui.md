# 长门军书抉择界面

## 资源与场景

- 背景：`NagatoSpire2/images/ui/choice/nagato_choice_table.png`，内置 imagegen 生成，1672 × 941，接近 16:9。
- 场景：`NagatoSpire2/scenes/ui/nagato_choice_table.tscn`。
- 纯表现脚本：`NagatoSpire2Code/Nodes/Ui/NagatoChoiceTable.cs`。
- 游戏接入：`NagatoSpire2Code/Nodes/Ui/NagatoChoiceScreenSkin.cs`。
- 挂载补丁：`NagatoSpire2Code/Patches/NagatoChoiceScreenPatch.cs`，由 `MainFile` 注册。
- 中英日文文案：各语言的 `gameplay_ui.json`。

朱漆桌面、展开的和纸军书、樱纹金边与朱印构成长门的军议桌。背景不烘焙文字或选项，文案、选项和反馈由 Godot 绘制。

## 行为与范围

仅当官方单选或简单多选界面的全部选项都是 `NagatoChoiceOption` 时挂载。`NagatoChoiceCmd`、选择结果、伤害/能力结算、联网同步、回放和自动选择流程均沿用原实现。普通抽取/弃牌选择、其他角色的抉择界面不受影响。

原版 `NGridCardHolder` 保留图鉴相同的 0.8 → 1.0 悬停放大、离开时的 Expo 回弹、音效、关键词提示、右键详情与控制器导航。桌面额外显示低亮度金色框线。多选仍使用官方选中状态及确认按钮。

手部直接实例化官方 `NHandImage`，从角色的 `ArmPointingTexture` 获取现有宝箱房手素材，保留平滑跟随、入场和按下/松开动画。长门手素材为 422 × 1200，按 50% 显示并校准指尖到点击坐标。手与背景均不截获输入。

指尖位置通过实际 alpha 边界测量为 `(260.5, 38)`；半尺寸贴图的偏移为 `(-130.25, -19)`，旋转/按下中心相应设为 `(130.25, 19)`，避免顶部透明留白产生点击偏差。

手在纸面内替代鼠标光标；进入顶部 UI、查看战场、打开详情/地图/菜单或关闭界面时恢复官方光标管理。控制器下由当前焦点驱动指向。桌面加入官方 PeekButton 的隐藏目标。

以 1920 × 1080 为设计坐标，等比布局，选项中心 y=548，间距最多 390。父容器负责适配尺寸，避免与官方悬停缩放争用同一属性。缺少新版场景或背景时保留官方界面，避免仅更新 DLL 导致卡住。

## 验证与使用

项目 Release 编译通过。独立 Godot 场景验证了实际场景加载、所有装饰节点穿透输入、1/2/3 个选项在 16:9、4:3、超宽布局下悬停范围仍在纸面内，以及移出时框线淡出。预览中的选项为布局代理，不是游戏截图。尚需游戏内核对真实单选、多选确认、右键查看、窥视战场与控制器切换。

独立补丁检查进程针对本机安装的 `sts2.dll`，实际安装并撤销生产 Postfix，确认 `NChooseACardSelectionScreen._Ready` 和 `NSimpleCardSelectScreen._Ready` 均接受补丁及继承/私有 `_cards` 字段注入。该检查没有操作运行中的游戏或存档。

已在工作区独立目录导出 PCK，并运行项目的 Finalize-ModPack.ps1 清除宿主全局配置；新背景纹理、场景、脚本和三种语言文本均已包含。导出只有原有角色选择背景备份场景的重复 UID 警告，与本次抉择场景无关。

最终将实际 PCK 挂载到独立 Godot 预览工程，再次通过资源存在性、场景实例化和布局渲染检查。可测试的 DLL 与 PCK 位于 `D:/GitClone/.codex-build/nagato-choice-qa/`，未部署覆盖游戏安装目录。

代码和资源同时变化，运行时需更新 DLL 和 PCK。无需修改原版场景。

## 背景生图 prompt（内置 imagegen）

Use case: stylized-concept. Asset type: production 16:9 landscape game UI background, 1920x1080 composition, high resolution. Primary request: an elegant Japanese naval shrine commander's writing desk for Nagato from Azur Lane, seen almost directly overhead. A very large unfolded ivory washi military dispatch / strategy folio spreads across the center of a dark reddish brown lacquer wood desk. Crisp premium anime game background painting, clean linework, restrained detailed textures, warm diffuse afternoon light. Composition is functional: broad almost-flat blank ivory paper surface from x=16% to84%, y=22% to80%, generous unmarked area for three selectable cards side by side. Paper is one coherent large unfolded horizontal document with subtle creases, thin antique-gold border, red cord binding at the far left, tiny tasteful sakura crest on corners. Keep central surface low contrast, uncluttered, no columns or text or drawn cards. Around outside edges only: closed indigo Japanese military record booklet upper left, a small red seal and inkstone with calligraphy brush at upper right, subtly rolled chart lower left, a few pink sakura petals. Nagato's dignified Sakura Empire leadership mood via vermilion lacquer, black, ivory and restrained gold. Refined handmade fantasy naval briefing desk, not photorealistic. Top center and bottom center leave quiet space for runtime heading and controls. Slight cinematic vignette on outer tabletop only. No humans, no hands, no guns, no UI widgets, no buttons, no lettering, no readable text, no watermark. Finely rendered paper edges and wood grain, no excessive ornaments; all props stay outside the large central choice area.
