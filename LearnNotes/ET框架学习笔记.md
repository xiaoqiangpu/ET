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

#### Analyzer_Attribute[代码分析器附加属性] （Core/Analyzer）
##### 层级约束类：树状结构,防止认错父物体
- 1.[ChildOf(typeof(parentType)] 可以理解为专属插槽
  - 约定当前Entity/Component只能作为指定parentType的子节点

- 2.[ComponentOf(Type entityType)] 可以理解为专属挂件
  - 与ChildOf类似，通常用于传统的Component组件.它声明该组件只能挂载到指定的Entity类型上
  - 语义上更偏向于功能组件，而ChildOf更偏向于逻辑子节点

##### 访问权限类： ET 是 ECS 架构，数据（Entity）和 逻辑（System）是分离的.System可能访问不到Entity Private类型字段，所以开启对应特性
- 3.[FriendOf(Type classType)]：
  - 标记在System上，允许System访问对应classType（一般为Component）对应的private/protected成员
  - 例如:MoveComponentSystem需要修改MoveComponent的数据, 需要在MoveComponentSystem加上[FriendOf(typeof(MoveComponent))]
- 4.[EnableAccessEntityChild] 可以理解为访问子物体标签
  - ET默认禁止直接访问entity.Children字典，也禁止随意遍历子实体，为了防止逻辑深度耦合
  - 当方法或属性内需要访问Entity类的child和component时 使用此标签
- 5.[EnableMethod] 可以理解为对方法的特批通行证，可以将平时限制访问的方法开发调用
  - 例如Internal 方法等。通常用于底层或者非常特殊的逻辑场景

##### 系统绑定类：告诉框架，哪些系统负责管理那个组件的生命周期
- 6.[EntitySystemOf(Type componentType)]
  - 标记在System类上,告诉ET事件系统,当componentType（Component）执行对应的生命周期（例如Awake、Update等）,调用对应标记的System上的生命周期
  - 标记Entity的System静态类 用于自动生成System函数
- 7.[LSEntitySystemOf(Type componentType)]
  - 帧同步系统绑定类，逻辑原理同上，只能用于帧同步模块
  - 标记LSEntity的System静态类 用于自动生成System函数
##### 安全规范与元数据类
- 8.[DisableNew] 可以理解为禁止私自创建
  - 标记在 Entity/Component 类上。
  - 效果：如果你在代码里写 new MyComponent()，分析器直接报错。
  - 目的：强制你使用 parent.AddChild<MyComponent>() 或 Entity.Create()。因为 ET 的对象需要统一管理 ID、对象池回收和父子关系，自己 new 出来的对象是“黑户”，会系统崩溃。
- 9.[StaticField]
  - ET 支持代码热重载（不重启服务器更新代码）。热重载时，旧的静态变量如果还有值，会污染新的代码。
  - 在 ET 的热更层（Model/Hotfix），默认是不允许写 static 字段的，因为热更无法自动处理静态数据的迁移。
  - 加上 [StaticField]，表示你向分析器保证：“我知道我在干什么，这个静态字段我会在热更时手动处理（或者它不需要清理）”，分析器就不报错了。
- 10.[UniqueId]
  - 用于配置表或网络协议。给一个类绑定一个固定的数字 ID。
  - 目的：当你在代码重构修改类名时，只要 UniqueId 不变，序列化到数据库的数据或者网络协议就能依然对应得上。
- 11.[EntityClass]
  - 告诉分析器：虽然这个类看起来是个普通的 C# 类，但请把它当作 ET 的 Entity 对待，必须遵守所有 Entity 的规则（比如不能随便 new，字段不能随便写等等）


### 3.事件定义与发布


### 4.异步编程ETTask


### 5.客户端和服务器通讯

#### 基础概念理解
- 1.RPC（RemoteProcedureCall,远程过程调用）
  - 通俗理解：你（客户端）去餐馆点菜
    - 1.你对服务员说：我要一份红烧肉（发送Request请求）
    - 2.服务员记下了你的取餐号（RpcId），把单子送进了厨房
    - 3.你坐在座位上玩儿手机开始等待（await,挂起当前逻辑）
    - 4.厨房做好了，服务员端上来喊对应的取餐号（发送响应Response）
    - 5.你确认这是你点的菜（通过RpcId匹配），然后开始吃饭（恢复执行逻辑）
  - 技术定义：RPC的目的是让调用远程服务器代码的函数写起来就像本地函数一样简单
    - 如果没有RPC,你发消息可能就要写成两个割裂的函数（发送函数A和接受响应函数B）
    - 有了RPC，一行代码就能搞定，例如： 就像调用本地的一个 int Login() 函数一样.await 关键字让代码在这里"暂停"，直到服务器返回结果
    - > G2C_Login response = await session.Call(new C2G_Login() { Account = "abc" });

- 2.Session(会话):发送和接受消息的句柄
  - 代表一条网络连接
  - 客户端发送一个Session连接服务器（通常连接Gate）
  - 服务器持有客户端的Session

- 3.Gate(网管服务器)
  - 连接管理：作为客户端和服务器之间的中间层，网关负责维护大量客户端的网络连接（如TCP/UDP等长连接）,减轻业务服务器压力
  - 协议转换：将客户端发送的数据包进行解析、校验和转发，可能设计不同协议间转换（如HTTP到WebSocket等）
  - 路由与负责均衡：根据业务逻辑将客户端请求分发到不同的内部服务器（如游戏服务器、聊天服务器等）
  - 安全过滤：实现防火墙、鉴权、加密（如TLS/SSL）等安全机制

- 4.Opcode(操作码)：网络层收到二进制流以后先读Opcode,就能知道把这段二进制反序列化成那个C#类
  - 每个消息类（Protobuf定义）都有一个唯一的数字ID

- 5.MailBoxComponent（邮箱组件）:消息先发到邮箱，邮箱再分发给具体的Handler
  - ET 是 Actor 模型。如果一个实体需要接收消息，必须挂载 MailBoxComponent。
  
- 6.Fiber（纤程）:ET中为了实现像单线程一样的思维写代码，却能享受多线程的性能而设计的逻辑容器
  - Fiber是Actor模型运行的基础容器
  - Process(进程)、线程（Thread）和Fiber(纤程)比较：
    - Process-可以理解为：工厂厂房
      - 拥有独立的资源（内存）、独立的门禁（隔离）
      - 工厂A的东西，工厂B拿不到，除非开卡车过去运过来（跨进程通讯）
      - 类似就像ET中的Server.exe 启动起来就是一个进程
    - Thread-可以理解为：工厂的工人
      - 工人是真正干活的动力源（CPU执行）
      - 可以工厂可以有多个工人（多线程）
      - 工人共享工厂的资源（共享内存）
      - 痛点：如果两个工人同时抢一把锤子（共享数据），就会打架，需要排队（加锁，Lock）,这样会大大降低效率
    - Fiber-可以理解为：一条完成的任务流水线
      - 这是一份逻辑上的工作清单，比如处理登录任务或者处理地图1上的逻辑等
      - 关键点：ET的调度器（Scheduler）会安排一个工人（Thread）去执行这条流水线（Fiber）
      - 它是虚拟的，操作系统看不到Fiber,操作系统只看到工人（Thread）
  - 为什么要引入Fiber:
    - 游戏服务器开发中的经典矛盾：
      - 单线程模型：逻辑简单，不用加锁，代码好写，但无法利用多核CPU,性能有上限
      - 多线程模型：利用多核CPU,性能强，但数据竞争极其复杂，死锁问题频发，开发难度极高
    - Fiber完美融合二者优点：
      - 逻辑上是单线程的：
        - 在一个Fiber内部，代码是顺序执行的。永远不用担心在一个Fiber内会有另外的Thread来修改你的变量
        - 写代码高效简捷方便，不用加锁Lock
      - 物理上是多线程的：
        - ET底层有一个线程池（Thread Pool）
        - ET会开启多线程
        - 假如你有1000个Fiber（1000个逻辑任务）,ET调度器会自动将这1000个Fiber分配给多个线程去轮流执行
  - 深入理解ET中的Fiber特性：
    - 实体隔离（Entity Container）
      - 一切皆实体：ET中的所有逻辑都是Entity
      - 归属权：每个Fiber出生时都必须属于某一个Fiber
      - Root节点：每个Fiber都有一个Root Entity(根实体，通常是Scene)
      - 规则：同一个Fiber内的Entity之间都可以直接互相调用函数（同步执行）。
        但不同Fiber的Entity之间，绝对不能直接调用函数，必须通过发消息（Actor Message）通讯
    - 上下文切换
      - 当一个线程执行Fiber A时，它拥有Fiber A的上下文
      - 如果Fiber A没任务啦（或await了），线程会把Fiber A放下，去执行Fiber B
      - 这个切换是用户态（User Mode）完成的，比操作系统切换线程（Kernerl Mode）要快的多，开销低
    - 消息驱动
      - Fiber本质上是由消息驱动运行的。
      - Fiber有一个消息队列。其他Fiber发给它的消息会排队
      - 线程拿到这个Fiber,就是在一个循环里面吧队列内的消息处理完，处理完了就去下一个Fiber
  - 举例理解：假设开了一个地图进程（Map Process）
    - 1.进程：操作系统启动了一个.exe
    - 2.线程：ET框架会在在这个进程内启动当前CPU能提供的逻辑核心数的工作线程,例如4核CPU就可以启动4个线程
    - 3.Fiber:
      - 假如你再这个进程内开了100个副本（Instance）
      - ET会对应创建100个Fiber,每个副本对应一个Fiber
      - 副本A里的怪物打玩家，逻辑都在Fiber A里跑，不需要锁
    - 4.调度：M:N调度模型，即M个Fiber映射到N个线程
      - 多个线程轮询执行100个Fiber
      - 那个Fiber有玩家操作（有消息），线程就去跑那个Fiber



#### 客户端消息传递流程
- 客户端内主要纤程：
  - Main Fiber：客户端主纤程,负责游戏逻辑、UI更新、渲染等，在Unity主线程
  - NetClient Fiber：客户端网络纤程，负责网络连接、消息收发等，在独立的一个线程中
- 路由服务（详见配置表StartSceneConfig@s.xlsx）
  - RouterManager路由服务器
  - Router1
  - Router2
  - Router3
  - Router4
- Realm：网关负载均衡服务器
  - 登录/鉴权
  - 负载均衡（网关分配）
- Gate网关服务器：维护连接（长连接,玩家在线期间全程存在）、消息转发、断线重连等
  - Gate1
  - Gate2
- Map服务器（业务逻辑服务器）
  - Map1
  - Map2
- Location定位服务器：注册和更新ActorId(位置信息)
- 完整的交互流程：Main2NetClient_LoginHandler
  - 1.客户端Main Fiber-->NetClient Fiber:发起登录请求
  - 2.NetClient -->路由服务器:获取Router地址
  - 3.NetClient -->连接到节点路由服务器Router
  - 4.节点路由服务器-->Realm网关负载均衡服务器：请求分配网关服务器地址和连接令牌Key
  - 5.Realm网关负载均衡服务器-->Gate网关服务器：请求获取请求获取网关服务器地址和链接令牌Key
  - 6.Gate网关服务器-->下发给Realm网关负责均衡服务器
  - 7.NetClient 拿到网关服务器和连接令牌Key后和Realm服务器断开
  - 8.NetClient -->Gate网关服务器 发起长连接.并发送user data和key数据
  - 9.Gate网关服务器校验有效，则绑定Session，玩家正式上线
  - 10.之后的消息流转流程则是：Client<--->Gate<--->Map(业务服务器)

#### 同一进程内，两个不同的Fiber（纤程）通讯：
- 例如Main Fiber和NetClient Fiber之间通讯，通过ProcessInnerSender 进行消息通讯
- 如果要发送给服务器，则是Main Fiber 通过通过ProcessInnerSender先发送给NetClient Fiber的ProcessInnerSender,
  然后再有NetClient Fiber 对应的Server.ProcessOuterSender发送给服务器

#### 不同的进程间不同的Fiber（纤程）通讯（主要指服务器端）：
- 不同的进程间两个Fiber需要通过NetInner Fiber来进行网络消息通讯
  - 参考Server/Module/NetInner/A2NetInner_Message.sc
  - 参考Hotfix/Server/Module/Message文件内文件

#### 调用接口记录：
- 1.通讯协议Protobuf文件位于Unity/Assets/Config/Ptoto目录下
- 2.客户端Main Fiber通过ClientSenderComponentSystem 调用LoginAsync/Send/Call等API发送消息给NetClient Fiber
- 3.客户端NetClient Fiber通过A2NetClient_MessageHandler、A2NetClient_RequestHandler等向服务区发送消息
- 4.再经过Session.cs 中的Send和Call API将Message通过MessageSerializeHelper 序列化成Memory Buffer传递给服务器
- 5.服务器主动向客户端发送消息主要通过MessageSenderSystem系统Send/Call API
- 6.客户端通过 NetComponentOnReadInvoker_NetClient监听服务器端消息
- 7.Realm网关负责均衡服务器通过NetComponentOnReadInvoker_Realm监听消息
- 8.服务器端通过 NetComponentOnReadInvoker_Gate监听来自网关（客户端）的消息
- 9.Location定位服务器通过MessageLocationSenderComponentSystem可向Gate(客户端)发送消息


#### 消息接口类型
- IMessage:不需要返回的消息类型;IRequest/IResponse 请求返回类型
  - FrameMessage:客户端主动向服务器发送的帧消息
  - 
- ISession:ISessionMessage 不需要返回; ISessionRequest/ISessionResponse 请求返回类型
- ILocation:ILocationMessage 不需要返回;ILocationRequest/ILocationResponse 请求返回类型
- IRoomMessage:IRoomMessage 不需要返回

  

