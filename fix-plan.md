# PWing 修复计划（已执行）

基线提交：`74c1119`（工作区原本干净）
执行原则：**挖掘节奏不变（每 tick 1000 次采样 / 单次洪水上限 1000）、渲染顺序与颜色不变、数值平衡不变、存档格式不变。**
结果：`dotnet build` 通过，0 error / 0 warning；变更 34 个文件，+835 / -1795 行。

## 执行状态

| 阶段 | 内容 | 状态 |
|---|---|---|
| 0 | 建立计划文件与基线 | 完成 |
| 1 | 崩溃与确定性错误 | 完成 |
| 2 | 移除自写自动保存 + 多人一致性 | 完成 |
| 3 | 采矿机性能（效率不变） | 完成 |
| 4 | 假玩家与渲染性能 | 完成（含 2 处评估后保留项） |
| 5 | 死代码与规范清理 | 完成 |
| 6 | 本地化整并与梦环文案 | 完成 |
| 7 | 构建与回归验证 | 构建/静态检查完成，游戏内回归待人工 |

---

## 阶段 1：崩溃与确定性错误（完成）

- [x] 1.1 采矿机白名单崩溃：删除 `SetUpList` 白名单预写入（`IsOre` 运行时读配置），`Add` 改索引器，去掉 `init` 懒初始化，改由 `PWing.PostSetupContent()` 调用一次。
- [x] 1.2 本地化键归位：`SlotInfo2`、`AzMinerOreWarning` 移到两个 culture 文件顶层。
- [x] 1.3 世界边缘越界：`PWingTesseractServant` 中 `Main.tile` 索引前加 `WorldGen.InWorld`。
- [x] 1.4 物品格边框错画：新增 `PendingPreDrawSlot`，仅在库存槽绘制期间执行 `PreDrawSlots`；越界防护；异常改 Logger 并 60 tick 限流。
- [x] 1.5 借体执行异常保护：`ItemCheckHack`、`PrepareDrawing`、`DrawFrontHandAndHeldProj`、`DrawMyHeldProjectile`、`HijackItemDrawing` 全部 `try/finally`。
- [x] 1.6 采矿机存档读取防护：缺失物品键时补空 Item。

## 阶段 2：移除自写自动保存 + 多人一致性（完成）

- [x] 2.1 删除自动保存体系与相关世界加载重置。
- [x] 2.2 删除 4 个自动保存配置项（tML 会自动忽略旧配置里的未知字段）。
- [x] 2.3 `PWingGlobalNPC` 仅记录击杀，且只在服务器/单人写入。
- [x] 2.4 新增 `RequestBossKills`：客户端 `OnEnterWorld` 请求，服务器定向回发。
- [x] 2.5 移除 `SyncGlobalCounter`、`SyncCreativeFlight` 与 `MachinaBoosterPlayer.SendPacket`。

## 阶段 3：采矿机性能（完成，效率不变）

- [x] 3.1 `dx/dy` 静态化、`HashSet/Queue` 复用、`types` 预分配数组、去 LINQ、`MineOreWithType` 复用已算出的掉落类型。**1000 采样 / 1000 上限 / 每 tick 执行全部保留。**
- [x] 3.2 `Main.dedServ` 改 `Main.netMode == NetmodeID.Server`，修复 host & play 方块不同步。
- [x] 3.3 UI 全量 `SendData` 改脏标记，仅操作或挖掘变更时发送。
- [x] 3.4 未实施（按计划）：`createTile` 索引快路径会改变模组矿石匹配路径。

## 阶段 4：假玩家与渲染性能（完成）

- [x] 4.1 `PWingFakePlayerHelper` 的 LINQ 全表查找改 `for` + `identity`。
- [x] 4.2 分身 `netUpdate` 改为「状态变化（≥3 tick）+ 位移 + 15 tick 心跳」。
- [x] 4.3 单色武器贴图按 `(物品, 类型, 槽位)` 缓存并在 Unload 释放；`FieldInfo/PropertyInfo` 静态缓存。
- [x] 4.4 绘制集合复用、贴图静态缓存、`GetFakePlayers` 改为复用缓冲。
- [x] 4.5 `lastUsedItem.Clone()` 仅在类型变化时执行。
- [x] 4.6 `PostUpdateMiscEffects` 无分身时跳过全弹幕扫描。

评估后保留（未改）：
- 未禁用远端 `UpdateItems`：远端玩家的分身表现依赖本地 `ItemCheck`，禁用会导致其他玩家看不到武器动画，与「保证现有功能」冲突。
- 未合并每个分身的 `spriteBatch.End/Begin`：`PlayerDrawSet` 为结构体且 `DrawFromCache` 接收 `ref`，合并需要额外缓存数组，收益低于风险。
- `DrawFakePlayers` 的 `Subspace` 分支保留（该类型当前无实例化路径）。

## 阶段 5：死代码与规范清理（完成）

- [x] 5.1 删除 `ImportantTilesWorld.cs`（395 行，含世界加载时的后台全图扫描）。
- [x] 5.2 删除 `PWingWorldgenHelper.cs`（仅被上者使用）、空文件 `PWingRenderSystem.cs`、`Content/Items/PWingGlobalItem.cs`、`CopyDust3/4`（含贴图）。
- [x] 5.3 移除 4 个条件恒假的 detour 与 `PostDrawPlayers` 空实现；保留 Hydro/Subspace 类型分支。
- [x] 5.4 清理未引用成员（`LastLightingMode/awaitTileCheck/MoonPhase*/SantuaryMoonPhase`、`superOverrideLightColor`、`Probe/Probe2`、`LoadValuesForCachedProjectileDraw`、`FakePlayerItemBlacklist`、`TesseractNumber`、`FullBrightThisDrawCycle`、`SmartCursorHover`、UI 死常量与 `dataStreamTimer/hoverProgress`、`AzafureMinerTile` 的 `inArea` 死块）。
- [x] 5.5 反编译强转与魔法数字清理（`((ModItem)this)`、`(IStorageProvider)(object)`、`(UIHandle)`、`(TileProcessor)`、`(SpriteEffects)`、`(AnchorType)11`、`DustID.RainbowMk2` 等）。
- [x] 5.6 统一 `PWing.Config`，删除 `PWingConfig.Instance`。
- [x] 5.7 `Unload` 清理静态引用与 GPU 贴图缓存（`SpriteDrawBuffer` 无 `Dispose`，改为置空）。

保留项：`PWingDrawStateID` 细分、超立方杖 1,000,000 伤害、梦环成长数值。

## 阶段 6：本地化整并与梦环文案（完成）

- [x] 6.1 合并两个 legacy 文件到 `Localization/<culture>/Mods.PWing.hjson` 后删除旧文件。
- [x] 6.2 删除伪键、空键、已删配置键与无对应成员的键；修正 en-US 自引用 Description。
- [x] 6.3 梦环 tooltip 全量本地化（`{0}` 占位符加引号），代码改 `Language.GetTextValue`。
- [x] 6.4 文案与代码对齐：「伤害 +20%/层」、「生命再生 +6/层」，数值未改。

## 阶段 7：验证

- [x] 7.1 `dotnet build` 0 error / 0 warning；两份 hjson 括号配平检查通过。
- [x] 7.2 全局搜索确认无 `CE.AzMiner`、`PWingConfig.Instance`、`ImportantTilesWorld`、`autoSave*`、`RealBorderDrawCycleSlot` 残留。
- [x] 7.3 `Identifier` 已改为 `PWing.AzMiner`（与 CalamityEntropy 隔离）。
- [ ] 7.4 游戏内人工回归（见下）。

## 人工回归清单

单人：
1. 采矿机放矿石过滤器，观察 10 分钟产出节奏与修改前一致；开白名单后可放入非矿石以外的白名单物品，非白名单物品应提示 `AzMinerOreWarning`。
2. 采矿机 UI 拖拽、左键交换、右键加一、Shift 合并/取出；物品格不应再出现超立方体边框。
3. 召唤 1 个与 10 个分身：逐槽位物品、左右键模式、充能、库存边框、武器描边、低精度模式。
4. 梦环 tooltip 中英文切换；飞行时长/速度/减伤/伤害百分比与实际效果一致。
5. 世界边缘召唤分身、贴墙战斗，确认无异常日志。
6. 击败 BOSS 后不再出现「世界已自动保存」，梦环层数增长正常。
7. 退出重进世界，采矿机存储物品保留；旧存档可正常加载。

多人（主机开服 + 1 客户端）：
1. 采矿机挖掉的方块在客户端同步消失；UI 中物品数量一致。
2. 分身弹幕数量正常（不重复施法），远端玩家能看到分身的武器动画。
3. 后加入的客户端梦环层数与服务器一致。
4. 检查客户端日志无 `PWing PreDrawSlots error` 刷屏。

## 回滚方式

所有改动未提交，`git diff` 可整体审阅；如需整体回退执行 `git checkout -- .` 并删除新增的 `fix-plan.md`。建议按阶段拆分提交后再逐阶段回滚。
