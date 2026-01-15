using TrueSync;

namespace ET
{
    /// <summary>
    /// 射线检测算法
    /// </summary>
    [FriendOf(typeof(LSPhysicsWorld))]
    [FriendOf(typeof(LSCollider))]
    public static class LSRaycastMath
    {
        /// <summary>
        /// 射线
        /// </summary>
        public struct Ray
        {
            /// <summary>
            /// 起点
            /// </summary>
            public TSVector Origin;

            /// <summary>
            /// 方向
            /// </summary>
            public TSVector Direction;
        }

        /// <summary>
        /// 射线检测
        /// </summary>
        /// <param name="world">LSPhysicsWorld</param>
        /// <param name="origin">TSVector</param>
        /// <param name="direction">TSVector</param>
        /// <param name="maxDistance">FP，最大距离</param>
        /// <param name="hitInfo">LSRaycastHit</param>
        /// <returns></returns>
        public static bool Raycast(LSPhysicsWorld world, TSVector origin, TSVector direction, FP maxDistance, out LSRaycastHit hitInfo)
        {
            hitInfo = default;
            Ray ray = new Ray { Origin = origin, Direction = direction };
            ray.Direction.Normalize();

            FP   minDistance = maxDistance;
            bool hitAny      = false;

            foreach (LSCollider collider in world.Colliders)
            {
                //TODO: Broad Phase: AABB 优化 (省略，建议加上)

                FP       dist   = -1;
                TSVector normal = TSVector.zero;
                bool     hit    = false;

                if (collider.ShapeType == LSColliderType.Sphere)
                {
                    hit = IntersectRaySphere(ray, collider.WorldCenter, collider.Radius, out dist);
                }
                else if (collider.ShapeType == LSColliderType.Box)
                {
                    hit = IntersectRayAABB(ray, collider.BoundsMin, collider.BoundsMax, out dist, out normal);
                }
                //TODO:缺少射线和Capsule之间的射线检测逻辑--pxq

                if (hit && dist >= 0 && dist < minDistance)
                {
                    minDistance = dist;
                    hitAny = true;
                    hitInfo.Entity = collider.GetParent<Unit>();
                    hitInfo.Distance = dist;
                    hitInfo.Point = ray.Origin + ray.Direction * dist;
                    hitInfo.Normal = normal; // 球体 Normal 需要单独算：Point - Center
                    if (collider.ShapeType == LSColliderType.Sphere)
                        hitInfo.Normal = (hitInfo.Point - collider.WorldCenter).normalized;
                }
            }

            return hitAny;
        }

        /// <summary>
        /// 射线和SphereCollider检测
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="center">TSVector</param>
        /// <param name="radius">radius</param>
        /// <param name="distance">FP</param>
        /// <returns></returns>
        private static bool IntersectRaySphere(Ray ray, TSVector center, FP radius, out FP distance)
        {
            distance = -1;
            TSVector m = ray.Origin - center;
            FP       b = TSVector.Dot(m, ray.Direction);
            FP       c = TSVector.Dot(m, m) - radius * radius;

            if (c > 0 && b > 0) return false; // 起点在球外且背离球

            FP discr = b * b - c;
            if (discr < 0) return false; // 无解

            distance = -b - TSMath.Sqrt(discr); // 取最近交点
            if (distance < 0) distance = 0;     // 起点在球内
            return true;
        }

        /// <summary>
        /// 射线与AABB检测
        /// Slab Method
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="min">TSVector</param>
        /// <param name="max">TSVector</param>
        /// <param name="distance">distance</param>
        /// <param name="normal">TSVector</param>
        /// <returns></returns>
        private static bool IntersectRayAABB(Ray ray, TSVector min, TSVector max, out FP distance, out TSVector normal)
        {
            distance = FP.Zero;
            normal = TSVector.zero;

            TSVector o = ray.Origin;
            TSVector d = ray.Direction;

            FP tmin = FP.Zero;     // 我们只关心 t >= 0 的部分
            FP tmax = FP.MaxValue; // 假设有 MaxValue（若无可用一个极大值如 FP(999999999)）

            int hitAxis = -1;      // 记录击中的轴（用于计算法线）
            FP  hitSign = FP.Zero; // 击中面的法线方向（-1 或 +1）

            for (int i = 0; i < 3; ++i)
            {
                FP di = d[i];

                if (di == FP.Zero)
                {
                    // 射线在该轴平行，若起点不在盒子范围内则无交点
                    if (o[i] < min[i] || o[i] > max[i])
                        return false;
                    // 平行轴不更新 tmin/tmax
                }
                else
                {
                    FP invDi = FP.One / di;

                    FP t1 = (min[i] - o[i]) * invDi;
                    FP t2 = (max[i] - o[i]) * invDi;

                    FP tn = t1 < t2 ? t1 : t2; // 进入该slab的t
                    FP tf = t1 > t2 ? t1 : t2; // 离开该slab的t

                    // 只在 tn >= tmin 时更新（保证 tmin >= 0，且记录限制性slab用于法线）
                    if (tn >= tmin)
                    {
                        if (tn > tmin) // > 用于严格更新，== 时保持之前轴（避免边角多轴竞争）
                        {
                            tmin = tn;
                            hitAxis = i;
                            hitSign = di > FP.Zero ? -FP.One : FP.One; // 射线正向击中min面（-轴），负向击中max面（+轴）
                        }
                    }

                    if (tf < tmax)
                        tmax = tf;

                    if (tmin > tmax)
                        return false;
                }
            }

            // 理论上 tmax < 0 的情况已被上面剪枝，这里可再保险（可选）
            // if (tmax < FP.Zero) return false;

            distance = tmin;

            // 计算法线：只有当真实击中在前方（tmin > 0 或在表面）且有明确限制轴时才设置
            // 起点在盒子内部或正好在表面但未触发更新时，法线保持 zero（符合“略简化”）
            if (hitAxis >= 0)
            {
                normal = TSVector.zero;
                if (hitAxis == 0) normal.x = hitSign;
                else if (hitAxis == 1) normal.y = hitSign;
                else if (hitAxis == 2) normal.z = hitSign;
            }

            return true;
        }
    }
}