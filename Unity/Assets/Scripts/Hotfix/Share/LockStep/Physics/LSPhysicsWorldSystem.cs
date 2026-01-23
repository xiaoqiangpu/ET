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
            FP dt = LSConstValue.UpdateInterval / 1000f;
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
                    if (rb.Drag > 0) rb.Velocity *= (FP.One - rb.Drag * dt);
                    // 【新增安全锁】限制最大速度为 20m/s (相当于 72km/h)
                    // 没有任何正常的游戏逻辑需要比这更快
                    FP maxSpeed = 20;
                    rb.Velocity.x = TSMath.Clamp(rb.Velocity.x, -1 * maxSpeed, maxSpeed);
                    rb.Velocity.y = TSMath.Clamp(rb.Velocity.y, -1 * maxSpeed, maxSpeed);
                    rb.Velocity.z = TSMath.Clamp(rb.Velocity.z, -1 * maxSpeed, maxSpeed);

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
            Log.Info($"pxq--LSPhysicsWorld Update----");
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

            // [核心修复 1] 引入 Slop (容错深度)
            // 允许 0.01m 的穿透不进行修正，这能消除微小抖动和不断上升的问题
            FP slop       = 0.01f;
            FP correction = TSMath.Max(depth - slop, 0);

            // 如果修正量为0，说明陷得不深，不需要推，直接退出
            if (correction == 0) return;

            // [核心修复 2] 百分比修正 (Baumgarte Stabilization)
            // 不要一帧把人推出去 100%，而是推 20%~80%，让它慢慢浮出来
            // 配合 Slop，这能让物理表现极其稳定
            // 0.2f ~ 0.8f 都可以，建议 0.5f
            TSVector move = normal * (correction * 0.5f);

            // --- 1. Player (c1) 撞 Wall (c2) ---
            if (p1Move && !p2Move)
            {
                ApplyPos(u1, move);

                // 消除速度 (保留)
                FP dot = TSVector.Dot(rb1.Velocity, normal);
                if (dot < 0) rb1.Velocity -= normal * dot;
            }
            // --- 2. Wall (c1) 撞 Player (c2) ---
            else if (!p1Move && p2Move)
            {
                ApplyPos(u2, -1 * move);

                FP dot = TSVector.Dot(rb2.Velocity, normal);
                if (dot > 0) rb2.Velocity -= normal * dot;
            }
            // --- 3. 互撞 ---
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