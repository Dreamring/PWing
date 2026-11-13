# PWing（耀刃之翼）

Terraria tModLoader 模组源码仓库：成长型翅膀、超立方召唤物、阿萨福勒数字型采矿机，以及内置动漫材质包与独立时装系统。

- 仓库地址：<https://github.com/Dreamring/PWing>
- 当前版本：见 `build.txt`

## 功能特性

### 耀刃之翼
- 击败 BOSS 后持续成长
- 每击败一个 BOSS：飞行时间 +0.5 秒，飞行速度与加速度提升
- 每击败一个 BOSS：生命 +5、伤害 +20%、伤害减免 +0.5%、防御 +1、生命再生 +6、魔力上限 +5
- 每击败 5 个 BOSS：+1 召唤栏、+1 哨兵栏
- 模组会同步并保存每位玩家的 BOSS 击杀数

### 超立方杖
- 召唤最多 10 个虚空分身，自动使用背包底部一排 10 个槽位中的物品
- 每个分身占用 2 个召唤栏
- 弹幕从分身位置发射，而非玩家位置
- 立方体线条渲染可配置：散光强度、线条粗细、颜色饱和度、动画速度
- 可开关星球纹理、翅膀与武器显示
- 攻击力显示为“???”，实际伤害取决于分身使用的物品
- 支持多人游戏，击杀记录由服务器统一同步

### 阿萨福勒数字型采矿机
- 自动挖掘并收集地图上的矿石
- 6 个过滤器槽位、35 个物品存储槽位
- 支持宝石挖掘与白名单扩展
- 多人游戏中方块与库存数据保持同步
- 粒子与界面动画反馈

### 内置动漫材质包
- 客户端将材质包贴图映射到原版纹理索引
- 覆盖玩家体型（可选变体 0–9）、发型、翅膀、背后饰品、Extra 光效、发光遮罩与弹幕
- 专用服务器不加载客户端贴图
- 原版盔甲贴图不再被覆盖，改为独立时装
- 模组卸载时恢复原版纹理

### 动漫时装
- 5 套独立时装，共 15 件：
  - 莉莉·阿斯卡
  - 科尔·巴赫
  - 莉雅丝·吉蒙里
  - 拉姆
  - 蕾姆
- 每套包含头饰、上衣、下装，可装备在时装栏
- 所有时装均可在织布机（Loom）用 5 个丝绸（Silk）合成
- “时装衣橱”：新建角色初始携带，使用后打开选择界面；点击卡片领取整套时装，衣橱随后消失

### 模组配置
- 超立方渲染参数、星球/翅膀/武器显示开关
- 采矿机白名单
- 低精度模式

## 获取方式

| 物品 | 配方 |
| --- | --- |
| 耀刃之翼 | 工作台：20 铁锭 + 20 落星 |
| 超立方杖 | 工作台：1 雀杖（Finch Staff） |
| 阿萨福勒数字型采矿机 | 重型工作台：10 铁锭 |
| 时装（每件） | 织布机：5 丝绸 |
| 时装衣橱 | 新建角色初始物品 |

## 环境要求

- Terraria 1.4.4 / tModLoader 1.4.4
- .NET 8 SDK
- InnoVault 1.9.x

源码需位于 tModLoader 的模组源码目录：

```text
Documents/My Games/Terraria/tModLoader/ModSources/PWing
```

`PWing.csproj` 通过相对路径引用 InnoVault：

```text
..\ModAssemblies\InnoVault_v1.9.94.dll
```

请确保该 DLL 存在，或修改 `PWing.csproj` 中的 `HintPath` 指向实际版本。

## 构建

在项目根目录执行：

```powershell
dotnet build PWing.csproj
```

构建完成后，tModLoader 会自动打包生成 `PWing.tmod` 并放入模组目录。

## 目录结构

```text
PWing/
├─ Assets/
│  └─ TexturePack/          内置材质包贴图
├─ Common/
│  ├─ Configs/              模组配置
│  ├─ GlobalItems/          物品全局钩子
│  ├─ GlobalNPCs/           NPC 全局钩子
│  ├─ PlayerDrawing/        玩家绘制辅助
│  └─ Systems/              世界数据、材质包纹理覆盖
├─ Content/
│  ├─ AzafureMiners/        采矿机方块、处理器与 UI
│  ├─ Buffs/                增益
│  ├─ Dusts/                粒子
│  ├─ FakePlayer/           超立方分身与渲染
│  ├─ Items/                翅膀、超立方杖
│  ├─ Players/              玩家数据与初始物品
│  ├─ Projectiles/          召唤物与弹幕
│  └─ Vanity/               时装物品、套装注册表、衣橱 UI
├─ Localization/
│  ├─ zh-Hans/              简体中文本地化
│  └─ en-US/                英文本地化
├─ build.txt                模组元数据与版本号
├─ description.txt          模组介绍（游戏内）
├─ description_workshop.txt 创意工坊介绍
└─ PWing.csproj
```

## 本地化

- 文本文件位于 `Localization/zh-Hans/Mods.PWing.hjson` 与 `Localization/en-US/Mods.PWing.hjson`
- 新增物品、UI 或配置项时，请同时补充两种语言，保持键名一致

## 资源与致谢

- 内置材质包：**Khoir Anime Frame Overhaul**，作者 Khoir（Steam 创意工坊 ID `2505056400`）
- UI 与工具框架：InnoVault（模组运行依赖）
- 部分翅膀与召唤物设计参考了 Terraria 社区模组的公开思路，代码为本仓库重新实现

## 仓库

<https://github.com/Dreamring/PWing>
