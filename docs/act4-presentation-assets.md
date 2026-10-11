# 第四幕表现素材

生成方式：内置 ImageGen，未使用 CLI/API 回退。最终素材已复制进项目，头像保留真实透明通道。

## 长门顶部头像

素材：`NagatoSpire2/images/map/ink_stained_steel_sakura/nagato_miko_boss.png`

参考图依次为项目内 `character_icon_nagato.png`、`char_select_nagato.png` 和官方 `queen_boss.png`。前两张限定长门的角色特征，第三张仅提供游戏顶部图标的绘画风格。

最终提示词：

```text
Use case: stylized-concept. Create a recognizable Azur Lane NAGATO character head icon, NOT an abstract emblem, NOT a mask, NOT an animal face. Images 1 and 2 are strict character identity references: preserve Nagato's actual youthful pale HUMAN face, round facial proportions, dark straight bangs with their precise blunt shape, long black side hair, tall black fox ears with peach-and-ivory inner fur, rich golden irises, and her distinctive asymmetric crescent-shaped gold naval head ornaments with thin hanging gold tassels. Her visible pale face must occupy the central half of the icon. Give her a restrained proud, composed look, not an evil grin or monster face. Image 3 is only a brushwork/palette reference: simple chunky hand-painted outlines and restrained matte shading like a Slay the Spire 2 tiny HUD icon. Head and a little red-and-ivory shrine-maiden collar only, frontal, centered square, clear silhouette readable at 44px. No shield, no badge frame, no crown, no face-covering helmet, no sacred crescent replacing her bangs. NO BLACK MASK. Keep the actual Nagato face plainly visible. Transparent alpha everywhere outside the hair/ears/head/collar cutout. No background, no text, no UI, no watermark. One standalone production icon.
```

头像用于第四幕未来 Boss 和当前 Boss 房间的顶部图标。提示名称分别为「长门」「長門」「Nagato」。地图大头像保持不变。

第四幕遭遇绑定 `NagatoBossEncounter`，怪物为 `NagatoBoss`。两阶段均使用默认皮肤 Spine，基础生命值 500，循环单次 20 伤害攻击。第一阶段死亡时留场并强制进入神威觉醒意图，于下一次敌方行动恢复生命，改名「长门·神威」，启用模型背后的动态黑气与火焰；第二阶段死亡正常结束战斗。多人生命值和敌人格挡复用官方第三幕缩放，特效不使用战斗随机数。旧第四幕存档中的女王占位遭遇与未完成房间在加载、主机规范化时迁移，其他幕不变。

## 日式室内火堆背景

素材：`NagatoSpire2/images/rest_site/ink_stained_steel_sakura_rest_site.png`

场景：`NagatoSpire2/scenes/rest_site/ink_stained_steel_sakura_rest_site.tscn`

参考图为官方 `glory_rest_site_00.png`，仅用于绘画质感与明暗风格。

最终提示词：

```text
Use case: stylized-concept. Asset type: wide 16:9 painted 2D Slay the Spire 2 rest-site background plate. Reference image is only for the game's matte painted environmental style, coarse brush texture, dark quiet atmosphere, and low-detail foreground readability. Replace the ruined stone space with a traditional Japanese shrine's cozy indoor rest room at night. Dark warm timber beams, shoji paper doors, tatami floor, understated sakura decoration in an alcove, warm low lantern light, faint cool moonlight behind the shoji. View directly into the room from a low seated eye-level camera, slightly elevated enough to see the floor. Keep the lower middle 60% of the image as clear empty floor so four resting game characters can be drawn over it without covering furniture. Place a small dark recessed irori hearth centered at 49% image width and 72% image height, seen as a shallow low oval/rectangle in perspective. The hearth has a few dull embers but NO flames and NO smoke: animated game flames are added separately over that exact spot. No large table, no hanging kettle, no chair, no people, no characters, no ships, no naval weapons, no outdoor campfire logs. Lived-in, quiet, restrained, slightly worn Japanese materials. Warm brown, faded red, muted gold, subdued teal moonlight, not bright orange. Flat painted background with modest depth, not photorealistic, not 3D, not clean anime line art. Leave the top center uncluttered for rest option buttons; no text, no interface, no watermark. Full opaque edge-to-edge background, wide landscape.
```

场景通过 RitsuLib 的幕级休息背景接口绑定，继承官方休息背景并保留 `%RestSiteLighting`、动态火焰、烟雾和火星。室外木柴、碎石前景和灰尘层被隐藏。暖色火焰与粒子材质按实例复制，不修改官方共享资源。角色、休息选项、多人同步和存档仍走原有逻辑。

发布时需同时更新 DLL 和 PCK 并重新启动游戏。

第四幕保留官方角色的暖光休息动画与长门原有睡姿，仅统一移动四个玩家容器，围坐在围炉两侧的室内地板上。布局以正确落点和场景协调为优先，避开门槛与炉坑，允许适度前后遮挡。没有额外坐垫、座具、角色类型分支或角色内部节点修改。
