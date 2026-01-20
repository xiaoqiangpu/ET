using TrueSync;

namespace ET
{
    /// <summary>
    /// 物理世界系统
    /// </summary>
    [LSEntitySystemOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    [FriendOf(typeof(LSRigidBody))]
    public static partial class LSPhysicsWorldSystem
    {
        [LSEntitySystem]
        private static void Awake(this ET.LSPhysicsWorld self)
        {
        }

        #region OldCode--Update

        // [EntitySystem]
        // private static void Update(this ET.LSPhysicsWorld self)
        // {
        //     FP dt = LSConstValue.UpdateInterval;
        //
        //     // ==========================================================
        //     // 1. 预处理：解包 EntityRef 并筛选有效对象
        //     // ==========================================================
        //     
        //     // 使用 ET 自带的对象池列表 ListComponent，避免 GC
        //     using ListComponent<LSCollider> activeColliders = ListComponent<LSCollider>.Create();
        //
        //     foreach (EntityRef<LSCollider> colliderRef in self.Colliders)
        //     {
        //         //EntityRef 支持隐式转换为实体对象
        //         LSCollider collider = colliderRef;
        //         // 必须判空！因为引用的实体可能已经被 Destroy 了，但还没来得及从 HashSet 移除
        //         if (collider == null || collider.IsDisposed)
        //         {
        //             continue;
        //         }
        //         activeColliders.Add(collider);
        //     }
        //
        //     // ==========================================================
        //     // 2. 积分阶段 (Integration)：应用重力、速度
        //     // ==========================================================
        //     
        //     foreach (LSCollider collider in activeColliders)
        //     {
        //         // 获取父节点 Unit
        //         LSUnit lsUnit = collider.GetParent<LSUnit>();
        //         if (lsUnit == null) continue;
        //
        //         // 更新包围盒 (AABB)，用于后续检测
        //         LSPhysicsMath.UpdateAABB(collider);
        //
        //         // 获取刚体组件
        //         LSRigidBody rb = lsUnit.GetComponent<LSRigidBody>();
        //         
        //         // 如果有刚体且不是运动学的(Kinematic)，应用物理力
        //         if (rb != null && !rb.IsKinematic)
        //         {
        //             // 应用重力 (v = v + g * t)
        //             if (rb.UseGravity) 
        //                 rb.Velocity += self.Gravity * dt;
        //
        //             // 应用阻力 (v = v * (1 - drag * t))
        //             if (rb.Drag > 0) 
        //                 rb.Velocity *= (1 - rb.Drag * dt);
        //
        //             // 计算位移 (s = v * t)
        //             TSVector move = rb.Velocity * dt;
        //             
        //             // 将定点数位置转为 Unit 位置
        //             // 注意：这里是临时应用，后续碰撞检测可能会把位置修正回来
        //             TSVector currentPos = lsUnit.Position;
        //             TSVector nextPos    = currentPos + move;
        //             
        //             lsUnit.Position = nextPos;
        //         }
        //     }
        //
        //     // ==========================================================
        //     // 3. 碰撞检测与解算阶段 (Detection & Resolution)
        //     // ==========================================================
        //     
        //     // 迭代多次以解决复杂的堆叠碰撞 (Solver Iterations)
        //     int iterations = 2; 
        //     for (int k = 0; k < iterations; k++)
        //     {
        //         // 双重循环检测所有组合
        //         for (int i = 0; i < activeColliders.Count; i++)
        //         {
        //             for (int j = i + 1; j < activeColliders.Count; j++)
        //             {
        //                 LSCollider c1 = activeColliders[i];
        //                 LSCollider c2 = activeColliders[j];
        //
        //                 // A. 优化：如果两个都是静态物体（墙），不需要检测
        //                 if (c1.IsStatic && c2.IsStatic) continue;
        //                 
        //                 // B. Broad Phase：使用 AABB 快速剔除
        //                 // 如果包围盒没碰到，就绝对没碰到
        //                 if (!LSPhysicsMath.CheckAABB(c1, c2)) continue;
        //
        //                 // C. Narrow Phase：精确数学检测
        //                 // 如果发生碰撞，Interset 返回 true，并输出法线和深度
        //                 if (LSPhysicsMath.Intersect(c1, c2, out TSVector normal, out FP depth))
        //                 {
        //                     // D. Resolution：应用物理推挤，把人推开
        //                     ResolveCollision(c1, c2, normal, depth);
        //                 }
        //             }
        //         }
        //     }
        // }

        #endregion

        [LSEntitySystem]
        private static void LSUpdate(this ET.LSPhysicsWorld self)
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
                LSUnit lsUnit = collider.GetParent<LSUnit>();
                if (lsUnit == null) continue;

                // 更新包围盒 (AABB)，用于后续检测
                LSPhysicsMath.UpdateAABB(collider);

                // 获取刚体组件
                LSRigidBody rb = lsUnit.GetComponent<LSRigidBody>();

                // 如果有刚体且不是运动学的(Kinematic)，应用物理力
                if (rb != null && !rb.IsKinematic)
                {
                    // 应用重力
                    if (rb.UseGravity) rb.Velocity += self.Gravity * dt;
                    // 应用阻力
                    if (rb.Drag > 0) rb.Velocity *= (1 - rb.Drag * dt);

                    // 【新增】最大终端速度限制 (防止无限下落或飞天)
                    // 假设最大下落速度 20m/s，最大水平速度 20m/s
                    rb.Velocity.x = TSMath.Clamp(rb.Velocity.x, -20, 20);
                    rb.Velocity.y = TSMath.Clamp(rb.Velocity.y, -20, 20);
                    rb.Velocity.z = TSMath.Clamp(rb.Velocity.z, -20, 20);

                    // 移动
                    lsUnit.Position += rb.Velocity * dt;
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
            LSUnit u1 = c1.GetParent<LSUnit>();
            LSUnit u2 = c2.GetParent<LSUnit>();
            if (u1 == null || u2 == null) return;

            LSRigidBody rb1 = u1.GetComponent<LSRigidBody>();
            LSRigidBody rb2 = u2.GetComponent<LSRigidBody>();

            bool p1Move = rb1 != null && !rb1.IsKinematic;
            bool p2Move = rb2 != null && !rb2.IsKinematic;

            // [安全锁] 防止深度计算错误导致瞬移飞天
            // 如果一帧穿透超过 1米，说明逻辑错了，强制限制住
            if (depth > 1)
            {
                // Log.Warning($"[Physics] 异常穿透深度: {depth}，已钳制。");
                depth = 1;
            }

            // 缓冲距离，防止浮点抖动
            FP correction = TSMath.Max(depth - 0.001f, 0);

            // move 向量指向 c1 (因为 normal 指向 c1)
            TSVector move = normal * correction;

            // --- 情况 1: c1 是玩家(动)，c2 是墙(不动) ---
            if (p1Move && !p2Move)
            {
                // Move 指向 c1，所以直接加给 c1，把它推出来
                ApplyPos(u1, move);

                // [消除速度]
                // Normal 指向 c1。如果 c1 的速度是迎面撞墙（与 Normal 夹角 > 90度，Dot < 0），则消除分量
                FP velDot = TSVector.Dot(rb1.Velocity, normal);
                if (velDot < 0)
                {
                    rb1.Velocity -= normal * velDot;
                }
            }
            // --- 情况 2: c1 是墙(不动)，c2 是玩家(动) ---
            else if (!p1Move && p2Move)
            {
                // Move 指向 c1 (墙)。我们需要推 c2。
                // 所以给 c2 施加 -move (反方向)
                ApplyPos(u2, -1*move);

                // [消除速度]
                // Normal 指向 c1 (墙)。-Normal 指向 c2 (玩家)。
                // 玩家撞墙，意味着玩家速度方向与 -Normal 相反。
                // 即玩家速度与 Normal 同向 (Dot > 0)。
                FP velDot = TSVector.Dot(rb2.Velocity, normal);
                if (velDot > 0)
                {
                    rb2.Velocity -= normal * velDot;
                }
            }
            // --- 情况 3: 互撞 ---
            else if (p1Move && p2Move)
            {
                ApplyPos(u1, move * 0.5f);
                ApplyPos(u2, move * -0.5f);
            }
        }

        /// <summary>
        /// 辅助方法：应用位置修正
        /// </summary>
        private static void ApplyPos(LSUnit lsUnit, TSVector offset)
        {
            lsUnit.Position += offset;
        }

        [LSEntitySystem]
        private static void Destroy(this ET.LSPhysicsWorld self)
        {
        }
    }
}