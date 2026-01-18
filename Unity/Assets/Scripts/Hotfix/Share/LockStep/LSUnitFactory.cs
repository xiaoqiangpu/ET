using TrueSync;

namespace ET
{
	[FriendOf(typeof(LSRigidBody))]
	[FriendOf(typeof(LSCollider))]
    public static partial class LSUnitFactory
    {
        public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
        {
	        LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
	        LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
			
	        lsUnit.Position = unitInfo.Position;
	        lsUnit.Rotation = unitInfo.Rotation;

			lsUnit.AddComponent<LSInputComponent>();
            return lsUnit;
        }
        
        /// <summary>
        /// 在 Share 层创建障碍物 Unit
        /// 这个方法既可以在客户端预测时跑，也可以在服务器逻辑中跑
        /// </summary>
        public static Unit CreateObstacle(Scene scene)
        {
	        // 1. 获取当前场景的 UnitComponent
	        UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
             
	        // 2. 生成 ID
	        long id = IdGenerater.Instance.GenerateId();
             
	        // 3. 创建 Unit
	        // 这里的 1001 是 ConfigId。障碍物虽然不需要属性，但 Unit 结构通常依赖配置
	        // 确保你的 UnitConfig.xlsx 里有 1001 这一行，或者新建一个 1003 作为 Obstacle
	        Unit unit = unitComponent.AddChildWithId<Unit, int>(id, 1001);
             
	        // 4. (可选) 设置类型，方便后续逻辑判断
	        // unit.Type = UnitType.Obstacle; 
    
	        // 5. 此时 unit 只是一个纯逻辑对象，没有 GameObject，也没有 MailBox
	        // 它的 View 层表现（加载模型）由 EventSystem 处理
             
	        return unit;
        }
        
        // 2. 扩展：给玩家添加物理 (需要修改原有的 Create 方法或在外部调用)
        // 建议在原有 CreatePlayer 的逻辑后补充：
        public static void AddPlayerPhysics(Unit unit)
        {
	        // 加刚体
	        var rb = unit.AddComponent<LSRigidBody>();
	        rb.Mass = 1;
	        rb.UseGravity = true;

	        // 加碰撞体 (球体半径 0.5)
	        var col = unit.AddComponent<LSCollider, LSColliderType>(LSColliderType.Sphere);
	        col.Radius = 0.5f;
	        col.Offset = new TSVector(0, 0.5f, 0); // 抬高一点防止陷地
        }
    }
}
