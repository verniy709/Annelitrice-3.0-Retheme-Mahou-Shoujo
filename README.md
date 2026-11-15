# Annelitrice 3.0 Retheme — Mahou Shoujo

适用于 RimWorld 1.7。启用原模组 **Annelitrice 3.0** 后加载本 Retheme。

本模组将相关物品、建筑、故事与四组技能表现改为魔法少女主题，包括心愿碎片、灵魂宝石、魔法棒棒糖、月光之泉与魔法少女舞台。英文内容位于 `1.7/Patches/`，简体中文位于 `1.7/Languages/ChineseSimplified/`。

## 四组特效与音效

| 原事件 | 中文主题 | 常速视觉时长 | 演出与声音 |
| --- | --- | --- | --- |
| Infest / `Anneli_Infest` | 分享魔法的奇迹 | 1.6 秒 | 翼心传递、爱心光印、小爱心气泡；开场动漫“啵”、大爱心亮响、气泡声 |
| Hatch egg / `Anneli_Hatch` | 让心愿与魔法降临 | 1.8 秒 | 愿望星降落、花冠与丝带展开、末尾闪星；柔和下行音、花朵绽放音与星光音 |
| Pupa start / `Anneli_PupationStart` | 魔力凝聚成水晶 | 2.0 秒 | 粉蓝丝带内聚、晶面封合、白金高光；上行拨弦铃音、三次晶面碰触、宝石叮声 |
| Pupa finish / `Anneli_PupationFinish` | 魔法少女闪亮登场 | 2.4 秒 | 光瓣与丝带展开、爱心星冠、舞台花环、星雨；亮相和弦、两拍回应与三声星屑；已去除开场中央菱形光壳 |

音效按视觉动作的游戏 tick 分段触发。声音的自然尾音可长于视觉，不会因视觉结束而被强制切断。前段及连续气泡序列提供 3、6、15 倍游戏速度的保留音高版本；单独的重音和闪星保留自然衰减。四组技能的自定义 SoundDef 维持 `volumeRange = 75~75`、`pitchRange = 1~1`。

四组声画表现已由用户完成游戏内测试。本次整理只调整目录、音频格式与说明；源码内容、演出几何、声点、音量及游戏逻辑保持原样。OGG 转换后已完成解码、波形对齐、音量与引用检查。更新后请重启 RimWorld 重新加载声音资源。

## 文件位置

| 内容 | 位置 |
| --- | --- |
| 特效程序集 | `1.7/Assemblies/Annelitrice.Retheme.MahouShoujo.dll` |
| 特效源码与构建项目 | `1.7/Source/Annelitrice.Retheme.MahouShoujo/` |
| 音效定义 | `1.7/Defs/SoundDefs/ShareMiracle.xml`、`WishArrival.xml`、`CrystalFormation.xml`、`ShiningDebut.xml` |
| 喷泉环境音定义 | `1.7/Defs/SoundDefs/FountainAmbience.xml` |
| 音频 | `Sounds/Anneli_Retheme_MahouShoujo/` 下四组技能目录与 `Fountain/` |
| 特效基础贴图 | `Textures/Anneli_Retheme_MahouShoujo/Anneli_Effect/` |
| Retheme 美术 | `Textures/Anneli_Retheme_MahouShoujo/` |
| 原事件接入 | `1.7/Patches/AbilityDef.xml` 与 `EffecterDef.xml` |

四组技能音频为 **Ogg Vorbis、48 kHz、双声道、质量等级 8**，共 30 份文件、33 个 SoundDef；喷泉另有一份 48 kHz 单声道、质量等级 8 的 OGG 和一个循环环境音 SoundDef，合计 31 份音频、34 个 SoundDef。定义中的 `clipPath` 不带扩展名；同一音频只保留一份 OGG，游戏目录不再存放对应 WAV。

源码按 `ShareMiracle`、`WishArrival`、`CrystalFormation`、`ShiningDebut` 四个目录归档，每组各含控制器、几何、声点时序三个文件。`MiracleGeometry` 中的点、颜色、标记与基础形状也被其他组复用；`MiracleAudioTiming.CastSpeed` 提供共用的速度分档。项目仅编译这四组目录中的 `.cs`，避免误包含构建缓存。命名空间及程序集名称统一为 `Annelitrice.Retheme.MahouShoujo`；声音定义使用 `Anneli_Retheme_MahouShoujo_` 前缀。

## 贴图与保留项

原有 75 张 PNG 保留原样；另有涟漪、扇形水花、舞台闪星和爱心四张粒子贴图。`Anneli_Effect/Solid.png` 是白色几何填充，`SoftGlow.png` 是透明柔光；爱心、星星、丝带、光环、晶面和光瓣由代码几何组合，不需要逐帧动画贴图。

灵魂宝石位于 `Anneli_Item/Pupa/`，有 A–I 九个随机变体；精灵有默认形象及 1–8 号变体，方向后缀与遮罩后缀按原模组使用。物品随机贴图目录、方向图、堆叠图和遮罩均参与运行，不应仅凭未直接出现在 XML 中就删除。

`Textures/Anneli_Item/Pupa/PupaA.png` 是刻意保留的原路径覆盖：原模组精灵组件的化晶开关图标直接引用此路径，因此它与 Retheme 路径中的水晶图并非可随意清除的重复项。

根目录保留 `.git` 与 `.gitattributes`。预览、原始 WAV、完整备份、转换检查报告及安装脚本存放在本次交付目录，不进入游戏资源目录。过期的贴图路径说明和空白图标说明已合并到本文件。

## 接入与原玩法

Infest 替换原目标特效组件，保留准备时间和原有技能结算。Hatch 同时覆盖组件和配方使用的原事件。水晶凝聚在原流程创建水晶前触发，控制器先记录旧物品，再接住同 tick 新生成的水晶。登场在角色释放后触发，上层装饰跟随新释放角色，脚下花环固定在出生处。

控制器只维护短暂表现，不改变角色移动、衣装、复活、阵营、容器转移、营养、成长和存档。暂停时视觉随游戏 tick 停止；离开当前地图的声点静默消耗；重载不恢复临时粒子。原生四事件的旧表现已在各接入口替换，避免与新表现重复播放。

物品与文本设置沿用现有版本：成人回收得到 30 个魔法棒棒糖、幼体 10 个，回收心愿碎片得到 5 个；治愈魔杖描述的是约五天内阻止新感染的预防作用。当前配方的 `Cook` 工作效果与 `Recipe_CookMeal` 工作声音改动也予以保留。

## 维护与构建

安装目录仅包含运行资源、源码项目与这份 README。程序集继续使用已通过游戏测试的版本；源码只是分组移动，内容未改动，并已通过重新编译验证。

构建环境沿用原项目的 .NET 9 SDK 与当前 RimWorld 提供的引用程序集。从源码项目目录执行 `dotnet build -c Release`，项目默认按 Mods 安装结构定位游戏目录；在其他位置构建时，通过 `-p:RimWorldDirectory="游戏完整路径"` 指定。发布时只将生成的 `Annelitrice.Retheme.MahouShoujo.dll` 放入 `1.7/Assemblies/`，构建缓存、PDB、调试输出和预览不进入模组。

完成整理：2026-11-15。完整备份与带校验的回退脚本随本次整理交付包保存。


## 月光之泉的涟漪与扇形水花（Fountain）

`Anneli_Fountain` 保留左右两处已确认的小涟漪，位置、大小、颜色、频率和寿命保持原样。新增两个原生 `CompProperties_ThrownFleckEmitter`，在左右落水点各播放低矮的扇形短水花：向两侧张开，随后压低、淡出，每次约 0.28 秒，左右分别每 9 / 11 tick 发射，约每秒 6.67 / 5.45 次；相邻水花交叠，表现持续不断的流水。水花尺寸为 0.165～0.209，中心高度在 -0.190～-0.180，略高于涟漪。没有重新加入独立飞溅水滴。

扇形水花使用单个连通的圆润水冠贴图 FanSplash.png，结合原生 CurvedScaler 控制横向展开和纵向收拢；静止在出生点，限制在下方水池，保持上方雕像清楚可见。专用定义在 `1.7/Defs/Effects/Fleck_Fountain.xml`，贴图在 `Textures/Anneli_Retheme_MahouShoujo/Anneli_Effect/Anneli_Fountain/`。

预览与游戏读取相同贴图，缩放时序由当前游戏的原生缩放函数核对。放置后持续播放，保留原有组件并使用 Normal tick。配合下方循环水声；不增加水资源、湿润、温度或配方结算，原有四组技能保持原样。更新后重启游戏加载；离线预览不是游戏实录。


### 喷泉循环水声

通过 `CompProperties_AmbientSound` 持续播放 `Anneli_Retheme_MahouShoujo_FountainAmbience`。声音使用实际喷泉录音，保留流水与落水水花的自然起伏。运行素材为 `Sounds/Anneli_Retheme_MahouShoujo/Fountain/FountainWaterLoop.ogg`，单声道 48 kHz、24 秒循环。经轻微滤波、响度调整与 1 秒等功率交叉淡化处理循环接缝。

环境音播放方式沿用 Dark Souls Retheme 的火焰音效，喷泉当前使用 `volumeRange = 20~40`、`pitchRange = 0.95~1.05`、`distRange = 5~25`，淡入 0.35 秒、淡出 0.50 秒。`sustain = true`、MapOnly，暂停静音，最近音源优先，最多同时保留两个声音。放置/载入地图后开始维持循环，移除建筑后停止；不要求建筑通电。

音源：[bubbling fountain.wav](https://freesound.org/people/cognito%20perceptu/sounds/36086/)，作者 **cognito perceptu**，Freesound #36086；授权 [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)。原作者描述为小型酒店室外喷泉的可循环实录。本版本使用该页面公开的高质量 MP3 进行截取、转单声道、滤波、循环与 OGG 转码，未使用账号限定的原始 WAV。素材可随模组分发；此处保留来源记录。

定义在 `1.7/Defs/SoundDefs/FountainAmbience.xml`；接入口与四个水面粒子组件合并在 Fountain 的条件补丁中，保留原有组件。更新后请重启游戏加载新增声音定义。

## 闪耀心愿舞台（PuppetTheatre）

`Anneli_PuppetTheatre` 使用六个原生 `CompProperties_ThrownFleckEmitter`：四组金白色四芒星在舞台上沿、两侧星饰和台沿明灭，两组轻透粉色爱心从左右缓缓向外上浮。中央角色面部保持清楚可见；四个朝向共用环绕舞台的分布。

闪星每次约 0.46 秒，爱心约 1.08 秒，错开频率、大小、颜色与轻微旋转，避免整齐闪烁。专用定义为 `1.7/Defs/Effects/Fleck_PuppetTheatre.xml`，两张实际粒子贴图位于 `Textures/Anneli_Retheme_MahouShoujo/Anneli_Effect/Anneli_PuppetTheatre/`。预览直接使用这两张贴图和同一组发射参数。

放置后持续播放，使用 Normal tick，保留现有组件与观看娱乐逻辑。没有新增舞台声音；原有建筑贴图、四组技能、音效、源码和程序集保持原样。更新后重启游戏加载；离线预览并非游戏实录。
