using TrueSync;

namespace ET
{
    /// <summary>
    /// 物理世界系统
    /// </summary>
    [EntitySystemOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    public static partial class LSPhysicsWorldSystem
    {
        [EntitySystem]
        private static void Awake(this ET.LSPhysicsWorld self)
        {
        }

        [EntitySystem]
        private static void Update(this ET.LSPhysicsWorld self)
        {
            // 1. 积分阶段 (Integration)：应用重力、速度
            // 遍历所有有 Rigidbody 的 Unit
            // 注意：这里需要遍历 Unit，但在 ECS 中更高效的是遍历 Component
            // 假设我们有一个管理器或者直接遍历 Colliders 找父亲

            
                       FP dt = LSConstValue.UpdateInterval;

            // --- 1. 遍历所有碰撞体 ---
            // 注意：不要在 foreach 里面直接 Remove，可能会报错，物理引擎通常会有 Cleanup 阶段，这里简化处理
            foreach (long id in self.ColliderIds)
            {
                // 【核心】：通过 InstanceId 获取实体对象
                LSCollider collider = self.Root().Get(id) as LSCollider;

                // 如果 collider 为空（可能刚被销毁）或者已经 Disposed，跳过
                if (collider == null || collider.IsDisposed)
                {
                    continue;
                }

                // 获取父节点 Unit (用于更新位置)
                Unit unit = collider.GetParent<Unit>();
                if (unit == null) continue;

                // 更新 AABB
                LSPhysicsMath.UpdateAABB(collider);
                
                // ... 处理积分移动逻辑 (参考之前的代码) ...
                LSRigidBody rb = unit.GetComponent<LSRigidBody>();
                if (rb != null && !rb.IsKinematic)
                {
                    // 简单的欧拉积分
                    if (rb.UseGravity) rb.Velocity += self.Gravity * dt;
                    if (rb.Drag > 0) rb.Velocity *= (1 - rb.Drag * dt);
                    
                    // 临时应用位置
                    TSVector move = rb.Velocity * dt;
                    TSVector curPos = new TSVector(unit.Position.x, unit.Position.y, unit.Position.z);
                    TSVector nextPos = curPos + move;
                    unit.Position = new Unity.Mathematics.float3(nextPos.x.AsFloat(), nextPos.y.AsFloat(), nextPos.z.AsFloat());
                }
            }

            // --- 2. 碰撞检测 ---
            // 为了避免 foreach 嵌套导致性能和迭代器问题，建议先把有效的 Collider 缓存到一个 List 中
            // 这是一个常用的物理引擎优化技巧
            using ListComponent<LSCollider> activeColliders = ListComponent<LSCollider>.Create();
            
            foreach (long id in self.ColliderIds)
            {
                LSCollider col = self.Root().Get(id) as LSCollider;
                if (col != null && !col.IsDisposed)
                {
                    activeColliders.Add(col);
                }
            }

            // 双重循环检测
            for (int i = 0; i < activeColliders.Count; i++)
            {
                for (int j = i + 1; j < activeColliders.Count; j++)
                {
                    LSCollider c1 = activeColliders[i];
                    LSCollider c2 = activeColliders[j];
                    
                    // ... 执行具体的检测逻辑 (Intersect) ...
                    // 逻辑同上一次回答
                }
            }
        }

        private static void ResolveCollision(LSCollider c1, LSCollider c2, TSVector normal, FP depth)
        {
            if (c1.IsTrigger || c2.IsTrigger) return; // 触发器不产生物理推挤

            Unit u1 = c1.GetParent<Unit>();
            Unit u2 = c2.GetParent<Unit>();
            LSRigidBody rb1 = u1.GetComponent<LSRigidBody>();
            LSRigidBody rb2 = u2.GetComponent<LSRigidBody>();

            bool p1Movable = rb1 != null && !rb1.IsKinematic;
            bool p2Movable = rb2 != null && !rb2.IsKinematic;

            TSVector moveVector = normal * depth;

            if (p1Movable && !p2Movable) // c1 动，c2 不动（墙）
            {
                ApplyPos(u1, moveVector); // 把 c1 推出来
                // 可选：清除垂直于法线的速度 (滑墙)
            }
            else if (!p1Movable && p2Movable) // c1 不动，c2 动
            {
                ApplyPos(u2, -1 * moveVector);
            }
            else if (p1Movable && p2Movable) // 两个都动
            {
                // 各退一半
                ApplyPos(u1, moveVector * 0.5f);
                ApplyPos(u2, -1 * moveVector * 0.5f);
            }
        }

        private static void ApplyPos(Unit unit, TSVector offset)
        {
            TSVector pos = new TSVector(unit.Position.x, unit.Position.y, unit.Position.z);
            pos += offset;
            unit.Position = new Unity.Mathematics.float3(pos.x.AsFloat(), pos.y.AsFloat(), pos.z.AsFloat());
        }



        [EntitySystem]
        private static void Destroy(this ET.LSPhysicsWorld self)
        {
        }
    }
}