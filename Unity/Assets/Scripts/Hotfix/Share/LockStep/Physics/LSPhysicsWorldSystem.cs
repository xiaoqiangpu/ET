using TrueSync;

namespace ET
{
    /// <summary>
    /// 物理世界系统
    /// </summary>
    [EntitySystemOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    [FriendOf(typeof(LSRigidBody))]
    public static partial class LSPhysicsWorldSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSPhysicsWorld self)
        {
            // 打印日志：包含组件名称和它挂载的场景ID
            
            Log.Info($"pxq--[物理系统] Awake---{self.Fiber().Root.Name}---SceneName: {self.GetParent<Scene>().Name}--ID: {self.Id}--InstanceId:{self.InstanceId}");
            
            // 打印当前已有的碰撞体数量 (刚初始化应该是 0，除非你在 Factory 里先加了阻挡)
            Log.Info($"pxq--[物理系统] 当前碰撞体数量: {self.Colliders.Count}");
        }

        [EntitySystem]
        private static void Update(this ET.LSPhysicsWorld self)
        {
            FP dt = LSConstValue.UpdateInterval;

            // ==========================================================
            // 1. 预处理：解包 EntityRef 并筛选有效对象
            // ==========================================================
            
            // 使用 ET 自带的对象池列表 ListComponent，避免 GC
            using ListComponent<LSCollider> activeColliders = ListComponent<LSCollider>.Create();

            foreach (EntityRef<LSCollider> colliderRef in self.Colliders)
            {
                //EntityRef 支持隐式转换为实体对象
                LSCollider collider = colliderRef;
                // 必须判空！因为引用的实体可能已经被 Destroy 了，但还没来得及从 HashSet 移除
                if (collider == null || collider.IsDisposed)
                {
                    continue;
                }
                activeColliders.Add(collider);
            }

            // ==========================================================
            // 2. 积分阶段 (Integration)：应用重力、速度
            // ==========================================================
            
            foreach (LSCollider collider in activeColliders)
            {
                // 获取父节点 Unit
                LSUnit unit = collider.GetParent<LSUnit>();
                if (unit == null) continue;

                // 更新包围盒 (AABB)，用于后续检测
                LSPhysicsMath.UpdateAABB(collider);

                // 获取刚体组件
                LSRigidBody rb = unit.GetComponent<LSRigidBody>();
                
                // 如果有刚体且不是运动学的(Kinematic)，应用物理力
                if (rb != null && !rb.IsKinematic)
                {
                    // 应用重力 (v = v + g * t)
                    if (rb.UseGravity) 
                        rb.Velocity += self.Gravity * dt;

                    // 应用阻力 (v = v * (1 - drag * t))
                    if (rb.Drag > 0) 
                        rb.Velocity *= (1 - rb.Drag * dt);

                    // 计算位移 (s = v * t)
                    TSVector move = rb.Velocity * dt;
                    
                    // 将定点数位置转为 Unit 位置
                    // 注意：这里是临时应用，后续碰撞检测可能会把位置修正回来
                    TSVector currentPos = new TSVector(unit.Position.x, unit.Position.y, unit.Position.z);
                    TSVector nextPos = currentPos + move;
                    
                    unit.Position = new TSVector(nextPos.x, nextPos.y, nextPos.z);
                }
            }

            // ==========================================================
            // 3. 碰撞检测与解算阶段 (Detection & Resolution)
            // ==========================================================
            
            // 迭代多次以解决复杂的堆叠碰撞 (Solver Iterations)
            int iterations = 2; 
            for (int k = 0; k < iterations; k++)
            {
                // 双重循环检测所有组合
                for (int i = 0; i < activeColliders.Count; i++)
                {
                    for (int j = i + 1; j < activeColliders.Count; j++)
                    {
                        LSCollider c1 = activeColliders[i];
                        LSCollider c2 = activeColliders[j];

                        // A. 优化：如果两个都是静态物体（墙），不需要检测
                        if (c1.IsStatic && c2.IsStatic) continue;
                        
                        // B. Broad Phase：使用 AABB 快速剔除
                        // 如果包围盒没碰到，就绝对没碰到
                        if (!LSPhysicsMath.CheckAABB(c1, c2)) continue;

                        // C. Narrow Phase：精确数学检测
                        // 如果发生碰撞，Interset 返回 true，并输出法线和深度
                        if (LSPhysicsMath.Intersect(c1, c2, out TSVector normal, out FP depth))
                        {
                            // D. Resolution：应用物理推挤，把人推开
                            ResolveCollision(c1, c2, normal, depth);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 碰撞解算：根据穿透深度和法线，把物体推开
        /// </summary>
        private static void ResolveCollision(LSCollider c1, LSCollider c2, TSVector normal, FP depth)
        {
            // 如果是触发器，只触发事件，不产生物理推挤
            if (c1.IsTrigger || c2.IsTrigger) 
            {
                // TODO: 这里可以抛出一个碰撞事件 EventSystem.Instance.Publish(...)
                return; 
            }

            LSUnit      u1  = c1.GetParent<LSUnit>();
            LSUnit      u2  = c2.GetParent<LSUnit>();
            LSRigidBody rb1 = u1.GetComponent<LSRigidBody>();
            LSRigidBody rb2 = u2.GetComponent<LSRigidBody>();

            // 判断谁能动
            bool p1Movable = rb1 != null && !rb1.IsKinematic;
            bool p2Movable = rb2 != null && !rb2.IsKinematic;

            // 计算推开向量 (沿着法线推出 depth 距离)
            TSVector moveVector = normal * depth;

            // 情况1: c1 能动，c2 不动 (人撞墙)
            if (p1Movable && !p2Movable) 
            {
                ApplyPos(u1, moveVector); 
                
                // 进阶：消除垂直于墙面的速度分量 (实现贴墙滑动)
                // TSVector normalVelocity = TSVector.Dot(rb1.Velocity, normal) * normal;
                // rb1.Velocity -= normalVelocity;
            }
            // 情况2: c1 不动，c2 能动 (墙撞人? 或者电梯)
            else if (!p1Movable && p2Movable) 
            {
                ApplyPos(u2, -1*moveVector);
            }
            // 情况3: 两个都能动 (人撞人)
            else if (p1Movable && p2Movable) 
            {
                // 根据质量分配推开距离 (这里简单处理：各退一半)
                ApplyPos(u1, moveVector * 0.5f);
                ApplyPos(u2, -1*moveVector * 0.5f);
            }
        }

        /// <summary>
        /// 辅助方法：应用位置修正
        /// </summary>
        private static void ApplyPos(LSUnit unit, TSVector offset)
        {
             TSVector pos = new TSVector(unit.Position.x, unit.Position.y, unit.Position.z);
             pos += offset;
             unit.Position = pos;
        }
        
        
        [EntitySystem]
        private static void Destroy(this ET.LSPhysicsWorld self)
        {
        }
    }
}