# ET框架学习笔记

***ECS架构***

> ## ET 8.X 学习笔记

### 1.目录结构

#### Model 和 ModelView是数据
- Mode层负责定义Entity和Component
- ModeView 主要存放与Unity交互的Component
- ET/DotNet/Model下的Client、Generate、Server、Share与客户端的Assets/Scripts/Model下的Client、Generate、Server、Share共用代码
- 

##### Hotfix 和 HotfixView 是逻辑
- Hotfix 主要定义System行为，对Mode层的相关数据进行操作，是主要逻辑层
- HotfixView 主要负责实现与Unity交互的System行为，是显示逻辑层
- ET/DotNet/Hotfix下的Client、Server、Share与客户端的Assets/Scripts/Hotfix下的Client、Server、Share共用代码

#### Core 为核心代码
- ET/DotNet 服务器端代码，其中Core模块实际与客户端的ET/Unity/Assets/Scripts/Core为同一份代码（即共用代码）

#### Editor 为编辑器相关代码
- Assembly 程序集编译相关代码逻辑
- BuildEditor ET BuildTool等相关内容

#### Loader 为程序的入口
- MonoBehaviour/Init.cs 为程序入口,其中CodeLoader为热更新逻辑
- CodeLoader代码逻辑中LoadHotfix加载热更新
- 然后执行Model/Share/Entry.Start() 开始进入游戏

#### ThirdParty 第三方插件库
- ET/DotNet/ThirdParty下的DotRecast,ETTask,Kcp,NativeCollection、TrueSync与客户端的Assets/Scripts/ThirdParty下的对应文件共用


 ### 2.组件定义与生命周期

#### Entity
- Scene（场景单元)：位于Core/Entity目录下;继承Entity,IScene;
- Unit(实体单元)：位于Model/Share/Module/Unit目录下;继承Entity,IAwake<int>
- UI(界面单元)：位于ModelView/Client/Module/UI目录下;继承Enttiy,IAwake<string,GameObject>,IDestroy
- Room(房间单元)：位于Model/Share/LockStep 目录下;继承Entity,IScene,IAwake,IUpdate
- Player(玩家单元)：位于Model/Server/Demo/Gate目录下;继承Entity,IAwake<string>

#### Component
- CurrentScenesComponent:可以用来管理多个客户端场景，比如大世界会加载多块场景
- UnitComponent:可以管理多个Unit实体,比如怪物、石头、NPC、玩家等
- UIComponent:管理场景上的UI
- RoomManagerComponent：管理场景中的房间
- UILSRoomComponent：管理帧同步对应的场景中的房间  
- PlayerComponent:管理玩家

#### System
- CurrentScenesComponentSystem 当前场景组件系统
- UnitComponentSystem 实体单元组件系统
- UIComponentSystem UI组件系统，管理Scene上的UI
- UILSRoomComponentSystem 管理帧同步状态下的客户端房间UI
- PlayerComponentSystem 管理玩家

#### 数据
##### Model:不依赖UnityEngine
- Client
- Server
- Share
- Generate 客户端和服务端共享的相关配置文件
##### ModelView:依赖UnityEngine
- Client

#### 逻辑
##### Hotfix:不依赖UnityEngine
- Client
- Server
- Share
##### HotfixView:依赖UnityEngine
- Client

#### Analyzer_Attribute （Core/Analyzer）
- ChildOf:标明父子关系
  - 子实体的父级实体类型约束
  - 父级实体类型唯一的 标记指定父级实体类型[ChildOf(typeof(parentType)]
  - 不唯一则标记[ChildOf]
- FriendOf：标明引用关系,例如System需要修改那个Component对应的数据等
  - 数据修改友好标记, 用于允许修改指定Component或Child数据的类上
  - 例如:MoveComponentSystem需要修改MoveComponent的数据, 需要在MoveComponentSystem加上[FriendOf(typeof(MoveComponent))]
- ComponentOf：标明所属关系
  - 组件类父级实体类型约束
  - 父级实体类型唯一的 标记指定父级实体类型[ComponentOf(typeof(parentType)]
  - 不唯一则标记[ComponentOf]
- EntitySystemOf：为System标明类型所属的静态System静态函数
  - 标记Entity的System静态类 用于自动生成System函数
- LSEntitySystemOf:为System标明类型所属的静态System函数
  - 标记LSEntity的System静态类 用于自动生成System函数
- DisableNew:添加该标记的类或结构体将禁止使用new关键字构造对象
- EnableAccessEntiyChild:访问Entity对应的子物体时使用
  - 当方法或属性内需要访问Entity类的child和component时 使用此标签
  - 仅供必要时使用 大多数情况推荐通过Entity的子类访问
- EnableClass:访问class类时使用
- EnableMethod:对于特殊实体类，允许内部声明方法的标签
- StaticField：静态字段需加此标签
  - valueToAssign:初始化时的字段值
  - assignNewTypeInstance:从默认构造函数初始化
- UniqueId:唯一Id标签
  - 使用此标签标记的类会检测类内部的const int 字段成员是否唯一
  - 可以指定唯一Id的最小值最大值区间

### 3.事件定义与发布


### 4.异步编程ETTask


### 5.客户端和服务器通讯



