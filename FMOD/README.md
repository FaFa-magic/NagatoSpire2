# Nagato FMOD 音频

实现沿用最新 Janus：仅发布自有 `Nagato.bank` 与 `Nagato.guids.txt`，由 RitsuLib 0.6.2 延迟加载到游戏现有 FMOD Studio 系统。不安装、初始化或发布 Godot FMOD 插件，不发布 Master bank，也不自行更新/关闭游戏的音频系统。

所有事件采用 0 dB（100%）增益，路由到游戏的 `bus:/master/sfx`，GUID 为 `{31a6a70e-1f02-4134-9316-869034f110cd}`。保留游戏 mixer GUID 是为了路由；Nagato bank 和事件 GUID 独立，不能复用 Janus/WineFox 的事件 GUID。

| 事件（`event:/sfx/nagato/`） | 音源 | 触发 |
| --- | --- | --- |
| attack | Nagato_attacksfx.mp3 | 攻击动画，每位玩家每回合仅第一次 |
| cast | Nagato_castsfx.mp3 | 施法动画 |
| death | Nagato_deathsfx.mp3 | 角色死亡 |
| character_select | Nagato_character_select.mp3 | 选角/换肤，替换旧语音 |
| character_transition | Nagato_character_transition.mp3 | 开始游戏过渡，先停止选角语音 |
| combat_start | Nagato_opensfx.mp3 | RitsuLib CombatStartingEvent，一场战斗仅一次 |
| orb_passive | tashkent_PassiveSfx.mp3 | 充能球被动音效配置 |
| orb_evoke | tashkent_EvokeSfx.mp3 | 激发炮弹充能球 |
| orb_channel | tashkent_ChannelSfx.mp3 | 生成炮弹充能球 |

充能球音源复制自 TashkentSpire2-ritsulib，与 TashkentSpire2 的同名文件 SHA-256 相同，因此无需同时安装 Tashkent。只替换声音配置，不新增充能球被动行为。

选角播放使用 Janus 相同的 `UseVanillaRouting=false`、Screen scope、ReplaceExisting 通道及立即停止/释放。切换角色、换肤、出发、关闭选角或退出场景时清理；释放失败保留句柄等待重试。攻击限流只影响表现，不改战斗数据或联机校验。

## 重建

安装本机 FMOD Studio 2.03.14 后执行：

```powershell
./FMOD/build-bank.ps1
dotnet publish NagatoSpire2.csproj -c Release
```

脚本允许通过 `-FmodCli` 指定其他安装位置。`create-events.js` 仅供从空工程首次创建事件，已有工程不要重复执行。日常编辑 Assets/事件后重新构建即可。

FMOD/Build、Logs、缓存、temp 不纳入 Git；FMOD/Assets 和 Metadata 是重建所需源文件，应保留。Godot 发布仅包含运行资源，不包含原始 MP3、FMOD 工程、Master bank 或临时测试；Finalize-ModPack.ps1 同时移除会覆盖宿主的项目配置/全局 UID 注册表，并校验音频资源存在。

已用游戏 FMOD 2.03.06 在独立静音进程验证九个事件各 20 次创建、开始、停止和释放。该验证不经过 Godot 桥接，也不替代游戏内回归；沿用游戏/RitsuLib 的桥接调用，因此不能保证所有原生访问违规均被消除。
