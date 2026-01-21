using UnityEngine;
using TrueSync; // 引用定点数库

namespace ET.Client
{
    // 这个系统专门用于调试绘制
    [EntitySystemOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    [FriendOf(typeof(LSUnit))]
    public static partial class LSPhysicsGizmosSystem
    {
        [EntitySystem]
        private static void Awake(this LSPhysicsWorld self)
        {
            // 注册回调：当 Unity 要画 Gizmos 时，调用 DrawPhysics
            // 使用 lambda 捕获 self (PhysicsWorld 实例)
            LSPhysicsGizmosDriver.OnDrawGizmosCallback += () => DrawPhysics(self);
        }

        [EntitySystem]
        private static void Destroy(this LSPhysicsWorld self)
        {
            // 销毁时取消注册，防止报错
            LSPhysicsGizmosDriver.OnDrawGizmosCallback -= () => DrawPhysics(self);
        }

        private static void DrawPhysics(LSPhysicsWorld self)
        {
            // 如果对象已销毁，不画
            if (self == null || self.IsDisposed) return;

            // 遍历所有碰撞体
            foreach (EntityRef<LSCollider> refCol in self.Colliders)
            {
                LSCollider col = refCol;
                if (col == null || col.IsDisposed) continue;

                // 1. 获取中心点 (定点数 -> Vector3)
                TSVector centerTS = col.WorldCenter;
                Vector3 center = new Vector3(centerTS.x.AsFloat(), centerTS.y.AsFloat(), centerTS.z.AsFloat());

                // 2. 根据类型设置颜色
                if (col.IsStatic)
                {
                    Gizmos.color = Color.green; // 静态障碍物 (墙/地) 显示绿色
                }
                else if (col.IsTrigger)
                {
                    Gizmos.color = Color.yellow; // 触发器显示黄色
                }
                else
                {
                    Gizmos.color = Color.red; // 动态角色显示红色
                }

                // 3. 根据形状绘制
                switch (col.ShapeType)
                {
                    case LSColliderType.Sphere:
                        float radius = col.Radius.AsFloat();
                        Gizmos.DrawWireSphere(center, radius);
                        break;

                    case LSColliderType.Box:
                        Vector3 size = new Vector3(col.Size.x.AsFloat(), col.Size.y.AsFloat(), col.Size.z.AsFloat());
                        Gizmos.DrawWireCube(center, size);
                        break;
                    
                    case LSColliderType.Capsule:
                        // 简单画一个球代替，或者画线框
                        Gizmos.DrawWireSphere(center, col.Radius.AsFloat());
                        Gizmos.DrawWireCube(center, new Vector3(col.Radius.AsFloat()*2, col.Height.AsFloat(), col.Radius.AsFloat()*2));
                        break;
                }

                // 4. (可选) 绘制 AABB 包围盒 (用灰色显示，用于检查 BroadPhase)
                // Gizmos.color = Color.gray;
                // Vector3 min = new Vector3(col.BoundsMin.x.AsFloat(), col.BoundsMin.y.AsFloat(), col.BoundsMin.z.AsFloat());
                // Vector3 max = new Vector3(col.BoundsMax.x.AsFloat(), col.BoundsMax.y.AsFloat(), col.BoundsMax.z.AsFloat());
                // Vector3 aabbCenter = (min + max) * 0.5f;
                // Vector3 aabbSize = max - min;
                // Gizmos.DrawWireCube(aabbCenter, aabbSize);
            }
        }
    }
}